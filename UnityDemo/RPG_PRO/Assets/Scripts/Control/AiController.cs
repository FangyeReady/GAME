using RPG.Combat;
using RPG.Movement;
using UnityEngine;

namespace RPG.Core {

    public class AiController : MonoBehaviour {

        [SerializeField] float toPlayerDistance = 10f;
        [SerializeField] float lastSawPlayerTime = 0f;
        [SerializeField] float timeToGoBack = 5f;
        private Vector3 gardPosition;

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
        }

        private void Update () {

            if (m_Health.IsDead ()) return;

            if (IsPlayerInRange () && m_Fighter.CanAttack (tg: player.gameObject))
            {
                AttackBehaviour();
            }
            else if (lastSawPlayerTime < timeToGoBack)
            {
                StayOrGardBehaviour();
            }
            else
            {
                MoveBackBehaviour();
            }

            lastSawPlayerTime += Time.deltaTime;

        }

        private void MoveBackBehaviour()
        {
            m_Mover.StartMoveAction(gardPosition);
        }

        private void StayOrGardBehaviour()
        {
            m_ActionScheduler.CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            m_Fighter.Attack(player);
            lastSawPlayerTime = 0f;
        }

        private bool IsPlayerInRange () {
            float distance = Vector3.Distance (this.transform.position, player.transform.position);
            return distance < toPlayerDistance;
        }

        /// <summary>
        /// 画出enemy的警戒范围
        /// </summary>
        private void OnDrawGizmosSelected () {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere (this.transform.position, toPlayerDistance);
        }
    }
}