using System.Collections;
using shaco.Base;

namespace shaco
{
    /// <summary>
    /// 该类主要用于加载包体资源(Resources.Load)或者下载资源(HotUpdateWWW)
    /// 如果设置了RESOURCES_FIRST宏，则会先加载包体资源，如果没有包体资源则加载下载资源，反之则先加载下载资源
    /// </summary>
    public partial class ResourcesEx
    {
        static public readonly string DEFAULT_PREFIX_PATH = "Assets/Resources_HotUpdate/";
        static public readonly string DEFAULT_PREFIX_PATH_LOWER = DEFAULT_PREFIX_PATH.ToLower();
        public const FileDefine.FileExtension DEFAULT_EXTENSION = FileDefine.FileExtension.None;

        static public T LoadResourcesOrLocal<T>(string path, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            return (T)_LoadResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(T), extension);
        }

        static public T LoadResourcesOrLocal<T>(string path, string prefixPath, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            return (T)_LoadResourcesOrLocal(path, multiVersionControlRelativePath, prefixPath, typeof(T), extension);
        }
        static public T LoadResourcesOrLocal<T>(string path, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            return (T)_LoadResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(T), ResourcesEx.DEFAULT_EXTENSION);
        }

        static public UnityEngine.Object LoadResourcesOrLocal(string path, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            return _LoadResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), extension);
        }
        static public UnityEngine.Object LoadResourcesOrLocal(string path, string prefixPath, System.Type type, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            return _LoadResourcesOrLocal(path, multiVersionControlRelativePath, prefixPath, type, extension);
        }
        static public UnityEngine.Object LoadResourcesOrLocal(string path, System.Type type, string multiVersionControlRelativePath = "")
        {
            return _LoadResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, type, DEFAULT_EXTENSION);
        }
        static public UnityEngine.Object LoadResourcesOrLocal(string path, string multiVersionControlRelativePath = "")
        {
            return _LoadResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), DEFAULT_EXTENSION);
        }

        static public void LoadResourcesOrLocalAsync<T>(string path, string prefixPath, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress = null, FileDefine.FileExtension extension = DEFAULT_EXTENSION, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, prefixPath, typeof(T), callbackLoadEnd, callbackProgress, extension);
        }
        static public void LoadResourcesOrLocalAsync<T>(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(T), callbackLoadEnd, callbackProgress, DEFAULT_EXTENSION);
        }
        static public void LoadResourcesOrLocalAsync<T>(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(T), callbackLoadEnd, null, DEFAULT_EXTENSION);
        }

        static public void LoadResourcesOrLocalAsync(string path, string prefixPath, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress = null, FileDefine.FileExtension extension = DEFAULT_EXTENSION, string multiVersionControlRelativePath = "")
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, prefixPath, type, callbackLoadEnd, callbackProgress, extension);
        }
        static public void LoadResourcesOrLocalAsync(string path, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress = null, string multiVersionControlRelativePath = "")
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, type, callbackLoadEnd, callbackProgress, DEFAULT_EXTENSION);
        }
        static public void LoadResourcesOrLocalAsync(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress, string multiVersionControlRelativePath = "")
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), callbackLoadEnd, callbackProgress, DEFAULT_EXTENSION);
        }
        static public void LoadResourcesOrLocalAsync(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, string multiVersionControlRelativePath = "")
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), callbackLoadEnd, null, DEFAULT_EXTENSION);
        }

        static public T[] LoadAllResourcesOrLocal<T>(string path, string prefixPath, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, prefixPath, typeof(T), extension).ToArray<UnityEngine.Object, T>();
        }
        static public T[] LoadAllResourcesOrLocal<T>(string path, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(T), extension).ToArray<UnityEngine.Object, T>();
        }
        static public T[] LoadAllResourcesOrLocal<T>(string path, string multiVersionControlRelativePath = "") where T : UnityEngine.Object
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(T), DEFAULT_EXTENSION).ToArray<UnityEngine.Object, T>();
        }

        static public UnityEngine.Object[] LoadAllResourcesOrLocal(string path, string prefixPath, System.Type type, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, prefixPath, type, extension);
        }
        static public UnityEngine.Object[] LoadAllResourcesOrLocal(string path, System.Type type, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, type, extension);
        }
        static public UnityEngine.Object[] LoadAllResourcesOrLocal(string path, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), extension);
        }
        static public UnityEngine.Object[] LoadAllResourcesOrLocal(string path, string multiVersionControlRelativePath = "")
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), DEFAULT_EXTENSION);
        }

        static public void LoadAllResourcesOrLocalAsync(string path, string prefixPath, System.Type type, shaco.Base.FileDefine.FileExtension extension, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress = null, string multiVersionControlRelativePath = "")
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, prefixPath, type, extension, callbackLoadEnd, callbackProgress);
        }
        static public void LoadAllResourcesOrLocalAsync(string path, string prefixPath, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress = null, string multiVersionControlRelativePath = "")
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, prefixPath, type, DEFAULT_EXTENSION, callbackLoadEnd, callbackProgress);
        }
        static public void LoadAllResourcesOrLocalAsync(string path, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress = null, string multiVersionControlRelativePath = "")
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, type, DEFAULT_EXTENSION, callbackLoadEnd, callbackProgress);
        }
        static public void LoadAllResourcesOrLocalAsync(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress, string multiVersionControlRelativePath = "")
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), DEFAULT_EXTENSION, callbackLoadEnd, callbackProgress);
        }
        static public void LoadAllResourcesOrLocalAsync(string path, shaco.Base.FileDefine.FileExtension extension, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, string multiVersionControlRelativePath = "")
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), extension, callbackLoadEnd, null);
        }
        static public void LoadAllResourcesOrLocalAsync(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, string multiVersionControlRelativePath = "")
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), DEFAULT_EXTENSION, callbackLoadEnd, null);
        }

        static public bool UnloadAssetBundleLocal(string path, string prefixPath, FileDefine.FileExtension extension, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = "")
        {
            string pathAssetBundle = string.Empty;
            GetAssetBundlePathAndFileName(path, prefixPath, ref pathAssetBundle, extension);

            return shaco.HotUpdateDataCache.UnloadAssetBundle(pathAssetBundle, multiVersionControlRelativePath, unloadAllLoadedObjects);
        }

        static public bool UnloadAssetBundleLocal(string path, string prefixPath, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = "")
        {
            return UnloadAssetBundleLocal(path, prefixPath, DEFAULT_EXTENSION, unloadAllLoadedObjects, multiVersionControlRelativePath);
        }

        static public bool UnloadAssetBundleLocal(string path, bool unloadAllLoadedObjects, FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            return UnloadAssetBundleLocal(path, DEFAULT_PREFIX_PATH_LOWER, extension, unloadAllLoadedObjects, multiVersionControlRelativePath);
        }

        static public bool UnloadAssetBundleLocal(string path, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = "")
        {
            return UnloadAssetBundleLocal(path, DEFAULT_PREFIX_PATH_LOWER, DEFAULT_EXTENSION, unloadAllLoadedObjects, multiVersionControlRelativePath);
        }

        static public bool ExistsResourcesOrLocal(string path, string multiVersionControlRelativePath = "")
        {
            return ExistsResourcesOrLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, DEFAULT_EXTENSION);
        }

        static public bool ExistsResourcesOrLocal(string path, string multiVersionControlRelativePath, string prefixPath)
        {
            return ExistsResourcesOrLocal(path, multiVersionControlRelativePath, prefixPath, DEFAULT_EXTENSION);
        }

        static public bool ExistsResourcesOrLocal(string path, string multiVersionControlRelativePath, string prefixPath, FileDefine.FileExtension extension)
        {
            //default load from resource
            bool retValue = false;
            var type = typeof(UnityEngine.Object);

#if RESOURCES_FIRST
            if (!retValue)
                retValue = _LoadFromResources(path, type) != null;

            if (!retValue)
            {
                retValue = ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension);
            }
#else
            if (!retValue)
            {
                retValue = ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension);
            }

            if (!retValue)
                retValue = _LoadFromResources(path, multiVersionControlRelativePath, type, extension) != null;
#endif
            return retValue;
        }

        static public bool ExistsLocal(string path, string multiVersionControlRelativePath = "")
        {
            return ExistsLocal(path, multiVersionControlRelativePath, DEFAULT_PREFIX_PATH_LOWER, DEFAULT_EXTENSION);
        }

        static public bool ExistsLocal(string path, string multiVersionControlRelativePath, string prefixPath, FileDefine.FileExtension extension)
        {
            var pathAssetBundle = string.Empty;
            GetAssetBundlePathAndFileName(path, prefixPath, ref pathAssetBundle, extension);
            return HotUpdateDataCache.ExistsAssetbundle(pathAssetBundle, multiVersionControlRelativePath);
        }
    }
}
