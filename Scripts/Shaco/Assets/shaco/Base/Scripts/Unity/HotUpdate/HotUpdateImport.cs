using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using shaco.Base;

namespace shaco
{
    public class HotUpdateImport
    {
        //callbacks
        public shaco.Base.EventCallBack onProcessIng = new shaco.Base.EventCallBack();
        public shaco.Base.EventCallBack onProcessEnd = new shaco.Base.EventCallBack();

        //protected params
        protected AssetBundle _assetBundleRead = null;                                               //读取到的资源对象
        protected TextOrigin _textAssetOrigin = null;                                                //当assetbundle为原始文件时候的输出
        protected byte[] _loadResourceData = null;                                                   //读取到的资源二进制数据
        protected Dictionary<string, Object> _loadedAllAssets = new Dictionary<string, Object>();    //已经加载过的所有资源对象
        protected bool _isWorking = false;                                                           //是否正在下载或读取
        protected string _strLastError = string.Empty;                                               //最新一次错误信息
        protected bool _isRequestStopWorking = false;                                                //是否正在请求结束工作
        protected float _fCurrentProgress = 0;                                                       //当前进度(范围0 ~ 1.0)
        protected string _url = string.Empty;                                                        //下载路径或本地地址
        protected bool _isClosed = false;                                                            //是否已经关闭过下载器
        protected string _multiVersionControlRelativePath = string.Empty;                            //资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源

        /// <summary>
        /// 读取unity对象，支持TextAssets, GameObject, Texture2D
        /// </summary>
        /// <param name="fileName">File name.</param> assetbundle中的资源名字
        public Object Read(string fileName)
        {
            return Read(fileName, typeof(UnityEngine.Object));
        }

        public Object Read(string fileName, System.Type type)
        {
            Object retValue = null;

            //从原始资源加载
            if (this._assetBundleRead == null)
            {
                if (!_textAssetOrigin.IsNull() && _textAssetOrigin.success)
                {
                    if (typeof(TextAsset) == type)
                        shaco.Log.Error("Please use 'TextOrigin' or 'Object.ToString()' instead of");
                    else if (typeof(Texture2D) == type || typeof(Texture) == type)
                    {
                        var tex2D = new Texture2D(0, 0);
                        tex2D.LoadImage(_textAssetOrigin.bytes);
                        retValue = tex2D;
                    }
                    else if (typeof(Sprite) == type)
                    {
                        var tex2D = Read(fileName, typeof(Texture2D)) as Texture2D;
                        if (!tex2D.IsNull())
                        {
                            var sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
                            retValue = sprite;
                        }
                    }
                    else
                    {
                        retValue = _textAssetOrigin;
                    }
                }
            }
            //从AssetBundle加载资源
            else
            {
                var strFileNameTmp = GetReadFileName(fileName);
                if (!string.IsNullOrEmpty(strFileNameTmp))
                {
                    retValue = LoadAssetEx(strFileNameTmp, type);

                    if (!retValue.IsNull())
                    {
                        UnityHelper.ResetShader(retValue);
                    }
                }
            }

            if (retValue.IsNull())
            {
                Log.Error("HotUpdate read error: fileName=" + fileName + " type=" + type + " is original=" + !_textAssetOrigin.IsNull());
            }
            return retValue;
        }

        public T Read<T>(string fileName) where T : UnityEngine.Object
        {
            return (T)Read(fileName, typeof(T));
        }

        public Object[] ReadAll()
        {
            return ReadAll(typeof(UnityEngine.Object));
        }

        public Object[] ReadAll(System.Type type)
        {
            if (this._assetBundleRead == null)
            {
                if (!_textAssetOrigin.IsNull() && _textAssetOrigin.success)
                {
                    if (typeof(TextAsset) == type)
                        shaco.Log.Error("Please use 'TextOrigin' or 'Object.ToString()' instead of");
                    return new Object[] { _textAssetOrigin };
                }
                else
                {
                    Log.Error("HotUpdate ReadAll error: no resource be loaded");
                    return new Object[0];
                }
            }

            //read all from one assetbundle
            var ret = _assetBundleRead.LoadAllAssets(type);
            if (ret.Length == 0)
            {
                Log.Info("ReadAll error: type=" + type);
            }

            return ret;
        }

        public T[] ReadAll<T>() where T : UnityEngine.Object
        {
            return ReadAll(typeof(T)).ToArray<UnityEngine.Object, T>();
        }

        /// <summary>
        /// 读取一个字符串，必须为纯文本对象
        /// </summary>
        /// <param name="fileName">File name.</param> assetbundle中的资源名字
        public string ReadString(string fileName)
        {
            return HotUpdateHelper.AssetToString(Read(fileName));
        }

        /// <summary>
        /// 读取文件字节信息
        /// </summary>
        /// <param name="fileName">File name.</param>
        public byte[] ReadByte(string fileName)
        {
            var readObjTmp = Read(fileName);
            return HotUpdateHelper.AssetToByte(readObjTmp);
        }

        /// <summary>
        /// 读取主资源对象
        /// </summary>
        /// <returns>The main asset.</returns> assetbundle中的资源名字
        public Object ReadMainAsset()
        {
            var bundleTmp = this._assetBundleRead;
            if (bundleTmp == null)
            {
                if (!_textAssetOrigin.IsNull() && _textAssetOrigin.success)
                    return _textAssetOrigin;
                else
                {
                    Log.Error("HotUpdate ReadMainAsset error: no resource be loaded");
                    return null;
                }
            }

            return bundleTmp.mainAsset;
        }

        /// <summary>
        /// 判断资源是否有效
        /// </summary>
        public bool IsValidAsset()
        {
            return this._assetBundleRead != null || (this._assetBundleRead == null && !_textAssetOrigin.IsNull() && _textAssetOrigin.success);
        }

        /// <summary>
        /// 读取所有资源对象
        /// </summary>
        /// <param name="assetBundle">Asset bundle.</param> assetbundle中的资源名字
        public Object[] ReadAllByAssetBundle(AssetBundle assetbundle)
        {
            if (assetbundle == null)
                assetbundle = _assetBundleRead;

            if (assetbundle == null)
            {
                if (!_textAssetOrigin.IsNull() && _textAssetOrigin.success)
                    return new Object[] { _textAssetOrigin };
                else
                {
                    Log.Error("HotUpdate no resource be loaded");
                    return new Object[0];
                }
            }

            var all = assetbundle.LoadAllAssets();
            return all;
        }

        /// <summary>
        /// 异步读取unity对象
        /// </summary>
        /// <param name="fileName">File name.</param> assetbundle中的资源名字
        public void ReadAsync(string fileName, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            ReadAsync(fileName, typeof(UnityEngine.Object), callbackReadEnd, callbackProgress);
        }

        public void ReadAsync(string fileName, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            if (null == _assetBundleRead)
            {
                if (!_textAssetOrigin.IsNull() && _textAssetOrigin.success)
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(_textAssetOrigin);
                }
                else
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(null);
                }
            }
            else
            {
                fileName = GetReadFileName(fileName);
                if (string.IsNullOrEmpty(fileName))
                    return;

                LoadAssetAsyncEx(fileName, type, callbackReadEnd, callbackProgress);
            }
        }

        public void ReadAsync<T>(string fileName, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress) where T : UnityEngine.Object
        {
            ReadAsync(fileName, typeof(T), callbackReadEnd, callbackProgress);
        }

        public void ReadAllAsync(HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            ReadAllAsync(typeof(UnityEngine.Object), callbackReadEnd, callbackProgress);
        }

        public void ReadAllAsync(System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
#if UNITY_5_3_OR_NEWER
            if (null == _assetBundleRead)
            {
                if (!_textAssetOrigin.IsNull() && _textAssetOrigin.success)
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(new Object[] { _textAssetOrigin });
                }
                else
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(null);
                }
            }
            else
            {
                var requsetAsync = this._assetBundleRead.LoadAllAssetsAsync(type);

                shaco.Base.WaitFor.Run(() =>
                {
                    if (null != callbackProgress)
                        callbackProgress(requsetAsync.progress);
                    return requsetAsync.isDone;
                },
                () =>
                {
                    if (requsetAsync.allAssets.Length == 0)
                    {
                        Log.Error("HotUpdate ReadAllAsync error: type=" + type);
                    }
                    callbackReadEnd(requsetAsync.allAssets);
                });
            }
#else 
            callbackReadEnd(null);
            shaco.Log.Warning("HotUpdateImport ReadAllAsync error: this function only support on Unity5.3 or upper !");
#endif
        }

        public void ReadAllAsync<T>(HotUpdateDefine.CALL_FUNC_READ_OBJECTS callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress) where T : UnityEngine.Object
        {
            ReadAllAsync(typeof(T), callbackReadEnd, callbackProgress);
        }

        /// <summary>
        /// 异步读取一个字符串，必须为纯文本对象
        /// </summary>
        /// <param name="fileName">File name.</param> assetbundle中的资源名字
        public void ReadStringAsync(string fileName, HotUpdateDefine.CALL_FUNC_READ_STRING callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            ReadAsync(fileName, (UnityEngine.Object obj) =>
            {
                callbackReadEnd(HotUpdateHelper.AssetToString(obj));
            }, callbackProgress);
        }

        /// <summary>
        /// 异步读取文件字节信息
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void ReadByteAsync(string fileName, HotUpdateDefine.CALL_FUNC_READ_BYTE callbackReadEnd, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            ReadAsync(fileName, (UnityEngine.Object obj) =>
            {
                callbackReadEnd(HotUpdateHelper.AssetToByte(obj));
            }, callbackProgress);
        }

        /// <summary>
        /// 打印所有资源名字
        /// </summary>
        /// <param name="assetBundle">Asset bundle.</param>
        public void PrintAllAsset(AssetBundle assetBundle = null)
        {
            var all = ReadAllByAssetBundle(assetBundle);

            Log.Info("PrintAllAsset count=" + all.Length);

            for (int i = 0; i < all.Length; ++i)
            {
                Log.Info("PrintAllAsset value=" + all[i]);
            }
        }

        /// <summary>
        /// 获取最近一次错误信息
        /// </summary>
        public string GetLastError()
        {
            return _strLastError;
        }

        /// <summary>
        /// 获取只读文件夹路径 + 文件名字
        /// </summary>
        /// <param name="fileName">File name.</param> 文件名字
        public string GetReadOnlyFullPath(string fileName)
        {
            return FileHelper.ContactPath(Application.streamingAssetsPath, fileName);
        }

        //是否发生过错误
        virtual public bool HasError()
        {
            return !string.IsNullOrEmpty(GetLastError());
        }

        /// <summary>
        /// 读取场景名字
        /// <param name="sceneName">场景路径，例如Assets/Scenes/Demo.unity</param>
        /// <return>如果获取场景失败则返回空字符串</return>
        /// </summary>
        protected string ReadScene(string sceneName)
        {
#if UNITY_5_3_OR_NEWER
            if (null == _assetBundleRead)
            {
                Log.Error("HotUpdate ReadScene error: invablid assetbundle");
                return string.Empty;
            }
            var scenePath = _assetBundleRead.GetAllScenePaths();

            sceneName = sceneName.AddBehindNotContains(".unity");

            var findSceneName = string.Empty;
            for (int i = scenePath.Length - 1; i >= 0; --i)
            {
                if (scenePath[i].ToLower() == sceneName)
                {
                    findSceneName = scenePath[i];
                    break;
                }
            }

            if (string.IsNullOrEmpty(findSceneName))
            {
                Log.Error("HotUpdate ReadScene error: not found scene=" + findSceneName);
                return string.Empty;
            }
            return findSceneName;
#else
            Log.Error("HotUpdate ReadScene error: only support on Unity 5.3 or upper");
            return string.Empty;
#endif

        }

        protected string GetReadFileName(string fileName)
        {
            if (this._assetBundleRead == null)
            {
                OnError("HotUpdate GetReadFileName error: no resource be loaded fileName=" + fileName);
                return string.Empty;
            }

            return fileName.ToLower();
        }

        protected Object CheckLoadValidWhenAssetIsNull(Object asset, string fileName, System.Type type)
        {
            if (!asset.IsNull())
                return asset;

            //sprite to texture
            if ((type == typeof(UnityEngine.Texture2D) || type == typeof(UnityEngine.Texture)) && type != typeof(UnityEngine.Sprite))
            {
                var readSpr = LoadAssetEx(fileName, typeof(UnityEngine.Sprite)) as Sprite;
                if (!readSpr.IsNull())
                {
                    asset = readSpr.texture;
                }

                Log.Error("HotUpdate read error: can't read asset with type=" + type + " we fix it from sprite to texture");
            }

            //texture to sprite
            if (type == typeof(UnityEngine.Sprite) && (type != typeof(UnityEngine.Texture2D) && type != typeof(UnityEngine.Texture)))
            {
                var texTmp = LoadAssetEx(fileName, typeof(UnityEngine.Texture2D)) as Texture2D;

                if (!texTmp.IsNull())
                {
                    asset = Sprite.Create(texTmp, new Rect(0, 0, texTmp.width, texTmp.height), Vector2.zero);
                }

                Log.Error("HotUpdate read error: can't read asset with type=" + type + " we fix it from texture to sprite");
            }

            return asset;
        }

        protected void InvokeCallBack(System.Action callback)
        {
            if (callback != null)
            {
                callback();
            }
        }

        protected Object LoadAssetEx(string filename, System.Type type)
        {
#if UNITY_5_3_OR_NEWER
            //加载场景
            if (this._assetBundleRead.isStreamedSceneAssetBundle)
            {
                var sceneName = ReadScene(filename);
                if (!string.IsNullOrEmpty(sceneName))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                }
                _textAssetOrigin.text = sceneName;
                return _textAssetOrigin;
            }
            //加载资源
            else
#endif
            {
                Object retValue = GetAssetFromLoadedAssets(filename);
                if (null != retValue)
                {
                    return retValue;
                }

                retValue = this._assetBundleRead.LoadAsset(filename, type);

                //删除文件夹路径尝试加载
                if (retValue.IsNull())
                {
                    var filenameTmp = FileHelper.GetLastFileName(filename);
                    retValue = this._assetBundleRead.LoadAsset(filenameTmp, type);
                }

                if (retValue.IsNull())
                {
                    if (FileHelper.HasFileNameExtension(filename))
                    {
                        //删除后缀名和路径尝试加载
                        if (retValue.IsNull())
                        {
                            var filenameTmp = FileHelper.RemoveExtension(filename);
                            filenameTmp = FileHelper.GetLastFileName(filenameTmp);
                            retValue = this._assetBundleRead.LoadAsset(filenameTmp, type);
                        }
                    }
                }

                //检查使用图集方式加载
                if (retValue.IsNull())
                {
                    var filenameTmp = FileHelper.GetLastFileName(filename);
                    var loadObjs = this._assetBundleRead.LoadAllAssets();

                    SaveToLoadedAssets(loadObjs);

                    if (_loadedAllAssets.ContainsKey(filenameTmp))
                        retValue = _loadedAllAssets[filenameTmp];
                }

                //检查资源类型自动转换
                retValue = CheckLoadValidWhenAssetIsNull(retValue, filename, type);
                return retValue;
            }
        }

        protected void LoadAssetAsyncEx(string filename, System.Type type, HotUpdateDefine.CALL_FUNC_READ_OBJECT callback, HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
#if UNITY_5_3_OR_NEWER
            //加载场景
            if (this._assetBundleRead.isStreamedSceneAssetBundle)
            {
                var sceneName = ReadScene(filename);
                AsyncOperation asyncOperation = null;
                asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
                shaco.Base.WaitFor.Run(() =>
                {
                    if (null != callbackProgress)
                        callbackProgress(asyncOperation.progress);
                    return asyncOperation.isDone;
                }, () =>
                {
                    if (null != callback)
                    {
                        _textAssetOrigin.text = sceneName;
                        callback(_textAssetOrigin);
                    }
                });
            }
            else
#endif
            {
                Object loadObj = GetAssetFromLoadedAssets(filename);
                if (null != loadObj && null != callback)
                {
                    callback(loadObj);
                    return;
                }

                var requsetAsync = this._assetBundleRead.LoadAssetAsync(filename, type);
                shaco.Base.WaitFor.Run(() =>
                {
                    if (null != callbackProgress)
                        callbackProgress(requsetAsync.progress);
                    return requsetAsync.isDone;
                }, () =>
                {
                    if (requsetAsync.asset.IsNull())
                    {
                        //当fileName忘记设定后缀名的时候会读取失败，改用文件名则可行
                        requsetAsync = this._assetBundleRead.LoadAssetAsync(FileHelper.GetLastFileName(filename), type);
                        shaco.Base.WaitFor.Run(() =>
                        {
                            return requsetAsync.isDone;
                        },
                        () =>
                        {
                            //检查使用图集方式加载
                            if (requsetAsync.asset.IsNull())
                            {
                                var filenameTmp = FileHelper.GetLastFileName(filename);
                                ReadAllAsync((UnityEngine.Object[] loadObjs) =>
                                {
                                    UnityEngine.Object loadAssetTmp = null;

                                    if (!loadObjs.IsNullOrEmpty())
                                    {
                                        SaveToLoadedAssets(loadObjs);

                                        if (_loadedAllAssets.ContainsKey(filenameTmp))
                                            loadAssetTmp = _loadedAllAssets[filenameTmp];

                                        //检查资源类型自动转换
                                        loadAssetTmp = CheckLoadValidWhenAssetIsNull(loadAssetTmp, filename, type);
                                    }
                                    callback(loadAssetTmp);
                                }, callbackProgress);
                            }
                            else if (null != callback)
                                callback(requsetAsync.asset);
                        });
                    }
                    else if (null != callback)
                    {
                        callback(requsetAsync.asset);
                    }
                });
            }
        }

        protected Object GetAssetFromLoadedAssets(string filename)
        {
            Object retValue = null;
            var filenameTmp = FileHelper.GetLastFileName(filename);
            if (_loadedAllAssets.Count > 0 && _loadedAllAssets.ContainsKey(filenameTmp))
            {
                retValue = _loadedAllAssets[filenameTmp];
            }
            return retValue;
        }

        protected void SaveToLoadedAssets(UnityEngine.Object[] assets)
        {
            for (int i = assets.Length - 1; i >= 0; --i)
            {
                var objTmp = assets[i];
                if (!_loadedAllAssets.ContainsKey(objTmp.name))
                    _loadedAllAssets.Add(objTmp.name, objTmp);
            }
        }

        /// <summary>
        /// 关闭并释放资源，建议在assetbundle使用完毕后调用，否则再次加载相同assetbundle的时候会报错
        /// </summary>
        virtual public void Close(bool unloadAllLoadedObjects = false)
        {
            if (!_isClosed)
            {
                _isClosed = true;
                if (this._assetBundleRead != null)
                {
                    this._assetBundleRead.Unload(unloadAllLoadedObjects);
                    this._assetBundleRead = null;
                    this._loadResourceData = null;
                }

                if (_isWorking)
                {
                    OnError(GetLastError());
                    StopWorking();
                }
            }
        }

        virtual public void StopWorking()
        {
            _isRequestStopWorking = true;
            _isWorking = false;
        }

        virtual public bool IsCompleted()
        {
            return _fCurrentProgress >= 1.0f;
        }

        /// <summary>
        /// 重制网络参数，一般在开始连接时候调用
        /// </summary>
        virtual protected void ResetParam()
        {
            _assetBundleRead = null;
            _loadResourceData = null;
            _isWorking = false;
            _strLastError = string.Empty;
            _fCurrentProgress = 0;
            _isRequestStopWorking = false;
            _isClosed = false;
        }

        virtual protected void OnError(string error)
        {
            CheckCompleted(error, false);
        }

        virtual protected void OnCompleted()
        {
            CheckCompleted(GetLastError(), true);
        }

        virtual protected void CheckCompleted(string error, bool isInvokeEndCallFunc)
        {
            SetLastError(error);

            if (isInvokeEndCallFunc)
            {
                if (HasError())
                {
                    Log.Error("HotUpdate " + error);
                    this.Close(true);
                }

                onProcessIng.InvokeAllCallBack();
                onProcessEnd.InvokeAllCallBack();
            }
        }

        protected void ClearLastError()
        {
            _strLastError = string.Empty;
        }

        private void SetLastError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg) && HasError())
            {
                Log.Error("HotUpdate SetLastError error: can't set error message with empty string when has error, old error msg=" + GetLastError());
                return;
            }

            _strLastError = errorMsg;
        }
    }
}