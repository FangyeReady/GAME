using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 事件回调信息，用于添加、删除、派发单个事件
    /// </summary>
    public class EventCallBack<T>
    {
        public delegate void CALL_FUNC_EVENT(object sender, T arg);

        /// <summary>
        /// 事件回调信息
        /// </summary>
        public class CallBackInfo
        {
            public object DefaultSender = null;
            public CALL_FUNC_EVENT CallFunc = null;
            public bool InvokeOnce = false;
            public bool WillRemove = false;
            public StackLocation CallAddEventStack = new StackLocation();
            public StackLocation CallInvokeEventStack = new StackLocation();
        }

        /// <summary>
        /// 定位日志文件用的类路径
        /// </summary>
        static private readonly string[] _classNames = new string[]
        {
            "shaco/Base/Scripts/CSharp/Event/EventManagerExtensionsClass.cs",
            "shaco/Base/Scripts/Unity/Event/EventManagerExtensionsClass.cs",
            "shaco/Base/Scripts/CSharp/Event/EventManager.cs",
            "shaco/Base/Scripts/CSharp/Event/EventCallBack"
        };

        /// <summary>
        /// 所有回调信息
        /// </summary>
        private List<CallBackInfo> _callBacks = new List<CallBackInfo>();

        /// <summary>
        /// 即将添加的回调信息
        /// </summary>
        private List<CallBackInfo> _willAddCallBacks = new List<CallBackInfo>();

        /// <summary>
        /// 即将删除的回调信息
        /// </summary>
        private List<CallBackInfo> _willRemoveCallBacks = new List<CallBackInfo>();

        /// <summary>
        /// 是否正在派发事件标志，用于解决线程冲突和事件回调锁死
        /// </summary>
        private bool _isInvoking = false;

        /// <summary>
        /// 绑定过的事件数量
        /// </summary>
        public int Count
        {
            get { return _callBacks.Count; }
        }

        /// <summary>
        /// 通过下标获取事件信息
        /// <param name="index">事件下标</param>
        /// </summary>
        public CallBackInfo this[int index]
        {
            get
            {
                if (index < 0 || index > _callBacks.Count - 1)
                {
                    Log.Exception("EventCallBackArg this[int index] error: out of range, index=" + index + " count=" + _callBacks.Count);
                    return null;
                }
                else
                {
                    return _callBacks[index];
                }
            }
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="defaultSender">绑定对象</param>
        /// <param name="callfunc">派发事件时的回调方法</param>
        /// <param name="InvokeOnce">是否只触发1次回调</param>
        public void AddCallBack(object defaultSender, CALL_FUNC_EVENT callfunc, bool invokeOnce = false)
        {
            lock (_callBacks)
            {
                if (defaultSender.IsNull() || callfunc == null)
                {
                    Log.Exception("EventCallBackArg AddCallBack error: has invalid parameters" + GetCallBackInformation(callfunc));
                }

                var callbacksTmp = GetCallBacks(defaultSender, callfunc, true);
                if (callbacksTmp.Count > 0)
                {
                    Log.Exception("EventCallBackArg AddCallBack error: has added callback by defaultSender=" + defaultSender.ToTypeString()
                                              + GetCallBackInformation(callbacksTmp[0].CallFunc));
                }

                CallBackInfo newCallBackTmp = new CallBackInfo();
                newCallBackTmp.CallAddEventStack.StartTimeSpanCalculate();
                {
                    newCallBackTmp.DefaultSender = defaultSender;
                    newCallBackTmp.CallFunc = callfunc;
                    newCallBackTmp.InvokeOnce = invokeOnce;
                    newCallBackTmp.CallAddEventStack.GetStatck(_classNames);

                    if (_isInvoking)
                    {
                        _willAddCallBacks.Add(newCallBackTmp);
                    }
                    else
                    {
                        _callBacks.Add(newCallBackTmp);
                    }
                }
                newCallBackTmp.CallAddEventStack.StopTimeSpanCalculate();
            }
        }

        /// <summary>
        /// 移除事件
        /// <param name="defaultSender">绑定对象</param>
        /// <param name="callfunc">派发事件时的回调方法</param>
        /// <return>被移除的事件信息</return>
        /// </summary>
        public CallBackInfo[] RemoveCallBack(object defaultSender, CALL_FUNC_EVENT callfunc)
        {
            lock (_callBacks)
            {
                CallBackInfo[] retValue = null;
                var callbacksTmp = GetCallBacks(defaultSender, callfunc, false);
                if (callbacksTmp.Count == 0)
                {
                    Log.Exception("EventCallBackArg RemoveCallBack error: not find callback by defaultSender=" + defaultSender.ToTypeString()
                                              + GetCallBackInformation(callfunc));
                }

                retValue = new CallBackInfo[callbacksTmp.Count];
                for (int i = 0; i < callbacksTmp.Count; ++i)
                {
                    retValue[i] = callbacksTmp[i];

                    if (_isInvoking)
                    {
                        if (!_willRemoveCallBacks.Contains(callbacksTmp[i]))
                        {
                            callbacksTmp[i].WillRemove = true;
                            _willRemoveCallBacks.Add(callbacksTmp[i]);
                        }
                    }
                    else
                    {
                        _callBacks.Remove(callbacksTmp[i]);
                    }
                }

                return retValue;
            }
        }

        /// <summary>
        /// 移除事件
        /// <param name="callfunc">派发事件时的回调方法</param>
        /// <return>被移除的事件信息</return>
        /// </summary>
        public CallBackInfo[] RemoveCallBack(CALL_FUNC_EVENT callfunc)
        {
            return RemoveCallBack(null, callfunc);
        }

        /// <summary>
        /// 移除事件
        /// <param name="defaultSender">绑定对象</param>
        /// <return>被移除的事件信息</return>
        /// </summary>
        public CallBackInfo[] RemoveCallBack(object defaultSender)
        {
            return RemoveCallBack(defaultSender, null);
        }

        /// <summary>
        /// 清空所有事件
        /// </summary>
        public void ClearCallBack()
        {
            lock (_callBacks)
            {
                if (_isInvoking)
                {
                    for (int i = _callBacks.Count - 1; i >= 0; --i)
                    {
                        if (!_willRemoveCallBacks.Contains(_callBacks[i]))
                        {
                            _callBacks[i].WillRemove = true;
                            _willRemoveCallBacks.Add(_callBacks[i]);
                        }
                    }
                }
                else
                {
                    _callBacks.Clear();
                }
            }
        }

        /// <summary>
        /// 
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        public bool HasCallBack(object defaultSender)
        {
            return GetCallBacksIndex(_callBacks, defaultSender, null, true).Count > 0
            || GetCallBacksIndex(_willAddCallBacks, defaultSender, null, true).Count > 0;
        }

        public bool HasCallBack(CALL_FUNC_EVENT callfunc)
        {
            return GetCallBacksIndex(_callBacks, null, callfunc, true).Count > 0
            || GetCallBacksIndex(_willAddCallBacks, null, callfunc, true).Count > 0;
        }

        public void InvokeAllCallBack(object defaultSender, T arg)
        {
            InvokeAllCallBackBase(defaultSender, arg);
        }

        public void InvokeAllCallBack(T arg)
        {
            InvokeAllCallBackBase(null, arg);
        }

        private void InvokeAllCallBackBase(object sender, T arg)
        {
            lock (_callBacks)
            {
                _isInvoking = true;
                for (int i = 0; i < _callBacks.Count; ++i)
                {
                    if (null == _callBacks)
                    {
                        Log.Error("EventCallBackArg invokeAllCallBack erorr: _callBacks is null !");
                        break;
                    }
                    if (i >= _callBacks.Count)
                    {
                        Log.Error("EventCallBackArg invokeAllCallBack erorr: index out of range ! index=" + i + " callback count=" + _callBacks.Count);
                        break;
                    }

                    CallBackInfo callbackInfoTmp = _callBacks[i];
                    callbackInfoTmp.CallInvokeEventStack.GetStatck(_classNames);

                    if (null == callbackInfoTmp)
                    {
                        Log.Error("EventCallBackArg invokeAllCallBack erorr: callback information is null !");
                        continue;
                    }

                    if (callbackInfoTmp.DefaultSender.IsNull())
                    {
                        if (!_willRemoveCallBacks.Contains(_callBacks[i])) _willRemoveCallBacks.Add(_callBacks[i]);
                        Log.Exception("EventCallBackArg invokeAllCallBack error: sender is missing, we will remove it" + GetCallBackInformation(callbackInfoTmp.CallFunc));
                    }

                    if (null != callbackInfoTmp.CallFunc)
                    {
                        if (!callbackInfoTmp.WillRemove)
                        {
                            callbackInfoTmp.CallInvokeEventStack.StartTimeSpanCalculate();
                            {
                                callbackInfoTmp.CallFunc(sender == null ? callbackInfoTmp.DefaultSender : sender, arg);
                            }
                            callbackInfoTmp.CallInvokeEventStack.StopTimeSpanCalculate();
                        }
                    }
                    else
                    {
                        Log.Warning("EventCallBackArg invokeAllCallBack warning: callback function is missing" + GetCallBackInformation(callbackInfoTmp.CallFunc));
                    }

                    if (callbackInfoTmp.InvokeOnce)
                    {
                        if (!_willRemoveCallBacks.Contains(_callBacks[i])) _willRemoveCallBacks.Add(_callBacks[i]);
                    }
                }
                _isInvoking = false;

                //check remove 
                if (_willRemoveCallBacks.Count > 0)
                {
                    for (int i = _willRemoveCallBacks.Count - 1; i >= 0; --i)
                    {
                        var callbackInfoTmp = _willRemoveCallBacks[i];
                        RemoveCallBack(callbackInfoTmp.DefaultSender, callbackInfoTmp.CallFunc);
                    }
                    _willRemoveCallBacks.Clear();
                }

                //check add
                if (_willAddCallBacks.Count > 0)
                {
                    for (int i = _willAddCallBacks.Count - 1; i >= 0; --i)
                    {
                        var callbackInfoTmp = _willAddCallBacks[i];
                        AddCallBack(callbackInfoTmp.DefaultSender, callbackInfoTmp.CallFunc, callbackInfoTmp.InvokeOnce);
                    }
                    _willAddCallBacks.Clear();
                }
            }
        }

        private List<CallBackInfo> GetCallBacks(object defaultSender, CALL_FUNC_EVENT callfunc, bool findOne)
        {
            lock (_callBacks)
            {
                var indexesTmp1 = GetCallBacksIndex(_callBacks, defaultSender, callfunc, findOne);
                var indexesTmp2 = GetCallBacksIndex(_willAddCallBacks, defaultSender, callfunc, findOne);
                var retValue = new List<CallBackInfo>();

                for (int i = 0; i < indexesTmp1.Count; ++i)
                {
                    retValue.Add(_callBacks[indexesTmp1[i]]);
                }
                for (int i = 0; i < indexesTmp2.Count; ++i)
                {
                    retValue.Add(_willAddCallBacks[indexesTmp2[i]]);
                }

                return retValue;
            }
        }

        private List<int> GetCallBacksIndex(List<CallBackInfo> callbacks, object defaultSender, CALL_FUNC_EVENT callfunc, bool findOne)
        {
            lock (_callBacks)
            {
                var retValue = new List<int>();

                if (defaultSender == null && callfunc == null)
                {
                    return retValue;
                }
                else if (defaultSender != null && callfunc == null)
                {
                    for (int i = callbacks.Count - 1; i >= 0; --i)
                    {
                        if (callbacks[i].DefaultSender.Equals(defaultSender))
                        {
                            retValue.Add(i);
                            if (findOne)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (defaultSender == null && callfunc != null)
                {
                    for (int i = callbacks.Count - 1; i >= 0; --i)
                    {
                        if (callbacks[i].CallFunc == callfunc)
                        {
                            retValue.Add(i);
                            if (findOne)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (defaultSender != null && callfunc != null)
                {
                    for (int i = callbacks.Count - 1; i >= 0; --i)
                    {
                        if (callbacks[i].DefaultSender.Equals(defaultSender) && callbacks[i].CallFunc == callfunc)
                        {
                            retValue.Add(i);
                            if (findOne)
                            {
                                break;
                            }
                        }
                    }
                }

                return retValue;
            }
        }

        private string GetCallBackInformation(CALL_FUNC_EVENT callfunc)
        {
            if (null == callfunc)
            {
                return "<<CallBack is null>>";
            }
            else
            {
                string targetDescription = callfunc.IsNull() || callfunc.Target.IsNull() ? "null" : callfunc.Target.GetType().ToString();
                string methodDescription = callfunc.IsNull() ? "null" : callfunc.Method.ToString();
                return "<<Sender type=" + targetDescription + ">> <<Callfunc method=" + methodDescription + ">>";
            }
        }
    }
}

