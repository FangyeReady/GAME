using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using shaco.Base;

namespace shacoEditor
{
    public class FindReferenceInspector : EditorWindow
    {
        public class DependenceInfo
        {
            public string assetSelect = null;
            public List<string> listDependencies = new List<string>();

            public DependenceInfo(string select)
            {
                this.assetSelect = select;
            }
        }

        public enum FindStatus
        {
            None,
            NoReference,
            FindReference,
        }

        private FindReferenceInspector _currentWindow = null;
        private FolderDrawerEditor _folderDrawer = new FolderDrawerEditor();
        private Rect _rectDrawFolder = new Rect();
        private FindStatus _statusFind = FindStatus.None;
        private int _iReferenceCountTmp = 0;
        private int _iUnUseCountTmp = 0;

        [MenuItem("Assets/Find References In Project %#f", false, ToolsGlobalDefine.ProjectMenuPriority.FIND_REFERENCE)]
        static void OpenFindReferenceWindowInProjectMenu()
        {
            var selectAssetTmp = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

            if (selectAssetTmp.Length > 0)
            {
                var assetsPathTmp = new string[selectAssetTmp.Length];
                for (int i = 0; i < selectAssetTmp.Length; ++i)
                {
                    assetsPathTmp[i] = AssetDatabase.GetAssetPath(selectAssetTmp[i]);
                }
                OpenFindReferenceWindow().FindReferencesInProject(assetsPathTmp);
            }
        }

        static FindReferenceInspector OpenFindReferenceWindow()
        {
            var retValue = EditorHelper.GetWindow<FindReferenceInspector>(null, true, "FindReference");
            retValue.Show();
            retValue.Init();
            return retValue;
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<FindReferenceInspector>(this, true, "FindReference");
        }

        void Init()
        {
            _folderDrawer.AddIgnoreFolderTag(".");
            _folderDrawer.AddCustomContextMenu("ChangeReferences", (string selectFullPath, List<string> allFiles) =>
            {
                selectFullPath = selectFullPath.Remove("(Reference)");
                selectFullPath = selectFullPath.Remove("(Unuse)");

                var allAssetsTmp = new List<Object>();
                for (int i = 0; i < allFiles.Count; ++i)
                {
                    var realPath = allFiles[i].Remove("(Reference)");
                    realPath = realPath.Remove("(Unuse)");
                    allAssetsTmp.Add(AssetDatabase.LoadAssetAtPath(realPath, typeof(Object)));
                }

                ChangeComponentDataInspector.OpenChangeComponentDataWindowInProjectMenu(AssetDatabase.LoadAssetAtPath(selectFullPath, typeof(Object)), allAssetsTmp);
            });
            _folderDrawer.AddCustomContextMenu("PrintSerializedstring", (string selectFullPath, List<string> allFiles) =>
            {
                selectFullPath = selectFullPath.Remove("(Reference)");
                selectFullPath = selectFullPath.Remove("(Unuse)");
                var loadObj = AssetDatabase.LoadAssetAtPath(selectFullPath, typeof(Object));
                EditorHelper.PrintSerializedObject(loadObj);
            });
        }

        void OnDestroy()
        {
            var windowTmp = EditorHelper.FindWindow<ChangeComponentDataInspector>();
            if (windowTmp != null)
            {
                windowTmp.Close();
            }

            _currentWindow = null;
        }

        void OnGUI()
        {
            if (_currentWindow == null)
            {
                return;
            }
            this.Repaint();

            if (_statusFind == FindStatus.NoReference)
            {
                GUILayout.Label("No reference find\n");
            }
            else if (_statusFind == FindStatus.FindReference)
            {
                GUILayout.Label("unuse count: " + _iUnUseCountTmp + "\nuse count: " + _iReferenceCountTmp);
            }
            else
                GUILayout.Label("please select a asset in ''Project' window\n");

            if (_rectDrawFolder.x == 0 && _rectDrawFolder.y == 0)
                _rectDrawFolder = GUILayoutUtility.GetLastRect();
            var rectFolderDraw = new Rect(0, _rectDrawFolder.y + _rectDrawFolder.height, _currentWindow.position.width, _currentWindow.position.height);
            _folderDrawer.DrawFolder(rectFolderDraw);
        }

        private void FindReferencesInProject(params string[] selectAssets)
        {
            _folderDrawer.ClearFile();
            _statusFind = FindStatus.None;

            //some prefab only can changed when save scene
            if (!Application.isPlaying)
            {
                EditorHelper.SaveCurrentScene();
            }

            _iReferenceCountTmp = 0;
            _iUnUseCountTmp = 0;
            if (selectAssets.Length > 0)
            {
                GetDependenciesInProject(selectAssets, (Dictionary<string, DependenceInfo> mapFindDependencies) =>
                {
                    foreach (var iter in mapFindDependencies)
                    {
                        var listDependenciesTmp = iter.Value.listDependencies;
                        var pathAsset = iter.Value.assetSelect;

                        if (listDependenciesTmp.Count == 0)
                        {
                            var pathUnuse = pathAsset.ReplaceFromBegin("Assets" + FileDefine.PATH_FLAG_SPLIT, "Assets(Unuse)" + FileDefine.PATH_FLAG_SPLIT, 1);
                            _folderDrawer.AddFile(pathUnuse, iter.Value.assetSelect);
                            ++_iUnUseCountTmp;
                        }
                        else
                        {
                            for (int j = listDependenciesTmp.Count - 1; j >= 0; --j)
                            {
                                var pathassetRef = iter.Value.assetSelect.ReplaceFromBegin("Assets" + FileDefine.PATH_FLAG_SPLIT, "Assets(Reference)" + FileDefine.PATH_FLAG_SPLIT, 1);
                                _folderDrawer.AddFile(pathassetRef + FileDefine.PATH_FLAG_SPLIT, listDependenciesTmp[j]);
                            }
                            ++_iReferenceCountTmp;
                        }
                    }

                    _statusFind = mapFindDependencies.Count > 0 ? FindStatus.FindReference : FindStatus.NoReference;
                    _folderDrawer.OpenAllFolder();

                    AssetDatabase.Refresh();
                });
            }
        }

        static private void GetDependenciesInProject(string[] select, System.Action<Dictionary<string, DependenceInfo>> callback)
        {
            var ret = new Dictionary<string, DependenceInfo>();
            var listSeekPathTmp = new List<string>();
            bool shouldCancel = false;

            shouldCancel = EditorUtility.DisplayCancelableProgressBar("get all files...", "please wait", 0);

            FileHelper.GetSeekPath(Application.dataPath, ref listSeekPathTmp, false, ".prefab", ".unity", ".asset");

            for (int i = 0; i < listSeekPathTmp.Count; ++i)
            {
                listSeekPathTmp[i] = listSeekPathTmp[i].Remove(0, listSeekPathTmp[i].IndexOf("Assets"));
            }

            Debug.Log("find all files count=" + listSeekPathTmp.Count);

            //collection all files dependencies
            var allDependencies = new Dictionary<string, string[]>();
            shaco.Base.Coroutine.Foreach(listSeekPathTmp, (object data) =>
            {
                CollectionDepencies(allDependencies, data);
                return !shouldCancel;
            }, (float percent) =>
            {
                shouldCancel = EditorUtility.DisplayCancelableProgressBar("collection reference...", "please wait", 0.5f * percent);
                if (shouldCancel)
                {
                    EditorUtility.ClearProgressBar();
                }

                if (percent >= 1)
                {
                    //select dependencies
                    shaco.Base.Coroutine.Foreach(select, (object data) =>
                    {
                        return !shouldCancel && SelectDepencies(ret, listSeekPathTmp, allDependencies, data);
                    }, (float percent2) =>
                    {
                        shouldCancel = EditorUtility.DisplayCancelableProgressBar("select depencies...", "please wait", 0.5f * percent2 + 0.5f);
                        if (percent2 >= 1)
                        {
                            EditorUtility.ClearProgressBar();
                            callback(ret);
                        }
                    }, 0.1f);
                }
            }, 0.1f);
        }

        static private void CollectionDepencies(Dictionary<string, string[]> allDependencies, object data)
        {
            var pathTmp = data as string;
#if UNITY_5_3_OR_NEWER
            var listDependence = AssetDatabase.GetDependencies(pathTmp, true);
#else
            var listDependence = AssetDatabase.GetDependencies(new string[]{ pathTmp });
#endif
            allDependencies.Add(pathTmp, listDependence);
        }

        static private bool SelectDepencies(Dictionary<string, DependenceInfo> selectDepencies, List<string> listSeekPath, Dictionary<string, string[]> allDependencies, object data)
        {
            var selectAsset = data as string;
            var selectPath = selectAsset;
            if (FileHelper.ExistsDirectory(selectPath))
                return true;

            var selectGUID = AssetDatabase.AssetPathToGUID(selectPath);

            if (!string.IsNullOrEmpty(selectPath) && !selectDepencies.ContainsKey(selectAsset))
            {
                selectDepencies.Add(selectAsset, new DependenceInfo(selectAsset));
            }
            DependenceInfo selectDependenceInfo = selectDepencies[selectAsset];

            for (int j = listSeekPath.Count - 1; j >= 0; --j)
            {
                var pathTmp = listSeekPath[j];
                var listDependence = allDependencies[pathTmp];
                for (int k = 0; k < listDependence.Length; ++k)
                {
                    if (AssetDatabase.AssetPathToGUID(listDependence[k]) == selectGUID)
                    {
                        selectDependenceInfo.listDependencies.Add(pathTmp);
                        break;
                    }
                }
            }

            if (!selectDepencies.ContainsKey(selectAsset))
            {
                Debug.LogError("not find asset=" + selectAsset + " in dictionary");
                return false;
            }

            //remove self dependence
            var listDependenciesTmp = selectDependenceInfo.listDependencies;
            for (int j = listDependenciesTmp.Count - 1; j >= 0; --j)
            {
                if (listDependenciesTmp[j] == selectAsset)
                {
                    listDependenciesTmp.RemoveAt(j);
                }
            }
            return true;
        }
    }
}
