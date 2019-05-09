using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.SequeueEventComponent))]
    public class SequeueEventComponentInspector : Editor
    {
        public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var targetTmp = target as shaco.SequeueEventComponent;

			if (GUILayout.Button("Location"))
			{
                EditorHelper.OpenAsset(targetTmp.statckLocation.statck, targetTmp.statckLocation.statckLine);
            }
		}
    }
}