using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using shaco.Base;
using shaco;

namespace shacoEditor
{
    public class EventManagerEditor : EditorWindow
    {
        private enum PanelType
        {
            API,
            Location
        }

        private readonly int MAX_SHOW_EVENT_COUNT = 50;

        private Vector2 _vec2ScrollPosition = Vector2.zero;
        private string _searchName = string.Empty;
        private bool _isSearchEventID = true;
        private object _currentSelectSender = null;
        private bool _isCurrentSelectSenderChanged = false;
        private Dictionary<string, List<EventCallBack<BaseEventArg>.CallBackInfo>> _searchCallBackInfos = new Dictionary<string, List<EventCallBack<BaseEventArg>.CallBackInfo>>();
        private bool _isTestBusy = false;
        private PanelType _panelType = PanelType.Location;

        private EventManagerEditor _currentWindow = null;


        [MenuItem("shaco/Viewer/EventViewer %&2", false, (int)ToolsGlobalDefine.MenuPriority.Viewer.EVENT_MANAGER)]
        static void Open()
        {
            EditorHelper.GetWindow<EventManagerEditor>(null, true, "EventViewer");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow(this, true, "EventViewer");
        }

        void OnGUI()
        {
            if (_currentWindow == null)
            {
                return;
            }
            this.Repaint();

            _vec2ScrollPosition = GUILayout.BeginScrollView(_vec2ScrollPosition);
            DrawSearchEvent();
            DrawAllEvent(shaco.Base.EventManager.GetCurrentEventManager());
            shaco.Base.EventManager.UseCurrentEventManagerEnd();

            GUILayout.EndScrollView();
        }

        private void DrawSearchEvent()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(_currentWindow.position.width / 3);
                GUILayout.Label("Search Mode: ", GUILayout.Width(_currentWindow.position.width / 8));
                if (GUILayout.Button(_isSearchEventID ? "Event ID" : "Sender"))
                {
                    _isSearchEventID = !_isSearchEventID;
                }

                if (_isSearchEventID)
                {
                    _searchName = GUILayoutHelper.SearchField(_searchName);
                }
                else
                {
                    var selectsTmp = Selection.GetFiltered(typeof(object), SelectionMode.TopLevel);
                    if (selectsTmp == null || selectsTmp.Length != 1)
                    {
                        GUILayout.Label("Please select a target in unity editor");
                    }
                    else
                    {
                        if (_currentSelectSender != (object)selectsTmp[0])
                        {
                            _searchCallBackInfos.Clear();
                            _isCurrentSelectSenderChanged = true;
                        }
                        _currentSelectSender = selectsTmp[0];
                        EditorGUILayout.ObjectField(selectsTmp[0], typeof(object), true);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAllEvent(shaco.Base.EventManager eventManager)
        {
            if (null == eventManager)
            {
                return;
            }

            int drawCount = 0;

            GUILayout.Label("Count: " + eventManager.Count);

            if (!_isSearchEventID)
            {
                if (_isCurrentSelectSenderChanged && _currentSelectSender != null)
                {
                    _searchCallBackInfos = shaco.Base.EventManager.GetEvents(_currentSelectSender);
                    _isCurrentSelectSenderChanged = false;
                }

                foreach (var key in _searchCallBackInfos.Keys)
                {
                    GUILayout.BeginVertical("box");
                    {
                        var value = _searchCallBackInfos[key];
                        if (DrawEventHeader(key, value.Count, false))
                        {
                            for (int i = 0; i < value.Count; ++i)
                            {
                                if (!DrawEvent(key, value[i]))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    if (++drawCount >= MAX_SHOW_EVENT_COUNT) break;
                }
            }
            else
            {
                eventManager.Foreach((string eventID, shaco.Base.EventManager.CallBackInfo callbackInfo) =>
                {
                    if (!string.IsNullOrEmpty(_searchName) && !eventID.ToLower().Contains(_searchName.ToLower()))
                    {
                        return true;
                    }

                    GUILayout.BeginVertical("box");
                    {
                        DrawEvents(eventID, callbackInfo);
                    }
                    GUILayout.EndVertical();

                    if (_isTestBusy)
                    {
                        _isTestBusy = false;
                        return false;
                    }
                    return ++drawCount < MAX_SHOW_EVENT_COUNT;
                });
            }
        }

        private bool DrawEventHeader(string eventID, int eventCount, bool drawInvoke)
        {
            bool isOpen = true;
            GUILayout.BeginHorizontal();
            {
                isOpen = GUILayoutHelper.DrawHeader("Event ID: " + eventID + "(Count: " + eventCount + ")", eventID, null, GUILayout.Width(_currentWindow.position.width / 3 * 2));

                if (drawInvoke)
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(0);
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Invoke", GUILayout.Width(50)))
                            {
                                _isTestBusy = true;
                                var defaultEventArg = (BaseEventArg)typeof(BaseEventArg).Assembly.CreateInstance(eventID);
                                EventManager.InvokeEvent(defaultEventArg);
                            }
                            if (GUILayout.Button("Delete", GUILayout.Width(50)))
                            {
                                _isTestBusy = true;
                                var defaultEventArg = (BaseEventArg)typeof(BaseEventArg).Assembly.CreateInstance(eventID);
                                EventManager.RemoveEvent(defaultEventArg.GetType());
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndHorizontal();

            return isOpen;
        }

        private void DrawEvents(string eventID, shaco.Base.EventManager.CallBackInfo callbackInfo)
        {
            if (DrawEventHeader(eventID, callbackInfo.CallBack.Count, true))
            {
                int countTmp = callbackInfo.CallBack.Count;
                for (int i = 0; i < countTmp; ++i)
                {
                    if (!DrawEvent(eventID, callbackInfo.CallBack[i]))
                    {
                        break;
                    }
                }
            }
        }

        private bool DrawEvent(string eventID, EventCallBack<BaseEventArg>.CallBackInfo callBackInfo)
        {
            GUILayout.BeginHorizontal();
            {
                var maxWidthTmp = GUILayout.MaxWidth(_currentWindow.position.width / 3 * 2 - 283);

                //draw sender
                GUILayout.Label("Default Sender:", GUILayout.Width(85));

                if (callBackInfo.DefaultSender as Object != null)
                {
                    EditorGUILayout.ObjectField(((Object)callBackInfo.DefaultSender), typeof(Object), true, GUILayout.Width(140));
                }
                else
                {
                    GUILayout.TextArea(callBackInfo.DefaultSender.ToTypeString(), GUILayout.Width(140));
                }

                //draw callback
                GUILayout.Label("Function:", GUILayout.Width(53));
                GUILayout.TextArea(callBackInfo.CallFunc.Method.Name, maxWidthTmp);

                //draw invoke type
                GUILayout.Label(callBackInfo.InvokeOnce ? "Once" : "Loop", GUILayout.Width(32));

                _panelType = (PanelType)EditorGUILayout.EnumPopup(_panelType, GUILayout.Width(60));
                switch (_panelType)
                {
                    case PanelType.API:
                        {
                            DrawHeaderAPI(eventID, callBackInfo);
                            break;
                        }
                    case PanelType.Location:
                        {
                            DrawHeaderLocation(eventID, callBackInfo);
                            break;
                        }
                    default: shaco.Log.Info("unsupport panel type=" + _panelType); break;
                }
            }
            GUILayout.EndHorizontal();

            return !_isTestBusy;
        }

        private void DrawHeaderLocation(string eventID, EventCallBack<BaseEventArg>.CallBackInfo callBackInfo)
        {
            //draw location 'AddCallBack' button
            if (!string.IsNullOrEmpty(callBackInfo.CallAddEventStack.statck) && GUILayout.Button("[Add:" + callBackInfo.CallAddEventStack.GetPerformanceDescription() + "]"))
            {
                EditorHelper.OpenAsset(callBackInfo.CallAddEventStack.statck, callBackInfo.CallAddEventStack.statckLine);
            }

            //draw location 'InvokeCallBack' button
            if (!string.IsNullOrEmpty(callBackInfo.CallInvokeEventStack.statck) && GUILayout.Button("[Invoke:" + callBackInfo.CallInvokeEventStack.GetPerformanceDescription() + "]"))
            {
                EditorHelper.OpenAsset(callBackInfo.CallInvokeEventStack.statck, callBackInfo.CallInvokeEventStack.statckLine);
            }
        }

        private void DrawHeaderAPI(string eventID, EventCallBack<BaseEventArg>.CallBackInfo callBackInfo)
        {
            //draw delete button
            if (GUILayout.Button("Delete", GUILayout.Width(45)))
            {
                _isTestBusy = true;
                var defaultEventArg = (BaseEventArg)typeof(BaseEventArg).Assembly.CreateInstance(eventID);
                EventManager.RemoveEvent(callBackInfo.DefaultSender, callBackInfo.CallFunc, defaultEventArg.GetType());
            }
        }
    }
}

