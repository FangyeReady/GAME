using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [InitializeOnLoad]
    public class UnityScripsCompiling : AssetPostprocessor
    {
        public UnityScripsCompiling()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length != 1)
            {
                EditorApplication.update -= Update;
            }
        }

        private static void Update()
        {
            if (!EditorApplication.isCompiling)
            {
                EditorApplication.update -= Update;
                CreateUIScriptEditor.AddScriptToPrefab();

#if HOTFIX_ENABLE
                if (shaco.DataSave.Instance.ReadBool("BuildInspector.BuildXluaSupportEndCallBack"))
                {
                    shaco.DataSave.Instance.Remove("BuildInspector.BuildXluaSupportEndCallBack");
                    BuildInspector.BuildXluaSupportEndCallBack();
                }
#endif
            }

            if (!EditorApplication.isCompiling)
            {
#if HOTFIX_ENABLE
                if (shaco.DataSave.Instance.ReadBool("Generator.XluaDelayHotfixInject"))
                {
                    shaco.DataSave.Instance.Remove("Generator.XluaDelayHotfixInject");
                    XLua.Hotfix.HotfixInject();
                }
#endif
            }

        }
    }
}