using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class UpdateTool
    {
        static private readonly string DOWNLOAD_TEMPORY_PATH = Application.dataPath.RemoveLastPathByLevel(1) + "/shaco_download_tmp";
        static private readonly string DOWNLOAD_URL = "https://codeload.github.com/shaco-6/shacoBase/zip/master";

        //预估要下载的包体大小
        static private readonly long PACKAGE_SIZE = 6 * 1024 * 1024;

        static IEnumerator DownloadZip(string url, System.Func<float, bool> callbackProgress, System.Action<byte[], string> callbackCompleted)
        {
            bool forceStopDownload = false;
#if UNITY_5_3_OR_NEWER
            var www = UnityEngine.Networking.UnityWebRequest.Get(url);
            www.Send();
#else
            var www = new WWW(url);
#endif
            {
                shaco.WaitFor.Run(() =>
                {
                    if (null != callbackProgress)
                    {
                        //下载过程中，强制提前停止下载
#if UNITY_5_3_OR_NEWER
                        float currentProgress = (float)((double)www.downloadedBytes / (double)PACKAGE_SIZE);
#else
                        float currentProgress = www.progress;
#endif
                        if (callbackProgress(currentProgress))
                        {
                            forceStopDownload = true;
                        }
                    }
                    return forceStopDownload || www.isDone || !string.IsNullOrEmpty(www.error);
                }, () =>
                {
                    if (null != callbackCompleted)
                    {
                        if (forceStopDownload)
                        {
                            callbackCompleted(null, string.Empty);
                        }
                        else
                        {
#if UNITY_5_3_OR_NEWER
                            callbackCompleted(www.downloadHandler.data, www.error);
#else
                            callbackCompleted(www.bytes, www.error);
#endif
                        }
                    }
                    www.Dispose();
                });
            }

            yield return 1;
        }

        [MenuItem("shaco/Tools/UpdateShacoGameFrameWork _F12", false, (int)ToolsGlobalDefine.MenuPriority.Tools.UPDATE)]
        static private void OpenUpdateWindow()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogError("UpdateTool OpenUpdateWindow error: cannot be allowed when compiling code");
                return;
            }
            if (EditorUtility.DisplayDialog("Update", "will update 'shaco' GameFrameWork", "Continue", "Cancel"))
            {
                UpdatePackages();
            }
        }

        static private bool IsIgnoreFile(string[] ignoreFilesTmp, string filename)
        {
            for (int i = 0; i < ignoreFilesTmp.Length; ++i)
            {
                if (filename.Contains(ignoreFilesTmp[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static private List<string> GetFilesWithoutGitHubDirectories(string rootPath)
        {
            List<string> retValue = new List<string>();
            shaco.Base.FileHelper.GetSeekPath(rootPath, ref retValue, (string value) =>
            {
                return !value.Contains(".git") && value.LastIndexOf("cs.meta") < 0;
            });
            return retValue;
        }

        static private void UpdatePackages()
        {
            //从github下载资源包
            var downloadedUnZipPath = DOWNLOAD_TEMPORY_PATH;
            var downloadedZipPath = DOWNLOAD_TEMPORY_PATH + ".zip";
            var localPath = shaco.Base.GlobalParams.GetShacoFrameworkRootPath();
            // var httpTmp = new shaco.Base.HttpHelper();
            bool useCancel = false;

            try
            {
                //issue: 
                //c# http方法无法访问https的github，暂时不知道原因，而Unity的WWW通过c++实现的却是可以访问github
                // httpTmp.SetAutoSaveWhenCompleted(downloadedZipPath);
                // httpTmp.DownloadAsync(DOWNLOAD_URL);

                useCancel = EditorUtility.DisplayCancelableProgressBar("Update", "will start update", 0);

                shaco.ActionS.GetDelegateMonoBehaviour().StartCoroutine(DownloadZip(DOWNLOAD_URL, (float progress) =>
                {
                    useCancel = EditorUtility.DisplayCancelableProgressBar("Update From GitHub", "Please wait", progress);
                    return useCancel;

                }, (byte[] data, string error) =>
                {
                    if (null != data)
                    {
                        Debug.Log("data=" + data.Length);
                    }
                    if (useCancel)
                    {
                        Debug.Log("Manual Cancel Update");
                        // httpTmp.CloseClient();
                        EditorUtility.ClearProgressBar();
                        return;
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError("Update error: " + error);
                        // httpTmp.CloseClient();
                        EditorUtility.ClearProgressBar();
                        return;
                    }

                    shaco.Base.FileHelper.WriteAllByteByUserPath(downloadedZipPath, data);

                    EditorUtility.ClearProgressBar();

                    var downloadedPaths = new List<string>();
                    var localPaths = new List<string>();
                    var willRemovePaths = new List<string>();
                    var downloadedPathsDic = new Dictionary<string, string>();

                    //解压zip
                    shaco.Base.ZipHelper.UnZip(downloadedZipPath, downloadedUnZipPath);

                    //重命名解压出来的文件夹名字
                    if (downloadedUnZipPath.StartsWith("//"))
                    {
                        downloadedUnZipPath = downloadedUnZipPath.ReplaceFromBegin("//", "\\\\", 1);
                    }
                    var dictionaryTmp = System.IO.Directory.GetDirectories(downloadedUnZipPath)[0];
                    dictionaryTmp = dictionaryTmp.Replace("\\", "/");
                    var newDownloadedUnZipPath = dictionaryTmp.RemoveLastPathByLevel(1) + "/Base";
                    shaco.Base.FileHelper.MoveFileByUserPath(dictionaryTmp, newDownloadedUnZipPath);

                    Debug.Log("downloadedUnZipPath=" + downloadedUnZipPath + " dictionaryTmp=" + dictionaryTmp + " newDownloadedUnZipPath=" + newDownloadedUnZipPath + " localPath=" + localPath);

                    //收集github上没有的文件，但是本地有的文件，准备进行删除
                    if (newDownloadedUnZipPath.StartsWith("//"))
                    {
                        newDownloadedUnZipPath = newDownloadedUnZipPath.ReplaceFromBegin("//", "\\\\", 1);
                    }
                    downloadedPaths = GetFilesWithoutGitHubDirectories(newDownloadedUnZipPath);

                    //收集本地的文件
                    localPaths = GetFilesWithoutGitHubDirectories(localPath);

                    Debug.Log("downloadedPaths=" + downloadedPaths.Count + " localPaths=" + localPaths.Count);

                    for (int i = 0; i < downloadedPaths.Count; ++i)
                    {
                        var pathTmp = downloadedPaths[i].RemoveFront("shaco_download_tmp/Base");
                        downloadedPathsDic.Add(pathTmp, pathTmp);
                    }
                    for (int i = 0; i < localPaths.Count; ++i)
                    {
                        var pathTmp = localPaths[i].RemoveFront("shaco/Base");
                        if (!downloadedPathsDic.ContainsKey(pathTmp))
                        {
                            willRemovePaths.Add(localPaths[i]);
                        }
                    }

                    Debug.Log("will remove file count=" + willRemovePaths.Count);

                    //删除本地多余文件
                    for (int i = 0; i < willRemovePaths.Count; ++i)
                    {
                        //删除文件
                        shaco.Base.FileHelper.DeleteByUserPath(willRemovePaths[i]);

                        //删除meta文件
                        shaco.Base.FileHelper.DeleteByUserPath(willRemovePaths[i] + ".meta");

                        //删除空白文件夹 
                        shaco.Base.FileHelper.DeleteEmptyFolder(willRemovePaths[i], ".meta");
                    }

                    //替换和新增本地文件
                    Debug.Log("copy downloadedUnZipPath=" + downloadedUnZipPath + " des=" + localPath.RemoveLastPathByLevel(1));
                    shaco.Base.FileHelper.CopyFileByUserPath(downloadedUnZipPath, localPath.RemoveLastPathByLevel(1), "cs.meta");

                    //删除下载的资源包
                    shaco.Base.FileHelper.DeleteByUserPath(downloadedZipPath);
                    shaco.Base.FileHelper.DeleteByUserPath(downloadedUnZipPath);

#if !UNITY_5_3_OR_NEWER
                    //在Unity4.x版本不能过滤dll库的编译，如果存在xlua/tools目录中的部分dll库则会导致编译报错(System.dll)
                    //xlua支持的版本在Unity5.3以上，所以5.x以下的xlua目录直接删掉吧
                    var xluaFolderTmp = localPath.ContactPath("Library/XLua");
                    if (shaco.Base.FileHelper.ExistsDirectory(xluaFolderTmp))
                    {
                        shaco.Base.FileHelper.DeleteByUserPath(xluaFolderTmp);
                    }
#endif

                    // httpTmp.CloseClient();
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Update error: " + e);
                // httpTmp.CloseClient();
                EditorUtility.ClearProgressBar();
            }
        }
    }
}