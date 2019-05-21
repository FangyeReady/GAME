using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface ISubjectBase
    {
        /// <summary>
        /// 移除所有观察对象
        /// </summary>
        void Clear();

        /// <summary>
        /// 通知所有观察对象数据有刷新
        /// </summary>
        void Notify();

        /// <summary>
        /// 获取数据主体绑定对象
        /// <return>绑定对象</return>
        /// </summary>
        object GetBindTarget();

        /// <summary>
        /// 设置数据主体绑定对象，同一时间只能绑定一个数据主体
        /// <param name="bindTarget">绑定对象</param>
        /// </summary>
        void SetBindTarget(object bindTarget);
    }

    /// <summary>
    /// 数据的主体，用于修改数据和通知观测者
    /// </summary>
    public interface ISubject<T> : ISubjectBase
    {
        /// <summary>
        /// 所有的观察对象
        /// </summary>
        System.Collections.ObjectModel.ReadOnlyCollection<IObserver<T>> observers { get; }
		
		/// <summary>
		/// 被观测的数据
		/// </summary>
		T value { get; set; }

        /// <summary>
        /// 添加数据观测对象
        /// <param name="observer">数据观察者</param>
        /// </summary>
        IObserver<T> Add(IObserver<T> observer);

        /// <summary>
        /// 移除数据观测对象
        /// <param name="observer">数据观察者</param>
        /// </summary>
        void Remove(IObserver<T> observer);
    }
}

