using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using shaco.Base;
using System.Linq;

namespace shaco
{
    public class HotUpdateHelper
    {
        // static readonly string UnityVersion = Application.unityVersion;

        //所有下载过的资源列表<资源版本列表, <AssetBundle本地不加密原始路径，AssetBundle对应的加密MD5🐎>>
        static private Dictionary<string, Dictionary<string, string>> _downloadedAllAssetbundleConfigs = new Dictionary<string, Dictionary<string, string>>();

        //动态网络资源下载地址
        //当参数不为空的时候，如果调用Async相关方法的时候，如果本地资源不存在，则会从服务器请求对应的资源，当服务器资源也没有的时候，才会返回null
        static private string[] _dynamicNetResourceAddress = null;

        //重置路径为原始路径，因为可能加入了平台识别的字符串
        static public string ResetAsssetBundleFileNameTag(string filename, string extension, HotUpdateDefine.Platform platform)
        {
            var pathTagOld = GetAssetBundlePathTagPlatform(platform);
            var fileTagOld = GetAssetBundleFileNameTagPlatform(platform);

            filename = filename.Remove(pathTagOld);
            filename = filename.Remove(fileTagOld);

            if (!string.IsNullOrEmpty(extension))
                filename = filename.Remove(extension);

            return filename;
        }

        static public string ResetAsssetBundleFileName(string filename, string extension, HotUpdateDefine.Platform platform)
        {
            filename = ResetAsssetBundleFileNameTag(filename, extension, platform);

            //remove file name md5 tag
            var lastFileNameTmp = FileHelper.GetLastFileName(filename, true);
            if (lastFileNameTmp.Contains(HotUpdateDefine.SIGN_FLAG))
            {
                var folderTmp = FileHelper.GetFolderNameByPath(filename);
                int indexBegin = lastFileNameTmp.IndexOf(HotUpdateDefine.SIGN_FLAG);
                int indexEnd = lastFileNameTmp.LastIndexOf(FileDefine.DOT_SPLIT);
                if (indexEnd < 0)
                {
                    Log.Error("HotUpdate ResetAsssetBundleFileName error: can't remove file name md5 tag, not find '" + FileDefine.DOT_SPLIT + "' in last file name=" + lastFileNameTmp);
                }
                else
                {
                    lastFileNameTmp = lastFileNameTmp.Remove(indexBegin, indexEnd - indexBegin);
                    filename = FileHelper.ContactPath(folderTmp, lastFileNameTmp);
                }
            }

            return filename;
        }

        static public string GetAssetBundlePathPlatform(string filename, string multiVersionControlRelativePath, HotUpdateDefine.Platform platform, string extension)
        {
            string ret = HotUpdateHelper.ResetAsssetBundleFileNameTag(filename, string.Empty, platform);

            //check extension
            if (!FileHelper.HasFileNameExtension(ret))
            {
                ret = FileHelper.AddExtensions(ret, extension);
            }

            //check path tag
            var pathTag = GetAssetBundlePathTagPlatform(platform);
            if (filename.Contains(FileDefine.PATH_FLAG_SPLIT))
            {
                //is folder
                ret = pathTag.ContactPath(ret);
            }
            else
            {
                //is file
                ret = FileHelper.AddFolderNameByPath(ret, pathTag, FileDefine.PATH_FLAG_SPLIT);
            }

            //convert to path
            ret = HotUpdateHelper.AssetBundleKeyToPath(ret);

            //add multy version control relative path
            if (!ret.Contains(multiVersionControlRelativePath.ContactPath(pathTag)))
            {
                ret = multiVersionControlRelativePath.ContactPath(ret);
            }

            ret = AssetBundleKeyToPath(ret);

            return ret;
        }

        static public string GetAssetBundlePathAutoPlatform(string filename, string multiVersionControlRelativePath, string extension)
        {
            var platform = GetAssetBundleAutoPlatform();
            return GetAssetBundlePathPlatform(filename, multiVersionControlRelativePath, platform, extension);
        }

        /// <summary>
        /// 获取版本配置文件目录
        /// </summary>
        /// <param name="filename">文件名字</param>
        /// <param name="platform">平台类型</param>
        static public string GetVersionControlFolder(string filename, string multiVersionControlRelativePath, HotUpdateDefine.Platform platform)
        {
            var pathTag = GetAssetBundlePathTagPlatform(platform);

            //is folder 
            if (filename.Contains(FileDefine.PATH_FLAG_SPLIT))
            {
                filename = filename.ContactPath(multiVersionControlRelativePath.ContactPath(pathTag));
            }
            //is file
            else
            {
                filename = multiVersionControlRelativePath.ContactPath(pathTag.ContactPath(filename));
            }

            filename = AssetBundleKeyToPath(filename);
            return filename;
        }

        static public string GetVersionControlFolderAuto(string filename, string multiVersionControlRelativePath)
        {
            return GetVersionControlFolder(filename, multiVersionControlRelativePath, GetAssetBundleAutoPlatform());
        }

        static public string GetVersionControlFilePath(string filename, string multiVersionControlRelativePath)
        {
            filename = HotUpdateHelper.GetVersionControlFolderAuto(filename, multiVersionControlRelativePath);
            filename = filename.ContactPath(HotUpdateDefine.VERSION_CONTROL);
            filename = FileHelper.AddExtensions(filename, HotUpdateDefine.EXTENSION_VERSION_CONTROL);
            filename = HotUpdateHelper.AddFileTagAutoPlatform(filename);
            return filename;
        }

        static public string GetAssetBundleManifestMemoryPath(string multiVersionControlRelativePath, HotUpdateDefine.Platform platform)
        {
            var strManifestPath = FileHelper.GetFullpath(HotUpdateHelper.GetVersionControlFolder(string.Empty, multiVersionControlRelativePath, platform));
            var pathTag = GetAssetBundlePathTagPlatform(platform);
            strManifestPath = FileHelper.ContactPath(strManifestPath, pathTag);
            strManifestPath += HotUpdateDefine.EXTENSION_MANIFEST;
            return strManifestPath;
        }

        static public string GetAssetBundleManifestEditorPath(string pathRoot, string multiVersionControlRelativePath, HotUpdateDefine.Platform platform)
        {
            var strManifestPath = FileHelper.ContactPath(pathRoot, HotUpdateHelper.GetVersionControlFolder(string.Empty, multiVersionControlRelativePath, platform));
            var pathTag = GetAssetBundlePathTagPlatform(platform);
            strManifestPath = FileHelper.ContactPath(strManifestPath, pathTag);
            strManifestPath += HotUpdateDefine.EXTENSION_MANIFEST;
            return strManifestPath;
        }

        static public string GetAssetBundleManifestMemoryPathAutoPlatform(string multiVersionControlRelativePath)
        {
            return GetAssetBundleManifestMemoryPath(multiVersionControlRelativePath, GetAssetBundleAutoPlatform());
        }

        static public string GetAssetBundleMainMD5MemoryPath(HotUpdateDefine.Platform platform, string multiVersionControlRelativePath)
        {
            var ret = HotUpdateHelper.GetVersionControlFolder(HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + HotUpdateDefine.EXTENSION_MAIN_MD5, multiVersionControlRelativePath, platform);
            ret = FileHelper.GetFullpath(ret);
            return ret;
        }

        static public string GetAssetBundleMainMD5MemoryPathAuto(string multiVersionControlRelativePath)
        {
            return GetAssetBundleMainMD5MemoryPath(GetAssetBundleAutoPlatform(), multiVersionControlRelativePath);
        }

        static public void DeleteAssetbundleConfigMainMD5(string multiVersionControlRelativePath)
        {
            var pathMainMD5Tmp = HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(multiVersionControlRelativePath);
            if (FileHelper.ExistsFile(pathMainMD5Tmp))
            {
                FileHelper.DeleteByUserPath(pathMainMD5Tmp);
            }
        }

        static public string GetAssetBundleFileNameTagPlatform(HotUpdateDefine.Platform platform)
        {
            string ret = string.Empty;
            switch (platform)
            {
                case HotUpdateDefine.Platform.Android: ret = HotUpdateDefine.FILENAME_TAG_ANDROID; break;
                case HotUpdateDefine.Platform.iOS: ret = HotUpdateDefine.FILENAME_TAG_IOS; break;
                default:
                    {
                        Log.Error("HotUpdateImport error: unsupport build platform");
                        break;
                    }
            }

            //In the later version of Unity5.3, Assetbundle is compatible with different versions
            // ret += HotUpdateDefine.FILENAME_TAG_UNITY_VERSION + UnityVersion;
            return ret;
        }

        static public string GetAssetBundlePathTagPlatform(HotUpdateDefine.Platform platform)
        {
            string ret = FileDefine.PATH_FLAG_SPLIT;
            switch (platform)
            {
                case HotUpdateDefine.Platform.Android: ret += HotUpdateDefine.PATH_TAG_ANDROID; break;
                case HotUpdateDefine.Platform.iOS: ret += HotUpdateDefine.PATH_TAG_IOS; break;
                default:
                    {
                        Log.Error("HotUpdateImport error: unsupport build platform");
                        break;
                    }
            }

            //In the later version of Unity5.3, Assetbundle is compatible with different versions
            // ret += HotUpdateDefine.PATH_TAG_UNITY_VERSION + UnityVersion;
            return ret;
        }

        static public HotUpdateDefine.Platform GetAssetBundleAutoPlatform()
        {
            HotUpdateDefine.Platform platform = HotUpdateDefine.Platform.None;

#if UNITY_ANDROID
            platform = HotUpdateDefine.Platform.Android;
#elif UNITY_IPHONE
            platform = HotUpdateDefine.Platform.iOS;
#else
			platform = HotUpdateDefine.Platform.Android;
			Log.Error("GetAssetBundleAutoPlatform error: unsupport platform! default set it as android platform");
#endif
            return platform;
        }

        static public HotUpdateDefine.Platform GetAssetBundlePlatformByPath(string path)
        {
            HotUpdateDefine.Platform ret = HotUpdateDefine.Platform.None;
            if (path.Contains(HotUpdateDefine.PATH_TAG_ANDROID) || path.Contains(HotUpdateDefine.FILENAME_TAG_ANDROID))
                ret = HotUpdateDefine.Platform.Android;
            else if (path.Contains(HotUpdateDefine.PATH_TAG_IOS) || path.Contains(HotUpdateDefine.FILENAME_TAG_IOS))
                ret = HotUpdateDefine.Platform.iOS;
            else
            {
                Debug.LogError("HotUpdateHelper GetAssetBundlePlatformByPath error: not find support platform by path=" + path);
            }

            return ret;
        }

        static public string AssetBundlePathToKey(string assetBundleName)
        {
            if (!string.IsNullOrEmpty(assetBundleName) && assetBundleName[0].ToString() == FileDefine.PATH_FLAG_SPLIT)
            {
                assetBundleName = assetBundleName.Substring(1);
            }
            return assetBundleName.Replace(FileDefine.PATH_FLAG_SPLIT, HotUpdateDefine.PATH_RELATIVE_FLAG);
        }

        static public string AssetBundleKeyToPath(string assetbundleKey)
        {
            return assetbundleKey = assetbundleKey.Replace(HotUpdateDefine.PATH_RELATIVE_FLAG, FileDefine.PATH_FLAG_SPLIT);
        }

        //执行版本描述文件(.version)中的APi
        static public bool ExcuteVersionControlAPI(string api, string multiVersionControlRelativePath, string pathParam, HotUpdateDefine.SerializeVersionControl versionServer)
        {
            if (api == HotUpdateDefine.ExportFileAPI.DeleteFile.ToString())
            {
                if (!FileHelper.ExistsFile(pathParam) && !FileHelper.ExistsDirectory(pathParam))
                {
                    Log.Error("ExcuteVersionControlAPI error: not find assetbundle file by path=" + pathParam);
                }
                else
                {
                    FileHelper.DeleteByUserPath(pathParam);
                }
            }
            else if (api == HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles.ToString())
            {
                string pathRoot = FileHelper.GetFullpath(string.Empty);
                string versionCurrentFolder = FileHelper.GetFullpath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath));
                if (!string.IsNullOrEmpty(versionCurrentFolder) && versionCurrentFolder[versionCurrentFolder.Length - 1].ToString() == FileDefine.PATH_FLAG_SPLIT)
                    versionCurrentFolder = versionCurrentFolder.Remove(versionCurrentFolder.Length - 1);

                //delete other version files
                List<string> listPath = new List<string>();

                string[] pathFolder = FileHelper.GetDirectories(pathRoot);
                for (int i = 0; i < pathFolder.Length; ++i)
                {
                    if (pathFolder[i].Contains(HotUpdateDefine.VERSION_CONTROL))
                    {
                        listPath.Add(pathFolder[i]);
                    }
                }

                for (int i = 0; i < listPath.Count; ++i)
                {
                    var folder = listPath[i];
                    if (folder != versionCurrentFolder)
                    {
                        FileHelper.DeleteByUserPath(folder);
                    }
                }

                //delete current version unuse files
                DeleteUnUseFiles(pathParam, versionServer);
                return false;
            }
            else
                return false;

            return true;
        }

        //删除当前文件夹中不用的文件
        static public void DeleteUnUseFiles(string pathVersionFolder, HotUpdateDefine.SerializeVersionControl versionConfig)
        {
            if (!string.IsNullOrEmpty(pathVersionFolder) && pathVersionFolder[pathVersionFolder.Length - 1].ToString() != FileDefine.PATH_FLAG_SPLIT)
            {
                pathVersionFolder += FileDefine.PATH_FLAG_SPLIT;
            }

            var listPath = new List<string>();
            FileHelper.GetSeekPath(pathVersionFolder, ref listPath);
            var platformTarget = HotUpdateHelper.GetAssetBundlePlatformByPath(pathVersionFolder);

            //set search map
            Dictionary<string, object> mapAssetbundlePath = new Dictionary<string, object>();
            Dictionary<string, object> mapAssetbundlePathWithMD5 = new Dictionary<string, object>();

            for (int j = 0; j < versionConfig.ListAssetBundles.Count; ++j)
            {
                var pathTmp = HotUpdateHelper.AssetBundleKeyToPath(versionConfig.ListAssetBundles[j].AssetBundleName);
                var pathWithMD5 = HotUpdateHelper.AddAssetBundleNameTag(pathTmp, versionConfig.ListAssetBundles[j].AssetBundleMD5);

                mapAssetbundlePath.Add(pathTmp, null);
                mapAssetbundlePathWithMD5.Add(pathWithMD5, null);
            }

            for (int i = 0; i < listPath.Count; ++i)
            {
                var pathTmp = listPath[i].Remove(pathVersionFolder);

                //ignore current version control file
                if (pathTmp.Contains(HotUpdateDefine.VERSION_CONTROL))
                    continue;

                bool isFindKey = false;

                if (pathTmp.Contains(HotUpdateDefine.EXTENSION_MANIFEST))
                {
                    var findKey = pathTmp.Remove(HotUpdateDefine.EXTENSION_MANIFEST);
                    findKey = HotUpdateHelper.ResetAsssetBundleFileName(findKey, string.Empty, platformTarget);
                    isFindKey = mapAssetbundlePath.ContainsKey(findKey);
                }
                else
                {
                    isFindKey = mapAssetbundlePathWithMD5.ContainsKey(pathTmp) || mapAssetbundlePath.ContainsKey(pathTmp);
                }

                if (!isFindKey)
                {
                    var pathDeleteTmp = listPath[i];
                    FileHelper.DeleteByUserPath(pathDeleteTmp);

                    if (pathTmp.Contains(HotUpdateDefine.EXTENSION_ASSETBUNDLE))
                    {
                        if (!mapAssetbundlePath.ContainsKey(HotUpdateHelper.ResetAsssetBundleFileName(pathTmp, string.Empty, platformTarget)))
                        {
                            DeleteManifest(pathDeleteTmp);
                        }
                    }

                    FileHelper.DeleteEmptyFolder(pathDeleteTmp);
                }
            }
        }

        static public void DeleteManifest(string pathAssetbundle)
        {
            //delete manifest 
            var filenameTmp = FileHelper.GetLastFileName(pathAssetbundle, true);
            var folderTmp = FileHelper.GetFolderNameByPath(pathAssetbundle);

            filenameTmp = HotUpdateHelper.RemoveAssetBundleNameTag(filenameTmp);

            filenameTmp = FileHelper.RemoveSubStringByFind(filenameTmp, HotUpdateDefine.SIGN_FLAG);
            filenameTmp = FileHelper.AddExtensions(filenameTmp, HotUpdateDefine.EXTENSION_MANIFEST);
            var pathManifest = FileHelper.ContactPath(folderTmp, filenameTmp);

            if (FileHelper.ExistsFile(pathManifest))
            {
                FileHelper.DeleteByUserPath(pathManifest);
            }
        }

        //添加文件路径的后缀，用于平台区分
        static public string AddFileTag(string path, string stringTag)
        {
            return shaco.Base.FileHelper.AddFileNameTag(path, stringTag);
        }

        static public string AddFileTag(string path, HotUpdateDefine.Platform platform)
        {
            return AddFileTag(path, HotUpdateHelper.GetAssetBundleFileNameTagPlatform(platform));
        }

        static public string AddFileTagAutoPlatform(string path)
        {
            return AddFileTag(path, HotUpdateHelper.GetAssetBundleAutoPlatform());
        }

        // static public string AddFolderTag(string path, string tag)
        // {
        //     var ret = path;
        //     int findIndex = -1;
        //     if (FileHelper.ExistsFile(path))
        //         findIndex = ret.LastIndexOf(FileDefine.DOT_SPLIT);
        //     else if (FileHelper.ExistsDirectory(path))
        //         findIndex = ret.LastIndexOf(FileDefine.PATH_FLAG_SPLIT);
        //     else 
        //     {
        //         Log.Error("HotUpdate AddFolderTag error: invaid path=" + path);
        //         return path;
        //     }

        //     if (!ret.Contains(tag))
        //         ret = ret.Insert(findIndex, tag);

        //     return ret;
        // }

        static public string AddAssetBundleNameTag(string assetbundleName, string assetbundleMD5)
        {
            string folder = FileHelper.GetFolderNameByPath(assetbundleName);
            string filename = FileHelper.GetLastFileName(assetbundleName, true);
            if (!filename.Contains(HotUpdateDefine.SIGN_FLAG))
                filename = AddFileTag(filename, HotUpdateDefine.SIGN_FLAG + assetbundleMD5.ToLower());
            return FileHelper.ContactPath(folder, filename);
        }

        static public string RemoveAssetBundleNameTag(string assetbundleName)
        {
            string ret = assetbundleName;
            int indexFindStart = ret.LastIndexOf(HotUpdateDefine.SIGN_FLAG);
            if (indexFindStart >= 0)
            {
                int indexFindEnd = ret.LastIndexOf(FileDefine.DOT_SPLIT);
                if (indexFindEnd < 0)
                {
                    Log.Error("HotUpdate RemoveAssetBundleNameTag error: not find '" + FileDefine.DOT_SPLIT + "' in assetbundleName=" + assetbundleName);
                }
                else
                {
                    ret = ret.Remove(indexFindStart, indexFindEnd - indexFindStart);
                }
            }
            return ret;
        }

        /// <summary>
        /// 将纯文本对象转换为字符串
        /// </summary>
        /// <param name="asset">Asset.</param> 纯文本对象
        static public string AssetToString(Object asset)
        {
            var readObject = asset as TextAsset;
            if (readObject == null)
            {
                var readObject2 = asset as TextOrigin;
                if (readObject2.IsNull())
                    return string.Empty;
                else
                    return readObject2.text;
            }
            return readObject.text;
        }

        static public byte[] AssetToByte(Object asset)
        {
            byte[] ret = null;
            var readText = asset as TextAsset;
            if (readText == null)
            {
                var readText2 = asset as TextOrigin;
                if (readText2.IsNull())
                {
                    var readTexture = asset as Texture2D;
                    if (readTexture != null)
                    {
                        ret = readTexture.EncodeToPNG();
                    }
                }
                else
                    return readText2.bytes;
            }
            else
                ret = readText.bytes;

            return ret;
        }

        static public List<HotUpdateDefine.DownloadAssetBundle> SelectUpdateFiles(
            string pathRoot,
            string multiVersionControlRelativePath,
            HotUpdateDefine.SerializeVersionControl versionOld,
            HotUpdateDefine.SerializeVersionControl versionNew,
        bool isEditor)
        {
            List<HotUpdateDefine.DownloadAssetBundle> listUpdateAssetBundle = new List<HotUpdateDefine.DownloadAssetBundle>();

            for (int i = 0; i < versionNew.ListAssetBundles.Count; ++i)
            {
                //check client assetbunle is valid
                var bundleServer = versionNew.ListAssetBundles[i];
                var bundleClient = FindExportBundle(versionOld.ListAssetBundles, bundleServer.AssetBundleName, true);

                var assetBundleNameCovert = bundleClient.AssetBundleName;
                if (isEditor)
                {
                    assetBundleNameCovert = HotUpdateHelper.AddAssetBundleNameTag(assetBundleNameCovert, bundleClient.AssetBundleMD5);
                }

                var fullPathTmp = FileHelper.ContactPath(pathRoot, HotUpdateHelper.GetAssetBundlePathAutoPlatform(assetBundleNameCovert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE));

                //Comparison client file md5 and server file md5
                if (!FileHelper.ExistsFile(fullPathTmp) || bundleClient.AssetBundleMD5 != bundleServer.AssetBundleMD5)
                {
                    //SceneManager.Log ("md5(client)=" + bundleClient.AssetBundleMD5 + "\nmd5(server)=" + bundleServer.AssetBundleMD5);

                    bundleClient.AssetBundleMD5 = bundleServer.AssetBundleMD5;
                    bundleClient.ListFiles = bundleServer.ListFiles;

                    var newUpdateInfo = new HotUpdateDefine.DownloadAssetBundle(bundleClient);
                    listUpdateAssetBundle.Add(newUpdateInfo);
                }
            }

            return listUpdateAssetBundle;
        }

        static public List<HotUpdateDefine.DownloadAssetBundle> SelectDeleteFiles(HotUpdateDefine.SerializeVersionControl versionOld,
                                                                                  HotUpdateDefine.SerializeVersionControl versionNew)
        {
            List<HotUpdateDefine.DownloadAssetBundle> listUpdateAssetBundle = new List<HotUpdateDefine.DownloadAssetBundle>();
            for (int i = 0; i < versionOld.ListAssetBundles.Count; ++i)
            {
                bool find = false;
                for (int j = 0; j < versionNew.ListAssetBundles.Count; ++j)
                {
                    if (versionNew.ListAssetBundles[j].AssetBundleName == versionOld.ListAssetBundles[i].AssetBundleName)
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    var bundleNew = new HotUpdateDefine.ExportAssetBundle();
                    bundleNew.AssetBundleName = versionOld.ListAssetBundles[i].AssetBundleName;
                    listUpdateAssetBundle.Add(new HotUpdateDefine.DownloadAssetBundle(bundleNew));
                }
            }
            return listUpdateAssetBundle;
        }

        static public HotUpdateDefine.SerializeVersionControl GetVersionControlConfigLocalPathAutoPlatform(string multiVersionControlRelativePath)
        {
            var strVersionControlSavePath = GetVersionControlFilePath(string.Empty, multiVersionControlRelativePath);
            string strVersionRead = FileHelper.ReadAllByPersistent(strVersionControlSavePath);

            HotUpdateDefine.SerializeVersionControl retValue = null;
            try
            {
                retValue = HotUpdateHelper.JsonToVersionControl(strVersionRead);
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate GetVersionControlConfigLocalPathAutoPlatform error: read json exception=" + e);
            }

            return retValue;
        }

        static public bool CheckAllAssetbundleValid(HotUpdateDefine.SerializeVersionControl versionControl, string multiVersionControlRelativePath)
        {
            bool isMissingFile = false;
            if (null == versionControl || 0 == versionControl.ListAssetBundles.Count)
            {
                Log.Warning("HotUpdate CheckAllAssetbundleValid warning: no version control config");
                isMissingFile = true;
            }
            else
            {
                for (int i = 0; i < versionControl.ListAssetBundles.Count; ++i)
                {
                    var bundleClient = versionControl.ListAssetBundles[i];
                    var fullPathTmp = FileHelper.GetFullpath(HotUpdateHelper.GetAssetBundlePathAutoPlatform(bundleClient.AssetBundleName, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE));

                    if (!FileHelper.ExistsFile(fullPathTmp))
                    {
                        Log.Info("HotUpdate check update local only fail: missing path=" + fullPathTmp);
                        isMissingFile = true;
                        break;
                    }
                }
            }

            return !isMissingFile;
        }

        static public bool CheckAllAssetbundleValidLocal(string multiVersionControlRelativePath)
        {
            var versionControlTmp = HotUpdateHelper.GetVersionControlConfigLocalPathAutoPlatform(multiVersionControlRelativePath);
            return CheckAllAssetbundleValid(versionControlTmp, multiVersionControlRelativePath);
        }

        static public HotUpdateDefine.SerializeVersionControl JsonToVersionControl(string json)
        {
            var retValue = new HotUpdateDefine.SerializeVersionControl();
            var jsonData = shaco.LitJson.JsonMapper.ToObject(json);
            retValue.AutoEncryt = jsonData.ContainKey("AutoEncryt") ? jsonData["AutoEncryt"].ToString().ToBool() : true;
            retValue.AutoBuildModifyPackage = jsonData.ContainKey("AutoBuildModifyPackage") ? jsonData["AutoBuildModifyPackage"].ToString().ToBool() : false;
            retValue.FileTag = jsonData["FileTag"].ToString();
            retValue.UnityVersion = jsonData["UnityVersion"].ToString();
            retValue.Version = jsonData["Version"].ToString();
            retValue.MD5Main = jsonData["MD5Main"].ToString();
            if (jsonData.ContainKey("TotalDataSize"))
            {
                retValue.TotalDataSize = jsonData["TotalDataSize"].ToString().ToLong();
            }
            else
            {
                retValue.TotalDataSize = 0;
            }

            var jsonAssetbundleDatas = jsonData["ListAssetBundles"];
            for (int i = 0; i < jsonAssetbundleDatas.Count; ++i)
            {
                var assetbundleNew = new HotUpdateDefine.ExportAssetBundle();
                var jsonAssetbundleData = jsonAssetbundleDatas[i];
                var jsonListFiles = jsonAssetbundleData["ListFiles"];

                assetbundleNew.AssetBundleName = jsonAssetbundleData["AssetBundleName"].ToString();
                assetbundleNew.AssetBundleMD5 = jsonAssetbundleData["AssetBundleMD5"].ToString();
                if (jsonAssetbundleData.ContainKey("fileSize"))
                    assetbundleNew.fileSize = jsonAssetbundleData["fileSize"].ToString().ToLong();
                for (int j = 0; j < jsonListFiles.Count; ++j)
                {
                    var jsonFile = jsonListFiles[j];
                    var assetbundleFileNew = new HotUpdateDefine.ExportAssetBundle.ExportFile();

                    assetbundleFileNew.Key = jsonFile["Key"].ToString();
                    assetbundleNew.ListFiles.Add(assetbundleFileNew);
                }

                retValue.ListAssetBundles.Add(assetbundleNew);
            }

            if (jsonData.ContainKey("ListKeepFileFolders"))
            {
                var jsonReadTmp = jsonData["ListKeepFileFolders"];
                for (int i = 0; i < jsonReadTmp.Count; ++i)
                {
                    var pathTmp = jsonReadTmp[i].ToString();
                    retValue.ListKeepFileFolders.Add(pathTmp);
                }
            }

            if (jsonData.ContainKey("ListIgnoreMetaExtensions"))
            {
                var jsonReadTmp = jsonData["ListIgnoreMetaExtensions"];
                for (int i = 0; i < jsonReadTmp.Count; ++i)
                {
                    var pathTmp = jsonReadTmp[i].ToString();
                    retValue.ListIgnoreMetaExtensions.Add(pathTmp);
                }
            }
            return retValue;
        }

        /// <summary>
        /// 判断是否为原始保留资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="shouldAutoLoadConfig">是否自动加载配置</param>
        /// <returns>
        static public bool IsKeepFile(string path)
        {
            var customParams = shaco.Base.EncryptDecrypt.GetCustomParameters(path);
            return !customParams.IsNullOrEmpty() ? customParams.Contains(shaco.HotUpdateDefine.ORIGINAL_FILE_TAG) : false;
        }

        static public void CheckLoadDownloadedAssetBundleConfig(string multiVersionControlRelativePath)
        {
            if (!_downloadedAllAssetbundleConfigs.ContainsKey(multiVersionControlRelativePath))
            {
                var versionFilePath = GetVersionControlFilePath(string.Empty, multiVersionControlRelativePath);
                versionFilePath = FileHelper.GetFullpath(versionFilePath);

                if (FileHelper.ExistsFile(versionFilePath))
                {
                    try
                    {
                        var newConfig = new Dictionary<string, string>();
                        var jsonReadTmp = FileHelper.ReadAllByUserPath(versionFilePath);
                        var versionControlTmp = HotUpdateHelper.JsonToVersionControl(jsonReadTmp);

                        for (int i = versionControlTmp.ListAssetBundles.Count - 1; i >= 0; --i)
                        {
                            var assetbundleInfoTmp = versionControlTmp.ListAssetBundles[i];
                            var key = AssetBundleKeyToPath(assetbundleInfoTmp.AssetBundleName);
                            var value = assetbundleInfoTmp.AssetBundleMD5;
                            if (!newConfig.ContainsKey(key))
                            {
                                newConfig.Add(key, value);
                            }
                            else
                            {
                                Log.Error("HotUpdateHelper CheckLoadDownloadedAssetBundleConfig error: has duplicate key=" + key + " path=" + versionFilePath);
                            }
                        }
                        _downloadedAllAssetbundleConfigs.Add(multiVersionControlRelativePath, newConfig);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("HotUpdateHelper CheckLoadDownloadedAssetBundleConfig error: can't read json path=" + versionFilePath + "\n" + e);
                    }
                }
                else
                {
                    Log.Error("HotUpdateHelper CheckLoadDownloadedAssetBundleConfig error: not found version control file path=" + versionFilePath);
                }
            }
        }

        /// <summary>
        /// 文件原始路径根据本地配置转换为加密的文件路径(资源的真实路径)
        /// </summary>
        /// <returns> 返回加密文件路径，返回空表示转化失败
        static public string ConvertToDownloadedEncodePath(string path, string multiVersionControlRelativePath, out string errorMessage)
        {
            var retValue = string.Empty;
            Dictionary<string, string> configFind = null;
            errorMessage = string.Empty;

            if (!_downloadedAllAssetbundleConfigs.TryGetValue(multiVersionControlRelativePath, out configFind))
            {
                errorMessage = "HotupdateHelper ConvertToDownloadedLocalPath error: not found config, path=" + path;
            }
            else
            {
                string md5Find = string.Empty;
                if (!configFind.TryGetValue(path, out md5Find))
                {
                    errorMessage = "HotupdateHelper ConvertToDownloadedLocalPath error: not found assetbundle config, path=" + path;
                }
                else
                {
                    if (path.Contains(md5Find))
                    {
                        Log.Warning("HotUpdateHelper ConvertToDownloadedLocalPath warning: has added version folder flag path=" + path);
                    }
                    else
                    {
                        path = HotUpdateHelper.GetAssetBundlePathAutoPlatform(path, multiVersionControlRelativePath, string.Empty);
                        path = HotUpdateHelper.AddAssetBundleNameTag(path, md5Find);

                        retValue = path;
                    }
                }
            }
            return retValue;
        }

        //设置网络动态下载资源路径
        static public void SetDynamicResourceAddress(params string[] address)
        {
            _dynamicNetResourceAddress = address;
        }

        //获取网络动态下载服务器地址
        static public string[] GetDynamicResourceAddress()
        {
            return _dynamicNetResourceAddress;
        }

        /// <summary>
        /// 依次从服务器地址中获取资源，如果获取到了正确资源，则停止遍历
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="callbackLoadEnd">下载完毕回调，如果回调参数不为空，则表示有发生错误</param>
        /// <param name="currentIndex">当前动态服务器地址使用下标</param>
        /// <returns>
        static public void ForeachGetDataCacheFromDynamicAddress(string path, string multiVersionControlRelativePath, System.Action<string> callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress, ref int currentIndex)
        {
            //网络动态加载Assetbundle
            if (!_dynamicNetResourceAddress.IsNullOrEmpty())
            {
                //所有下载地址遍历完毕也下载资源失败，退出循环
                if (currentIndex < 0 || currentIndex > _dynamicNetResourceAddress.Length - 1)
                {
                    var errorMessges = "HotUpdate ForeachGetDataCacheFromDynamicAddress error: can't download resource, path=" + path;
                    Log.Error(errorMessges);
                    callbackLoadEnd(errorMessges);
                    return;
                }

                //获取相对路径
                var relativePath = AssetBundleFullPathToRelativePath(path, multiVersionControlRelativePath);

                //确定本地资源配置是否加载过了
                CheckLoadDownloadedAssetBundleConfig(multiVersionControlRelativePath);

                //获取加密后的地址
                string errorMessage = null;
                var relativePathWithMD5 = HotUpdateHelper.ConvertToDownloadedEncodePath(relativePath, multiVersionControlRelativePath, out errorMessage);

                //没有该资源记录，停止下载
                if (string.IsNullOrEmpty(relativePathWithMD5))
                {
                    callbackLoadEnd(errorMessage);
                    return;
                }

                //删除动态网址中尾部包含版本目录的地方
                var address = _dynamicNetResourceAddress[currentIndex];
                if (address.LastIndexOf(multiVersionControlRelativePath) >= 0)
                {
                    address = address.RemoveBehind(multiVersionControlRelativePath);
                }

                //获取请求资源地址
                address = FileHelper.ContactPath(address, relativePathWithMD5);

                //给网址添加随机数，以免获取到服务器旧的缓存资源，导致资源更新失败
                address = HttpHelper.AddHeaderURL(address, new shaco.Base.HttpHelper.HttpComponent("rand_num", shaco.Base.Utility.Random().ToString()));

                //开始从服务器下载资源
                var wwwTmp = new HotUpdateImportWWW();
                wwwTmp.CreateByWWW(address);

                //下载成功后自动写入文件路径
                var versionControlFullPath = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                var localFullPath = FileHelper.ContactPath(versionControlFullPath, path.RemoveFront(versionControlFullPath));
                localFullPath = FileHelper.GetFullpath(localFullPath);

                wwwTmp.GetHttpHelper().SetAutoSaveWhenCompleted(localFullPath);

                //等待下载完毕回调
                var currentIndexTmp = currentIndex;
                wwwTmp.onProcessEnd.AddCallBack(wwwTmp, (object sender2) =>
                {
                    if (wwwTmp.HasError())
                    {
                        //下载发生错误，进入下一个动态地址进行下载
                        ++currentIndexTmp;
                        ForeachGetDataCacheFromDynamicAddress(path, multiVersionControlRelativePath, callbackLoadEnd, callbackProgress, ref currentIndexTmp);
                    }
                    else if (wwwTmp.IsCompleted())
                    {
                        //下载资源成功
                        if (null != callbackLoadEnd)
                        {
                            callbackLoadEnd(string.Empty);
                        }
                    }
                });

                //下载进度
                if (null != callbackProgress)
                {
                    wwwTmp.onProcessIng.AddCallBack(wwwTmp, (object sender) =>
                    {
                        callbackProgress(wwwTmp.GetDownloadResourceProgress());
                    });
                }
            }
        }

        static protected HotUpdateDefine.ExportAssetBundle FindExportBundle(List<HotUpdateDefine.ExportAssetBundle> listAssetBundle, string bundleName, bool isAutoCreate)
        {
            HotUpdateDefine.ExportAssetBundle ret = null;
            for (int i = 0; i < listAssetBundle.Count; ++i)
            {
                if (listAssetBundle[i].AssetBundleName == bundleName)
                {
                    ret = listAssetBundle[i];
                    break;
                }
            }

            if (isAutoCreate && ret == null)
            {
                ret = new HotUpdateDefine.ExportAssetBundle();
                ret.AssetBundleName = bundleName;
                listAssetBundle.Add(ret);
            }

            return ret;
        }

        /// <summary>
        /// 检查本地资源管理配置文件是否都存在
        /// </summary>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <returns>true:配置ok false:配置有丢失</returns>
        static public bool HasAllVersionControlFiles(string multiVersionControlRelativePath)
        {
            var pathMainMD5 = HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(multiVersionControlRelativePath);
            var pathManifest1 = HotUpdateHelper.GetAssetBundleManifestMemoryPathAutoPlatform(multiVersionControlRelativePath);
            var pathManifest2 = FileHelper.RemoveExtension(pathManifest1);

            var pathVersionControl = HotUpdateHelper.GetVersionControlFolderAuto(HotUpdateDefine.VERSION_CONTROL + HotUpdateDefine.EXTENSION_VERSION_CONTROL, multiVersionControlRelativePath);
            pathVersionControl = HotUpdateHelper.AddFileTagAutoPlatform(pathVersionControl);
            pathVersionControl = FileHelper.GetFullpath(pathVersionControl);

            bool b1 = FileHelper.ExistsFile(pathMainMD5);
            bool b2 = FileHelper.ExistsFile(pathManifest1);
            bool b3 = FileHelper.ExistsFile(pathManifest2);
            bool b4 = FileHelper.ExistsFile(pathVersionControl);

            return b1 && b2 && b3 && b4;
        }

        /// <summary>
        /// 从本地校验是否有文件需要更新(不需要联网)
        /// </summary>
        /// <param name="packageVersion">包名的版本号，如果设置为空默认使用Application.Version</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <returns>true:有文件需要更新 false:不需要更新</returns>
        static public bool CheckUpdateLocalOnly(string packageVersion, string multiVersionControlRelativePath = "")
        {
            return IsCheckUpdateRequest(packageVersion, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 获取热更新目录路径
        /// <param name="path">文件路径</param>
        /// </summary>
        static public string GetHotUpdatePath(string path, string multiVersionControlRelativePath)
        {
            var retValue = shaco.HotUpdateHelper.GetAssetBundlePathAutoPlatform(path, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            retValue = FileHelper.GetFullpath(retValue);
            retValue = FileHelper.AddFolderNameByPath(retValue, shaco.ResourcesEx.DEFAULT_PREFIX_PATH_LOWER);
            return retValue;
        }

        static private bool IsCheckUpdateRequest(string packageVersion, string multiVersionControlRelativePath)
        {
            //get client version
            if (!HasAllVersionControlFiles(multiVersionControlRelativePath))
            {
                return true;
            }
            try
            {
                var versionClient = HotUpdateHelper.GetVersionControlConfigLocalPathAutoPlatform(multiVersionControlRelativePath);
                if (versionClient == null)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(packageVersion))
                {
                    //if resource Version code < package Version code
                    //we must requst user download new resource from server !
                    bool isRequstUpdate = false;
                    var resourceVersionCodes = versionClient.Version.Split(FileDefine.DOT_SPLIT);
                    var packageVersionCodes = packageVersion.Split(FileDefine.DOT_SPLIT);
                    for (int i = 0; i < packageVersionCodes.Length; ++i)
                    {
                        int codeResourceTmp = i < resourceVersionCodes.Length ? resourceVersionCodes[i].ToInt() : -1;
                        int codePackageTmp = packageVersionCodes[i].ToInt();
                        if (codeResourceTmp < codePackageTmp)
                        {
                            isRequstUpdate = true;
                            break;
                        }
                    }

                    if (isRequstUpdate)
                    {
                        Log.Info("HotUpdate CheckUpdateRequest: client version code is less than package version code, we must requst user download new resource from server"
                        + "\n client version code=" + versionClient.Version + " package version code=" + packageVersion);

                        return true;
                    }
                }

                if (!HotUpdateHelper.CheckAllAssetbundleValid(versionClient, multiVersionControlRelativePath))
                {
                    Log.Info("HotUpdate CheckUpdateRequest: The resources are incomplete and need to be updated online");
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate check update request error: msg=" + e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 完整的资源路径转换为VersionControl根目录开始的相对路径
        /// <param name="path">资源路径</param>
        /// <return>资源相对路径</return>
        /// </summary>
        static private string AssetBundleFullPathToRelativePath(string path, string multiVersionControlRelativePath)
        {
            var versionControlTag = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
            if (!string.IsNullOrEmpty(versionControlTag) && versionControlTag[versionControlTag.Length - 1].ToString() != FileDefine.PATH_FLAG_SPLIT)
            {
                versionControlTag += FileDefine.PATH_FLAG_SPLIT;
            }
            return path.RemoveFront(versionControlTag);
        }
    }
}
