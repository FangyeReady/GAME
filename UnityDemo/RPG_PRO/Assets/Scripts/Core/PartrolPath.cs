using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core {
    public class PartrolPath : MonoBehaviour {

        [SerializeField] float partrolRadius = 0.4f;

        private void OnDrawGizmos () {  
            for (int i = 0; i < transform.childCount; i++) {
                Gizmos.DrawSphere (transform.GetChild (i).transform.position, partrolRadius);
            }
        }
    }
}