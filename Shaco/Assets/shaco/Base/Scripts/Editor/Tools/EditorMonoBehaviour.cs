#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace shacoEditor
{
    [InitializeOnLoad]
    public class EditorMonoBehaviour
    {
        public enum PlayModeState
        {
            Playing,
            Paused,
            Stop,
            PlayingOrWillChangePlaymode
        }

        static private bool _isPlayed = false;
        static private bool _isPausedButton = false;

        private System.DateTime _nowTime = System.DateTime.Now;

        static EditorMonoBehaviour()
        {
            new EditorMonoBehaviour().OnEditorMonoBehaviour();
        }

        private void OnEditorMonoBehaviour()
        {
            EditorApplication.update += Update;
            //EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
            //EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            //EditorApplication.projectWindowChanged += OnProjectWindowChanged;
            //EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
            //EditorApplication.modifierKeysChanged += OnModifierKeysChanged;

            // globalEventHandler
            //EditorApplication.CallbackFunction function = () => OnGlobalEventHandler(Event.current);
            //FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            //EditorApplication.CallbackFunction functions = (EditorApplication.CallbackFunction)info.GetValue(null);
            //functions += function;
            //info.SetValue(null, (object)functions);

            //EditorApplication.searchChanged += OnSearchChanged;

            EditorApplication.playmodeStateChanged += () =>
            {
                if (EditorApplication.isPaused)
                {
                    _isPausedButton = true;
                    OnPlaymodeStateChanged(PlayModeState.Paused);
                }
                else if (EditorApplication.isPlaying)
                {
                    if (!_isPausedButton)
                    {
                        OnPlaymodeStateChanged(!_isPlayed ? PlayModeState.Playing : PlayModeState.Stop);
                        _isPlayed = !_isPlayed;
                    }
                    else 
                    {
                        _isPausedButton = false;
                    }
                }
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    OnPlaymodeStateChanged(PlayModeState.PlayingOrWillChangePlaymode);
                }
            };
        }

        public virtual void Update()
        {
            //SceneManager.Log ("每一帧回调一次");

            if (!Application.isPlaying)
            {
                // shaco.GameInitComponent.CheckConstantsData();

                var curTotalSeconds = (DateTime.Now - _nowTime).TotalSeconds;
                _nowTime = DateTime.Now;
                shaco.Base.GameEntry.GetInstance<shaco.ActionS>().MainUpdate((float)curTotalSeconds);
                shaco.Base.BehaviourRootTree.BaseUpdate((float)curTotalSeconds);
            }
        }

        public virtual void OnHierarchyWindowChanged()
        {
            //Log.Info("层次视图发生变化");
        }

        public virtual void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            //	SceneManager.Log (string.Format ("{0} : {1} - {2}", EditorUtility.InstanceIDToObject (instanceID), instanceID, selectionRect));
        }

        public virtual void OnProjectWindowChanged()
        {
            //	SceneManager.Log ("当资源视图发生变化");

        }

        public virtual void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            //根据GUID得到资源的准确路径
            //SceneManager.Log (string.Format ("{0} : {1} - {2}", AssetDatabase.GUIDToAssetPath (guid), guid, selectionRect));
        }

        public virtual void OnModifierKeysChanged()
        {
            //	SceneManager.Log ("当触发键盘事件");
        }

        public virtual void OnGlobalEventHandler(Event e)
        {
            //SceneManager.Log ("全局事件回调: " + e);
        }

        public virtual void OnSearchChanged()
        {
        }

        public virtual void OnPlaymodeStateChanged(PlayModeState playModeState)
        {
            if (playModeState == PlayModeState.Playing)
            {
            }
            else if (PlayModeState.PlayingOrWillChangePlaymode == playModeState)
            {
                if (!_isPlayed)
                    shaco.ActionS.StopAllAction(true);
            }
            else if (playModeState == PlayModeState.Stop)
            {
                shaco.GameEntry.ClearIntances();
                shaco.ActionS.StopAllAction(true);
                shaco.Base.BehaviourRootTree.StopAll();
                UnityEditor.EditorUtility.ClearProgressBar();
            }
        }

        [UnityEditor.Callbacks.OnOpenAssetAttribute()]
        private static bool OnOpenAsset(int instanceID, int line)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Object selected = EditorUtility.InstanceIDToObject(instanceID);
            if (null != selected && null != selected as SceneAsset)
#else
            var pathTmp = AssetDatabase.GetAssetPath(instanceID);
            if (shaco.Base.FileHelper.GetFilNameExtension(pathTmp) == "unity")
#endif
            {
                //change scene in editor
                shaco.ActionS.StopAllAction();
            }
            return false;
        }
    }
}
#endif