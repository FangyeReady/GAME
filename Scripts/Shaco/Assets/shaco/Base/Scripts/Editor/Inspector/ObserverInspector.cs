using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ObserverInspector : EditorWindow
    {
        /// <summary>
        /// 窗口对象
        /// </summary>
        private ObserverInspector _currentWindow = null;

        /// <summary>
        /// 所有数据主体堆栈信息
        /// </summary>
        private Dictionary<object, Dictionary<shaco.Base.ISubjectBase, shaco.Base.SubjectManager.SubjectLocation>> _allStackLocation = null;

        /// <summary>
        /// 搜索的数据名字
        /// </summary>
        private string _searchName = string.Empty;

        /// <summary>
        /// 搜索的数据名字(小写)
        /// </summary>
        private string _searchNameLower = string.Empty;

        /// <summary>
        /// 滚动视图当前位置
        /// </summary>
        private Vector2 _scrollPosition = Vector2.zero;


        [MenuItem("shaco/Viewer/ObserverViewer %&3", false, (int)ToolsGlobalDefine.MenuPriority.Viewer.Observer)]
        static void Open()
        {
            EditorHelper.GetWindow<ObserverInspector>(null, true, "ObserverViewer");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<ObserverInspector>(this, true, "ObserverViewer");
            this.UpdateAllStackLocation();
        }

        void OnGUI()
        {
            if (null == _currentWindow)
            {
                return;
            }
            this.Repaint();

            DrawSearchField();

            GUILayout.Space(10);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                foreach (var iter in _allStackLocation)
                {
                    DrawSubjects(iter.Key, iter.Value);
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawSearchField()
        {
            GUILayout.BeginHorizontal();
            {
                float refreshButtonWidth = Screen.width * 0.1f;
                if (GUILayout.Button("Refresh", GUILayout.Width(refreshButtonWidth)))
                {
                    UpdateAllStackLocation();
                }

                GUILayout.Space(Screen.width / 3 * 2 - refreshButtonWidth);

                GUI.changed = false;
                _searchName = GUILayoutHelper.SearchField(_searchName, GUILayout.Width(Screen.width / 3 * 1 - 11));
                if (GUI.changed)
                {
                    _searchNameLower = _searchName.ToLower();
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制数据主体的堆栈信息
        /// <param name="bindTarget">数据主体绑定对象</param>
        /// <param name="stackLocations">数据主体绑定的所有观测者堆栈信息</param>
        /// </summary>
        private void DrawSubjects(object bindTarget, Dictionary<shaco.Base.ISubjectBase, shaco.Base.SubjectManager.SubjectLocation> subjectLocations)
        {
            var unityObj = bindTarget as UnityEngine.Object;
            var headerKey = unityObj.IsNull() ? bindTarget.ToString() : unityObj.name;

            if (!string.IsNullOrEmpty(_searchNameLower) && !headerKey.ToLower().Contains(_searchNameLower))
            {
                return;
            }

            if (GUILayoutHelper.DrawHeader(string.Empty, headerKey, false, () =>
            {
                EditorGUI.BeginDisabledGroup(true);
                {
                    if (!unityObj.IsNull())
                    {
                        EditorGUILayout.ObjectField(unityObj, unityObj.GetType(), unityObj);
                    }
                    else
                    {
                        GUILayout.TextArea(headerKey);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }))
            {
                foreach (var iter in subjectLocations)
                {
                    DrawObservers(iter.Key, iter.Value.observserLocations);
                }
            }
        }

        /// <summary>
        /// 绘观测者的堆栈信息
        /// <param name="subject">数据主体</param>
        /// <param name="observerLocations">数据主体绑定的所有观测者堆栈信息</param>
        /// </summary>
        private void DrawObservers(shaco.Base.ISubjectBase subject, Dictionary<shaco.Base.IObserverBase, shaco.Base.SubjectManager.ObserverLocation> observerLocations)
        {
            var headerKey = subject.ToString();
            GUILayout.BeginVertical("box");
            {
                if (GUILayoutHelper.DrawHeader(headerKey, headerKey, false, "HorizontalMinMaxScrollbarThumb"))
                {
                    foreach (var iter in observerLocations)
                    {
                        DrawObserver(iter.Value);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制观测者的堆栈信息
        /// <param name="observerLocations">观测者堆栈信息</param>
        /// </summary>
        private void DrawObserver(shaco.Base.SubjectManager.ObserverLocation observerLocation)
        {
            GUILayout.BeginHorizontal("box");
            {
                //draw location 'Init' button
                DrawStackLocationButton("Init", observerLocation.stackLocationObserverInit, observerLocation.callbackInitDelegate, Screen.width / 4 - 9);

                //draw location 'ValueChange' button
                DrawStackLocationButton("Change", observerLocation.stackLocationValueChange, observerLocation.callbackUpdateDelegate, Screen.width / 4 - 9);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制堆栈定位信息
        /// <param name="prefix">标题前缀</param>
        /// <param name="stackLocation">堆栈信息</param>
		/// <param name="del">调用委托</param>
		/// <param name="oneItemWidth">一个组件所占宽度</param>
        /// <return></return>
        /// </summary>
        private void DrawStackLocationButton(string prefix, shaco.Base.StackLocation stackLocation, System.Delegate del, float oneItemWidth)
        {
            if (!string.IsNullOrEmpty(stackLocation.statck))
            {
                GUILayout.Label("Target: " + (null == del || null == del.Target ? "null" : del.Target.ToString()), GUILayout.Width(oneItemWidth));
                
                var buttonStr = "[" + prefix + ":" + stackLocation.GetPerformanceDescription() + "]";
                if (GUILayout.Button(buttonStr, GUILayout.Width(oneItemWidth)))
                {
                    EditorHelper.OpenAsset(stackLocation.statck, stackLocation.statckLine);
                }
            }
        }

        /// <summary>
        /// 刷新并获取所有数据主体堆栈信息
        /// </summary>
        private void UpdateAllStackLocation()
        {
            _allStackLocation = shaco.Base.SubjectManager.GetSubjectStatckLocations();
        }
    }
}