using System.Collections;
using shaco.Base;

namespace shaco
{
    public partial class ResourcesEx
    {
        //查看路径中是否包含了版本管理路径，如果有则拆分开来
        static private void CheckMultiVersionControlPathSplit(ref string path, ref string multiVersionControlRelativePath)
        {
            var versionControlTag = HotUpdateHelper.GetAssetBundlePathTagPlatform(HotUpdateHelper.GetAssetBundleAutoPlatform());
            if (path.Contains(versionControlTag))
            {
                var splitPaths = path.Split(versionControlTag);

                if (splitPaths.Length == 2)
                {
                    path = splitPaths[1];
                    multiVersionControlRelativePath = splitPaths[0];

                    if (!string.IsNullOrEmpty(path) && path[0].ToString() == FileDefine.PATH_FLAG_SPLIT)
                    {
                        path = path.RemoveFront(FileDefine.PATH_FLAG_SPLIT);
                    }
                }
                else 
                {
                    Log.Error("ResourcesEx CheckMultiVersionControlPathSplit error: unsupport split type path=" + path);
                }
            }
        }

        static private UnityEngine.Object _LoadResourcesOrLocal(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, FileDefine.FileExtension extension)
        {
            //default load from resource
            UnityEngine.Object ret = null;

            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);

#if RESOURCES_FIRST
            if (ret.IsNull())
                ret = _LoadFromResources(path, multiVersionControlRelativePath, type, extension);

            if (ret.IsNull() && ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension))
                ret = _LoadFromLocal(path, multiVersionControlRelativePath, prefixPath, type, extension);
#else
            if (ret.IsNull() && ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension))
                ret = _LoadFromLocal(path, multiVersionControlRelativePath, prefixPath, type, extension);

            if (ret.IsNull())
                ret = _LoadFromResources(path, multiVersionControlRelativePath, type, extension);
#endif
            if (ret.IsNull())
            {
                shaco.Log.Error("_LoadResourcesOrLocal error: missing path=" + multiVersionControlRelativePath.ContactPath(path));
            }

            return ret;
        }

        static private UnityEngine.Object _LoadFromResources(string path, string multiVersionControlRelativePath, System.Type type, FileDefine.FileExtension extension)
        {
            UnityEngine.Object ret = null;
            var fullPath = FileHelper.ContactPath(multiVersionControlRelativePath, path);

            if (extension == FileDefine.FileExtension.Lua)
            {
                fullPath = FileHelper.AddExtensions(fullPath, FileHelper.GetExtension(FileDefine.FileExtension.Txt));
            }

            //default load from resource
            if (FileHelper.HasFileNameExtension(fullPath))
            {
                ret = UnityEngine.Resources.Load(FileHelper.RemoveExtension(fullPath), type);
            }
            else
            {
                ret = UnityEngine.Resources.Load(fullPath, type);
            }

            //load internal from resource
            if (ret.IsNull())
            {
                var pathPrevLevel = FileHelper.RemoveLastPathByLevel(fullPath, 1);
                var loadAll = UnityEngine.Resources.LoadAll(pathPrevLevel);
                if (null != loadAll && loadAll.Length > 0)
                {
                    ret = GetResourceWithInternal<UnityEngine.Object>(loadAll, fullPath);
                }
            }
            return ret;
        }

        static private UnityEngine.Object _LoadFromLocal(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, FileDefine.FileExtension extension)
        {
            UnityEngine.Object ret = null;
            var pathAssetBundle = string.Empty;
            GetAssetBundlePathAndFileName(path, prefixPath, ref pathAssetBundle, extension);

            if (GetResourceStatus(pathAssetBundle, multiVersionControlRelativePath) == ResourceStatus.InternalResource)
            {
                //load internal resource from assetbundle
                pathAssetBundle = FileHelper.RemoveLastPathByLevel(pathAssetBundle, 1);
                path = FileHelper.GetLastFileName(path);
                ret = shaco.HotUpdateDataCache.Read(pathAssetBundle, multiVersionControlRelativePath, path, type);
            }
            else
            {
                //load from assetbundle
                ret = shaco.HotUpdateDataCache.Read(pathAssetBundle, multiVersionControlRelativePath, path, type);
            }
            return ret;
        }

        static private UnityEngine.Object[] _LoadAllResourcesOrLocal(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, FileDefine.FileExtension extension)
        {
            UnityEngine.Object[] ret = null;

            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);

#if RESOURCES_FIRST
            if (ret.IsNullOrEmpty())
                ret = _LoadAllFromResources(path, multiVersionControlRelativePath, type);

            if (ret.IsNullOrEmpty() && ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension))
                ret = _LoadAllFromLocal(path, multiVersionControlRelativePath, prefixPath, extension, type);
#else
            if (ret.IsNullOrEmpty() && ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension))
                ret = _LoadAllFromLocal(path, multiVersionControlRelativePath, prefixPath, extension, type);

            if (ret.IsNullOrEmpty())
                ret = _LoadAllFromResources(path, multiVersionControlRelativePath, type);
#endif
            if (ret.IsNullOrEmpty())
            {
                shaco.Log.Error("_LoadAllResourcesOrLocal error: missing path=" + multiVersionControlRelativePath.ContactPath(path));
            }
            return ret;
        }

        static private UnityEngine.Object[] _LoadAllFromResources(string path, string multiVersionControlRelativePath, System.Type type)
        {
            if (FileHelper.HasFileNameExtension(path))
                path = FileHelper.RemoveExtension(path);
            path = multiVersionControlRelativePath.ContactPath(path);    
            return UnityEngine.Resources.LoadAll(path, type);
        }

        static private UnityEngine.Object[] _LoadAllFromLocal(string path, string multiVersionControlRelativePath, string prefixPath, FileDefine.FileExtension extension, System.Type type)
        {
            var pathAssetBundle = string.Empty;
            GetAssetBundlePathAndFileName(path, prefixPath, ref pathAssetBundle, extension);
            var retValue =  shaco.HotUpdateDataCache.ReadAll(pathAssetBundle, multiVersionControlRelativePath, type);
            return retValue;
        }

        static private void _LoadResourcesOrLocalAsync(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress, FileDefine.FileExtension extension)
        {
            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);

#if RESOURCES_FIRST
            _LoadFromResourcesAsync(path, multiVersionControlRelativePath, prefixPath, type, (UnityEngine.Object readObj)=>
            {
                if (readObj.IsNull())
                {
                    _LoadFromLocalAsync(path, multiVersionControlRelativePath, prefixPath, type, (UnityEngine.Object obj) =>
                    {
                        if (obj.IsNull())
                        {
                            shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync erorr: missing path=" + multiVersionControlRelativePath.ContactPath(path));
                        }
                        callbackLoadEnd(obj);
                    }, callbackProgress, extension);
                }
                else
                {
                    callbackLoadEnd(readObj);
                }
            }, callbackProgress);
#else
            if (ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension) || !HotUpdateHelper.GetDynamicResourceAddress().IsNullOrEmpty())
            {
                _LoadFromLocalAsync(path, multiVersionControlRelativePath, prefixPath, type, (UnityEngine.Object readObj) =>
                {
                    if (readObj.IsNull())
                    {
                        _LoadFromResourcesAsync(path, multiVersionControlRelativePath, prefixPath, type, (UnityEngine.Object obj) =>
                        {
                            if (obj.IsNull())
                            {
                                shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync erorr: missing path=" + multiVersionControlRelativePath.ContactPath(path));
                            }
                            callbackLoadEnd(obj);
                        }, callbackProgress);
                    }
                    else 
                    {
                        callbackLoadEnd(readObj);
                    }
                }, callbackProgress, extension);
            }
            else 
            {
                _LoadFromResourcesAsync(path, multiVersionControlRelativePath, prefixPath, type, (UnityEngine.Object obj) =>
                {
                    if (obj.IsNull())
                    {
                        shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync erorr: missing path=" + multiVersionControlRelativePath.ContactPath(path));
                    }
                    callbackLoadEnd(obj);
                }, callbackProgress);
            }
#endif
        }

        static private void _LoadFromResourcesAsync(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            var request = UnityEngine.Resources.LoadAsync(multiVersionControlRelativePath.ContactPath(FileHelper.RemoveExtension(path)));

            shaco.Base.WaitFor.Run(() =>
            {
                if (null != callbackProgress && (request.isDone && null != request.asset))
                {
                    callbackProgress(request.progress);
                }
                return request.isDone;
            }, () =>
            {
                if (request.asset.IsNull())
                {
                    var pathPrevLevel = FileHelper.RemoveLastPathByLevel(path, 1);
                    ActionS.GetDelegateMonoBehaviour().StartCoroutine(_LoadAllFromResourcesAsync(pathPrevLevel, multiVersionControlRelativePath, type, (UnityEngine.Object[] loadObjs) =>
                    {
                        //load internal from resource
                        var loadObj = GetResourceWithInternal<UnityEngine.Object>(loadObjs, path);
                        callbackLoadEnd(loadObj);
                    }, callbackProgress));
                }
                else
                {
                    //default load from resource
                    callbackLoadEnd(request.asset);
                }
            });
        }

        static private void _LoadFromLocalAsync(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress, FileDefine.FileExtension extension)
        {
            var pathAssetBundle = string.Empty;
            GetAssetBundlePathAndFileName(path, prefixPath, ref pathAssetBundle, extension);

            if (GetResourceStatus(pathAssetBundle, multiVersionControlRelativePath) == ResourceStatus.InternalResource)
            {
                //load internal resource from assetbundle
                pathAssetBundle = FileHelper.RemoveLastPathByLevel(pathAssetBundle, 1);
                path = FileHelper.GetLastFileName(path);
                shaco.HotUpdateDataCache.ReadAsync(pathAssetBundle, multiVersionControlRelativePath, path, type, (UnityEngine.Object loadObjTmp) =>
                {
                    callbackLoadEnd(loadObjTmp);
                }, callbackProgress);
            }
            else
            {
                //load from assetbundle
                shaco.HotUpdateDataCache.ReadAsync(pathAssetBundle, multiVersionControlRelativePath, path, type, (UnityEngine.Object loadObjTmp) =>
                {
                    callbackLoadEnd(loadObjTmp);
                }, callbackProgress);
            }
        }

        static private void _LoadAllResourcesOrLocalAsync(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, shaco.Base.FileDefine.FileExtension extension, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);

#if RESOURCES_FIRST
            ActionS.GetDelegateMonoBehaviour().StartCoroutine(_LoadAllFromResourcesAsync(path, multiVersionControlRelativePath, type, (UnityEngine.Object[] loadObjs) =>
            {
                if (loadObjs.IsNullOrEmpty())
                {
                    _LoadAllFromLocalAsync(path, multiVersionControlRelativePath, prefixPath, type, (UnityEngine.Object[] objs) =>
                    {
                        if (objs.IsNull())
                        {
                            shaco.Log.Error("_LoadAllResourcesOrLocalAsync error: missing path=" + multiVersionControlRelativePath.ContactPath(path));
                        }
                        callbackLoadEnd(objs);
                    }, callbackProgress);
                }
                else
                {
                    callbackLoadEnd(loadObjs);
                }
            }, callbackProgress));
#else
            if (ExistsLocal(path, multiVersionControlRelativePath, prefixPath, extension) || !HotUpdateHelper.GetDynamicResourceAddress().IsNullOrEmpty())
            {
                _LoadAllFromLocalAsync(path, multiVersionControlRelativePath, prefixPath, type, extension, (UnityEngine.Object[] loadObjs) =>
                {
                    if (loadObjs.IsNull())
                    {
                        ActionS.GetDelegateMonoBehaviour().StartCoroutine(_LoadAllFromResourcesAsync(path, multiVersionControlRelativePath, type, (UnityEngine.Object[] objs) =>
                        {
                            if (objs.IsNullOrEmpty())
                            {
                                shaco.Log.Error("_LoadAllResourcesOrLocalAsync error: missing path=" + multiVersionControlRelativePath.ContactPath(path));
                            }
                            callbackLoadEnd(objs);
                        }, callbackProgress));
                    }
                    else 
                    {
                        callbackLoadEnd(loadObjs);
                    }
                }, callbackProgress);
            }
            else 
            {
                ActionS.GetDelegateMonoBehaviour().StartCoroutine(_LoadAllFromResourcesAsync(path, multiVersionControlRelativePath, type, (UnityEngine.Object[] objs) =>
                {
                    if (objs.IsNullOrEmpty())
                    {
                        shaco.Log.Error("_LoadAllResourcesOrLocalAsync error: missing path=" + multiVersionControlRelativePath.ContactPath(path));
                    }
                    callbackLoadEnd(objs);
                }, callbackProgress));
            }
#endif
        }

        static private IEnumerator _LoadAllFromResourcesAsync(string path, string multiVersionControlRelativePath, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            var loadObjs = UnityEngine.Resources.LoadAll(multiVersionControlRelativePath.ContactPath(path), type);
            yield return new UnityEngine.WaitForFixedUpdate();

            if (null != callbackProgress && !loadObjs.IsNullOrEmpty())
            {
                callbackProgress(1);
            }
            callbackLoadEnd(loadObjs);
        }

        static private void _LoadAllFromLocalAsync(string path, string multiVersionControlRelativePath, string prefixPath, System.Type type, shaco.Base.FileDefine.FileExtension extension, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackLoadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            var pathAssetBundle = string.Empty;
            GetAssetBundlePathAndFileName(path, prefixPath, ref pathAssetBundle, extension);
            shaco.HotUpdateDataCache.ReadAllAsync(pathAssetBundle, multiVersionControlRelativePath, type, (UnityEngine.Object[] objs) =>
            {
                callbackLoadEnd(objs);
            }, callbackProgress);
        }

        static private void GetAssetBundlePathAndFileName(string inPath, string prefixPath, ref string outPath, FileDefine.FileExtension extension)
        {
            //check input path extension
            if (!inPath.Contains(FileDefine.DOT_SPLIT))
            {
                if (extension != FileDefine.FileExtension.None)
                {
                    inPath = FileHelper.AddExtensions(inPath, FileHelper.GetExtension(extension));
                }
            }
            else
            {
                if (extension == FileDefine.FileExtension.None)
                {
                    inPath = FileHelper.RemoveExtension(inPath);
                }
            }

            if (!string.IsNullOrEmpty(prefixPath))
            {
                outPath = FileHelper.ContactPath(prefixPath, inPath.ToLower());
            }
            else
                outPath = inPath;
        }

        static private void GetAssetBundlePathAndFileName(string inPath, ref string outPath, FileDefine.FileExtension extension)
        {
            GetAssetBundlePathAndFileName(inPath, DEFAULT_PREFIX_PATH_LOWER, ref outPath, extension);
        }

        static private T GetResourceWithInternal<T>(T[] loadObjs, string name) where T : UnityEngine.Object
        {
            T retValue = null;
            name = FileHelper.GetLastFileName(name);
            for (int i = 0; i < loadObjs.Length; ++i)
            {
                if (name == loadObjs[i].name)
                {
                    retValue = loadObjs[i];
                    break;
                }
            }
            return retValue;
        }

        private enum ResourceStatus
        {
            DefaultResource,
            InternalResource,
            NotFoundResource
        }

        static private ResourceStatus GetResourceStatus(string relativePath, string multiVersionControlRelativePath)
        {
            var fullPathTmp = GetAssetbundleFullPath(relativePath, multiVersionControlRelativePath);
            if (!FileHelper.ExistsFile(fullPathTmp))
            {
                var subPath = FileHelper.RemoveLastPathByLevel(fullPathTmp, 1);
                subPath = FileHelper.AddExtensions(subPath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);

                if (FileHelper.ExistsFile(subPath))
                {
                    return ResourceStatus.InternalResource;
                }
                else
                {
                    return ResourceStatus.NotFoundResource;
                }
            }
            return ResourceStatus.DefaultResource;
        }

        static private string GetAssetbundleFullPath(string relativePath, string multiVersionControlRelativePath)
        {
            var pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(HotUpdateHelper.AssetBundlePathToKey(relativePath), multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            return FileHelper.GetFullpath(pathConvert);
        }
    }
}
