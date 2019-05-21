using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class BehaviourTreeRunTimeViewer : EditorWindow
    {
		private class RootTreeInfo
		{
			public string variableName = string.Empty;
			public Object target = null;
			public shaco.Base.BehaviourRootTree rootTree = null;
		}

	    private BehaviourTreeRunTimeViewer _currentWindow = null;
		private Dictionary<Object, List<RootTreeInfo>> _currentRootTrees = new Dictionary<Object, List<RootTreeInfo>>();
		private RootTreeInfo _currentRootTree = null; 
		private GameObject[] _currentSelectTargets = null;
		private Vector2 _leftWindowScrollPosition = Vector2.zero;
        private Vector2 _rightWindowScrollPosition = Vector2.zero;
		private string _searchRooTreeName = string.Empty;
		private string _searchParameterName = string.Empty;
        private bool _isApplicationPlaying = false;

        [MenuItem("shaco/Viewer/BehavirourTreeRuntimeViewer %&6", false, (int)ToolsGlobalDefine.MenuPriority.Viewer.BEHAVIOUR_RUNTIME_TREE)]
        static private void OpenBehaviourTreeRunTimeViewer()
		{
            EditorHelper.GetWindow<BehaviourTreeRunTimeViewer>(null, true, "BehaviourTreeRunTimeViewer").Init();
		}

		void OnEnable()
		{
            _currentWindow = EditorHelper.GetWindow(this, true, "BehaviourTreeRunTimeViewer");
            _currentWindow.Init();
        }

		void OnDestroy()
		{
            CheckBehaviourViwerClose();
		}

		void OnGUI()
		{
			if (null == _currentWindow)
            {
                return;
            }

            UpdateApplicationChanged();

            DrawLeftWindow();
            DrawCurrentSelectRootTree();
			base.Repaint();
		}

        private void Init()
        {
            _isApplicationPlaying = Application.isPlaying;
            UpdateInScene();
        }

		private void UpdateInScene()
		{
            _currentSelectTargets = GameObject.FindObjectsOfType<GameObject>();
            CollectionBehaviourRootTrees(_currentSelectTargets);
		}

        private void UpdateApplicationChanged()
        {
            if (_isApplicationPlaying != Application.isPlaying)
            {
                _isApplicationPlaying = Application.isPlaying;
                UpdateInScene();
            }
        }

		private void DrawLeftWindow()
		{
            GUILayout.BeginArea(new Rect(0, 0, Screen.width / 3, Screen.height));
            {
                GUILayout.BeginVertical("box");
                {
                    _searchRooTreeName = GUILayoutHelper.SearchField(_searchRooTreeName);
                    _leftWindowScrollPosition = GUILayout.BeginScrollView(_leftWindowScrollPosition);
                    {
                        // if (!Selection.gameObjects.IsNullOrEmpty() && GUILayout.Button("Update In Selected GameObjects"))
                        // {
                        //     _currentSelectTargets = Selection.gameObjects;
                        //     CollectionBehaviourRootTrees(_currentSelectTargets);
                        // }
                        if (GUILayout.Button("Update In Scene"))
                        {
                            UpdateInScene();
                        }
                        DrawCurrentRootTress();
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
		}

		private void DrawCurrentRootTress()
		{
			if (_currentRootTrees.Count == 0)
				return;

			if (string.IsNullOrEmpty(_searchRooTreeName))
			{
                foreach (var iter in _currentRootTrees)
                {
                    DrawVaribles(iter.Key.name, iter.Value);
                }
			}
			else 
			{
                var searchLowerName = _searchRooTreeName.ToLower();
                foreach (var iter in _currentRootTrees)
				{
					if (iter.Key.name.ToLower().Contains(searchLowerName))
					{
                        DrawVaribles(iter.Key.name, iter.Value);
                    }
				}
			}
		}

		private void DrawVaribles(string label, List<RootTreeInfo> treeInfo)
		{
            if (GUILayoutHelper.DrawHeader(label, label))
            {
                foreach (var iter2 in treeInfo)
                {
                    if (GUILayout.Button(iter2.variableName))
                    {
                        _currentRootTree = iter2;
                        BehaviourTreeViewer.OpenBehaviourTreeViewer(_currentRootTree.rootTree);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
		}

		private void DrawCurrentSelectRootTree()
		{
            if (null == _currentRootTree || _currentRootTree.rootTree == null)
                return;

            var rightWindowRect = new Rect(Screen.width / 3, 0, Screen.width / 3 * 2, Screen.height);
			GUILayout.BeginArea(rightWindowRect);
			{
                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Parameter view can only be used at run time", MessageType.Warning);
                }
                else 
                {
                    GUILayout.BeginVertical("box");
                    {
                        _searchParameterName = GUILayoutHelper.SearchField(_searchParameterName);

                        //draw parameters
                        var parameters = _currentRootTree.rootTree.GetParameters();
                        GUILayoutHelper.DrawHeaderText("Params Count: " + parameters.Count);

                        _rightWindowScrollPosition = GUILayout.BeginScrollView(_rightWindowScrollPosition);
                        {
                            if (string.IsNullOrEmpty(_searchParameterName))
                            {
                                foreach (var iter in parameters)
                                {
                                    if (iter.IsNull())
                                        continue;

                                    if (GUILayoutHelper.DrawHeader(iter.ToString(), iter.ToString()))
                                    {
                                        GUILayoutHelper.DrawObject(iter);
                                    }
                                }
                            }
                            else
                            {
                                var searchLowerName = _searchParameterName.ToLower();
                                foreach (var iter in parameters)
                                {
                                    if (iter.IsNull() || !iter.ToString().ToLower().Contains(searchLowerName))
                                        continue;

                                    if (GUILayoutHelper.DrawHeader(iter.ToString(), iter.ToString()))
                                    {
                                        GUILayoutHelper.DrawObject(iter);
                                    }
                                }
                            }
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
			}
			GUILayout.EndArea();
		}

		//选中对象是否有改变 
		private bool IsChangedSelectGameObjects()
		{
            bool retValue = false;
            
			if (_currentSelectTargets == Selection.gameObjects)
				return retValue;

			if (null == _currentSelectTargets || _currentSelectTargets.Length != Selection.gameObjects.Length)
			{
				retValue = true;
			}
			else if (!Selection.gameObjects.IsNullOrEmpty())
			{
                for (int i = Selection.gameObjects.Length - 1; i >= 0; --i)
                {
					if (i > _currentSelectTargets.Length - 1 || _currentSelectTargets[i] != Selection.gameObjects[i])
					{
						retValue = true;
						break;
					}
                }
			}
			return retValue;
		}

        //从对象中查找并收集shaco.Base.BehaviourRootTree
        private void CollectionBehaviourRootTrees(GameObject[] targets)
		{
            _currentRootTree = null;
            _currentRootTrees.Clear();
            CheckBehaviourViwerClose();

            for (int i = 0; i < targets.Length; ++i)
			{
				var target = targets[i];
                CollectionBehaviourRootTrees(target);
			}
		}

        private void CollectionBehaviourRootTrees(Object target)
		{
            var serializedObject = new SerializedObject(target);
            var iter = serializedObject.GetIterator();

            while (iter.Next(true))
            {
                if (iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue != null)
                {
                    var component = iter.objectReferenceValue as UnityEngine.Component;
                    if (component != null)
                    {
                        var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        foreach (var field in fields)
                        {
                            var valueTmp = field.GetValue(component);
                            if (null != valueTmp)
                            {
                                var rootTree = valueTmp as shaco.Base.BehaviourRootTree;
                                if (null != rootTree)
                                {
                                    List<RootTreeInfo> rootTreesList = null;
                                    if (!_currentRootTrees.ContainsKey(target))
                                    {
                                        rootTreesList = new List<RootTreeInfo>();
                                        _currentRootTrees.Add(target, rootTreesList);
                                    }
                                    else
                                    {
                                        rootTreesList = _currentRootTrees[target];
                                    }

                                    rootTreesList.Add(new RootTreeInfo()
                                    {
                                        variableName = field.Name,
                                        target = target,
                                        rootTree = rootTree
                                    });
                                }
                            }
                        }
                    }
                }
            }
		}

		private void CheckBehaviourViwerClose()
		{
            var treeViewer = EditorHelper.FindWindow<BehaviourTreeViewer>();
            if (null != treeViewer)
            {
                treeViewer.Close();
            }
		}
    }
}