using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class EventAutoReleaseComponent : MonoBehaviour
    {

        public object defaultSender = null;

        /// <summary>
        /// 在该组件被销毁的时候，自动释放相关联的事件对象
        /// </summary>
        void OnDestroy()
        {
            if (null != defaultSender)
            {
                //移除事件	
                shaco.Base.EventManager.RemoveAllEvent(defaultSender);

                //清理引用
                defaultSender = null;
            }
        }
    }
}