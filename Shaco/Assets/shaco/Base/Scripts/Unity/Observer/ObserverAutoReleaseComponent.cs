using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class ObserverAutoReleaseComponent : MonoBehaviour
    {
        /// <summary>
        /// 清理所有绑定到同一数据的观察者
        /// </summary>
        public System.Action callbackClearObservers = null;

        /// <summary>
        /// 设置清理观测者的回调方法
        /// <param name="callback">回调方法</param>
        /// </summary>
        public void SetClearCallBack(System.Action callback)
        {
            callbackClearObservers = callback;
        }

        /// <summary>
        /// 在该组件被销毁的时候，自动释放相关联的事件对象
        /// </summary>
        void OnDestroy()
        {
            if (null != callbackClearObservers)
            {
                //移除所有观察者
                callbackClearObservers();

                //清理引用
                callbackClearObservers = null;
            }
        }
    }
}