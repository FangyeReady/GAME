using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco
{
    public class SceneManager : MonoBehaviour
    {
        public delegate void SceneLoadEndCallFunc();

        public delegate void SceneLoadingCallFunc(float progress);

        static public bool OpenDebugMode
        {
            set
            {
                _openDebugMode = value;
#if DEBUG_LOG && DEBUG_WINDOW
                bool hasOutLogScript = shaco.GameEntry.GetComponentInstance<SceneManager>().GetComponent<UnityGameFramework.Runtime.DebuggerComponent>() != null;
                if (_openDebugMode)
                {
                    if (!hasOutLogScript)
                    {
                        var target = shaco.GameEntry.GetComponentInstance<SceneManager>().gameObject;
                        target.AddComponent<UnityGameFramework.Runtime.DebuggerComponent>();
                        if (null == target.GetComponent<UnityGameFramework.Runtime.SettingComponent>())
                        {
                            target.AddComponent<UnityGameFramework.Runtime.SettingComponent>();
                        }
                        // Screen.orientation = ScreenOrientation.AutoRotation;
                        // Screen.autorotateToLandscapeLeft = true;
                        // Screen.autorotateToLandscapeRight = true;
                        // Screen.autorotateToPortrait = true;
                        // Screen.autorotateToPortraitUpsideDown = true;
                    }
                }
                else
                {
                    if (hasOutLogScript)
                    {
                        DestroyImmediate(shaco.GameEntry.GetComponentInstance<SceneManager>().GetComponent<UnityGameFramework.Runtime.DebuggerComponent>(), true);
                    }
                }
#endif
            }
            get { return _openDebugMode; }
        }

        public Camera GameCameraMain;
        public SceneLoadEndCallFunc CallFuncLoadEnd = null;
        public SceneLoadingCallFunc CallFunLoading = null;

#if DEBUG_LOG
        static private bool _openDebugMode = true;
#else
        static private bool _openDebugMode = false;
#endif
        private AsyncOperation _asyncInfo = null;

        private string _currentSceneName = string.Empty;

        void Start()
        {
            shaco.GameEntry.GetComponentInstance<SceneManager>();

#if UNITY_5_3_OR_NEWER
            _currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
#else 
            _currentSceneName = Application.loadedLevelName;
#endif

            //这里刷新一次Mode标志，以保证Debug脚本一定可以被挂上去
            if (_openDebugMode)
            {
                OpenDebugMode = true;
            }
        }

        void OnDestroy()
        {
            _asyncInfo = null;
            HotUpdateDataCache.Unload();

#if DEBUG_WINDOW && !UNITY_EDITOR
            UnityGameFramework.Runtime.GameEntry.Shutdown(UnityGameFramework.Runtime.ShutdownType.None);
#endif
        }

        static public bool IsLoading()
        {
            return shaco.GameEntry.GetComponentInstance<SceneManager>()._asyncInfo != null;
        }

        static public void LoadScene(string sceneName)
        {
            var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
            if (manager._asyncInfo != null)
                return;

#if UNITY_5_3_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
#else
            Application.LoadLevel(sceneName);
#endif
        }

        static public void LoadScene(int index)
        {
            var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
            if (manager._asyncInfo != null)
                return;

#if UNITY_5_3_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.LoadScene(index);
#else
            Application.LoadLevel(index);
#endif
        }

        static public void LoadSceneAsync(string sceneName)
        {
            var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
            if (manager._asyncInfo != null)
                return;

            manager.StartCoroutine(manager.LoadSceneAsyncBase(sceneName));
        }

        static public void LoadSceneAdditiveAsync(string sceneName)
        {
            var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
            if (manager._asyncInfo != null)
                return;

            manager.StartCoroutine(manager.LoadSceneAdditiveAsyncBase(sceneName));
        }

        static public string GetCurrentSceneName()
        {
            return shaco.GameEntry.GetComponentInstance<SceneManager>()._currentSceneName;
        }

        private IEnumerator LoadSceneAsyncBase(string sceneName)
        {
            shaco.GameEntry.GetComponentInstance<SceneManager>()._currentSceneName = sceneName;

#if UNITY_5_3_OR_NEWER
            _asyncInfo = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
#else
            _asyncInfo = Application.LoadLevelAsync(sceneName);
#endif

            while (!_asyncInfo.isDone)
            {
                if (_asyncInfo.progress < 1.0f)
                {
                    if (CallFunLoading != null)
                    {
                        CallFunLoading(_asyncInfo.progress);
                    }
                }
                yield return 1;
            }

            OnloadSceneEnd();

            shaco.Log.Info("LoadSceneAsync completed !! current scene=" + _currentSceneName);
        }

        private IEnumerator LoadSceneAdditiveAsyncBase(string sceneName)
        {
            shaco.GameEntry.GetComponentInstance<SceneManager>()._currentSceneName = sceneName;
#if UNITY_5_3_OR_NEWER
            _asyncInfo = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
#else
            _asyncInfo = Application.LoadLevelAdditiveAsync(sceneName);
#endif

            while (!_asyncInfo.isDone)
            {
                if (_asyncInfo.progress < 1.0f)
                {
                    if (CallFunLoading != null)
                    {
                        CallFunLoading(_asyncInfo.progress);
                    }
                }
                yield return 1;
            }

            OnloadSceneEnd();

            shaco.Log.Info("LoadSceneAdditiveAsync completed current scene=" + _currentSceneName);
        }

        private void OnloadSceneEnd()
        {
            if (_asyncInfo != null)
            {
                if (CallFunLoading != null)
                    CallFunLoading(1.0f);
                _asyncInfo = null;
            }

            if (CallFuncLoadEnd != null)
            {
                CallFuncLoadEnd();
                CallFuncLoadEnd = null;
            }
        }
    }
}
