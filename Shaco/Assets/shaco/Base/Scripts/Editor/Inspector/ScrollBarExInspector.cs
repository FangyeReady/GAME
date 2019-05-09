using UnityEngine;
using System.Collections;
using UnityEditor;
using shaco.Base;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ScrollBarEx))]
    public class ScrollBarExInspector : Editor
    {
		public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
		}
    }
}
