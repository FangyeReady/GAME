using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class ResourcesOrLocalSequeue
    {

        private class LoadRequestInfo
        {
            public string path = string.Empty;
            public string multiVersionControlRelativePath = string.Empty;
            public string prefixPath = string.Empty;
            public System.Type type;
            public shaco.Base.FileDefine.FileExtension extension = shaco.Base.FileDefine.FileExtension.None;
            public HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd = null;
        }

        private System.Collections.Generic.List<LoadRequestInfo> _loadRequests = new System.Collections.Generic.List<LoadRequestInfo>();

        public void AddRequest(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, string multiVersionControlRelativePath = "")
        {
            AddRequest(path, callbackLoadEnd, ResourcesEx.DEFAULT_PREFIX_PATH_LOWER, typeof(UnityEngine.Object), ResourcesEx.DEFAULT_EXTENSION, multiVersionControlRelativePath);
        }

        public void AddRequest<T>(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, string multiVersionControlRelativePath = "")
        {
            AddRequest(path, callbackLoadEnd, ResourcesEx.DEFAULT_PREFIX_PATH_LOWER, typeof(T), ResourcesEx.DEFAULT_EXTENSION, multiVersionControlRelativePath);
        }

        public void AddRequest<T>(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, string prefixPath, shaco.Base.FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            AddRequest(path, callbackLoadEnd, prefixPath, typeof(T), extension, multiVersionControlRelativePath);
        }

        public void AddRequest(string path, HotUpdateDefine.CALL_FUNC_READ_OBJECT callbackLoadEnd, string prefixPath, System.Type type, shaco.Base.FileDefine.FileExtension extension, string multiVersionControlRelativePath = "")
        {
            _loadRequests.Add(new LoadRequestInfo()
            {
                path = path,
                prefixPath = prefixPath,
                type = type,
                extension = extension,
                multiVersionControlRelativePath = multiVersionControlRelativePath,
                callbackLoadEnd = callbackLoadEnd
            });
        }

        public void Start(HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress)
        {
            if (_loadRequests.IsNullOrEmpty())
            {
                Log.Warning("ResourcesEx+Public StartLoadResourcesOrLocalRequestInSequeue error: load request sequeue is empty ! please call 'AddLoadResourcesOrLocalRequestInSequeue' at first");

                if (null != callbackProgress)
                {
                    callbackProgress(1.0f);
                }
                return;
            }

            _StartLoadResourcesOrLocalRequestInSequeue(callbackProgress, 0);
        }

        private void _StartLoadResourcesOrLocalRequestInSequeue(HotUpdateDefine.CALL_FUNC_READ_PROGRESS callbackProgress, int currentLoadIndex)
        {
            if (currentLoadIndex < 0 || currentLoadIndex > _loadRequests.Count - 1)
            {
                _loadRequests.Clear();
                return;
            }

            var loadRequestInfo = _loadRequests[currentLoadIndex];
            ResourcesEx.LoadResourcesOrLocalAsync(loadRequestInfo.path, loadRequestInfo.prefixPath, loadRequestInfo.type, (UnityEngine.Object obj) =>
            {
                if (null != loadRequestInfo.callbackLoadEnd)
                {
                    loadRequestInfo.callbackLoadEnd(obj);
                }
                _StartLoadResourcesOrLocalRequestInSequeue(callbackProgress, currentLoadIndex + 1);

            }, (float percent) =>
            {
                if (null != callbackProgress)
                {
                    float currentProgress = percent / (float)_loadRequests.Count + (1.0f / _loadRequests.Count * currentLoadIndex);
                    callbackProgress(currentProgress);
                }
            }, loadRequestInfo.extension, loadRequestInfo.multiVersionControlRelativePath);
        }
    }
}