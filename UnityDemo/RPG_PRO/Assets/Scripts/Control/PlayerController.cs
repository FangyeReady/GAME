using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;
using UnityEngine.AI;
namespace RPG.Control {
    public class PlayerController : MonoBehaviour {

        private Mover m_Mover;
        private Fighter m_Fighter;
        private Health m_Health;
        [Range(0, 1)] public float speedRatio = 1f;

        void Start () {
            m_Mover = this.GetComponent<Mover> ();
            m_Fighter = this.GetComponent<Fighter> ();
            m_Health = this.GetComponent<Health>();
        }

        void Update () {

            if(m_Health.IsDead()) return;

            if (InteractWithCombat ()) return; //战斗要在移动前面，不然就会一直移动到指定位置，而不会去战斗
            if (InteractWithMovement ()) return;
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <returns></returns>
        private bool InteractWithMovement () {
            if (Input.GetMouseButton (0)) {
                MoveToCorsur ();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 战斗
        /// </summary>
        /// <returns></returns>
        private bool InteractWithCombat () {
            RaycastHit[] hits = Physics.RaycastAll (GetMouseRay ());
            foreach (RaycastHit hit in hits) {
                GameObject target = hit.transform.gameObject;
                if (!m_Fighter.CanAttack (target)) continue;
              
                if (Input.GetMouseButton (0)) {
                    this.transform.LookAt (target.transform);
                    m_Fighter.StartAttackAction (target);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据鼠标点击的位置移动
        /// </summary>
        private void MoveToCorsur () {
            RaycastHit hitInfo;
            bool hasHit = Physics.Raycast (GetMouseRay (), out hitInfo);

            if (hasHit) {
                m_Mover.StartMoveAction (hitInfo.point, speedRatio);
            }
        }

        private Ray GetMouseRay () {
            return Camera.main.ScreenPointToRay (Input.mousePosition);
        }

    }

}