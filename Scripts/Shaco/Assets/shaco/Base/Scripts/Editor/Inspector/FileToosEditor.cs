using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;

namespace shacoEditor
{
    public class FileToosEditor : EditorWindow
    {
        private class FileSizeInfo
        {
            public Object asset = null;
            public string path = string.Empty;
            public long fileSize = 0;
        }

        private Object _objPrevSelect = null;
        private string _strGetMD5 = string.Empty;
        private string _strSelectPath = string.Empty;
        private List<string> _listDeleteFileTag = new List<string>();
        private bool _isFolder = false;
        private List<FileSizeInfo> _listFileSize = new List<FileSizeInfo>();
        private long _lTotalSize = 0;
        private Vector2 _vec2ScrollPos = Vector2.zero;

        private FileToosEditor _currentWindow = null;

        [MenuItem("shaco/Tools/FileTools", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.FILE_TOOLS)]
        static FileToosEditor OpenFileToolsWindow()
        {
            var retValue = EditorHelper.GetWindow<FileToosEditor>(null, true, "FileTools");
            retValue.Init();
            retValue.Show();
            return retValue;
        }

        [MenuItem("shaco/OpenFolder/DataPath")]
        static void OpenDataPath()
        {
            System.Diagnostics.Process.Start(Application.dataPath);
        }

        [MenuItem("shaco/OpenFolder/PersistentDataPath")]
        static void OpenPersistentDataPath()
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }

        [MenuItem("shaco/OpenFolder/StreamingAssetsPath")]
        static void OpenStreamingAssetsPath()
        {
            System.Diagnostics.Process.Start(Application.streamingAssetsPath);
        }

        [MenuItem("shaco/OpenFolder/TemporaryCachePath")]
        static void OpenTemporaryCachePath()
        {
            System.Diagnostics.Process.Start(Application.temporaryCachePath);
        }

        [MenuItem("shaco/OpenFolder/ShacoFrameworkPath")]
        static void OpenShacoFrameworkPath()
        {
            System.Diagnostics.Process.Start(Application.dataPath + "/shaco");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<FileToosEditor>(this, true, "FileTools");
        }

        void Init()
        {

        }

        void OnDestroy()
        {

        }

        void OnGUI()
        {
            if (_currentWindow == null)
            {
                return;
            }

            this.Repaint();
            _vec2ScrollPos = GUILayout.BeginScrollView(_vec2ScrollPos);

            //draw select asset md5
            var select = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            if (select != null && select.Length == 1)
            {
                if (select[0] != _objPrevSelect)
                {
                    _objPrevSelect = select[0];
                    _strSelectPath = Application.dataPath.Remove("Assets");
                    _strSelectPath = FileHelper.ContactPath(_strSelectPath, AssetDatabase.GetAssetPath(_objPrevSelect));
                    _strGetMD5 = FileHelper.MD5FromFile(_strSelectPath);

                    _isFolder = FileHelper.ExistsDirectory(_strSelectPath);
                }
            }
            else
            {
                _objPrevSelect = null;
                _strSelectPath = string.Empty;
                _strGetMD5 = string.Empty;
            }

            if (!string.IsNullOrEmpty(_strGetMD5))
            {
                EditorGUILayout.TextField("MD5", _strGetMD5);
            }

            if (!string.IsNullOrEmpty(_strSelectPath))
            {
                if (_isFolder)
                {
                    //delete folder files by tag
                    EditorGUILayout.HelpBox("Delete all files by tag by select folder's path", MessageType.Info);
                    EditorGUILayout.ObjectField("select folder", _objPrevSelect, typeof(Object), true);
                    GUILayoutHelper.DrawList(_listDeleteFileTag, "Delete File Tag");

                    if (_listDeleteFileTag.Count > 0)
                    {
                        if (GUILayout.Button("Delete File By Tag"))
                        {
                            //修建掉空字符串
                            _listDeleteFileTag.Trim(string.Empty);

                            if (_listDeleteFileTag.Count > 0)
                            {
                                FileHelper.DeleteFileByTag(_strSelectPath, _listDeleteFileTag.ToArray());
                                AssetDatabase.Refresh();
                            }
                        }
                    }
                }

                if (GUILayout.Button("Computer File Size"))
                {
                    _lTotalSize = 0;
                    _listFileSize.Clear();
                    var listFiles = new List<string>();
                    if (_isFolder)
                    {
                        FileHelper.GetSeekPath(_strSelectPath, ref listFiles);
                    }
                    else
                    {
                        listFiles.Add(_strSelectPath);
                    }

                    for (int i = 0; i < listFiles.Count; ++i)
                    {
                        var newSizeInfo = new FileSizeInfo();
                        var pathRelative = listFiles[i].Remove(Application.dataPath);
                        newSizeInfo.asset = AssetDatabase.LoadAssetAtPath(FileHelper.ContactPath("Assets/", pathRelative), typeof(Object));
                        if (newSizeInfo.asset == null)
                            continue;

                        newSizeInfo.path = listFiles[i];
                        newSizeInfo.fileSize = GetFileSize(listFiles[i]);
                        _lTotalSize += newSizeInfo.fileSize;

                        _listFileSize.Add(newSizeInfo);
                    }

                    _listFileSize.Sort((FileSizeInfo x, FileSizeInfo y) =>
                    {
                        if (x.fileSize < y.fileSize)
                            return 1;
                        else if (x.fileSize > y.fileSize)
                            return -1;
                        else
                            return 0;
                    });
                }
            }
            else
            {
                if (_listFileSize.Count == 0)
                    GUILayout.Label("Please select a file or folder in 'Project' window");
            }

            //draw computer files size
            if (_listFileSize.Count > 0)
            {
                EditorGUILayout.HelpBox("Select Total Size: " + FileHelper.GetFileSizeFormatString(_lTotalSize) + "\nFile Count: " + _listFileSize.Count, MessageType.Info);

                for (int i = 0; i < _listFileSize.Count; ++i)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(_listFileSize[i].asset, typeof(Object), true, GUILayout.Width(_currentWindow.position.width / 2));
                    GUILayout.Label("size: " + FileHelper.GetFileSizeFormatString(_listFileSize[i].fileSize), GUILayout.Width(_currentWindow.position.width / 2));
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
        }

        private long GetFileSize(string path)
        {
            var ReadByteTmp = FileHelper.ReadAllByteByUserPath(path);
            return ReadByteTmp.Length;
        }
    }
}
