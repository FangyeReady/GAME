using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.NumberLoopScrollAction))]
    public class NumberLoopScrollActionInspector : Editor
    {
        private shaco.NumberLoopScrollAction _target = null;
        private GridLayoutGroup _gridLayout = null;
        private Vector2 _prevCellSize;
        private Vector2 _prevSpacing;

        void OnEnable()
        {
            _target = target as shaco.NumberLoopScrollAction;
            _gridLayout = _target.gameObject.GetComponent<GridLayoutGroup>();
            _prevCellSize = _gridLayout.cellSize;
            _prevSpacing = _gridLayout.spacing;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Text");
                _target.text = EditorGUILayout.TextArea(_target.text);
            }
            GUILayout.EndHorizontal();

            if (CheckGridLayoutUpdate())
            {
                _target.UpdateTextLayout();
            }
        }

        private bool CheckGridLayoutUpdate()
        {
            if (null == _gridLayout)
                return false;

            if (_prevCellSize != _gridLayout.cellSize)
            {
                _prevCellSize = _gridLayout.cellSize;
                return true;
            }
            else if (_prevSpacing != _gridLayout.spacing)
            {
                _prevSpacing = _gridLayout.spacing;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
