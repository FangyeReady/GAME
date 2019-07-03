using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using RPG.Movement;
using RPG.Saving;
using UnityEngine;
using RPG.Resources;
namespace RPG.Combat {

    public class Fighter : MonoBehaviour, IAction, ISaveble {

        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform RightHand;
        [SerializeField] Transform LeftHand;
        [SerializeField] Weapon defaultWeapon;

        [SerializeField] string weaponName = "Unarmed";


        private Health target;
        private Mover m_Mover;
        private ActionScheduler m_ActionScheduler;
        private Animator m_AniController;
        private Weapon currentWeapon = null;
       

        private float timeSinceLastAttack = 0;

        private void Start ()
        {
            m_Mover = GetComponent<Mover>();
            m_ActionScheduler = GetComponent<ActionScheduler>();
            m_AniController = GetComponent<Animator>();
            if( currentWeapon == null)
                EquipWeapon(defaultWeapon);
            else
                weaponName = currentWeapon.name;
        }

        public void EquipWeapon(Weapon weapon)
        {
            currentWeapon = weapon;
            weaponName = currentWeapon.name;
            if (currentWeapon != null)
            {
                currentWeapon.Spawn(RightHand, LeftHand, GetComponent<Animator>());
            }     
        }

        private void Update () {
            timeSinceLastAttack += Time.deltaTime;

            if (null == target) return;
            if (target.IsDead ()) return;

            //若不在武器范围内则先移动过去
            if (!GetIsInRange ()) {
                m_Mover.MoveTo (target.transform.position, 1f);
            } else {
                m_Mover.Cancel();
                AttackBehaviour ();
            }
        }

        /// <summary>
        /// 是否能够攻击  
        /// </summary>
        /// <returns></returns>
        public bool CanAttack (GameObject tg) {
            if(tg == null) return false;
            Health tgHealth = tg.GetComponent<Health> ();
            return tgHealth != null && !tgHealth.IsDead ();
        }

        public void StartAttackAction (GameObject target) {
            m_ActionScheduler.StartAction (this);
            this.target = target.GetComponent<Health> (); // target在update中被使用，一旦被赋值就直接开启战斗逻辑
        }

        public void AttackBehaviour () {
            this.transform.LookAt(target.transform);
            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                timeSinceLastAttack = 0f;
                TriggerAttack();
            }
        }

        private void Hit () {
            if (null == target) return;
            if (currentWeapon.HasProjectile())
            {
                currentWeapon.SpawnLongRangeBullet( RightHand, LeftHand, target );
            }
            else
            {
                target.GetComponent<Health>().TakeDamage(currentWeapon.GetDamege());
            }
           
        }


        private void Shoot()
        {
            Hit();
        }

        private bool GetIsInRange () {
            return Vector3.Distance (this.transform.position, target.transform.position) < currentWeapon.GetRange();
        }

        public void Cancel ()
        {
            target = null;
            m_Mover.Cancel();
            StopAttack();
        }

        private void StopAttack()
        {
            m_AniController.ResetTrigger("attack");
            m_AniController.SetTrigger("stopAttack");
        }

        private void TriggerAttack()
        {
            m_AniController.ResetTrigger("stopAttack");
            m_AniController.SetTrigger("attack");
        }

        public void RestoreState(object state)
        {
            weaponName = (string)state;
            Weapon temp = UnityEngine.Resources.Load<Weapon>(weaponName);
            EquipWeapon(temp);
        }

        public object CaptureState()
        {
            return weaponName;
        }
    }
}