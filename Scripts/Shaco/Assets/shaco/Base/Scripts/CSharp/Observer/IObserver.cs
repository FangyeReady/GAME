using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 数据观测者基础信息
    /// </summary>
    public interface IObserverBase
    {
        /// <summary>
        /// 数据初始化的方法
        /// </summary>
        void OnInitCallBack();

        /// <summary>
        /// 数据发生变化的刷新方法
        /// </summary>
        void OnUpdateCallBack();

        /// <summary>
        /// 数据被销毁的方法
        /// </summary>
        void OnDestroyCallBack();
    }

    /// <summary>
    /// 数据观测者，用于观测数据变化并作出逻辑动作
    /// </summary>
    public interface IObserver<T> : IObserverBase
    {
        /// <summary>
        /// 数据主体
        /// </summary>
        ISubject<T> subject { get; set; }

        /// <summary>
        /// 数据初始化的方法
        /// </summary>
        System.Action<ISubject<T>> callbackInit { get; set; }

        /// <summary>
        /// 数据发生变化的刷新方法
        /// </summary>
        System.Action<ISubject<T>> callbackUpdate { get; set; }

        /// <summary>
        /// 数据被销毁的方法
        /// </summary>
        System.Action<ISubject<T>> callbackDestroy { get; set; }
    }
}