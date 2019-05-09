using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public partial class HotUpdateImportWWW : HotUpdateImport
	{
        /// <summary>
        /// 通过http下载文件
        /// <param name="url">请求地址</param>
        /// <param name="callbackProgress">下载进度回调</param>
        /// <param name="callBackCompleted">下载完成回调</param>
        /// <return></return>
        /// </summary>
        protected void DownloadByHttp(string url, System.Action callbackProgress, System.Action callBackCompleted)
        {
            StopAllAction();
            _httpHelper.DownloadAsync(url);

            _actionCheckTimeout = WaitFor.Run(() =>
                                              {
                                                  CheckRemoveUnuseUpdateInfo();
                                                  _fCurrentProgress = _httpHelper.GetProgress();

                                                  if (_httpHelper.IsCompleted() || _httpHelper.HasError())
                                                      return true;
                                                  else
                                                  {
                                                      InvokeCallBack(callbackProgress);
                                                      return false;
                                                  }
                                              },
            () =>
            {
                CheckRemoveUnuseUpdateInfo();
                OnError(_httpHelper.GetLastError());
                Close();

                InvokeCallBack(callbackProgress);
                InvokeCallBack(callBackCompleted);

                //下载完毕后回收内存
                System.GC.Collect();
            });
        }

        //检查是否有未使用的需要被清理的下载对象
        protected void CheckRemoveUnuseUpdateInfo()
        {
            if (_listWillRemoveUpdate.Count == 0)
                return;

            for (int i = _listWillRemoveUpdate.Count - 1; i >= 0; --i)
            {
                var downloadAssetBundle = _listWillRemoveUpdate[i];
                downloadAssetBundle.HotUpdateDel.Close();
                _listWillRemoveUpdate.RemoveAt(i);

#if DEBUG_LOG
                if (!_listCurrentUpdate.Contains(downloadAssetBundle))
                {
                    shaco.Log.Error("HotUpdate CheckRemoveUnuseUpdateInfo error: not found download info, url=" + downloadAssetBundle.HotUpdateDel._url);
                }
#endif
                _listCurrentUpdate.Remove(downloadAssetBundle);
            }
        }

        /// <summary>
        /// 更新manifest(资源关系依赖)文件
        /// <param name="urlResourcePrefix">资源路径根目录</param>
        /// <param name="isRequestUpdateAssetBundle">是否请求强制更新manifest</param>
        /// <param name="onlyDownloadConfig">是否只下载配置文件，不下载资源</param>
        /// <param name="paths">manifest下载路径</param>
        /// </summary>
        protected void UpdateAssetBundleManifest(string urlResourcePrefix, bool isRequestUpdateAssetBundle, bool onlyDownloadConfig, params string[] paths)
        {
            var urls = new List<string>();
            for (int i = 0; i < paths.Length; ++i)
            {
                var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(paths[i]);
                var urlTmp = shaco.Base.FileHelper.ContactPath(urlResourcePrefix, fileNameTmp);
                urls.Add(urlTmp);
            }

            //download manifest from server
            var listUpdateTmp = new List<HotUpdateImportWWW>();
            var downloadCount = 0;

            List<HotUpdateDefine.DownloadAssetBundle> listDownloadTmp = new List<HotUpdateDefine.DownloadAssetBundle>();

            for (int i = 0; i < urls.Count; ++i)
            {
                var urlTmp = urls[i];
                var pathTmp = paths[i];

                var updateTmp = new HotUpdateImportWWW();
                listUpdateTmp.Add(updateTmp);

                var assetBundleTmp = new HotUpdateDefine.DownloadAssetBundle();
                assetBundleTmp.HotUpdateDel = updateTmp;
                assetBundleTmp.HttpDel = updateTmp._httpHelper;
                listDownloadTmp.Add(assetBundleTmp);

                urlTmp = shaco.Base.HttpHelper.AddHeaderURL(urlTmp, new shaco.Base.HttpHelper.HttpComponent("main_md5", _strNeedRewriteMainMD5));

                updateTmp.DownloadByHttp(urlTmp, () =>
                                                {
                                                    _fCurrentProgress += updateTmp._httpHelper.GetUseProgressInFrame() / (float)HotUpdateDefine.ALL_VERSION_CONTROL_FILE_COUNT * HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO;
                                                    onProcessIng.InvokeAllCallBack();
                                                },
                                                () =>
                                                {
                                                    if (updateTmp._httpHelper.IsCompleted())
                                                    {
                                                        shaco.Base.FileHelper.WriteAllByteByPersistent(pathTmp, updateTmp._httpHelper.GetDownloadByte());
                                                        ++downloadCount;

                                                        if (downloadCount >= urls.Count)
                                                        {
                                                            CheckManifestBaseEnd(urlResourcePrefix, isRequestUpdateAssetBundle, onlyDownloadConfig);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (updateTmp._httpHelper.HasError())
                                                        {
                                                            for (int j = 0; j < listUpdateTmp.Count; ++j)
                                                                listUpdateTmp[j].Close();
                                                            OnError("check manifest error: msg=" + updateTmp.GetLastError());
                                                            OnCompleted();
                                                        }
                                                    }
                                                });
            }

            ComputerDownloadSpeedAll();
        }

        /// <summary>
        /// 按队列依次下载资源
        /// <param name="squeueCount">队列中同时下载的文件数量</param>
        /// <param name="urlResourcePrefix">资源路径根目录</param>
        /// <param name="isMainThread">是否在主线程下载</param>
        /// </summary>
        protected void CheckUpdateAssetBundleBySequeue(int squeueCount, string urlResourcePrefix, bool isMainThread)
        {
            if (squeueCount < 0 || _iCurrentDownloadCount >= _iTotalDownloadCount || HasError())
            {
                Log.Info("HotUpdate download all assetbundle completed ! " + (HasError() ? "error message=" + GetLastError() : string.Empty)
                 + "\ndownload count=" + _iCurrentDownloadCount + " all count=" + _iTotalDownloadCount, Color.white);

                if (HasError())
                    OnError(GetLastError());
                else
                {
                    _isLoadAllAssetbundleSuccess = true;
                }
                return;
            }
            squeueCount = squeueCount < _listUpdateAssetBundle.Count ? squeueCount : _listUpdateAssetBundle.Count;

            float downloadProgress = 0;
            int currentDownloadCount = 0;
            int totalDownloadCount = squeueCount;
            float prevDownloadProgress = 0;
            double downloadMaxProgress = (double)_iTotalDownloadCount * 1.0f;
            var versionControlFullPath = shaco.Base.FileHelper.GetFullpath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, _multiVersionControlRelativePath));

            for (int i = squeueCount - 1; i >= 0; --i)
            {
                var downloadAssetBundle = _listUpdateAssetBundle[i];

                lock (_listCurrentUpdate)
                {
                    _listUpdateAssetBundle.RemoveAt(i);
                }

                if (downloadAssetBundle.HttpDel != null)
                {
                    --totalDownloadCount;
                    Log.Error("HotUpdate create http error: this http connect has be created !");
                    continue;
                }

                HotUpdateDefine.ExportAssetBundle exportBundleParam = downloadAssetBundle.ExportInfo;

                float prevProgress = 0;

                //for safe url
                var assetbundleNameConvert = HotUpdateHelper.AddAssetBundleNameTag(exportBundleParam.AssetBundleName, exportBundleParam.AssetBundleMD5);
                var fullUrl = shaco.Base.FileHelper.ContactPath(urlResourcePrefix, assetbundleNameConvert);

                //start downloading assetbundle
                var updateTmp = new HotUpdateImportWWW();

                //start downloading
                fullUrl = HotUpdateHelper.AssetBundleKeyToPath(fullUrl);
                // fullUrl = HttpHelper.AddHeaderURL(fullUrl, new HttpHelper.HttpComponent("md5", downloadAssetBundle.ExportInfo.AssetBundleMD5));
                updateTmp.CreateByWWW(fullUrl, isMainThread);

                downloadAssetBundle.HotUpdateDel = updateTmp;
                downloadAssetBundle.HttpDel = updateTmp._httpHelper;

                updateTmp.onProcessIng.AddCallBack(this, (object sender) =>
                {
                    var updateTarget = updateTmp;

                    if (_isRequestStopWorking)
                    {
                        return;
                    }

                    if (null == updateTarget)
                    {
                        shaco.Log.Error("HotUpdate check download progress error: updateTarget is null");
                        return;
                    }

                    float targetProgress = updateTarget.GetDownloadResourceProgress();
                    float useProgress = targetProgress - prevProgress;
                    prevProgress = targetProgress;

                    downloadProgress += useProgress;

                    float percentTmp = (float)((double)downloadProgress / (double)downloadMaxProgress);

                    float offsetProgress = percentTmp - prevDownloadProgress;
                    prevDownloadProgress = percentTmp;

                    if (offsetProgress > 0)
                    {
                        _fCurrentProgress += offsetProgress * (1 - HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO);
                    }

                    if (null == downloadAssetBundle)
                    {
                        shaco.Log.Error("HotUpdate check download progress error: downloadAssetBundle is null");
                        return;
                    }
                    else if (null == downloadAssetBundle.HttpDel)
                    {
                        shaco.Log.Error("HotUpdate check download progress error: downloadAssetBundle.HttpDel is null");
                        return;
                    }

                    _currentDownloadedDataSize += downloadAssetBundle.HttpDel.GetCurrentDownloadSize();

                    if (updateTarget.HasError())
                    {
                        OnError(updateTarget.GetLastError());

                        //request quit all download task
                        Close();
                    }
                });

                //set auto save path
                var fileNameTmp = HotUpdateHelper.AssetBundleKeyToPath(downloadAssetBundle.ExportInfo.AssetBundleName);
                var pathTmp = shaco.Base.FileHelper.ContactPath(versionControlFullPath, fileNameTmp);
                downloadAssetBundle.HttpDel.SetAutoSaveWhenCompleted(pathTmp);

                //downloaded compeleted callback
                downloadAssetBundle.HttpDel.CallBackCompleted = () =>
                {
                    OnSequeueDownloadCompletedAsync(downloadAssetBundle, squeueCount, urlResourcePrefix, ref currentDownloadCount, ref totalDownloadCount, false);
                };

                lock (_listCurrentUpdate)
                {
                    _listCurrentUpdate.Add(downloadAssetBundle);
                }
            }
        }

        /// <summary>
        /// 开启计时器在后台持续计算下载速度
        /// </summary>
        protected void ComputerDownloadSpeedAll()
        {
            if (_actionUpdateDownloadSpeed != null)
            {
                _actionUpdateDownloadSpeed.StopMe();
                _actionUpdateDownloadSpeed = null;
            }

            var actionTmp = shaco.DelayTime.Create(UpdateDownloadSpeedTime);
            actionTmp.onCompleteFunc = (shaco.ActionS ac) =>
            {
                _iDownloadSpeed = UpdateDownloadSpeed(false);
            };
            _actionUpdateDownloadSpeed = shaco.Repeat.CreateRepeatForver(actionTmp);
            _actionUpdateDownloadSpeed.RunAction(shaco.ActionS.GetDelegateInvoke());
        }

        /// <summary>
        /// 刷新当前下载速度
        /// <param name="isPrevSpeed">是否包含上一次的下载速度计算</param>
        /// <return>返回当前下载速度</return>
        /// </summary>
        protected long UpdateDownloadSpeed(bool isPrevSpeed)
        {
            long downloadSpeed = 0;
            long allDownloadCount = 0;
            long allDownloadSpeed = 0;

            lock (_listCurrentUpdate)
            {
                for (int i = 0; i < _listCurrentUpdate.Count; ++i)
                {
                    var httpTarget = _listCurrentUpdate[i].HttpDel;

                    if (httpTarget != null)
                    {
                        long downloadSpeedTmp = httpTarget.GetDownloadSpeed();
                        allDownloadSpeed += downloadSpeedTmp;
                        ++allDownloadCount;

                        httpTarget.ResetDownloadSpeed();
                    }
                }
            }

            if (_lPrevDownloadSpeed > 0)
            {
                downloadSpeed = _lPrevDownloadSpeed;
            }
            else
            {
                downloadSpeed = allDownloadSpeed;
            }

            if (!isPrevSpeed)
                _lPrevDownloadSpeed = 0;
            
            return downloadSpeed;
        }

        /// <summary>
        /// 检查版本更新开始
        /// </summary>
        protected void CheckVersionBase(string urlVersion, string packageVersion, bool onlyDownloadConfig)
        {
            CheckMainMD5File(urlVersion, packageVersion, onlyDownloadConfig);
        }

        /// <summary>
        /// 检查manifest文件结束
        /// </summary>
        protected void CheckManifestBaseEnd(string urlResourcePrefix, bool isRequestUpdateAssetBundle, bool onlyDownloadConfig)
        {
            onProcessIng.InvokeAllCallBack();

            if (onlyDownloadConfig)
            {
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.OnlyDownloadConfig);
            }
            else if (_listUpdateAssetBundle.Count <= 0)
            {
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.NoNeedToDownloadResource);
            }

            if (isRequestUpdateAssetBundle && !onlyDownloadConfig)
            {
                CheckDownloadAllAssetBundle(urlResourcePrefix);
            }
            else
            {
                //no assetbundle need download, quickly completed
                OnCompleted();
            }
        }

        /// <summary>
        /// 通过网络下载资源
        /// <param name="url">下载地址</param>
        /// <param name="isMainThread">是否在主线程下载</param>
        /// </summary>
        protected void LoadResourceByWWW(string url, bool isMainThread)
        {
            _httpHelper.DownloadAsync(url);

            StopAllAction();


            if (isMainThread)
            {
                _actionCheckTimeout = WaitFor.Run(() =>
                                                  {
                                                      _fCurrentProgress = _httpHelper.GetProgress();

                                                      if (_httpHelper.IsCompleted() || _httpHelper.HasError())
                                                          return true;
                                                      else
                                                      {
                                                          onProcessIng.InvokeAllCallBack();
                                                          return false;
                                                      }

                                                  }, () =>
                                                  {

                                                      this._loadResourceData = _httpHelper.GetDownloadByte();

                                                      OnError(_httpHelper.GetLastError());
                                                      OnCompleted();
                                                  });
            }
            else
            {
                var threadTmp = new System.Threading.Thread(() =>
                                                            {
                                                                while (!_httpHelper.IsCompleted() && !_httpHelper.HasError())
                                                                {
                                                                    _fCurrentProgress = _httpHelper.GetProgress();
                                                                    onProcessIng.InvokeAllCallBack();

                                                                    System.Threading.Thread.Sleep(1);
                                                                }

                                                                OnError(_httpHelper.GetLastError());
                                                                OnCompleted();

                                                            });
                threadTmp.IsBackground = true;
                threadTmp.Start();
            }
        }

        protected long ComputerCurrentNeedUpdateDataSize(List<HotUpdateDefine.DownloadAssetBundle> needUpdateAssetBundles)
        {
            long retValue = 0;
            for (int i = needUpdateAssetBundles.Count - 1; i >= 0; --i)
            {
                retValue += needUpdateAssetBundles[i].ExportInfo.fileSize;
            }
            return retValue;
        }
	}
}	