using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class BehaviourTreeViewer : EditorWindow
    {
        private readonly Vector2 DRAG_OFFSET_AREA = new Vector2(0.3f, 0.3f);

        private List<shaco.Base.BehaviourTree> _currentSelectTrees = new List<shaco.Base.BehaviourTree>();
        private List<shaco.Base.BehaviourTree> _currentDragTrees = new List<shaco.Base.BehaviourTree>();
        private shaco.Base.BehaviourTree _currentDragTargetParentTree = null;
        private shaco.Direction _currentDragTargetDirection = shaco.Direction.None;
        private TextAsset _currentSelectScript = null;

        public void UpdateEvent()
        {
            var curEvent = Event.current;

            switch (curEvent.type)
            {
                case EventType.MouseUp:
                    {
                        if (curEvent.button == 0)
                        {
                            SelectBehaviourTree(curEvent, !EditorHelper.IsPressedControlButton());
                        }
                        else if (curEvent.button == 1)
                        {
                            if (_currentSelectTrees.Count > 1) 
                                ShowContextMenu(false);
                            else if (SelectBehaviourTree(curEvent))
                                ShowContextMenu(false);
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (KeyCode.Delete == curEvent.keyCode)
                        {
                            ExcueteRemove();
                        }
                        break;
                    }
                case EventType.MouseDrag:
                {
                    OnMouseDragEvent(curEvent);
                    break;
                }
                case EventType.DragUpdated:
                {
                    CheckDragTargetParentHighLight(curEvent);
                    break;
                }
                case EventType.DragExited:
                {
                    OnDragEnd(curEvent);
                    break;
                }
                default: break;
            }
        }

        private bool SelectBehaviourTree(Event curEvent, bool shouldRefreshSelect = true)
        {
            var realMousePosition = curEvent.mousePosition - GetDrawOffset();

            if (shouldRefreshSelect)
                _currentSelectTrees.Clear();

            if (_rootTree.editorDrawPosition.Contains(realMousePosition))
            {
                SelectOrUnSelectTree(_rootTree);
            }
            else
            {
                _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
                {
                    if (tree.editorDrawPosition.Contains(realMousePosition))
                    {
                        SelectOrUnSelectTree(tree);
                    }
                    return true;
                });
            }
            
            bool retValue = _currentSelectTrees.Count > 0;
            if (!retValue)
            {
                GUI.FocusControl(string.Empty);
                _searchTreeName = string.Empty;
            }

            return retValue;
        }

        private void SelectOrUnSelectTree(shaco.Base.BehaviourTree tree)
        {
            if (!_currentSelectTrees.Contains(tree))
            {
                _currentSelectTrees.Add(tree);
            }
            else
                _currentSelectTrees.Remove(tree);
        }

        private shaco.Base.BehaviourTree SelectDragTreeInSelectedTrees(Event curEvent)
        {
            shaco.Base.BehaviourTree retValue = null;
            if (_currentSelectTrees.Count == 0) return retValue;

            var realMousePosition = curEvent.mousePosition - GetDrawOffset();

            for (int i = _currentSelectTrees.Count - 1; i >= 0; --i)
            {
                if (_currentSelectTrees[i].editorDrawPosition.Contains(realMousePosition))
                {
                    retValue = _currentSelectTrees[i];
                    break;
                }
            }

            return retValue;
        }

        private void ShowContextMenu(bool newOnly)
        {
            GenericMenu menuTmp = new GenericMenu();

            var allBehaviourTreeTypes = shaco.Base.Utility.GetAttributes<shaco.Base.BehaviourProcessTreeAttribute>(typeof(shaco.Base.BehaviourRootTree));

            for (int i = 0; i < allBehaviourTreeTypes.Length; ++i)
            {
                var typeTmp = allBehaviourTreeTypes[i];
                var objTmp = typeTmp.Instantiate() as shaco.Base.BehaviourTree;
                menuTmp.AddItem(new GUIContent("Add(" + objTmp.GetDisplayName() + ")"), false, 
                                (object context) => 
                                {
                                    for (int j = 0; j < _currentSelectTrees.Count; ++j)
                                    {
                                        _currentSelectTrees[j].AddChild(objTmp);
                                    }
                                    _updateFormatPositionsDirty = true;
                                }, null);
            }

            if (!newOnly)
            {
                menuTmp.AddItem(new GUIContent("Remove(Delete)"), false, (object context) => { ExcueteRemove(); }, null);
                menuTmp.AddItem(new GUIContent("RemoveChildren"), false, (object context) => { _currentSelectTrees[0].RemoveChildren(); _updateFormatPositionsDirty = true; }, null);
            }

            menuTmp.ShowAsContext();
        }

        private void ShowChangeTreeTypeMenu(shaco.Base.BehaviourTree tree)
        {
            GenericMenu menuTmp = new GenericMenu();
            var allBehaviourTreeTypes = shaco.Base.Utility.GetAttributes<shaco.Base.BehaviourProcessTreeAttribute>(typeof(shaco.Base.BehaviourRootTree));

            for (int i = 0; i < allBehaviourTreeTypes.Length; ++i)
            {
                var typeTmp = allBehaviourTreeTypes[i];
                var objTmp = typeTmp.Instantiate() as shaco.Base.BehaviourTree;

                if (tree.GetType() == objTmp.GetType())
                {
                    menuTmp.AddItem(new GUIContent(objTmp.GetDisplayName()), true, null, null);
                }
                else 
                {
                    menuTmp.AddItem(new GUIContent(objTmp.GetDisplayName()), false,
                                (object context) =>
                                {
                                    objTmp.CopyEditorDataFrom(tree);
                                    tree.Replace(objTmp);
                                    _updateFormatPositionsDirty = true;
                                }, null);
                }
            }

            menuTmp.ShowAsContext();
        }

        private void ShowSelectMenu(System.Action<string> callbackSelect, string[] enabledItems, string[] disabledItems = null)
        {
            GenericMenu menuTmp = new GenericMenu();

            for (int i = 0; i < enabledItems.Length; ++i)
            {
                menuTmp.AddItem(new GUIContent(enabledItems[i]), false, (object context)=>
                {
                    callbackSelect(context.ToString());
                }, enabledItems[i]);
            }

            for (int i = 0; i < disabledItems.Length; ++i)
            {
                menuTmp.AddDisabledItem(new GUIContent(disabledItems[i]));
            }

            menuTmp.ShowAsContext();
        }

        private bool IsSelectedTree(shaco.Base.BehaviourTree tree)
        {
            bool retValue = false;
            for (int i = _currentSelectTrees.Count - 1; i >= 0; --i)
            {
                if (tree == _currentSelectTrees[i])
                {
                    retValue = true;
                    break;
                }
            }
            return retValue;
        }

        private void ForceSelectTrees(shaco.Base.BehaviourTree[] tree)
        {
            _currentSelectTrees.Clear();
            _currentSelectTrees.AddRange(tree);
        }

        private void ForceUnSelectTrees()
        {
            _currentSelectTrees.Clear();
        }

        private void ExcueteRemove()
        {
            if (_currentSelectTrees.Count == 0) return;

            for (int i = _currentSelectTrees.Count - 1; i >= 0; --i)
                _currentSelectTrees[i].RemoveMe();
            _currentSelectTrees.Clear();
                
            _updateFormatPositionsDirty = true;
        }

        private void OnDragEnd(Event curEvent)
        {
            if (_currentDragTrees.Count > 0)
            {
                if (SelectBehaviourTree(curEvent))
                {
                    var parentTree = _currentSelectTrees[0];
                    for (int i = 0; i < _currentDragTrees.Count; ++i)
                    {
                        var dragTree = _currentDragTrees[i];
                        if (dragTree == parentTree)
                            continue;
                        else
                        {
                            int indexDragTree = dragTree.GetSiblingIndex();
                            var oldDragParent = dragTree.parent;
                            dragTree.RemoveMe(true);
                            switch (_currentDragTargetDirection)
                            {
                                case shaco.Direction.Up: 
                                {
                                    var parent = parentTree.parent;
                                    if (null != parent)
                                    {
                                        int indexParentTree = parentTree.GetSiblingIndex();
                                        parentTree.RemoveMe(true);
                                        parent.InsertChild(dragTree, indexParentTree);
                                        dragTree.InsertChild(parentTree, indexDragTree);
                                    }
                                    break;
                                }
                                case shaco.Direction.Down: 
                                {
                                    if (null != oldDragParent && parentTree.parent == dragTree)
                                    {
                                        parentTree.RemoveMe(true);
                                        oldDragParent.InsertChild(parentTree, indexDragTree);
                                    }
                                    parentTree.AddChild(dragTree);
                                    break;
                                }
                                case shaco.Direction.Left:
                                {
                                    parentTree.InsertSibling(dragTree, 0);
                                    break;
                                }
                                case shaco.Direction.Right: 
                                    parentTree.AddSibling(dragTree); 
                                    break;
                                default: shaco.Log.Error("unsupport direction !"); break;
                            }
                            _updateFormatPositionsDirty = true;
                        }
                    }
                }
                _currentDragTrees.Clear();
                _currentSelectTrees.Clear();
                _currentDragTargetParentTree = null;
            }
        }

        private void OnMouseDragEvent(Event curEvent)
        {
            if (_currentDragTrees.Count == 0)
            {
                if (_currentSelectTrees.Count < 2)
                    SelectBehaviourTree(curEvent);

                _currentDragTrees.Clear();
                _currentDragTrees.AddRange(_currentSelectTrees);
                if (_currentDragTrees.Count > 0)
                {
                    DragAndDrop.PrepareStartDrag();
                    Object[] dragAssets = new Object[_currentDragTrees.Count];
                    for (int i = 0; i < dragAssets.Length; ++i)
                        dragAssets[i] = _currentDragTrees[i].editorAssetProcess;
                    DragAndDrop.objectReferences = dragAssets;

                    DragAndDrop.StartDrag(dragAssets.Length == 1 ? "MoveChild" : "MoveMuiltyChild");
                }
            }
        }

        private void CheckDragTargetParentHighLight(Event curEvent)
        {
            if (_currentDragTrees.Count == 0) return;

            if (SelectBehaviourTree(curEvent))
            {
                var realMousePosition = curEvent.mousePosition - GetDrawOffset();
                _currentDragTargetParentTree = _currentSelectTrees[0];

                bool isDragTarget = false;
                for (int i = _currentDragTrees.Count - 1; i >= 0; --i)
                {
                    if (_currentDragTrees[i] == _currentDragTargetParentTree)
                    {
                        isDragTarget = true;
                        break;
                    }
                }

                if (isDragTarget)
                    _currentDragTargetParentTree = null;
                else 
                {
                    var areaOffset = new Vector2(_currentDragTargetParentTree.editorDrawPosition.width * DRAG_OFFSET_AREA.x, _currentDragTargetParentTree.editorDrawPosition.height * DRAG_OFFSET_AREA.y);

                    //left 
                    if (!_currentDragTargetParentTree.IsRoot() && realMousePosition.x < _currentDragTargetParentTree.editorDrawPosition.x + areaOffset.x)
                        _currentDragTargetDirection = shaco.Direction.Left;
                    //right
                    else if (!_currentDragTargetParentTree.IsRoot() && realMousePosition.x > _currentDragTargetParentTree.editorDrawPosition.xMax - areaOffset.x)
                        _currentDragTargetDirection = shaco.Direction.Right;
                    //up
                    else if (!_currentDragTargetParentTree.IsRoot() && _currentDragTrees.Count == 1 && realMousePosition.y < _currentDragTargetParentTree.editorDrawPosition.y + areaOffset.y)
                        _currentDragTargetDirection = shaco.Direction.Up;
                    //down
                    else if (realMousePosition.y > _currentDragTargetParentTree.editorDrawPosition.yMax - areaOffset.y)
                        _currentDragTargetDirection = shaco.Direction.Down;
                }
            }
            else 
                _currentDragTargetParentTree = null;
        }
    }
}