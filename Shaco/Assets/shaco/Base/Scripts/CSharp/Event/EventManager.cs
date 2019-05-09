using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 事件管理类，用于添加、删除、派发多个事件，具有穿透性和易管理性
    /// </summary>
    public partial class EventManager
    {
        /// <summary>
        /// 事件回调信息
        /// <param name="eventID">事件ID</param>
        /// <param name="CallBack">回调方法</param>
        /// </summary>
        public class CallBackInfo
        {
            public string eventID = string.Empty;

            public EventCallBack<BaseEventArg> CallBack = new EventCallBack<BaseEventArg>();
        }

        /// <summary>
        /// 是否启用该事件管理器
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// 当前绑定过的事件数量
        /// </summary>
        public int Count
        {
            get { return _callBacks.Count; }
        }

        /// <summary>
        /// 事件线程锁，支持异步访问事件
        /// </summary>
        static private System.Threading.Mutex _mutexCallBacks = new System.Threading.Mutex();

        /// <summary>
        /// 所有事件回调信息<事件ID，回调信息>
        /// </summary>
        private Dictionary<string, CallBackInfo> _callBacks = new Dictionary<string, CallBackInfo>();

        /// <summary>
        /// 内部使用事件缓存，用于快速添加和查找<事件ID，<事件ID，绑定对象>>
        /// </summary>
        private Dictionary<object, Dictionary<string, object>> _defaultSenderToEventIDs = new Dictionary<object, Dictionary<string, object>>();

        /// <summary>
        /// 添加(绑定事件)
        /// </summary>
        /// <param name="defaultSender">默认事件绑定对象，不能为空</param>
        /// <param name="callfunc">事件回调函数</param>
        /// <param name="invokeOnce">是否只触发1次回调</param>
        static public bool AddEvent<T>(object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce = false) where T : BaseEventArg
        {
            string eventID = Utility.ToTypeString<T>();
            return AddEvent(eventID, defaultSender, callfunc, invokeOnce);
        }

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="T">事件类型</param>
        /// <param name="callfunc">事件回调函数</param>
        /// <return>true:移除成功，false:移除失败</return>
        static public bool RemoveEvent<T>(EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc) where T : BaseEventArg
        {
            return RemoveEvent(null, callfunc, typeof(T));
        }

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="callfunc">事件回调函数</param>
        /// <param name="type">事件类型</param>
        /// <return>true:移除成功，false:移除失败</return>
        static public bool RemoveEvent(EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, System.Type type)
        {
            return RemoveEvent(null, callfunc, type);
        }

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="T">事件类型</param>
        /// <param name="defaultSender">事件绑定的对象</param>
        /// <param name="callfunc">事件回调函数</param>
        /// <return>true:移除成功，false:移除失败</return>
        static public bool RemoveEvent<T>(object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc)
        {
            return RemoveEvent(defaultSender, callfunc, typeof(T));
        }

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="defaultSender">事件绑定的对象</param>
        /// <param name="callfunc">事件回调函数</param>
        /// <param name="type">事件类型</param>
        /// <return>true:移除成功，false:移除失败</return>
        static public bool RemoveEvent(object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, System.Type type)
        {
            string eventID = type.ToTypeString();
            if (!HasEventID(eventID))
            {
                Log.Exception("EventManager Remove by callfunc error: not find event id=" + eventID);
            }

            var callBackInfosTmp = GetCurrentEventManager().GetEvent(eventID);
            Remove(eventID, defaultSender, callfunc, callBackInfosTmp);

            UseCurrentEventManagerEnd();
            return true;
        }

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="T">事件类型</param>
        /// <param name="defaultSender">事件绑定的对象</param>
        static public bool RemoveEvent<T>(object defaultSender) where T : BaseEventArg
        {
            return RemoveEvent(defaultSender, null, typeof(T));
        }

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="defaultSender">事件绑定的对象</param>
        /// <param name="type">事件类型</param>
        static public bool RemoveEvent(object defaultSender, System.Type type)
        {
            return RemoveEvent(defaultSender, null, type);
        }

        /// <summary>
        /// 移除defaultSender绑定过的所有事件
        /// </summary>
        /// <param name="defaultSender">事件绑定的对象</param>
        static public void RemoveAllEvent(object defaultSender)
        {
            var allCallBackInfosTmp = GetAllCallBackInformation(defaultSender);
            foreach (var value in allCallBackInfosTmp.Values)
            {
                for (int i = value.Count - 1; i >= 0; --i)
                {
                    var callBackInfosTmp = value[i];
                    if (null != callBackInfosTmp)
                        Remove(callBackInfosTmp.eventID, defaultSender, null, callBackInfosTmp);
                }
            }
            System.GC.Collect();
        }

        /// <summary>
        /// 移除T类型相关的所有添加过的事件
        /// </summary>
        /// <param name="T">事件类型</param>
        /// <return>true:移除成功，false:移除失败</return>
        static public bool RemoveEvent<T>() where T : BaseEventArg
        {
            return RemoveEvent(typeof(T));
        }

        /// <summary>
        /// 移除type类型相关的所有添加过的事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <return>true:移除成功，false:移除失败</return>
        static public bool RemoveEvent(System.Type type)
        {
            string eventID = type.ToTypeString();
            if (!HasEventID(eventID))
            {
                Log.Exception("EventManager Remove by event argument error: not find event id=" + eventID);
            }

            RemoveEventID(eventID);
            return true;
        }

        /// <summary>
        /// 清空所有事件监听
        /// </summary>
        static public void ClearEvent()
        {
            lock (_mutexCallBacks)
            {
                var currentEventManager = GetCurrentEventManager();
                currentEventManager._callBacks.Clear();
                UseCurrentEventManagerEnd();
            }
        }

        /// <summary>
        /// 获取T类型绑定过该事件的所有回调信息
        /// </summary>
        /// <param name="T">事件类型</param>
        /// <return>所有T类型相关的回调信息</return>
        static public EventCallBack<BaseEventArg> GetEvents<T>() where T : BaseEventArg
        {
            lock (_mutexCallBacks)
            {
                string eventID = Utility.ToTypeString<T>();
                EventCallBack<BaseEventArg> retValue = null;
                if (!HasEventID<T>())
                {
                    Log.Exception("EventManager Get error: not find event id=" + eventID);
                }

                retValue = GetCurrentEventManager().GetEvent(eventID).CallBack;

                UseCurrentEventManagerEnd();
                return retValue;
            }
        }

        /// <summary>
        /// 获取默认绑定对象相关的所有回调信息
        /// </summary>
        /// <param name="defaultSender">事件绑定的对象</param>
        /// <returns>返回字典对象[key:事件ID, value:回调方法信息]</returns>
        static public Dictionary<string, List<EventCallBack<BaseEventArg>.CallBackInfo>> GetEvents(object defaultSender)
        {
            lock (_mutexCallBacks)
            {
                var retValue = new Dictionary<string, List<EventCallBack<BaseEventArg>.CallBackInfo>>();
                var allCallBackInfosTmp = GetAllCallBackInformation(defaultSender);

                foreach (var key in allCallBackInfosTmp.Keys)
                {
                    var value = allCallBackInfosTmp[key];
                    for (int i = value.Count - 1; i >= 0; --i)
                    {
                        var callbackInfosTmp = value[i];
                        for (int j = callbackInfosTmp.CallBack.Count - 1; j >= 0; --j)
                        {
                            var callbackInfoTmp = callbackInfosTmp.CallBack[j];
                            if (callbackInfoTmp.DefaultSender.Equals(defaultSender))
                            {
                                if (!retValue.ContainsKey(key))
                                {
                                    retValue.Add(key, new List<EventCallBack<BaseEventArg>.CallBackInfo>());
                                }
                                retValue[key].Add(callbackInfoTmp);
                            }
                        }
                    }
                }

                return retValue;
            }
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="sender">派发事件的对象，如果为空，则会使用默认事件绑定对象</param>
        /// <param name="arg">事件参数</param>
        /// <return>true:派发成功，false:派发失败</return>
        static public bool InvokeEvent(object sender, BaseEventArg arg)
        {
            lock (_mutexCallBacks)
            {
                var currentEventManager = GetCurrentEventManager();
                string eventID = arg.eventID;
                var callbackTmp = currentEventManager.GetEvent(eventID);

                if (null == callbackTmp)
                {
                    Log.Exception("EventManager Invoke error: not find event id=" + eventID);
                }

                if (!currentEventManager.Enabled)
                {
                    Log.Exception("EventManager Invoke error: event manager is disabled ! manager id=" + _currentEventManagerID);
                }

                if (callbackTmp.CallBack.Count == 0)
                {
                    RemoveEventID(eventID);
                    Log.Exception("EventManager Invoke error: not find callback function by <<event id=" + eventID + ">>we will remove event id");
                }

                callbackTmp.CallBack.InvokeAllCallBack(sender, arg);
                if (callbackTmp.CallBack.Count == 0)
                {
                    CheckCallBackEmpty(eventID);
                }

                UseCurrentEventManagerEnd();
                return true;
            }
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="arg">事件参数</param>
        /// <return>true:派发成功，false:派发失败</return>
        static public bool InvokeEvent(BaseEventArg arg)
        {
            return InvokeEvent(null, arg);
        }

        /// <summary>
        /// 是否已经添加过T类型绑定过的事件
        /// </summary>
        /// <return>true:包含该事件，false:不包含该事件</return>
        static public bool HasEventID<T>() where T : BaseEventArg
        {
            string eventID = Utility.ToTypeString<T>();
            return HasEventID(eventID);
        }

        /// <summary>
        /// 遍历所有的事件
        /// </summary>
        /// <param name="callfunc">遍历事件回调方法<事件ID，事件回调信息, [返回值，true：继续遍历 false：停止遍历]></param>
        public void Foreach(System.Func<string, CallBackInfo, bool> callfunc)
        {
            lock (_mutexCallBacks)
            {
                var callbacksTmp = this._callBacks;
                if (null == callbacksTmp)
                {
                    Log.Exception("EventManager Foreach error: event manager is disabled !");
                }
                if (null == callfunc)
                {
                    Log.Exception("EventManager Foreach error: callfunc is null");
                }

                foreach (var value in callbacksTmp.Values)
                {
                    if (!callfunc(value.eventID, value))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 是否已经添加过T类型绑定过的事件
        /// </summary>
        /// <param name="eventID">事件ID</param>
        /// <return>true:包含该事件，false:不包含该事件</return>
        static private bool HasEventID(string eventID)
        {
            lock (_mutexCallBacks)
            {
                var currentEventManager = GetCurrentEventManager();
                bool retValue = currentEventManager._callBacks.ContainsKey(eventID);
                UseCurrentEventManagerEnd();
                return retValue;
            }
        }

        /// <summary>
        /// defalutSender对象是否有绑定过事件
        /// </summary>
        /// <param name="defaultSender">事件绑定对象</param>
        /// <return>true:绑定过事件，false:没绑定过事件</return>
        static private bool HasDefaultSenderWithEventID(object defaultSender)
        {
            bool retValue = defaultSender != null ? GetCurrentEventManager()._defaultSenderToEventIDs.ContainsKey(defaultSender) : false;
            UseCurrentEventManagerEnd();
            return retValue;
        }

        /// <summary>
        /// 派发事件，调用事件回调方法
        /// </summary>
        /// <param name="eventID">事件ID</param>
        static private void CheckCallBackEmpty(string eventID)
        {
            var currentEventManager = GetCurrentEventManager();
            if (currentEventManager._callBacks.ContainsKey(eventID))
            {
                if (currentEventManager.GetEvent(eventID).CallBack.Count == 0)
                {
                    RemoveEventID(eventID);
                }
            }
            UseCurrentEventManagerEnd();
        }

        /// <summary>
        /// 添加(绑定事件)
        /// </summary>
        /// <param name="defaultSender">事件绑定对象</param>
        /// <param name="callfunc">事件派发时的回调方法</param>
        /// <param name="invokeOnce">是否只触发1次回调</param>
        /// <return>true:绑定事件成功，false:绑定事件失败</return>
        static private bool AddEvent(string eventID, object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce)
        {
            lock (_mutexCallBacks)
            {
                var currentEventManager = GetCurrentEventManager();
                CallBackInfo callbackInfoTmp = null;

                if (defaultSender.IsNull())
                {
                    shaco.Log.Error("EventManager AddEvent error: defaultSender is null !");
                    return false;
                }

                if (!HasEventID(eventID))
                {
                    currentEventManager._callBacks.Add(eventID, new CallBackInfo());
                }
                callbackInfoTmp = currentEventManager.GetEvent(eventID);


                SafeAddDefaultSenderEventID(defaultSender, eventID);
                callbackInfoTmp.eventID = eventID;
                callbackInfoTmp.CallBack.AddCallBack(defaultSender, callfunc, invokeOnce);

                UseCurrentEventManagerEnd();
                return true;
            }
        }

        /// <summary>
        /// 获取事件回调信息
        /// </summary>
        /// <param name="eventID">事件ID</param>
        /// <return>事件回调信息</return>
        private CallBackInfo GetEvent(string eventID)
        {
            CallBackInfo retValue = null;

            if (_callBacks.ContainsKey(eventID))
            {
                retValue = _callBacks[eventID];
            }
            return retValue;
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventID">事件ID</param>
        static private void RemoveEventID(string eventID)
        {
            lock (_mutexCallBacks)
            {
                var currentEventManager = GetCurrentEventManager();
                var callbackInfoTmp = currentEventManager.GetEvent(eventID);
                callbackInfoTmp.CallBack.ClearCallBack();

                currentEventManager._callBacks.Remove(eventID);

                CheckRemoveDefaultSenderEventID(eventID, callbackInfoTmp);
                UseCurrentEventManagerEnd();
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventID">事件ID</param>
        /// <param name="defaultSender">事件绑定对象</param>
        /// <param name="callfunc">事件派发时的回调方法</param>
        /// <param name="callbackInfos">事件回调信息</param>
        static private void Remove(string eventID, object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, CallBackInfo callbackInfos)
        {
            lock (_mutexCallBacks)
            {
                EventCallBack<BaseEventArg>.CallBackInfo[] RemoveCallBacks = null;

                if (defaultSender != null && callfunc == null)
                {
                    RemoveCallBacks = callbackInfos.CallBack.RemoveCallBack(defaultSender);
                }
                else if (defaultSender == null && callfunc != null)
                {
                    RemoveCallBacks = callbackInfos.CallBack.RemoveCallBack(callfunc);
                }
                else if (defaultSender != null && callfunc != null)
                {
                    RemoveCallBacks = callbackInfos.CallBack.RemoveCallBack(defaultSender, callfunc);
                }
                else
                {
                    Log.Exception("EventManager Remove error: invalid parameter !");
                }

                for (int i = RemoveCallBacks.Length - 1; i >= 0; --i)
                {
                    if (!callbackInfos.CallBack.HasCallBack(RemoveCallBacks[i].DefaultSender))
                    {
                        SafeRemoveDefaultSenderEventID(RemoveCallBacks[i].DefaultSender, eventID);
                    }
                }
                CheckCallBackEmpty(eventID);
            }
        }

        /// <summary>
        /// 检查事件并从缓存中安全移除
        /// </summary>
        /// <param name="eventID">事件ID</param>
        /// <param name="callbackInfo">事件回调信息</param>
        static private void CheckRemoveDefaultSenderEventID(string eventID, CallBackInfo callbackInfo)
        {
            for (int i = callbackInfo.CallBack.Count - 1; i >= 0; --i)
            {
                SafeRemoveDefaultSenderEventID(callbackInfo.CallBack[i].DefaultSender, eventID);
            }
        }

        /// <summary>
        /// 检查事件并安全添加到缓存中
        /// </summary>
        /// <param name="defaultSender">事件绑定对象</param>
        /// <param name="eventID">事件ID</param>
        static private void SafeAddDefaultSenderEventID(object defaultSender, string eventID)
        {
            var mapTmp = GetCurrentEventManager()._defaultSenderToEventIDs;
            if (!mapTmp.ContainsKey(defaultSender))
            {
                mapTmp.Add(defaultSender, new Dictionary<string, object>());
            }
            if (!mapTmp[defaultSender].ContainsKey(eventID))
            {
                mapTmp[defaultSender].Add(eventID, defaultSender);
            }
            UseCurrentEventManagerEnd();
        }

        /// <summary>
        /// 检查事件并从缓存中安全移除
        /// </summary>
        /// <param name="defaultSender">事件绑定对象</param>
        /// <param name="eventID">事件ID</param>
        static private void SafeRemoveDefaultSenderEventID(object defaultSender, string eventID)
        {
            var mapTmp = GetCurrentEventManager()._defaultSenderToEventIDs;
            if (mapTmp.ContainsKey(defaultSender) && mapTmp[defaultSender].ContainsKey(eventID))
            {
                var eventIDsTmp = mapTmp[defaultSender];
                eventIDsTmp.Remove(eventID);
                if (eventIDsTmp.Count == 0)
                {
                    mapTmp.Remove(defaultSender);
                }
            }
            UseCurrentEventManagerEnd();
        }

        /// <summary>
        /// 获取对象绑定过的所有事件信息
        /// </summary>
        /// <param name="defaultSender">事件绑定对象</param>
        /// <return>所有绑定过对象的事件信息<事件ID，回调信息></return>
        static private Dictionary<string, List<CallBackInfo>> GetAllCallBackInformation(object defaultSender)
        {
            Dictionary<string, List<CallBackInfo>> retValue = new Dictionary<string, List<CallBackInfo>>();
            if (!HasDefaultSenderWithEventID(defaultSender))
            {
                return retValue;
            }

            var currentEventManager = GetCurrentEventManager();
            var eventIDsTmp = currentEventManager._defaultSenderToEventIDs[defaultSender];

            foreach (var key in eventIDsTmp.Keys)
            {
                var callbackInfosTmp = currentEventManager.GetEvent(key);
                if (!retValue.ContainsKey(key))
                {
                    retValue.Add(key, new List<CallBackInfo>());
                }
                retValue[key].Add(callbackInfosTmp);
            }

            UseCurrentEventManagerEnd();
            return retValue;
        }
    }
}

