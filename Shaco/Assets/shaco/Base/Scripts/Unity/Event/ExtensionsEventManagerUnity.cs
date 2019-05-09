using System.Collections;
using shaco.Base;
using UnityEngine;

/// <summary>
/// 事件扩展方法，用于更加方便调用事件
/// </summary>
static public class shaco_ExtensionsEventManagerUnity
{
    /// <summary>
    /// 添加自动销毁的事件，会随着defaultSender被销毁的时候，自动释放
    /// <param name="defaultSender">事件默认绑定对象</param>
    /// <param name="callfunc">事件回调方法</param>
    /// <param name="invokeOnce">是否只触发1次回调，会自动在触发1次之后自动销毁事件</param>
    /// <return>添加事件是否成功</return>
    /// </summary>
    static public bool AddAutoRealeaseEvent<T>(this GameObject defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce = false) where T : BaseEventArg
    {
        BindAutoReleaseComponent(defaultSender, defaultSender);
        return EventManager.AddEvent<T>(defaultSender, callfunc, invokeOnce);
    }
    static public bool AddAutoRealeaseEvent<T>(this Component defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce = false) where T : BaseEventArg
    {
        BindAutoReleaseComponent(defaultSender.gameObject, defaultSender);
        return EventManager.AddEvent<T>(defaultSender, callfunc, invokeOnce);
    }
    static public bool AddAutoRealeaseEvent<T>(this Transform defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce = false) where T : BaseEventArg
    {
        BindAutoReleaseComponent(defaultSender.gameObject, defaultSender);
        return EventManager.AddEvent<T>(defaultSender, callfunc, invokeOnce);
    }

    /// <summary>
    /// 绑定自动销毁组件对象，在绑定对象销毁时候，自动销毁相关联事件
    /// <param name="defaultSender">事件默认绑定对象</param>
    /// </summary>
    static private void BindAutoReleaseComponent(GameObject bindTarget, object defaultSender)
	{
        shaco.EventAutoReleaseComponent autoReleaseComponent = bindTarget.GetComponent<shaco.EventAutoReleaseComponent>();
		if (null == autoReleaseComponent)
		{
            autoReleaseComponent = bindTarget.AddComponent<shaco.EventAutoReleaseComponent>();
		}
        autoReleaseComponent.defaultSender = defaultSender;
		autoReleaseComponent.transform.parent = bindTarget.transform;
	}
}