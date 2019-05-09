using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using shaco;
using System.Linq;

namespace shacoEditor
{
    public partial class HotUpdateExportEditor : EditorWindow
    {
        public class SelectFile
        {
            public class FileInfo
            {
                public string Asset = null;

                public FileInfo(string Asset)
                {
                    this.Asset = Asset;
                }
            }

            public string AssetBundleMD5 = string.Empty;
            public Dictionary<string, FileInfo> ListAsset = new Dictionary<string, FileInfo>();
        }

        public int MaxShowAssetCount = 5;
        public int MaxShowAssetBundleCount = 5;
        public string SaveFileName = "FileName";
        public List<int> versionCodes = new List<int>();
        public string PrintPath = string.Empty;
        public string VerifyVersionControlPath = string.Empty;
        public Dictionary<string, SelectFile> MapAssetbundlePath = new Dictionary<string, SelectFile>();

        //该字典仅为了快速查看是否有相同id的资源倒入了
        public Dictionary<string, string> _mapAllExportAsset = new Dictionary<string, string>();
        public HotUpdateDefine.VersionControlLocalConfigEditor LocalConfigAsset = new HotUpdateDefine.VersionControlLocalConfigEditor();
        public string _strCurrentRootPath = string.Empty;
        public HotUpdateDefine.ExportFileAPI _apiVersion = HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles;
        public HotUpdateDefine.SerializeVersionControl _versionControlConfigOld = null;
        public HotUpdateDefine.SerializeVersionControl _versionControlConfig = new HotUpdateDefine.SerializeVersionControl();
        public HotUpdateVersionViewerEditor _windowVersionViewer = new HotUpdateVersionViewerEditor();
        public bool IsClosed = false;

        private HotUpdateExportEditor _currentWindow = null;

        private SelectFile.FileInfo[] _selectAsset = null;
        private Vector2 _vec2ScrollPosition = Vector2.zero;
        private string[] IGNORE_RESOURCE_EXTENSIONS = new string[] { "meta", "cs", "xlsx.txt", "xls.txt", "csv.txt", ".DS_Store", "*.git*", ".sdata" };
        private SelectFile.FileInfo _objSearchAssetBundle = null;
        private object _objPrevSearchAssetBundle = null;
        private AnimBool _isShowSettings = new AnimBool(true);
        private AnimBool _isShowBaseButton = new AnimBool(true);
        private AnimBool _isShowToolsButton = new AnimBool(true);
        private TextAsset _buildVersionAsset = null;
        private List<ExportReplaceAssetInfo> _replaceAssetInfos = new List<ExportReplaceAssetInfo>();
        private GUIHelper.WindowSplitter _dragLineSeparator = new GUIHelper.WindowSplitter();

        [MenuItem("shaco/HotUpdateResourceBuilder %#e", false, ToolsGlobalDefine.MenuPriority.Default.HOT_UPDATE_EXPORT)]
        static void ShowHotUpdateExportWindow()
        {
            EditorHelper.GetWindow<HotUpdateExportEditor>(null, true, "HotUpdateResourceBuilder");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<HotUpdateExportEditor>(this, true, "HotUpdateResourceBuilder");
            _currentWindow.Init();
        }

        public void UpdateDatas()
        {
            if (null != _windowVersionViewer)
            {
                _windowVersionViewer.Init(this);
                _windowVersionViewer.UpdateDrawFolder();
            }
        }

        private void Init()
        {
            this.OnClearButtonClick();

            //读取保存配置
            // _rectWindow = new Rect(_currentWindow.position.x, _currentWindow.position.y, _currentWindow.position.width, _currentWindow.position.height);
            SaveFileName = EditorPrefs.GetString(GetDataSaveKey("SaveFileName"));
            PrintPath = EditorPrefs.GetString(GetDataSaveKey("PrintPath"), string.Empty);
            VerifyVersionControlPath = EditorPrefs.GetString(GetDataSaveKey("VerifyVersionControlPath"), string.Empty);
            MaxShowAssetCount = EditorPrefs.GetInt(GetDataSaveKey("MaxShowAssetCount"), 5);
            MaxShowAssetBundleCount = EditorPrefs.GetInt(GetDataSaveKey("MaxShowAssetBundleCount"), 5);
            _isShowSettings.value = EditorPrefs.GetBool(GetDataSaveKey("_isShowSettings"), true);
            _isShowBaseButton.value = EditorPrefs.GetBool(GetDataSaveKey("_isShowBaseButton"), true);
            _isShowToolsButton.value = EditorPrefs.GetBool(GetDataSaveKey("_isShowToolsButton"), true);
            var buildVersionSavePath = EditorPrefs.GetString(GetDataSaveKey("_buildVersionAsset"));

            shaco.DataSave.Instance.Remove(GetDataSaveKey("_leftWindowPercent"));
            shaco.DataSave.Instance.Remove(GetDataSaveKey("_rightWindowPercent"));
            var leftWindowPercent = shaco.DataSave.Instance.ReadFloat(GetDataSaveKey("_leftWindowPercent"));
            var rightWindowPercent = shaco.DataSave.Instance.ReadFloat(GetDataSaveKey("_rightWindowPercent"));

            _windowVersionViewer.Init(this);

            //设置版本号信息自动显示
            if (!string.IsNullOrEmpty(buildVersionSavePath))
            {
                _buildVersionAsset = AssetDatabase.LoadAssetAtPath(buildVersionSavePath, typeof(TextAsset)) as TextAsset;
            }
            SetVersionAutomatic();

            if (leftWindowPercent <= 0 || rightWindowPercent <= 0)
            {
                leftWindowPercent = 0.3f;
                rightWindowPercent = 0.7f;
            }
            _dragLineSeparator.SetSplitWindow(this, leftWindowPercent, rightWindowPercent);

            //刷新界面数据
            UpdateDatas();
        }

        private void OnClearButtonClick()
        {
            PrintPath = string.Empty;
            VerifyVersionControlPath = string.Empty;
            MapAssetbundlePath.Clear();
            _mapAllExportAsset.Clear();
            LocalConfigAsset.Clear();
            _strCurrentRootPath = string.Empty;
            _versionControlConfigOld = null;

            if (null != _windowVersionViewer)
                _windowVersionViewer.ClearDrawFolder();

            //清理资源
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        private string GetDataSaveKey(string key)
        {
            return "HotUpdateExportEditor" + key;
        }

        private void SetVersionAutomatic()
        {
            //重置version参数
            versionCodes.Clear();

            if (null == _buildVersionAsset)
            {
                return;
            }

            var readString = _buildVersionAsset.ToString();

            if (string.IsNullOrEmpty(readString))
            {
                return;
            }

            int indexFindVersion = readString.IndexOf("appVersion");
            if (indexFindVersion < 0)
            {
                var splitString = readString.Split(".");
                for (int i = 0; i < splitString.Length; ++i)
                {
                    versionCodes.Add(splitString[i].ToInt());
                }
                return;
            }

            indexFindVersion += "appVersion".Length;

            //find version code
            for (int i = indexFindVersion; i < readString.Length; ++i, ++indexFindVersion)
            {
                char cTmp = readString[i];
                if (shaco.Base.FileHelper.isNumber(cTmp))
                {
                    break;
                }
            }

            //get version code
            for (int i = indexFindVersion; i < readString.Length; ++i)
            {
                char cTmp = readString[i];
                if (!shaco.Base.FileHelper.isNumber(cTmp))
                {
                    break;
                }
                else
                {
                    if (cTmp != '.')
                    {
                        versionCodes.Add(cTmp.ToString().ToInt());
                    }
                }
            }
        }

        void OnGUI()
        {
            if (_currentWindow == null)
                return;
            this.Repaint();

            var leftWindowRect = _dragLineSeparator.GetSplitWindowRect(0);
            var rightWindowRect = _dragLineSeparator.GetSplitWindowRect(1);

            try
            {
                GUILayout.BeginArea(leftWindowRect);
                _vec2ScrollPosition = GUILayout.BeginScrollView(_vec2ScrollPosition);
            }
            catch (System.Exception)
            {
                //ignore exception
            }

            DrawSettings();
            DrawExcueteButtons();
            DrawTestButtons();
            // DrawCurrentSelectAssets();

            try
            {
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            catch (System.Exception)
            {
                //ignore exception
            }

            // draw separator line
            _dragLineSeparator.Draw();

            _windowVersionViewer.DrawInspector(rightWindowRect);
        }

        private void DrawSettings()
        {
            _isShowSettings.target = EditorHelper.Foldout(_isShowSettings.target, "Settings");
            if (EditorGUILayout.BeginFadeGroup(_isShowSettings.faded))
            {
                if (!string.IsNullOrEmpty(_strCurrentRootPath))
                {
                    GUILayout.BeginVertical("box");
                    {
                        GUI.changed = false;
                        var versionFolderNameTmp = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty);
                        var pathTmp = GUILayoutHelper.PathField("Root Path", _strCurrentRootPath.ContactPath(versionFolderNameTmp), string.Empty);
                        if (GUI.changed)
                        {
                            _strCurrentRootPath = pathTmp.Remove(versionFolderNameTmp);
                        }
                        _versionControlConfig.AutoEncryt = EditorGUILayout.Toggle("Auto Encrypt", _versionControlConfig.AutoEncryt);
                        _versionControlConfig.AutoBuildModifyPackage = EditorGUILayout.Toggle("Auto Build Modify Package", _versionControlConfig.AutoBuildModifyPackage);
                    }
                    GUILayout.EndVertical();
                }

                // MaxShowAssetBundleCount = EditorGUILayout.IntField("MaxShowAssetBundle", MaxShowAssetBundleCount);
                // MaxShowAssetCount = EditorGUILayout.IntField("MaxShowAsset", MaxShowAssetCount);

                DrawVersionSettings();

                //统一默认导出文件都是加密的，如果想不加密文件，则需要从外部导入版本控制文件和资源，再设置是否加密
                if (null != _versionControlConfig)
                {
                    _versionControlConfig.AutoEncryt = HotUpdateVersionControlDrawerEditor.DrawLocalConfig(this, _versionControlConfig.AutoEncryt);
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawExcueteButtons()
        {
            _isShowBaseButton.target = EditorHelper.Foldout(_isShowBaseButton.target, "Execuete");
            if (EditorGUILayout.BeginFadeGroup(_isShowBaseButton.faded))
            {
                if (EditorApplication.isCompiling)
                {
                    EditorGUILayout.HelpBox("Please wait for compiling", MessageType.Info);
                }
                EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Import"))
                        {
                            HotUpdateVersionImportEditor.GetConfigByVersionControl(ref _currentWindow);
                        }

                        if (Application.dataPath.Contains("liuchang"))
                        {
                            if (GUILayout.Button("ImportPrevious"))
                            {
                                ReloadExportDatas();
                            }
                        }
                    }

                    GUILayout.EndHorizontal();

                    if (MapAssetbundlePath.Count > 0 && GUILayout.Button("Clear"))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "Clear all export config ?", "Confirm", "Canel"))
                        {
                            OnClearButtonClick();
                        }
                    }

                    if (MapAssetbundlePath.Count > 0)
                    {
                        if (GUILayout.Button("ExportAll", GUILayout.Height(50)))
                        {
                            ExcuteExportProcess();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                HotUpdateVersionControlDrawerEditor.DrawCurrentSelectAssets(_selectAsset, MaxShowAssetCount, NewAssetBundle, NewAssetBundleDeepAssets);
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawTestButtons()
        {
            //draw test Button
            _isShowToolsButton.target = EditorHelper.Foldout(_isShowToolsButton.target, "Test");
            if (EditorGUILayout.BeginFadeGroup(_isShowToolsButton.faded))
            {
                if (GUILayout.Button("Print"))
                {
                    var pathTmp = EditorUtility.OpenFilePanel("Open Assetbundle", string.IsNullOrEmpty(PrintPath) ? Application.dataPath : PrintPath, "assetbundle");
                    if (!string.IsNullOrEmpty(pathTmp))
                    {
                        var loadTmp = new HotUpdateImportMemory();
                        loadTmp.CreateByMemoryByUserPath(pathTmp);

                        Debug.Log("is encryt=" + shaco.Base.EncryptDecrypt.IsEncryption(pathTmp) + " original=" + shaco.Base.EncryptDecrypt.GetCustomParameters(pathTmp).Contains(shaco.HotUpdateDefine.ORIGINAL_FILE_TAG));
                        loadTmp.PrintAllAsset();

                        loadTmp.Close();

                        PrintPath = shaco.Base.FileHelper.GetFolderNameByPath(pathTmp);
                        EditorPrefs.SetString(GetDataSaveKey("PrintPath"), PrintPath);
                    }
                }

                if (GUILayout.Button("ConvertToDownloadedResources"))
                {
                    var pathTmp = EditorUtility.OpenFolderPanel("Open Export Folder", string.IsNullOrEmpty(PrintPath) ? Application.dataPath : PrintPath, string.Empty);
                    if (!string.IsNullOrEmpty(pathTmp))
                    {
                        var fileNameCheck = shaco.Base.FileHelper.GetLastFileName(pathTmp);
                        if (fileNameCheck.Contains(shaco.HotUpdateDefine.VERSION_CONTROL))
                        {
                            var allFiles = new List<string>();
                            var sourceVersionControlFolderName = shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty);
                            var copyVersionControlFolderName = "/Copy/" + shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty);
                            shaco.Base.FileHelper.GetSeekPath(pathTmp.ContactPath("assets"), ref allFiles, "assetbundle");

                            //copy assetbundles
                            for (int i = allFiles.Count - 1; i >= 0; --i)
                            {
                                var saveTmpFilePath = allFiles[i].ReplaceFromBegin(sourceVersionControlFolderName, copyVersionControlFolderName, 1);
                                saveTmpFilePath = saveTmpFilePath.Substring(0, saveTmpFilePath.LastIndexOf(shaco.HotUpdateDefine.SIGN_FLAG));
                                saveTmpFilePath += shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE;
                                shaco.Base.FileHelper.CopyFileByUserPath(allFiles[i], saveTmpFilePath);
                            }

                            //copy version control configs
                            var configFiles = System.IO.Directory.GetFiles(pathTmp);
                            for (int i = configFiles.Length - 1; i >= 0; --i)
                            {
                                var saveTmpFilePath = configFiles[i].ReplaceFromBegin(sourceVersionControlFolderName, copyVersionControlFolderName, 1);
                                shaco.Base.FileHelper.CopyFileByUserPath(configFiles[i], saveTmpFilePath);
                            }

                            PrintPath = shaco.Base.FileHelper.GetFolderNameByPath(pathTmp);
                        }
                        else
                        {
                            Debug.LogError("HotUpdateExportEditor ConvertToDownloadedResources error: not a version control folder=" + pathTmp);
                        }
                    }
                }

                // if (GUILayout.Button("VerifyVersionControl"))
                // {
                //     var pathTmp = EditorUtility.OpenFolderPanel("Open VersionControl Folder", string.IsNullOrEmpty(VerifyVersionControlPath) ? Application.dataPath : VerifyVersionControlPath, HotUpdateDefine.VERSION_CONTROL);
                //     if (!string.IsNullOrEmpty(pathTmp))
                //     {

                //         var pathVersionControlJson = shaco.Base.FileHelper.ContactPath(pathTmp, HotUpdateHelper.AddFileTag(HotUpdateDefine.VERSION_CONTROL + HotUpdateDefine.EXTENSION_VERSION_CONTROL, string.Empty));
                //         var readJsonTmp = shaco.Base.FileHelper.ReadAllByUserPath(pathTmp);
                //         var versionControlTmp = shaco.LitJson.JsonMapper.ToObject<HotUpdateDefine.SerializeVersionControl>(string.Empty);
                //     }
                // } 
            }
            EditorGUILayout.EndFadeGroup();
        }

        private SelectFile.FileInfo[] GetCurrentSelection()
        {
            SelectFile.FileInfo[] retValue = null;
            var currentSelectAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            if (!currentSelectAsset.IsNullOrEmpty())
            {
                retValue = new SelectFile.FileInfo[currentSelectAsset.Length];
                for (int i = 0; i < currentSelectAsset.Length; ++i)
                {
                    retValue[i] = new SelectFile.FileInfo(AssetDatabase.GetAssetPath(currentSelectAsset[i]));
                }
            }
            return retValue;
        }

        private void DrawCurrentSelectAssets()
        {
            _selectAsset = GetCurrentSelection();

            if (_selectAsset.IsNullOrEmpty())
                return;

            if (MaxShowAssetCount < 1)
                MaxShowAssetCount = 1;
            if (MaxShowAssetBundleCount < 1)
                MaxShowAssetBundleCount = 1;

            //check current search assetbundle
            if (MapAssetbundlePath.Count > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    if (_selectAsset.Length == 1)
                    {
                        _objSearchAssetBundle = _selectAsset[0];
                    }

                    if ((object)_objPrevSearchAssetBundle != (object)_objSearchAssetBundle)
                    {
                        _objPrevSearchAssetBundle = _objSearchAssetBundle;

                        var pathTmp = _objSearchAssetBundle.Asset.ToLower();

                        if (shaco.Base.FileHelper.HasFileNameExtension(pathTmp))
                        {
                            pathTmp = shaco.Base.FileHelper.RemoveExtension(_objSearchAssetBundle.Asset.ToLower());
                        }
                        pathTmp = shaco.Base.FileHelper.AddExtensions(pathTmp, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawVersionSettings()
        {
            GUILayout.BeginVertical("box");
            {
                if (null == _buildVersionAsset)
                {
                    EditorGUILayout.HelpBox("If the version file is null, 'Application.version' is used by default", MessageType.Warning);
                }

                GUI.changed = false;
                _buildVersionAsset = EditorGUILayout.ObjectField("Version Asset", _buildVersionAsset, typeof(TextAsset), true) as TextAsset;
                if (GUI.changed)
                {
                    SetVersionAutomatic();
                }

                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.LabelField("Version", GetResourceVersion());
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();
        }

        void OnDestroy()
        {
            IsClosed = true;

            EditorPrefs.SetString(GetDataSaveKey("SaveFileName"), SaveFileName);
            EditorPrefs.SetString(GetDataSaveKey("PrintPath"), PrintPath);
            EditorPrefs.SetString(GetDataSaveKey("VerifyVersionControlPath"), VerifyVersionControlPath);
            EditorPrefs.SetInt(GetDataSaveKey("MaxShowAssetCount"), MaxShowAssetCount);
            EditorPrefs.SetInt(GetDataSaveKey("MaxShowAssetBundleCount"), MaxShowAssetBundleCount);
            EditorPrefs.SetBool(GetDataSaveKey("_isShowSettings"), _isShowSettings.value);
            EditorPrefs.SetBool(GetDataSaveKey("_isShowBaseButton"), _isShowBaseButton.value);
            EditorPrefs.SetBool(GetDataSaveKey("_isShowToolsButton"), _isShowToolsButton.value);
            shaco.DataSave.Instance.Write(GetDataSaveKey("_leftWindowPercent"), _dragLineSeparator.GetSplitWindowPercent(0));
            shaco.DataSave.Instance.Write(GetDataSaveKey("_rightWindowPercent"), _dragLineSeparator.GetSplitWindowPercent(1));

            if (null != _buildVersionAsset)
            {
                var buildVersionPath = AssetDatabase.GetAssetPath(_buildVersionAsset);
                EditorPrefs.SetString(GetDataSaveKey("_buildVersionAsset"), buildVersionPath);
            }

            this.OnClearButtonClick();

            if (_windowVersionViewer != null)
            {
                _windowVersionViewer = null;
            }
            _currentWindow = null;
        }

        public void NewAssetBundle(SelectFile.FileInfo[] selects)
        {
            OpenFileDialog(true, false, selects);
        }

        public void NewAssetBundleDeepAssets(SelectFile.FileInfo[] selects)
        {
            OpenFileDialog(true, true, selects);
        }

        void ReloadExportDatas()
        {
            HotUpdateVersionImportEditor.GetPreviousConfigByVersionControl(ref _currentWindow);
        }

        //打开路径选择对话框
        void OpenFileDialog(bool isOneFileToOneAssetBundle, bool isDeepAssets, SelectFile.FileInfo[] selects)
        {
            if (selects.Length > 0)
                _selectAsset = selects;

            if (MapAssetbundlePath.Count == 0)
            {
                // 打开保存面板，获取用户选择的路径
                var strDefaultPath = string.Empty;
                if (string.IsNullOrEmpty(_strCurrentRootPath))
                {
                    strDefaultPath = Application.streamingAssetsPath;
                }
                else
                {
                    strDefaultPath = shaco.Base.FileHelper.GetFolderNameByPath(_strCurrentRootPath, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
                }

                _strCurrentRootPath = EditorUtility.SaveFolderPanel("Save Resource", strDefaultPath, string.Empty);
                if (!string.IsNullOrEmpty(_strCurrentRootPath))
                {
                    var strInputFilename = shaco.Base.FileHelper.GetLastFileName(_strCurrentRootPath, shaco.Base.FileDefine.PATH_FLAG_SPLIT, true);

                    strInputFilename = HotUpdateHelper.AssetBundleKeyToPath(strInputFilename);

                    _strCurrentRootPath = shaco.Base.FileHelper.GetFolderNameByPath(_strCurrentRootPath, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
                    _strCurrentRootPath = shaco.Base.FileHelper.RemoveSubStringByFind(_strCurrentRootPath, HotUpdateDefine.SIGN_FLAG);
                    OpenFileDialogEnd(strInputFilename, isOneFileToOneAssetBundle, isDeepAssets, _selectAsset);
                }
            }
            else
            {
                //自动使用第一个选中的文件夹名字作为资源包的名字
                if (isOneFileToOneAssetBundle && _selectAsset.Length > 0)
                {
                    var path = _selectAsset[0].Asset.ToLower();
                    OpenFileDialogEnd(path, isOneFileToOneAssetBundle, isDeepAssets, _selectAsset);
                }
                else
                {
                    //默认使用第一次使用过的路径作为前置路径
                    HotUpdatePathIntputEditor.OpenHotUpdatePathInputWindow((string path) =>
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (path[0].ToString() == shaco.Base.FileDefine.PATH_FLAG_SPLIT)
                                path = path.Remove(0, 1);

                            OpenFileDialogEnd(path, isOneFileToOneAssetBundle, isDeepAssets, _selectAsset);
                        }
                    }, this.position);
                }
            }
            _objPrevSearchAssetBundle = null;
        }

        void OpenFileDialogEnd(string filename, bool isOneFileToOneAssetBundle, bool isDeepAssets, params SelectFile.FileInfo[] selects)
        {
            if (!isOneFileToOneAssetBundle)
            {
                if (shaco.Base.FileHelper.HasFileNameExtension(filename))
                {
                    filename = shaco.Base.FileHelper.RemoveExtension(filename);
                }

                filename = shaco.Base.FileHelper.AddExtensions(filename, HotUpdateDefine.EXTENSION_ASSETBUNDLE);

                var newItem = new SelectFile();
                if (SelectCurrentAssetBundleFiles(filename, newItem, selects))
                {
                    if (!MapAssetbundlePath.ContainsKey(filename))
                    {
                        MapAssetbundlePath.Add(filename, newItem);
                    }
                    else
                    {
                        var oldItem = MapAssetbundlePath[filename];
                        oldItem.AssetBundleMD5 = newItem.AssetBundleMD5;

                        foreach (var key in newItem.ListAsset.Keys)
                        {
                            if (oldItem.ListAsset.ContainsKey(key))
                            {
                                Debug.LogError("add asset to assetbundle erorr: has same asset=" + key);
                            }
                            else
                            {
                                oldItem.ListAsset.Add(key, newItem.ListAsset[key]);
                            }
                        }
                    }
                    SaveFileName = filename;
                }
            }
            else
            {
                if (isDeepAssets)
                {
                    if (selects.Length == 0)
                    {
                        Debug.LogError("HotUpdateExportEditor OpenFileDialogEnd nothing need build, Probably filtered out all over, please check 'IGNORE_RESOURCE_EXTENSIONS'");
                        return;
                    }

                    int exportAssetsCount = 0;
                    for (int i = 0; i < selects.Length; ++i)
                    {
                        if (null == selects[i] || string.IsNullOrEmpty(selects[i].Asset))
                        {
                            continue;
                        }

                        //如果是文件夹，则遍历该文件夹，对每个文件打包
                        var fullPathTmp = EditorHelper.GetFullPath(selects[i].Asset);
                        if (shaco.Base.FileHelper.ExistsDirectory(fullPathTmp))
                        {
                            var allFilesTmp = new List<string>();

                            //自动过滤不需要打包的文件
                            shaco.Base.FileHelper.GetSeekPath(fullPathTmp, ref allFilesTmp, true, IGNORE_RESOURCE_EXTENSIONS);
                            for (int j = 0; j < allFilesTmp.Count; ++j)
                            {
                                var pathTmp = EditorHelper.FullPathToUnityAssetPath(allFilesTmp[j]);
                                var pathLower = pathTmp.ToLower();
                                if (!string.IsNullOrEmpty(pathTmp))
                                {
                                    OpenFileDialogEnd(pathLower, false, isDeepAssets, new SelectFile.FileInfo(pathLower));
                                    ++exportAssetsCount;
                                }
                            }
                        }
                        //如果是文件，直接打包
                        else if (shaco.Base.FileHelper.ExistsFile(fullPathTmp))
                        {
                            if (!string.IsNullOrEmpty(selects[i].Asset))
                            {
                                var pathTmp = selects[i].Asset;
                                OpenFileDialogEnd(pathTmp.ToLower(), false, isDeepAssets, selects[i]);
                                ++exportAssetsCount;
                            }
                        }
                        //丢失文件
                        else
                        {
                            Debug.LogError("HotUpdateExportEditor OpenFileDialogEnd error: missing file=" + fullPathTmp);
                        }
                    }

                    if (exportAssetsCount <= 0)
                    {
                        LogErrorNoAssetExport("Asset", selects.ToArrayList());
                    }
                }
                else
                {
                    var listAllSelect = selects.IsNullOrEmpty() ? GetCurrentSelection() : selects;
                    for (int i = 0; i < listAllSelect.Length; ++i)
                    {
                        var pathTmp = listAllSelect[i].Asset.ToLower();
                        List<string> listSelect = new List<string>();
                        if (shaco.Base.FileHelper.ExistsDirectory(pathTmp))
                        {
                            shaco.Base.FileHelper.GetSeekPath(pathTmp, ref listSelect, true, IGNORE_RESOURCE_EXTENSIONS);
                        }
                        else if (shaco.Base.FileHelper.ExistsFile(pathTmp))
                            listSelect.Add(pathTmp);
                        else
                        {
                            DisplayDialogError("missing file path=" + pathTmp);
                            continue;
                        }

                        var selectsTmp = new SelectFile.FileInfo[listSelect.Count];
                        for (int j = 0; j < listSelect.Count; ++j)
                        {
                            selectsTmp[j] = new SelectFile.FileInfo(listSelect[j]);
                        }
                        OpenFileDialogEnd(pathTmp, false, isDeepAssets, selectsTmp);
                    }
                }
            }
        }

        //设置当前assetbundle对应的文件名字
        //return: 如果设置失败返回false
        public bool SelectCurrentAssetBundleFiles(string filename, SelectFile SelectAssets, SelectFile.FileInfo[] selects)
        {
            List<SelectFile.FileInfo> listSameAsset = new List<SelectFile.FileInfo>();
            return SelectCurrentAssetBundleFiles(filename, SelectAssets, selects, ref listSameAsset);
        }
        public bool SelectCurrentAssetBundleFiles(string filename, SelectFile SelectAssets, SelectFile.FileInfo[] selects, ref List<SelectFile.FileInfo> listSameAsset)
        {
            if (!string.IsNullOrEmpty(_strCurrentRootPath))
            {
                int exportAssetsCount = 0;

                for (int i = 0; i < selects.Length; ++i)
                {
                    if (selects[i] == null)
                        continue;

                    var pathTmp = selects[i].Asset.ToLower();
                    bool isSameKey = _mapAllExportAsset.ContainsKey(selects[i].Asset);

                    if (isSameKey)
                    {
                        Debug.LogWarning("has same item in asssetbundle, asset path=" + pathTmp);
                        listSameAsset.Add(selects[i]);
                    }
                    else
                    {
                        SelectAssets.ListAsset.Add(pathTmp, new SelectFile.FileInfo(selects[i].Asset));
                        _mapAllExportAsset.Add(selects[i].Asset, null);
                        _windowVersionViewer.AddFile(filename, selects[i]);
                        ++exportAssetsCount;
                    }
                }

                if (exportAssetsCount <= 0)
                {
                    LogErrorNoAssetExport(filename, selects.ToArrayList());
                }

                return exportAssetsCount > 0;
            }
            else
            {
                DisplayDialogError("SelectCurrentAssetBundleFiles error: forget set prefix path");
            }
            return false;
        }

        // //获取当前选中的所有文件对象
        // public SelectFile.FileInfo[] GetSelectAssetWithCheckIgnore(SelectFile.FileInfo[] assets, ref List<SelectFile.FileInfo> listIgnoreAsset)
        // {
        //     listIgnoreAsset.Clear();

        //     if (assets == null || assets.Length == 0)
        //     {
        //         assets = GetCurrentSelection();
        //     }

        //     for (int i = assets.Length - 1; i >= 0; --i)
        //     {
        //         var pathCheck = assets[i].Asset.ToLower();
        //         bool isRemove = false;

        //         if (!shaco.Base.FileHelper.ExistsFile(pathCheck) && !shaco.Base.FileHelper.ExistsDirectory(pathCheck))
        //         {
        //             isRemove = true;
        //         }
        //         else
        //         {
        //             var filenameCheck = shaco.Base.FileHelper.GetLastFileName(pathCheck, shaco.Base.FileDefine.PATH_FLAG_SPLIT, true);
        //             if (filenameCheck.Contains(HotUpdateDefine.SIGN_FLAG) || filenameCheck.Contains(HotUpdateDefine.PATH_RELATIVE_FLAG))
        //             {
        //                 DisplayDialogError("select asset=" + filenameCheck + " can't contain special flag=" + HotUpdateDefine.SIGN_FLAG + " or " + HotUpdateDefine.PATH_RELATIVE_FLAG);
        //                 isRemove = true;
        //             }
        //             else
        //             {
        //                 for (int j = IGNORE_RESOURCE_EXTENSIONS.Length - 1; j >= 0; --j)
        //                 {
        //                     if (pathCheck.EndsWith(IGNORE_RESOURCE_EXTENSIONS[j]))
        //                     {
        //                         isRemove = true;
        //                         break;
        //                     }
        //                 }
        //             }
        //         }

        //         if (isRemove)
        //         {
        //             listIgnoreAsset.Add(assets[i]);
        //             assets[i] = null;
        //         }
        //     }

        //     return assets;
        // }

        void DrawAssetBundleList(string prefix, string assetbundleName, SelectFile target, ref List<string> ListRemoveAssetBundle, bool isDrawAll)
        {
            int loopCountTmp = target.ListAsset.Count < MaxShowAssetCount ? target.ListAsset.Count : MaxShowAssetCount;
            int indexTmp = 0;

            if (isDrawAll)
            {
                foreach (var key2 in target.ListAsset.Keys)
                {
                    var value2 = target.ListAsset[key2];
                    EditorGUILayout.LabelField(value2.Asset);

                    if (++indexTmp >= loopCountTmp)
                        break;
                }
            }

            if (target.ListAsset.Count > MaxShowAssetCount || !isDrawAll)
            {
                GUILayout.Label("total " + target.ListAsset.Count + " count asset ...");
            }
        }

        public void CheckAssetBundleValid(List<string> ListRemoveAssetBundle)
        {
            for (int i = 0; i < ListRemoveAssetBundle.Count; ++i)
            {
                var selectInfo = MapAssetbundlePath[ListRemoveAssetBundle[i]];
                foreach (var value in selectInfo.ListAsset.Values)
                {
                    _mapAllExportAsset.Remove(value.Asset);
                }

                MapAssetbundlePath.Remove(ListRemoveAssetBundle[i]);
            }

            if (MapAssetbundlePath.Count == 0)
                _strCurrentRootPath = string.Empty;
        }

        static private void DisplayDialogError(string message)
        {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }
    }
}
