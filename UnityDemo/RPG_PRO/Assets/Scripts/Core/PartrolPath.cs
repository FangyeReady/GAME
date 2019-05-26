using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core {
    public class PartrolPath : MonoBehaviour {

        [SerializeField] float partrolRadius = 0.4f;

        private void OnDrawGizmos () {
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 childPoint = GetPoint(i);
                Gizmos.DrawSphere(childPoint, partrolRadius);
                Gizmos.DrawLine(childPoint, GetPoint(GetNextIndex(i)));
            }
        }

        public Vector3 GetPoint(int i)
        {
            return transform.GetChild(i).position;
        }

        public int GetNextIndex (int i) {
            if (i + 1 < transform.childCount) {
                return i + 1;
            }
            return 0;
        }
    }
}