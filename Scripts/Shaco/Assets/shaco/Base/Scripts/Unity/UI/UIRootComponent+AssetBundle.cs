using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
	public partial class UIRootComponent : MonoBehaviour
    {
        private partial class UIState : MonoBehaviour, IUIState
		{
            public Dictionary<string, AutoUnloadAssetBundleInfo> _autoUnloadAssetBundlesInfo = new Dictionary<string, AutoUnloadAssetBundleInfo>();
        }

        private class AutoUnloadAssetBundleInfo
        {
            public bool unloadAllLoadedObjects = true;
        }

        public string SetAutoUnloadAssetBundle(UnityEngine.Component target, string pathAssetBundle, string multiVersionControlRelativePath, bool unloadAllLoadedObjects)
		{
			return SetAutoUnloadAssetBundle(target.ToTypeString(), pathAssetBundle, multiVersionControlRelativePath, unloadAllLoadedObjects);
		}

        public string SetAutoUnloadAssetBundle(string key, string pathAssetBundle, string multiVersionControlRelativePath, bool unloadAllLoadedObjects)
		{
            var uiState = (UIState)GetUIState(key);
            var assetbundleKey = HotUpdateDataCache.GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            var retValue = assetbundleKey;

            //只支持从ResourcesEx类加载的路径
            if (!pathAssetBundle.Contains(ResourcesEx.DEFAULT_PREFIX_PATH_LOWER))
            {
                assetbundleKey = HotUpdateDataCache.GetFullAssetBundleKey(ResourcesEx.DEFAULT_PREFIX_PATH_LOWER.ContactPath(pathAssetBundle), multiVersionControlRelativePath);;
            }

            if (uiState == null)
            {
                shaco.Log.Error("UIRootComponent SetAutoUnloadAssetBundle error: not found ui, key=" + key);
            }
            else
            {
                var newInfo = new AutoUnloadAssetBundleInfo();
                newInfo.unloadAllLoadedObjects = unloadAllLoadedObjects;

                if (uiState._autoUnloadAssetBundlesInfo.ContainsKey(assetbundleKey))
                {
                    shaco.Log.Error("UIRootComponent SetAutoUnloadAssetBundle error: has setted auto unload assetbundle, key=" + key + " path=" + pathAssetBundle);
                    return retValue;
                }
                uiState._autoUnloadAssetBundlesInfo.Add(assetbundleKey, newInfo);
            }
            return retValue;
		}

		private void CheckAutoAssetBundlesRemove(UIState uiState)
		{
			foreach (var iter in uiState._autoUnloadAssetBundlesInfo)
			{
				shaco.HotUpdateDataCache.UnloadAssetBundle(iter.Key, string.Empty, iter.Value.unloadAllLoadedObjects);
			}
			uiState._autoUnloadAssetBundlesInfo.Clear();
		}
    }
}
