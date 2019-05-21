using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;
using UnityEditor.AnimatedValues;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ListView))]
    public class ListViewInspector : Editor
    {
        private readonly GUIContent[] HorizontalDirections = new GUIContent[] { new GUIContent("Automatic"), new GUIContent("Left->Right"), new GUIContent("Right->Left") };
        private readonly GUIContent[] VerticalDirections = new GUIContent[] { new GUIContent("Automatic"), new GUIContent("Top->Bottom"), new GUIContent("Bottom->Top") };
        private readonly GUIContent[] AllDirections = new GUIContent[] { new GUIContent("Left->Right"), new GUIContent("Right->Left"), new GUIContent("Top->Bottom"), new GUIContent("Bottom->Top") };

        private shaco.ListView _target = null;

        private AnimBool _isShowAdvancedSetting = new AnimBool(true);

        void OnEnable()
        {
            _target = target as shaco.ListView;
            _isShowAdvancedSetting.value = shaco.DataSave.Instance.ReadBool("ListViewInspector.ShowAdvancedSetting", false);
        }

        void OnDisable()
        {
            shaco.DataSave.Instance.Write("ListViewInspector.ShowAdvancedSetting", _isShowAdvancedSetting.value);
        }

        public override void OnInspectorGUI()
        {
            Repaint();
            base.OnInspectorGUI();
            DrawListViewInsepectorGUI();
        }

        private void DrawListViewInsepectorGUI()
        {
            GUIContent[] validGroupDirection = null;
            int indexOffset = 0;
            int directionCountPre = 2;

            switch (_target.GetScrollDirection())
            {
                case shaco.Direction.Right:
                case shaco.Direction.Left: validGroupDirection = VerticalDirections; indexOffset = directionCountPre; break;
                case shaco.Direction.Down:
                case shaco.Direction.Up: validGroupDirection = HorizontalDirections; indexOffset = 0; break;
                default: shaco.Log.Error("unsupport direction=" + _target.GetScrollDirection()); break;
            }

            //Main Scroll Direction
            GUI.changed = false;
            int scrollDirection = (int)_target.GetScrollDirection();
            scrollDirection = EditorGUILayout.Popup(new GUIContent("ScrollDirection", "主滚动方向"), scrollDirection, AllDirections);
            if (GUI.changed)
            {
                _target.SetGroupDirection(shaco.Direction.Automatic);
                _target.ChangeDirection((shaco.Direction)scrollDirection);
            }         

            //Group Item Scroll Direction
            if (_target.eachOfGroup > 1)
            {
                GUI.changed = false;
                int groupDirection = (int)_target.GetGroupDirection() % directionCountPre + 1;
                groupDirection = EditorGUILayout.Popup(new GUIContent("GroupDirection", "次要滚动方向"), groupDirection, validGroupDirection);
                if (GUI.changed)
                    _target.ChangeGroupItemDirection((shaco.Direction)(groupDirection + indexOffset - 1));
            }

            //center layout flag
            var centerLayoutTypeTmp = _target.GetCenterLayoutType();
            if (centerLayoutTypeTmp != shaco.ListView.CenterLayoutType.NoCenter)
            {
                _target.isCenterLayout = EditorGUILayout.Toggle(new GUIContent(centerLayoutTypeTmp.ToString(), "中心布局"), _target.isCenterLayout);
                if (GUI.changed)
                {
                    _target.UpdateCenterLayout();
                }
            }
            
            //first item offset
            _target.firstItemOffset = EditorGUILayout.Vector3Field(new GUIContent("First Item Offset", "第一个组建偏移量，会带动其他组建整体偏移"), _target.firstItemOffset);

            //item margin
            _target.itemMargin = EditorGUILayout.Vector3Field(new GUIContent("Item Margin", "主组建之间间隔大小"), _target.itemMargin);

            //group item margin
            if (_target.eachOfGroup > 1)
            {
                _target.groupItemMargin = EditorGUILayout.Vector3Field(new GUIContent("Group Item Margin", "次要组建之间间隔大小"), _target.groupItemMargin);
            }

            //each of group item count
            _target.eachOfGroup = EditorGUILayout.IntSlider(new GUIContent("Each Of Group", "每组组建数量"), _target.eachOfGroup, 1, 1000);

            DrawDebugTest();
            DrawAdvancedSetting();

            //check valud changed
            if (GUI.changed)
            {
                shacoEditor.EditorHelper.SetDirty(_target);
                _target.OnValidate();
            }
        }

        private void DrawAdvancedSetting()
        {
            _isShowAdvancedSetting.target = EditorHelper.Foldout(_isShowAdvancedSetting.target, new GUIContent("Advanced Settings", "高级设置"));
            if (EditorGUILayout.BeginFadeGroup(_isShowAdvancedSetting.faded))
            {
                //auto update content size
                _target.autoUpdateContentSize = EditorGUILayout.Toggle(new GUIContent("Auto Update Scroll Content Size", "自动刷新滚动区域大小"), _target.autoUpdateContentSize);

                //front arrow
                _target.frontArrow = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Front Arrow", "前置箭头，会在滚动到达最前端自动隐藏"), _target.frontArrow, typeof(GameObject), true);

                //behind arrow
                _target.behindArrow = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Behind Arrow", "后置箭头，会在滚动到达最后端自动隐藏"), _target.behindArrow, typeof(GameObject), true);

                //max drag out of bounds ratio
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Max Drag Out of Bounds");
                        _target.isMultipleDragOutOfBoundsSet = EditorGUILayout.Toggle(new GUIContent("Allowed Multiple", "允许同时前后拖拽比率"), _target.isMultipleDragOutOfBoundsSet, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndHorizontal();

                    if (_target.isMultipleDragOutOfBoundsSet)
                    {
                        _target.maxDragOutOfFrontBoundsRatio = EditorGUILayout.Slider(new GUIContent("Front Ratio", "允许拖拽超出前置组建的比率"), _target.maxDragOutOfFrontBoundsRatio, 0, 1.0f);
                        _target.maxDragOutOfBehindBoundsRatio = EditorGUILayout.Slider(new GUIContent("Behind Ratio", "允许拖拽超出后置组建的比率"), _target.maxDragOutOfBehindBoundsRatio, 0, 1.0f);
                    }
                    else
                    {
                        _target.maxDragOutOfFrontBoundsRatio = EditorGUILayout.Slider(new GUIContent("Ratio", "允许拖拽超出前后组建的比率"), _target.maxDragOutOfFrontBoundsRatio, 0, 1.0f);
                    }
                }
                GUILayout.EndVertical();

                //auto update params
                GUILayout.BeginVertical("box");
                {
                    _target.autoUpdateItemCountWhenSpringback = EditorGUILayout.IntSlider(new GUIContent("Auto Update Item Count When Springback", "当拖拽到底的时候回弹后自动刷新的组建数量"), _target.autoUpdateItemCountWhenSpringback, 0, 1000);
                    if (_target.autoUpdateItemCountWhenSpringback > 0)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            _target.autoUpdateItemMinIndex = EditorGUILayout.IntField(new GUIContent("MinIndex", "自动刷新组建的最小下标"), _target.autoUpdateItemMinIndex);
                            _target.autoUpdateItemMaxIndex = EditorGUILayout.IntField(new GUIContent("MaxIndex", "自动刷新组建的最大下标"), _target.autoUpdateItemMaxIndex);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawDebugTest()
        {
            //open debug mode
            if (_target.openDebugMode)
            {
                GUILayout.BeginVertical("box");
                {
                    _target.openDebugMode = EditorGUILayout.Toggle(new GUIContent("Debug Mode", "调试模式"), _target.openDebugMode);

                    if (_target.openDebugMode)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Add"))
                            {
                                _target.AddItemByModel();
                            }

                            if (GUILayout.Button("Remove"))
                            {
                                _target.RemoveItem(_target.Count - 1);
                            }

                            if (GUILayout.Button("Clear"))
                            {
                                _target.ClearItem();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }
            else 
            {
                _target.openDebugMode = EditorGUILayout.Toggle(new GUIContent("Debug Mode", "调试模式"), _target.openDebugMode);
            }
        }
    }
}