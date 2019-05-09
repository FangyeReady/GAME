using UnityEngine;
using System.Collections;
using shaco.Base;
using System.Collections.Generic;

namespace shaco
{
    public class HotUpdateManifest
    {
#if UNITY_5_3_OR_NEWER

        private class ManifestInfo
        {
            public AssetBundleManifest manifest = null;
            public AssetBundle manifestAssetBundle = null;
        }
        
        private Dictionary<string, ManifestInfo> _allManifestInfo = new Dictionary<string, ManifestInfo>();

        static public bool CheckDependenciesAsync(MonoBehaviour target, string multiVersionControlRelativePath, string assetBundleName, System.Action callbackCheckEnd)
        {
            CheckLoaded(multiVersionControlRelativePath);
            if (!GameEntry.GetInstance<HotUpdateManifest>()._allManifestInfo.ContainsKey(multiVersionControlRelativePath))
            {
                Log.Warning("HotUpdateManifest CheckDependenciesAsync warning: manifet not setup...");
                callbackCheckEnd();
                return true;
            }

            var listDepend = GetDependencies(assetBundleName, multiVersionControlRelativePath);
            if (listDepend == null)
            {
                return false;
            }

            int createCount = 0;

            for (int i = 0; i < listDepend.Length; ++i)
            {
                if (HotUpdateDataCache.IsLoadedCache(listDepend[i], multiVersionControlRelativePath))
                {
                    continue;
                }
                else
                {
                    HotUpdateImportMemory updateTmp = new HotUpdateImportMemory();
                    updateTmp.CreateByMemoryAsyncAutoPlatform(listDepend[i], multiVersionControlRelativePath);
                    updateTmp.onProcessEnd.AddCallBack(updateTmp, (object sender) =>
                    {
                        if (updateTmp.IsCompleted())
                        {
                            ++createCount;

                            if (createCount >= listDepend.Length)
                            {
                                if (callbackCheckEnd != null)
                                    callbackCheckEnd();
                            }
                        }
                    });

                    HotUpdateDataCache.AddUpdateCache(listDepend[i], multiVersionControlRelativePath, updateTmp);
                }
            }

            if (listDepend.Length == 0)
            {
                if (callbackCheckEnd != null)
                    callbackCheckEnd();
            }
            return true;
        }

        static public bool CheckDependencies(string assetBundleName, string multiVersionControlRelativePath)
        {
            CheckLoaded(multiVersionControlRelativePath);
            if (!GameEntry.GetInstance<HotUpdateManifest>()._allManifestInfo.ContainsKey(multiVersionControlRelativePath))
            {
                Log.Warning("HotUpdateManifest CheckDependencies warning: manifet not setup...");
                return true;
            }

            assetBundleName = FileHelper.AddExtensions(assetBundleName, HotUpdateDefine.EXTENSION_ASSETBUNDLE);

            var listDepend = GetDependencies(assetBundleName, multiVersionControlRelativePath);
            if (listDepend == null)
            {
                return false;
            }

            for (int i = 0; i < listDepend.Length; ++i)
            {
                if (HotUpdateDataCache.IsLoadedCache(listDepend[i], multiVersionControlRelativePath))
                {
                    continue;
                }
                else
                {
                    HotUpdateImportMemory updateTmp = new HotUpdateImportMemory();
                    updateTmp.CreateByMemoryAutoPlatform(listDepend[i], multiVersionControlRelativePath);

                    HotUpdateDataCache.AddUpdateCache(listDepend[i], multiVersionControlRelativePath, updateTmp);
                }
            }
            return true;
        }

        static public void PrintManifest(AssetBundleManifest manifest)
        {
            if (manifest == null)
                return;

            var allAB = manifest.GetAllAssetBundles();
            Log.Info("all assetbundle count=" + allAB.Length);

            for (int i = 0; i < allAB.Length; ++i)
            {
                Log.Info("assetbundle name=" + allAB[i]);
            }
        }

        static private string[] GetDependencies(string assetBundleName, string multiVersionControlRelativePath)
        {
            var instance = GameEntry.GetInstance<HotUpdateManifest>();
            assetBundleName = FileHelper.AddExtensions(assetBundleName, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            string[] retValue = instance._allManifestInfo.ContainsKey(multiVersionControlRelativePath) ? instance._allManifestInfo[multiVersionControlRelativePath].manifest.GetAllDependencies(assetBundleName) : null;
            if (retValue == null)
            {
                Log.Error("HotUpdate check dependencies async error: not find depend ! assetBundleName=" + assetBundleName);
            }
            return retValue;
        }

        public static void Unload()
        {
            var instance = GameEntry.GetInstance<HotUpdateManifest>();
            foreach (var iter in instance._allManifestInfo)
            {
                iter.Value.manifestAssetBundle.Unload(true);
            }
            GameEntry.RemoveIntance<HotUpdateManifest>();
        }

        static private void CheckLoaded(string multiVersionControlRelativePath)
        {
            var instance = GameEntry.GetInstance<HotUpdateManifest>();
            if (instance._allManifestInfo.ContainsKey(multiVersionControlRelativePath))
            {
                return;
            }

            if (!Application.isPlaying)
            {
                shaco.Log.Warning("HotUpdateManifest CheckLoaded warning: The method can only be called when the game is run");
                return;
            }

            var newManifestInfo = new ManifestInfo();
            var pathVersion = HotUpdateHelper.GetAssetBundleManifestMemoryPathAutoPlatform(multiVersionControlRelativePath);
            pathVersion = FileHelper.RemoveExtension(pathVersion);

            if (!FileHelper.ExistsFile(pathVersion))
            {
                //资源文件可能存在本地，则不加载manifest了
                // Log.Warning("HotUpdate inti manifest error: not found path=" + pathVersion);
                return;
            }

            try
            {
                newManifestInfo.manifestAssetBundle = AssetBundle.LoadFromFile(pathVersion);
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate init manifest error: exception msg=" + e + " path=" + pathVersion);
                return;
            }

            if (newManifestInfo.manifestAssetBundle == null)
            {
                Log.Error("HotUpdate init manifest error: load resource is null ! " + " path=" + pathVersion);
                return;
            }

            newManifestInfo.manifest = newManifestInfo.manifestAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
            if (newManifestInfo.manifest == null)
            {
                Log.Error("HotUpdate load manifest erorr: pathVersion=" + pathVersion);
            }
            else
            {
                instance._allManifestInfo.Add(multiVersionControlRelativePath, newManifestInfo);
            }
        }
#else 
        static public bool CheckDependenciesAsync(MonoBehaviour target, string multiVersionControlRelativePath, string assetBundleName, System.Action callbackCheckEnd)
        {
            if (null != callbackCheckEnd)
            {
                callbackCheckEnd();
            }
            return true;
        }

        static public bool CheckDependencies(string assetBundleName, string multiVersionControlRelativePath)
        {
            return true;
        }
#endif
    }
}