//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Entry = EventDelegateS.Entry;

namespace shacoEditor
{
    public static class EventDelegateEditorS
    {
        /// <summary>
        /// Collect a list of usable delegates from the specified target game object.
        /// </summary>

        static public List<Entry> GetMethods(GameObject target)
        {
            MonoBehaviour[] comps = target.GetComponents<MonoBehaviour>();

            List<Entry> list = new List<Entry>();

            MethodInfo infoCheck = null;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            for (int i = 0, imax = comps.Length; i < imax; ++i)
            {
                MonoBehaviour mb = comps[i];
                if (mb == null) continue;

                var typeTmp = mb.GetType();
                MethodInfo[] methods = typeTmp.GetMethods(flags);

                for (int b = 0; b < methods.Length; ++b)
                {
                    MethodInfo mi = methods[b];

                    //add by shaco 2016/6/12
                    //TODO: fixed Ambiguous matching in method resolution error ~
                    try
                    {
                        infoCheck = typeTmp.GetMethod(mi.Name, flags);
                    }
                    catch (System.Exception)
                    {
                        infoCheck = null;
                    }
                    //add end

                    if (mi.ReturnType == typeof(void) && infoCheck != null)
                    {
                        string name = mi.Name;
                        if (name == "Invoke") continue;
                        if (name == "InvokeRepeating") continue;
                        if (name == "CancelInvoke") continue;
                        if (name == "StopCoroutine") continue;
                        if (name == "StopAllCoroutines") continue;
                        if (name == "BroadcastMessage") continue;
                        if (name.StartsWith("SendMessage")) continue;
                        if (name.StartsWith("set_")) continue;

                        Entry ent = new Entry();
                        ent.target = mb;
                        ent.name = mi.Name;
                        list.Add(ent);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Convert the specified list of delegate entries into a string array.
        /// </summary>

        static public string[] GetNames(List<Entry> list, string choice, out int index)
        {
            index = 0;
            string[] names = new string[list.Count + 1];
            names[0] = "<GameObject>";

            for (int i = 0; i < list.Count;)
            {
                Entry ent = list[i];
                string del = EventDelegateEditorS.GetFuncName(ent.target, ent.name);
                names[++i] = del;
                if (index == 0 && string.Equals(del, choice))
                    index = i;
            }
            return names;
        }

        /// <summary>
        /// Convenience function that converts Class + Function combo into Class.Function representation.
        /// </summary>

        static public string GetFuncName(object obj, string method)
        {
            if (obj == null) return "<null>";
            string type = obj.GetType().ToString();
            int period = type.LastIndexOf('/');
            if (period > 0) type = type.Substring(period + 1);
            return string.IsNullOrEmpty(method) ? type : type + shaco.Base.FileDefine.PATH_FLAG_SPLIT + method;
        }

        /// <summary>
        /// Whether we can convert one type to another for assignment purposes.
        /// </summary>

        static public bool Convert(System.Type from, System.Type to)
        {
            object temp = null;
            return Convert(ref temp, from, to);
        }

        /// <summary>
        /// Whether we can convert one type to another for assignment purposes.
        /// </summary>

        static public bool Convert(object value, System.Type to)
        {
            if (value == null)
            {
                value = null;
                return Convert(ref value, to, to);
            }
            return Convert(ref value, value.GetType(), to);
        }

        /// <summary>
        /// Whether we can convert one type to another for assignment purposes.
        /// </summary>

        static public bool Convert(ref object value, System.Type from, System.Type to)
        {
#if REFLECTION_SUPPORT
// If the value can be assigned as-is, we're done
#if NETFX_CORE
if (to.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo())) return true;
#else
if (to.IsAssignableFrom(from)) return true;
#endif

#else
            if (from == to) return true;
#endif
            // If the target type is a string, just convert the value
            if (to == typeof(string))
            {
                value = (value != null) ? value.ToString() : "null";
                return true;
            }

            // If the value is null we should not proceed further
            if (value == null) return false;

            if (to == typeof(int))
            {
                if (from == typeof(string))
                {
                    int val;

                    if (int.TryParse((string)value, out val))
                    {
                        value = val;
                        return true;
                    }
                }
                else if (from == typeof(float))
                {
                    value = Mathf.RoundToInt((float)value);
                    return true;
                }
            }
            else if (to == typeof(float))
            {
                if (from == typeof(string))
                {
                    float val;

                    if (float.TryParse((string)value, out val))
                    {
                        value = val;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Convenience function that converts the specified component + property pair into its string representation.
        /// </summary>

        static public string ToString(Component comp, string property)
        {
            if (comp != null)
            {
                string typeName = comp.GetType().ToString();
                int period = typeName.LastIndexOf('.');
                if (period > 0) typeName = typeName.Substring(period + 1);

                if (!string.IsNullOrEmpty(property)) return typeName + "." + property;
                else return typeName + ".[property]";
            }
            return null;
        }

        /// <summary>
        /// Draw an editor field for the Unity Delegate.
        /// </summary>

        static public bool Field(Object undoObject, EventDelegateS del, float horizontalSpace = 0)
        {
            return Field(undoObject, del, true, NGUIEditorToolsS.minimalisticLook, horizontalSpace);
        }

        /// <summary>
        /// Draw an editor field for the Unity Delegate.
        /// </summary>

        static public bool Field(Object undoObject, EventDelegateS del, bool removeButton, bool minimalistic, float horizontalSpace = 0)
        {
            if (del == null) return false;
            bool prev = GUI.changed;
            GUI.changed = false;
            bool retVal = false;
            MonoBehaviour target = del.target;
            bool remove = false;

            GUILayout.BeginHorizontal();

            GUILayout.Space(del.target != null ? horizontalSpace / 2 : horizontalSpace);
            if (del.target != null && GUILayout.Button("-", GUILayout.Width(20f)))
            {
                target = null;
                remove = true;
            }

            NGUIEditorToolsS.SetLabelWidth(82f);

            if (removeButton && (del.target != null || del.isValid))
            {
                if (del.target == null && del.isValid)
                {
                    EditorGUILayout.LabelField("Notify", del.ToString());
                }
                else
                {
                    target = EditorGUILayout.ObjectField("Notify", del.target, typeof(MonoBehaviour), true) as MonoBehaviour;
                }
            }
            else
            {
                target = EditorGUILayout.ObjectField("Notify", del.target, typeof(MonoBehaviour), true) as MonoBehaviour;
            }

            GUILayout.EndHorizontal();

            if (remove)
            {
                NGUIEditorToolsS.RegisterUndo("Delegate Selection", undoObject);
                del.Clear();
                EditorUtility.SetDirty(undoObject);
            }
            else if (del.target != target)
            {
                NGUIEditorToolsS.RegisterUndo("Delegate Selection", undoObject);
                del.target = target;
                EditorUtility.SetDirty(undoObject);
            }

            if (del.target != null && del.target.gameObject != null)
            {
                //modify by shaco 2016/12/18
                if (del.ListMethodEntry.Count == 0)
                {
                    GameObject go = del.target.gameObject;
                    del.ListMethodEntry = EventDelegateEditorS.GetMethods(go);
                }
                //modify end

                int index = 0;
                int choice = 0;
                string[] names = PropertyReferenceDrawerS.GetNames(del.ListMethodEntry, del.ToString(), out index);

                GUILayout.BeginHorizontal();
                GUILayout.Space(horizontalSpace);
                choice = EditorGUILayout.Popup("Method", index, names);
                NGUIEditorToolsS.DrawPadding();
                GUILayout.EndHorizontal();

                if (choice > 0 && choice != index)
                {
                    Entry entry = del.ListMethodEntry[choice - 1];
                    NGUIEditorToolsS.RegisterUndo("Delegate Selection", undoObject);
                    del.target = entry.target as MonoBehaviour;
                    del.methodName = entry.name;
                    EditorUtility.SetDirty(undoObject);
                    retVal = true;
                }

                GUI.changed = false;
                EventDelegateS.Parameter[] ps = del.parameters;

                if (ps != null)
                {
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        EventDelegateS.Parameter param = ps[i];

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(horizontalSpace);
                        Object obj = EditorGUILayout.ObjectField("Arg " + i, param.obj, typeof(Object), true);
                        GUILayout.EndHorizontal();

                        if (GUI.changed)
                        {
                            GUI.changed = false;
                            param.obj = obj;
                            EditorUtility.SetDirty(undoObject);
                        }

                        if (obj == null) continue;

                        GameObject selGO = null;
                        System.Type type = obj.GetType();
                        if (type == typeof(GameObject)) selGO = obj as GameObject;
                        else if (type.IsSubclassOf(typeof(Component))) selGO = (obj as Component).gameObject;

                        if (selGO != null)
                        {
                            // Parameters must be exact -- they can't be converted like property bindings
                            PropertyReferenceDrawerS.filter = param.expectedType;
                            PropertyReferenceDrawerS.canConvert = false;
                            List<EventDelegateS.Entry> ents = PropertyReferenceDrawerS.GetProperties(selGO, true, false);

                            int selection;
                            string[] props = GetNames(ents, EventDelegateEditorS.GetFuncName(param.obj, param.field), out selection);

                            GUILayout.BeginHorizontal();
                            int newSel = EditorGUILayout.Popup(" ", selection, props);
                            NGUIEditorToolsS.DrawPadding();
                            GUILayout.EndHorizontal();

                            if (GUI.changed)
                            {
                                GUI.changed = false;

                                if (newSel == 0)
                                {
                                    param.obj = selGO;
                                    param.field = null;
                                }
                                else
                                {
                                    param.obj = ents[newSel - 1].target;
                                    param.field = ents[newSel - 1].name;
                                }
                                EditorUtility.SetDirty(undoObject);
                            }
                        }
                        else if (!string.IsNullOrEmpty(param.field))
                        {
                            param.field = null;
                            EditorUtility.SetDirty(undoObject);
                        }

                        PropertyReferenceDrawerS.filter = typeof(void);
                        PropertyReferenceDrawerS.canConvert = true;
                    }
                }
            }
            else retVal = GUI.changed;
            GUI.changed = prev;
            return retVal;
        }

        /// <summary>
        /// Draw a list of fields for the specified list of delegates.
        /// </summary>

        static public void Field(Object undoObject, List<EventDelegateS> list, float horizontalSpace = 0)
        {
            Field(undoObject, list, null, null, NGUIEditorToolsS.minimalisticLook, horizontalSpace);
        }

        /// <summary>
        /// Draw a list of fields for the specified list of delegates.
        /// </summary>

        static public void Field(Object undoObject, List<EventDelegateS> list, bool minimalistic, float horizontalSpace = 0)
        {
            Field(undoObject, list, null, null, minimalistic, horizontalSpace);
        }

        /// <summary>
        /// Draw a list of fields for the specified list of delegates.
        /// </summary>

        static public void Field(Object undoObject, List<EventDelegateS> list, string noTarget, string notValid, bool minimalistic, float horizontalSpace = 0)
        {
            if (list == null) return;

            bool targetPresent = false;
            bool isValid = false;

            // Draw existing delegates
            for (int i = 0; i < list.Count;)
            {
                EventDelegateS del = list[i];

                if (del == null || (del.target == null && !del.isValid))
                {
                    list.RemoveAt(i);
                    continue;
                }

                Field(undoObject, del, true, minimalistic, horizontalSpace);
                EditorGUILayout.Space();

                if (del.target == null && !del.isValid)
                {
                    list.RemoveAt(i);
                    continue;
                }
                else
                {
                    if (del.target != null) targetPresent = true;
                    isValid = true;
                }
                ++i;
            }

            // Draw a new delegate
            EventDelegateS newDel = new EventDelegateS();
            Field(undoObject, newDel, true, minimalistic, horizontalSpace);

            if (newDel.target != null)
            {
                targetPresent = true;
                list.Add(newDel);
            }

            if (!targetPresent)
            {
                if (!string.IsNullOrEmpty(noTarget))
                {
                    GUILayout.Space(6f);
                    EditorGUILayout.HelpBox(noTarget, MessageType.Info, true);
                    GUILayout.Space(6f);
                }
            }
            else if (!isValid)
            {
                if (!string.IsNullOrEmpty(notValid))
                {
                    GUILayout.Space(6f);
                    EditorGUILayout.HelpBox(notValid, MessageType.Warning, true);
                    GUILayout.Space(6f);
                }
            }
        }


        //todo: 比EventDelegateEditorS支持更好的GetMethods
        static public List<EventDelegateS.Entry> GetMethods(GameObject target, params System.Type[] typeReturn)
        {
            if (typeReturn.Length == 0)
            {
                typeReturn = new System.Type[1] { typeof(void) };
            }
            MonoBehaviour[] comps = target.GetComponents<MonoBehaviour>();

            List<EventDelegateS.Entry> list = new List<EventDelegateS.Entry>();

            System.Reflection.MethodInfo infoCheck = null;
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

            for (int i = 0, imax = comps.Length; i < imax; ++i)
            {
                MonoBehaviour mb = comps[i];
                if (mb == null) continue;

                var typeTmp = mb.GetType();
                System.Reflection.MethodInfo[] methods = typeTmp.GetMethods(flags);

                for (int b = 0; b < methods.Length; ++b)
                {
                    System.Reflection.MethodInfo mi = methods[b];

                    //add by shaco 2016/6/12
                    //TODO: fixed Ambiguous matching in method resolution error ~
                    try
                    {
                        infoCheck = typeTmp.GetMethod(mi.Name, flags);
                    }
                    catch (System.Exception)
                    {
                        infoCheck = null;
                    }
                    //add end

                    if (infoCheck != null)
                    {
                        bool isCheckOK = false;
                        for (int j = 0; j < typeReturn.Length; ++j)
                        {
                            if (typeReturn[j] == mi.ReturnType)
                            {
                                isCheckOK = true;
                                break;
                            }
                        }
                        if (isCheckOK)
                        {
                            string name = mi.Name;
                            if (name == "Invoke") continue;
                            if (name == "InvokeRepeating") continue;
                            if (name == "CancelInvoke") continue;
                            if (name == "StopCoroutine") continue;
                            if (name == "StopAllCoroutines") continue;
                            if (name == "BroadcastMessage") continue;
                            if (name.StartsWith("SendMessage")) continue;
                            if (name.StartsWith("set_")) continue;
                            if (name.StartsWith("get_")) continue;

                            EventDelegateS.Entry ent = new EventDelegateS.Entry();
                            ent.target = mb;
                            ent.name = mi.Name;
                            list.Add(ent);
                        }
                    }
                }
            }
            return list;
        }

        static public bool DrawEvent(GameObject undoObject, EventDelegateS eventDelegate, params System.Type[] typeReturn)
        {
            bool isValueChanged = false;

            if (typeReturn.Length > 0)
            {
                if (eventDelegate.getRequstMethodReturnType() != typeReturn[0])
                    eventDelegate.setRequstMethodReturnType(typeReturn[0]);
            }
            else if (typeReturn.Length == 0)
            {
                typeReturn = new System.Type[1] { typeof(void) };
            }

            if (eventDelegate.ListMethodEntry.Count == 0 && eventDelegate.target != null)
                eventDelegate.ListMethodEntry = GetMethods(eventDelegate.target.gameObject, typeReturn);

            MonoBehaviour targetSetTmp = eventDelegate.target;

            GUILayout.BeginHorizontal();

            //set return type
            if (typeReturn.Length > 1)
            {
                shaco.Log.Info("typeReturn len must <= 1");
            }

            //draw target
            var prevValue = eventDelegate.target;
            targetSetTmp = EditorGUILayout.ObjectField(targetSetTmp, typeof(MonoBehaviour), true) as MonoBehaviour;
            if (prevValue != targetSetTmp)
            {
                isValueChanged = true;
                eventDelegate.target = targetSetTmp;
            }

            //draw method
            int index = 0;
            string[] names = PropertyReferenceDrawerS.GetNames(eventDelegate.ListMethodEntry, eventDelegate.ToString(), out index);

            GUILayout.Label("Method");
            int choice = EditorGUILayout.Popup(index, names);

            if (choice > 0 && choice != index)
            {
                EventDelegateS.Entry entry = eventDelegate.ListMethodEntry[choice - 1];
                eventDelegate.target = entry.target as MonoBehaviour;
                eventDelegate.methodName = entry.name;
                isValueChanged = true;
            }

            GUILayout.EndHorizontal();

            //draw parameters
            if (eventDelegate != null && eventDelegate.target != null && !string.IsNullOrEmpty(eventDelegate.methodName))
            {
                if (eventDelegate.parameters != null)
                {
                    for (int i = 0; i < eventDelegate.parameters.Length; ++i)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(16f);
                        if (DrawPrameter(eventDelegate.getParameterInfo(i).Name, eventDelegate.parameters[i]))
                            isValueChanged = true;
                        GUILayout.EndHorizontal();
                    }
                }
            }

            if (isValueChanged)
                EditorHelper.SetDirty(undoObject);

            return isValueChanged;
        }

        //todo: 如果参数有改变，返回true
        static public bool DrawPrameter(string parameterName, EventDelegateS.Parameter parameter)
        {
            var prevValue = parameter.value;

            string prefixTmp = parameterName + "(" + parameter.expectedType.Name + ") ";
            GUILayout.Label(prefixTmp);
            string prefix = string.Empty;

            if (parameter.expectedType == typeof(bool))
            {
                if (parameter.value == null) parameter.value = false;
                parameter.value = EditorGUILayout.Toggle(prefix, (bool)parameter.value);
            }
            else if (parameter.expectedType == typeof(int) || parameter.expectedType == typeof(short))
            {
                if (parameter.value == null) parameter.value = 0;
                parameter.value = EditorGUILayout.IntField(prefix, (int)parameter.value);
            }
#if UNITY_5_3_OR_NEWER
            else if (parameter.expectedType == typeof(long))
            {
                if (parameter.value == null) parameter.value = 0L;
                parameter.value = EditorGUILayout.LongField(prefix, (long)parameter.value);
            }
            else if (parameter.expectedType == typeof(double))
            {
                if (parameter.value == null) parameter.value = 0.0;
                parameter.value = EditorGUILayout.DoubleField(prefix, (double)parameter.value);
            }
#endif
            else if (parameter.expectedType == typeof(float))
            {
                if (parameter.value == null) parameter.value = 0.0f;
                parameter.value = EditorGUILayout.FloatField(prefix, (float)parameter.value);
            }
            else if (parameter.expectedType == typeof(string))
            {
                if (parameter.value == null) parameter.value = string.Empty;
                parameter.value = EditorGUILayout.TextField(prefix, (string)parameter.value);
            }
            else if (parameter.expectedType == typeof(Object))
            {
                parameter.value = EditorGUILayout.ObjectField(prefix, (Object)parameter.value, typeof(Object), true);
            }
            else if (parameter.expectedType == typeof(Vector2))
            {
                if (parameter.value == null) parameter.value = Vector2.zero;
                parameter.value = EditorGUILayout.Vector2Field(prefix, (Vector2)parameter.value);
            }
            else if (parameter.expectedType == typeof(Vector3))
            {
                if (parameter.value == null) parameter.value = Vector3.zero;
                parameter.value = EditorGUILayout.Vector3Field(prefix, (Vector3)parameter.value);
            }
            else if (parameter.expectedType == typeof(Vector4))
            {
                if (parameter.value == null) parameter.value = Vector4.zero;
                parameter.value = EditorGUILayout.Vector4Field(prefix, (Vector4)parameter.value);
            }
            else if (parameter.expectedType == typeof(Rect))
            {
                if (parameter.value == null) parameter.value = new Rect();
                parameter.value = EditorGUILayout.RectField(prefix, (Rect)parameter.value);
            }
            else if (parameter.expectedType == typeof(Color))
            {
                if (parameter.value == null) parameter.value = new Color();
                parameter.value = EditorGUILayout.ColorField(prefix, (Color)parameter.value);
            }
            else if (parameter.expectedType == typeof(Bounds))
            {
                if (parameter.value == null) parameter.value = new Bounds();
                parameter.value = EditorGUILayout.BoundsField(prefix, (Bounds)parameter.value);
            }
            else
            {
                //Log.Error("DrawPrameter error: unsupport param type=" + parameter.expectedType);
            }

            return prevValue != parameter.value;
        }

        static public bool DrawEvent(GameObject undoObject, List<EventDelegateS> listEvent, params System.Type[] typeReturn)
        {
            bool isValueChanged = false;
            GUILayoutHelper.DrawList(listEvent, "List", (value) => { isValueChanged = true; return true; });
            return isValueChanged;
        }
    }
}