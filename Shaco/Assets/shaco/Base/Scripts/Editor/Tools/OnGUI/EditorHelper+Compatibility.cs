using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class EditorHelper
    {
        static public void SetDirty(Object target)
        {
            if (target != null)
                EditorUtility.SetDirty(target);

#if UNITY_5_3_OR_NEWER
            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        static public string GetEditorPrefsKey(string key)
        {
#if UNITY_5_3_OR_NEWER
            return Application.companyName + "_" + Application.productName + "_" + Application.unityVersion + "_shaco_" + key;
#else
            return PlayerSettings.companyName + "_" + PlayerSettings.productName + "_" + Application.unityVersion + "_shaco_" + key;
#endif
        }

        static public T GetWindow<T>(T window, bool utility, string title) where T : UnityEditor.EditorWindow
        {
            T ret = window;

#if UNITY_5_3_OR_NEWER
            if (null != window && window.titleContent.text == typeof(T).FullName)
#else
            if (null != window && window.title == typeof(T).FullName)
#endif
            {
                return ret;
            }

            if (ret == null)
            {
                ret = FindWindow(ret);
            }

            if (ret == null)
            {
                ret = EditorWindow.GetWindow(typeof(T), utility, title) as T;

                ret.Show();
            }
            EditorWindow.FocusWindowIfItsOpen(typeof(T));
            return ret;
        }

        static public bool IsPressedControlButton()
        {
            if (null == Event.current) return false;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return Event.current.command;
#else
            return Event.current.control;
#endif
        }

        static public bool Foldout(bool foldout, string content)
        {
#if UNITY_5_3_OR_NEWER
            return EditorGUILayout.Foldout(foldout, content, true);
#else
            return EditorGUILayout.Foldout(foldout, content);
#endif
        }

        static public bool Foldout(bool foldout, GUIContent content)
        {
#if UNITY_5_3_OR_NEWER
            return EditorGUILayout.Foldout(foldout, content, true);
#else
            return EditorGUILayout.Foldout(foldout, content);
#endif
        }

        static public void SaveCurrentScene()
        {
#if UNITY_5_3_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
#else
            EditorApplication.SaveScene();
#endif
        }

        static public void OpenScene(string sceneName)
        {
#if UNITY_5_3_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneName);
#else
            EditorApplication.OpenScene(sceneName);
#endif
        }
    }
}