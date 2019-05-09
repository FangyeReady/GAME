using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using shaco.Base;

namespace shaco
{
    public partial class HotUpdateImportWWW : HotUpdateImport
    {
        //step 1 - 检查main md5文件是否正确
        protected void CheckMainMD5File(string urlVersion, string packageVersion, bool onlyDownloadConfig)
        {
            var urlResourcePrefix = FileHelper.GetFolderNameByPath(urlVersion, FileDefine.PATH_FLAG_SPLIT);
            var filenameMainMD5 = HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + HotUpdateDefine.EXTENSION_MAIN_MD5;
            var urlMainMD5File = FileHelper.ContactPath(urlResourcePrefix, filenameMainMD5);
            urlMainMD5File = shaco.Base.HttpHelper.AddHeaderURL(urlMainMD5File, new shaco.Base.HttpHelper.HttpComponent("rand_num", new System.Random().Next().ToString()));

            var updateTmp = new HotUpdateImportWWW();
            updateTmp.DownloadByHttp(urlMainMD5File, null, () =>
            {
                if (updateTmp._httpHelper.IsCompleted())
                {
                    OnError(updateTmp.GetLastError());

                    var mainMD5Path = HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(_multiVersionControlRelativePath);
                    var readMainMD5 = string.Empty;
                    if (FileHelper.ExistsFile(mainMD5Path))
                        readMainMD5 = FileHelper.ReadAllByUserPath(HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(_multiVersionControlRelativePath));

                    var mainMD5Server = updateTmp._httpHelper.GetDownloadByte().ToStringArray();
                    if (string.IsNullOrEmpty(readMainMD5) || readMainMD5 != mainMD5Server || !HotUpdateHelper.HasAllVersionControlFiles(_multiVersionControlRelativePath)
                       || (!onlyDownloadConfig && !HotUpdateHelper.CheckAllAssetbundleValidLocal(_multiVersionControlRelativePath)))
                    {
                        //delete main md5 file at first, if downloading have some error, we can check update again
                        HotUpdateHelper.DeleteAssetbundleConfigMainMD5(_multiVersionControlRelativePath);

                        //set need resave main md5
                        _strNeedRewriteMainMD5 = mainMD5Server;

                        //remove last '/' if have
                        if (!string.IsNullOrEmpty(urlVersion) && urlVersion[urlVersion.Length - 1].ToString() == FileDefine.PATH_FLAG_SPLIT)
                        {
                            urlVersion = urlVersion.Remove(urlVersion.Length - 1);
                        }

                        urlVersion = shaco.Base.HttpHelper.AddHeaderURL(urlVersion, new shaco.Base.HttpHelper.HttpComponent("main_md5", _strNeedRewriteMainMD5));
                        CheckVersionControlFile(urlVersion, onlyDownloadConfig);

                        Log.Info("check main md5 file end, will check version control file");
                    }
                    else
                    {
                        _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.NoNeedToUpdateConfig);

                        onCheckVersionEnd.InvokeAllCallBack();

                        //quickly check update end, if main md5 is not changed !
                        OnError(updateTmp.GetLastError());
                        OnCompleted();

                        Log.Info("check main md5 file end, nothing changed");
                    }
                }
                else
                {
                    if (updateTmp.HasError())
                    {
                        //If can't download main md5 file, it means url is missing
                        if (updateTmp._httpHelper.IsNotFound404() || updateTmp._httpHelper.IsForbidden403())
                        {
                            _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.ErrorNotFoundResource);
                        }
                        else
                        {
                            //Check whether the local resource needs to be updated without network
                            if (HotUpdateHelper.CheckUpdateLocalOnly(packageVersion, _multiVersionControlRelativePath))
                            {
                                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.ErrorNeedUpdateResourceWithNetWork);
                            }
                            else
                            {
                                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.NoNeedToUpdateConfig);
                            }
                        }

                        if (!HasStatus(HotUpdateDownloadStatus.Status.NoNeedToUpdateConfig))
                        {
                            OnError("check main md5 file error: msg=" + updateTmp.GetLastError());
                        }
                        OnCompleted();
                    }
                }
            });
        }

        //step 2 - 检查版本控制文件是否正确
        protected void CheckVersionControlFile(string urlVersion, bool onlyDownloadConfig)
        {
            _fCurrentProgress = 0;
            var updateTmp = new HotUpdateImportWWW();
            updateTmp.DownloadByHttp(urlVersion, () =>
                                                {
                                                    _fCurrentProgress += updateTmp._httpHelper.GetUseProgressInFrame() / (float)HotUpdateDefine.ALL_VERSION_CONTROL_FILE_COUNT * HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO;
                                                    onProcessIng.InvokeAllCallBack();
                                                },
                                                () =>
                                                {
                                                    if (updateTmp._httpHelper.IsCompleted())
                                                    {
                                                        var strReadVersion = updateTmp._httpHelper.GetDownloadByte().ToStringArray();
                                                        var urlResourcePrefix = FileHelper.GetFolderNameByPath(urlVersion, FileDefine.PATH_FLAG_SPLIT);

                                                        CheckUpdateByWWW(strReadVersion, urlResourcePrefix, onlyDownloadConfig);

                                                        Log.Info("check version control file end", Color.white);
                                                    }
                                                    else
                                                    {
                                                        if (updateTmp.HasError())
                                                        {
                                                            OnError("check version control file error msg=" + updateTmp.GetLastError() + "url=" + urlVersion);
                                                            OnCompleted();
                                                        }
                                                    }

                                                });
        }

        //step 3 - 对比版本控制文件，检查需要更新的资源包
        protected void CheckUpdateByWWW(string jsonVersion, string urlResourcePrefix, bool onlyDownloadConfig)
        {
            bool needReSaveVersion = false;

            //get server version
            var jsonVersionUTF8 = jsonVersion.ToStringArrayUTF8();
            HotUpdateDefine.SerializeVersionControl versionServer = null;

            try
            {
                versionServer = HotUpdateHelper.JsonToVersionControl(jsonVersionUTF8);
            }
            catch (System.Exception e)
            {
                versionServer = null;
                OnError("HotUpdate CheckUpdateByWWW error: unable to parse json exception msg=" + e + "\njson=" + jsonVersionUTF8);
                OnCompleted();
            }
            if (versionServer == null)
            {
                return;
            }

            //get client verion - force check and get version control
            _versionControlClient = null;
            var strVersionControlSavePath = CheckGetVersionControl();

            //update version
            if (_versionControlClient.Version != versionServer.Version)
            {
                Log.Info("HotUpdate check update version: client version=" + _versionControlClient.Version + " server version=" + versionServer.Version);
                _versionControlClient.Version = versionServer.Version;
            }

            var versionControlFullPath = FileHelper.GetFullpath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, _multiVersionControlRelativePath));
            if (HotUpdateHelper.ExcuteVersionControlAPI(versionServer.MD5Main, _multiVersionControlRelativePath, versionControlFullPath, versionServer))
            {
                //force completed
                OnCompleted();
                return;
            }

            //          if (versionServer.MD5Main != _versionControlClient.MD5Main) no use ?
            {
                _versionControlClient.MD5Main = versionServer.MD5Main;

                _listUpdateAssetBundle.Clear();

                //select need update resource
                _listUpdateAssetBundle = HotUpdateHelper.SelectUpdateFiles(FileHelper.GetFullpath(string.Empty), _multiVersionControlRelativePath, _versionControlClient, versionServer, false);

                //计算本次需要下载的文件总大小
                _currentNeedUpdateDataSize = ComputerCurrentNeedUpdateDataSize(_listUpdateAssetBundle);

                if (_listUpdateAssetBundle.Count > 0)
                {
                    Log.Info("check update assetbundle end, ready to download count=" + _listUpdateAssetBundle.Count);
                    _iTotalDownloadCount = _listUpdateAssetBundle.Count;

                    //delete old assetbundles
                    if (!DeleteAssetbundles(_listUpdateAssetBundle, _multiVersionControlRelativePath))
                    {
                        needReSaveVersion = false;
                        Log.Error("HotUpdate CheckUpdateByWWW error: can't delete assetbundle, please see log with tag=FileHelper");
                    }
                    else
                    {
                        needReSaveVersion = true;
                    }
                }
                else
                    Log.Info("check update assetbundle end");

                //如果只下载配置，那么清理下载资源列表，不再下载资源了
                if (onlyDownloadConfig)
                {
                    _listUpdateAssetBundle.Clear();
                }

                onCheckVersionEnd.InvokeAllCallBack();
            }

            //If this operation only deletes assetbundles, then the version control file will not be updated automatically
            //Therefore, we need to check whether the local and server number of assetbundles are consistent to determine whether the local version control file needs to be refreshed
            if (!needReSaveVersion)
            {
                needReSaveVersion = (_versionControlClient.ListAssetBundles.Count != versionServer.ListAssetBundles.Count);
            }

            if (needReSaveVersion)
            {
                var strJsonServer = shaco.LitJson.JsonMapper.ToJson(versionServer);
                FileHelper.WriteAllByPersistent(strVersionControlSavePath, strJsonServer);
            }

            //check update manifest and download assetbundle
            CheckManifestBase(urlResourcePrefix, _listUpdateAssetBundle.Count > 0, onlyDownloadConfig);
        }

        //step 4 - 检查manifest(关系依赖文件)是否正确
        protected void CheckManifestBase(string urlResourcePrefix, bool isRequestUpdateAssetBundle, bool onlyDownloadConfig)
        {
            var strManifestPath1 = HotUpdateHelper.GetAssetBundleManifestMemoryPathAutoPlatform(_multiVersionControlRelativePath);
            var strManifestPath2 = FileHelper.RemoveExtension(strManifestPath1);

            if (isRequestUpdateAssetBundle || !HotUpdateHelper.HasAllVersionControlFiles(_multiVersionControlRelativePath))
            {
                Log.Info("check manifest end, need download manifest files");
                UpdateAssetBundleManifest(urlResourcePrefix, isRequestUpdateAssetBundle, onlyDownloadConfig, strManifestPath1, strManifestPath2);
            }
            else
            {
                Log.Info("check manifest end, onlyDownloadConfig=" + onlyDownloadConfig);
                CheckManifestBaseEnd(urlResourcePrefix, isRequestUpdateAssetBundle, onlyDownloadConfig);
            }
        }

        //step 5 - 检查并开始更新资源
        protected void CheckDownloadAllAssetBundle(string urlResourcePrefix)
        {
            if (_listUpdateAssetBundle.Count <= 0)
            {
                return;
            }

            Log.Info("will start download assetbundle count=" + _listUpdateAssetBundle.Count);

            _fCurrentProgress = HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO;
            _iCurrentDownloadCount = 0;

            CheckUpdateAssetBundleBySequeue(DownloadSqueueCount, urlResourcePrefix, false);
            ComputerDownloadSpeedAll();

            _actionCheckDownload = WaitFor.Run(() =>
                                               {
                                                   if (IsCompleted() || HasError() || _isLoadAllAssetbundleSuccess)
                                                   {
                                                       return true;
                                                   }
                                                   else
                                                   {
                                                       onProcessIng.InvokeAllCallBack();
                                                       return false;
                                                   }

                                               }, () =>
                                               {
                                                   //all assetbundle download completed
                                                   OnError(GetLastError());
                                                   OnCompleted();
                                               });
        }
    }
}