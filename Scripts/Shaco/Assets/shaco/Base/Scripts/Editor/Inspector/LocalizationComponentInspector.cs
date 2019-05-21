using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.LocalizationComponent))]
    public class LocalizationComponentInspector : Editor
    {
        private UnityEditorInternal.ReorderableList _formatParamsList = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            shaco.LocalizationComponent targetTmp = (shaco.LocalizationComponent)target;
            targetTmp.languageKey = EditorGUILayout.TextField("LanguageKey", targetTmp.languageKey);
            _formatParamsList = GUILayoutHelper.DrawList(_formatParamsList, serializedObject, shaco.Base.Utility.ToVariableName(() => targetTmp.formatParams));
        }
    }
}