using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class BuildInspector : EditorWindow
    {
        private enum IOSExportOption
        {
            AdHoc = 0,
            AppStore = 1,
            Enterprise,
            Development
        }

        public enum IOSCodeIndentity
        {
            Development,
            Distribution
        }

        private enum ProjectUpdateType
        {
            GitHub = 0,
            Svn = 1
        }

        //入口点场景，以便测试的时候快速启动
        static private Object _entryScene = null;

        //配置路径
        static private Object _configPath = null;
        static private shaco.DataSave _dataSave = new shaco.DataSave();

        //本地version文件路径，主要用于在游戏内显示version
        private const string VERSION_PATH = "BuildVersion.txt";

        //安装包名字 
        private string _applicationIdentifier = "com.shaco.test";
        //版本号
        private string _versionCode = "1.0.0";
        //Build号
        private string _buildCode = "1";
        //游戏服务器版本
        static private CustomEnumsField _buildServerMode = new CustomEnumsField("DEV", "STG", "PRD");
        //打包宏
        static private string _globalDefines = string.Empty;

        //Android Keyalias
        private string _androidKeyaliasPass = string.Empty;
        //Android Keystore
        private string _androidKeystorePass = string.Empty;

        //iOS打包身份描述
        static private IOSCodeIndentity _iOSCodeIndentity = IOSCodeIndentity.Development;
        //iOS开发者账号team
        static private string _iOSDevelopmentTeam = string.Empty;
        //iOS证书描述名字
        static private string _iOSProvisioningProfileSpecifier = string.Empty;
        //iOS打包导出ipa的配置
        static private IOSExportOption _iOSExportOption = IOSExportOption.AppStore;

        //windows下需要一个可以运行shell的执行工具
        static private string _windowsRunShellExePath = string.Empty;
        private string _windowsRunShellExeDisplayPath = string.Empty;

        //项目更新类型
        static private ProjectUpdateType _projectUpdateType = ProjectUpdateType.GitHub;
        //渠道类型
        static private CustomEnumsField _channel = new CustomEnumsField();

        private bool _isDebugSettings = true;
        private int _toobalIndex = 0;
        private UnityEditor.AnimatedValues.AnimBool _isShowVerionSettings = new UnityEditor.AnimatedValues.AnimBool(true);

        static public string GetIOSDevelepmentTeam() { return _iOSDevelopmentTeam; }
        static public string GetIOSProvisioningProfileSpecifier() { return _iOSProvisioningProfileSpecifier; }
        static public IOSCodeIndentity GetIOSCodeIndentity() { return _iOSCodeIndentity; }

        private void DrawVersions()
        {
            DrawDebugReleaseTab();

            _isShowVerionSettings.target = EditorHelper.Foldout(_isShowVerionSettings.target, "Version Settings");

            if (EditorGUILayout.BeginFadeGroup(_isShowVerionSettings.faded))
            {
                GUILayout.BeginVertical("box");
                {
                    DrawConfigPath();
#if UNITY_EDITOR_WIN
                    DrawWindowsRunShellExePath();   
#endif

#if UNITY_5_3_OR_NEWER
                    _entryScene = EditorGUILayout.ObjectField("Entry Scene", _entryScene, typeof(SceneAsset), true) as SceneAsset;
#else
                    _entryScene = EditorGUILayout.ObjectField("Entry Scene", _entryScene, typeof(Object), true);
#endif
                    DrawBundleIdentifier();
                    DrawVersion();
                    DrawBuild();
                    DrawServerMode();
                    _channel.DrawEnums("Channel");
                    DrawGlobalDefines();

#if UNITY_ANDROID
                    //只有android平台才使用的参数
                    DrawAndroidKeystore();
#endif

#if UNITY_IPHONE
                    //只有ios平台才使用的参数
                    _iOSDevelopmentTeam = EditorGUILayout.TextField("iOS Development Team", _iOSDevelopmentTeam);
                    if (!string.IsNullOrEmpty(_iOSDevelopmentTeam)) _iOSProvisioningProfileSpecifier = EditorGUILayout.TextField("iOS Provisioning Profile", _iOSProvisioningProfileSpecifier);
                    DrawIOSCodeIndentity();
                    DrawIOSExportOption();
#endif

                    _projectUpdateType = (ProjectUpdateType)EditorGUILayout.EnumPopup("Update Type", _projectUpdateType);
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawDebugReleaseTab()
        {
            GUI.changed = false;
            _toobalIndex = GUILayout.Toolbar(_toobalIndex, new string[] { "Debug", "Release" });
            if (GUI.changed)
            {
                //默认取消焦点
                GUI.FocusControl(string.Empty);

                SaveSettings();
                _isDebugSettings = _toobalIndex == 0;
                _dataSave.Write(GetSaveKey("_isDebugSettings"), _isDebugSettings);

                if (_isDebugSettings && _buildServerMode.Contains("DEV"))
                    _buildServerMode.SetEnum("DEV");
                else if (_buildServerMode.Contains("PRD"))
                    _buildServerMode.SetEnum("PRD");
                _dataSave.Write(GetSaveKey("_buildServerMode"), (int)_buildServerMode.currentSelectIndex);
                LoadSettings();
            }
        }

        private void DrawConfigPath()
        {
            GUI.changed = false;
            var configPathTmp = EditorGUILayout.ObjectField("Config", _configPath, typeof(Object), true);
            if (GUI.changed)
            {
                var pathTmp = Application.dataPath.ContactPath(AssetDatabase.GetAssetPath(configPathTmp).Remove("Assets"));
                if (null != configPathTmp && shaco.Base.FileHelper.GetFilNameExtension(pathTmp) != shaco.Base.GlobalParams.DATA_SAVE_EXTENSIONS)
                {
                    Debug.LogError("BuildInspector+Version DrawConfigPath error: must select the file with the extensions name=" + shaco.Base.GlobalParams.DATA_SAVE_EXTENSIONS);
                }
                else
                {
                    SaveSettings();
                    if (null != configPathTmp)
                    {
                        _dataSave.ReloadFromFile(pathTmp, false);
                    }
                    else
                    {
                        shaco.DataSave.Instance.Remove("BuildInspector_configPath");
                        _configPath = null;
                    }
                    shaco.DataSave.Instance.Write("BuildInspector_configPath", AssetDatabase.GetAssetPath(configPathTmp));
                    LoadSettings();
                }
            }
        }

        private void DrawWindowsRunShellExePath()
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.TextField("Run Shell application Path", _windowsRunShellExeDisplayPath);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Browse"))
                {
                    var selectPath = EditorUtility.OpenFilePanel("Select a program", Application.dataPath, "exe");
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _windowsRunShellExePath = selectPath;
                        _windowsRunShellExeDisplayPath = shaco.Base.FileHelper.GetLastFileName(_windowsRunShellExePath);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawBundleIdentifier()
        {
            GUI.changed = false;
            _applicationIdentifier = EditorGUILayout.TextField("Bundle Identifier", _applicationIdentifier);
            if (GUI.changed)
            {
#if UNITY_5_3_OR_NEWER
                PlayerSettings.SetApplicationIdentifier(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), _applicationIdentifier);
#else
                PlayerSettings.bundleIdentifier = _applicationIdentifier;
#endif
                SaveSettings();
            }
        }

        private void DrawVersion()
        {
            GUI.changed = false;
            _versionCode = EditorGUILayout.TextField("Version", _versionCode);
            if (GUI.changed)
            {
                UpdateVersionCode();
            }
        }

        private void DrawBuild()
        {
            GUI.changed = false;
            var oldBuildCode = _buildCode;
            _buildCode = EditorGUILayout.TextField("Build", _buildCode);
            if (GUI.changed)
            {
                int resultTmp = 0;
                if (string.IsNullOrEmpty(_buildCode))
                {
                    //ignore case
                }
                else if (!int.TryParse(_buildCode, out resultTmp))
                {
                    _buildCode = oldBuildCode;
                    Debug.LogWarning("BuildInspector+Version DrawVersion warning: build code must be integer");
                }
                else
                {
                    UpdateBuildCode();
                }
            }
        }

        private void DrawServerMode()
        {
            GUI.changed = false;
            _buildServerMode.DrawEnums("Server Mode");
        }

        private void DrawAndroidKeystore()
        {
            GUI.changed = false;
            _androidKeystorePass = EditorGUILayout.TextField("Android keystorePass", _androidKeystorePass);
            if (GUI.changed)
            {
                PlayerSettings.keystorePass = _androidKeystorePass;
            }
            
            GUI.changed = false;
            _androidKeyaliasPass = EditorGUILayout.TextField("Android keyaliasPass", _androidKeyaliasPass);
            if (GUI.changed)
            {
                PlayerSettings.keyaliasPass = _androidKeyaliasPass;
            }
        }

        private void DrawIOSCodeIndentity()
        {
            GUI.changed = false;
            var newCodeIndentity = (IOSCodeIndentity)EditorGUILayout.EnumPopup("iOS Code Indentity", _iOSCodeIndentity);
            if (GUI.changed)
            {
                _iOSCodeIndentity = newCodeIndentity;

                //当代码描述配置为开发模式的时候，导出配置也需要为开发模式才能导出ipa
                if (_iOSCodeIndentity == IOSCodeIndentity.Development)
                {
                    _iOSExportOption = IOSExportOption.Development;
                }
            }
        }

        private void DrawIOSExportOption()
        {
            if (!string.IsNullOrEmpty(_iOSDevelopmentTeam) && !string.IsNullOrEmpty(_iOSProvisioningProfileSpecifier) && _iOSCodeIndentity != IOSCodeIndentity.Development)
            {
                var newOption = (IOSExportOption)EditorGUILayout.EnumPopup("iOS Export Option", _iOSExportOption);
                if (GUI.changed)
                {
                    if (_iOSCodeIndentity == IOSCodeIndentity.Development && newOption != IOSExportOption.Development)
                    {
                        newOption = IOSExportOption.Development;
                    }
                    _iOSExportOption = newOption;
                }
            }
        }

        private void DrawGlobalDefines()
        {
            GUILayout.BeginHorizontal();
            {
                _globalDefines = EditorGUILayout.TextField("Global Defines", _globalDefines);
                if (GUILayout.Button("UpdateProjectDefines", GUILayout.Width(140)))
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), _globalDefines);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void SaveSettings()
        {
            shaco.DataSave.Instance.Write("BuildInspector_configPath", AssetDatabase.GetAssetPath(_configPath));
            shaco.DataSave.Instance.Write("BuildInspector_entryScene", AssetDatabase.GetAssetPath(_entryScene));

            _dataSave.Write(GetSaveKey("_isShowVerionSettings"), _isShowVerionSettings.value);
            _dataSave.Write(GetSaveKey("_isDebugSettings"), _isDebugSettings);
            _dataSave.Write(GetSaveKey("_buildServerMode"), (int)_buildServerMode.currentSelectIndex);
            _dataSave.Write(GetSaveKey("_buildServerModeDisplay"), _buildServerMode.GetEnumsDisplay().ToSplit(","));
            _dataSave.Write(GetSaveKey("_windowsRunShellExePath"), _windowsRunShellExePath);
            _dataSave.WriteEnum(GetSaveKey("_projectUpdateType"), _projectUpdateType);

            _dataSave.Write(GetFullSaveKey("_currentSelectChannel"), _channel.currentSelectIndex);
            _dataSave.Write(GetFullSaveKey("_channelsDisplay"), _channel.GetEnumsDisplay().ToSplit(","));
            _dataSave.WriteEnum(GetFullSaveKey("_androidKeyaliasPass"), _androidKeyaliasPass);
            _dataSave.WriteEnum(GetFullSaveKey("_androidKeystorePass"), _androidKeystorePass);
            _dataSave.WriteEnum(GetFullSaveKey("_iOSCodeIndentity"), _iOSCodeIndentity); 
            _dataSave.Write(GetFullSaveKey("_iOSDevelopmentTeam"), _iOSDevelopmentTeam);
            _dataSave.Write(GetFullSaveKey("_iOSProvisioningProfileSpecifier"), _iOSProvisioningProfileSpecifier);
            _dataSave.Write(GetFullSaveKeyWithChannel("_globalDefines"), _globalDefines);
            _dataSave.Write(GetFullSaveKey("_applicationIdentifier"), _applicationIdentifier);
            _dataSave.Write(GetFullSaveKey("_iOSExportOption"), (int)_iOSExportOption);
        }

        private void InitSettings()
        {
            LoadSettings();

            _channel.onEnumWillChangeCallBack = () => { SaveSettings(); };
            _channel.onEnumChangedCallBack = () =>
            {
                _dataSave.Write(GetFullSaveKey("_currentSelectChannel"), _channel.currentSelectIndex);
                _dataSave.Write(GetFullSaveKey("_channelsDisplay"), _channel.GetEnumsDisplay().ToSplit(","));
                LoadSettings();
            };

            _buildServerMode.onEnumWillChangeCallBack = () => { SaveSettings(); };
            _buildServerMode.onEnumChangedCallBack = () =>
            {
                _dataSave.Write(GetSaveKey("_buildServerMode"), (int)_buildServerMode.currentSelectIndex);
                _dataSave.Write(GetSaveKey("_buildServerModeDisplay"), _buildServerMode.GetEnumsDisplay().ToSplit(","));
                LoadSettings();
            };
        }

        private void LoadSettings()
        {
            var defaultBundleIdentifier = string.Empty;
#if UNITY_5_3_OR_NEWER
            defaultBundleIdentifier = PlayerSettings.applicationIdentifier;
#else
            defaultBundleIdentifier = PlayerSettings.bundleIdentifier;
#endif
            _versionCode = PlayerSettings.bundleVersion;
            _buildCode = GetBuildCode();

            _configPath = AssetDatabase.LoadAssetAtPath(shaco.DataSave.Instance.ReadString("BuildInspector_configPath"), typeof(Object));
            if (null != _configPath)
            {
                _dataSave.ReloadFromFile(AssetDatabase.GetAssetPath(_configPath), false);
            }

            _entryScene = AssetDatabase.LoadAssetAtPath(shaco.DataSave.Instance.ReadString("BuildInspector_entryScene"), typeof(Object));
            _isShowVerionSettings.value = _dataSave.ReadBool(GetSaveKey("_isShowVerionSettings"), true);
            _isDebugSettings = _dataSave.ReadBool(GetSaveKey("_isDebugSettings"), true);
            _buildServerMode.currentSelectIndex = _dataSave.ReadInt(GetSaveKey("_buildServerMode"));
            _buildServerMode.SetEnumDisplay(_dataSave.ReadString(GetSaveKey("_buildServerModeDisplay"), "DEV,STD,PRD").Split(","));
            _windowsRunShellExePath = _dataSave.ReadString(GetSaveKey("_windowsRunShellExePath"));
            _projectUpdateType = _dataSave.ReadEnum<ProjectUpdateType>(GetSaveKey("_projectUpdateType"));
            _channel.currentSelectIndex = _dataSave.ReadInt(GetFullSaveKey("_currentSelectChannel"));
            _channel.SetEnumDisplay(_dataSave.ReadString(GetFullSaveKey("_channelsDisplay"), "Default").Split(","));
            _androidKeyaliasPass = _dataSave.ReadString(GetFullSaveKey("_androidKeyaliasPass"), PlayerSettings.keyaliasPass);
            _androidKeystorePass = _dataSave.ReadString(GetFullSaveKey("_androidKeystorePass"), PlayerSettings.keystorePass);
            _iOSCodeIndentity = _dataSave.ReadEnum<IOSCodeIndentity>(GetFullSaveKey("_iOSCodeIndentity"));
            _iOSDevelopmentTeam = _dataSave.ReadString(GetFullSaveKey("_iOSDevelopmentTeam"));
            _iOSProvisioningProfileSpecifier = _dataSave.ReadString(GetFullSaveKey("_iOSProvisioningProfileSpecifier"));
            _globalDefines = _dataSave.ReadString(GetFullSaveKeyWithChannel("_globalDefines"));
            _applicationIdentifier = _dataSave.ReadString(GetFullSaveKey("_applicationIdentifier"), defaultBundleIdentifier);
            _iOSExportOption = _dataSave.ReadEnum<IOSExportOption>(GetFullSaveKey("_iOSExportOption"));

            _toobalIndex = _isDebugSettings ? 0 : 1;
#if UNITY_5_3_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), _applicationIdentifier);
#else
            PlayerSettings.bundleIdentifier = _applicationIdentifier;
#endif
            _windowsRunShellExeDisplayPath = shaco.Base.FileHelper.GetLastFileName(_windowsRunShellExePath);
            _channel.UpdateEnumsDisplay();
        }

        private void UpdateVersionCode()
        {
            PlayerSettings.bundleVersion = _versionCode;
        }

        private void UpdateBuildCode()
        {
#if UNITY_5_3_OR_NEWER
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android: PlayerSettings.Android.bundleVersionCode = _buildCode.ToInt(); break;
                case BUILD_TARGET_IOS: PlayerSettings.iOS.buildNumber = _buildCode; break;
                default: Debug.LogError("BuildInspector+Version UpdateBuildCode errro: unsupport type=" + EditorUserBuildSettings.activeBuildTarget); break;
            }
#else
            PlayerSettings.shortBundleVersion = _buildCode;
            PlayerSettings.Android.bundleVersionCode = _buildCode.ToInt();
#endif
        }

        private string GetBuildCode()
        {
#if UNITY_5_3_OR_NEWER
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android: return PlayerSettings.Android.bundleVersionCode.ToString();
                case BUILD_TARGET_IOS: return PlayerSettings.iOS.buildNumber;
                default: Debug.LogError("BuildInspector+Version GetBuildCode errro: unsupport type=" + EditorUserBuildSettings.activeBuildTarget); return string.Empty;
            }
#else
            return PlayerSettings.shortBundleVersion;
#endif
        }

        private string GetSaveKey(string key)
        {
            return this.ToTypeString() + key;
        }

        private string GetFullSaveKey(string key)
        {
            return this.ToTypeString() + key + (_isDebugSettings ? "_Debug" : "_Release") + "_" + _buildServerMode.GetCurrentEnumString();
        }

        private string GetFullSaveKeyWithChannel(string key)
        {
            return this.ToTypeString() + key + (_isDebugSettings ? "_Debug" : "_Release") + "_" + _channel.GetCurrentEnumString() + "_" + _buildServerMode.GetCurrentEnumString();
        }
    }
}