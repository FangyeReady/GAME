using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;
using shaco;

namespace shacoEditor
{
    public class HotUpdateVersionImportEditor
    {
        static public readonly string PREVIOUS_PATH_KEY = "PreviousConfigImportPath";

        public static void GetConfigByVersionControl(ref HotUpdateExportEditor windowExport)
        {

            var pathFolder = EditorUtility.OpenFolderPanel("Select a version control folder", string.Empty, string.Empty);

            if (string.IsNullOrEmpty(pathFolder))
            {
                return;
            }
            else
            {
                GetConfigByVersionControl(pathFolder, ref windowExport);
            }
        }

        public static void GetPreviousConfigByVersionControl(ref HotUpdateExportEditor windowExport)
        {
            var previousConfigImportPath = EditorPrefs.GetString(EditorHelper.GetEditorPrefsKey(PREVIOUS_PATH_KEY), string.Empty);
            if (string.IsNullOrEmpty(previousConfigImportPath))
            {
                GetConfigByVersionControl(ref windowExport);
            }
            else
            {
                GetConfigByVersionControl(previousConfigImportPath, ref windowExport);
            }
        }

        public static void GetConfigByVersionControl(string pathFolder, ref HotUpdateExportEditor windowExport)
        {
            var pathTmp = string.Empty;

            //将路径转换为当前平台使用路径
            pathFolder = shaco.Base.FileHelper.RemoveSubStringByFind(pathFolder, shaco.HotUpdateDefine.SIGN_FLAG);
            pathFolder = pathFolder.ContactPath(shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty));

            if (!FileHelper.ExistsDirectory(pathFolder))
            {
                EditorPrefs.DeleteKey(EditorHelper.GetEditorPrefsKey(PREVIOUS_PATH_KEY));
                shaco.Log.Error("previous save path not found=" + pathFolder);
                return;
            }
            var allFiles = System.IO.Directory.GetFiles(pathFolder);
            for (int i = 0; i < allFiles.Length; ++i)
            {
                if (allFiles[i].EndsWith(HotUpdateDefine.EXTENSION_VERSION_CONTROL) && allFiles[i].Contains(HotUpdateDefine.VERSION_CONTROL))
                {
                    pathTmp = allFiles[i];
                    break;
                }
            }

            if (string.IsNullOrEmpty(pathTmp))
            {
                EditorUtility.DisplayDialog("Error", "Not find version control config json file", "OK");
                return;
            }

            var strVersionRead = FileHelper.ReadAllByUserPath(pathTmp);
            windowExport._versionControlConfig = HotUpdateHelper.JsonToVersionControl(strVersionRead);
            windowExport._versionControlConfigOld = windowExport._versionControlConfig;

            if (windowExport._versionControlConfig == null)
            {
                Debug.LogError("select version control file path=" + pathTmp);
                EditorUtility.DisplayDialog("Error", "Not find version control file, please check your unity version or path is valid !", "OK");
                return;
            }

            if (windowExport._versionControlConfig.FileTag != HotUpdateDefine.VERSION_CONTROL)
            {
                EditorUtility.DisplayDialog("Error", "Not version control file", "OK");
                return;
            }
            if (windowExport._versionControlConfig.UnityVersion != Application.unityVersion)
            {
                EditorUtility.DisplayDialog("Warning", "opened a different version of the Unity version control file", "OK");
            }

            var platformTarget = HotUpdateHelper.GetAssetBundlePlatformByPath(pathTmp);
            windowExport._strCurrentRootPath = FileHelper.GetFolderNameByPath(pathTmp, FileDefine.PATH_FLAG_SPLIT);
            windowExport._strCurrentRootPath = HotUpdateHelper.ResetAsssetBundleFileName(windowExport._strCurrentRootPath, string.Empty, platformTarget);

            //set main md5 and api
            if (windowExport._versionControlConfig.MD5Main == HotUpdateDefine.ExportFileAPI.DeleteFile.ToString())
            {
                windowExport._apiVersion = HotUpdateDefine.ExportFileAPI.DeleteFile;
            }
            else if (windowExport._versionControlConfig.MD5Main == HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles.ToString())
                windowExport._apiVersion = HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles;
            else
                windowExport._apiVersion = HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles;

            //set local config(editor only)
            windowExport.LocalConfigAsset.Clear();

            //set keep file folders
            for (int i = 0; i < windowExport._versionControlConfig.ListKeepFileFolders.Count; ++i)
            {
                var loadTmp = AssetDatabase.LoadAssetAtPath(windowExport._versionControlConfig.ListKeepFileFolders[i], typeof(Object));
                if (loadTmp != null && !windowExport.LocalConfigAsset.ListKeepFolder.Contains(loadTmp))
                {
                    windowExport.LocalConfigAsset.ListKeepFolder.Add(loadTmp);
                }
            }

            //set ignore meta extensions
            for (int i = 0; i < windowExport._versionControlConfig.ListIgnoreMetaExtensions.Count; ++i)
            {
                var loadTmp = AssetDatabase.LoadAssetAtPath(windowExport._versionControlConfig.ListIgnoreMetaExtensions[i], typeof(Object));
                if (loadTmp != null)
                {
                    windowExport.LocalConfigAsset.ListIgnoreMetaModel.Add(loadTmp);
                }
            }

            //set all assetbundle
            for (int i = windowExport._versionControlConfig.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var assetBundleTmp = windowExport._versionControlConfig.ListAssetBundles[i];

                if (windowExport.MapAssetbundlePath.ContainsKey(assetBundleTmp.AssetBundleName))
                    continue;

                var selectFiles = new HotUpdateExportEditor.SelectFile();
                selectFiles.AssetBundleMD5 = assetBundleTmp.AssetBundleMD5;

                string keyCheck = HotUpdateHelper.AssetBundleKeyToPath(assetBundleTmp.AssetBundleName);
                keyCheck = HotUpdateHelper.ResetAsssetBundleFileName(keyCheck, string.Empty, platformTarget);

                for (int j = 0; j < assetBundleTmp.ListFiles.Count; ++j)
                {
                    var fileName = assetBundleTmp.ListFiles[j];
                    var convertFileName = GetDecrptPath(fileName.Key);
                    var loadObj = convertFileName;

                    if (loadObj == null)
                    {
                        shaco.Log.Error("can't find Object by fileName=" + convertFileName + " in assetbundle name=" + keyCheck);
                        continue;
                    }

                    selectFiles.ListAsset.Add(convertFileName, new HotUpdateExportEditor.SelectFile.FileInfo(loadObj));

                    if (!windowExport._mapAllExportAsset.ContainsKey(loadObj))
                        windowExport._mapAllExportAsset.Add(loadObj, null);
                }

                if (selectFiles.ListAsset.Count > 0 && !windowExport.MapAssetbundlePath.ContainsKey(keyCheck))
                    windowExport.MapAssetbundlePath.Add(keyCheck, selectFiles);
            }

            EditorPrefs.SetString(EditorHelper.GetEditorPrefsKey(PREVIOUS_PATH_KEY), pathFolder);
            windowExport.UpdateDatas();
        }

        /// <summary>
        /// 获取解密后的路径，部分文件名字做了特殊处理
        /// <param name="path">加密后的路径</param>
        /// <return>真实路径</return>
        /// </summary>
        static private string GetDecrptPath(string path)
        {
            if (HotUpdateExportEditor.IsEncryptExcelPath(path))
            {
                path = path.Remove(shaco.HotUpdateDefine.EXTENSION_ENCRPYT);
            }
            return path;
        }
    }
}
