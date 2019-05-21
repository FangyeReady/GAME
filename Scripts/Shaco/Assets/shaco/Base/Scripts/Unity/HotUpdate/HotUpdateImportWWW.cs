using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using shaco.Base;

namespace shaco
{
    public partial class HotUpdateImportWWW : HotUpdateImport
    {
        //超时时间
        public double TimeoutSeconds = 6.0f;
        //超时后重试次数
        public int TimeoutRetryTimes = 5;
        //刷新下载速度的间隔时间
        public float UpdateDownloadSpeedTime = 1.0f;
        //同一时间下载队列中http请求数量
        public int DownloadSqueueCount = 2;
        //检查需要的下载的版本完毕回掉
        public EventCallBack onCheckVersionEnd = new EventCallBack();


        //当前文件下载数量
        protected int _iCurrentDownloadCount = 0;
        //当前文件需要总共下载的数量
        protected int _iTotalDownloadCount = 0;
        //超时计时器
        protected shaco.ActionS _actionCheckTimeout = null;
        //刷新下载速度计时器
        protected shaco.ActionS _actionUpdateDownloadSpeed = null;
        //检查下载进度是否完成计时器
        protected shaco.ActionS _actionCheckDownload = null;
        //http请求对象
        protected shaco.Base.HttpHelper _httpHelper = new shaco.Base.HttpHelper();
        //当前下载速度
        protected long _iDownloadSpeed = 0;
        //上一次的下载速度
        protected long _lPrevDownloadSpeed = 0;
        //当前版本配置信息
        protected HotUpdateDefine.SerializeVersionControl _versionControlClient = new HotUpdateDefine.SerializeVersionControl();
        //当前需要下载更新的资源包
        protected List<HotUpdateDefine.DownloadAssetBundle> _listUpdateAssetBundle = new List<HotUpdateDefine.DownloadAssetBundle>();
        //当前正在下载更新的资源包
        protected List<HotUpdateDefine.DownloadAssetBundle> _listCurrentUpdate = new List<HotUpdateDefine.DownloadAssetBundle>();
        //当前需要被清理的下载完毕的对象资源
        protected List<HotUpdateDefine.DownloadAssetBundle> _listWillRemoveUpdate = new List<HotUpdateDefine.DownloadAssetBundle>();
        //当前需要重新写入的Main MD5
        protected string _strNeedRewriteMainMD5 = string.Empty;
        //下载所有assetbundle完毕
        protected bool _isLoadAllAssetbundleSuccess = false;
        //当前下载状态
        protected HotUpdateDownloadStatus _updateStatus = new HotUpdateDownloadStatus();
        //当前已经下载数据大小
        protected long _currentDownloadedDataSize = 0;
        //当前需要更新的总数据大小
        protected long _currentNeedUpdateDataSize = 0;

        /// <summary>
        /// 检查并更新资源
        /// </summary>
        /// <param name="urlVersion">服务器版本描述文件下载地址(例如VersionControl@@Android文件夹所在地址)</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="packageVersion">安装包版本号，例如1.0.0，如果填写了该字段，则在断网情况下对本地资源做版本管理检测，如果本地资源版本低于服务器资源则要求联网更新</param>
        /// <param name="onlyDownloadConfig">是否只下载资源版本管理配置，不下载资源内容</param>
        public void CheckUpdate(string urlVersion, string multiVersionControlRelativePath)
        {
            CheckUpdate(urlVersion, string.Empty, multiVersionControlRelativePath, false);
        }

        public void CheckUpdate(string urlVersion, bool onlyDownloadConfig, string multiVersionControlRelativePath = "")
        {
            CheckUpdate(urlVersion, string.Empty, multiVersionControlRelativePath, onlyDownloadConfig);
        }

        public void CheckUpdate(string urlVersion, string packageVersion = "", string multiVersionControlRelativePath = "", bool onlyDownloadConfig = false)
        {
            try
            {
                _multiVersionControlRelativePath = multiVersionControlRelativePath;
                CheckUpdateBase(urlVersion, packageVersion, onlyDownloadConfig);
            }
            catch (System.Exception e)
            {
                OnError("check update exception=" + e);
            }
        }

        /// <summary>
        /// 从网络下载或者本地只读路径中创建资源
        /// </summary>
        /// <param name="url">URL.</param> 网址或者只读文件夹相对路径
        /// <param name="callbackLoadProgress">Callback load progress.</param> 进度回调函数
        /// <param name="param">Parameter.</param> 自定义参数，会在进度回调函数中作为参数返回
        /// <param name="isMainThread">Parameter.</param> 是否在主线程运行的
        public void CreateByWWW(string url, bool isMainThread)
        {
            if (_isWorking)
            {
                Log.Warning("HotUpdateImporWWW warning: is busy now, please wait");
                return;
            }

            OnStartWorking();

            _url = url;
            LoadResourceByWWW(_url, isMainThread);
        }

        public void CreateByWWW(string url)
        {
            CreateByWWW(url, true);
        }

        public void CreateByWWWAutoPlatform(string url, bool isMainThread)
        {
            url = HotUpdateHelper.GetAssetBundlePathAutoPlatform(url, _multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            CreateByWWW(url, isMainThread);
        }

        public void CreateByWWWAutoPlatform(string url)
        {
            CreateByWWWAutoPlatform(url, true);
        }

        //确认是否存在该状态信息
        public bool HasStatus(HotUpdateDownloadStatus.Status status)
        {
            return _updateStatus.HasStatus(status);
        }

        /// <summary>
        /// 是否需要更新资源
        /// </summary>
        /// <returns><c>true</c>, if need update was ised, <c>false</c> otherwise.</returns>
        public bool IsNeedUpdate()
        {
            return _listUpdateAssetBundle.Count > 0;
        }

        /// <summary>
        /// 重置所有回调
        /// </summary>
        public void ResetAllCallBack()
        {
            onCheckVersionEnd.ClearCallBack();
            onProcessIng.ClearCallBack();
            onProcessEnd.ClearCallBack();
        }

        //是否完成下载，没有其他错误
        public override bool IsCompleted()
        {
            return HasStatus(HotUpdateDownloadStatus.Status.UpdateCompleted) && !HasError();
        }

        protected void OnSequeueDownloadCompletedAsync(HotUpdateDefine.DownloadAssetBundle downloadAssetBundle,
                                                        int squeueCount,
                                                        string urlResourcePrefix,
                                                        ref int currentDownloadCount,
                                                        ref int totalDownloadCount,
                                                        bool isMainThread)
        {

            lock (_listCurrentUpdate)
            {
                ++currentDownloadCount;
                ++_iCurrentDownloadCount;

                _lPrevDownloadSpeed = UpdateDownloadSpeed(true);
#if DEBUG_LOG
                if (_listWillRemoveUpdate.Contains(downloadAssetBundle))
                {
                    shaco.Log.Error("HotUpdate OnSequeueDownloadCompletedAsync error: have duplicate download info, url" + downloadAssetBundle.HotUpdateDel._url);
                }
#endif
                _listWillRemoveUpdate.Add(downloadAssetBundle);

                OnError(downloadAssetBundle.HttpDel.GetLastError());

                CheckUpdateAssetBundleBySequeue(1, urlResourcePrefix, isMainThread);
            }
        }

        protected void CheckUpdateBase(string urlVersion, string packageVersion, bool onlyDownloadConfig)
        {
            //Unity自带的这个网络有效性检查速度太慢，切换网络后很久状态不会更新
            // if (Application.internetReachability == NetworkReachability.NotReachable)
            // {
            //     OnError("CheckUpdateByWWW warning: no network !");
            //     OnCompleted();
            //     return;
            // }

            if (string.IsNullOrEmpty(urlVersion))
            {
                OnError("CheckUpdateByWWW error: params is valid !");
                OnCompleted();
                return;
            }

            if (_isWorking)
            {
                Log.Warning("HotUpdateImporWWW warning: is busy now, please wait");
                return;
            }

            urlVersion = FileHelper.RemoveSubStringByFind(urlVersion, HotUpdateDefine.SIGN_FLAG);
            urlVersion = HotUpdateHelper.GetVersionControlFilePath(urlVersion, _multiVersionControlRelativePath);

            OnStartWorking();

            _url = urlVersion;
            CheckVersionBase(_url, packageVersion, onlyDownloadConfig);

            Log.Info("check update start");
        }

        protected void StopAllAction()
        {
            if (_actionCheckTimeout != null)
            {
                _actionCheckTimeout.StopMe();
                _actionCheckTimeout = null;
            }

            if (_actionUpdateDownloadSpeed != null)
            {
                _actionUpdateDownloadSpeed.StopMe();
                _actionUpdateDownloadSpeed = null;
            }
        }

        protected bool DeleteAssetbundles(List<HotUpdateDefine.DownloadAssetBundle> downloadAssetbundles, string multiVersionControlRelativePath)
        {
            bool deleteSuccess = true;
            var pathRootTmp = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, _multiVersionControlRelativePath);
            pathRootTmp = FileHelper.GetFullpath(pathRootTmp);
            for (int i = 0; i < downloadAssetbundles.Count; ++i)
            {
                var relativePath = HotUpdateHelper.AssetBundleKeyToPath(downloadAssetbundles[i].ExportInfo.AssetBundleName);
                var fullPath = FileHelper.ContactPath(pathRootTmp, relativePath);

                if (FileHelper.ExistsFile(fullPath))
                {
                    if (!FileHelper.DeleteByUserPath(fullPath))
                    {
                        deleteSuccess = false;
                    }
                    else
                    {
                        //删除资源成功后，如果Cache中还存在旧的资源引用也要清理掉
                        if (HotUpdateDataCache.IsLoadedAssetBundle(relativePath, multiVersionControlRelativePath))
                        {
                            HotUpdateDataCache.UnloadAssetBundle(relativePath, multiVersionControlRelativePath, true);
                        }
                    }
                }
            }

            return deleteSuccess;
        }

        public override void Close(bool unloadAllLoadedObjects = false)
        {
            lock (_listCurrentUpdate)
            {
                if (!_isClosed)
                {
                    cleanCache();
                    ResetAllCallBack();
                    StopAllAction();
                    base.Close(unloadAllLoadedObjects);
                }
                else
                {
                    shaco.Log.Warning("HotUpdateImportWWW Close warning: dont need close again");
                }
            }
        }

        protected override void ResetParam()
        {
            base.ResetParam();

            _iCurrentDownloadCount = 0;
            _iTotalDownloadCount = 0;
            _currentDownloadedDataSize = 0;
            _currentNeedUpdateDataSize = 0;
            
            if (_httpHelper != null)
            {
                _httpHelper.CloseClient();
            }
            _iDownloadSpeed = 0;
            _lPrevDownloadSpeed = 0;
            _httpHelper.TimeoutSeconds = TimeoutSeconds;
            _httpHelper.TimeoutRetryTimes = TimeoutRetryTimes;
            _strNeedRewriteMainMD5 = string.Empty;
            _isLoadAllAssetbundleSuccess = false;
            _updateStatus.ResetDownloadStatus();

            lock (_listCurrentUpdate)
            {
                _listCurrentUpdate.Clear();
                _listUpdateAssetBundle.Clear();
            }

            StopAllAction();
        }

        public override bool HasError()
        {
            return base.HasError()
                || _updateStatus.HasStatus(HotUpdateDownloadStatus.Status.HasError)
                || _updateStatus.HasStatus(HotUpdateDownloadStatus.Status.ErrorNeedUpdateResourceWithNetWork)
                || _updateStatus.HasStatus(HotUpdateDownloadStatus.Status.ErrorNotFoundResource);
        }

        protected void cleanCache()
        {
            if (_httpHelper != null)
            {
                _httpHelper.CloseClient();
            }

            for (int i = 0; i < _listCurrentUpdate.Count; ++i)
            {
                _listCurrentUpdate[i].HotUpdateDel.Close();
            }
            _listCurrentUpdate.Clear();
        }

        protected void OnStartWorking()
        {
            cleanCache();
            ResetParam();
            _isWorking = true;
        }

        protected override void OnError(string error)
        {
            base.OnError(error);

            if (HasError())
            {
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.HasError);
            }
        }

        protected override void CheckCompleted(string error, bool isInvokeEndCallFunc)
        {
            if (isInvokeEndCallFunc)
            {
                StopAllAction();
                if (_actionCheckDownload != null)
                {
                    _actionCheckDownload.StopMe();
                    _actionCheckDownload = null;
                }

                if (!HasError() && string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(_strNeedRewriteMainMD5))
                {
                    Log.Info("write main md5 files status=" + GetStatusDescription());
                    FileHelper.WriteAllByPersistent(HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(_multiVersionControlRelativePath), _strNeedRewriteMainMD5);
                    _strNeedRewriteMainMD5 = string.Empty;
                }

                if (isInvokeEndCallFunc)
                {
                    if (HasStatus(HotUpdateDownloadStatus.Status.UpdateCompleted))
                    {
                        Log.Error("HotUpdate " + "onCompleted warning: It's a callback method that has been executed at one time");
                    }

                    _updateStatus.SetStatus(HasError() ? HotUpdateDownloadStatus.Status.HasError : HotUpdateDownloadStatus.Status.UpdateCompleted);
                    _isWorking = false;
                    _isRequestStopWorking = false;

                    if (IsCompleted())
                        _fCurrentProgress = 1.0f;

                    if (HasError())
                        Log.Error("HotUpdate " + error);
                }
            }

            base.CheckCompleted(error, isInvokeEndCallFunc);

            if (isInvokeEndCallFunc)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 检查获取本地配置信息，并记录下来
        /// </summary>
        /// <returns> 返回本地主配置文件地址
        protected string CheckGetVersionControl()
        {
            var retValue = string.Empty;
            if (null != _versionControlClient && _versionControlClient.ListAssetBundles.Count > 0)
            {
                return retValue;
            }

            var strVersionControlSavePath = FileHelper.GetFullpath(string.Empty);
            strVersionControlSavePath = HotUpdateHelper.GetVersionControlFilePath(strVersionControlSavePath, _multiVersionControlRelativePath);

            string strVersionRead = FileHelper.ExistsFile(strVersionControlSavePath) ? FileHelper.ReadAllByPersistent(strVersionControlSavePath) : string.Empty;
            if (string.IsNullOrEmpty(strVersionRead))
                _versionControlClient = new HotUpdateDefine.SerializeVersionControl();
            else
                _versionControlClient = HotUpdateHelper.JsonToVersionControl(strVersionRead);

            retValue = strVersionControlSavePath;
            return retValue;
        }
    }
}
