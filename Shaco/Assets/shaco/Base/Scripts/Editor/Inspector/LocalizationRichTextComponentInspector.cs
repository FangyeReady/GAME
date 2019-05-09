using UnityEngine;
using System.Collections;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.LocalizationRichTextComponent))]
    public class LocalizationRichTextComponentInspector : Editor
    {
		private UnityEditorInternal.ReorderableList _formatParamsList = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var targetTmp = (shaco.LocalizationRichTextComponent)target;
            targetTmp.CheckRichTextComponent();

            GUILayout.BeginHorizontal();
            {
                GUI.changed = false;
                EditorGUILayout.PrefixLabel("Language Key");
                var languageKeyTmp = EditorGUILayout.TextArea(targetTmp.languageKey);
                if (GUI.changed)
                {
                    targetTmp.languageKey = languageKeyTmp;
                }
            }
            GUILayout.EndHorizontal();
            
            GUI.changed = false;
            _formatParamsList = GUILayoutHelper.DrawList(_formatParamsList, serializedObject, shaco.Base.Utility.ToVariableName(() => targetTmp.formatParams));
            if (GUI.changed)
            {
                //force update rich text
                targetTmp.languageKey = targetTmp.languageKey;
            }
        }
    }
}
