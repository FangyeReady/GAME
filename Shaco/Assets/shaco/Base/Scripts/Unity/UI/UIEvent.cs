using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace shaco
{
    public class UIEvent
    {
        public enum EventType
        {
            None,
            OnPreLoad,
            OnInit,
            OnOpen,
            OnHide,
            OnResume,
            OnClose,
            OnCustom,

            EventCount
        }

        public class UIEventInfo
        {
            public shaco.Base.StackLocation statckLocationUI = new shaco.Base.StackLocation();
        }

        public UIEventInfo[] uiEventInfo = new UIEventInfo[(int)EventType.EventCount];

        static public readonly string[] UI_CLASS_PATH = new string[] { "shaco/Base/Scripts/Unity/UI/UIManager.cs", "shaco/Base/Scripts/Unity/UI/UIRootComponent.cs" };

        private bool _isPreLoaded = false;

        public UIEvent()
        {
            RestStatckLocations();
        }

        public void RestStatckLocations()
        {
            for (int i = this.uiEventInfo.Length - 1; i >= 0; --i)
            {
                if (this.uiEventInfo[i] == null)
                    this.uiEventInfo[i] = new UIEventInfo();
                else
                {
                    this.uiEventInfo[i].statckLocationUI.Reset();
                }
            }
        }

        public bool DispatchEvent(shaco.IUIState uiState, shaco.Base.BaseEventArg arg, EventType type)
        {
            bool hasError = false;
            int count = uiState.prefabs.Count;
            for (int i = 0; i < count; ++i)
            {
                var prefabTmp = uiState.prefabs[i];
                hasError |= !DispatchEvent(uiState.key, prefabTmp, arg, type);
            }
            return !hasError;
        }

        public bool DispatchEvent(string key, UIPrefab uiPrefab, shaco.Base.BaseEventArg arg, EventType type)
        {
            var uiEventInfo = this.uiEventInfo[(int)type];
            var methodTarget = uiPrefab.prefab;

            //dont dispatch event when target is inactive in hierarchy
            if (null == methodTarget || !methodTarget.activeInHierarchy && type != EventType.OnPreLoad)
            {
                return false;
            }

            uiEventInfo.statckLocationUI.GetStatck(UI_CLASS_PATH);
            uiEventInfo.statckLocationUI.StartTimeSpanCalculate();

            switch (type)
            {
                case EventType.OnPreLoad:
                {
                    InitEventMethod(uiPrefab);
                    InvokeMethod(methodTarget, uiPrefab.methodOnPreLoad, arg);
                    _isPreLoaded = true;
                    break;
                }
                case EventType.OnInit:
                    {
                        if (!_isPreLoaded)
                        {
                            InitEventMethod(uiPrefab);
                        }
                        InvokeMethod(methodTarget, uiPrefab.methodOnInit, arg);
                        break;
                    }
                case EventType.OnOpen: InvokeMethod(methodTarget, uiPrefab.methodOnOpen, arg); break;
                case EventType.OnHide: InvokeMethod(methodTarget, uiPrefab.methodOnHide, arg); break;
                case EventType.OnResume: InvokeMethod(methodTarget, uiPrefab.methodOnResume, arg); break;
                case EventType.OnClose: InvokeMethod(methodTarget, uiPrefab.methodOnClose, arg); break;
                case EventType.OnCustom: InvokeMethod(methodTarget, uiPrefab.methodOnCustom, arg); break;
                default: Log.Error("DispatchEvent error: unsupport event type=" + type); break;
            }

            uiEventInfo.statckLocationUI.StopTimeSpanCalculate();
            UIStateChangeSave.SaveUIStateChangedInfo(key, uiPrefab, uiEventInfo.statckLocationUI, type);

            DispatchUIStateChangedEvent(key, uiPrefab.prefab, type);
            return true;
        }

        private void DispatchUIStateChangedEvent(string key, GameObject prefab, EventType type)
        {
            UIStateChangedArgs.OnUIStateChangedBaseArg dispatchArgTmp = null;

            switch (type)
            {
                case EventType.OnPreLoad:
                    {
                        if (shaco.Base.EventManager.HasEventID<UIStateChangedArgs.OnUIPreLoadArg>())
                            dispatchArgTmp = new UIStateChangedArgs.OnUIPreLoadArg();
                        break;
                    }
                case EventType.OnInit:
                    {
                        if (shaco.Base.EventManager.HasEventID<UIStateChangedArgs.OnUIInitArg>())
                            dispatchArgTmp = new UIStateChangedArgs.OnUIInitArg();
                        break;
                    }
                case EventType.OnOpen:
                    {
                        if (shaco.Base.EventManager.HasEventID<UIStateChangedArgs.OnUIOpenArg>())
                            dispatchArgTmp = new UIStateChangedArgs.OnUIOpenArg();
                        break;
                    }
                case EventType.OnHide:
                    {
                        if (shaco.Base.EventManager.HasEventID<UIStateChangedArgs.OnUIHideArg>())
                            dispatchArgTmp = new UIStateChangedArgs.OnUIHideArg();
                        break;
                    }
                case EventType.OnResume:
                    {
                        if (shaco.Base.EventManager.HasEventID<UIStateChangedArgs.OnUIResumeArg>())
                            dispatchArgTmp = new UIStateChangedArgs.OnUIResumeArg();
                        break;
                    }
                case EventType.OnClose:
                    {
                        if (shaco.Base.EventManager.HasEventID<UIStateChangedArgs.OnUICloseArg>())
                            dispatchArgTmp = new UIStateChangedArgs.OnUICloseArg();
                        break;
                    }
                case EventType.OnCustom:
                    {
                        if (shaco.Base.EventManager.HasEventID<UIStateChangedArgs.OnUICustomArg>())
                            dispatchArgTmp = new UIStateChangedArgs.OnUICustomArg();
                        break;
                    }
                default: Log.Error("DispatchUIStateChangedEvent error: unsupport event type=" + type); break;
            }

            if (null != dispatchArgTmp)
            {
                dispatchArgTmp.uiKey = key;
                dispatchArgTmp.uiTarget = prefab.gameObject;
                dispatchArgTmp.uiTarget.InvokeEvent(dispatchArgTmp);
            }
        }

        private void InitEventMethod(shaco.UIPrefab uiPrefab)
        {
            bool haveMethod = false;

            //clear all invoke method
            uiPrefab.ClearAllMethod();

            for (int j = uiPrefab.componets.Length - 1; j >= 0; --j)
            {
                var componet = uiPrefab.componets[j];
                haveMethod |= SaveMethod(uiPrefab.methodOnPreLoad, componet, "OnUIPreLoad");
                haveMethod |= SaveMethod(uiPrefab.methodOnInit, componet, "OnUIInit");
                haveMethod |= SaveMethod(uiPrefab.methodOnOpen, componet, "OnUIOpen");
                haveMethod |= SaveMethod(uiPrefab.methodOnHide, componet, "OnUIHide");
                haveMethod |= SaveMethod(uiPrefab.methodOnResume, componet, "OnUIResume");
                haveMethod |= SaveMethod(uiPrefab.methodOnClose, componet, "OnUIClose");
                haveMethod |= SaveMethod(uiPrefab.methodOnCustom, componet, "OnUICustom");
            }

            if (!haveMethod)
            {
                Log.Warning("UIEvent InitEventMethod error: no method found, target=" + uiPrefab.prefab);
            }
        }

        private void InvokeMethod(object target, List<MethodInfoEx> methods, shaco.Base.BaseEventArg arg)
        {
            for (int i = methods.Count - 1; i >= 0; --i)
            {
                var method = methods[i];
                if (null == method || null == method.method)
                    Log.Error("UIEvent InvokeMethod error: method is missing !");
                else
                {
                    method.method.Invoke(method.target, new object[] { arg });
                }
            }
        }

        private bool SaveMethod(List<MethodInfoEx> methods, object target, string methodName)
        {
            if (null == target)
                return false;

            
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var method = target.GetType().GetMethod(methodName, flag);
            if (null != method)
            {
                var newMethod = new MethodInfoEx();
                newMethod.target = target;
                newMethod.method = method;
                methods.Add(newMethod);
            }
            return methods.Count > 0;
        }
    }
}