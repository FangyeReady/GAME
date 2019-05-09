using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ChangeComponentDataInspector : EditorWindow
    {
        private class TargetObjectInfo
        {
            public Object target;
            public List<Component> components = null;
        }

        private ChangeComponentDataInspector _currentWindow = null;
        private Object _sourceTarget = null;
        private Dictionary<Object, TargetObjectInfo> _targetObjects = new Dictionary<Object, TargetObjectInfo>();
        private List<Object> _inputTargetObjects = null;
        private List<Object> _willRemoveTargetObjects = new List<Object>();
        private string _inputPropertyPath = "m_Sprite";
        private string _inputComponentType = typeof(UnityEngine.UI.Image).ToTypeString();
        private ChangeComponentDataHelper.ValueType _selectValueType = ChangeComponentDataHelper.ValueType.String;
        private shaco.AutoValue _autoValue = new shaco.AutoValue();
        private Vector2 _srollPosition = Vector2.zero;
        private string _searchName = string.Empty;
        private bool _isGlobalSearch = false;
        private bool _isChanging = false;

        private readonly int MAX_SHOW_COUNT = 50;


        [MenuItem("shaco/Tools/ChangeComponentData", false, (int)ToolsGlobalDefine.MenuPriority.Tools.CHANGE_COMPONENT_DATA)]
        static public void OpenChangeComponentDataWindowInProjectMenu()
        {
            OpenChangeComponentDataWindowInProjectMenu(null, null)._isGlobalSearch = true;
        }

        static public ChangeComponentDataInspector OpenChangeComponentDataWindowInProjectMenu(Object sourceTarget, List<Object> objs)
        {
            var retValue = EditorHelper.GetWindow<ChangeComponentDataInspector>(null, true, "ChangeReference");
            if (!retValue.Init(sourceTarget, objs))
            {
                retValue.Close();
                Debug.LogWarning("ChangeComponentDataInspector OpenChangeComponentDataWindowInProjectMenu warning: no data");
            }
            return retValue;
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<ChangeComponentDataInspector>(this, true, "ChangeReference");
        }

        void OnGUI()
        {
            if (_currentWindow == null || _isChanging)
                return;

            base.Repaint();

            GUILayout.BeginVertical("box");
            {
                DrawSourceTarget();
                _inputComponentType = EditorGUILayout.TextField("ComponentTypeName", _inputComponentType);
                _inputPropertyPath = EditorGUILayout.TextField("PropertyPath", _inputPropertyPath);

                ChangeComponentDataHelper.DrawValueInput(_autoValue, _selectValueType);

            }
            GUILayout.EndVertical();


            GUILayout.BeginHorizontal();
            {
                if (!string.IsNullOrEmpty(_inputComponentType) && !string.IsNullOrEmpty(_inputPropertyPath) && GUILayout.Button("Update"))
                {
                    UpdateTargetComponent();
                }
                DrawChangeButton();
            }
            GUILayout.EndHorizontal();

            DrawSearchName();
            EditorGUILayout.LabelField("Total Count", _targetObjects.Count.ToString());

            _srollPosition = GUILayout.BeginScrollView(_srollPosition);
            {
                int index = 0;
                foreach (var iter in _targetObjects)
                {
                    try 
                    {
                        DrawTarget(iter.Value);
                    }
                    catch (System.Exception)
                    {
                        _willRemoveTargetObjects.Add(iter.Key);
                    }

                    if (++index > MAX_SHOW_COUNT)
                    {
                        break;
                    }
                }

                if (_willRemoveTargetObjects.Count > 0)
                {
                    foreach (var iter in _willRemoveTargetObjects)
                    {
                        _targetObjects.Remove(iter);
                    }
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawSourceTarget()
        {
            _isGlobalSearch = EditorGUILayout.Toggle("GlobalSearch", _isGlobalSearch);
            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUILayout.ObjectField("SearchTarget", _sourceTarget, typeof(Object), true);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawChangeButton()
        {
            if (!string.IsNullOrEmpty(_inputPropertyPath) && _selectValueType >= 0 && GUILayout.Button("Change"))
            {
                try
                {
                    _isChanging = true;
                    bool shouldCancel = false;
                    shouldCancel = EditorUtility.DisplayCancelableProgressBar("change datas", "please wait...", 0);
                    shaco.Base.Coroutine.Foreach(_targetObjects, (object data) =>
                    {
                        var iter = (KeyValuePair<Object, TargetObjectInfo>)data;

                        //如果是场景需要特殊处理
                        if (IsSceneAsset(iter.Value.target))
                        {
                            var dependenciesTmp = CollectionSceneDependencies(_sourceTarget, new List<string>() { AssetDatabase.GetAssetPath(iter.Value.target) }, _isGlobalSearch);
                            foreach (var iter2 in dependenciesTmp)
                            {
                                var newItem = new TargetObjectInfo();
                                newItem.target = iter2;
                                newItem.components = GetComponentsWithDependencies(newItem.target);
                                SetComponentValue(newItem, _inputComponentType, _inputPropertyPath, _autoValue);
                            }
                        }
                        else
                        {
                            SetComponentValue(iter.Value, _inputComponentType, _inputPropertyPath, _autoValue);
                        }
                        return !shouldCancel;
                    }, (float percent) =>
                    {
                        shouldCancel = EditorUtility.DisplayCancelableProgressBar("change datas", "please wait...", percent);

                        if (percent >= 1)
                        {
                            _isChanging = false;
                            EditorUtility.ClearProgressBar();
                        }
                    }, _targetObjects.Count > MAX_SHOW_COUNT ? 0.02f : 0.1f);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("ChangeComponentDataInspector Change error: e=" + e);
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private List<Component> GetComponentsWithDependencies(Object obj)
        {
            var retValue = new List<Component>();
            var gameObjTmp = obj as GameObject;
            if (null == gameObjTmp)
            {
                return retValue;
            }
            var comopnents = gameObjTmp.GetComponentsInChildren(typeof(Component));
            if (null == comopnents)
            {
                return retValue;
            }

            var pathTmp = AssetDatabase.GetAssetPath(_sourceTarget);
            for (int i = 0; i < comopnents.Length; ++i)
            {
                if (null == comopnents[i])
                    continue;

                var typeString = comopnents[i].GetType().ToString();
                if (typeString != _inputComponentType)
                {
                    continue;
                }

                var serializedObject = new SerializedObject(comopnents[i]);
                var propertyTmp = serializedObject.FindProperty(_inputPropertyPath);
                if (null == propertyTmp)
                {
                    continue;
                }

                if (_isGlobalSearch || (propertyTmp.propertyType != SerializedPropertyType.ObjectReference || AssetDatabase.GetAssetPath(propertyTmp.objectReferenceValue) == pathTmp))
                {
                    retValue.Add(comopnents[i]);
                }
            }
            return retValue;
        }

        private void DrawSearchName()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Search Type Name");
                _searchName = GUILayoutHelper.SearchField(_searchName, GUILayout.Width(Screen.width / 3 * 1));
            }
            GUILayout.EndHorizontal();
        }

        private bool IsSceneAsset(Object asset)
        {
#if UNITY_5_3_OR_NEWER
            return null != asset as SceneAsset;
#else
            var pathTmp = AssetDatabase.GetAssetPath(asset);
            return shaco.Base.FileHelper.GetFilNameExtension(pathTmp) == "unity";
#endif
        }

        private void DrawTarget(TargetObjectInfo targetInfo)
        {
            if (GUILayoutHelper.DrawHeader(targetInfo.target.name, targetInfo.target.name, () => { EditorGUILayout.ObjectField(targetInfo.target, typeof(Object), true); }))
            {
                for (int i = 0; i < targetInfo.components.Count; ++i)
                {
                    if (null == targetInfo.components[i])
                        continue;

                    var typeString = targetInfo.components[i].GetType().ToString();
                    if (typeString != _inputComponentType)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(_searchName))
                    {
                        if (!typeString.ToLower().Contains(_searchName.ToLower()))
                        {
                            continue;
                        }
                    }

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.ObjectField(targetInfo.components[i], typeof(Object), true);
                        if (GUILayout.Button("Print"))
                        {
                            EditorHelper.PrintSerializedObject(targetInfo.components[i]);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private bool Init(Object sourceTarget, List<Object> objs)
        {
            if ((sourceTarget == null || objs.IsNullOrEmpty()))
                return true;

            _sourceTarget = sourceTarget;
            _inputTargetObjects = objs;
            _inputComponentType = _sourceTarget.GetType().ToTypeString();
            return _inputTargetObjects.Count > 0;
        }

        //根据输入的属性名字，查找包含该属性的组件 
        private void UpdateTargetComponent()
        {
            _targetObjects.Clear();

            if (_isGlobalSearch)
            {
                AddTargetObjectToCache(Resources.FindObjectsOfTypeAll(typeof(Object)).ToArrayList());
                var scenes = EditorHelper.GetEnabledEditorScenes();
                var scenesList = new List<Object>();
                for (int i = 0; i < scenes.Length; ++i)
                {
                    scenesList.Add(AssetDatabase.LoadAssetAtPath(scenes[i], typeof(Object)));
                }
                AddTargetObjectToCache(scenesList);
            }
            else 
            {
                AddTargetObjectToCache(_inputTargetObjects);
            }
        }

        private void AddTargetObjectToCache(List<Object> targetObjects)
        {
            foreach (var iter in targetObjects)
            {
                if (_targetObjects.ContainsKey(iter))
                    continue;

                var newItem = new TargetObjectInfo();
                newItem.target = iter;
                newItem.components = GetComponentsWithDependencies(newItem.target);
                if (newItem.components.Count > 0 || IsSceneAsset(newItem.target))
                {
                    _targetObjects.Add(newItem.target, newItem);
                }
            }
        }

        private void DrawValueInput(shaco.AutoValue autoValue, string valueTypeString)
        {
            switch (valueTypeString)
            {
                case "bool": autoValue.Set(EditorGUILayout.Toggle(autoValue)); break;
                case "int": autoValue.Set(EditorGUILayout.IntField(autoValue)); break;
                case "float": autoValue.Set(EditorGUILayout.FloatField(autoValue)); break;
                case "string": autoValue.Set(EditorGUILayout.TextArea(autoValue)); break;
                case "Object": autoValue.Set(EditorGUILayout.ObjectField(autoValue, typeof(Object), true)); break;
                case "Vector2": autoValue.Set(EditorGUILayout.Vector2Field(string.Empty, autoValue)); break;
                case "Vector3": autoValue.Set(EditorGUILayout.Vector3Field(string.Empty, autoValue)); break;
                case "Vector4": autoValue.Set(EditorGUILayout.Vector4Field(string.Empty, autoValue)); break;
                case "Rect": autoValue.Set(EditorGUILayout.RectField(autoValue)); break;
                case "Color": autoValue.Set(EditorGUILayout.ColorField(autoValue)); break;
                case "Bounds": autoValue.Set(EditorGUILayout.BoundsField(autoValue)); break;
                default: Debug.LogError("ChangeComponentDataInspector SetComponentValue error: unsupport type=" + valueTypeString); break;
            }
        }

        //查找数组中含有场景的对象，并筛选出正确的对象
        private void CheckTargets(List<Object> objs, out List<Object> normalTargets, out List<string> sceneTargetsPath)
        {
            normalTargets = new List<Object>();
            sceneTargetsPath = new List<string>();
            for (int i = objs.Count - 1; i >= 0; --i)
            {
                var objTmp = objs[i];
                var scenePath = AssetDatabase.GetAssetPath(objTmp);
                var extensionTmp = shaco.Base.FileHelper.GetFilNameExtension(scenePath);
                if ("unity" == extensionTmp)
                {
                    sceneTargetsPath.Add(scenePath);
                }
                else
                {
                    normalTargets.Add(objs[i]);
                }
            }
        }

        static private void SetComponentValue(TargetObjectInfo targetInfo, string componentName, string propertyPath, shaco.AutoValue autoValue)
        {
            if (null == targetInfo.target)
                return;
                
            var components = targetInfo.components;

            if (components.Count == 0)
                return;

            Object findComponent = null;
            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (null != components[i] && components[i].GetType().ToTypeString() == componentName)
                {
                    findComponent = components[i];
                    break;
                }
            }

            if (null == findComponent)
            {
                Debug.LogError("SetComponentReference error: not found component, type=" + componentName + " obj=" + targetInfo.target);
                return;
            }

            ChangeComponentDataHelper.SetSerializedPropertyValue(findComponent, propertyPath, autoValue);
        }

        //收集场景中包含的引用
        static private List<Object> CollectionSceneDependencies(Object sourceTarget, List<string> scenePaths, bool isGlobalSearch)
        {
            List<Object> retValue = new List<Object>();
            for (int i = 0; i < scenePaths.Count; ++i)
            {
                EditorHelper.SaveCurrentScene();
                EditorHelper.OpenScene(scenePaths[i]);
#if UNITY_5_3_OR_NEWER
                var rootsTmp = Resources.FindObjectsOfTypeAll<GameObject>();
#else
                var rootsTmp = GameObject.FindObjectsOfTypeAll(typeof(GameObject)).ToArrayConvert<Object, GameObject>();
#endif

                for (int j = 0; j < rootsTmp.Length; ++j)
                {
                    shaco.UnityHelper.ForeachChildren(rootsTmp[j], (int index, GameObject child) =>
                    {
                        var listDependence = EditorUtility.CollectDependencies(new Object[] { child });
                        if (isGlobalSearch)
                        {
                            retValue.Add(child);
                        }
                        else 
                        {
                            for (int k = 0; k < listDependence.Length; ++k)
                            {
                                if (sourceTarget == listDependence[k])
                                {
                                    retValue.Add(child);
                                    break;
                                }
                            }
                        }
                        return true;
                    });
                }
            }
            return retValue;
        }
    }
}
