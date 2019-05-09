using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class shaco_ExtensionsObserverUnity
{
    /// <summary>
    /// 观察者开始观察数据，并绑定一个对象，数据主体和观测者会在对象销毁时候，也自动销毁
    /// <param name="observer">观察者</param>
    /// <param name="bindTarget">绑定对象</param>
    /// </summary>
    static public shaco.Base.IObserver<T> Start<T>(this shaco.Base.IObserver<T> observer, UnityEngine.GameObject bindTarget)
    {
        if (null == bindTarget)
        {
            shaco.Base.Log.Error("ObserverExtension BindAutoRelease error: bind target is null");
            return observer;
        }

        if (null == observer)
        {
            shaco.Base.Log.Error("ObserverExtension BindAutoRelease error: observer is null");
            return observer;
        }

        if (null == observer.subject)
        {
            shaco.Base.Log.Error("ObserverExtension BindAutoRelease error: subject is null");
            return observer;
        }

        var autoReleaseComponent = bindTarget.GetComponent<shaco.ObserverAutoReleaseComponent>();
        if (null == autoReleaseComponent)
        {
            autoReleaseComponent = bindTarget.AddComponent<shaco.ObserverAutoReleaseComponent>();
			if (null == autoReleaseComponent)
			{
				shaco.Base.Log.Error("ObserverExtension BindAutoRelease error: can't add component 'ObserverAutoReleaseComponent'");
				return observer;
			}
        }

        autoReleaseComponent.SetClearCallBack(observer.subject.Clear);
        autoReleaseComponent.transform.parent = bindTarget.transform;

        observer.subject.SetBindTarget(bindTarget);
        observer.subject.Add(observer);
        return observer;
    }

    /// <summary>
    /// 观察者开始观察数据，并绑定一个对象，数据主体和观测者会在对象销毁时候，也自动销毁
    /// <param name="observer">观察者</param>
    /// <param name="bindTarget">绑定对象</param>
    /// </summary>
    static public shaco.Base.IObserver<T> Start<T>(this shaco.Base.IObserver<T> observer, UnityEngine.Component bindTarget)
	{
		return Start<T>(observer, bindTarget.gameObject);
	}
}