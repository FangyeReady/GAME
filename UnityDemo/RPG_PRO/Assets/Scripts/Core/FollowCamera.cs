using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core {

    public class FollowCamera : MonoBehaviour
    {

        [SerializeField] Transform player;

        private void Awake() {
            if( null == player )
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }


        /// <summary>
        /// Excution order of event functions   U3d的事件调用
        /// </summary>
        void LateUpdate()
        {
            this.transform.position = player.position;
        }
    }

}

