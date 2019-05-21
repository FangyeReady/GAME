using System.Collections;
using shaco.Base;

/// <summary>
/// 事件扩展方法，用于更加方便调用事件
/// </summary>
static public class shaco_ExtensionsEventManagerCSharp
{
    static public bool AddEvent<T>(this object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce = false) where T : BaseEventArg
    {
        return EventManager.AddEvent<T>(defaultSender, callfunc, invokeOnce);
    }

    static public bool RemoveEvent<T>(this object defaultSender) where T : BaseEventArg
    {
        return EventManager.RemoveEvent<T>(defaultSender);
    }

    static public void RemoveAllEvent(this object defaultSender)
    {
        EventManager.RemoveAllEvent(defaultSender);
    }

    static public bool InvokeEvent(this object sender, BaseEventArg arg)
    {
        return EventManager.InvokeEvent(sender, arg);
    }

    static public shaco.SequeueEventComponent InvokeSequeueEvent(this object unuseSender, params IEnumerator[] coroutines)
    {
        var sequeueEvent = new shaco.SequeueEvent();
        for (int i = 0; i < coroutines.Length; ++i)
        {
            sequeueEvent = sequeueEvent.AppendEvent(coroutines[i]);
        }
        return sequeueEvent.Run();
    }
}