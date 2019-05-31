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
                return;
            }

            //若不在武器范围内则先移动过去
            if (!GetIsInRange ()) {
                m_Mover.MoveTo (target.transform.position, 1f);
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

        public void StartAttackAction (GameObject target) {
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
            m_Mover.Cancel();
            m_AniController.ResetTrigger ("attack");
            m_AniController.SetTrigger ("stopAttack");
        }

    }
}