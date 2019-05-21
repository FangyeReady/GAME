using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public class UIInspectorEditor : EditorWindow
    {
        private enum PanelType
        {
            Location
        }

        private readonly int MAX_SHOW_UI_COUNT = 50;
        private string _searchKey = string.Empty;
        private PanelType _panelType = PanelType.Location;
        private Dictionary<shaco.IUIState, Object> _uiStateBindScriptAssets = new Dictionary<shaco.IUIState, Object>();
        private Vector3 _scrollviewPosition = Vector3.zero;

        private UIInspectorEditor _currentWindow = null;

        [MenuItem("shaco/Viewer/UIViewer %&1", false, (int)ToolsGlobalDefine.MenuPriority.Viewer.UI_MANAGER)]
        static void OpenUIManagerWindow()
        {
            var retValue = EditorHelper.GetWindow<UIInspectorEditor>(null, true, "UI Inspector");
            retValue.Init();
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow(this, true, "UI Inspector");
            _currentWindow.Init();
        }

        void OnDestroy()
        {
        }

        void OnGUI()
        {
            this.Repaint();

            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUILayout.ObjectField("PrefabPath: ", GetPrefabPathAsset(), typeof(Object), true);
            }
            EditorGUI.EndDisabledGroup();

            DrawSearchField();

            GUILayoutHelper.DrawSeparatorLine();

            if (Application.isPlaying)
            {
                _scrollviewPosition = GUILayout.BeginScrollView(_scrollviewPosition);
                {
                    shaco.UIManager.ForeachActiveUIRoot((shaco.UIRootComponent uiRoot) =>
                    {
                        var keyTmp = "UI Root" + "(Layer Index:" + uiRoot.layerIndex + ")";
                        if (GUILayoutHelper.DrawHeader(keyTmp, keyTmp, () => { EditorGUILayout.ObjectField(uiRoot, typeof(Object), true); }))
                        {
                            DrawActiveUIComponent(uiRoot);
                        }
                    });
                }
                GUILayout.EndScrollView();
            }
        }

        static public Object GetScriptAssetFromUI(string key, shaco.UIPrefab uiPrefab)
        {
            Object retValue = null;

            var classTypeName = key.Remove(shaco.UIManagerConfig.GetPrefabPath());
            Component findScriptComponent = null;

            for (int i = uiPrefab.componets.Length - 1; i >= 0; --i)
            {
                var classTypeNameTmp = uiPrefab.componets[i].ToTypeString();
                if (classTypeNameTmp == classTypeName)
                {
                    findScriptComponent = uiPrefab.componets[i];
                    break;
                }
            }

            if (null != findScriptComponent)
            {
                var serializedObject = new UnityEditor.SerializedObject(findScriptComponent);
                var instanceID = serializedObject.FindProperty("m_Script").objectReferenceInstanceIDValue;
                retValue = AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GetAssetPath(instanceID), typeof(Object));
            }
            return retValue;
        }

        private void Init()
        {
        }

        private void DrawActiveUIComponent(shaco.UIRootComponent uiRoot)
        {
            //ui
            int index = 0;
            int loopCount = MAX_SHOW_UI_COUNT < uiRoot.Count ? MAX_SHOW_UI_COUNT : uiRoot.Count;

            uiRoot.Foreach((shaco.IUIState uiStateTmp) =>
            {
                if (!string.IsNullOrEmpty(_searchKey) && !uiStateTmp.key.ToLower().Contains(_searchKey.ToLower()))
                    return true;

                if (uiStateTmp.prefabs.Count == 0)
                {
                    shaco.Log.Error("not find prefab by ui=" + uiStateTmp.key);
                    return true;
                }

                //script
                Object assetScript = GetScriptAssetFromCache(uiStateTmp);
                if (null == assetScript)
                {
                    var prefabPath = shaco.UIManagerConfig.GetFullPrefabPath(uiStateTmp.key);
                    shaco.Log.Warning("UIInspectorEditor DrawActiveUIComponent warning: not find script\n[key=" + uiStateTmp.key + "][prefab path=" + prefabPath[0] + "]");
                }

                EditorGUILayout.ObjectField("Script: ", assetScript, typeof(Object), true);

                //prefabs
                GUILayout.Label("Prefabs: ");
                GUILayout.BeginHorizontal();
                {
                    for (int i = 0; i < uiStateTmp.prefabs.Count; ++i)
                    {
                        var prefabTmp = uiStateTmp.prefabs[i];
                        EditorGUILayout.ObjectField(prefabTmp.prefab, typeof(Object), true);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    DrawBottom(uiStateTmp);
                }
                GUILayout.EndHorizontal();

                return ++index < loopCount;
            });
        }

        private Object GetScriptAssetFromCache(shaco.IUIState uiState)
        {
            if (uiState.prefabs.Count == 0)
                return null;

            if (!_uiStateBindScriptAssets.ContainsKey(uiState))
                _uiStateBindScriptAssets.Add(uiState, GetScriptAssetFromUI(uiState.key, uiState.prefabs[0]));
            return _uiStateBindScriptAssets[uiState];
        }

        private void DrawSearchField()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Count: " + shaco.UIManager.GetUIRootCount());
                GUILayout.Space(_currentWindow.position.width / 2);
                _searchKey = GUILayoutHelper.SearchField(_searchKey, GUILayout.Width(250));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawBottom(shaco.IUIState uiState)
        {
            _panelType = (PanelType)EditorGUILayout.EnumPopup(_panelType, GUILayout.Width(60));

            switch (_panelType)
            {
                // case PanelType.API:
                //     {
                //         DrawPanelAPI(uiState);
                //         break;
                //     }
                case PanelType.Location:
                    {
                        DrawPanelLocation(uiState);
                        break;
                    }
                default: shaco.Log.Info("unsupport panel type=" + _panelType); break;
            }
        }

        private void DrawPanelLocation(shaco.IUIState uiState)
        {
            if (null == uiState) return;

            var statckLocationPreLoadUI = uiState.uiEvent.uiEventInfo[(int)shaco.UIEvent.EventType.OnPreLoad].statckLocationUI;
            var statckLocationInitUI = uiState.uiEvent.uiEventInfo[(int)shaco.UIEvent.EventType.OnInit].statckLocationUI;
            var statckLocationOpenUI = uiState.uiEvent.uiEventInfo[(int)shaco.UIEvent.EventType.OnOpen].statckLocationUI;
            var statckLocationResumeUI = uiState.uiEvent.uiEventInfo[(int)shaco.UIEvent.EventType.OnResume].statckLocationUI;
            var statckLocationHideUI = uiState.uiEvent.uiEventInfo[(int)shaco.UIEvent.EventType.OnHide].statckLocationUI;

            DrawLocationButton("OnPreLoad", statckLocationPreLoadUI);
            DrawLocationButton("OnInit", statckLocationInitUI);
            DrawLocationButton("OnOpen", statckLocationOpenUI);
            DrawLocationButton("OnHide", statckLocationHideUI);
            DrawLocationButton("OnResume", statckLocationResumeUI);
        }

        private void DrawLocationButton(string title, shaco.Base.StackLocation statckLocation, params GUILayoutOption[] options)
        {
            if (null == statckLocation || !statckLocation.HasStatck()) return;

            if (GUILayout.Button("[" + title + ":" + statckLocation.GetPerformanceDescription() + "]", options))
            {
                EditorHelper.OpenAsset(statckLocation.statck, statckLocation.statckLine);
            }
        }

        private Object GetPrefabPathAsset()
        {
            Object retValue = null;
            var prefabPath = shaco.UIManagerConfig.GetPrefabPath();
            prefabPath = prefabPath.RemoveRangeIfHave(prefabPath.Length - 1, prefabPath.Length - 1, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            retValue = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
            return retValue;
        }

        // private void DrawPanelAPI(shaco.UIRootComponent.UIState uiState)
        // {
        //     if (GUILayout.Button("OpenUI", GUILayout.Width(50)))
        //     {
        //         var arg = System.Activator.CreateInstance(typeof(Component), string.Empty);
        //         // shaco.UIManager.OpenUI(key);
        //     }

        //     if (null == uiState || !uiState.callStatcks[(int)shaco.UIRootComponent.CallStatckType.OpenUI].HasStatck()) return;

        //     if (GUILayout.Button("Hide", GUILayout.Width(40)))
        //     {
        //         // shaco.UIManager.HideUI(uiState.key);
        //     }

        //     if (GUILayout.Button("Close", GUILayout.Width(40)))
        //     {
        //         // shaco.UIManager.CloseUI(uiState.key);
        //     }
        // }
    }
}


