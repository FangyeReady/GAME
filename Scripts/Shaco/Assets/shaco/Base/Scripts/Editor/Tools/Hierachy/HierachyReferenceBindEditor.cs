using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public class HierachyReferenceBindEditor : EditorWindow
    {
        private HierachyReferenceBindEditor _currentWindow = null;
        private int _defaultArraryCount = 1;
        private float _defaultNumberOffset = 0;
        private MonoBehaviour _assetScript = null;
        private GameObject _prevSelectActiveGameObject = null;

        [MenuItem("GameObject/ReferenceBind", false, (int)ToolsGlobalDefine.HierachyMenuPriority.REFERENCE_BIND)]
        static public void OpenWindow()
        {
            if (Selection.activeGameObject != null)
            {
                var retValue = EditorHelper.GetWindow<HierachyReferenceBindEditor>(null, true, "HierachyReferenceBind");
                retValue.Init();
            }
            
        }

        void OnEnable()
        {
            if (Selection.activeGameObject != null)
            {
                _currentWindow = EditorHelper.GetWindow<HierachyReferenceBindEditor>(this, true, "HierachyReferenceBind");
                _currentWindow.Init();
            }
        }

        void OnGUI()
        {
            this.Repaint();

            CheckSelectActiveGameObjectChanged();

            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUILayout.ObjectField("Script", _assetScript, typeof(MonoBehaviour), true);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.ObjectField("Select Parent", Selection.activeGameObject, typeof(GameObject), true);

            if (null != _assetScript)
            {
                _defaultArraryCount = EditorGUILayout.IntField("ArrayCount: ", _defaultArraryCount);
                _defaultNumberOffset = EditorGUILayout.FloatField("NumberOffset: ", _defaultNumberOffset);

                SerializedObject serializedObject = new SerializedObject(_assetScript);
                var iter = serializedObject.GetIterator();
                bool isChanged = false;

                while (iter.NextVisible(true))
                {
                    if (iter.isArray)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("RefreshListData", GUILayout.Width(150)))
                            {
                                RefreshListData(iter);
                                isChanged = true;
                            }
                            GUILayout.TextField(iter.name);
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                if (isChanged)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void RefreshListData(SerializedProperty list)
        {
            if (list == null) return;

            list.ClearArray();

            for (int i = _defaultArraryCount - 1; i >= 0; --i)
            {
                list.InsertArrayElementAtIndex(0);
                var propertyTmp = list.GetArrayElementAtIndex(0);

                switch (propertyTmp.propertyType)
                {
                    case SerializedPropertyType.Integer: ChangePropertyInt(propertyTmp, i); break;
                    case SerializedPropertyType.Float: ChangePropertyFloat(propertyTmp, i); break;
                    case SerializedPropertyType.ObjectReference: ChangePropertyObjectReference(propertyTmp, i); break;
                    default: shaco.Log.Info("unsupport type=" + propertyTmp.propertyType); break;
                }
            }
        }

        private void ChangePropertyInt(SerializedProperty property, int index)
        {
            property.intValue = index + (int)_defaultNumberOffset;
        }

        private void ChangePropertyFloat(SerializedProperty property, int index)
        {
            property.floatValue = index + _defaultNumberOffset;
        }

        private void ChangePropertyObjectReference(SerializedProperty property, int index)
        {
            index += (int)_defaultNumberOffset;

            if (null == Selection.activeGameObject)
            {
                shaco.Log.Error("Please select a GameObject in 'Hierachy' Window as parent");
                return;
            }

            if (index < 0 || index > Selection.activeGameObject.transform.childCount - 1) return;

            var childTmp = Selection.activeGameObject.transform.GetChild(index);

            if (property.type.Contains(typeof(GameObject).Name))
                property.objectReferenceValue = childTmp.gameObject;
            else if (property.type.Contains(typeof(Transform).Name))
                property.objectReferenceValue = childTmp.transform;
            else 
            {
                var components = childTmp.GetComponents<UnityEngine.Component>();
                for (int i = 0; i < components.Length; ++i)
                {
                    if (components[i] == null) continue;
                    
                    if (property.type.Contains(components[i].GetType().Name))
                    {
                        property.objectReferenceValue = components[i];
                    }
                }
            }
        }

        private void Init()
        {
            _assetScript = Selection.activeGameObject.GetComponent<MonoBehaviour>();
        }

        private void CheckSelectActiveGameObjectChanged()
        {
            if (_prevSelectActiveGameObject != Selection.activeGameObject)
            {
                _prevSelectActiveGameObject = Selection.activeGameObject;
                if (_prevSelectActiveGameObject != null)
                    _defaultArraryCount = _prevSelectActiveGameObject.transform.childCount;
            }
        }
    }
}