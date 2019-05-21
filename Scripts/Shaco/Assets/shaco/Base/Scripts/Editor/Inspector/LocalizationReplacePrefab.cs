using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class LocalizationReplacePrefab : shacoEditor.ILocalizationReplace
    {
        //组建类型 
        private string _componentTypeFullName = "UILabel";

        //属性名字
        private string _propertyPath = "mText";

        /// <summary>
        /// 获取所有语言文本
        /// <param name="importPath">导入路径</param>
        /// <param name="callbackCollectInfo">加载完毕后语言文本收集信息</param>
        /// <param name="collectExtensions">需要收集的文件后缀名</param>
        /// </summary>
        public void GetAllLanguageString(string importPath, System.Action<List<shaco.Base.Utility.LocalizationCollectnfo>> callbackCollectInfo, params string[] collectExtensions)
        {
            //获取要收集的组建类型
            System.Type componentType = null;
            try
            {
                componentType = shaco.Base.Utility.Instantiate(_componentTypeFullName).GetType();
            }
            catch
            {
                //ignore
            }
            if (null == componentType)
            {
                Debug.LogError("LocalizationReplacePrefab GetAllLanguageString error: can't create compnent type by name=" + _componentTypeFullName);
                return;
            }

            //查找目录中的prefab
            var allPrefabPath = new List<string>();
            shaco.Base.FileHelper.GetSeekPath(importPath, ref allPrefabPath, ".prefab");

            if (allPrefabPath.IsNullOrEmpty())
            {
                Debug.LogWarning("LocalizationReplacePrefab GetAllLanguageString warning: not found prefab in path=" + importPath);
                return;
            }

            if (null != callbackCollectInfo)
            {
                var retValue = new List<shaco.Base.Utility.LocalizationCollectnfo>();
                for (int i = 0; i < allPrefabPath.Count; ++i)
                {
                    var relativePathTmp = shacoEditor.EditorHelper.FullPathToUnityAssetPath(allPrefabPath[i]);
                    var prefabTmp = AssetDatabase.LoadAssetAtPath(relativePathTmp, typeof(GameObject)) as GameObject;
                    if (null != prefabTmp)
                    {
                        var labelComponentsTmp = prefabTmp.GetComponentsInChildren(componentType);
                        if (!labelComponentsTmp.IsNullOrEmpty())
                        {
                            for (int j = 0; j < labelComponentsTmp.Length; ++j)
                            {
                                //prefab组建路径
                                var internalPath = GetPrefabInternalPath(labelComponentsTmp[j].gameObject);

                                //添加到收集路径信息
                                object value = ChangeComponentDataHelper.GetSerializedPropertyValue(labelComponentsTmp[j], _propertyPath);
                                if (!value.IsNull())
                                {
                                    retValue.Add(new shaco.Base.Utility.LocalizationCollectnfo()
                                    {
                                        path = allPrefabPath[i],
                                        languageString = value.ToString(),
                                        parameter = internalPath
                                    });
                                }
                            }
                        }
                    }
                }

                callbackCollectInfo(retValue);
            }
        }

        /// <summary>
        /// 替换语言文本信息
        /// <param name="path">路径</param>
        /// <param name="exportInfo">导出的语言包信息</param>
        /// </summary>
        public void RepalceLanguageString(string path, List<shaco.Base.Utility.LocalizationExportInfo> exportInfos)
        {
            //获取要收集的组建类型
            var componentType = shaco.Base.Utility.Instantiate(_componentTypeFullName).GetType();
            if (null == componentType)
            {
                Debug.LogError("LocalizationReplacePrefab RepalceLanguageString error: can't create compnent type by name=" + _componentTypeFullName);
                return;
            }

            var filePath = GetPrefabFilePath(path);
            var relativeFilePath = shacoEditor.EditorHelper.FullPathToUnityAssetPath(filePath);
            var prefab = AssetDatabase.LoadAssetAtPath(relativeFilePath, typeof(GameObject)) as GameObject;
            var internalPath = exportInfos[0].parameter;

            var findChild = prefab.transform.Find(internalPath);
            if (null == findChild)
            {
                Debug.LogError("LocalizationReplacePrefab RepalceLanguageString error: not found child internal path=" + internalPath + " file path=" + filePath);
                return;
            }

            var componentTmp = findChild.GetComponent(componentType);
            if (null == componentTmp)
            {
                Debug.LogError("LocalizationReplacePrefab RepalceLanguageString error: not found component internal path=" + internalPath + " file path=" + filePath);
                return;
            }

            for (int i = 0; i < exportInfos.Count; ++i)
            {
                var infoTmp = exportInfos[i];
                var oldValue = ChangeComponentDataHelper.GetSerializedPropertyValue(componentTmp, _propertyPath);
                if (null != oldValue)
                {
                    string newValue = oldValue.ToString().Replace(infoTmp.textOriginal, infoTmp.textTranslation);
                    var autoValueTmp = new shaco.AutoValue();
                    autoValueTmp.Set(newValue);
                    ChangeComponentDataHelper.SetSerializedPropertyValue(componentTmp, _propertyPath, autoValueTmp);
                }
            }
        }

        /// <summary>
        /// 获取prefab内部层级路径，直到父节点为空停止
        /// <param name="target">对象</param>
        /// </summary>
        private string GetPrefabInternalPath(GameObject target)
        {
            var retValue = new System.Text.StringBuilder();
            var levelsPath = new List<string>();
            var parentTmp = target.transform;

            while (null != parentTmp)
            {
                levelsPath.Add(parentTmp.name);
                parentTmp = parentTmp.parent;
            }

            for (int i = levelsPath.Count - 2; i >= 0; --i)
            {
                retValue.Append(levelsPath[i]);
                retValue.Append(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            }

            if (retValue.Length > 0)
            {
                retValue = retValue.Remove(retValue.Length - shaco.Base.FileDefine.PATH_FLAG_SPLIT.Length, shaco.Base.FileDefine.PATH_FLAG_SPLIT.Length);
            }
            return retValue.ToString();
        }

        /// <summary>
        /// 绘制编辑器
        /// </summary>
        public void DrawInspector()
        {
            _componentTypeFullName = EditorGUILayout.TextField("Component Name", _componentTypeFullName);
            _propertyPath = EditorGUILayout.TextField("Property Name", _propertyPath);
        }

        /// <summary>
        /// 获取prefab文件路径
        /// <param name="prefabPath">全程路径</param>
        /// <return>文件路径</return>
        /// </summary>
        private string GetPrefabFilePath(string prefabPath)
        {
            var retValue = prefabPath;

            while (!shaco.Base.FileHelper.ExistsFile(retValue) && !string.IsNullOrEmpty(retValue))
            {
                retValue = shaco.Base.FileHelper.RemoveLastPathByLevel(retValue, 1);
            }
            return retValue;
        }

        /// <summary>
        /// 是否只在主线程下替换文件
        /// </summary>
        public bool OnlyReplaceInMainThread() { return true; }
    }
}