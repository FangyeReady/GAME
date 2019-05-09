using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class Observer<T> : IObserver<T>
    {
        /// <summary>
        /// 数据主体
        /// </summary>
        public ISubject<T> subject { get; set; }

        /// <summary>
        /// 数据初始化的方法
        /// </summary>
        public System.Action<ISubject<T>> callbackInit { get; set; }
        virtual public void OnInitCallBack() { }

        /// <summary>
        /// 数据发生变化的刷新方法
        /// </summary>
        public System.Action<ISubject<T>> callbackUpdate { get; set; }
        virtual public void OnUpdateCallBack() { }

        /// <summary>
        /// 数据被销毁的方法
        /// </summary>
        public System.Action<ISubject<T>> callbackDestroy { get; set; }
        virtual public void OnDestroyCallBack() { }
    }
}

