#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUILayoutHelper
    {
        private static string[] TOP_ANCHORS { get { return new string[] { "┏", "━", "┓" }; } }
        private static string[] MIDDLE_ANCHORS { get { return new string[] { "┃", "╋", "┃" }; } }
        private static string[] BOTTOM_ANCHORS { get { return new string[] { "┗", "━", "┛" }; } }
        private static readonly string[] READ_ONLY_ANCHORS = new string[]
        {
            "X", "X", "X",
            "X", "X", "X",
            "X", "X", "X"
        };

        private static float anchorLayoutOffsetX = 0;

        private static readonly TextAnchor[] INDEX_TO_ANCHOR = new TextAnchor[]
        {
            TextAnchor.UpperLeft, TextAnchor.UpperCenter, TextAnchor.UpperRight,
            TextAnchor.MiddleLeft, TextAnchor.MiddleCenter, TextAnchor.MiddleRight,
            TextAnchor.LowerLeft, TextAnchor.LowerCenter, TextAnchor.LowerRight
        };

        static public TextAnchor DrawAnchor(string prefix, TextAnchor anchor, params TextAnchor[] readonlyAnchors)
        {
            var oldAnchor = anchor;
            TextAnchor retValue = anchor;
            var buttonWidth = GUILayout.Width(20 * TOP_ANCHORS.Length);
            var buttonHeight = GUILayout.Height(18);

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(prefix);
                var lastRect1 = GUILayoutUtility.GetLastRect();

                retValue = DrawAnchorGroup(retValue, 0, TOP_ANCHORS, readonlyAnchors, buttonWidth, buttonHeight);

                if (lastRect1.width > 1)
                    anchorLayoutOffsetX = lastRect1.width + 5;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(-5);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(anchorLayoutOffsetX);
                retValue = DrawAnchorGroup(retValue, 1, MIDDLE_ANCHORS, readonlyAnchors, buttonWidth, buttonHeight);

            }
            GUILayout.EndHorizontal();

            GUILayout.Space(-5);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(anchorLayoutOffsetX);
                retValue = DrawAnchorGroup(retValue, 2, BOTTOM_ANCHORS, readonlyAnchors, buttonWidth, buttonHeight);
            }
            GUILayout.EndHorizontal();

            if (retValue != oldAnchor)
            {
                GUI.changed = true;
            }

            return retValue;
        }

        static private string[] CheckConvertAnchorToReadOnly(int groupIndex, string[] groupAnchorName, TextAnchor[] readonlyAnchors)
        {
            if (readonlyAnchors.IsNullOrEmpty())
                return groupAnchorName;

            var startIndex = groupAnchorName.Length * groupIndex;
            for (int i = 0; i < groupAnchorName.Length; ++i)
            {
                var oldAnchor = INDEX_TO_ANCHOR[i + startIndex];
                var isReadonlyAnchor = false;

                for (int j = 0; j < readonlyAnchors.Length; ++j)
                {
                    if (readonlyAnchors[j] == oldAnchor)
                    {
                        isReadonlyAnchor = true;
                        break;
                    }
                }

                if (isReadonlyAnchor)
                {
                    groupAnchorName[i] = READ_ONLY_ANCHORS[startIndex + i];
                }
            }

            return groupAnchorName;
        }

        static private TextAnchor DrawAnchorGroup(TextAnchor anchor, int groupIndex, string[] groupAnchorName, TextAnchor[] readonlyAnchors, params GUILayoutOption[] options)
        {
            TextAnchor retValue = anchor;

            groupAnchorName = CheckConvertAnchorToReadOnly(groupIndex, groupAnchorName, readonlyAnchors);

            int anchorIndex = AnchorToIndex(anchor);
            int selectIndex = anchorIndex >= groupAnchorName.Length * groupIndex && anchorIndex < groupAnchorName.Length * (groupIndex + 1) ? anchorIndex : -1;

            selectIndex -= groupAnchorName.Length * groupIndex;

            GUI.changed = false;
            selectIndex = GUILayout.Toolbar(selectIndex, groupAnchorName, options);
            if (selectIndex != -1 && GUI.changed)
            {
                var startIndex = groupAnchorName.Length * groupIndex;
                if (groupAnchorName[selectIndex] != READ_ONLY_ANCHORS[startIndex + selectIndex])
                {
                    retValue = INDEX_TO_ANCHOR[selectIndex + groupAnchorName.Length * groupIndex];
                }
            }

            return retValue;
        }

        static private int AnchorToIndex(TextAnchor anchor)
        {
            int retValue = -1;
            switch (anchor)
            {
                case TextAnchor.UpperLeft: retValue = 0; break;
                case TextAnchor.UpperCenter: retValue = 1; break;
                case TextAnchor.UpperRight: retValue = 2; break;
                case TextAnchor.MiddleLeft: retValue = 3; break;
                case TextAnchor.MiddleCenter: retValue = 4; break;
                case TextAnchor.MiddleRight: retValue = 5; break;
                case TextAnchor.LowerLeft: retValue = 6; break;
                case TextAnchor.LowerCenter: retValue = 7; break;
                case TextAnchor.LowerRight: retValue = 8; break;
                default: retValue = -1; break;
            }
            return retValue;
        }
    }
}

#endif