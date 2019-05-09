using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.CurveMoveComponent))]
    public class CurveMoveComponentInsepector : Editor
    {
        private List<shaco.CurveMoveComponent.MoveInfo> listRemovePath = new List<shaco.CurveMoveComponent.MoveInfo>();
        private readonly string[] selectModes = new string[] { "NoControl", "OneControl", "TwoControl" };
        private shaco.CurveMoveComponent _target = null;

        void OnEnable()
        {
            _target = (shaco.CurveMoveComponent)target;
            if (_target.editorMode)
            {
                _target.CheckComponents();
            }
        }

        public override void OnInspectorGUI()
        {
            this.Repaint();
            base.OnInspectorGUI();

            GUI.changed = false;
            _target.editorMode = EditorGUILayout.Toggle("EditorMode", _target.editorMode);
            if (GUI.changed)
            {
                _target.CheckComponents();
            }
            _target.moveTarget = ObjectFieldWithoutDelete("MoveTarget", _target.moveTarget, typeof(Transform), true);

            GUILayout.BeginVertical("box");
            {
                bool isOpened = true;
                GUILayout.BeginHorizontal();
                {
                    isOpened = GUILayoutHelper.DrawHeader("Move Paths", "CurveMovePaths", () =>
                    {
                        GUILayout.FlexibleSpace();
                        GUILayoutHelper.DrawHeaderText("Count: " + _target.movePaths.Count);
                    });
                }
                GUILayout.EndHorizontal();

                if (isOpened)
                {
                    if (_target.IsPlayingMoveAction())
                    {
                        GUILayout.BeginHorizontal();
                        {
                            var buttonWidthTmp = GUILayout.Width(Screen.width / 2 - 15);
                            var oldFontSize = GUI.skin.button.fontSize;

                            GUI.skin.button.fontSize = 30;
                            if (!_target.IsPausedMoveAction() && GUILayout.Button("||", buttonWidthTmp, GUILayout.Height(40)))
                            {
                                _target.SafePauseMoveAction();
                            }
                            if (_target.IsPausedMoveAction() && GUILayout.Button("▶", buttonWidthTmp, GUILayout.Height(40)))
                            {
                                _target.SafeResumeMoveAction();
                            }
                            GUI.skin.button.fontSize = oldFontSize;

                            GUI.skin.button.fontSize = 50;
                            if (GUILayout.Button("■", buttonWidthTmp, GUILayout.Height(40)))
                            {
                                _target.SafeStopMoveAction();
                            }
                            GUI.skin.button.fontSize = oldFontSize;
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("New"))
                            {
                                _target.movePaths.Add(new shaco.CurveMoveComponent.MoveInfo());
                                _target.CheckComponents();
                                EditorHelper.SetDirty(_target);
                            }
                            if (GUILayout.Button("Clear"))
                            {
                                if (EditorUtility.DisplayDialog("Clear", "Clear all move path ?", "OK", "Cancel"))
                                {
                                    _target.movePaths.Clear();
                                    _target.CheckComponents();
                                    EditorHelper.SetDirty(_target);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (_target.movePaths.Count > 0)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("▶"))
                                {
                                    _target.PlayMoveAction();
                                }
                                _target.moveDuration = EditorGUILayout.FloatField("Duration", _target.moveDuration);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }

                    EditorGUI.BeginDisabledGroup(_target.IsPlayingMoveAction());
                    {
                        for (int i = 0; i < _target.movePaths.Count; ++i)
                        {
                            DrawPath(_target.movePaths[i]);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            GUILayout.EndVertical();

            if (listRemovePath.Count > 0)
            {
                for (int i = 0; i < listRemovePath.Count; ++i)
                {
                    _target.RemoveComponentWithMoveInfo(listRemovePath[i]);
                    _target.movePaths.Remove(listRemovePath[i]);
                }
                _target.CheckComponents();
                EditorHelper.SetDirty(_target);
                listRemovePath.Clear();
            }
        }

        private void DrawPath(shaco.CurveMoveComponent.MoveInfo moveInfo)
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("-"))
                    {
                        if (!listRemovePath.Contains(moveInfo))
                            listRemovePath.Add(moveInfo);
                    }

                    GUI.changed = false;
                    moveInfo.controlPointMode = (shaco.CurveMoveComponent.ControlPointMode)GUILayout.Toolbar((int)moveInfo.controlPointMode, selectModes, GUILayout.Width(Screen.width - 60));
                    if (GUI.changed)
                    {
                        ((shaco.CurveMoveComponent)target).CheckComponents();
                    }
                }
                GUILayout.EndHorizontal();

                moveInfo.beginPoint = ObjectFieldWithoutDelete("Begin", moveInfo.beginPoint, typeof(Transform), true);
                moveInfo.endPoint = ObjectFieldWithoutDelete("End", moveInfo.endPoint, typeof(Transform), true);

                switch (moveInfo.controlPointMode)
                {
                    case shaco.CurveMoveComponent.ControlPointMode.NoControlPoint: break;
                    case shaco.CurveMoveComponent.ControlPointMode.OneControlPoint:
                        {
                            moveInfo.controlPoints[0] = ObjectFieldWithoutDelete("ControlPoint(1)", moveInfo.controlPoints[0], typeof(Transform), true);
                            break;
                        }
                    case shaco.CurveMoveComponent.ControlPointMode.TwoControlPoint:
                        {
                            moveInfo.controlPoints[0] = ObjectFieldWithoutDelete("ControlPoint(1)", moveInfo.controlPoints[0], typeof(Transform), true);
                            moveInfo.controlPoints[1] = ObjectFieldWithoutDelete("ControlPoint(2)", moveInfo.controlPoints[1], typeof(Transform), true);
                            break;
                        }
                    default: shaco.Log.Error("DrawPath erorr: unsupport type=" + moveInfo.controlPointMode); break;
                }
            }
            GUILayout.EndVertical();
        }

        private T ObjectFieldWithoutDelete<T>(string title, T obj, System.Type type, bool allowSceneObjects, params GUILayoutOption[] option) where T : Object
        {
            GUI.changed = false;
            var retValue = (T)EditorGUILayout.ObjectField(title, obj, type, true, option);
            if (GUI.changed)
            {
                if (retValue == null)
                    retValue = obj;
            }
            return retValue;
        }

        private void EnsureControlPointCount(shaco.CurveMoveComponent.MoveInfo moveInfo, int requireCount)
        {
            int addCout = requireCount - moveInfo.controlPoints.Count;
            if (addCout > 0)
            {
                moveInfo.controlPoints.AddRange(new Transform[addCout]);
            }
        }

        private int GetValidControlPointCount(shaco.CurveMoveComponent.MoveInfo moveInfo)
        {
            int retValue = 0;
            if (moveInfo.controlPoints != null)
            {
                for (int i = 0; i < moveInfo.controlPoints.Count; ++i)
                {
                    if (moveInfo.controlPoints[i] != null)
                        ++retValue;
                }
            }
            return retValue;
        }
    }
}

