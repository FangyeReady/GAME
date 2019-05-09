using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class BehaviourTreeViewer : EditorWindow
    {
        private shaco.Base.BehaviourRootTree _rootTree = new shaco.Base.BehaviourRootTree();
        private Object _assetBehaviourTree = null;
        private Vector2 _minItemMargin = new Vector2(10, 10);
        private float _defaultGUIWidth = 200;
        private float _defaultGUIHeight = 16;
        private Rect _maxTreeItemRect = new Rect();
        private Vector2 _scrollPossition = Vector2.zero;
        private bool _updateFormatPositionsDirty = false;
        private bool _updateFormatPositionsInNextLoopDirty = false;
        private Vector2 _rootTreeMoveToCenterOffset = Vector2.zero;
        private string _searchTreeName = string.Empty;
        private bool _shouldHideRootTreeFilePath = false;
        private float _arrowActionPercent = 0;

        [MenuItem("shaco/Viewer/BehavirourTreeViewer %&5", false, (int)ToolsGlobalDefine.MenuPriority.Viewer.BEHAVIOUR_TREE)]
        static public void OpenBehaviourTreeViewer()
        {
            var window = EditorHelper.GetWindow<BehaviourTreeViewer>(null, true, "BehaviourTreeViewer");
            window.Init();
        }

        static public BehaviourTreeViewer OpenBehaviourTreeViewer(shaco.Base.BehaviourRootTree rootTree)
        {
            if (null != rootTree)
            {
                var window = EditorHelper.GetWindow<BehaviourTreeViewer>(null, true, "BehaviourTreeViewer");
                window._rootTree = rootTree;
                window._shouldHideRootTreeFilePath = true;
                window.Init();
                return window;
            }
            else 
            {
                Debug.LogError("BehaviourTreeViewer OpenBehaviourTreeViewer error: root tree is invalid");
                return null;
            }
        }

        void OnDestroy()
        {
            Exit();
        }

        public void Draw()
        {
            var drawPosition = new Rect(0, 0, this.position.width, _defaultGUIHeight);

            if (!_shouldHideRootTreeFilePath)
            {
                GUI.changed = false;
                _assetBehaviourTree = (TextAsset)EditorGUI.ObjectField(drawPosition, "BehaviourTreePath: ", _assetBehaviourTree, typeof(TextAsset), true);
                if (GUI.changed)
                {
                    UpdateTree();
                    _updateFormatPositionsDirty = true;
                }
            }

            if (_updateFormatPositionsDirty)
            {
                FormatTreesPosition();
                _updateFormatPositionsDirty = false;
            }

            var windowRect = new Rect(0, _defaultGUIHeight, this.position.width, this.position.height - _defaultGUIHeight);
            _scrollPossition = GUI.BeginScrollView(windowRect, _scrollPossition, _maxTreeItemRect);
            {
                DrawTrees();
            }
            GUI.EndScrollView();

            DrawTreeLinkLines();
            DrawSearchField();

            if (_updateFormatPositionsInNextLoopDirty && Event.current.type == EventType.Layout)
            {
                _updateFormatPositionsInNextLoopDirty = false;
                _updateFormatPositionsDirty = true;
            }

            UpdateEvent();
        }

        void OnGUI()
        {
            this.Repaint();
            this.Draw();

            _arrowActionPercent += 0.01f;
            if (_arrowActionPercent > 1.0f)
                _arrowActionPercent = 0;
        }

        private void DrawSearchField()
        {
            GUILayout.Space(_defaultGUIHeight);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(this.position.width / 3 * 2);
                GUI.changed = false;
                _searchTreeName = GUILayoutHelper.SearchField(_searchTreeName, GUILayout.Width(this.position.width / 3 * 1));
                if (GUI.changed)
                {
                    if (!string.IsNullOrEmpty(_searchTreeName))
                    {
                        shaco.Base.BehaviourTree findTree = null;
                        _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
                        {
                            if (tree.name.ToLower().Contains(_searchTreeName.ToLower()))
                            {
                                findTree = tree;
                                return false;
                            }
                            else
                                return true;
                        });
                        if (null != findTree)
                        {
                            _scrollPossition = findTree.editorDrawPosition.position - _maxTreeItemRect.position - this.position.size / 2 + findTree.editorDrawPosition.size / 2;
                            ForceSelectTrees(new shaco.Base.BehaviourTree[] { findTree });
                        }
                    }
                    else
                    {
                        ForceUnSelectTrees();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawTrees()
        {
            if (null == _rootTree)
                return;

            DrawTree(_rootTree, 0, 0);
            _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
            {
                DrawTree(tree, index, level);
                return true;
            });
        }

        private void DrawTree(shaco.Base.BehaviourTree tree, int index, int level)
        {
            bool isSelectedTree = IsSelectedTree(tree) && tree != _currentDragTargetParentTree && !tree.IsRoot();
            var oldColor = GUI.color;

            if (isSelectedTree)
                GUI.color = Color.green;

            GUI.Box(tree.editorDrawPosition, string.Empty);

            GUILayout.BeginArea(tree.editorDrawPosition);
            {
                if (tree.editorHasInValidParam)
                {
                    EditorGUILayout.HelpBox("Has invalid param", MessageType.Warning);
                }

                if (!tree.IsRoot())
                {
                    GUI.changed = false;
                    EditorGUI.BeginDisabledGroup(_currentDragTrees.Count > 0);
                    {
                        tree.editorAssetProcess = (TextAsset)EditorGUILayout.ObjectField(tree.editorAssetProcess, typeof(TextAsset), true);
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUI.changed)
                        CheckScriptAssetChanged(tree);

                    EditorGUI.BeginDisabledGroup(true);
                    {
                        EditorGUILayout.TextField("Class: " + tree.name);
                    }
                    EditorGUI.EndDisabledGroup();
                }

                if (tree.root == tree)
                {
                    DrawRootTree(tree as shaco.Base.BehaviourRootTree);
                }
                else
                {
                    DrawTreeType(tree.GetDisplayName(), tree);
                }
                tree.OnGUIDraw();
                FixFormatTreeSizeWithGUILayout(tree, _defaultGUIWidth);
            }
            GUILayout.EndArea();

            if (isSelectedTree)
                GUI.color = oldColor;

            if (tree == _currentDragTargetParentTree)
                DragDragParentTree(tree);
        }

        private void DragDragParentTree(shaco.Base.BehaviourTree tree)
        {
            var oldColor = GUI.color;
            GUI.color = Color.green;

            var areaOffset = new Vector2(tree.editorDrawPosition.width * DRAG_OFFSET_AREA.x, tree.editorDrawPosition.height * DRAG_OFFSET_AREA.y);
            var rectPos = tree.editorDrawPosition;

            switch (_currentDragTargetDirection)
            {
                case shaco.Direction.Up:
                    {
                        var tmpVec = new Vector2(rectPos.width, areaOffset.y);
                        GUI.Button(new Rect(rectPos.position.x, rectPos.position.y, tmpVec.x, tmpVec.y), "Up");
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        var tmpVec1 = rectPos.position + new Vector2(0, rectPos.height - areaOffset.y);
                        var tmpVec2 = new Vector2(rectPos.width, areaOffset.y);
                        GUI.Button(new Rect(tmpVec1.x, tmpVec1.y, tmpVec2.x, tmpVec2.y), "Down");
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        var tmpVec = new Vector2(areaOffset.x, rectPos.height);
                        GUI.Button(new Rect(rectPos.position.x, rectPos.position.y, tmpVec.x, tmpVec.y), "Left");
                        break;
                    }
                case shaco.Direction.Right:
                    {
                        var tmpVec1 = rectPos.position + new Vector2(rectPos.width - areaOffset.x, 0);
                        var tmpVec2 = new Vector2(areaOffset.x, rectPos.height);
                        GUI.Button(new Rect(tmpVec1.x, tmpVec1.y, tmpVec2.x, tmpVec2.y), "Right");
                        break;
                    }
                default:
                    shaco.Log.Error("unsupport direction");
                    break;
            }

            GUI.color = oldColor;
        }

        private void DrawTreeLinkLines()
        {
            if (null == _rootTree)
                return;

            Color oldColor = GUI.color;

            _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
            {
                var rect1 = ((shaco.Base.BehaviourTree)tree.parent).editorDrawPosition;
                var rect2 = tree.editorDrawPosition;
                var offsetPos = GetDrawOffset();
                var point1 = new Vector3(rect1.center.x + offsetPos.x, +rect1.yMax + offsetPos.y);
                var point2 = new Vector3(rect2.center.x + offsetPos.x, +rect2.yMin + offsetPos.y);

                bool isRunning = tree.IsRunning();
                if (isRunning)
                {
                    GUI.color = Color.green;
                    GUIHelper.LineDraw.DrawLineWithArrow(point1, point2, 1, 10, _arrowActionPercent);
                    GUI.color = oldColor;
                }
                else 
                {
                    GUIHelper.LineDraw.DrawLine(point1, point2, 1);
                }

                return true;
            });
        }

        private Vector2 GetDrawOffset()
        {
            return new Vector2(-_maxTreeItemRect.x - _scrollPossition.x, -_maxTreeItemRect.y - _scrollPossition.y + _defaultGUIHeight);
        }

        protected void Init()
        {
            if (_rootTree.Count == 0)
            {
                var behaviourTreePath = shaco.DataSave.Instance.ReadString(EditorHelper.GetEditorPrefsKey("BehaviourTreeViewer.BehaviourtTree"));
                if (!string.IsNullOrEmpty(behaviourTreePath))
                {
                    _assetBehaviourTree = AssetDatabase.LoadAssetAtPath(behaviourTreePath, typeof(Object));
                    UpdateTree();
                }
            }
            _updateFormatPositionsDirty = true;
        }

        private void Exit()
        {
            if (null != _assetBehaviourTree)
                shaco.DataSave.Instance.Write(EditorHelper.GetEditorPrefsKey("BehaviourTreeViewer.BehaviourtTree"), AssetDatabase.GetAssetPath(_assetBehaviourTree));
        }

        private void UpdateTree()
        {
            if (null == _assetBehaviourTree)
                return;

            AssetDatabase.Refresh();

            if (!string.IsNullOrEmpty(_assetBehaviourTree.ToString()))
                shaco.Base.BehaviourTreeConfig.LoadFromJson(_assetBehaviourTree.ToString(), _rootTree);
        }

        private string GetLoadJsonPath()
        {
            if (null == _assetBehaviourTree)
            {
                return string.Empty;
            }
            else 
            {
                return shaco.Base.FileHelper.ContactPath(Application.dataPath.Remove("Assets"), AssetDatabase.GetAssetPath(_assetBehaviourTree));
            }
        }

        private void DrawRootTree(shaco.Base.BehaviourRootTree tree)
        {
            if (GUILayout.Button("New"))
            {
                ForceSelectTrees(new shaco.Base.BehaviourTree[] { _rootTree });
                ShowContextMenu(true);
            }

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save"))
                {
                    if (null == _assetBehaviourTree)
                    {
                        SaveAs(GetLoadJsonPath());
                    }
                    else
                    {
                        _rootTree.SaveToJson(GetLoadJsonPath());
                        AssetDatabase.Refresh();
                    }
                }
                if (GUILayout.Button("Save As"))
                {
                    SaveAs(GetLoadJsonPath());
                }
            }
            GUILayout.EndHorizontal();
            
            if (null != _assetBehaviourTree && GUILayout.Button("Revert"))
            {
                _rootTree.LoadFromJsonPath(GetLoadJsonPath());
            }
            // if (GUILayout.Button("CheckValid"))
            // {
            //     CheckAllTreeParamValid();
            // }
            FixFormatTreeSizeWithGUILayout(tree, _defaultGUIWidth);
        }

        private void SaveAs(string defaultPath)
        {
            var folderTmp = string.IsNullOrEmpty(defaultPath) ? Application.dataPath : shaco.Base.FileHelper.GetFolderNameByPath(defaultPath);
            var filenameTmp = string.IsNullOrEmpty(defaultPath) ?  "BehaviourConfig" : shaco.Base.FileHelper.GetLastFileName(defaultPath);
            var pathTmp = EditorUtility.SaveFilePanel("Save Config", folderTmp, filenameTmp, "json");
            if (!string.IsNullOrEmpty(pathTmp))
            {
                _rootTree.SaveToJson(pathTmp);
                AssetDatabase.Refresh();
            }
        }

        private void DrawTreeType(string typeName, shaco.Base.BehaviourTree tree, int fontSize = 20)
        {
            GUILayout.BeginHorizontal();
            {
                var oldSize = GUI.skin.label.fontSize;
                GUI.skin.label.fontSize = fontSize;
                GUILayout.Label(typeName, GUILayout.Height(30));
                GUI.skin.label.fontSize = oldSize;

                if (GUILayout.Button("Change", GUILayout.Width(55)))
                {
                    ShowChangeTreeTypeMenu(tree);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void GetITreeProcessClassNames(shaco.Base.BehaviourTree tree, out List<string> enabledClasses, out List<string> disabledClasses)
        {
            enabledClasses = new List<string>();
            disabledClasses = new List<string>();

            var textScript = tree.editorAssetProcess.text;
            var parentFullName = tree.parent.fullName;
            var rootTreeTmp = tree.root as shaco.Base.BehaviourRootTree;

            while (true)
            {
                var className = shaco.Base.Utility.GetFullClassName(ref textScript, typeof(shaco.Base.IBehaviourProcess));
                if (string.IsNullOrEmpty(className))
                    break;
                else
                {
                    if (rootTreeTmp.HasTree(parentFullName + "." + className))
                    {
                        disabledClasses.Add(className);
                    }
                    else 
                    {
                        enabledClasses.Add(className);
                    }
                }
            }
        }

        private void CheckScriptAssetChanged(shaco.Base.BehaviourTree tree)
        {
            if (tree.editorAssetProcess == null)
            {
                tree.name = string.Empty;
                tree.editorHasInValidParam = true;
                return;
            }

            List<string> enabledClasses = null;
            List<string> disabledClasses = null;
            GetITreeProcessClassNames(tree, out enabledClasses, out disabledClasses);
            if (enabledClasses.Count == 0 && disabledClasses.Count == 0)
            {
                tree.editorAssetProcess = null;
                tree.name = string.Empty;
                shaco.Log.Error("BehaviourTreeViewer DrawTree error: asset not a 'IBehaviourProcess' script ! please check class Inherit from 'shaco.Base.IBehaviourProcess'");
            }
            else if (enabledClasses.Count == 1)
                tree.name = enabledClasses[0];
            else
            {
                ShowSelectMenu((string selectName) =>
                {
                    tree.name = selectName;
                    if (null != _currentSelectScript)
                    {
                        tree.editorAssetProcess = _currentSelectScript;
                        tree.editorAssetPathProcess = AssetDatabase.GetAssetPath(tree.editorAssetProcess);

                        if (tree.editorAssetProcess != null && !string.IsNullOrEmpty(tree.name))
                        {
                            tree.editorHasInValidParam = false;
                        }
                    }
                }, enabledClasses.ToArray(), disabledClasses.ToArray());

                _currentSelectScript = tree.editorAssetProcess;
                tree.editorAssetProcess = null;
                tree.name = string.Empty;
                tree.editorHasInValidParam = true;
            }
        }
    }
}