using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    [ExecuteInEditMode]
    public class UIStateChangeViewer : EditorWindow
    {
        private Dictionary<string, Object> _uiStateBindScriptAssets = new Dictionary<string, Object>();
        private string _searchKey = string.Empty;
        private Vector3 _scrollViewPosition = Vector3.zero;
        private bool _isLockScroll = true;

        private UIStateChangeViewer _currentWindow = null;

        [MenuItem("shaco/Viewer/UIStateChangeViewer %&4", false, (int)ToolsGlobalDefine.MenuPriority.Viewer.UI_STATE_CHANGED_INSPECTOR)]
        static void OpenUIManagerWindow()
        {
            var retValue = EditorHelper.GetWindow<UIStateChangeViewer>(null, true, "UI State Change View");
            retValue.Init();
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow(this, true, "UI State Change View");
            _currentWindow.Init();
        }

        void OnGUI()
        {
            this.Repaint();

            DrawSearchField();

            GUILayout.BeginHorizontal();
            {
                DrawSettings();
            }
            GUILayout.EndHorizontal();

            _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition);
            {
                DrawAllUIChangedInfo();
            }
            GUILayout.EndScrollView();
        }

        private void DrawSearchField()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Count: " + shaco.UIStateChangeSave.GetAllUIStateChangedInfo().Count);
                GUILayout.Space(_currentWindow.position.width / 2);
                _searchKey = GUILayoutHelper.SearchField(_searchKey, GUILayout.Width(250));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSettings()
        {
            _isLockScroll = EditorGUILayout.Toggle("Lock Scroll", _isLockScroll);

            if (_isLockScroll)
            {
                _scrollViewPosition.y = float.MaxValue;
            }

            if (GUILayout.Button(shaco.UIStateChangeSave.isUIStateInsepctorOpened ? "PauseRecord" : "ResumeRecord"))
            {
                shaco.UIStateChangeSave.isUIStateInsepctorOpened = !shaco.UIStateChangeSave.isUIStateInsepctorOpened;
            }

            if (GUILayout.Button("Clear"))
            {
                shaco.UIStateChangeSave.ClearUIStateChangedInfo();
            }
        }

        private void DrawAllUIChangedInfo()
        {
            var allUIStateChangedInfoTmp = shaco.UIStateChangeSave.GetAllUIStateChangedInfo();
            bool shouldCheckSearchKey = !string.IsNullOrEmpty(_searchKey);
            var searchKeyLower = !shouldCheckSearchKey ? string.Empty : _searchKey.ToLower();

            for (int i = 0; i < allUIStateChangedInfoTmp.Count; ++i)
            {
                shaco.UIStateChangeSave.UIStateChangedInfo changedInfoTmp = allUIStateChangedInfoTmp[i];

                if (shouldCheckSearchKey && !changedInfoTmp.uiKey.ToLower().Contains(searchKeyLower))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                {
                    //script
                    Object assetScript = GetScriptAssetFromCache(changedInfoTmp.uiKey, changedInfoTmp.uiPrefab);
                    if (null == assetScript)
                    {
                        var prefabPath = shaco.UIManagerConfig.GetFullPrefabPath(changedInfoTmp.uiKey);
                        shaco.Log.Warning("UIStateChangeInsectorEditor DrawActiveUIComponent warning: not find script\n[key=" + changedInfoTmp.uiKey + "][prefab path=" + prefabPath[0] + "]");
                    }
                    EditorGUILayout.ObjectField("Script: ", assetScript, typeof(Object), true);

                    //prefab
                    EditorGUILayout.ObjectField("Prefab: ", changedInfoTmp.uiPrefab.prefab, typeof(Object), true);

                    //event
                    GUILayout.Label("Event: ");
                    if (GUILayout.Button("[" + changedInfoTmp.eventType + ":" + changedInfoTmp.statckLocationUI.GetPerformanceDescription() + "]", GUILayout.Width(200)))
                    {
                        EditorHelper.OpenAsset(changedInfoTmp.statckLocationUI.statck, changedInfoTmp.statckLocationUI.statckLine);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        void OnDestroy()
        {
            shaco.UIStateChangeSave.isUIStateInsepctorOpened = false;
            shaco.UIStateChangeSave.ClearUIStateChangedInfo();
            _currentWindow = null;
        }

        private void Init()
        {
            shaco.UIStateChangeSave.isUIStateInsepctorOpened = true;
        }

        private Object GetScriptAssetFromCache(string uiKey, shaco.UIPrefab uiPrefab)
        {
            if (!_uiStateBindScriptAssets.ContainsKey(uiKey))
                _uiStateBindScriptAssets.Add(uiKey, UIInspectorEditor.GetScriptAssetFromUI(uiKey, uiPrefab));
            return _uiStateBindScriptAssets[uiKey];
        }
    }
}

