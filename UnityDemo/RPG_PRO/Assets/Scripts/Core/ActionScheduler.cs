using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RPG.Core {
    public class ActionScheduler : MonoBehaviour {
        IAction action;

        public void StartAction (IAction behaviour) {
            if (action == behaviour) return;

            if (null != action) {

                Debug.Log ("cancel~!");
                action.Cancel ();
            }
            this.action = behaviour;
        }

        /// <summary>
        /// 取消当前行为
        /// </summary>
        public void CancelCurrentAction () {
            StartAction (null);
        }
    }
}