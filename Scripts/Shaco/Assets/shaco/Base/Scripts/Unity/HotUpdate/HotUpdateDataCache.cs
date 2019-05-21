using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using shaco.Base;

namespace shaco
{
    public class HotUpdateDataCache
    {
        static Dictionary<string, DataCache> _mapDataCache = new Dictionary<string, DataCache>();

        //加载Assetbundle所占进度百分比
        static private readonly float LOAD_ASSETBUNDLE_PERCENT = 0.7f;

        public class DataCache
        {
            //是否正在加载
            public bool isLoading = false;
            public HotUpdateImportMemory hotUpdateDelMemory = new HotUpdateImportMemory();
            public List<System.Action<DataCache>> listCallBackReadEnd = new List<System.Action<DataCache>>();
        }

        static public Object Read(string pathAssetBundle, string multiVersionControlRelativePath, string fileName)
        {
            return GetDataCacheWithAutoCreate(pathAssetBundle, multiVersionControlRelativePath).hotUpdateDelMemory.Read(fileName);
        }

        static public Object Read(string pathAssetBundle, string multiVersionControlRelativePath, string fileName, System.Type type)
        {
            return GetDataCacheWithAutoCreate(pathAssetBundle, multiVersionControlRelativePath).hotUpdateDelMemory.Read(fileName, type);
        }

        static public T Read<T>(string pathAssetBundle, string multiVersionControlRelativePath, string fileName) where T : UnityEngine.Object
        {
            return GetDataCacheWithAutoCreate(pathAssetBundle, multiVersionControlRelativePath).hotUpdateDelMemory.Read<T>(fileName);
        }

        static public Object[] ReadAll(string pathAssetBundle, string multiVersionControlRelativePath, System.Type type)
        {
            Object[] ret = null;
            var pathConvert = HotUpdateHelper.AssetBundlePathToKey(pathAssetBundle);
            pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(pathConvert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            var fullPath = FileHelper.GetFullpath(pathConvert);

            //read from one assetbundle
            if (FileHelper.ExistsFile(fullPath))
            {
                ret = GetDataCacheWithAutoCreate(pathAssetBundle, multiVersionControlRelativePath).hotUpdateDelMemory.ReadAll(type);
            }
            //read from directory
            else
            {
                var extensionTmp = FileHelper.GetFilNameExtension(fullPath);
                if (string.IsNullOrEmpty(extensionTmp))
                {
                    Log.Error("HotUpdateDataCache ReadAll error: read from directory path not have extension, path=" + fullPath);
                }
                else
                {
                    fullPath = FileHelper.RemoveExtension(fullPath);
                    if (FileHelper.ExistsDirectory(fullPath))
                    {
                        List<string> listPath = new List<string>();
                        var strVersionFolder = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                        var strPrefix = FileHelper.ContactPath(FileHelper.GetFullpath(string.Empty), strVersionFolder);

                        FileHelper.GetSeekPath(fullPath, ref listPath, extensionTmp);
                        ret = new Object[listPath.Count];

                        for (int i = 0; i < listPath.Count; ++i)
                        {
                            var filenameAssetBundle = listPath[i].Remove(strPrefix);
                            ret[i] = Read(filenameAssetBundle, multiVersionControlRelativePath, FileHelper.RemoveExtension(filenameAssetBundle), type);
                        }
                    }
                }
            }

            if (ret == null || ret.Length == 0)
            {
                Debug.LogError("HotUpdate ReadAll error: not a assetbundle path=" + fullPath);
            }

            return ret;
        }

        static public Object[] ReadAll(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            return ReadAll(pathAssetBundle, multiVersionControlRelativePath, typeof(UnityEngine.Object));
        }

        static public Object[] ReadAll<T>(string pathAssetBundle, string multiVersionControlRelativePath) where T : UnityEngine.Object
        {
            return ReadAll(pathAssetBundle, multiVersionControlRelativePath, typeof(T));
        }

        static public string ReadString(string pathAssetBundle, string multiVersionControlRelativePath, string filename)
        {
            return HotUpdateHelper.AssetToString(Read(pathAssetBundle, multiVersionControlRelativePath, filename));
        }

        static public void ReadAsync(string pathAssetBundle, string multiVersionControlRelativePath, string filename, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            GetDataCacheWithAutoCreateAsync(pathAssetBundle, multiVersionControlRelativePath, (DataCache dataCache) =>
            {
                dataCache.hotUpdateDelMemory.ReadAsync(filename, type, callbackReadEnd, (float percent) =>
                {
                    if (null != callbackProgress)
                    {
                        callbackProgress(percent * (1 - LOAD_ASSETBUNDLE_PERCENT) + LOAD_ASSETBUNDLE_PERCENT);
                    }
                });
            }, callbackProgress);
        }

        static public void ReadAsync<T>(string pathAssetBundle, string multiVersionControlRelativePath, string filename, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress) where T : UnityEngine.Object
        {
            ReadAsync(pathAssetBundle, multiVersionControlRelativePath, filename, typeof(T), callbackReadEnd, callbackProgress);
        }

        static public void ReadAsync(string pathAssetBundle, string multiVersionControlRelativePath, string filename, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            GetDataCacheWithAutoCreateAsync(pathAssetBundle, multiVersionControlRelativePath, (DataCache dataCache) =>
            {
                dataCache.hotUpdateDelMemory.ReadAsync(filename, typeof(UnityEngine.Object), callbackReadEnd, (float percent) =>
                {
                    if (null != callbackProgress)
                    {
                        callbackProgress(percent * (1 - LOAD_ASSETBUNDLE_PERCENT) + LOAD_ASSETBUNDLE_PERCENT);
                    }
                });
            }, callbackProgress);
        }

        static public void ReadAllAsync(string pathAssetBundle, string multiVersionControlRelativePath, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            var pathConvert = HotUpdateHelper.AssetBundlePathToKey(pathAssetBundle);
            pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(pathConvert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            var fullPath = FileHelper.GetFullpath(pathConvert);

            if (FileHelper.ExistsFile(fullPath))
            {
                GetDataCacheWithAutoCreateAsync(pathAssetBundle, multiVersionControlRelativePath, (DataCache dataCache) =>
                {
                    dataCache.hotUpdateDelMemory.ReadAllAsync(type, callbackReadEnd, (float percent) =>
                    {
                        if (null != callbackProgress)
                        {
                            callbackProgress(percent * (1 - LOAD_ASSETBUNDLE_PERCENT) + LOAD_ASSETBUNDLE_PERCENT);
                        }
                    });
                }, callbackProgress);
            }
            else
            {
                var extensionTmp = FileHelper.GetFilNameExtension(fullPath);
                if (string.IsNullOrEmpty(extensionTmp))
                {
                    Log.Error("HotUpdateDataCache ReadAll error: read from directory path not have extension, path=" + fullPath);
                }
                else
                {
                    fullPath = FileHelper.RemoveExtension(fullPath);
                    if (FileHelper.ExistsDirectory(fullPath))
                    {
                        List<string> listPath = new List<string>();
                        var strVersionFolder = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                        var strPrefix = FileHelper.ContactPath(FileHelper.GetFullpath(string.Empty), strVersionFolder);

                        FileHelper.GetSeekPath(fullPath, ref listPath, extensionTmp);
                        var loadedObjs = new List<Object>();

                        if (listPath.Count > 0)
                        {
                            ReadAllAsyncLoop(listPath, loadedObjs, strPrefix, multiVersionControlRelativePath, 0, callbackReadEnd, callbackProgress);
                        }
                        else
                        {
                            callbackReadEnd(new Object[0]);
                        }
                    }
                    else
                    {
                        callbackReadEnd(null);
                    }
                }
            }
        }

        static public void ReadAllAsync<T>(string pathAssetBundle, string multiVersionControlRelativePath, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress) where T : UnityEngine.Object
        {
            ReadAllAsync(pathAssetBundle, multiVersionControlRelativePath, typeof(T), callbackReadEnd, callbackProgress);
        }

        static public void ReadAllAsync(string pathAssetBundle, string multiVersionControlRelativePath, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            ReadAllAsync(pathAssetBundle, multiVersionControlRelativePath, typeof(UnityEngine.Object), callbackReadEnd, callbackProgress);
        }

        static public void ReadStringAsync(string pathAssetBundle, string multiVersionControlRelativePath, string filename, HotUpdateDefine.CALL_FUNC_READ_STRING callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            GetDataCacheWithAutoCreateAsync(pathAssetBundle, multiVersionControlRelativePath, (DataCache dataCache) =>
            {
                dataCache.hotUpdateDelMemory.ReadStringAsync(filename, callbackReadEnd, (float percent) =>
                {
                    if (null != callbackProgress)
                    {
                        callbackProgress(percent * (1 - LOAD_ASSETBUNDLE_PERCENT) + LOAD_ASSETBUNDLE_PERCENT);
                    }
                });
            }, callbackProgress);
        }

        static public void Unload(bool unloadAllLoadedObjects = false)
        {
            foreach (var key in _mapDataCache.Keys)
            {
                var value = _mapDataCache[key];
                value.hotUpdateDelMemory.Close(unloadAllLoadedObjects);
            }

            _mapDataCache.Clear();

#if UNITY_5_3_OR_NEWER
            HotUpdateManifest.Unload();
#endif
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        static public bool IsLoadedCache(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            return _mapDataCache.ContainsKey(key);
        }

        static public void AddUpdateCache(string pathAssetBundle, string multiVersionControlRelativePath, HotUpdateImportMemory updateDelegate)
        {
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            if (_mapDataCache.ContainsKey(key))
            {
                return;
            }

            var newItem = new DataCache();
            newItem.hotUpdateDelMemory = updateDelegate;

            AddDataCache(key, newItem);
        }

        static public bool UnloadAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath, bool unloadAllLoadedObjects)
        {
            DataCache findDateCache = null;
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);

            if (!_mapDataCache.TryGetValue(key, out findDateCache))
            {
#if DEBUG_LOG
                var relativePath = key.Remove(ResourcesEx.DEFAULT_PREFIX_PATH_LOWER);
                if (FileHelper.GetFilNameExtension(relativePath) == FileHelper.GetExtension(FileDefine.FileExtension.Lua))
                {
                    relativePath = FileHelper.AddExtensions(relativePath, FileHelper.GetExtension(FileDefine.FileExtension.Txt));
                }
                relativePath = FileHelper.RemoveExtension(relativePath);
                relativePath = relativePath.Remove(HotUpdateHelper.GetAssetBundlePathTagPlatform(HotUpdateHelper.GetAssetBundleAutoPlatform()));

                var loadObjTmp = UnityEngine.Resources.Load(relativePath);
                if (null != loadObjTmp)
                {
                    shaco.Log.Error("HotUpdateDataCache UnloadAssetBundle error: The resource is not 'AssetBundle' but is loaded from the 'Resources' directory" + " relative path=" + relativePath);
                    return false;
                }
#endif
                shaco.Log.Error("HotUpdateDataCache UnloadAssetBundle error: not find assetbundle by path=" + key);
                return false;
            }

            findDateCache.hotUpdateDelMemory.Close(unloadAllLoadedObjects);
            _mapDataCache.Remove(key);
            return true;
        }

        static public bool ExistsAssetbundle(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var pathConvert = HotUpdateHelper.AssetBundlePathToKey(pathAssetBundle);
            pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(pathConvert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            pathConvert = HotUpdateHelper.AssetBundleKeyToPath(pathConvert);
            var fullPath = FileHelper.GetFullpath(pathConvert);
            return FileHelper.ExistsFile(fullPath);
        }

        static public bool IsLoadedAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            return _mapDataCache.ContainsKey(key);
        }

        static public string GetFullAssetBundleKey(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var retValue = pathAssetBundle.ToLower();
            if (!retValue.StartsWith(multiVersionControlRelativePath) && !FileHelper.ExistsFile(retValue))
            {
                retValue = HotUpdateHelper.GetVersionControlFolderAuto(HotUpdateHelper.AssetBundlePathToKey(retValue), multiVersionControlRelativePath);
            }
            if (!FileHelper.HasFileNameExtension(retValue))
            {
                retValue = FileHelper.AddExtensions(retValue, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            }
            return retValue;
        }

        /// <summary>
        /// 循环加载文件夹中所有文件资源
        /// </summary>
        static private void ReadAllAsyncLoop(List<string> listPath, List<Object> loadedObjs, string strPrefix, string multiVersionControlRelativePath, int index, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            //没有文件需要加载
            if (listPath.IsNullOrEmpty())
            {
                if (null != callbackReadEnd)
                {
                    callbackReadEnd(null);
                }
                return;
            }

            var filenameAssetBundle = listPath[index].Remove(strPrefix);
            ReadAsync(filenameAssetBundle, multiVersionControlRelativePath, FileHelper.RemoveExtension(filenameAssetBundle), (Object readObj) =>
            {
                //当前文件加载完毕
                if (!readObj.IsNull())
                    loadedObjs.Add(readObj);

                //准备加载下一个
                ++index;

                //没有下一个文件了，加载完毕
                if ((index < 0 || index > listPath.Count - 1) && null != callbackReadEnd)
                {
                    callbackReadEnd(loadedObjs.ToArray());
                }
                //继续加载下一个文件
                else
                {
                    ReadAllAsyncLoop(listPath, loadedObjs, strPrefix, multiVersionControlRelativePath, index, callbackReadEnd, callbackProgress);
                }

            }, (float percent) =>
            {
                if (null != callbackProgress)
                {
                    callbackProgress((1.0f / listPath.Count * index) + ((float)percent / (float)listPath.Count));
                }
            });
        }

        static private DataCache GetDataCacheWithAutoCreate(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            if (!FileHelper.HasFileNameExtension(pathAssetBundle))
            {
                pathAssetBundle = FileHelper.AddExtensions(pathAssetBundle, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            }
            var fullkey = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);

            DataCache ret = null;
            if (!_mapDataCache.ContainsKey(fullkey))
            {
                ret = new DataCache();
                ret.isLoading = true;
                ret.hotUpdateDelMemory.CreateByMemoryAutoPlatform(pathAssetBundle, multiVersionControlRelativePath);

                AddDataCache(fullkey, ret);
                ret.isLoading = false;
            }
            else
            {
                ret = _mapDataCache[fullkey];

                //如果对象正在loading，是无法获取数据的
                if (ret.isLoading)
                {
                    Log.Error("HotUpdate GetDataCacheWithAutoCreate error: data is loading, please wait... path=" + pathAssetBundle + " multiVersionControlRelativePath=" + multiVersionControlRelativePath);
                    ret = new DataCache();
                }
            }
            return ret;
        }

        static private void AddDataCache(string key, DataCache data)
        {
            //只添加有效资源到缓存
            if (data.hotUpdateDelMemory.IsValidAsset())
            {
                if (_mapDataCache.ContainsKey(key))
                {
                    Log.Error("HotUpdate AddDataCache error: duplicate key=" + key);
                }
                else
                {
                    _mapDataCache.Add(key, data);
                }
            }
        }

        static private void GetDataCacheWithAutoCreateAsync(string pathAssetBundle, string multiVersionControlRelativePath, System.Action<DataCache> callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);

            DataCache ret = null;
            if (!_mapDataCache.ContainsKey(key))
            {
                ret = new DataCache();

                //添加资源缓存
                ret.isLoading = true;
                _mapDataCache.Add(key, ret);

                ret.listCallBackReadEnd.Add(callbackLoadEnd);
                ret.hotUpdateDelMemory.onProcessEnd.AddCallBack(ret.hotUpdateDelMemory, (object sender) =>
                {
                    ret.isLoading = false;

                    //本地资源异步加载失败，清理缓存
                    if (!ret.hotUpdateDelMemory.IsCompleted())
                    {
                        _mapDataCache.Remove(key);
                    }

                    for (int i = 0; i < ret.listCallBackReadEnd.Count; ++i)
                    {
                        ret.listCallBackReadEnd[i](ret);
                    }

                    bool isLocked = true;
                    do
                    {
                        lock (ret)
                        {
                            ret.listCallBackReadEnd.Clear();
                            isLocked = false;
                        }
                    }
                    while (isLocked);
                });

                if (callbackProgress != null)
                {
                    ret.hotUpdateDelMemory.onProcessIng.AddCallBack(ret, (object defaultSender) =>
                    {
                        callbackProgress(ret.hotUpdateDelMemory.GetLoadProgress() * LOAD_ASSETBUNDLE_PERCENT);
                    });
                }

                ret.hotUpdateDelMemory.CreateByMemoryAsyncAutoPlatform(pathAssetBundle, multiVersionControlRelativePath);
            }
            else
            {
                ret = _mapDataCache[key];

                //如果目标正在加载，则需要等待一下，以免资源使用发生冲突
                if (ret.isLoading)
                {
                    shaco.WaitFor.Run(() =>
                    {
                        return !ret.isLoading;
                    }, () =>
                    {
                        GetDataCacheAsync(ret, callbackLoadEnd);
                    });
                }
                else
                {
                    GetDataCacheAsync(ret, callbackLoadEnd);
                }
            }
        }

        /// <summary>
        /// 异步获取缓存对象
        /// </summary>
        static private void GetDataCacheAsync(DataCache dataCache, System.Action<DataCache> callbackLoadEnd)
        {
            bool isLocked = true;
            do
            {
                if (dataCache.listCallBackReadEnd.Count > 0)
                {
                    lock (dataCache)
                    {
                        dataCache.listCallBackReadEnd.Add(callbackLoadEnd);
                        isLocked = false;
                    }
                }
                else
                {
                    callbackLoadEnd(dataCache);
                    isLocked = false;
                }
            } while (isLocked);
        }
    }
}