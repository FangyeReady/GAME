using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RPG.Core {
    public class ActionScheduler : MonoBehaviour {
        IAction action;

        public void StartAction (IAction nowAction) {
            if (action == nowAction) return;

            if (null != action) {
                action.Cancel ();
            }
            this.action = nowAction;
        }

        /// <summary>
        /// 取消当前行为
        /// </summary>
        public void CancelCurrentAction () {
            StartAction (null);
        }
    }
}