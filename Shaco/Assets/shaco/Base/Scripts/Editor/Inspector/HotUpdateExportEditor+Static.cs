using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using shaco;

namespace shacoEditor
{
    public partial class HotUpdateExportEditor : EditorWindow
    {
        //打包前需要临时替换导出的资源文件，因为有的文件unity不支持导出的
        public class ExportReplaceAssetInfo
        {
            //需要替换资源使用到的导出ab包配置对象
            public SelectFile removeAssetBundle = null;

            //需要被替换的资源路径
            public string removeFilePath = string.Empty;

            //新的资源路径
            public string excelSaveTxtPath = string.Empty;

            //excel临时数据
            public shaco.Base.ExcelData excelData = null;
        }

        static public void BuildAndroid(string pathFolder, Dictionary<string, SelectFile> allAssetBundles)
        {
            HotUpdateExportEditor.BuildAssetBundles(pathFolder, allAssetBundles, HotUpdateDefine.Platform.Android);
        }

        static public void BuildIos(string pathFolder, Dictionary<string, SelectFile> allAssetBundles)
        {
            HotUpdateExportEditor.BuildAssetBundles(pathFolder, allAssetBundles, HotUpdateDefine.Platform.iOS);
        }

        /// <summary>
        /// 获取excel转化为txt需要的数据，因为unity不支持excel文件格式导出，也不方便读取
        /// <param name="allAssetBundles">需要打包的assetbundle</param>
        /// <param name="exportTmpPath">excel导出临时目录</param>
        /// <return>返回需要替换的excel数据</return>
        /// </summary>
        static public List<ExportReplaceAssetInfo> GetExcelToTxtInfo(Dictionary<string, SelectFile> allAssetBundles)
        {
            //导出前需要替换的临时数据
            var retValue = new List<ExportReplaceAssetInfo>();

            foreach (var key in allAssetBundles.Keys)
            {
                var value = allAssetBundles[key];

                foreach (var key2 in value.ListAsset.Keys)
                {
                    var pathTmp = value.ListAsset[key2].Asset;

                    if (pathTmp.EndsWith(".xlsx") || pathTmp.EndsWith(".xls"))
                    {
                        //打开excel文件
                        var fullPath = shaco.Base.FileHelper.ContactPath(Application.dataPath.Remove("Assets"), pathTmp);
                        var excelDataTmp = shaco.Base.ExcelHelper.OpenWithFile(fullPath);

                        //获取原始文件路径
                        var exportTmpPath = EditorHelper.GetFullPath(value.ListAsset[key2].Asset);

                        //导出txt文件
                        var exportTxtPathTmp = exportTmpPath + HotUpdateDefine.EXTENSION_ENCRPYT;
                        excelDataTmp.SaveAsTxt(exportTxtPathTmp);

                        var newReplaceInfo = new ExportReplaceAssetInfo()
                        {
                            removeAssetBundle = value,
                            removeFilePath = key2,
                            excelData = excelDataTmp,
                            excelSaveTxtPath = exportTxtPathTmp
                        };
                        retValue.Add(newReplaceInfo);
                    }
                }
            }

            //删除导出assetbundle的旧配置
            if (retValue.Count > 0)
            {
                AssetDatabase.Refresh();
            }

            //替换临时导出数据
            for (int i = 0; i < retValue.Count; ++i)
            {
                var infoTmp = retValue[i];
                var assetPath = EditorHelper.FullPathToUnityAssetPath(infoTmp.excelSaveTxtPath);

                //更新导出ab包配置
                infoTmp.removeAssetBundle.ListAsset.Remove(infoTmp.removeFilePath);
                infoTmp.removeAssetBundle.ListAsset.Add(assetPath, new SelectFile.FileInfo(assetPath));
            }

            return retValue;
        }

        static public void BuildAssetBundles(string pathFolder, Dictionary<string, SelectFile> allAssetBundles, HotUpdateDefine.Platform platform)
        {
            shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(pathFolder);

#if UNITY_5_3_OR_NEWER
            AssetBundleBuild[] allAssetBundleBuild = new AssetBundleBuild[allAssetBundles.Count];
            int index = 0;

            foreach (var key in allAssetBundles.Keys)
            {
                var value = allAssetBundles[key];

                var assetsTmp = new List<string>();
                foreach (var key2 in value.ListAsset.Keys)
                {
                    // var pathTmp = AssetDatabase.GetAssetPath(value.ListAsset[key2].Asset);
                    assetsTmp.Add(value.ListAsset[key2].Asset);
                }

                allAssetBundleBuild[index] = GetAssetBundleBuild(key, assetsTmp.ToArray(), platform);
                ++index;
            }

            var buildTarget = GetBuildTargetByPlatForm(platform);
            BuildAssetBundleOptions optionsTmp = BuildAssetBundleOptions.ChunkBasedCompression
                                               | BuildAssetBundleOptions.StrictMode
                                               | BuildAssetBundleOptions.IgnoreTypeTreeChanges;

            shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(pathFolder.AddBehindNotContains(shaco.Base.FileDefine.PATH_FLAG_SPLIT));
            BuildPipeline.BuildAssetBundles(pathFolder, allAssetBundleBuild, optionsTmp, buildTarget);
#else
            //每N个打包后需要回收一次内存，否则因为LoadAssetAtPath导致内存溢出
            int recoveryCount = 10;
            int buildCount = 0;
            
            foreach (var iter in allAssetBundles)
            {
                Object[] assetsTmp = new Object[iter.Value.ListAsset.Count];
                int index2 = 0;
                foreach (var iter2 in iter.Value.ListAsset)
                {
                    assetsTmp[index2++] = AssetDatabase.LoadAssetAtPath(iter2.Value.Asset, typeof(Object));
                    ++buildCount;
                }

                if (assetsTmp.Length > 0)
                {
                    var fullPath = shaco.Base.FileHelper.ContactPath(pathFolder, iter.Key);
                    var buildTarget = GetBuildTargetByPlatForm(platform);
                    shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(shaco.Base.FileHelper.GetFolderNameByPath(fullPath));
                    BuildPipeline.BuildAssetBundle(assetsTmp[0], assetsTmp, fullPath, 
                        BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, buildTarget);
                }

                if (buildCount >= recoveryCount)
                {
                    buildCount = 0;
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }
#endif       
        }

        // static public void CopyResourcesToResourcesHotUpdate(List<string> keepFolderPaths)
        // {
        //     foreach (var keepFolderPath in keepFolderPaths)
        //     {
        //         var keepFolderPathLower = keepFolderPath.ToLower();
        //         if (keepFolderPathLower.Contains(shaco.ResourcesEx.DEFAULT_PREFIX_PATH_LOWER))
        //         {
        //             var hotupdatePathTmp = shaco.Base.FileHelper.ContactPath(Application.dataPath, keepFolderPathLower.Remove("assets/"));
        //             var resourcesPathTmp = shaco.Base.FileHelper.ContactPath(Application.dataPath, keepFolderPathLower.Replace(shaco.ResourcesEx.DEFAULT_PREFIX_PATH_LOWER, "Resources/"));

        //             shaco.Base.FileHelper.CopyFileByUserPath(resourcesPathTmp, hotupdatePathTmp);
        //         }
        //     }
        // }

        static public void BuildOriginalFiles(string pathFolder, Dictionary<string, SelectFile> orignialFiles, bool isAutoEncryptOriginalFile)
        {
            var applicationPathTmp = Application.dataPath.Replace('\\', '/').Remove("Assets");

            foreach (var iter in orignialFiles)
            {
                if (iter.Value.ListAsset.Count > 1)
                {
                    Log.Error("export assetbundle=" + iter.Key);
                    Log.Error("HotUpdateExportEditor BuildOriginalFiles error: When exported as a original resource file, only 1 files are supported into 1 AssetBundle, Otherwise the back of the original resource will be automatically filtered out");
                }
                foreach (var iter2 in iter.Value.ListAsset)
                {
                    var sourceFileNameTmp = iter2.Value.Asset.ToLower();
                    var sourcePathTmp = shaco.Base.FileHelper.ContactPath(applicationPathTmp, sourceFileNameTmp);
                    var destinationPathTmp = shaco.Base.FileHelper.ContactPath(pathFolder, sourceFileNameTmp);

                    //删除加密路径
                    destinationPathTmp = destinationPathTmp.Remove(shaco.HotUpdateDefine.EXTENSION_ENCRPYT);

                    //替换后缀名为assetbundle
                    destinationPathTmp = shaco.Base.FileHelper.ReplaceExtension(destinationPathTmp, HotUpdateDefine.EXTENSION_ASSETBUNDLE);

                    //查看带有md5的目标文件是否存在，如果存在，则不用再次导出原始文件了
                    var destinationPathWithMD5Tmp = shaco.HotUpdateHelper.AddAssetBundleNameTag(destinationPathTmp, iter.Value.AssetBundleMD5);
                    if (shaco.HotUpdateHelper.IsKeepFile(destinationPathWithMD5Tmp))
                    {
                        continue;
                    }
                    else
                    {
                        //因为现在在打包保留文件，如果存在非保留文件，则优先删除掉它
                        if (shaco.Base.FileHelper.ExistsFile(destinationPathWithMD5Tmp))
                        {
                            shaco.Base.FileHelper.DeleteByUserPath(destinationPathWithMD5Tmp);
                        }
                    }

                    //拷贝原始文件
                    shaco.Base.FileHelper.CopyFileByUserPath(sourcePathTmp, destinationPathTmp);

                    //原始文件加密，无论是否加密文件内容，都需要加密文件头，因为会写入是否为原始文件的标记，以方便在读取的时候判断
                    var secretRandomCode = shaco.Base.Utility.Random(int.MinValue, int.MaxValue);
                    shaco.Base.EncryptDecrypt.Encrypt(destinationPathTmp, isAutoEncryptOriginalFile ? 3.0f : 1.0f, secretRandomCode, shaco.HotUpdateDefine.ORIGINAL_FILE_TAG);

                    //删除manifest
                    HotUpdateHelper.DeleteManifest(destinationPathTmp);

                    //只导出第一个文件作为原始资源，后续的资源会被自动过滤
                    break;
                }
            }
        }

        static public void BuildAssetBundlesAutoPlatform(string pathFolder, Dictionary<string, SelectFile> allAssetBundles)
        {
            BuildAssetBundles(pathFolder, allAssetBundles, HotUpdateHelper.GetAssetBundleAutoPlatform());
        }

#if UNITY_5_3_OR_NEWER
        static public AssetBundleBuild GetAssetBundleBuild(string filename, string[] assets, HotUpdateDefine.Platform platform)
        {
            AssetBundleBuild ret = new AssetBundleBuild();

            string[] assetPath = new string[assets.Length];
            for (int i = 0; i < assets.Length; ++i)
            {
                assetPath[i] = assets[i].ToLower();

                //this bug has fixed in unity version 5.3 or newer
                // for (int i = 0; i < assets.Length; ++i)
                // {
                //     assetPath[i] = EditorHelper.GetAssetPathLower(assets[i]);

                //     //if have spritePackingTag, we can't read texture object from assetbundle
                //     //but can read texture by 'Resource.Load', unkown reason now...
                //     //fixed: we force set spritePackingTag is empty string can fix it
                //     var texImportTmp = AssetImporter.GetAtPath(assetPath[i]) as TextureImporter;
                //     if (texImportTmp != null && !string.IsNullOrEmpty(texImportTmp.spritePackingTag))
                //     {
                //         texImportTmp.spritePackingTag = string.Empty;
                //         AssetDatabase.ImportAsset(assetPath[i]);
                //     }
                // }
            }

            ret.assetBundleName = filename;
            ret.assetNames = assetPath;

            return ret;
        }
#endif

        static public UnityEditor.BuildTarget GetBuildTargetByPlatForm(HotUpdateDefine.Platform platform)
        {
            UnityEditor.BuildTarget ret = UnityEditor.BuildTarget.Android;

            switch (platform)
            {
                case HotUpdateDefine.Platform.Android:
                    ret = UnityEditor.BuildTarget.Android;
                    break;
                case HotUpdateDefine.Platform.iOS:
#if UNITY_5_3_OR_NEWER
                    ret = UnityEditor.BuildTarget.iOS;
#else
                    ret = UnityEditor.BuildTarget.iPhone;
#endif
                    break;
                default:
                    ret = UnityEditor.BuildTarget.Android;
                    DisplayDialogError("HoUpdateExportEditor getBuildTargetByPlatForm error: unsupport platform");
                    break;
            }

            return ret;
        }

        static public List<string> ObjectsToStrings(List<Object> listObjects)
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < listObjects.Count; ++i)
            {
                if (listObjects[i] != null)
                {
                    var pathTmp = EditorHelper.GetAssetPathLower(listObjects[i]);
                    ret.Add(pathTmp);
                }
            }
            return ret;
        }

        /// <summary>
        /// 是否为加密后的excel路径
        /// <param name="path">加密后的路径</param>
        /// </summary>
        static public bool IsEncryptExcelPath(string path)
        {
            return (path.LastIndexOf(".xlsx" + HotUpdateDefine.EXTENSION_ENCRPYT) > 0 || path.LastIndexOf(".xls" + HotUpdateDefine.EXTENSION_ENCRPYT) > 0 || path.LastIndexOf(".csv" + HotUpdateDefine.EXTENSION_ENCRPYT) > 0);
        }

        /// <summary>
        /// 获取导出的assetbundle文件路径
        /// <param name="pathRoot">导出根目录，一般为asset</param>
        /// <param name="assetbundleInfo">导出文件信息</param>
        /// <return>导出的assetbundle文件路径</return>
        /// </summary>
        static private string GetAssetBundleFullPath(string pathRoot, shaco.HotUpdateDefine.ExportAssetBundle assetbundleInfo)
        {
            return GetAssetBundleFullPath(pathRoot, assetbundleInfo.AssetBundleName, assetbundleInfo.AssetBundleMD5);
        }

        /// <summary>
        /// 获取导出的assetbundle文件路径
        /// <param name="pathRoot">导出根目录，一般为asset</param>
        /// <param name="assetbundleName">assetbundle名字</param>
        /// <param name="assetbundleMD5">assetbundle的MD5</param>
        /// <return>导出的assetbundle文件路径</return>
        /// </summary>
        static private string GetAssetBundleFullPath(string pathRoot, string assetbundleName, string assetbundleMD5)
        {
            var fileNameTmp = shaco.HotUpdateHelper.AssetBundleKeyToPath(assetbundleName);

            int findIndexTmp = pathRoot.LastIndexOf("/assets");
            if (findIndexTmp >= 0)
            {
                pathRoot = pathRoot.Remove(findIndexTmp);
            }

            var fullPathTmp = pathRoot.ContactPath(fileNameTmp);

            if (!shaco.Base.FileHelper.ExistsFile(fullPathTmp))
            {
                fullPathTmp = shaco.HotUpdateHelper.AddAssetBundleNameTag(fullPathTmp, assetbundleMD5);
            }

            return fullPathTmp;
        }
    }
}