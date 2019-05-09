using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;

namespace shacoEditor
{
    public class ExportPackageEditor : EditorWindow
    {
        public class ExportPackageInfo
        {
            public string PathPackage = string.Empty;
            public List<string> ListIncludePath = new List<string>();
        }

        public class ExportInfo
        {
            public ExportPackageInfo PackageInfo = new ExportPackageInfo();
        }

        private List<ExportInfo> _listExportInfo = new List<ExportInfo>();
        private Object[] _selectAsset = null;
        private ExportPackageEditor _currentWindow = null;
        static private Rect _rectWindow = new Rect();
        private Vector2 _vec2ScrollPosition = new Vector2();

        [MenuItem("shaco/Tools/ExportUnityPackage", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.EXPORT_PACKAGE_1)]
        static ExportPackageEditor OpenExportDialog()
        {
            var retValue = EditorHelper.GetWindow<ExportPackageEditor>(null, true, "ExportPackage") as ExportPackageEditor;
            retValue.Show();
            retValue.autoRepaintOnSceneChange = true;
            retValue.LoadConfig();
            _rectWindow = retValue.position;

            return retValue;
        }

        [MenuItem("shaco/Tools/ExportUnityPackage(Previous) %#p", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.EXPORT_PACKAGE_2)]
        static void ExportPackagePrevious()
        {
            var retValue = OpenExportDialog();
            retValue.Close();
            retValue.OnExportButtonClick();

            Debug.Log("export package success");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<ExportPackageEditor>(this, true, "ExportPackage") as ExportPackageEditor;
        }

        void OnGUI()
        {
            if (_currentWindow == null)
            {
                return;
            }
            this.Repaint();

            _rectWindow = new Rect(_rectWindow.x, _rectWindow.y, _currentWindow.position.width, _currentWindow.position.height);

            _selectAsset = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            GUILayout.BeginArea(_rectWindow);
            _vec2ScrollPosition = GUILayout.BeginScrollView(_vec2ScrollPosition);

            if (_selectAsset.Length > 0 && GUILayout.Button("New Package"))
            {
                var savePath = EditorUtility.SaveFilePanel("Select a New Package Path", Application.dataPath, string.Empty, "unitypackage");

                if (!string.IsNullOrEmpty(savePath))
                {
                    var newInfo = new ExportInfo();
                    newInfo.PackageInfo.PathPackage = savePath;
                    _listExportInfo.Add(newInfo);
                    UpdateAllSelectPath();
                }
            }

            if (_listExportInfo.Count > 0)
            {
                if (GUILayout.Button("Clear"))
                {
                    if (EditorUtility.DisplayDialog("Warning", "Clear All Config", "Confirm", "Cancel"))
                    {
                        _listExportInfo.Clear();
                    }
                }

                if (GUILayout.Button("ExportPackages", GUILayout.Height(50)))
                {
                    OnExportButtonClick();
                }
            }

            var oldColor = GUI.color;

            List<string> listRemove = new List<string>();
            for (int i = 0; i < _listExportInfo.Count; ++i)
            {
                if (string.IsNullOrEmpty(_listExportInfo[i].PackageInfo.PathPackage))
                    GUI.color = Color.green;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    if (!listRemove.Contains(_listExportInfo[i].PackageInfo.PathPackage))
                        listRemove.Add(_listExportInfo[i].PackageInfo.PathPackage);
                }
                EditorGUILayout.TextField("Package Path", _listExportInfo[i].PackageInfo.PathPackage);
                GUILayout.EndHorizontal();

                if (string.IsNullOrEmpty(_listExportInfo[i].PackageInfo.PathPackage))
                    GUI.color = oldColor;

                if (!string.IsNullOrEmpty(_listExportInfo[i].PackageInfo.PathPackage))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.TextField("Data Size", _listExportInfo[i].PackageInfo.ListIncludePath.Count.ToString());
                    GUILayout.EndHorizontal();

                    for (int j = 0; j < _listExportInfo[i].PackageInfo.ListIncludePath.Count; ++j)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        EditorGUILayout.TextField(_listExportInfo[i].PackageInfo.ListIncludePath[j]);
                        GUILayout.EndHorizontal();
                    }
                }
            }

            for (int i = 0; i < listRemove.Count; ++i)
            {
                for (int j = _listExportInfo.Count - 1; j >= 0; --j)
                {
                    if (_listExportInfo[j].PackageInfo.PathPackage == listRemove[i])
                    {
                        _listExportInfo.RemoveAt(j);
                        break;
                    }
                }
            }

            //draw current select assets
            EditorGUILayout.LabelField("Select Folder Count", _selectAsset.Length.ToString());
            for (int i = 0; i < _selectAsset.Length; ++i)
            {
                EditorGUILayout.ObjectField(_selectAsset[i], typeof(Object), true);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            SaveConfig();
        }

        void SaveConfig()
        {
            var jsonWrite = shaco.LitJson.JsonMapper.ToJson(_listExportInfo);
            FileHelper.WriteAllByPersistent("ExportPackageEditorConfig.json", jsonWrite);
        }

        void LoadConfig()
        {
            var jsonRead = FileHelper.ReadAllByPersistent("ExportPackageEditorConfig.json");

            if (!string.IsNullOrEmpty(jsonRead))
                _listExportInfo = shaco.LitJson.JsonMapper.ToObject<List<ExportInfo>>(jsonRead.ToStringArrayUTF8());
        }

        void UpdateAllSelectPath()
        {
            if (_listExportInfo.Count == 0)
                return;

            var packageInfo = _listExportInfo[_listExportInfo.Count - 1];
            packageInfo.PackageInfo.ListIncludePath.Clear();

            for (int i = 0; i < _selectAsset.Length; ++i)
            {
                var pathTmp = AssetDatabase.GetAssetPath(_selectAsset[i]);
                packageInfo.PackageInfo.ListIncludePath.Add(pathTmp);
            }
        }

        string[] GetAssetsDeep(List<string> listFolder)
        {
            List<string> listFiles = new List<string>();

            for (int i = 0; i < listFolder.Count; ++i)
            {
                List<string> listSeek = new List<string>();
                FileHelper.GetSeekPath(listFolder[i], ref listSeek);

                listFiles.AddRange(listSeek);
            }

            return listFiles.ToArray();
        }

        private void OnExportButtonClick()
        {
            ExportPackageOptions options = ExportPackageOptions.Recurse;

            for (int i = 0; i < _listExportInfo.Count; ++i)
            {
                var packageInfo = _listExportInfo[i].PackageInfo;

                if (packageInfo.ListIncludePath.Count > 0)
                {
                    var listFilesTmp = GetAssetsDeep(packageInfo.ListIncludePath);

                    AssetDatabase.ExportPackage(listFilesTmp, packageInfo.PathPackage, options);
                }
            }
        }
    }
}
