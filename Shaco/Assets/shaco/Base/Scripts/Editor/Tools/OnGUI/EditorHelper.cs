#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco;

namespace shacoEditor
{
    public partial class EditorHelper
    {
        /// <summary>
        /// 绘制List的帮助类，现在基本都用GUILayoutHelper.DrawList代替了
        /// </summary>
        public class ListHelper<T>
        {
            public delegate bool IS_NULL_FUNC(T t1);
            public delegate void INDEX_CALLBACK(int index);
            public delegate T DEFAULT_CALLFUNC();

            public DEFAULT_CALLFUNC OnCreateCallBack = null;

            public void AutoListSize(string prefixName, List<T> listData, IS_NULL_FUNC isNullCallFunc, INDEX_CALLBACK onDeleteEndCallFunc = null, INDEX_CALLBACK onCreateEndCallFunc = null)
            {
                if (listData.Count == 0 || !isNullCallFunc(listData[listData.Count - 1]))
                {
                    listData.Add(OnCreateCallBack != null ? OnCreateCallBack() : default(T));

                    if (onCreateEndCallFunc != null)
                        onCreateEndCallFunc(listData.Count - 1);
                }

                for (int i = listData.Count - 2; i >= 0; --i)
                {
                    if (isNullCallFunc(listData[i]))
                    {
                        if (onDeleteEndCallFunc != null)
                            onDeleteEndCallFunc(i);
                        listData.RemoveAt(i);

                        EditorHelper.SetDirty(null);
                    }
                }

                if (!string.IsNullOrEmpty(prefixName))
                    EditorGUILayout.IntField(prefixName, listData.Count - 1);
            }
        }

        public class PositionInput
        {
            static private Rect _rect = new Rect(0, 0, 50, 50);
            static public Rect Draw()
            {
                _rect = EditorGUILayout.RectField(_rect);
                return _rect;
            }
        }

        static public string GetAssetPathLower(Object asset)
        {
            return AssetDatabase.GetAssetPath(asset).ToLower();
        }
        static public T FindWindow<T>(T window = null) where T : UnityEditor.EditorWindow
        {
            T retValue = window;
            if (null == window)
            {
                EditorWindow.FocusWindowIfItsOpen(typeof(T));
                var findWindow = EditorWindow.focusedWindow as T;
                if (null != findWindow)
                {
                    retValue = findWindow;
                }
            }
            return retValue;
        }

        static public void OpenAsset(string path, int line)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("EditorHelper OpenAsset error: path is invalid");
                return;
            }

            path = path.Replace("\\", shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            var indexTmp = path.IndexOf("Assets/");
            if (indexTmp >= 0)
                path = path.Substring(indexTmp, path.Length - indexTmp);

            var loadAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            AssetDatabase.OpenAsset(loadAsset, line);
        }

        static public void DrawForegourndFrame(float thickness = 2)
        {
            var rectTmp = GUILayoutUtility.GetLastRect();
            var colorOld = GUI.color;
            GUI.color = new Color(62.0f / 255, 95.0f / 255, 150.0f / 255);

            var rectUp = new Rect(rectTmp.x, rectTmp.y, rectTmp.width, thickness);
            var rectDown = new Rect(rectTmp.x, rectTmp.y + (rectTmp.height - thickness), rectTmp.width, thickness);
            var rectLeft = new Rect(rectTmp.x, rectTmp.y, thickness, rectTmp.height);
            var rectRight = new Rect(rectTmp.x + (rectTmp.width - thickness), rectTmp.y, thickness, rectTmp.height);

            GUI.DrawTexture(rectUp, Texture2D.whiteTexture);
            GUI.DrawTexture(rectDown, Texture2D.whiteTexture);
            GUI.DrawTexture(rectLeft, Texture2D.whiteTexture);
            GUI.DrawTexture(rectRight, Texture2D.whiteTexture);

            GUI.color = colorOld;
        }

        //获取编辑器中勾选上的场景
        static public string[] GetEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;
                EditorScenes.Add(scene.path);
            }

            return EditorScenes.ToArray();
        }

        static public void PrintSerializedObject(Object obj)
        {
            SerializedObject serializedObject = new SerializedObject(obj);
            var iter = serializedObject.GetIterator();
            while (iter.Next(true))
            {
                if (iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue != null)
                {
                    Debug.Log("name=" + iter.name + " obj=" + iter.objectReferenceValue);
                }
                else
                {
                    Debug.Log("name=" + iter.name);
                }
            }
        }

        /// <summary>
        /// 获取绝对路径
        /// <param name="obj">unity对象</param>
        /// <return>绝对路径</return>
        /// </summary>
        static public string GetFullPath(Object obj)
        {
            return shaco.Base.FileHelper.ContactPath(Application.dataPath.Remove("Assets"), AssetDatabase.GetAssetPath(obj));
        }

        /// <summary>
        /// 获取绝对路径
        /// <param name="obj">unity对象</param>
        /// <return>绝对路径</return>
        /// </summary>
        static public string GetFullPath(string relativePath)
        {
            if (!relativePath.Contains(Application.dataPath))
            {
                relativePath = shaco.Base.FileHelper.ContactPath(Application.dataPath.Remove("Assets"), relativePath);
            }
            return relativePath;
        }

        /// <summary>
        /// 获取unity对象相对路径
        /// <param name="path">绝对路径</param>
        /// <return>unity对象相对路径</return>
        /// </summary>
        static public string FullPathToUnityAssetPath(string path)
        {
            var currentPath = shaco.Base.FileHelper.GetCurrentSourceFilePath();
            var applicationPath = currentPath.RemoveBehind("Assets/");
            return path.RemoveFront(applicationPath);
        }
    }
}
#endif