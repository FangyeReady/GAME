using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Base
{
    /// <summary>
    /// 数据的主体，用于修改数据和通知观测者
    /// </summary>
    public class Subject<T> : ISubject<T>
    {
        /// <summary>
        /// 所有的观察对象
        /// </summary>
        virtual public System.Collections.ObjectModel.ReadOnlyCollection<IObserver<T>> observers { get { return _observers.AsReadOnly(); } }

        /// <summary>
        /// 被观测的数据
        /// </summary>
        virtual public T value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!_value.Equals(value))
                {
                    _value = value;
                    Notify();
                }
            }
        }

        /// <summary>
        /// 被观测的数据实例
        /// </summary>
        protected T _value = default(T);

        /// <summary>
        /// 所有的观察对象实例
        /// </summary>
        private List<IObserver<T>> _observers = new List<IObserver<T>>();

        /// <summary>
        /// 数据主体绑定对象
        /// </summary>
        private object _bindTarget = shaco.Base.SubjectManager.DEFAULT_NULL_KEY;

        /// <summary>
        /// 添加数据观测对象
        /// <param name="observer">数据观测对象</param>
        /// </summary>
        virtual public IObserver<T> Add(IObserver<T> observer)
        {
            if (null == observer)
            {
                Log.Error("Subject Add error: observer is nul");
                return observer;
            }

            if (_observers.Contains(observer))
            {
                Log.Error("Subject Add erorr: has duplicate observer=" + observer.GetType().FullName);
                return observer;
            }

            _observers.Add(observer);
            SubjectManager.AddObserver(this, observer);

            observer.subject = this;

            SubjectManager.WillValueInit(this, observer);
            if (null != observer.callbackInit)
            {
                observer.callbackInit(this);
            }
            observer.OnInitCallBack();
            SubjectManager.ValueInited(this, observer);

            return observer;
        }

        /// <summary>
        /// 移除数据观测对象
        /// <param name="observer">数据观测对象</param>
        /// </summary>
        virtual public void Remove(IObserver<T> observer)
        {
            int findIndex = -1;
            for (int i = _observers.Count - 1; i >= 0; --i)
            {
                if (_observers[i] == observer)
                {
                    findIndex = i;
                    break;
                }
            }

            if (findIndex < 0)
            {
                Log.Error("Subject Remove error: not found observer=" + observer.GetType().FullName);
            }
            else
            {
                if (null != observer.callbackDestroy)
                {
                    observer.callbackDestroy(this);
                }
                observer.OnDestroyCallBack();
                _observers.RemoveAt(findIndex);
                SubjectManager.RemoveObserver(this, observer);
            }
        }

		/// <summary>
        /// 移除所有观察对象
        /// </summary>
        virtual public void Clear()
        {
            _observers.Clear();
            SubjectManager.RemoveSubject(this);
        }

        /// <summary>
        /// 通知所有观察对象数据有刷新
        /// </summary>
        virtual public void Notify()
        {
            if (GlobalParams.OpenDebugLog)
            {
                if (observers.Count == 0)
                {
                    Log.Error("Subject Notify error: observer is empty, value=" + value);
                }

                for (int i = 0; i < observers.Count; ++i)
                {
                    var observerTmp = observers[i];
                    if (null != observerTmp)
                    {
                        SubjectManager.WillValueUpdate(this, observerTmp);
                        {
                            if (null != observerTmp.callbackUpdate)
                            {
                                observerTmp.callbackUpdate(this);
                            }
                            observerTmp.OnUpdateCallBack();
                        }
                        SubjectManager.ValueUpdated(this, observerTmp);
                    }
                }
            }
            else 
            {
                for (int i = 0; i < observers.Count; ++i)
                {
                    var observerTmp = observers[i];
                    if (null != observerTmp && null != observerTmp.callbackUpdate)
                    {
                        observerTmp.callbackUpdate(this);
                    }
                    observerTmp.OnUpdateCallBack();
                }
            }
        }

        /// <summary>
        /// 获取数据主体绑定对象
        /// <return>绑定对象</return>
        /// </summary>
        virtual public object GetBindTarget()
        {
            return _bindTarget;
        }

        /// <summary>
        /// 设置数据主体绑定对象，同一时间只能绑定一个数据主体
        /// <param name="bindTarget">绑定对象</param>
        /// </summary>
        virtual public void SetBindTarget(object bindTarget)
        {
            if (_bindTarget == bindTarget)
            {
                return;
            }

            //已经绑定过对象了，则销毁上一个绑定信息，并重新绑定
            if (null != _bindTarget && SubjectManager.DEFAULT_NULL_KEY != _bindTarget.ToString())
            {
                Log.Error("Subject SetBindTarget error: have already bound an object, not allowed to bind other target repeatedly");
                return;
            }
            _bindTarget = bindTarget;
        }
    }
}