using System;
using RPG.Combat;
using RPG.Movement;
using UnityEngine;

namespace RPG.Core {

    public class AiController : MonoBehaviour {

        [SerializeField] float toPlayerDistance = 5f;
        [SerializeField] float waitTimeToGoBack = 3f;
        [SerializeField] float waitTimeAtPoint = 1.5f;
        [SerializeField] PartrolPath partrolPath = null;
        [Range(0, 1)] public float  speedRatio = 0.5f;

        /// <summary>
        /// 失去目标后的停滞时间变量
        /// </summary>
        float lastSawPlayerTime = Mathf.Infinity;//无穷大

        /// <summary>
        /// 寻点的暂停时间变量
        /// </summary>
        float lastInPathPoint = Mathf.Infinity;//无穷大

        /// <summary>
        /// 当前寻路的点
        /// </summary>
        private int currentGardIndex = 0;

        /// <summary>
        /// 距离目标路径点的距离
        /// </summary>
        private float disToPathPoint = 3f;

        /// <summary>
        /// 初始位置
        /// </summary>
        private Vector3 gardPosition;

        /// <summary>
        /// 下一个路径点的位置
        /// </summary>
        private Vector3 nextPosition;

        private GameObject player;
        private Fighter m_Fighter;
        private Health m_Health;
        private Mover m_Mover;
        private ActionScheduler m_ActionScheduler;

        private void Start () {
            player = GameObject.FindGameObjectWithTag ("Player");
            m_Fighter = GetComponent<Fighter> ();
            m_Health = GetComponent<Health> ();
            m_Mover = GetComponent<Mover> ();
            m_ActionScheduler = GetComponent<ActionScheduler> ();

            gardPosition = this.transform.position;
            nextPosition = gardPosition;
        }

        private void Update ()
        {

            if (m_Health.IsDead()) return;

            if (IsPlayerInRange() && m_Fighter.CanAttack(tg: player.gameObject))
            {
                AttackBehaviour();
            }
            else if (lastSawPlayerTime < waitTimeToGoBack)  //如果lastSawPlayerTime增加到等于waitTimeToGoBack，那么就取消当前操作（战斗），然后回到初始position
            {
                CancelBehaviour();
            }
            else
            {
                GardMoveBehaviour();
            }

            UpdateTimers();

        }

        private void UpdateTimers()
        {
            lastSawPlayerTime += Time.deltaTime;
            lastInPathPoint += Time.deltaTime;
        }

        private void GardMoveBehaviour () {
            if (partrolPath != null) {
                if (AtPartrolPoint ()) { //这里在初次进来后并不会频繁进入，因为此时已经更新了index
                    currentGardIndex = CyclePointIndex ();
                    nextPosition = GetCurrentWayPoint ();
                    lastInPathPoint = 0;
                }
                if (lastInPathPoint > waitTimeAtPoint)
                {
                    m_Mover.StartMoveAction(nextPosition,  speedRatio);
                }
                return;
            }
            m_Mover.StartMoveAction (gardPosition, speedRatio);

        }

        private Vector3 GetCurrentWayPoint () {
            return partrolPath.GetPoint (currentGardIndex);
        }

        private int CyclePointIndex () {
            return partrolPath.GetNextIndex (currentGardIndex);
        }

        private bool AtPartrolPoint () {
            float dis = Vector3.Distance (this.transform.position, GetCurrentWayPoint ());
            return dis < disToPathPoint;
        }

        /// <summary>
        /// 结束当前行为
        /// </summary>
        private void CancelBehaviour () {
            m_ActionScheduler.CancelCurrentAction ();
        }

        private void AttackBehaviour () {
            lastSawPlayerTime = 0f;
            this.transform.LookAt(player.transform);
            m_Fighter.StartAttackAction (player);
        }

        private bool IsPlayerInRange () {
            float distance = Vector3.Distance (this.transform.position, player.transform.position);
            return distance < toPlayerDistance;
        }

        /// <summary>
        /// 画出enemy的警戒范围,系统接口
        /// </summary>
        private void OnDrawGizmosSelected () {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere (this.transform.position, toPlayerDistance);
        }
    }
}