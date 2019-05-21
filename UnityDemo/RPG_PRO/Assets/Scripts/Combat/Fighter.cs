using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Combat {

    public class Fighter : MonoBehaviour, IAction {

        private Transform target;
        private Mover m_Mover;
        private ActionScheduler m_ActionScheduler;
        private Animator m_AniController;

        [SerializeField] float weaponRange = 3f;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] float weaponDamage = 5f;

        private float timeSinceLastAttack = 0;

        private void Start () {
            m_Mover = GetComponent<Mover> ();
            m_ActionScheduler = GetComponent<ActionScheduler> ();
            m_AniController = GetComponent<Animator> ();
        }

        private void Update () {
            timeSinceLastAttack += Time.deltaTime;

            if (null == target) return;

            //若不在武器范围内则先移动过去
            if (!GetIsInRange ()) {
                //这里应该直接移动过去，而不是调用Mover.StartMoveAction(pos)  （事实上MoveAction已经开启）,
                //Mover.StartMoveAction(pos)其中包含了ActionScheduler.StartAction (this)，
                //因为Attack中已经调用了 ActionScheduler.StartAction (this)，
                //如果此时又调用ActionScheduler.StartAction (this)就会调用Figher的Cancel方法， target = null
                //那么战斗逻辑就会被终止
                //m_Mover.MoveTo(target.position);

                //TODO:待验证, 在此处开启移动Action
                m_Mover.StartMoveAction (target.position);
            } else {
                //m_Mover.Cancel (); //因为Attack中已经调用了 ActionScheduler.StartAction (this) ，此时如果再调用就会直接return, 无意义
                //TODO:待验证，真正开启战斗
                m_ActionScheduler.StartAction (this);
                AttackBehaviour ();
            }
        }

        public void Attack (GameObject target) {
            //m_ActionScheduler.StartAction (this); //当需要攻击时，移动的Cancel被调用

            //TODO:待验证，试试此处不调用m_ActionScheduler.StartAction (this)，因为此处即使设置了target，由于武器范围原因，仍然不一定停止移动
            this.target = target.transform; // target在update中被使用，一旦被赋值就直接开启战斗逻辑
        }

        public void AttackBehaviour () {
            if (timeSinceLastAttack > timeBetweenAttacks) {
                timeSinceLastAttack = 0f;
                m_AniController.SetTrigger ("attack");
            }
        }

        void Hit () {
            target.GetComponent<Health> ().TakeDamage (weaponDamage);
        }

        private bool GetIsInRange () {
            return Vector3.Distance (this.transform.position, target.transform.position) < weaponRange;
        }

        public void Cancel () {
            target = null;
        }
    }
}