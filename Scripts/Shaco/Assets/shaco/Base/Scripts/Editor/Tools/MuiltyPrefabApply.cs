using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class MuiltyPrefabApply
    {
        [MenuItem("shaco/Tools/ApplyPrefabs %#a", false, (int)ToolsGlobalDefine.MenuPriority.Tools.MUILTY_PREFABS_APPLY)]
        public static void ApplyPrefabs()
        {
            for (int i = Selection.gameObjects.Length - 1; i >= 0; --i)
			{
				ApplyPrefab(Selection.gameObjects[i]);
			}
			AssetDatabase.SaveAssets();
        }

        [MenuItem("shaco/Tools/ApplyPrefabs %#a", true, (int)ToolsGlobalDefine.MenuPriority.Tools.MUILTY_PREFABS_APPLY)]
        public static bool ApplyPrefabsValid()
        {
        	if (Selection.gameObjects.IsNullOrEmpty())
        		return false;

        	bool hasNotPrefabGameObject = false;

        	for (int i = Selection.gameObjects.Length - 1; i >= 0; --i)
        	{
        		if (!IsPrefabInstance(Selection.gameObjects[i]))
        		{
        			hasNotPrefabGameObject = true;
        			break;
        		}
        	}
        	return !hasNotPrefabGameObject;
        }

        //执行prefab的apply方法
        static private void ApplyPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            if (!IsPrefabInstance(prefab))
            {
                return;
            }

            //这里必须获取到prefab实例的根节点，否则ReplacePrefab保存不了
            GameObject prefabGo = GetPrefabInstanceParent(prefab);
            UnityEngine.Object prefabAsset = null;
            if (prefabGo != null)
            {
                prefabAsset = PrefabUtility.GetPrefabParent(prefabGo);
                if (prefabAsset != null)
                {
                    PrefabUtility.ReplacePrefab(prefabGo, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
                }
            }
        }

        static private bool IsPrefabInstance(GameObject prefab)
        {
            return PrefabUtility.GetPrefabType(prefab) == PrefabType.PrefabInstance;
        }

        //遍历获取prefab节点所在的根prefab节点
        static private GameObject GetPrefabInstanceParent(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            PrefabType pType = PrefabUtility.GetPrefabType(go);
            if (pType != PrefabType.PrefabInstance)
            {
                return null;
            }
            if (go.transform.parent == null)
            {
                return go;
            }
            pType = PrefabUtility.GetPrefabType(go.transform.parent.gameObject);
            if (pType != PrefabType.PrefabInstance)
            {
                return go;
            }
            return GetPrefabInstanceParent(go.transform.parent.gameObject);
        }
    }
}