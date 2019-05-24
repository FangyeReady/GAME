using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Combat {

    public class Fighter : MonoBehaviour, IAction {

        private Health target;
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
            if (target.IsDead ()) {
                //m_ActionScheduler.CancelCurrentAction ();
                return;
            }

            //若不在武器范围内则先移动过去
            if (!GetIsInRange ()) {
                //这里应该直接移动过去，而不是调用Mover.StartMoveAction(pos)  （事实上MoveAction已经开启）,
                //Mover.StartMoveAction(pos)其中包含了ActionScheduler.StartAction (this)，
                //因为Attack中已经调用了 ActionScheduler.StartAction (this)，
                //如果此时又调用ActionScheduler.StartAction (this)就会调用Figher的Cancel方法， target = null
                //那么战斗逻辑就会被终止
                m_Mover.MoveTo (target.transform.position);

                //:待验证, 在此处开启移动Action
                //m_Mover.StartMoveAction (target.transform.position);
            } else {
                AttackBehaviour ();
            }
        }

        /// <summary>
        /// 是否能够攻击  
        /// </summary>
        /// <returns></returns>
        public bool CanAttack (GameObject tg) {
            Health tgHealth = tg.GetComponent<Health> ();
            return tgHealth != null && !tgHealth.IsDead ();
        }

        public void Attack (GameObject target) {
            m_ActionScheduler.StartAction (this);
            this.target = target.GetComponent<Health> (); // target在update中被使用，一旦被赋值就直接开启战斗逻辑
        }

        public void AttackBehaviour () {

            if (timeSinceLastAttack > timeBetweenAttacks) {
                timeSinceLastAttack = 0f;
                m_AniController.ResetTrigger ("stopAttack");
                m_AniController.SetTrigger ("attack");
            }
        }

        void Hit () {
            if (null == target) return;
            target.GetComponent<Health> ().TakeDamage (weaponDamage);
        }

        private bool GetIsInRange () {
            return Vector3.Distance (this.transform.position, target.transform.position) < weaponRange;
        }

        public void Cancel () {
            target = null;
            m_AniController.ResetTrigger ("attack");
            m_AniController.SetTrigger ("stopAttack");
        }

    }
}