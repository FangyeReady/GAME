using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Combat;
using RPG.Movement;
namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        private Mover m_Mover;
        private Fighter m_Fighter;

        void Start()
        {
            m_Mover = this.GetComponent<Mover>();
            m_Fighter = this.GetComponent<Fighter>();
        }

        void Update()
        {
            InteractWithMovement();
            InteractWithCombat();
        }

        private void InteractWithMovement()
        {
            if (Input.GetMouseButton(0))
            {
                MoveToCorsur();
            }
        }

        private void InteractWithCombat()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();
                if (null == target) continue;

                if (Input.GetKeyDown( KeyCode.A))
                {
                    m_Fighter.Attack(target);
                }    
            }
        }


        /// <summary>
        /// 根据鼠标点击的位置移动
        /// </summary>
        private void MoveToCorsur()
        {
 
            RaycastHit hitInfo;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hitInfo);

            if (hasHit)
            {
                m_Mover.MoveToTarget(hitInfo.point);
            }
        }

        private Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }



    }

}
