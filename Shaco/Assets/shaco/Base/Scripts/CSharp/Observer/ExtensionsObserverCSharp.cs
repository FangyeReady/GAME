using System.Collections;
using System.Collections.Generic;

static public class shaco_ExtensionsObserverCSharp
{
    /// <summary>
    /// 监听数据发生变化
    /// <param name="subject">数据主体</param>
    /// <param name="callbackUpdate">数据发生变化时的回调方法</param>
    /// <return>数据主体</return>
    /// </summary>
    static public shaco.Base.IObserver<T> OnSubjectValueUpdate<T>(this shaco.Base.ISubject<T> subject, System.Action<shaco.Base.ISubject<T>> callbackUpdate)
    {
        var retValue = new shaco.Base.Observer<T>()
        {
            callbackUpdate = callbackUpdate
        };
        retValue.subject = subject;
        return retValue;
    }

    /// <summary>
    /// 监听数据发生变化
    /// <param name="subject">数据主体</param>
    /// <param name="callbackUpdate">数据发生变化时的回调方法</param>
    /// <return>数据主体</return>
    /// </summary>
    static public shaco.Base.IObserver<T> OnValueUpdate<T>(this shaco.Base.ISubject<T> subject, System.Action<T> callbackUpdate)
    {
        var retValue = new shaco.Base.Observer<T>()
        {
            callbackUpdate = (shaco.Base.ISubject<T> subjectTmp) =>
            {
                callbackUpdate(subjectTmp.value);
            }
        };
        retValue.subject = subject;
        return retValue;
    }

    /// <summary>
    /// 给数据主体快速添加观察者
    /// <param name="subject">数据主体</param>
    /// <param name="callback">数据发生变化时的回调方法[数据主体，观测对象]</param>
    /// <return>观察者</return>
    /// </summary>
    static public TOBSERVER OnValueUpdate<TValue, TOBSERVER>(this shaco.Base.ISubject<TValue> subject, System.Action<shaco.Base.ISubject<TValue>> callbackUpdate) where TOBSERVER : shaco.Base.IObserver<TValue>, new()
    {
        var newObserver = new TOBSERVER();
        newObserver.callbackUpdate = callbackUpdate;
        newObserver.subject = subject;
        return newObserver;
    }

    /// <summary>
    /// 给数据主体快速添加观察者
    /// <param name="subject">数据主体</param>
    /// <param name="callback">数据发生变化时的回调方法[数据主体，观测对象]</param>
    /// <return>观察者</return>
    /// </summary>
    static public TObserver OnValueUpdate<TValue, TObserver>(this shaco.Base.ISubject<TValue> subject, System.Action<TValue> callbackUpdate) where TObserver : shaco.Base.IObserver<TValue>, new()
    {
        var newObserver = new TObserver();
        newObserver.callbackUpdate = (shaco.Base.ISubject<TValue> subjectTmp) =>
        {
            callbackUpdate(subjectTmp.value);
        };
        newObserver.subject = subject;
        return newObserver;
    }

    /// <summary>
    /// 监听数据初始化
    /// <param name="observer">观察者</param>
    /// <param name="callbackInit">数据初始化的时候调用</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<T> OnValueInit<T>(this shaco.Base.IObserver<T> observer, System.Action<shaco.Base.ISubject<T>> callbackInit)
    {
        observer.callbackInit = callbackInit;
        return observer;
    }

    /// <summary>
    /// 监听数据初始化
    /// <param name="observer">观察者</param>
    /// <param name="callbackInit">数据初始化的时候调用</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<T> OnValueInit<T>(this shaco.Base.IObserver<T> observer, System.Action<T> callbackInit)
    {
        observer.callbackInit = (shaco.Base.ISubject<T> subjectTmp) =>
        {
            callbackInit(subjectTmp.value);
        };
        return observer;
    }

    /// <summary>
    /// 监听数据被销毁
    /// <param name="observer">观察者</param>
    /// <param name="callbackDestroy">数据被销毁时的回调方法</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<T> OnValueDestroy<T>(this shaco.Base.IObserver<T> observer, System.Action<shaco.Base.ISubject<T>> callbackDestroy)
    {
        observer.callbackDestroy = callbackDestroy;
        return observer;
    }

    /// <summary>
    /// 监听数据被销毁
    /// <param name="observer">观察者</param>
    /// <param name="callbackDestroy">数据被销毁时的回调方法</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<T> OnValueDestroy<T>(this shaco.Base.IObserver<T> observer, System.Action<T> callbackDestroy)
    {
        observer.callbackDestroy = (shaco.Base.ISubject<T> subjectTmp) =>
        {
            callbackDestroy(subjectTmp.value);
        };
        return observer;
    }

    /// <summary>
    /// 观察者开始观察数据
    /// <param name="observer">观察者</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<T> Start<T>(this shaco.Base.IObserver<T> observer)
    {
        observer.subject.Add(observer);
        return observer;
    }
}