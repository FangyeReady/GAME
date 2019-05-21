using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class LogLocationInspector : EditorWindow
    {
        public class LogEditorConfig
        {
            public string logScriptPath = string.Empty;
            public TextAsset logScriptAsset = null;
            public string logTypeName = string.Empty;
            public string logTypeFullName = string.Empty;
            public int instanceID = 0;

            public LogEditorConfig(string logScriptPath, string typeFullName)
            {
                var indexFind = typeFullName.LastIndexOf('.');
                if (indexFind >= 0)
                {
                    this.logTypeName = typeFullName.Substring(indexFind + 1);
                }
                else
                {
                    this.logTypeName = typeFullName;
                }

                this.logScriptPath = logScriptPath;
                this.logScriptAsset = AssetDatabase.LoadAssetAtPath(logScriptPath, typeof(TextAsset)) as TextAsset;
                this.logTypeFullName = typeFullName;
            }
        }

        private LogLocationInspector _currentWindow = null;
        static private Dictionary<string, LogEditorConfig> _logEditorConfig = new Dictionary<string, LogEditorConfig>();
        private List<string> _removeKeys = new List<string>();
        private Vector2 _scrollPos = Vector2.zero;

        [MenuItem("shaco/Tools/LogLocationInspetor", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.LOG_LOCATION)]
        static void OpenLogLocationInspector()
        {
            shacoEditor.EditorHelper.GetWindow<LogLocationInspector>(null, true, "LogLocationInspetor").Init();
        }

        void OnEnable()
        {
            _currentWindow = shacoEditor.EditorHelper.GetWindow<LogLocationInspector>(this, true, "LogLocationInspetor");
            _currentWindow.Init();
        }

        void OnGUI()
        {
            if (null == _currentWindow)
                return;

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            {
                foreach (var iter in _logEditorConfig)
                {
                    GUI.changed = false;
                    var assetTmp = EditorGUILayout.ObjectField(iter.Value.logScriptAsset, typeof(TextAsset), true) as TextAsset;
                    if (GUI.changed)
                    {
                        if (null != assetTmp)
                        {
                            var pathTmp = AssetDatabase.GetAssetPath(assetTmp);
                            if (SetLocationSetting(pathTmp))
                            {
                                _removeKeys.Add(iter.Key);
                            }
                        }
                        else
                        {
                            _removeKeys.Add(iter.Key);
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Drag c# script here to add");
                GUI.changed = false;
                var assetTmp = EditorGUILayout.ObjectField(null, typeof(TextAsset), true) as TextAsset;
                if (GUI.changed)
                {
                    if (null != assetTmp)
                    {
                        var pathTmp = AssetDatabase.GetAssetPath(assetTmp);
                        SetLocationSetting(pathTmp);
                        SaveSettings();
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset"))
            {
                _logEditorConfig.Clear();
                CheckDefaultConfig();
                SaveSettings();
                LoadSettings();
            }

            if (_removeKeys.Count > 0)
            {
                for (int i = 0; i < _removeKeys.Count; ++i)
                {
                    _logEditorConfig.Remove(_removeKeys[i]);
                }
                _removeKeys.Clear();

                SaveSettings();
            }

            this.Repaint();
        }

        static public IDictionary<string, LogEditorConfig> GetLocationConfig()
        {
            LoadSettings();
            return _logEditorConfig;
        }

        static private void SaveSettings()
        {
            var formatString = string.Empty;
            foreach (var iter in _logEditorConfig)
            {
                formatString += (iter.Value.logScriptPath + ",");
            }
            if (formatString.Length > 0 && formatString[formatString.Length - 1] == ',')
            {
                formatString = formatString.Remove(formatString.Length - 1);
            }
            shaco.DataSave.Instance.Write("LogLocationInspector_logEditorConfig", formatString);
        }

        private void Init()
        {
            _logEditorConfig.Clear();
            LoadSettings();
        }

        static private void LoadSettings()
        {
            if (_logEditorConfig.Count > 0)
                return;

            bool hasError = false;
            var formatString = shaco.DataSave.Instance.ReadString("LogLocationInspector_logEditorConfig");
            var configsSplit = formatString.Split(",");
            for (int i = 0; i < configsSplit.Length; ++i)
            {
                var pathTmp = configsSplit[i];
                if (!string.IsNullOrEmpty(pathTmp))
                {
                    if (!SetLocationSetting(pathTmp))
                    {
                        hasError = true;
                    }
                }
            }

            CheckDefaultConfig();
            if (hasError)
            {
                SaveSettings();
            }
        }

        static private void CheckDefaultConfig()
        {
            //默认有日志定位过滤
            if (_logEditorConfig.Count == 0)
            {
                _logEditorConfig.Clear();

                var folderTmp = shaco.Base.FileHelper.GetCurrentSourceFolderPath();
                folderTmp = shaco.Base.FileHelper.RemoveLastPathByLevel(folderTmp, 3);
                folderTmp = "Assets".ContactPath(folderTmp.Remove(Application.dataPath));

                //default class
                SetLocationSetting(folderTmp.ContactPath("/CSharp/Debug/Log.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/Debug/Log.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/Event/EventCallBack.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/Event/EventCallBackArg.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/Event/EventManager.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/File/FileHelper.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/UI/UIManager.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/UI/UIRootComponent.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/HotUpdate/ResourcesEx.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/HotUpdate/HotUpdateDataCache.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/HotUpdate/HotUpdateImportMemory.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/HotUpdate/HotUpdateImport.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/BehaviourTree/Coroutine/ForeachCoroutine.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/BehaviourTree/Coroutine/ForeachThreadCoroutine.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/BehaviourTree/Coroutine/SequeueCoroutine.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/BehaviourTree/Coroutine/WhileCoroutine.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/BehaviourTree/Coroutine/WhileThreadCoroutine.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/HotUpdate/ResourcesEx+Public.cs"));

                //extensions class
                SetLocationSetting(folderTmp.ContactPath("/Unity/Event/ExtensionsEventManagerUnity.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/Event/ExtensionsEventManagerCSharp.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/Tools/ExtensionsClassCSharp.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/Tools/ExtensionsClassUnity.cs"));
                SetLocationSetting(folderTmp.ContactPath("/CSharp/Tools/ExtensionsString.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/Tools/PrefabPool/ExtensionsPrefabPool.cs"));
                SetLocationSetting(folderTmp.ContactPath("/Unity/UI/ExtensionsUI.cs"));
            }
        }

        static private bool CanAddScriptPath(string pathScript)
        {
            if (shaco.Base.FileHelper.GetFilNameExtension(pathScript) != "cs")
            {
                Debug.LogError("LogLocationInspector CanAddScriptPath error: not a csharp script, path=" + pathScript);
                return false;
            }

            if (_logEditorConfig.ContainsKey(pathScript))
            {
                Debug.LogError("LogLocationInspector CanAddScriptPath error: duplicate script, path=" + pathScript);
                return false;
            }

            var loadAsset = AssetDatabase.LoadAssetAtPath(pathScript, typeof(TextAsset));
            if (null == loadAsset)
            {
                Debug.LogError("LogLocationInspector CanAddScriptPath error: missing script, path=" + pathScript);
                return false;
            }
            return true;
        }

        static private bool SetLocationSetting(string pathScript)
        {
            if (!CanAddScriptPath(pathScript))
            {
                return false;
            }

            var loadAsset = AssetDatabase.LoadAssetAtPath(pathScript, typeof(TextAsset));
            var strScrit = loadAsset.ToString();
            var className = shaco.Base.Utility.GetFullClassName(ref strScrit);
            if (string.IsNullOrEmpty(className))
            {
                Debug.LogError("LogLocationInspector SetLocationSetting error: not found class name, path=" + pathScript);
                return false;
            }

            _logEditorConfig[pathScript] = new LogEditorConfig(pathScript, className);
            return true;
        }
    }
}