using UnityEngine;
using System.Collections;
using System.IO;
using shaco.Base;

namespace shaco
{
    public class HotUpdateImportMemory : HotUpdateImport
    {
        //异步读取超时时间
        public float ReadTimeoutSeconds = 1.0f;

        private float _fReadEplaseTime = 0;

        /// <summary>
        /// 从本地储存中异步创建资源，该路径可读可写
        /// </summary>
        public void CreateByMemoryAsyncAutoPlatform(string fileName, string multiVersionControlRelativePath = "")
        {
            fileName = fileName.ToLower();
            _multiVersionControlRelativePath = multiVersionControlRelativePath;
            var pathConvert = HotUpdateHelper.AssetBundlePathToKey(fileName);
            pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(pathConvert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            CreateByMemoryAsync(pathConvert, fileName);
        }

        /// <summary>
        /// 从本地储存中创建资源，该路径可读可写
        /// 该方法仅支持unity5.x以上版本(因为unity4.x以下无法直接解析未压缩的assetbundle)
        /// </summary>
        public void CreateByMemoryAutoPlatform(string fileName, string multiVersionControlRelativePath = "")
        {
            fileName = fileName.ToLower();
            _multiVersionControlRelativePath = multiVersionControlRelativePath;
            var pathConvert = HotUpdateHelper.AssetBundlePathToKey(fileName);
            pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(pathConvert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            CreateByMemory(pathConvert, fileName);
        }

        /// <summary>
        /// 从用户自定义路径中异步加载assetbundle
        /// <param name="path">路径</param>
        /// </summary>
        public void CreateByMemoryAsyncByUserPath(string path)
        {
            _multiVersionControlRelativePath = string.Empty;
            var pathCheck = GetAssetbundlePath(path);
            if (string.IsNullOrEmpty(pathCheck))
            {
                OnCompleted();
                return;
            }
            Create(pathCheck, HotUpdateDefine.ResourceCreateMode.MemoryAsync);
        }

        /// <summary>
        /// 从用户自定义路径中加载assetbundle
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        public void CreateByMemoryByUserPath(string path)
        {
            _multiVersionControlRelativePath = string.Empty;
            var pathCheck = GetAssetbundlePath(path);
            if (string.IsNullOrEmpty(pathCheck))
            {
                return;
            }
            Create(pathCheck, HotUpdateDefine.ResourceCreateMode.Memory);
        }

        /// <summary>
        /// 保存已经加载过的assetbundle到本地储存中
        /// </summary>
        /// <param name="fileName">File name.</param> 文件名字
        public void SaveDataToStorage(string fileName)
        {
            var byteDataTmp = this._loadResourceData;
            if (byteDataTmp.IsNull() || byteDataTmp.Length == 0)
            {
                OnErrorAndPrint("HotUpdate SaveDataToStorage error: no data to save, please load resource first");
                return;
            }

            SaveDataToStorage(fileName, byteDataTmp);
        }

        /// <summary>
        /// 保存二进制数据到本地储存中
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="byteData">Byte data.</param>
        public void SaveDataToStorage(string fileName, byte[] byteData)
        {
            var path = GetStorageFullPath(fileName);
            FileHelper.CheckFolderPathWithAutoCreate(path);
            File.WriteAllBytes(path, byteData);

            Log.Info("SaveDataToStorage success, path=" + path);
        }

        /// <summary>
        /// 获取加载进度，范围(0 ~ 1)
        /// </summary>
        public float GetLoadProgress()
        {
            return _fCurrentProgress;
        }

        /// <summary>
        /// 获取可读文件夹路径
        /// </summary>
        protected string GetReadOnlyFolder()
        {
            //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
            string ret =
#if UNITY_ANDROID && !UNITY_EDITOR
                "jar:file://" + Application.dataPath + "!/assets/";  
#elif UNITY_IPHONE && !UNITY_EDITOR
            Application.dataPath + "/Raw/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            "file://" + Application.dataPath + "/StreamingAssets/";
#else
            Application.streamingAssetsPath;
#endif

            return ret;
        }

        /// <summary>
        /// 从本地文件中读取assetbundle
        /// </summary>
        /// <returns>返回加载assetbundle请求</returns>
        /// <param name="path">assetbundle路径</param> 
        /// <param name="callbackEnd">获取完毕assetbundle请求文件回调</param>
        protected void LoadResourceByMemoryAsync(string path, System.Action<AssetBundleCreateRequest> callbackEnd)
        {
            if (!Application.isPlaying)
            {
                OnErrorAndPrint("HotUpdate LoadResourceByMemory error: this function only use on playing mode");
                return;
            }

            //从内存中读取则可以使用中文路径了
            shaco.Base.EncryptDecrypt.DecryptAsync(path, (byte[] bytes) =>
            {
                //从内存中创建资源
                AssetBundleCreateRequest assetBundleRequst = null;

                if (!bytes.IsNullOrEmpty())
                {
                    _loadResourceData = bytes;
                    if (_loadResourceData != null)
                    {
#if UNITY_5_3_OR_NEWER
                        assetBundleRequst = AssetBundle.LoadFromMemoryAsync(_loadResourceData);
#else
                        assetBundleRequst = AssetBundle.CreateFromMemory(_loadResourceData);
#endif
                    }
                }

                if (null != callbackEnd)
                {
                    callbackEnd(assetBundleRequst);
                }
            });
        }

        protected void LoadResourceByMemory(string path)
        {
            bool isOriginalTextFile = HotUpdateHelper.IsKeepFile(path);

            //如果后缀名不是AssetBundle的格式，则默认作为原始资源加载
            if (FileHelper.GetFilNameExtension(path) != HotUpdateDefine.EXTENSION_ASSETBUNDLE.RemoveFront(FileDefine.DOT_SPLIT))
            {
                isOriginalTextFile = true;
            }

            if (!isOriginalTextFile)
            {
                _loadResourceData = shaco.Base.EncryptDecrypt.Decrypt(path);
                if (!_loadResourceData.IsNullOrEmpty())
                {
#if UNITY_5_3_OR_NEWER
                    _assetBundleRead = AssetBundle.LoadFromMemory(_loadResourceData);
#else
                    _assetBundleRead = AssetBundle.CreateFromMemoryImmediate(_loadResourceData);
#endif
                }
            }
            CheckAssetbundleValid(path, isOriginalTextFile);
        }

        /// <summary>
        /// 获取可读可写文件夹路径 + 文件名字
        /// </summary>
        /// <param name="fileName">File name.</param> 文件名字
        protected string GetStorageFullPath(string fileName)
        {
            if (fileName.Contains(Application.persistentDataPath))
                return fileName;

            var ret = FileHelper.ContactPath(Application.persistentDataPath, fileName);
            return ret;
        }

        protected IEnumerator LoadResourceByUserPathAsync(string path, System.Action callbackLoadEnd)
        {
            bool isOriginalTextFile = HotUpdateHelper.IsKeepFile(path);
            AssetBundleCreateRequest assetBundleRequst = null;

            //如果后缀名不是AssetBundle的格式，则默认作为原始资源加载
            if (FileHelper.GetFilNameExtension(path) != HotUpdateDefine.EXTENSION_ASSETBUNDLE.RemoveFront(FileDefine.DOT_SPLIT))
            {
                isOriginalTextFile = true;
            }

            if (!isOriginalTextFile)
            {
                bool loadCompleted = false;
                LoadResourceByMemoryAsync(path, (AssetBundleCreateRequest request) =>
                {
                    assetBundleRequst = request;
                    loadCompleted = true;
                });

                //等待assetbundle请求文件加载完毕
                while (!loadCompleted)
                {
                    yield return null;
                }

                if (!assetBundleRequst.IsNull())
                {
                    //开始加载assetbundle
                    while (!assetBundleRequst.isDone)
                    {
                        if (_isRequestStopWorking)
                        {
                            OnCompleted();
                            break;
                        }

                        _fCurrentProgress = assetBundleRequst.progress;

                        if (_fCurrentProgress == 0)
                        {
                            _fReadEplaseTime += Time.fixedDeltaTime;
                            if (_fReadEplaseTime >= ReadTimeoutSeconds)
                            {
                                break;
                            }
                        }

                        if (assetBundleRequst.progress < 1.0f)
                        {
                            if (onProcessIng != null)
                            {
                                onProcessIng.InvokeAllCallBack();
                            }
                        }
                        yield return null;
                    }

                    if (assetBundleRequst.assetBundle != null)
                    {
                        this._assetBundleRead = assetBundleRequst.assetBundle;
                    }
                    else
                    {
                        OnError("HotUpdate LoadResourceByUserPathAsync error: can't load assetbundle, path=" + path);
                    }
                }
                else
                {
                    OnError("HotUpdate LoadResourceByUserPathAsync error: can't create load request, path=" + path);
                }
            }

            CheckAssetbundleValid(path, isOriginalTextFile);
            _fCurrentProgress = 1.0f;
            if (null != callbackLoadEnd)
            {
                callbackLoadEnd();
            }
        }

        /// <summary>
        /// 创建assetbundle加载请求
        /// </summary>
        /// <param name="url">网址、地址或者路径</param>
        /// <param name="mode">创建assetbundle的方式</param>
        protected void Create(string url, HotUpdateDefine.ResourceCreateMode createMode)
        {
            if (_isWorking)
            {
                Log.Warning("downloading, please wait");
                return;
            }

            Close();
            ResetParam();
            _isWorking = true;
            _isRequestStopWorking = false;

            switch (createMode)
            {
                case HotUpdateDefine.ResourceCreateMode.Memory:
                    {
                        LoadResourceByMemory(url);
                        break;
                    }
                case HotUpdateDefine.ResourceCreateMode.MemoryAsync:
                    {
                        CreateWithAsync(url);
                        break;
                    }
                default: Log.Error("HotUpdateImportMemory Create error: unsupport mode=" + createMode); break;
            }
        }

        protected void CreateWithAsync(string url)
        {
            var target = ActionS.GetDelegateMonoBehaviour();

            //先从本地加载资源
            target.StartCoroutine(LoadResourceByUserPathAsync(url, () =>
            {
                //本地资源加载出现错误，在线更新资源
                if (HasError() && !HotUpdateHelper.GetDynamicResourceAddress().IsNullOrEmpty())
                {
                    CreateWithAsyncDynamic(target, url);
                }
                else
                {
                    //load end
                    OnError(GetLastError());
                    OnCompleted();
                }
            }));
        }

        /// <summary>
        /// 在线更新资源
        /// <param name="target">携程执行方法对象</param>
        /// <param name="url">下载地址或本地路径</param>
        /// </summary>
        protected void CreateWithAsyncDynamic(MonoBehaviour target, string url)
        {
            int currentIndex = 0;
            HotUpdateHelper.ForeachGetDataCacheFromDynamicAddress(url, _multiVersionControlRelativePath, (string errorMessage) =>
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    //restart load local resource with async
                    target.StartCoroutine(LoadResourceByUserPathAsync(url, () =>
                    {
                        //load end - success
                        ClearLastError();
                        OnCompleted();
                    }));
                }
                else
                {
                    //has some error, quickly end
                    OnCompleted();
                }
            },
            (float progress) =>
            {
                _fCurrentProgress = progress;

                if (null != this.onProcessIng)
                {
                    this.onProcessIng.InvokeAllCallBack();
                }
            }, ref currentIndex);
        }

        protected string GetAssetbundlePath(string path)
        {
            string retPath = string.Empty;
            if (!FileHelper.ExistsFile(path))
            {
                OnError("HotUpdate load assetbundle error: missing file name=" + FileHelper.GetLastFileName(path) + "\npath=" + path);
            }
            else
            {
                retPath = path;
            }

            return retPath;
        }

        /// <summary>
        /// 检查assetbundle是否正常可以使用，如果不能使用则会删除assetbundle文件，如果可以使用并且是原始文件则转换为原始文件使用
        /// </summary>
        /// <param name="path">assetbundle路径</param>
        /// <param name="isAssetBundle">该路径的文件是否为assetbundle，反之为原始文件</param>
        private void CheckAssetbundleValid(string path, bool isOriginalTextFile)
        {
            if (_assetBundleRead != null)
                return;
            else
            {
                if (!FileHelper.ExistsFile(path))
                {
                    OnError("HotUpdate CheckAssetbundleValid: not found assetbundle path=" + path);
                }
                else
                {
                    if (isOriginalTextFile)
                    {
                        _textAssetOrigin = new TextOrigin();

                        _textAssetOrigin.bytes = shaco.Base.EncryptDecrypt.Decrypt(path);

                        if (shaco.Base.FileHelper.IsTextFile(path))
                        {
                            _textAssetOrigin.text = _textAssetOrigin.bytes.ToStringArray();
                         
                            //为了避免内存浪费，清理bytes引用
                            _textAssetOrigin.bytes = null;
                        }
                    }
                    else
                    {
                        bool isEncryption = shaco.Base.EncryptDecrypt.IsEncryption(FileHelper.ReadAllByteByUserPath(path));
                        OnErrorAndPrint("HotUpdate CheckAssetbundleValid: assetbundle is invalid will delete it isEncryption=" + isEncryption + "\npath=" + path);
                        FileHelper.DeleteByUserPath(path);
                    }
                }
            }
        }

        private void OnErrorAndPrint(string msg)
        {
            OnError(msg);

            if (!string.IsNullOrEmpty(GetLastError()))
            {
                Log.Error(GetLastError());
            }
        }

        protected override void ResetParam()
        {
            base.ResetParam();

            _fReadEplaseTime = 0;
            _fCurrentProgress = 0;
        }

        public override bool IsCompleted()
        {
            return base.IsCompleted() && !HasError();
        }

        private void CreateByMemoryAsync(string pathConvert, string fileName)
        {
            HotUpdateManifest.CheckDependenciesAsync(ActionS.GetDelegateMonoBehaviour(), _multiVersionControlRelativePath, HotUpdateHelper.AssetBundleKeyToPath(fileName), () =>
            {
                string pathTmp = GetStorageFullPath(pathConvert);
                Create(pathTmp, HotUpdateDefine.ResourceCreateMode.MemoryAsync);
            });
        }

        private void CreateByMemory(string pathConvert, string fileName)
        {
            if (!HotUpdateManifest.CheckDependencies(HotUpdateHelper.AssetBundleKeyToPath(fileName), _multiVersionControlRelativePath))
                return;

            string pathTmp = GetStorageFullPath(pathConvert);
            Create(pathTmp, HotUpdateDefine.ResourceCreateMode.Memory);
        }
    }
}
