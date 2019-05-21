using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ActionDelegate))]
    public class ActionDelegateInspector : Editor
    {
        public override void OnInspectorGUI()
        {
			this.Repaint();
            base.OnInspectorGUI();

			var allActions = shaco.ActionS.GetAllActions();
			foreach (var iter in allActions)
			{
				GUILayout.BeginVertical("box");
				{
					if (DrawHeader(iter.Key))
                    {
						for (int i = 0; i < iter.Value.Count; ++i)
						{
							var action = iter.Value[i];

							if (!action.isRemoved)
							{
								GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(24);
                                    GUILayout.Label(action.ActionName);
                                }
                                GUILayout.EndHorizontal();
							}
                        }
                    }
				}
				GUILayout.EndVertical();
			}
        }
		
		private bool DrawHeader(GameObject target)
		{
			bool isOpened = shaco.DataSave.Instance.ReadBool(target.name, true);

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(isOpened ? "-" : "+", GUILayout.Width(20)))
				{
					isOpened = !isOpened;
					shaco.DataSave.Instance.Write(target.name, isOpened);
				}
				
				EditorGUI.BeginDisabledGroup(true);
				{
                    EditorGUILayout.ObjectField(target, typeof(GameObject), true);
                }
				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();

			return isOpened;
		}
    }
}