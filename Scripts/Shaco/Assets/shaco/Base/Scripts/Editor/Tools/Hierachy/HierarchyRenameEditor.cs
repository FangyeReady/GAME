using UnityEditor;
using UnityEngine;
using System.Collections;

namespace shacoEditor
{
	public class HierarchyRenameEditor : EditorWindow
    {
        [MenuItem("GameObject/RenameBySequeue", false, ToolsGlobalDefine.HierachyMenuPriority.RENAME_BY_SEQUEUE)]
		static void RenameBySequeue()
		{
			if (null == Selection.activeGameObject)
			{
				return;
			}
			var parentTmp = Selection.activeGameObject.transform;
            if (0 == parentTmp.childCount)
			{
				return;
			}
			string firstNameTmp = parentTmp.GetChild(0).name;
			if (firstNameTmp.Length == 0)
			{
				Debug.LogError("select active GameObject name is empty !");
				return;
			}
			var baseNameTmp = GetBaseNameWithoutNumber(firstNameTmp);
			for (int i = 0; i < parentTmp.childCount; ++i)
			{
				var childTmp = parentTmp.GetChild(i);
				childTmp.name = baseNameTmp + (i + 1);
			}
		}

		static private string GetBaseNameWithoutNumber(string name)
		{
			string retValue = name;
			for (int i = retValue.Length - 1; i >= 0; --i)
			{
				if (char.IsNumber(retValue[i]))
				{
					retValue = retValue.Remove(i);
				}
				else 
				{
					break;
				}
			}
			return retValue;
		}
    }
}

