using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomPropertyDrawer(typeof(shaco.ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			EditorGUI.BeginDisabledGroup(true);
			{
                EditorGUI.PropertyField(position, property);
			}
			EditorGUI.EndDisabledGroup();
        }
    }
}