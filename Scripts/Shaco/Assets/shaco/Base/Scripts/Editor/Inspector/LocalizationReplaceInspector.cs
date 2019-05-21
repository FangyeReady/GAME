using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 本地化语言替换编辑窗口类，用于查找、统计和替换文本中本地化语言
/// </summary>
namespace shacoEditor
{
    public class LocalizationReplaceInspector : EditorWindow
    {
        private enum Language
        {
            Chinese,
            English,
            Japanese
        }

        //特殊符号转义符表，以防止编码解析问题
        private string[,] TRANSFER_FLAGS = new string[2,2]
        {
            {" ", "##1"},
            {"\t", "##2"}
        };

        //需要收集文件的后缀名
        private List<string> _collectionFileExtensions = new List<string> { "cs" };

        //查找文本的前后关联符号
        private List<shaco.Base.Utility.LocalizationCollectPairFlag> _collectFlags = new List<shaco.Base.Utility.LocalizationCollectPairFlag>
        {
            new shaco.Base.Utility.LocalizationCollectPairFlag() { startFlag = "\"", endFlag = "\"" }
        };

        //需要过滤的前后关联符号
        private List<shaco.Base.Utility.LocalizationCollectPairFlag> _ignoreCollectFlags = new List<shaco.Base.Utility.LocalizationCollectPairFlag>
        {
            new shaco.Base.Utility.LocalizationCollectPairFlag() { startFlag = "//", endFlag = "" },
            new shaco.Base.Utility.LocalizationCollectPairFlag() { startFlag = "/*", endFlag = "*/" }
        };

        //外部导入文本分割符号
        private List<string> _externalImportSplitFlag = new List<string> { "\t", " " };

        //外部导入文本中，文件路径所在列
        private int _externalImportFileCol = 0;

        //外部导入文本中，原文所在列
        private int _externalImportOriginalCol = 2;

        //外部导入文本中，译文所在列
        private int _externalImportTranslationCol = 3;

        //外部导入文本中，额外参数所在列
        private int _externalImportExtraParameter = 4;

        //外部导入文本中，最大列数
        private int _externalImportMaxLine = 5;

        //收集语言包和替换类
        private ILocalizationReplace _localizationReplaceInterface = new LocalizationReplaceDefault();

        private string[] _inputCollectFlag = new string[2];
        private string[] _inputIgnoreCollectFlag = new string[2];

        private LocalizationReplaceInspector _currentWindow = null;
        private string _importCollectCharactersFolder = string.Empty;
        private string _exportCollectCharacetersFolder = string.Empty;
        private Language _collectLanguage = Language.Chinese;
        private Vector2 _scrollPositionTitleWindow = Vector2.zero;
        private Vector2 _scrollPositionUpWindow = Vector2.zero;
        private Vector2 _scrollPositionDownWindow = Vector2.zero;
        private GUIHelper.WindowSplitter _lineSeparator = new GUIHelper.WindowSplitter();


        [MenuItem("shaco/Tools/LocalizationReplaceInspector _F11", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.LOCALIZATION_REPLACE)]
        static void OpenLocalizationReplaceInspector()
        {
            EditorHelper.GetWindow<LocalizationReplaceInspector>(null, true, "LocalizationReplaceInspector");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<LocalizationReplaceInspector>(this, true, "LocalizationReplaceInspector");
            _currentWindow.LoadSettings();

            //设置窗口分割显示大小
            _lineSeparator.SetSplitWindow(this, 0.3f, 0.3f, 0.3f);
        }

        void OnDestroy()
        {
            SaveSettings();
        }

        void OnGUI()
        {
            if (null == _currentWindow)
                return;

            GUILayout.BeginArea(_lineSeparator.GetSplitWindowRect(0));
            {
                _scrollPositionTitleWindow = GUILayout.BeginScrollView(_scrollPositionTitleWindow);
                {
                    DrawTitle();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(_lineSeparator.GetSplitWindowRect(1));
            {
                _scrollPositionUpWindow = GUILayout.BeginScrollView(_scrollPositionUpWindow);
                {
                    DrawExportLanguage();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(_lineSeparator.GetSplitWindowRect(2));
            {
                _scrollPositionDownWindow = GUILayout.BeginScrollView(_scrollPositionDownWindow);
                {
                    DrawImportLanguage();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();

            //绘制分割线
            _lineSeparator.Draw();
        }

        private void SaveSettings()
        {
            shaco.DataSave.Instance.Write("LocalizationReplaceInspector+_importCollectCharactersFolder", _importCollectCharactersFolder);
            shaco.DataSave.Instance.Write("LocalizationReplaceInspector+_exportCollectCharacetersFolder", _exportCollectCharacetersFolder);
            if (null != _localizationReplaceInterface)
            {
                shaco.DataSave.Instance.Write("LocalizationReplaceInspector+_localizationReplaceInterface", _localizationReplaceInterface.ToTypeString());
            }
            shaco.DataSave.Instance.Write("LocalizationReplaceInspector+_collectionFileExtensions", _collectionFileExtensions.ToArray());
            shaco.DataSave.Instance.Write("LocalizationReplaceInspector+_localizationReplaceInterface", _localizationReplaceInterface.ToTypeString());
        }

        private void LoadSettings()
        {
            _importCollectCharactersFolder = shaco.DataSave.Instance.ReadString("LocalizationReplaceInspector+_importCollectCharactersFolder");
            _exportCollectCharacetersFolder = shaco.DataSave.Instance.ReadString("LocalizationReplaceInspector+_exportCollectCharacetersFolder");
            var localizationReplaceInterfaceNameTmp = shaco.DataSave.Instance.ReadString("LocalizationReplaceInspector+_localizationReplaceInterface");
            if (!string.IsNullOrEmpty(localizationReplaceInterfaceNameTmp))
            {
                _localizationReplaceInterface = shaco.Base.Utility.Instantiate(localizationReplaceInterfaceNameTmp) as ILocalizationReplace;
            }
            _collectionFileExtensions = shaco.DataSave.Instance.ReadStringArray("LocalizationReplaceInspector+_collectionFileExtensions", new string[] { "cs" }).ToArrayList();
            _localizationReplaceInterface = shaco.Base.Utility.Instantiate(shaco.DataSave.Instance.ReadString("LocalizationReplaceInspector+_localizationReplaceInterface")) as ILocalizationReplace;
        }

        private void DrawTitle()
        {
            _localizationReplaceInterface = GUILayoutHelper.PopupTypeField("Repalce Interface", _localizationReplaceInterface);
            _localizationReplaceInterface.DrawInspector();
        }

        private void DrawExportLanguage()
        {
            GUILayout.BeginVertical("box");
            {
                //导入路径
                _importCollectCharactersFolder = GUILayoutHelper.PathField("Import Path", _importCollectCharactersFolder, string.Empty);

                //导出路径
                _exportCollectCharacetersFolder = GUILayoutHelper.PathField("Export Path", _exportCollectCharacetersFolder, string.Empty);

                //需要收集语言信息的文件后缀名
                GUILayoutHelper.DrawList(_collectionFileExtensions, "File Extension");

                //收集的符号
                GUILayout.BeginVertical("box");
                {
                    //收集的语言类型
                    GUILayout.BeginVertical("box");
                    {
                        _collectLanguage = (Language)EditorGUILayout.EnumPopup("Language", _collectLanguage);
                    }
                    GUILayout.EndVertical();

                    //需要收集的符号
                    DrawFlags("Collect Flags", _collectFlags, _inputCollectFlag);

                    //收集过程中需要过滤的符号
                    if (!_collectFlags.IsNullOrEmpty() && (!string.IsNullOrEmpty(_collectFlags[0].startFlag) || !string.IsNullOrEmpty(_collectFlags[0].endFlag)))
                    {
                        DrawFlags("Ignore Collect Flags", _ignoreCollectFlags, _inputIgnoreCollectFlag);
                    }

                    EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_importCollectCharactersFolder) || string.IsNullOrEmpty(_exportCollectCharacetersFolder));
                    {
                        //从导入目录中收集文本，并导出到目录中
                        if (GUILayout.Button("Collect And Export"))
                        {
                            //测试用
                            // var s = "\"layerNo\"/// <param name=\"layerNo\">Layer no.</param>//1   hh";
                            // var s = shaco.Base.FileHelper.ReadAllByUserPath("/Users/liuchang/Desktop/1.txt");
                            // s = s.RemoveAnnotation("/*", "*/", _collectFlags.ToArray());
                            // s = s.RemoveAnnotation("//", "", _collectFlags.ToArray());
                            // Debug.Log(s);
                            CollectAndExportLanguage(_importCollectCharactersFolder, _exportCollectCharacetersFolder, _collectLanguage);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawImportLanguage()
        {
            GUILayout.BeginVertical("box");
            {
                //翻译文件路径
                _exportCollectCharacetersFolder = GUILayoutHelper.PathField("Tranditional Folder Path", _exportCollectCharacetersFolder, string.Empty);

                //替换目标语言类型
                _collectLanguage = (Language)EditorGUILayout.EnumPopup("Language", _collectLanguage);

                //收集替换翻译的分割符号
                GUILayout.BeginVertical("box");
                {
                    GUILayoutHelper.DrawList(_externalImportSplitFlag, "External Import Split Flags");
                }
                GUILayout.EndVertical();

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_exportCollectCharacetersFolder));
                {
                    //从导入目录中收集文本，并替换到导出目录中
                    if (GUILayout.Button("Import And Replace"))
                    {
                        ImportAndReplaceLanguage(_exportCollectCharacetersFolder, _collectLanguage, _collectionFileExtensions);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制标记编辑界面组件
        /// <param name="prefix">前缀名字</param>
        /// <param name="flags">标记内容</param>
        /// <param name="inputFlags">正在输入(新增)的标记</param>
        /// </summary>
        private void DrawFlags(string prefix, List<shaco.Base.Utility.LocalizationCollectPairFlag> flags, string[] inputFlags)
        {
            if (2 != inputFlags.Length)
            {
                Debug.LogError("LocalizationReplaceInspector DrawFlags erorr: input flag not a pair");
                return;
            }

            GUILayout.BeginVertical("box");
            {
                if (GUILayoutHelper.DrawHeader(prefix, prefix))
                {
                    for (int i = 0; i < flags.Count; ++i)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUI.changed = false;

                            //start flag
                            if (string.IsNullOrEmpty(flags[i].startFlag))
                            {
                                EditorGUI.BeginDisabledGroup(true);
                                {
                                    EditorGUILayout.TextField("Start", "Any");
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            else
                            {
                                flags[i].startFlag = EditorGUILayout.TextField("Start", flags[i].startFlag);
                            }

                            //end flag
                            if (string.IsNullOrEmpty(flags[i].endFlag))
                            {
                                EditorGUI.BeginDisabledGroup(true);
                                {
                                    EditorGUILayout.TextField("End", "Any");
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            else
                            {
                                flags[i].endFlag = EditorGUILayout.TextField("End", flags[i].endFlag);
                            }

                            //当有设置为空字符串的时候，默认取消一次焦点
                            if (GUI.changed)
                            {
                                if (string.IsNullOrEmpty(flags[i].startFlag) || string.IsNullOrEmpty(flags[i].endFlag))
                                {
                                    GUI.FocusControl(string.Empty);
                                }
                            }

                            EditorGUI.BeginDisabledGroup(flags.Count <= 1);
                            {
                                if (GUILayout.Button("-"))
                                {
                                    flags.RemoveAt(i);
                                    break;
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.BeginHorizontal();
                {
                    inputFlags[0] = EditorGUILayout.TextField("New Start", inputFlags[0]);
                    inputFlags[1] = EditorGUILayout.TextField("New End", inputFlags[1]);

                    EditorGUI.BeginDisabledGroup(inputFlags.Length % 2 != 0);
                    {
                        if (GUILayout.Button("+"))
                        {
                            flags.Add(new shaco.Base.Utility.LocalizationCollectPairFlag() { startFlag = inputFlags[0], endFlag = inputFlags[1] });
                            inputFlags[0] = string.Empty;
                            inputFlags[1] = string.Empty;
                            GUI.FocusControl(string.Empty);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 收集文本并导出结果
        /// <param name="importPath">导入文本目录，会遍历该目录所有文本</param>
        /// <param name="exportPath">导出目录，将遍历后的语言包内容打包写入导出目录</param>
        /// <param name="language">需要收集的语言类型</param>
        /// </summary>
        private void CollectAndExportLanguage(string importPath, string exportPath, Language language)
        {
            if (string.IsNullOrEmpty(exportPath))
            {
                Debug.LogError("LocalizationReplaceInspector CollectAndExportLanguage erorr: invalid export path");
                return;
            }

            //开始筛选文本并导出结果 
            _localizationReplaceInterface.GetAllLanguageString(importPath, (List<shaco.Base.Utility.LocalizationCollectnfo> importInfo) =>
            {
                CollectAndExportLanguage(importInfo, exportPath, language);
            }, _collectionFileExtensions.ToArray());
        }

        /// <summary>
        /// 筛选文本并导出结果s
        /// <param name="allLanguageStrings">所有语言文本</param>
        /// <param name="exportPath">导出目录，将遍历后的语言包内容打包写入导出目录</param>
        /// <param name="language">需要收集的语言类型</param>
        /// </summary>
        private void CollectAndExportLanguage(List<shaco.Base.Utility.LocalizationCollectnfo> importInfo, string exportPath, Language language)
        {
            if (importInfo.IsNullOrEmpty())
            {
                Debug.Log("LocalizationReplaceInspector CollectAndExportLanguage: import information is empty");
                return;
            }

            bool hasError = false;
            int collectLanguageCount = 0;

            //<文本内容，导出信息>
            var exportStrings = new List<Dictionary<string, shaco.Base.Utility.LocalizationExportInfo>>();

            //筛选出的单行文本信息
            Dictionary<string, shaco.Base.Utility.LocalizationExportInfo> selectLineCharacters = null;

            //用户主动停止
            bool userForceStop = false;

            //当前文件下标
            int currentIndex = 0;

            //当前文件路径
            string currentPath = string.Empty;

            //获取文件中包含有相应语言的文本
            shaco.Base.Coroutine.ForeachAsync(importInfo, (object value) =>
            {
                try
                {
                    if (userForceStop)
                    {
                        return false;
                    }

                    var importInfoTmp = value as shaco.Base.Utility.LocalizationCollectnfo;
                    var fileString = importInfoTmp.languageString;
                    var dicTmp = new Dictionary<string, shaco.Base.Utility.LocalizationExportInfo>();
                    currentPath = importInfoTmp.path;

                    exportStrings.Add(dicTmp);

                    //去除多余的换行符
                    fileString = fileString.RemoveAll("\r");

                    //删除注释内容
                    if (!_ignoreCollectFlags.IsNullOrEmpty())
                    {
                        //删除注释
                        for (int i = _ignoreCollectFlags.Count - 1; i >= 0; --i)
                        {
                            fileString = fileString.RemoveAnnotation(_ignoreCollectFlags[i].startFlag, _ignoreCollectFlags[i].endFlag, _collectFlags.ToArray());
                        }
                    }

                    //获取文本每一行的字符串
                    var lineString = fileString.Split('\n');

                    //筛选对应语言字符串
                    bool shouldBreak = false;
                    for (int i = 0; i < lineString.Length; ++i)
                    {
                        var stringTmp = lineString[i];

                        //不处理空字符串
                        if (string.IsNullOrEmpty(stringTmp))
                        {
                            continue;
                        }

                        switch (language)
                        {
                            case Language.Chinese:
                                {
                                    selectLineCharacters = shaco.Base.Utility.SelectChineseCharacter(stringTmp, _collectFlags);
                                    break;
                                }
                            case Language.Japanese:
                                {
                                    selectLineCharacters = shaco.Base.Utility.SelectJapaneseCharacter(stringTmp, _collectFlags);
                                    break;
                                }
                            case Language.English:
                                {
                                    selectLineCharacters = shaco.Base.Utility.SelectEnglishCharacter(stringTmp, _collectFlags);
                                    break;
                                }
                            default: Debug.LogError("LocalizationReplaceInspector CollectAndExportLanguage erorr: unsupport language type=" + language); shouldBreak = true; break;
                        }

                        if (!selectLineCharacters.IsNullOrEmpty())
                        {
                            //设置筛选出来的文本内容所在文件行 
                            foreach (var iter in selectLineCharacters)
                            {
                                //记录文件地址
                                iter.Value.path = importInfoTmp.path;

                                //文本的行数是 + 1开始的
                                iter.Value.lineOriginal = i + 1;

                                //记录参数
                                iter.Value.parameter = importInfoTmp.parameter;

                                if (!dicTmp.ContainsKey(iter.Key))
                                {
                                    dicTmp.Add(iter.Key, iter.Value);
                                    ++collectLanguageCount;
                                }
                            }
                        }

                        if (shouldBreak)
                        {
                            break;
                        }
                    }

                    ++currentIndex;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e + "\nurrentPath=" + currentPath);
                    hasError = true;
                    return false;
                }
                return !userForceStop;
            }
            , (float progress) =>
            {
                if (userForceStop)
                {
                    return;
                }

                string pathSafe = currentIndex >= 0 && currentIndex < importInfo.Count ? importInfo[currentIndex].path : string.Empty;
                pathSafe = EditorHelper.FullPathToUnityAssetPath(pathSafe);

                //太长的路径显示不下，则只保留后面的
                if (pathSafe.Length > 70)
                {
                    pathSafe = pathSafe.Substring(pathSafe.Length - 70);
                }

                userForceStop = EditorUtility.DisplayCancelableProgressBar("Collect Language (" + currentIndex + "/" + importInfo.Count + ")", pathSafe, progress * 0.8f);
                if (userForceStop)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                if (progress >= 1.0f)
                {
                    if (hasError)
                    {
                        EditorUtility.ClearProgressBar();
                    }
                    else
                    {
                        //写入收集的内容到文件
                        var writeStringTmp = new System.Text.StringBuilder();

                        //写入文件头
                        writeStringTmp.Append("File Path");
                        writeStringTmp.Append("\t");
                        writeStringTmp.Append("File line");
                        writeStringTmp.Append("\t");
                        writeStringTmp.Append("Original");
                        writeStringTmp.Append("\t");
                        writeStringTmp.Append("Translation");
                        writeStringTmp.Append("\t");
                        writeStringTmp.Append("Parameter");
                        writeStringTmp.Append("\n");

                        foreach (var iter in exportStrings)
                        {
                            //统一使用制表符分割开来，这样方便直接导入Excel自动分割
                            foreach (var iter2 in iter)
                            {
                                writeStringTmp.Append(SourceToTransferString(iter2.Value.path));
                                writeStringTmp.Append("\t");
                                writeStringTmp.Append(iter2.Value.lineOriginal);
                                writeStringTmp.Append("\t");
                                writeStringTmp.Append(SourceToTransferString(iter2.Value.textOriginal));
                                writeStringTmp.Append("\t");
                                writeStringTmp.Append(SourceToTransferString(iter2.Value.textTranslation));
                                writeStringTmp.Append("\t");
                                writeStringTmp.Append(SourceToTransferString(iter2.Value.parameter));
                                writeStringTmp.Append("\n");
                            }
                        }

                        //移除最后一个换行符
                        if (writeStringTmp.Length > 0)
                        {
                            writeStringTmp.Remove(writeStringTmp.Length - 1, 1);
                        }

                        shaco.Base.FileHelper.WriteAllByUserPathAsync(exportPath.ContactPath(language.ToString() + ".txt"), writeStringTmp.ToString(), (bool success) =>
                        {
                            Debug.Log("LocalizationReplaceInspector CollectAndExportLanguage success=" + success + " collect language count=" + collectLanguageCount + " path=" + exportPath);
                        }, (float progress2) =>
                        {
                            EditorUtility.DisplayProgressBar("Export Collection to path", "please wait", progress2 * 0.2f + 0.8f);
                            if (progress2 >= 1.0f)
                            {
                                EditorUtility.ClearProgressBar();
                            }
                        });
                    }
                }
            });
        }

        /// <summary>
        /// 导入外部文本内容并替换本地的对应语言文本，如果读取文件失败，可能是翻译文件路径与本地路径不一致导致，请在翻译文件中手动批量替换路径
        /// <param name="tranditionalPath">翻译文本目录，会遍历该目录所有文本，读取翻译信息并替换</param>
        /// <param name="language">替换语言类型</param>
        /// <param name="defaultCollectFileExtensions">默认收集文件的文件后缀名，主要用于文件替换的时候冲突检测</param>
        /// </summary>
        private void ImportAndReplaceLanguage(string tranditionalPath, Language language, List<string> defaultCollectFileExtensions)
        {
            //从外部文件中获取翻译信息
            CollectLanguageInfoFromExternalFile(tranditionalPath, language, (Dictionary<string, List<shaco.Base.Utility.LocalizationExportInfo>> exportInfo) =>
            {
                bool hasError = false;

                if (_localizationReplaceInterface.OnlyReplaceInMainThread())
                {
                    EditorUtility.DisplayProgressBar("Replace Language(Sync)", "please wait", 0.5f);
                    foreach (var iter in exportInfo)
                    {
                        var pathTmp = RepleaceTransferToSourceString(iter.Key, iter.Value);
                        _localizationReplaceInterface.RepalceLanguageString(pathTmp, iter.Value);
                    }
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    //替换本地翻译文本
                    shaco.Base.Coroutine.ForeachAsync(exportInfo, (object data) =>
                    {
                        try
                        {
                            var keyValuePairTmp = (System.Collections.Generic.KeyValuePair<string, List<shaco.Base.Utility.LocalizationExportInfo>>)data;

                            //如果目标不是配置默认导出的文件类型，则弹窗停止遍历替换
                            var fileExtensions = shaco.Base.FileHelper.GetFilNameExtension(keyValuePairTmp.Key);
                            if (!defaultCollectFileExtensions.Contains(fileExtensions))
                            {
                                hasError = true;
                                Debug.LogError(string.Format("An unknown file type will be replaced {0} \nyou can also add this type to the 'File Extension' List", keyValuePairTmp.Key));
                                return false;
                            }

                            //将所有转义文本转换为原文本
                            var pathTmp = RepleaceTransferToSourceString(keyValuePairTmp.Key, keyValuePairTmp.Value);
                            _localizationReplaceInterface.RepalceLanguageString(pathTmp, keyValuePairTmp.Value);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                            hasError = true;
                        }

                        return true;

                    }, (float progress) =>
                    {
                        EditorUtility.DisplayProgressBar("Replace Language(Async)", "please wait", progress);

                        if (progress >= 1.0f || hasError)
                        {
                            EditorUtility.ClearProgressBar();
                            AssetDatabase.SaveAssets();
                            EditorHelper.SaveCurrentScene();
                            Debug.Log("LocalizationReplaceInspector ImportAndReplaceLanguage success=" + !hasError);
                        }
                    });
                }
            });
        }

        /// <summary>
        /// 从外部文件中获取翻译信息
        /// <param name="tranditionalPath">翻译文件文件夹路径，会依次遍历该文件夹下符合_collectionFileExtensions后缀名的文件</param>
        /// <param name="callbackEnd">收集翻译信息完毕回调</param>
        /// <return>收集到的文件信息</return>
        /// </summary>
        private void CollectLanguageInfoFromExternalFile(string tranditionalPath, Language language, System.Action<Dictionary<string, List<shaco.Base.Utility.LocalizationExportInfo>>> callbackEnd)
        {
            var retValue = new Dictionary<string, List<shaco.Base.Utility.LocalizationExportInfo>>();
            var allFiles = new List<string>();
            var languageString = language.ToString();

            //文件信息需要的最大列
            int maxNeedCol = Mathf.Max(_externalImportFileCol, _externalImportOriginalCol);
            maxNeedCol = Mathf.Max(maxNeedCol, _externalImportMaxLine - 1);
            bool hasError = false;

            //开始遍历文件夹
            EditorUtility.DisplayProgressBar("Seeking all files in directory", "please wait", 0);
            shaco.Base.ThreadPool.RunThreadSafeCallBack(() =>
            {
                shaco.Base.FileHelper.GetSeekPath(tranditionalPath, ref allFiles, false, ".txt");
            }, () =>
            {
                shaco.Base.Coroutine.ForeachAsync(allFiles, (object value) =>
                {
                    //如果不是目标语言文件，则忽略掉
                    string path = value.ToString();
                    string fileNameWithoutExtensions = shaco.Base.FileHelper.RemoveExtension(shaco.Base.FileHelper.GetLastFileName(path));
                    if (!fileNameWithoutExtensions.Contains(languageString))
                    {
                        Debug.LogWarning("LocalizationReplaceInspector CollectLanguageInfoFromExternalFile warning: not traditional file, language=" + languageString + "\npath=" + path);
                        return true;
                    }

                    try
                    {
                        var readStringTmp = shaco.Base.FileHelper.ReadAllByUserPath(path);

                        if (!string.IsNullOrEmpty(readStringTmp))
                        {
                            var lineStrings = readStringTmp.Split("\n");

                            //这里i >= 1是为了过滤文件头第一行的描述文本
                            for (int i = lineStrings.Length - 1; i >= 1; --i)
                            {
                                var lineString = lineStrings[i];
                                var splitStrings = lineString.SplitLineStringRecursive(_externalImportSplitFlag);

                                //一行文本信息数量小于最大列数量，无法正确解析文本
                                if (splitStrings.Count < maxNeedCol)
                                {
                                    Debug.LogError("LocalizationReplaceInspector CollectLanguageInfoFromExternalFile error: not enough file col, split col=" + splitStrings.Count + " max need col=" + maxNeedCol + " lineString=" + lineString + "\npath=" + path);
                                }
                                else
                                {
                                    List<shaco.Base.Utility.LocalizationExportInfo> findTmp = null;
                                    if (!retValue.TryGetValue(splitStrings[_externalImportFileCol], out findTmp))
                                    {
                                        findTmp = new List<shaco.Base.Utility.LocalizationExportInfo>();
                                        retValue.Add(splitStrings[_externalImportFileCol], findTmp);
                                    }

                                    findTmp.Add(new shaco.Base.Utility.LocalizationExportInfo()
                                    {
                                        textOriginal = splitStrings[_externalImportOriginalCol],
                                        textTranslation = splitStrings[_externalImportTranslationCol],
                                        parameter = splitStrings[_externalImportExtraParameter]
                                    });
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                        hasError = true;
                    }
                    return true;
                },
                (float progress) =>
                {
                    EditorUtility.DisplayProgressBar("Collect Language", "please wait", progress);

                    if (progress >= 1.0f || hasError)
                    {
                        EditorUtility.ClearProgressBar();

                        if (null != callbackEnd) callbackEnd(retValue);
                    }
                });
            });
        }

        /// <summary>
        /// 原文文转换为转义文本
        /// <param name="source">原文本</param>
        /// <return>转义文本</return>
        /// </summary>
        private string SourceToTransferString(string source)
        {
            var retValue = source;
            for (int i = TRANSFER_FLAGS.Rank - 1; i >= 0; --i)
            {
                retValue = retValue.Replace(TRANSFER_FLAGS[i, 0], TRANSFER_FLAGS[i, 1]);
            }
            return retValue;
        }

        /// <summary>
        /// 转义文本转换为原文本
        /// <param name="transfer">转义文本</param>
        /// <return>原文本</return>
        /// </summary>
        private string TransferToSourceString(string transfer)
        {
            var retValue = transfer;
            for (int i = TRANSFER_FLAGS.Rank - 1; i >= 0; --i)
            {
                retValue = retValue.Replace(TRANSFER_FLAGS[i, 1], TRANSFER_FLAGS[i, 0]);
            }
            return retValue;
        }

        /// <summary>
        /// 将所有转义文本转换为原文本
        /// <param name="path">需要翻译的转义路径</param>
        /// <param name="exportInfo">导出信息</param>
        /// <return>需要翻译的原文本路径</return>
        /// </summary>
        private string RepleaceTransferToSourceString(string path, List<shaco.Base.Utility.LocalizationExportInfo> exportInfo)
        {
            for (int i = exportInfo.Count - 1; i >= 0; --i)
            {
                exportInfo[i].path = TransferToSourceString(exportInfo[i].path);
                exportInfo[i].textOriginal = TransferToSourceString(exportInfo[i].textOriginal);
                exportInfo[i].textTranslation = TransferToSourceString(exportInfo[i].textTranslation);
                exportInfo[i].parameter = TransferToSourceString(exportInfo[i].parameter);
            }
            return TransferToSourceString(path);
        }
    }
}