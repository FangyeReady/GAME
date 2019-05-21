#if HOTFIX_ENABLE

using UnityEngine;
using System.Collections;
using System.IO;
using XLua;
using System.Collections.Generic;
using System.Linq;

namespace shaco
{
    public class XLuaManager
    {
        public static class HotfixCfg
        {
            static private string[] ignore_flags = new string[] { "editor", "xlua", "GameFramework" };

            [LuaCallCSharp]
            static private List<System.Type> by_extensions_class
            {
                get
                {
                    return (from type in System.Reflection.Assembly.Load("Assembly-CSharp").GetTypes()
                            where type.IsAbstract && type.IsSealed && IsIgnoreFlagType(type)
                            select type).ToList();
                }
            }

            [Hotfix]
            static private List<System.Type> by_all_type
            {
                get
                {
                    return (from type in System.Reflection.Assembly.Load("Assembly-CSharp").GetTypes()
                            where !type.IsAbstract && !type.IsSealed && IsIgnoreFlagType(type)
                            select type).ToList();
                }
            }

            [CSharpCallLua]
            static private List<System.Type> by_generic_type 
            {
                get 
                {
                    return new List<System.Type>
                        { 
                            typeof(System.Action<object>),
                            typeof(shaco.Base.EventCallBack<object>.CALL_FUNC_EVENT)
                        };
                }
            }

            /// <summary>
            /// 是否为Xlua热更新忽略的类型
            /// <param name="type">类型</param>
            /// <return>是否需要忽略</return>
            /// </summary>
            static private bool IsIgnoreFlagType(System.Type type)
            {
                bool retValue = type.IsPublic && !type.IsInterface && !type.IsEnum;
                if (retValue)
                {
                    var lowerTypeName = type.FullName.ToLower();
                    for (int i = ignore_flags.Length - 1; i >= 0; --i)
                    {
                        retValue = !lowerTypeName.Contains(ignore_flags[i].ToLower());
                        if (!retValue)
                        {
                            break;
                        }
                    }
                }
                return retValue;
            }
        }

        //lua状态机
        private LuaEnv _luaenv = new LuaEnv();

        /// <summary>
        /// 运行目录下所有lua脚本
        /// </summary>
        /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="callbackEnd">加载lua脚本完毕后回调</param>
        /// <param name="extension">资源后缀名，如果为None则默认以AssetBundle资源进行加载</param>
        static public void RunWithFolder(string path, System.Action callbackEnd, string multiVersionControlRelativePath = "", shaco.Base.FileDefine.FileExtension extension = shaco.Base.FileDefine.FileExtension.None)
        {
            ResourcesEx.LoadAllResourcesOrLocalAsync(path, extension, (Object[] readObjs) =>
            {
                Log.Info("XLuaManager RunWithFolder: path=" + path);

                //如果存在下载目录，则自动销毁ab包资源
                var fullPath = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                fullPath = shaco.Base.FileHelper.GetFullpath(fullPath);
                fullPath = fullPath.ContactPath(ResourcesEx.DEFAULT_PREFIX_PATH_LOWER.ContactPath(path));

                //如果存在下载目录，则从异步线程加载
                if (shaco.Base.FileHelper.ExistsDirectory(fullPath))
                {
                    shaco.Base.Coroutine.ForeachAsync(readObjs, (object obj) =>
                    {
                        var pathOneFile = obj.ToString();
                        RunWithString(pathOneFile);
                        return true;
                    }, (float percent) =>
                    {
                        //加载完毕
                        if (percent >= 1.0f)
                        {
                            //如果存在下载目录，则自动销毁ab包资源
                            var allFiles = new List<string>();
                            shaco.Base.FileHelper.GetSeekPath(fullPath, ref allFiles, shaco.Base.FileHelper.GetExtension(shaco.Base.FileDefine.FileExtension.Lua));
                            var removePrefixPath = shaco.Base.FileHelper.GetFullpath(string.Empty);
                            removePrefixPath = removePrefixPath.ContactPath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath));

                            for (int i = allFiles.Count - 1; i >= 0; --i)
                            {
                                var pathTmp = allFiles[i];
                                pathTmp = pathTmp.Remove(removePrefixPath);
                                shaco.HotUpdateDataCache.UnloadAssetBundle(pathTmp, multiVersionControlRelativePath, true);
                            }

                            if (null != callbackEnd)
                            {
                                System.GC.Collect();
                                callbackEnd();
                            }
                        }
                    });
                }
                //如果加载unity目录，则从协程加载
                else
                {
                    shaco.Base.Coroutine.Foreach(readObjs, (object obj) =>
                    {
                        var pathOneFile = obj.ToString();
                        RunWithString(pathOneFile);
                        return true;
                    }, (float percent) =>
                    {
                        //加载完毕
                        if (percent >= 1.0f)
                        {
                            if (null != callbackEnd)
                            {
                                System.GC.Collect();
                                callbackEnd();
                            }
                        }
                    });
                }

            }, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 运行目录下所有lua脚本
        /// </summary>
        /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="extension">资源后缀名，如果为None则默认以AssetBundle资源进行加载</param>
        static public void RunWithFolder(string path, string multiVersionControlRelativePath = "", shaco.Base.FileDefine.FileExtension extension = shaco.Base.FileDefine.FileExtension.None)
        {
            RunWithFolder(path, null, multiVersionControlRelativePath, extension);
        }

        /// <summary>
        /// 运行一个lua脚本
        /// </summary>
        /// <param name="path">文件路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="extension">资源后缀名，如果为None则默认以AssetBundle资源进行加载</param>
        static public void RunWithFile(string path, string multiVersionControlRelativePath = "", shaco.Base.FileDefine.FileExtension extension = shaco.Base.FileDefine.FileExtension.None)
        {
            var readObjTmp = ResourcesEx.LoadResourcesOrLocal(path, extension, multiVersionControlRelativePath);
            if (readObjTmp.IsNull())
                return;

            try
            {
                Log.Info("XLuaManager RunWithFile: path=" + path);
                shaco.Base.GameEntry.GetInstance<XLuaManager>()._luaenv.DoString(readObjTmp.ToString());

                if (shaco.Base.FileHelper.GetFilNameExtension(path) != shaco.Base.FileHelper.GetExtension(shaco.Base.FileDefine.FileExtension.None))
                {
                    path = shaco.Base.FileHelper.AddExtensions(path, shaco.Base.FileHelper.GetExtension(extension));
                }

                //如果存在下载文件，则自动销毁ab包资源
                var fullPath = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                fullPath = shaco.Base.FileHelper.GetFullpath(fullPath);
                fullPath = fullPath.ContactPath(ResourcesEx.DEFAULT_PREFIX_PATH_LOWER.ContactPath(path));
                if (shaco.Base.FileHelper.ExistsFile(fullPath))
                {
                    ResourcesEx.UnloadAssetBundleLocal(path, true, extension, multiVersionControlRelativePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("XLuaManager RunWithFile error=" + e);
            }
        }

        /// <summary>
        /// 直接运行lua脚本
        /// </summary>
        /// <param name="luaScript">lua脚本内容</param>
        static public void RunWithString(string luaScript)
        {
            try
            {
                shaco.Base.GameEntry.GetInstance<XLuaManager>()._luaenv.DoString(luaScript);
            }
            catch (System.Exception e)
            {
                Debug.LogError("XLuaManager RunWithString error=" + e);
            }
        }

        /// <summary>
        /// 销毁lua管理器和lua状态机
        /// </summary>
        /// <param name="luaScript">lua脚本内容</param>
        static public void DestroyInstance()
        {
            if (shaco.Base.GameEntry.HasInstance<XLuaManager>())
            {
                var intanceTmp = shaco.Base.GameEntry.GetInstance<XLuaManager>();
                intanceTmp._luaenv.Dispose();
                intanceTmp._luaenv = null;
                shaco.Base.GameEntry.RemoveIntance<XLuaManager>();
            }
        }
    }
}
#endif