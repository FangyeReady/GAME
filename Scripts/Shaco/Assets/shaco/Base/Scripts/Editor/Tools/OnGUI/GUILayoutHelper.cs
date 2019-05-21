#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUILayoutHelper
    {
        static public void DrawSeparatorLine()
        {
            GUILayout.Space(5);
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            GUILayout.Space(5);
        }

        public static string SearchField(string value, params GUILayoutOption[] options)
        {
            System.Reflection.MethodInfo info = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(string), typeof(GUILayoutOption[]) }, null);
            if (info != null)
            {
                value = (string)info.Invoke(null, new object[] { value, options });
            }
            return value;
        }

        static public bool DrawHeader(string text, string key, System.Action onHeaderDrawCallBack = null, params GUILayoutOption[] options)
        {
            return DrawHeader(text, key, true, onHeaderDrawCallBack, options);
        }

        static public bool DrawHeader(string text, string key, bool defaultShow, string backgroundStyle, System.Action onHeaderDrawCallBack = null, params GUILayoutOption[] options)
        {
            bool state = shaco.DataSave.Instance.ReadBool(key, defaultShow);
            var oldColor = GUI.backgroundColor;

            if (!state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

            // text = "<b><size=11>" + text + "</size></b>";

            bool isEmptyText = string.IsNullOrEmpty(text);
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;

            GUILayout.BeginHorizontal(backgroundStyle, options);
            {
                if (isEmptyText)
                {
                    options = new GUILayoutOption[] { GUILayout.Width(50) };
                }

                // GUILayout.BeginVertical();
                {
                    // GUILayout.Space(5);
                    GUI.changed = false;
                    if (GUILayout.Button(text, EditorStyles.label, options)) state = !state;
                    if (GUI.changed) shaco.DataSave.Instance.Write(key, state);
                }
                // GUILayout.EndVertical();

                if (null != onHeaderDrawCallBack)
                    onHeaderDrawCallBack();
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = oldColor;
            return state;
        }

        static public bool DrawHeader(string text, string key, bool defaultShow, System.Action onHeaderDrawCallBack = null, params GUILayoutOption[] options)
        {
            return DrawHeader(text, key, defaultShow, "TE NodeBackground", onHeaderDrawCallBack, options);
        }

        static public bool DrawSimpleHeader(string text, string key)
        {
            bool state = shaco.DataSave.Instance.ReadBool(key, true);
            var oldColor = GUI.backgroundColor;

            if (!state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUI.changed = false;
                    if (GUILayout.Button(text, EditorStyles.label)) state = !state;
                    if (GUI.changed) shaco.DataSave.Instance.Write(key, state);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();


            GUI.backgroundColor = oldColor;
            return state;
        }

        static public void DrawHeaderText(string text)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(5);
                GUILayout.Label(text);
            }
            GUILayout.EndVertical();
        }

        static public UnityEditorInternal.ReorderableList DrawList(UnityEditorInternal.ReorderableList list, SerializedObject serializedObject, string valueName)
        {
            if (null == list)
            {
                list = new UnityEditorInternal.ReorderableList(serializedObject,
                    serializedObject.FindProperty(valueName),
                    true, true, true, true);

                list.index = 0;

                list.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, itemData, GUIContent.none);
                };

                list.drawHeaderCallback = (Rect rect) =>
                {
                    var titleTmp = valueName;

                    if (titleTmp.Length > 0)
                        titleTmp = titleTmp.Replace(char.ToUpper(titleTmp[0]).ToString(), 0, 1);

                    GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height), titleTmp + " [Count:" + list.count + "]");
                    if (list.count > 0 && GUI.Button(new Rect(rect.x + rect.width - 52, rect.y, 52, rect.height), "Clear"))
                    {
                        list.serializedProperty.ClearArray();
                    }
                };
            }

            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            return list;
        }

        static public bool DrawList<T>(List<T> list, string title, System.Func<T, bool> onCheckValueChangeCallBack) where T : new()
        {
            return DrawListBase<T>(list, title, onCheckValueChangeCallBack, () => new T());
        }

        static public bool DrawList(List<string> list, string title, System.Func<string, bool> onCheckValueChangeCallBack = null)
        {
            return DrawListBase<string>(list, title, onCheckValueChangeCallBack, () => string.Empty);
        }

        static private bool DrawListBase<T>(List<T> list, string title, System.Func<T, bool> onCheckValueChangeCallBack, System.Func<T> onCreateCallBack)
        {
            var removeIndexs = new List<int>();
            bool isOpened = false;

            GUILayout.BeginHorizontal();
            {
                isOpened = DrawHeader(title, title, null);

                GUILayout.BeginVertical();
                {
                    GUILayout.Space(0);

                    GUILayout.BeginHorizontal();
                    {
                        if (!isOpened)
                            GUILayout.Space(4.5f);
                        if (GUILayout.Button("+", GUILayout.Width(20f)))
                        {
                            list.Add(onCreateCallBack());

                            if (!isOpened)
                            {
                                shaco.DataSave.Instance.Write(title, true);
                                isOpened = true;
                            }
                        }
                        if (GUILayout.Button("-", GUILayout.Width(20f)))
                        {
                            list.Clear();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            if (!isOpened) return false;

            T oldValue = default(T);
            for (int i = 0; i < list.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("-", GUILayout.Width(20f)))
                    {
                        removeIndexs.Add(i - removeIndexs.Count);
                    }
                    GUI.changed = false;
                    oldValue = list[i];
                    list[i] = (T)DrawValueWithAutoType("Item " + i, (object)list[i]);
                    if (null != onCheckValueChangeCallBack && GUI.changed)
                    {
                        if (!onCheckValueChangeCallBack(list[i]))
                        {
                            list[i] = oldValue;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            for (int i = 0; i < removeIndexs.Count; ++i)
            {
                var removeItemTmp = list[removeIndexs[i]];
                list.RemoveAt(removeIndexs[i]);
            }
            return true;
        }

        static public object DrawValueWithAutoType(string valueName, object value)
        {
            var typeTmp = value.GetType();

            if (typeof(string) == typeTmp)
                value = (object)EditorGUILayout.TextField(valueName, (string)value);
             else if (typeTmp.IsInherited<UnityEngine.Object>())
                value = (object)EditorGUILayout.ObjectField(valueName, (Object)value, typeTmp, true);
            else if (typeof(int) == typeTmp)
                value = (object)EditorGUILayout.IntField(valueName, (int)value);
            else if (typeof(float) == typeTmp)
                value = (object)EditorGUILayout.FloatField(valueName, (float)value);
            
            else
            {
                shaco.Log.Error("GUILayoutHelper DrawList error: unsupport type, please add new code here to support it, type=" + typeTmp);
            }

            return value;
        }

        static public Quaternion QuaternionField(string label, Quaternion value, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            {
                value.x = EditorGUILayout.FloatField("x", value.x);
                value.y = EditorGUILayout.FloatField("y", value.y);
                value.z = EditorGUILayout.FloatField("z", value.z);
                value.w = EditorGUILayout.FloatField("w", value.w);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// 地址输入控件
        /// <param name="prefixGUI">UI前缀名字</param>
        /// <param name="path">地址</param>
        /// <param name="extensions">地址后缀名，如果不设定后缀名，默认为文件夹，如果设定后缀名默认为文件</param>
        /// <return>当前地址</return>
        /// </summary>
        static public string PathField(string prefixGUI, string path, string extensions)
        {
            GUILayout.BeginHorizontal();
            {
                int indexFind = path.IndexOf(Application.dataPath);
                if (indexFind >= 0)
                {
                    var assetObjectTmp = AssetDatabase.LoadAssetAtPath(EditorHelper.FullPathToUnityAssetPath(path), typeof(Object));

                    GUI.changed = false;
                    assetObjectTmp = EditorGUILayout.ObjectField(prefixGUI, assetObjectTmp, typeof(Object), true);
                    if (GUI.changed)
                    {
                        var fullPathTmp = EditorHelper.GetFullPath(assetObjectTmp);
                        if (!string.IsNullOrEmpty(extensions))
                        {
                            if (shaco.Base.FileHelper.ExistsFile(fullPathTmp))
                            {
                                path = fullPathTmp;
                            }
                            else 
                            {
                                Debug.LogError("GUILayoutHelper PathField error: not a file path=" + fullPathTmp);
                            }
                        }
                        else 
                        {
                            if (shaco.Base.FileHelper.ExistsDirectory(fullPathTmp))
                            {
                                path = fullPathTmp;
                            }
                            else 
                            {
                                Debug.LogError("GUILayoutHelper PathField error: not a directory path=" + fullPathTmp);
                            }
                        }
                    }
                }
                else 
                {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        EditorGUILayout.TextField(prefixGUI, path);
                    }
                    EditorGUI.EndDisabledGroup();
                }

                if (GUILayout.Button("Browse", GUILayout.Width(55)))
                {
                    var defaultPath = !shaco.Base.FileHelper.ExistsFile(path) && !shaco.Base.FileHelper.ExistsDirectory(path) ? Application.dataPath : path;
                    var selectPath = string.Empty;
                    if (string.IsNullOrEmpty(extensions))
                    {
                        selectPath = EditorUtility.OpenFolderPanel("Select a folder", defaultPath, extensions);
                    }
                    else 
                    {
                        selectPath = EditorUtility.OpenFilePanel("Select a file", defaultPath, extensions);
                    }
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        path = selectPath;
                        GUI.FocusControl(string.Empty);
                    }
                }
                if (!string.IsNullOrEmpty(path) && GUILayout.Button("Open", GUILayout.Width(55)))
                {
                    if (shaco.Base.FileHelper.ExistsDirectory(path))
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                    else if (shaco.Base.FileHelper.ExistsFile(path))
                    {
                        var folderPathTmp = shaco.Base.FileHelper.GetFolderNameByPath(path);
                        System.Diagnostics.Process.Start(folderPathTmp);
                    }
                    else 
                    {
                        Debug.Log("GUILayoutHelper PathFiled error: not found path=" + path);
                    }
                    GUI.changed = false;
                }
            }
            GUILayout.EndHorizontal();
            return path;
        }

        /// <summary>
        /// 属性输入弹出框
        /// <param name="prefix">前缀描述</param>
        /// <param name="select">当前选择</param>
        /// <return>当前选择</return>
        /// </summary>
        static public T PopupTypeField<T>(string prefix, T select)
        {
            var typeNames = shaco.Base.DataSave.Instance.ReadString("PopupTypeField+Types");
            string[] splitTypeNames = null;
            var selectIndex = -1;

            if (string.IsNullOrEmpty(typeNames))
            {
                splitTypeNames = GetPopupTypes<T>();
            }
            else
            {
                splitTypeNames = typeNames.Split(",");
            }

            //确认类型是否匹配，如果不匹配需要重新加载类型
            if (null != select)
            {
                selectIndex = splitTypeNames.IndexOf(select.ToTypeString());
            }

            //初始化当前选择
            if (selectIndex < 0 && !splitTypeNames.IsNullOrEmpty())
            {
                splitTypeNames = GetPopupTypes<T>();
                selectIndex = 0;
                select = (T)shaco.Base.Utility.Instantiate(splitTypeNames[selectIndex]);
            }

            GUI.changed = false;
            selectIndex = EditorGUILayout.Popup(prefix, selectIndex, splitTypeNames);
            if (GUI.changed)
            {
                select = (T)shaco.Base.Utility.Instantiate(splitTypeNames[selectIndex]);
                shaco.Base.DataSave.Instance.Remove("PopupTypeField+Types");
            }
            return select;
        }

        /// <summary>
        /// 获取编辑器弹出类型名字
        /// <return>类型名字</return>
        /// </summary>
        static private string[] GetPopupTypes<T>()
        {
            var strBuilder = new System.Text.StringBuilder();
            var classNamesTmp = shaco.Base.Utility.GetClassNames<T>();
            for (int i = 0; i < classNamesTmp.Length; ++i)
            {
                strBuilder.Append(classNamesTmp[i]);
                strBuilder.Append(",");
            }
            if (strBuilder.Length > 0)
            {
                strBuilder.Remove(strBuilder.Length - 1, 1);
            }

            var writeStringTmp = strBuilder.ToString();
            shaco.Base.DataSave.Instance.Write("PopupTypeField+Types", writeStringTmp);
            return writeStringTmp.Split(",");
        }
    }
}

#endif