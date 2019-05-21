using UnityEngine;
using System.Collections;
using UnityEditor;

namespace shacoEditor
{
	[CustomEditor(typeof(shaco.LogicLinker))]
	public class LogickLinkerButtonEditor : Editor 
	{
		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();
			var _target = (shaco.LogicLinker)target;

			if (GUILayout.Button("LinkScript"))
			{
				_target.ButtonClickLinkScript.Execute();
			}
			if (GUILayout.Button("ExcuteAllActiveLinkScriptInScene"))
			{
				var listScripts = GameObject.FindObjectsOfType<shaco.LogicLinker>();
				for (int i = 0; i < listScripts.Length; ++i)
				{
					listScripts[i].ButtonClickLinkScript.Execute();
				}
			}
		}
	}
}

