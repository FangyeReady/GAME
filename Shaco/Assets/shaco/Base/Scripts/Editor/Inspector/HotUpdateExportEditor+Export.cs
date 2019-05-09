using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using shaco;
using System.Linq;

namespace shacoEditor
{
    public partial class HotUpdateExportEditor : EditorWindow
    {
        private void ExcuteExportProcess()
        {
            do
            {
                if (!EditorUtility.DisplayDialog("Export ?", string.Empty, "Confirm", "Cancel"))
                {
                    break;
                }

                //先保存一下场景，以防止某些场景或者prefab资源的修改没有保存到，而导出了旧的资源
#if UNITY_5_3_OR_NEWER
                AssetDatabase.SaveAssets();
#else
                EditorApplication.SaveAssets();
#endif

                //export Android & iOS assetbundle
                CollectionExportInfo();

                var fullPath = shaco.Base.FileHelper.ContactPath(_strCurrentRootPath, HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty));
                var platform = HotUpdateHelper.GetAssetBundleAutoPlatform();

                ExcuteBuildProcess(fullPath, _versionControlConfig, platform);

                OnClearButtonClick();

            } while (false);
        }

        //获取需要导出的保留文件，和默认导出的文件
        private void GetExportFiles(HotUpdateDefine.SerializeVersionControl versionControl, out Dictionary<string, SelectFile> keepFiles, out Dictionary<string, SelectFile> defaultFiles)
        {
            keepFiles = new Dictionary<string, SelectFile>();
            defaultFiles = new Dictionary<string, SelectFile>();
            var versionControlFullPath = _strCurrentRootPath.ContactPath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty));

            foreach (var iter in MapAssetbundlePath)
            {
                bool isKeepFile = false;
                foreach (var iterAssetInfo in iter.Value.ListAsset)
                {
                    var pathTmp = iterAssetInfo.Value.Asset.ToLower();
                    isKeepFile = IsKeepFileInEditorFolder(versionControl, pathTmp);
                    break;
                }

                if (isKeepFile)
                {
                    keepFiles.Add(iter.Key, iter.Value);
                }
                else
                {
                    //如果打包的assetbundle已经存在，则不加入默认打包列表中
                    var pathAssetbundleWithMD5Tmp = GetAssetBundleFullPath(versionControlFullPath, iter.Key, iter.Value.AssetBundleMD5);
                    if (!shaco.Base.FileHelper.ExistsFile(pathAssetbundleWithMD5Tmp))
                    {
                        defaultFiles.Add(iter.Key, iter.Value);
                    }
                }
            }
        }

        private void CollectionExportInfo()
        {
            _versionControlConfig = new HotUpdateDefine.SerializeVersionControl(_versionControlConfig);

            //清理旧assetbundle目录
            _versionControlConfig.ListAssetBundles.Clear();

            //替换导出的excel
            _replaceAssetInfos = GetExcelToTxtInfo(MapAssetbundlePath);

            //set version code
            _versionControlConfig.Version = GetResourceVersion();

            //set all assetbundle's files path and md5
            foreach (var key in MapAssetbundlePath.Keys)
            {
                var value = MapAssetbundlePath[key];

                HotUpdateDefine.ExportAssetBundle newExportAB = new HotUpdateDefine.ExportAssetBundle();
                newExportAB.AssetBundleName = HotUpdateHelper.AssetBundlePathToKey(key);

                //computer path
                foreach (var key2 in value.ListAsset.Keys)
                {
                    var value2 = value.ListAsset[key2];

                    var newFileInfo = new HotUpdateDefine.ExportAssetBundle.ExportFile();
                    var pathTmp = value2.Asset.ToLower();

                    newFileInfo.Key = pathTmp;

                    newExportAB.ListFiles.Add(newFileInfo);
                }

                if (newExportAB.ListFiles.Count > 0)
                    _versionControlConfig.ListAssetBundles.Add(newExportAB);
                else
                    Debug.LogWarning("Hotupdate editor warning: no asset in assetbundle=" + key);
            }
        }

        /// <summary>
        /// 打包热更新文件，其中包含原始文件和AssetBundle
        /// <param name="pathRoot">打包根目录</param>
        /// <param name="versionControl">打包的版本控制文件信息</param>
        /// <param name="platform">打包平台</param>
        /// </summary>
        private void BuildHotUpdateFiles(string pathRoot, HotUpdateDefine.SerializeVersionControl versionControl, HotUpdateDefine.Platform platform)
        {
            var keepFiles = new Dictionary<string, SelectFile>();
            var defaultFiles = new Dictionary<string, SelectFile>();

            //获取导出文件分类
            GetExportFiles(versionControl, out keepFiles, out defaultFiles);

            //开始导出文件
            BuildOriginalFiles(pathRoot, keepFiles, versionControl.AutoEncryt);
            BuildAssetBundles(pathRoot, defaultFiles, platform);
        }

        private void ExcuteBuildProcess(string pathRoot, HotUpdateDefine.SerializeVersionControl versionControl, HotUpdateDefine.Platform platform)
        {
            //初始化保留文件路径，这个必须放置在导出代码最前
            versionControl.ListKeepFileFolders = ObjectsToStrings(LocalConfigAsset.ListKeepFolder);

            //如果KeepFolder目录中存在Resources相对目录文件，则先拷贝Resources相对目录到Resources_HotUpdate目录中来
            // CopyResourcesToResourcesHotUpdate(versionControl.ListKeepFileFolders);

            //计算所有文件md5
            ComputerAssetbundleMD5(versionControl);

            //偶尔的在文件更新后，unity却没有识别到assetbundle变化而重新打包时，需要检查强制重新打包assetbundle
            CheckForceBuildAssetbundle(versionControl, pathRoot);

            //打包更新文件
            BuildHotUpdateFiles(pathRoot, versionControl, platform);

            //对aasetbundle用md5重命名
            RenameAssetbundleByMD5(pathRoot, versionControl);

            //对assetbundle加密，需要放在RenameAssetbundleByMD5后执行，否则会找不到assetbundle
            if (versionControl.AutoEncryt)
            {
                EncryptAssetbundles(pathRoot, versionControl);
            }

            //删除旧的不用资源
            CheckDeleteUnuseFiles(platform, versionControl);

            //计算所有文件大小
            ComputerAllDataSize(pathRoot.ContactPath("assets"), versionControl);

            //导出资源版本描述文件
            ExportVersionControl(platform, versionControl);

            //打包差异化文件，需要ExportVersionControl后执行，否则拷贝出来的文件还是旧的
            BuildModifyOnlyFilesAndEncrypt(platform);

            //因为现在版本keepfolder文件夹已经作为原始文件导出了，这2个方法暂时弃用吧
            // SaveRetainOriginalFile(HotUpdateHelper.GetVersionControlFolder(_strCurrentRootPath, platform), platform);
            // CheckKeepFolderFilesValid(platform);
        }
        
        //计算并设置单个文件和所有文件大小
        private void ComputerAllDataSize(string pathRoot, HotUpdateDefine.SerializeVersionControl versionControl)
        {
            var fullPathTmp = string.Empty;
            long totalDataSize = 0;

            for (int i = versionControl.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var exportInfoTmp = versionControl.ListAssetBundles[i];
                fullPathTmp = GetAssetBundleFullPath(pathRoot, exportInfoTmp);
                exportInfoTmp.fileSize = shaco.Base.FileHelper.GetFileSize(fullPathTmp);
                totalDataSize += exportInfoTmp.fileSize;
            }

            //设置需要下载的总资源文件大小
            versionControl.TotalDataSize = totalDataSize;
        }

        /// <summary>
        /// 通过文件夹名字判断是否为保留文件，这样比HotUpdateHelper.IsKeepFile通过文件内容判断更快
        /// <param name="versionControl">版本控制文件</param>
        /// <param name="path">文件路径</param>
        /// <return>是否为保留文件</return>
        /// </summary>
        private bool IsKeepFileInEditorFolder(HotUpdateDefine.SerializeVersionControl versionControl, string path)
        {
            int indexStart = path.IndexOf("assets");
            int indexEnd = path.LastIndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT);

            if (indexStart < 0)
            {
                return false;
            }

            var folderTmp = path.Substring(indexStart, indexEnd - indexStart);
            return versionControl.ListKeepFileFolders.Contains(folderTmp);
        }

        /// <summary>
        /// 计算所有文件的md5
        /// <param name="versionControl">版本控制文件</param>
        /// </summary>
        private void ComputerAssetbundleMD5(HotUpdateDefine.SerializeVersionControl versionControl)
        {
            foreach (var iter in MapAssetbundlePath)
            {
                //computer all files to one md5
                string strAllFilesMD5 = string.Empty;
                foreach (var key2 in iter.Value.ListAsset.Keys)
                {
                    var value2 = iter.Value.ListAsset[key2];
                    var pathTmp = value2.Asset.ToLower();
                    var readFileTmp = shaco.Base.FileHelper.ReadAllByteByUserPath(pathTmp);
                    if (readFileTmp == null)
                    {
                        DisplayDialogError("can't read string by user path=" + pathTmp);
                    }

                    //computer file md5
                    strAllFilesMD5 += shaco.Base.FileHelper.MD5FromByte(readFileTmp);

                    //computer meta md5
                    if (!IsIgnoreMetaExtensions(pathTmp))
                    {
                        var pathMetaTmp = shaco.Base.FileHelper.AddExtensions(pathTmp, HotUpdateDefine.EXTENSION_META);
                        pathMetaTmp = pathMetaTmp.Remove("assets");
                        pathMetaTmp = shaco.Base.FileHelper.ContactPath(Application.dataPath, pathMetaTmp);
                        strAllFilesMD5 += shaco.Base.FileHelper.MD5FromFile(pathMetaTmp);
                    }
                }

                iter.Value.AssetBundleMD5 = shaco.Base.FileHelper.MD5FromString(strAllFilesMD5);
            }

            for (int i = versionControl.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var assetBundleTmp = versionControl.ListAssetBundles[i];
                var pathReal = HotUpdateHelper.AssetBundleKeyToPath(assetBundleTmp.AssetBundleName);
                versionControl.ListAssetBundles[i].AssetBundleMD5 = MapAssetbundlePath[pathReal].AssetBundleMD5;
            }
        }

        private void ComputerMainMD5(HotUpdateDefine.SerializeVersionControl versionControl, string jsonConfig, HotUpdateDefine.Platform platform)
        {
            //computer main md5
            string allFileMD5 = string.Empty;
            int filesCount = versionControl.ListAssetBundles.Count;
            for (int i = 0; i < filesCount; ++i)
            {
                allFileMD5 += versionControl.ListAssetBundles[i].AssetBundleMD5;
            }
            string strMainMD5 = shaco.Base.FileHelper.MD5FromString(allFileMD5);

            //write main md5 file
            var pathMainMD5 = shaco.Base.FileHelper.ContactPath(_strCurrentRootPath, HotUpdateHelper.GetVersionControlFolder(HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + HotUpdateDefine.EXTENSION_MAIN_MD5, string.Empty, platform));
            shaco.Base.FileHelper.WriteAllByUserPath(pathMainMD5, strMainMD5);
        }

        private void ComputerManifestMD5(HotUpdateDefine.SerializeVersionControl versionControl, HotUpdateDefine.Platform platform)
        {
            var filenameManifest1 = HotUpdateHelper.AddFileTag(HotUpdateDefine.VERSION_CONTROL + ".tmp", platform);
            filenameManifest1 = shaco.Base.FileHelper.RemoveExtension(filenameManifest1);
            var pathMainManifest1 = shaco.Base.FileHelper.ContactPath(_strCurrentRootPath, HotUpdateHelper.GetVersionControlFolder(filenameManifest1, string.Empty, platform));
            var pathMainManifest2 = shaco.Base.FileHelper.AddExtensions(pathMainManifest1, HotUpdateDefine.EXTENSION_MANIFEST);
            var md5Tmp1 = shaco.Base.FileHelper.MD5FromFile(pathMainManifest1);
            var md5Tmp2 = shaco.Base.FileHelper.MD5FromFile(pathMainManifest2);
            var mainManifestMD5 = shaco.Base.FileHelper.MD5FromString(md5Tmp1 + md5Tmp2);
            versionControl.MD5Main = mainManifestMD5;
        }

        private bool IsIgnoreMetaExtensions(string path)
        {
            bool ret = false;
            if (string.IsNullOrEmpty(path))
                return ret;

            for (int i = 0; i < LocalConfigAsset.ListIgnoreMetaModel.Count; ++i)
            {
                if (LocalConfigAsset.ListIgnoreMetaModel[i] == null)
                    continue;
                var pathAssetTmp = EditorHelper.GetAssetPathLower(LocalConfigAsset.ListIgnoreMetaModel[i]);
                var extensionTmp1 = shaco.Base.FileHelper.GetFilNameExtension(path);
                var extensionTmp2 = shaco.Base.FileHelper.GetFilNameExtension(pathAssetTmp);
                if (extensionTmp1 == extensionTmp2)
                {
                    ret = true;
                    break;
                }
            }

            if (!ret)
            {
                ret = IsEncryptExcelPath(path);
            }
            return ret;
        }

        private void ExportVersionControl(HotUpdateDefine.Platform platform, HotUpdateDefine.SerializeVersionControl versionControl)
        {
            //computer all asssetbunlde md5
            if (versionControl.ListAssetBundles.Count != MapAssetbundlePath.Count)
            {
                DisplayDialogError("version assetbundle count != export assetbundle file count, please check export your code, export count=" + versionControl.ListAssetBundles.Count + " collect count=" + MapAssetbundlePath.Count);
                return;
            }

            CheckCurrentFolderPathValid();

            ComputerManifestMD5(versionControl, platform);

            var pathSaveVersion = HotUpdateHelper.GetVersionControlFilePath(_strCurrentRootPath, string.Empty);

            //set api function
            versionControl.MD5Main = _apiVersion.ToString();

            //export json - version control
            versionControl.FileTag = HotUpdateDefine.VERSION_CONTROL;
            versionControl.UnityVersion = Application.unityVersion;
            versionControl.ListAssetBundles.Sort((a, b) => { return a.AssetBundleMD5.CompareTo(b.AssetBundleMD5); });

            var strJson = shaco.LitJson.JsonMapper.ToJson(versionControl);
            ComputerMainMD5(versionControl, strJson, platform);

            shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(pathSaveVersion);
            shaco.Base.FileHelper.WriteAllByUserPath(pathSaveVersion, strJson);

            EditorPrefs.SetString(EditorHelper.GetEditorPrefsKey(HotUpdateVersionImportEditor.PREVIOUS_PATH_KEY), shaco.Base.FileHelper.GetFolderNameByPath(pathSaveVersion));

#if !UNITY_5_3_OR_NEWER
            //低版本Unity不会自动导出MainManifest相关文件，需要手动添加一个站位文件用，否则更新文件流程会出问题
            var pathVersionFolder = HotUpdateHelper.GetVersionControlFolder(_strCurrentRootPath, string.Empty, platform);
            var mainManifestAssetbundlePath = pathVersionFolder.ContactPath(shaco.Base.FileHelper.GetLastFileName(pathVersionFolder));
            var mainManifestPath = shaco.Base.FileHelper.AddExtensions(mainManifestAssetbundlePath, HotUpdateDefine.EXTENSION_MANIFEST);
            shaco.Base.FileHelper.WriteAllByUserPath(mainManifestAssetbundlePath, string.Empty);
            shaco.Base.FileHelper.WriteAllByUserPath(mainManifestPath, string.Empty);
#endif

            Debug.Log("Export AssetBundle Resource success !\n" + "Platform=" + platform + " Version=" + versionControl.Version);
        }

        private void CheckDeleteUnuseFiles(HotUpdateDefine.Platform platform, HotUpdateDefine.SerializeVersionControl versionControl)
        {
            if (_apiVersion == HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles)
            {
                var pathVersionFolder = HotUpdateHelper.GetVersionControlFolder(_strCurrentRootPath, string.Empty, platform);
                var strVersionFolder = shaco.Base.FileHelper.GetFolderNameByPath(pathVersionFolder, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
                HotUpdateHelper.DeleteUnUseFiles(strVersionFolder, versionControl);
            }

            //收集替换过的文件夹目录
            var replaceFolderPaths = new List<string>();
            for (int i = 0; i < _replaceAssetInfos.Count; ++i)
            {
                var folderPath = shaco.Base.FileHelper.GetFolderNameByPath(_replaceAssetInfos[i].excelSaveTxtPath);
                if (!replaceFolderPaths.Contains(folderPath))
                {
                    replaceFolderPaths.Add(folderPath);
                }
            }

            //删除目录下不匹配的旧替换文件
            for (int i = 0; i < replaceFolderPaths.Count; ++i)
            {
                var allFiles = new List<string>();
                shaco.Base.FileHelper.GetSeekPath(replaceFolderPaths[i], ref allFiles, false, HotUpdateDefine.EXTENSION_ENCRPYT);

                for (int j = 0; j < allFiles.Count; ++j)
                {
                    //移除加密的后缀名，获取真实名字，如果没有找到对应的文件，则被视为旧文件做删除处理
                    var realPath = allFiles[j].Remove(HotUpdateDefine.EXTENSION_ENCRPYT);

                    if (!shaco.Base.FileHelper.ExistsFile(realPath))
                    {
                        shaco.Base.FileHelper.DeleteByUserPath(allFiles[j]);
                    }
                }
            }
        }

        private void CheckCurrentFolderPathValid()
        {
            if (string.IsNullOrEmpty(_strCurrentRootPath))
            {
                foreach (var key in MapAssetbundlePath.Keys)
                {
                    _strCurrentRootPath = shaco.Base.FileHelper.GetFolderNameByPath(key, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
                    break;
                }
            }
            _strCurrentRootPath = shaco.Base.FileHelper.GetFolderNameByPath(_strCurrentRootPath, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            _strCurrentRootPath = shaco.Base.FileHelper.RemoveSubStringByFind(_strCurrentRootPath, HotUpdateDefine.SIGN_FLAG);
        }

        //单独打包出需要更新的文件夹
        private void BuildModifyOnlyFilesAndEncrypt(HotUpdateDefine.Platform platform)
        {
            if (_versionControlConfig == null || string.IsNullOrEmpty(_strCurrentRootPath))
                return;

            var prefixVersionControl = HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platform);
            var pathRootNow = shaco.Base.FileHelper.ContactPath(_strCurrentRootPath, prefixVersionControl);
            var pathRootCopy = pathRootNow + HotUpdateDefine.FILE_UPDATE_FLAG;
            var pathRootDelete = pathRootNow + HotUpdateDefine.FILE_DELETE_FLAG;

            var listModifyAssetBundle = new List<HotUpdateDefine.DownloadAssetBundle>();
            var listDeleteAssetBundle = new List<HotUpdateDefine.DownloadAssetBundle>();
            if (null != _versionControlConfigOld)
            {
                listModifyAssetBundle = HotUpdateHelper.SelectUpdateFiles(_strCurrentRootPath, string.Empty, _versionControlConfigOld, _versionControlConfig, true); ;
                listDeleteAssetBundle = HotUpdateHelper.SelectDeleteFiles(_versionControlConfigOld, _versionControlConfig);
            }

            //delete old version control folder 
            if (shaco.Base.FileHelper.ExistsDirectory(pathRootCopy))
                shaco.Base.FileHelper.DeleteByUserPath(pathRootCopy);
            if (shaco.Base.FileHelper.ExistsDirectory(pathRootDelete))
                shaco.Base.FileHelper.DeleteByUserPath(pathRootDelete);

            if (listModifyAssetBundle.Count > 0)
            {
                if (_versionControlConfig.AutoBuildModifyPackage)
                {
                    //copy version control manifest
                    if (!string.IsNullOrEmpty(prefixVersionControl) && prefixVersionControl[0].ToString() == shaco.Base.FileDefine.PATH_FLAG_SPLIT)
                    {
                        prefixVersionControl = prefixVersionControl.Remove(0, 1);
                    }
                    if (!string.IsNullOrEmpty(prefixVersionControl) && prefixVersionControl[prefixVersionControl.Length - 1].ToString() == shaco.Base.FileDefine.PATH_FLAG_SPLIT)
                    {
                        prefixVersionControl = prefixVersionControl.Remove(prefixVersionControl.Length - 1, 1);
                    }
                    var pathManifestNow = shaco.Base.FileHelper.ContactPath(pathRootNow, prefixVersionControl);
                    var pathManifestCopy = shaco.Base.FileHelper.ContactPath(pathRootCopy, prefixVersionControl);
                    shaco.Base.FileHelper.CopyFileByUserPath(pathManifestNow, pathManifestCopy);
                    shaco.Base.FileHelper.CopyFileByUserPath(shaco.Base.FileHelper.AddExtensions(pathManifestNow, HotUpdateDefine.EXTENSION_MANIFEST), shaco.Base.FileHelper.AddExtensions(pathManifestCopy, HotUpdateDefine.EXTENSION_MANIFEST));

                    //copy version control json
                    var filenameVersion = shaco.Base.FileHelper.AddExtensions(HotUpdateDefine.VERSION_CONTROL, HotUpdateDefine.EXTENSION_VERSION_CONTROL);
                    filenameVersion = HotUpdateHelper.AddFileTag(filenameVersion, platform);
                    var pathVersionJsonNow = shaco.Base.FileHelper.ContactPath(pathRootNow, filenameVersion);
                    var pathVersionJsonCopy = shaco.Base.FileHelper.ContactPath(pathRootCopy, filenameVersion);
                    shaco.Base.FileHelper.CopyFileByUserPath(pathVersionJsonNow, pathVersionJsonCopy);

                    //copy main md5 file
                    var filenameMainMD5 = HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + HotUpdateDefine.EXTENSION_MAIN_MD5;
                    var pathMainMD5Now = shaco.Base.FileHelper.ContactPath(pathRootNow, filenameMainMD5);
                    var pathMainMD5Copy = shaco.Base.FileHelper.ContactPath(pathRootCopy, filenameMainMD5);
                    shaco.Base.FileHelper.CopyFileByUserPath(pathMainMD5Now, pathMainMD5Copy);
                }

                for (int i = 0; i < listModifyAssetBundle.Count; ++i)
                {
                    var pathAssetBundleNowWithMD5 = GetAssetBundleFullPath(pathRootNow, listModifyAssetBundle[i].ExportInfo);
                    var pathAssetBundleCopyWithMD5 = GetAssetBundleFullPath(pathRootCopy, listModifyAssetBundle[i].ExportInfo);

                    //encrypt assetbundle
                    if (shaco.Base.FileHelper.ExistsFile(pathAssetBundleNowWithMD5) && !IsKeepFileInEditorFolder(_versionControlConfig, pathAssetBundleNowWithMD5))
                    {
                        var secretRandomCode = shaco.Base.Utility.Random(int.MinValue, int.MaxValue);
                        shaco.Base.EncryptDecrypt.Encrypt(pathAssetBundleNowWithMD5, 0.314f, secretRandomCode);
                    }

                    if (_versionControlConfig.AutoBuildModifyPackage)
                    {
                        //copy assetbundle 
                        if (!shaco.Base.FileHelper.ExistsFile(pathAssetBundleNowWithMD5))
                        {
                            Debug.LogError("build modify only files error: not find assetbundle file by path=" + pathAssetBundleNowWithMD5);
                            continue;
                        }
                        shaco.Base.FileHelper.CopyFileByUserPath(pathAssetBundleNowWithMD5, pathAssetBundleCopyWithMD5);
                    }

                    //copy assetbundle manifest
                    // var pathAssetBundleNow = shaco.Base.FileHelper.ContactPath(pathRootNow, filenameAssetBundle);
                    // var pathAssetBundleCopy = shaco.Base.FileHelper.ContactPath(pathRootCopy, filenameAssetBundle);
                    // var pathAssetbundleManifestNow = shaco.Base.FileHelper.AddExtensions(pathAssetBundleNow, HotUpdateDefine.EXTENSION_MANIFEST);
                    // var pathAssetbundleManifestCopy = shaco.Base.FileHelper.AddExtensions(pathAssetBundleCopy, HotUpdateDefine.EXTENSION_MANIFEST);
                    // shaco.Base.FileHelper.CopyFileByUserPath(pathAssetbundleManifestNow, pathAssetbundleManifestCopy);
                }
            }

            //构建excel转换后的txt文件和自动生成的脚本
            for (int i = 0; i < _replaceAssetInfos.Count; ++i)
            {
                var exportExcelTxtPath = _replaceAssetInfos[i].excelSaveTxtPath;

                //自动化导出excel脚本
                if (null != _replaceAssetInfos[i].excelData)
                {
                    //获取加密的assetbundle key
                    var assetKeyPath = EditorHelper.FullPathToUnityAssetPath(exportExcelTxtPath);
                    assetKeyPath = assetKeyPath.Remove(HotUpdateDefine.EXTENSION_ENCRPYT);
                    assetKeyPath = shaco.Base.FileHelper.ReplaceExtension(assetKeyPath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                    assetKeyPath = HotUpdateHelper.AssetBundlePathToKey(assetKeyPath);
                    assetKeyPath = assetKeyPath.ToLower();

                    //获取导出脚本的绝对路径
                    var filename = shaco.Base.FileHelper.GetLastFileName(_replaceAssetInfos[i].excelSaveTxtPath);
                    filename = filename.Remove(HotUpdateDefine.EXTENSION_ENCRPYT);
                    filename = shaco.Base.FileHelper.ReplaceExtension(filename, "cs");
                    var exportScriptPathTmp = shaco.Base.FileHelper.ContactPath(shaco.Base.GlobalParams.GetShacoFrameworkRootPath().RemoveLastPathByLevel(1), "ExcelDataGeneratorScripts/" + filename);

                    //如果导出代码不存在，或者在修改ab中有找到(说明导出文件有修改)，则重新构建脚本
                    if (!shaco.Base.FileHelper.ExistsFile(exportScriptPathTmp) || listModifyAssetBundle.Any(ab => ab.ExportInfo.AssetBundleName == assetKeyPath))
                    {
                        _replaceAssetInfos[i].excelData.SerializableAsCSharpScript(exportScriptPathTmp);
                    }
                }
            }

            if (_versionControlConfig.AutoBuildModifyPackage)
            {
                if (listDeleteAssetBundle.Count > 0)
                {
                    for (int i = 0; i < listDeleteAssetBundle.Count; ++i)
                    {
                        //write delete assetbundle 
                        var pathAssetBundleCopy = GetAssetBundleFullPath(pathRootDelete, listDeleteAssetBundle[i].ExportInfo);
                        var pathAssetBundleNow = GetAssetBundleFullPath(pathRootNow, listDeleteAssetBundle[i].ExportInfo);
                        shaco.Base.FileHelper.WriteAllByUserPath(pathAssetBundleCopy, string.Empty);
                        HotUpdateHelper.DeleteManifest(pathAssetBundleNow);
                    }
                }
            }
        }

        private void LogErrorNoAssetExport(string assetBundleName, List<SelectFile.FileInfo> listIgnoreeAsset)
        {
            bool isFindIgnore = false;
            for (int i = 0; i < listIgnoreeAsset.Count; ++i)
            {
                if (listIgnoreeAsset[i] != null)
                {
                    isFindIgnore = true;
                    break;
                }
            }
            if (!isFindIgnore)
                return;

            var errorMsg = "AssetBundle '" + assetBundleName + "' error: no asset can be export ! please check ignore extensions ";
            for (int i = 0; i < IGNORE_RESOURCE_EXTENSIONS.Length; ++i)
            {
                errorMsg += "[" + IGNORE_RESOURCE_EXTENSIONS[i] + "]";
            }
            Debug.LogWarning(errorMsg);

            for (int i = 0; i < listIgnoreeAsset.Count; ++i)
            {
                if (listIgnoreeAsset[i] != null)
                    Debug.LogWarning("ignore asset=" + listIgnoreeAsset[i].Asset);
            }
        }

        //用md5码对所有资源文件重命名
        private void RenameAssetbundleByMD5(string pathRoot, HotUpdateDefine.SerializeVersionControl versionControl)
        {
            foreach (var key in MapAssetbundlePath.Keys)
            {
                var value = MapAssetbundlePath[key];
                var pathSource = shaco.Base.FileHelper.ContactPath(pathRoot, key);

                if (!shaco.Base.FileHelper.ExistsFile(pathSource))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(value.AssetBundleMD5))
                {
                    DisplayDialogError("rename error: md5 is empty !");
                    continue;
                }

                var pathDest = HotUpdateHelper.AddAssetBundleNameTag(pathSource, value.AssetBundleMD5);
                bool isKeepOriginalFileSetting = IsKeepFileInEditorFolder(versionControl, pathSource);
                bool isExistsDestinationFile = shaco.Base.FileHelper.ExistsFile(pathDest);

                //如果目标文件已经存在则先删除目标文件，以防止可能的文件md5与文件本身不对应导致的资源错乱
                if (isExistsDestinationFile)
                {
                    //是否为原始文件的标记不一致的时候，也要删除原始文件
                    bool isKeepOriginalFileFile = HotUpdateHelper.IsKeepFile(pathDest);
                    if (isKeepOriginalFileFile != isKeepOriginalFileSetting)
                    {
                        shaco.Base.FileHelper.DeleteByUserPath(pathDest);
                        isExistsDestinationFile = false;
                    }
                }

                //强制原文件打包或者当目标文件不存在的时候，拷贝文件
                if (!isExistsDestinationFile)
                {
                    shaco.Base.FileHelper.MoveFileByUserPath(pathSource, pathDest);
                }
                //已有打包文件，则删除临时文件
                else
                {
                    shaco.Base.FileHelper.DeleteByUserPath(pathSource);
                }
            }
        }

        private void EncryptAssetbundles(string pathRoot, HotUpdateDefine.SerializeVersionControl versionControl)
        {
            foreach (var iter in MapAssetbundlePath)
            {
                var value = MapAssetbundlePath[iter.Key];
                var pathSource = shaco.Base.FileHelper.ContactPath(pathRoot, iter.Key);
                var pathSourceWithMD5 = HotUpdateHelper.AddAssetBundleNameTag(pathSource, iter.Value.AssetBundleMD5);

                var secretRandomCode = shaco.Base.Utility.Random(int.MinValue, int.MaxValue);

                if (!shaco.Base.EncryptDecrypt.IsEncryption(pathSourceWithMD5))
                {
                    shaco.Base.EncryptDecrypt.Encrypt(pathSourceWithMD5, 0.314f, secretRandomCode);
                }
            }
        }

        private string GetResourceVersion()
        {
            if (versionCodes.IsNullOrEmpty())
            {
#if UNITY_5_3_OR_NEWER
                return Application.version;
#else
                return PlayerSettings.bundleVersion;
#endif
            }
            string retValue = string.Empty;
            for (int i = 0; i < versionCodes.Count; ++i)
            {
                retValue += versionCodes[i] + shaco.Base.FileDefine.DOT_SPLIT;
            }
            return retValue.Remove(retValue.Length - shaco.Base.FileDefine.DOT_SPLIT.Length);
        }

        /// <summary>
        /// assetbundle are sometimes modified, but unity does not detect changes,
        /// there is no automatic build, and we need to delete the manifest file and force unity to generate new assetbundle
        /// </summary>
        /// <returns> if return true, we need call 'BuildAssetBundles' again </returns>
        /// <param name="versionControl"> version control file </param>
        private bool CheckForceBuildAssetbundle(HotUpdateDefine.SerializeVersionControl versionControl, string pathRoot)
        {
            bool needRebuild = false;

            for (int i = 0; i < versionControl.ListAssetBundles.Count; ++i)
            {
                var assetbundleExportInfo = versionControl.ListAssetBundles[i];
                var pathAssetbundleTmp = shaco.Base.FileHelper.ContactPath(pathRoot, HotUpdateHelper.AssetBundleKeyToPath(assetbundleExportInfo.AssetBundleName));
                var pathAssetbundleWithMD5Tmp = HotUpdateHelper.AddAssetBundleNameTag(pathAssetbundleTmp, assetbundleExportInfo.AssetBundleMD5);
                bool isMissingFile = false;

                if (!IsKeepFileInEditorFolder(versionControl, pathAssetbundleTmp))
                {
                    if (!shaco.Base.FileHelper.ExistsFile(pathAssetbundleWithMD5Tmp) && !shaco.Base.FileHelper.ExistsFile(pathAssetbundleTmp))
                    {
                        isMissingFile = true;
                        Debug.LogWarning("check force build assetbundle warning: missing assetbundle with md5 path=" + pathAssetbundleWithMD5Tmp);
                    }
                }

                //文件丢失的时候强制重新打包
                if (isMissingFile)
                {
                    HotUpdateHelper.DeleteManifest(pathAssetbundleTmp);
                    needRebuild = true;
                }
                //加密规则改变的时候强制重新打包
                else if (shaco.Base.FileHelper.ExistsFile(pathAssetbundleWithMD5Tmp))
                {
                    //如果是保留原始文件，它本身也做了加密头，所以它的是否加密标记用isKeepFile来代替了
                    bool isKeepFile = HotUpdateHelper.IsKeepFile(pathAssetbundleWithMD5Tmp);
                    bool isEncryptFile = isKeepFile ? shaco.Base.EncryptDecrypt.GetJumpSpeed(pathAssetbundleWithMD5Tmp) != 1.0f : shaco.Base.EncryptDecrypt.IsEncryption(pathAssetbundleWithMD5Tmp);
                    if (isEncryptFile != versionControl.AutoEncryt)
                    {
                        HotUpdateHelper.DeleteManifest(pathAssetbundleTmp);
                        shaco.Base.FileHelper.DeleteByUserPath(pathAssetbundleWithMD5Tmp);
                        needRebuild = true;
                    }
                }
            }

            return needRebuild;
        }
    }
}