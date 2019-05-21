using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace shacoEditor
{
    public partial class BuildInspector : EditorWindow
    {
        static private BuildInspector _currentWindow = null;

#if UNITY_5_3_OR_NEWER
        private const BuildTarget BUILD_TARGET_IOS = BuildTarget.iOS;
#else
        private const BuildTarget BUILD_TARGET_IOS = BuildTarget.iPhone;
#endif

        private UnityEditor.AnimatedValues.AnimBool _isShowBuild = new UnityEditor.AnimatedValues.AnimBool(true);

        [MenuItem("shaco/Tools/BuildInspector %&b", false, (int)ToolsGlobalDefine.MenuPriority.Tools.BUILD_INSPECTOR)]
        static void OpenBuildInspector()
        {
            _currentWindow = shacoEditor.EditorHelper.GetWindow<BuildInspector>(null, true, "BuildInspector");
            _currentWindow.InitSettings();
        }

        [MenuItem("shaco/Tools/RunGame _F5", false, (int)ToolsGlobalDefine.MenuPriority.Tools.RUN_GAME)]
        static void RunGame()
        {
            if (null == _entryScene)
            {
                _entryScene = AssetDatabase.LoadAssetAtPath(shaco.DataSave.Instance.ReadString("BuildInspector_entryScene"), typeof(UnityEngine.Object));
            }
            if (!EditorApplication.isPlaying && null != _entryScene)
            {
                var scenePathTmp = AssetDatabase.GetAssetPath(_entryScene);

                EditorHelper.OpenScene(scenePathTmp);
                EditorApplication.isPlaying = true;
            }
        }

        void OnEnable()
        {
            _currentWindow = shacoEditor.EditorHelper.GetWindow<BuildInspector>(this, true, "BuildInspector");
            _currentWindow.InitSettings();
        }

        void OnDestroy()
        {
            _currentWindow.SaveSettings();
        }

        public void OnGUI()
        {
            if (_currentWindow == null)
                return;

            base.Repaint();

            DrawVersions();

            _isShowBuild.target = EditorHelper.Foldout(_isShowBuild.target, "Build Package");

            if (EditorGUILayout.BeginFadeGroup(_isShowBuild.faded))
            {
                GUILayout.BeginHorizontal("box");
                {
                    var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                    if (GUILayout.Button("Build Android", GUILayout.Height(50)))
                    {
                        UpdateAndroiKeystore();
                        StartBuildProcessReady(BuildTarget.Android, _buildServerMode.GetCurrentEnumString(), _channel.GetCurrentEnumString(), true, false);
                    }
                    if (currentBuildTarget == BuildTarget.Android) EditorHelper.DrawForegourndFrame();
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                    if (GUILayout.Button("Build iOS(ipa)", GUILayout.Height(50)))
                    {
                        StartBuildProcessReady(BUILD_TARGET_IOS, _buildServerMode.GetCurrentEnumString(), _channel.GetCurrentEnumString(), true, false);
                    }
                    if (currentBuildTarget == BUILD_TARGET_IOS) EditorHelper.DrawForegourndFrame();
#endif
                    if (GUILayout.Button("Build iOS(Xcode)", GUILayout.Height(50)))
                    {
                        StartBuildProcessReady(BUILD_TARGET_IOS, _buildServerMode.GetCurrentEnumString(), _channel.GetCurrentEnumString(), false);
                    }
                    if (currentBuildTarget == BUILD_TARGET_IOS) EditorHelper.DrawForegourndFrame();
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndFadeGroup();

            if (GUILayout.Button("Load Settings"))
            {
                var pathSelect = EditorUtility.OpenFilePanel("Select a Data Config", Application.dataPath, shaco.Base.GlobalParams.DATA_SAVE_EXTENSIONS);
                if (!string.IsNullOrEmpty(pathSelect))
                {
                    _dataSave.ReloadFromFile(pathSelect, false);
                    _configPath = AssetDatabase.LoadAssetAtPath(pathSelect.Remove(Application.dataPath.Remove("Assets")), typeof(UnityEngine.Object));
                    SaveSettings();
                    LoadSettings();
                }
            }

            if (GUILayout.Button("Reset Settings"))
            {
                bool checkContinue = EditorUtility.DisplayDialog("Warning", "Should Reset All Settings ?", "Continue", "Cancel");
                if (checkContinue)
                {
                    _dataSave.RemoveStartWith(this.ToTypeString());
                    InitSettings();
                }
            }
        }

        //xlua支持文件创建完毕回调
        static public void BuildXluaSupportEndCallBack()
        {
            EditorUtility.ClearProgressBar();

            var buildTarget = shaco.DataSave.Instance.ReadEnum<BuildTarget>("shaco.BuildInspectorParams.buildTarget");
            var serverMode = shaco.DataSave.Instance.ReadString("shaco.BuildInspectorParams.serverMode");
            var buildChannel = shaco.DataSave.Instance.ReadString("shaco.BuildInspectorParams.buildChannel");
            var shoudlBuildPackage = shaco.DataSave.Instance.ReadBool("shaco.BuildInspectorParams.shoudlBuildPackage");
            var isBatchMode = shaco.DataSave.Instance.ReadBool("shaco.BuildInspectorParams.isBatchMode");
            var options = shaco.DataSave.Instance.ReadEnum<BuildOptions>("shaco.BuildInspectorParams.options");

            StartBuildProcessBase(buildTarget, serverMode, buildChannel, shoudlBuildPackage, isBatchMode, options);
        }

        //shell脚本调用打包函数
        static private void StartBuildProcessWithShell()
        {
            AssetDatabase.Refresh();

            var strBuildTarget = shaco.UnityHelper.GetEnviromentCommandValue("BUILD_TARGET");
            var strBuildChannel = shaco.UnityHelper.GetEnviromentCommandValue("BUILD_CHANNEL");

#if UNITY_5_3_OR_NEWER
            var buildTarget = BuildTarget.NoTarget;
#else
            var buildTarget = BuildTarget.Android;
#endif
            switch (strBuildTarget)
            {
                case "Android": buildTarget = BuildTarget.Android; break;
                case "iOS": buildTarget = BUILD_TARGET_IOS; break;
                case "Automatic": buildTarget = EditorUserBuildSettings.activeBuildTarget; break;
                default: Debug.LogError("BuildInspector StartBuildProcess error: unsupport target type=" + strBuildTarget); return;
            }
            var strBuildServer = shaco.UnityHelper.GetEnviromentCommandValue("BUILD_SERVER");

            Debug.Log("shell buildTarget=" + buildTarget + " buildServer=" + strBuildServer);
            StartBuildProcessReady(buildTarget, strBuildServer, strBuildChannel, true, true);
        }

        //确认是否需要切换平台，返回false表示不需要切换平台，并停止打包
        private static bool CheckChangePlatform(BuildTarget buildTarget, bool isBatchMode)
        {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
            {
                if (!isBatchMode)
                {
                    Debug.LogError("BuildInspector CheckChangePlatform errro: Current platform(" + EditorUserBuildSettings.activeBuildTarget + "), should change to target platform(" + buildTarget + ")");
                    return false;
                }
                else
                {
                    bool changed = EditorUtility.DisplayDialog("Should Change Build Target", "Current platform(" + EditorUserBuildSettings.activeBuildTarget + "), target platform(" + buildTarget + ")", "OK", "Cancel");

                    if (changed)
                    {
#if UNITY_5_3_OR_NEWER
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetToBuildTargetGroup(buildTarget), buildTarget);
#else
                        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
#endif
                    }
                    return changed;
                }
            }
            else
            {
                return true;
            }
        }

        private static BuildTargetGroup BuildTargetToBuildTargetGroup(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android: return BuildTargetGroup.Android;
#if UNITY_5_3_OR_NEWER
                case BUILD_TARGET_IOS: return BuildTargetGroup.iOS;
#else
                case BUILD_TARGET_IOS: return BuildTargetGroup.iPhone;
#endif
                default: Debug.LogError("BuildInspector BuildTargetToBuildTargetGroup error: unsupport type=" + buildTarget); return BuildTargetGroup.Android;
            }
        }

        private static void StartBuildProcessReady(BuildTarget buildTarget, string serverMode, string buildChannel, bool shoudlBuildPackage, bool isBatchMode = false, BuildOptions options = BuildOptions.None)
        {
            CheckProjectUpdate(isBatchMode, (bool success) =>
            {
                if (!success)
                {
                    if (!isBatchMode)
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
                else
                {
                    //保存参数，等项目更新和编译完毕后再打包
                    shaco.DataSave.Instance.WriteEnum("shaco.BuildInspectorParams.buildTarget", buildTarget);
                    shaco.DataSave.Instance.Write("shaco.BuildInspectorParams.serverMode", serverMode);
                    shaco.DataSave.Instance.Write("shaco.BuildInspectorParams.buildChannel", buildChannel);
                    shaco.DataSave.Instance.Write("shaco.BuildInspectorParams.shoudlBuildPackage", shoudlBuildPackage);
                    shaco.DataSave.Instance.Write("shaco.BuildInspectorParams.isBatchMode", isBatchMode);
                    shaco.DataSave.Instance.WriteEnum("shaco.BuildInspectorParams.options", options);

#if HOTFIX_ENABLE
                    BuildXluaSupport(isBatchMode);
#else
                    StartBuildProcessBase(buildTarget, serverMode, buildChannel, shoudlBuildPackage, isBatchMode, options);
#endif
                }
            });
        }

        //刷新并设置android的keystore
        private void UpdateAndroiKeystore()
        {
            PlayerSettings.keyaliasPass = _androidKeyaliasPass;
            PlayerSettings.keyaliasPass = _androidKeyaliasPass;
        }

        //执行打包流程
        private static void StartBuildProcessBase(BuildTarget buildTarget, string serverMode, string buildChannel, bool shoudlBuildPackage, bool isBatchMode = false, BuildOptions options = BuildOptions.None)
        {
            Debug.Log("StartBuildProcess begin...");

            try
            {
                //使用shell脚本打包unity工程的时候，需要加载一次配置
                if (_currentWindow == null && isBatchMode)
                {
                    _currentWindow = shacoEditor.EditorHelper.GetWindow<BuildInspector>(null, true, "BuildInspector");
                    _currentWindow.InitSettings();
                    Debug.Log("BuildInspector StartBuildProcess: loaded settings");
                }

                //检查channel是否合法
                if (!_channel.Contains(buildChannel))
                {
                    Debug.LogError("BuildInspector StartBuildProcess error: unsupport channel=" + buildChannel);
                    goto PROGRESS_END;
                }

                if (buildTarget == BUILD_TARGET_IOS)
                {
                    if (!string.IsNullOrEmpty(_iOSDevelopmentTeam))
                    {
                        if (shoudlBuildPackage && string.IsNullOrEmpty(_iOSProvisioningProfileSpecifier))
                        {
                            Debug.LogError("BuildInspector StartBuildProcess error: missing provisioning profile specifier, please check settings");
                            goto PROGRESS_END;
                        }
                    }
                    else
                    {
                        Debug.LogError("BuildInspector StartBuildProcess error: missing development team, please check settings");
                        goto PROGRESS_END;
                    }
                    UpdateiOSCertificate();
                }

#if UNITY_EDITOR_WIN
                if (string.IsNullOrEmpty(_windowsRunShellExePath))
                {
                    Debug.LogError("BuildInspector StartBuildProcess error: Please set up an executable program that can run the shell script");
                    goto PROGRESS_END;
                }
                if (!shaco.Base.FileHelper.ExistsFile(_windowsRunShellExePath))
                {
                    Debug.LogError("BuildInspector StartBuildProcess error: Program is not found, Path:\n" + _windowsRunShellExePath);
                    goto PROGRESS_END;
                }
#endif

                if (!CheckChangePlatform(buildTarget, isBatchMode))
                {
                    goto PROGRESS_END;
                }

                var scenesTmp = EditorHelper.GetEnabledEditorScenes();
                if (scenesTmp == null || scenesTmp.Length == 0)
                {
                    Debug.LogError("BuildInspector StartBuildProcess error: no scene need to build");
                    goto PROGRESS_END;
                }

                //设置全局宏
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetToBuildTargetGroup(buildTarget), _globalDefines);

                var exportPackageFolder = Application.dataPath.Remove("/Assets") + "/BuildPackages/";
                var exportPackagePath = exportPackageFolder + GetbuildTargetFlag(buildTarget, serverMode);

                //确认打包文件夹存在
                if (!System.IO.Directory.Exists(exportPackageFolder))
                {
                    System.IO.Directory.CreateDirectory(exportPackageFolder);
                }

                OverriteSettings(buildTarget);

                //开始打包
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                Debug.Log("BuildInspector BuildPipeline.BuildPlayer: buildTarget=" + buildTarget + " options=" + options);
                shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(exportPackagePath);
                BuildPipeline.BuildPlayer(scenesTmp, exportPackagePath, buildTarget, options);

                //打包并导出ipa
                if (buildTarget == BUILD_TARGET_IOS && shoudlBuildPackage)
                {
                    BuiliPA(exportPackagePath, isBatchMode);
                }

                //打开导出包目录
                var openBuildPath = shaco.Base.FileHelper.ExistsFile(exportPackagePath) ? exportPackagePath : shaco.Base.FileHelper.RemoveLastPathByLevel(exportPackagePath, 1);

                //如果是ios项目，该目录为打包文件夹路径，可以直接打开，否则可能为安装包目录需要取上级文件夹路径
                if (buildTarget == BuildTarget.Android)
                {
                    Debug.Log(shaco.Base.FileHelper.GetFolderNameByPath(openBuildPath));
                    System.Diagnostics.Process.Start(shaco.Base.FileHelper.GetFolderNameByPath(openBuildPath));
                }
                else
                {
                    System.Diagnostics.Process.Start(openBuildPath);
                }

#if HOTFIX_ENABLE
                //删除本地xlua临时生成文件
                CSObjectWrapEditor.Generator.ClearAll();
#endif                
            }
            catch (System.Exception e)
            {
                Debug.LogError("BuildInspector StartBuildProcess error: e=" + e);
                goto PROGRESS_END;
            }

        PROGRESS_END:

            if (!isBatchMode)
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.Log("StartBuildProcess end...");
        }

        //创建xlua支持文件
        static private void BuildXluaSupport(bool isBatchMode)
        {
#if HOTFIX_ENABLE
            shaco.DataSave.Instance.Write("BuildInspector.BuildXluaSupportEndCallBack", true);

            //开始构建文件
            CSObjectWrapEditor.Generator.GenAll();

            //等待构建完毕，允许中途退出
            shaco.WaitFor.Run(() =>
            {
                if (!isBatchMode)
                {
                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("Buildind Player", "Build Xlua support...", 0.2f);
                    if (isCancel)
                    {
                        shaco.DataSave.Instance.Remove("BuildInspector.BuildXluaSupportEndCallBack");
                    }
                }
                return !shaco.DataSave.Instance.ContainsKey("BuildInspector.BuildXluaSupportEndCallBack");
            }, () =>
            {
                EditorUtility.ClearProgressBar();
            });
#else
            Debug.LogError("should Enable HOTFIX_ENABLE marco in ProjectSettings !");
#endif
        }

        static private void OverriteSettings(BuildTarget buildTarget)
        {
            if (!string.IsNullOrEmpty(VERSION_PATH))
            {
                //写入版本
                shaco.Base.FileHelper.WriteAllByUserPath(Application.dataPath + "/" + VERSION_PATH, PlayerSettings.bundleVersion.ToString());
            }

            //设置c# API兼容性级别，至少应该是.Net 2.0以上
#if UNITY_5_3_OR_NEWER
            var targetGroup = BuildTargetToBuildTargetGroup(buildTarget);
            if (PlayerSettings.GetApiCompatibilityLevel(targetGroup) == ApiCompatibilityLevel.NET_2_0_Subset)
            {
                PlayerSettings.SetApiCompatibilityLevel(targetGroup, ApiCompatibilityLevel.NET_2_0);
            }
#else
            if (PlayerSettings.apiCompatibilityLevel == ApiCompatibilityLevel.NET_2_0_Subset)
            {
                PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
            }
#endif
        }

        /// <summary>
        /// 执行shell脚本
        /// <param name="shellScriptPath">脚本文件路径</param>
        /// <param name="isWaitForEnd">是否阻塞线程等待脚本执行完毕</param>
        /// <param name="args">附带参数</param>
        /// <return></return>
        /// </summary>
        private static string RunShell(string shellScriptPath, bool isWaitForEnd, params string[] args)
        {
            var process = new System.Diagnostics.Process();

            var argsCombine = string.Empty;
            for (int i = 0; i < args.Length; ++i)
            {
                argsCombine += args[i];
                if (i < args.Length - 1)
                    argsCombine += " ";
            }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            process.StartInfo.FileName = "/bin/bash";
#else
            process.StartInfo.FileName = _windowsRunShellExePath;
#endif

            process.StartInfo.Arguments = shellScriptPath + (args.Length > 0 ? " " + argsCombine : string.Empty);

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            if (isWaitForEnd)
            {
                process.WaitForExit();
            }

            return string.Empty;
        }

        private static string GetFullShellScriptsPath(string replativePath)
        {
            return shaco.Base.FileHelper.GetCurrentSourceFolderPath().RemoveLastPathByLevel(2).ContactPath("/ShellScripts/" + replativePath);
        }

        //从git更新工程，并检查工程是否还有修改内容，如果有修改内容则需要确认是否继续打包
        private static void CheckProjectUpdate(bool isBatchMode, System.Action<bool> callbackEnd)
        {
            var projectDiffPath = GetFullShellScriptsPath("diff.tmp");
            bool isCancel = false;

            //删除上一次的项目更新日志文件
            shaco.Base.FileHelper.DeleteByUserPath(projectDiffPath);

            var shellPath = GetFullShellScriptsPath("CheckProjectUpdate.sh");
            Debug.Log("check project update=" + RunShell(shellPath, false, _projectUpdateType.ToString()));

            //等待项目更新的日志文件写入完毕
            shaco.WaitFor.Run(() =>
            {
                //非jenkins脚本打包模式下才显示进度条
                if (!isBatchMode)
                {
                    isCancel = EditorUtility.DisplayCancelableProgressBar("Buildind Player", "check project update...(" + _projectUpdateType + ")", 0.1f);
                }
                return isCancel || shaco.Base.FileHelper.ExistsFile(projectDiffPath);
            }, () =>
            {
                if (isCancel)
                {
                    callbackEnd(false);
                    return;
                }

                var gitDataTmp = shaco.Base.FileHelper.ReadAllByUserPath(projectDiffPath);

                if (gitDataTmp.Length > 1)
                {
                    if (isBatchMode)
                    {
                        Debug.LogWarning("BuildInspector CheckProjectUpdate warning: Has some changed need commit, please check your project ~");
                        callbackEnd(true);
                    }
                    else
                    {
                        var retButton = EditorUtility.DisplayDialogComplex("Git warning", "Has some files need to commit, Do you need to continue build ?", "Force Continue", "Stop", "Discard All Local Changes");

                        //回滚所有数据
                        if (retButton == 2)
                        {
                            RunShell(shellPath, true, _projectUpdateType.ToString(), "DiscardAllLocalChanges");
                            CheckProjectUpdate(isBatchMode, callbackEnd);
                        }
                        else
                        {
                            callbackEnd(retButton == 0);
                        }
                    }
                }
                else
                {
                    callbackEnd(true);
                }
            });
        }

        //编译xcode，导出ipa
        private static void BuiliPA(string exportXcodePath, bool isBatchMode)
        {
            //设置plist内容
            var exportOptionPlistPath = GetFullShellScriptsPath("iOS/export_option.plist");
            if (!shaco.Base.FileHelper.ExistsFile(exportOptionPlistPath))
            {
                Debug.LogError("BuildInspector BuildXcodeAndiPA erorr: missing exportOption.plist");
                return;
            }
            var exportOptionPlist = new shaco.iOS.Xcode.PlistDocument();
            exportOptionPlist.ReadFromFile(exportOptionPlistPath);
            var exportOptionDict = exportOptionPlist.root.AsDict();

            //set method
            switch (_iOSExportOption)
            {
                case IOSExportOption.AdHoc: exportOptionDict.SetString("method", "ad-hoc"); break;
                case IOSExportOption.AppStore: exportOptionDict.SetString("method", "app-store"); break;
                case IOSExportOption.Enterprise: exportOptionDict.SetString("method", "enterprise"); break;
                case IOSExportOption.Development: exportOptionDict.SetString("method", "development"); break;
                default: Debug.LogError("BuildInspector BuildXcodeAndiPA error: unsupport export option=" + _iOSExportOption); return;
            }

            //set provisioningProfiles
#if UNITY_5_3_OR_NEWER
            var bundleIdentifier = PlayerSettings.applicationIdentifier;
#else
            var bundleIdentifier = PlayerSettings.bundleIdentifier;
#endif
            var exportOptionProvisioningDic = exportOptionDict["provisioningProfiles"].AsDict();
            exportOptionProvisioningDic.values.Clear();
            exportOptionDict["provisioningProfiles"].AsDict().SetString(bundleIdentifier, _iOSProvisioningProfileSpecifier);
            exportOptionPlist.WriteToFile(exportOptionPlistPath);

            //如果jenins运行的打包脚本，会在jenkins上调用它，以便查看日志
            if (!isBatchMode)
            {
                var shellPath = GetFullShellScriptsPath("WillBuildXcode.sh");
                Debug.Log("build ipa=" + RunShell(shellPath, false, exportXcodePath, "buildPackage=true"));
            }
        }

        //刷新iOS打包证书
        private static void UpdateiOSCertificate()
        {
            //已经在XcodeBuildListener中设置过了
        }

        //获取打包平台对应的路径标记
        private static string GetbuildTargetFlag(BuildTarget buildTarget, string serverMode)
        {
            DateTime now = DateTime.Now;
            string formatTime = now.Year.ToString("0000") + "_" + now.Month.ToString("00") + "_" + now.Day.ToString("00") + "_" + now.Hour.ToString("00") + "_" + now.Minute.ToString("00");
            var formatVersion = PlayerSettings.bundleVersion + "_";
#if UNITY_5_3_OR_NEWER
            if (buildTarget == BuildTarget.Android)
            {
                formatVersion += PlayerSettings.Android.bundleVersionCode;
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                formatVersion += PlayerSettings.iOS.buildNumber;
            }
#else
            formatVersion += PlayerSettings.shortBundleVersion;
#endif

            var extensions = string.Empty;
            switch (buildTarget)
            {
                case BuildTarget.Android: extensions = ".apk"; break;
                case BUILD_TARGET_IOS: /*extensions = ".ipa";*/ break;
                default: Debug.LogError("BuildInspector GetbuildTargetFlag error: unsuppport platform=" + buildTarget); break;
            }

            return serverMode + "/" + PlayerSettings.productName + "_" + formatVersion + "_" + formatTime + extensions;
        }
    }
}