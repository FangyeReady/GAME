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

            if (!GetIsInRange ()) {
                m_Mover.MoveTo (target.position);
            } else {
                m_Mover.Cancel ();
                AttackBehaviour ();
            }
        }

        public void Attack (GameObject target) {
            m_ActionScheduler.StartAction (this);
            this.target = target.transform;
        }

        public void AttackBehaviour () {
            if (timeSinceLastAttack > timeBetweenAttacks) {
                timeSinceLastAttack = 0f;
                m_AniController.SetTrigger ("attack");
            }
        }

        void Hit() {
            target.GetComponent<Health>().TakeDamage( weaponDamage );
        }

        private bool GetIsInRange () {
            return Vector3.Distance (this.transform.position, target.transform.position) < weaponRange;
        }

        public void Cancel () {
            target = null;
        }
    }
}