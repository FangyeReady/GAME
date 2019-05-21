using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public class CreateUIScriptEditor : Editor
    {
        private const string _modelScritPath = "Scripts/Editor/Tools/CreateUIScriptModel.cs";
        private const string WILL_ADD_SCRIPT_TARGET_PATH = "CreateUIScriptEditor.WillAddScriptTarget";

        [MenuItem("Assets/Create/UIScript", false, (int)ToolsGlobalDefine.HierachyMenuPriority.CREATE_UI)]
        static public void CreateUIScript()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return;

            for (int i = 0; i < Selection.gameObjects.Length; ++i)
            {
                var targetTmp = Selection.gameObjects[i];
                var newScriptFile = GetScriptFile(targetTmp);

                if (!string.IsNullOrEmpty(newScriptFile))
                {
                    var newScriptPath = GetScriptPath(targetTmp);
                    newScriptFile = ReplaceScriptClassName(newScriptFile, targetTmp.name);
                    shaco.Base.FileHelper.WriteAllByUserPath(newScriptPath, newScriptFile);
                }
            }

            SetWillAddScriptTargets(Selection.gameObjects);
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/UIScript", true, (int)ToolsGlobalDefine.HierachyMenuPriority.CREATE_UI)]
        static public bool CreateUIScriptValidate()
        {
            return Selection.activeGameObject != null && !EditorApplication.isCompiling;
        }

        static private string GetScriptFile(GameObject obj)
        {
            string retValue = string.Empty;
            if (null == obj) return retValue;

            var fullPath = shaco.Base.FileHelper.ContactPath(shaco.Base.GlobalParams.GetShacoFrameworkRootPath(), _modelScritPath);
            retValue = shaco.Base.FileHelper.ReadAllByUserPath(fullPath);

            return retValue;
        }

        static public void AddScriptToPrefab()
        {
            var willAddScriptTargets = GetWillAddScriptTargets();

            if (null == willAddScriptTargets || willAddScriptTargets.Count == 0) return;
            RemoveWillAddScriptTargets();

            for (int i = 0; i < willAddScriptTargets.Count; ++i)
            {
                var targetTmp = willAddScriptTargets[i];

                var newScriptPath = GetScriptPath(targetTmp);
                var applicationRootPath = Application.dataPath.Remove("Assets");
                newScriptPath = newScriptPath.Remove(applicationRootPath);
                var newScript = AssetDatabase.LoadAssetAtPath(newScriptPath, typeof(MonoScript)) as MonoScript;
                var classType = newScript.GetClass();

                if (null != newScript && targetTmp.GetComponent(classType) == null)
                {
                    targetTmp.AddComponent(newScript.GetClass());
                }
            }

            AssetDatabase.Refresh();
        }

        static private string GetScriptPath(GameObject target)
        {
            string retValue = string.Empty;

            if (null == target) return retValue;

            var prefabPath = AssetDatabase.GetAssetPath(target).Replace('\\', '/');
            var folder = shaco.Base.FileHelper.GetFolderNameByPath(prefabPath).Remove("Assets/");
            retValue = shaco.Base.FileHelper.ContactPath(Application.dataPath, folder + target.name + ".cs");

            return retValue;
        }

        static private void SetWillAddScriptTargets(GameObject[] targets)
        {
            if (targets != null && targets.Length > 0)
            {
                var prefabPaths = string.Empty;
                for (int i = 0; i < targets.Length; ++i)
                {
                    var prefabPath = AssetDatabase.GetAssetPath(targets[i]);
                    prefabPaths += prefabPath + "@";
                }

                prefabPaths = prefabPaths.Remove(prefabPaths.Length - 1, 1);
                shaco.DataSave.Instance.Write(WILL_ADD_SCRIPT_TARGET_PATH, prefabPaths);
            }
        }

        static private List<GameObject> GetWillAddScriptTargets()
        {
            var retValue = new List<GameObject>();
            var prefabPaths = shaco.DataSave.Instance.ReadString(WILL_ADD_SCRIPT_TARGET_PATH);
            if (!string.IsNullOrEmpty(prefabPaths))
            {
                var prefabSplitPaths = prefabPaths.Split("@");
                for (int i = 0; i < prefabSplitPaths.Length; ++i)
                {
                    var prefabPath = prefabSplitPaths[i];
                    if (!string.IsNullOrEmpty(prefabPath))
                    {
                        var newPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
                        if (newPrefab == null)
                        {
                            shaco.Log.Error("CreateUIScriptEditor GetWillAddScriptTargets error: can't load prefab path=" + prefabPath);
                            shaco.DataSave.Instance.Remove(WILL_ADD_SCRIPT_TARGET_PATH);
                        }
                        else
                            retValue.Add(newPrefab);
                    }
                }
            }
            return retValue;
        }

        static private void RemoveWillAddScriptTargets()
        {
            shaco.DataSave.Instance.Remove(WILL_ADD_SCRIPT_TARGET_PATH);
        }

        static private string ReplaceScriptClassName(string script, string newClassName)
        {
            int indexClass = script.IndexOf("class");
            if (indexClass < 0)
            {
                shaco.Log.Error("CreateUIScriptEditor ReplaceScriptClassName error: not find 'class' flag ");
                return script;
            }

            indexClass += "class".Length;

            //get old class name
            string oldClassName = string.Empty;
            for (int i = indexClass; i < script.Length; ++i)
            {
                var cTmp = script[i];
                if (cTmp != ' ' && cTmp != '\t')
                {
                    oldClassName += cTmp;
                }
                else
                {
                    if (!string.IsNullOrEmpty(oldClassName))
                    {
                        break;
                    }
                }
            }

            return script.Replace(oldClassName, newClassName);
        }
    }
}