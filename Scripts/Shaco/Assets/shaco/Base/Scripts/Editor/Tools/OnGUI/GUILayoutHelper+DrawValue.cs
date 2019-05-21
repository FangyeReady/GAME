#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUILayoutHelper
    {
        static private Dictionary<System.Type, object> dictionaryNewKeys = new Dictionary<System.Type, object>();
        static private Dictionary<System.Type, object> dictionaryNewValues = new Dictionary<System.Type, object>();

        //绘制序列化属性
        static public void DrawObject(object obj, params System.Reflection.BindingFlags[] flags)
        {
            System.Reflection.BindingFlags flagTmp = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
            for (int i = 0; i < flags.Length; ++i)
            {
                flagTmp |= flags[i];
            }

            var fields = obj.GetType().GetFields(flagTmp);
            foreach (var field in fields)
            {
                DrawFieldInfo(obj, field);
            }
        }

        static private void DrawFieldInfo(object obj, System.Reflection.FieldInfo field)
        {
            var valueTmp = field.GetValue(obj);

            GUI.changed = false;
            valueTmp = DrawValue(field.Name, valueTmp, field.FieldType, 0);
            if (GUI.changed)
            {
                field.SetValue(obj, valueTmp);
            }
        }

        static private object DrawValue(string valueName, object obj, System.Type type, int deepIndex)
        {
            var valueTmp = obj;
            var typeTmp = type;

            if (typeTmp == typeof(bool))
            {
                valueTmp = EditorGUILayout.Toggle(valueName, (bool)valueTmp);
            }
            else if (typeTmp == typeof(char))
            {
                valueTmp = EditorGUILayout.TextField(valueName, ((char)valueTmp).ToString()).ToChar();
            }
            else if (typeTmp == typeof(short))
            {
                valueTmp = EditorGUILayout.IntField(valueName, (short)valueTmp);
            }
            else if (typeTmp == typeof(int))
            {
                valueTmp = EditorGUILayout.IntField(valueName, (int)valueTmp);
            }
            else if (typeTmp == typeof(long))
            {
#if UNITY_5_3_OR_NEWER
                valueTmp = EditorGUILayout.LongField(valueName, (long)valueTmp);
#else
                valueTmp = EditorGUILayout.IntField(valueName, (int)valueTmp);
#endif
            }
            else if (typeTmp == typeof(float))
            {
                valueTmp = EditorGUILayout.FloatField(valueName, (float)valueTmp);
            }
            else if (typeTmp == typeof(double))
            {
#if UNITY_5_3_OR_NEWER
                valueTmp = EditorGUILayout.DoubleField(valueName, (double)valueTmp);
#else
                valueTmp = EditorGUILayout.FloatField(valueName, (float)valueTmp);
#endif
            }
            else if (typeTmp == typeof(string))
            {
                valueTmp = EditorGUILayout.TextField(valueName, (string)valueTmp);
            }
            else if (typeTmp.IsInherited<UnityEngine.Object>())
            {
                valueTmp = EditorGUILayout.ObjectField(valueName, (UnityEngine.Object)valueTmp, type, true);
            }
            else if (typeTmp == typeof(UnityEngine.Vector2))
            {
                valueTmp = EditorGUILayout.Vector2Field(valueName, (UnityEngine.Vector2)valueTmp);
            }
            else if (typeTmp == typeof(UnityEngine.Vector3))
            {
                valueTmp = EditorGUILayout.Vector3Field(valueName, (UnityEngine.Vector3)valueTmp);
            }
            else if (typeTmp == typeof(UnityEngine.Vector4))
            {
                valueTmp = EditorGUILayout.Vector4Field(valueName, (UnityEngine.Vector4)valueTmp);
            }
            else if (typeTmp == typeof(UnityEngine.Rect))
            {
                valueTmp = EditorGUILayout.RectField(valueName, (UnityEngine.Rect)valueTmp);
            }
            else if (typeTmp == typeof(UnityEngine.Color))
            {
                valueTmp = EditorGUILayout.ColorField(valueName, (UnityEngine.Color)valueTmp);
            }
            else if (typeTmp == typeof(UnityEngine.Bounds))
            {
                valueTmp = EditorGUILayout.BoundsField(valueName, (UnityEngine.Bounds)valueTmp);
            }
            else if (typeTmp == typeof(UnityEngine.Quaternion))
            {
                valueTmp = GUILayoutHelper.QuaternionField(valueName, (UnityEngine.Quaternion)valueTmp);
            }
            else if (typeTmp.IsInherited<System.Collections.ICollection>())
            {
                var collection = valueTmp as System.Collections.ICollection;
                bool isOpen = true;
                int count = 0;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 * (deepIndex + 1));
                    var keyTmp = valueName;
                    isOpen = GUILayoutHelper.DrawSimpleHeader(keyTmp, keyTmp + GetTypeDisplayName(typeTmp));
                }
                GUILayout.EndHorizontal();

                if (isOpen)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5 * (deepIndex + 1) + 16);
                        count = EditorGUILayout.IntField("Size", collection.Count);
                    }
                    GUILayout.EndHorizontal();

                    if (collection is IList)
                    {
                        DrawValueList(collection, count, deepIndex);
                    }
                    else if (collection is IDictionary)
                    {
                        DrawValueDictionary(collection, count, deepIndex);
                    }
                    else
                    {
                        EditorGUILayout.TextField(valueName + GetTypeDisplayName(typeTmp), valueTmp.ToString());
                    }
                }
                GUI.changed = false;
            }
            else
            {
                EditorGUILayout.TextField(valueName + GetTypeDisplayName(typeTmp), valueTmp.ToString());
            }

            return valueTmp;
        }

        static private void DrawValueList(ICollection collection, int count, int deepIndex)
        {
            var listTmp = collection as IList;
            if (null == listTmp)
                return;

            var defalutType = listTmp.GetType().GetGenericArguments()[0];
            var listRemoveIndex = new List<int>();

            //list remove
            if (count < collection.Count)
            {
                for (int i = listTmp.Count - count - 1; i >= 0; --i)
                {
                    listTmp.RemoveAt(listTmp.Count - 1);
                }
            }

            //list add
            else if (count > collection.Count)
            {
                for (int i = count - listTmp.Count - 1; i >= 0; --i)
                {
                    listTmp.Add(defalutType.Instantiate());
                }
            }

            for (int i = 0; i < listTmp.Count; ++i)
            {
                var item = listTmp[i];
                object valueTmp = null;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 * (deepIndex + 1) + 16);
                    GUILayout.BeginVertical();
                    {
                        valueTmp = DrawValue("Element " + i, item, defalutType, item is ICollection ? deepIndex + 1 : deepIndex);
                    }
                    GUILayout.EndVertical();

                    if (GUILayout.Button("-", GUILayout.Width(20f)))
                    {
                        listRemoveIndex.Add(i);
                    }
                }
                GUILayout.EndHorizontal();
                if (GUI.changed)
                {
                    listTmp[i] = valueTmp;
                }
            }

            //delay remove
            if (listRemoveIndex.Count > 0)
            {
                for (int i = 0; i < listRemoveIndex.Count; ++i)
                {
                    listTmp.RemoveAt(listRemoveIndex[i] - i);
                }
            }
        }

        static private void DrawValueDictionary(ICollection collection, int count, int deepIndex)
        {
            var dicTmp = collection as IDictionary;
            if (null == dicTmp)
                return;

            var defalutTypes = dicTmp.GetType().GetGenericArguments();
            var keyType = defalutTypes[0];
            var valueType = defalutTypes[1];

            var listKeys = new List<object>();
            var listRemovedKeys = new List<object>();
            var listChangedKeys = new List<object>();
            var listChangedValues = new List<object>();

            foreach (var key in dicTmp.Keys)
            {
                listKeys.Add(key);
                var value = dicTmp[key];
                int index = dicTmp.Count - 1;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 * (deepIndex + 1) + 16);
                    GUILayout.BeginVertical();
                    {
                        GUILayout.BeginHorizontal();
                        {
                            var keyTmp = DrawValue("Element " + index, key, keyType, key is ICollection ? deepIndex + 1 : deepIndex);
                            var valueTmp = DrawValue(string.Empty, value, valueType, value is ICollection ? deepIndex + 1 : deepIndex);
                            if (GUI.changed)
                            {
                                listRemovedKeys.Add(key);
                                listChangedKeys.Add(keyTmp);
                                listChangedValues.Add(valueTmp);
                            }
                            if (GUILayout.Button("-", GUILayout.Width(20f)))
                            {
                                listRemovedKeys.Add(key);
                            }
                        }
                        GUILayout.EndHorizontal();

                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            //delay update
            if (listChangedKeys.Count > 0)
            {
                for (int i = 0; i < listChangedKeys.Count; ++i)
                {
                    var removeKey = listRemovedKeys[i];
                    var changedKey = listChangedKeys[i];
                    var changedValue = listChangedValues[i];

                    if (null != changedKey)
                    {
                        if (changedKey != removeKey)
                        {
                            dicTmp.Remove(removeKey);
                            dicTmp[changedKey] = changedValue;
                        }
                        else
                        {
                            dicTmp[changedKey] = changedValue;
                        }
                    }
                    else
                    {
                        dicTmp.Remove(removeKey);
                    }
                }
            }

            //delay delete
            if (listRemovedKeys.Count > 0 && listRemovedKeys.Count != listChangedKeys.Count)
            {
                for (int i = 0; i < listRemovedKeys.Count; ++i)
                {
                    dicTmp.Remove(listRemovedKeys[i]);
                }
            }

            //dictionary remove
            if (count < collection.Count)
            {
                for (int i = dicTmp.Count - count - 1; i >= 0; --i)
                {
                    dicTmp.Remove(listKeys[listKeys.Count - 1]);
                    listKeys.RemoveAt(listKeys.Count - 1);
                }
            }

            //dictionary add
            if (!dictionaryNewKeys.ContainsKey(keyType))
            {
                dictionaryNewKeys.Add(keyType, keyType.Instantiate());
            }
            if (!dictionaryNewValues.ContainsKey(valueType))
            {
                dictionaryNewValues.Add(valueType, valueType.Instantiate());
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5 * (deepIndex + 1) + 16);

                dictionaryNewKeys[keyType] = DrawValue("Insert Element", dictionaryNewKeys[keyType], keyType, 0);
                dictionaryNewValues[valueType] = DrawValue(string.Empty, dictionaryNewValues[valueType], valueType, 0);

                var newKeyTmp = dictionaryNewKeys[keyType];
                if (null != newKeyTmp && !dicTmp.Contains(newKeyTmp) && GUILayout.Button("+", GUILayout.Width(20f)))
                {
                    dicTmp.Add(newKeyTmp, dictionaryNewValues[valueType]);
                    dictionaryNewKeys[keyType] = null;
                    dictionaryNewValues[valueType] = null;
                    GUI.FocusControl(string.Empty);
                }
            }
            GUILayout.EndHorizontal();
        }

        static private string GetTypeDisplayName(System.Type type)
        {
            return "(" + type.FullName + ")";
        }
    }
}

#endif