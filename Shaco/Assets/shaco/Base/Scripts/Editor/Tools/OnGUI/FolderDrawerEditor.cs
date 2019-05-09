using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;

namespace shacoEditor
{
    public partial class FolderDrawerEditor
    {
        public enum FileType
        {
            UNKNOWN,
            ROOT,
            FOLDER,
            FILE,
        }

        public class FileInfo
        {
            public bool isOpen = true;
            public bool IsIgnoreFolder = false;
            public string name = string.Empty;
            public Object displayFile = null;
            public FileInfo parent = null;
            public List<FileInfo> child = new List<FileInfo>();
            public FileType fileType = FileType.UNKNOWN;
            public Rect RectDraw = new Rect();

            public FileInfo() { }
            public FileInfo(FileType type) { this.fileType = type; }
        }

        private class CustomContextMenu
        {
            public string description = string.Empty;
            public System.Action<string, List<string>> callbackClick = null;
        }

        //是否允许重复路径
        public bool AllowedDuplicatePath = false;
        //是否允许无效的文件存在，无效的文件会默认显示为字符串
        public bool AllowedInvalidAsset = false;

        //文件被改变时
        public System.Action<string, FileInfo> OnChangedCallBack = null;
        //文件被删除时
        public System.Action<List<FileInfo>> OnDeleteCallBack = null;
        //文件被添加时
        public System.Action<List<FileInfo>> OnAddFileCallBack = null;
        //文件夹需要被添加时(该类不会主动添加文件夹，请通过AddFile来自动添加文件夹)
        public System.Func<List<string>, bool> OnNeedAddFolderCallBack = null;

        //文件夹图标与右边名字的间隔距离
        private float _fIconAndLabelDistance = 13;

        //每个节点大小
        private Vector2 _vec2FileNodeSize = new Vector2(15, 15);

        //文件目录根节点
        private FileInfo _rootFile = new FileInfo(FileType.ROOT);

        //绘制窗口矩形
        private Rect _windowsRect = new Rect(0, 0, 0, 0);

        //当前选中文件(左窗口)
        private List<FileInfo> _currentSelectFileInfo = new List<FileInfo>();

        //当前选中文件或文件夹中包含的所有文件(右窗口)
        private List<FileInfo> _currentSelectAllFiles = new List<FileInfo>();
        //当前搜索到的所有文件夹
        private List<FileInfo> _currentSearchAllFolders = new List<FileInfo>();
        //当前绘制顺序列表
        private List<FileInfo> _currentDrawList = new List<FileInfo>();

        //当前显示的节点数量
        private int _iShowFileCount = 0;

        private Color _colorSelect = new Color(62.0f / 255, 95.0f / 255, 150.0f / 255);
        private Vector2 _vec2ScrollView = Vector2.zero;
        private Vector2 _vec2ScrollViewFiles = Vector2.zero;
        private float _fScrollBarWidth = 0; //16
        //被忽略的文件夹路径标记，被标记的文件夹不作为读取资源路径计算
        private List<string> _listIgnoreFolderTag = new List<string>();
        private string _searchName = string.Empty;
        //用于绘制分割线的对象
        private GUIHelper.WindowSplitter _dragLineSeparator = new GUIHelper.WindowSplitter();
        //是否开启绘制列表记录
        private bool _isOpenDrawListRecord = false;
        //按住shift和键盘多选时候的方向: -1上 1下 0无方向
        private int _iShitMuiltySelectDirection = 0;
        private bool _isDragAndDelete = false;
        private bool _isFirstDragMove = true;
        //右键菜单自定义栏
        private Dictionary<string, CustomContextMenu> _customContextMenus = new Dictionary<string, CustomContextMenu>();
        private Rect leftWindowRect;
        private Rect rightWindowRect;
        //右侧窗口最大显示对象数量
        private readonly int MAX_SHOW_ITEM_COUNT = 30;
        //当前选中文件数量
        private int _currentSelectFilesCount = 0;

        public void DrawFolder(Rect rectWindow)
        {
            GUILayout.BeginArea(rectWindow);
            {
                rectWindow.x = rectWindow.y = 0;

                DrawCurrentSelectFolderSign();

                float maxHeight = _iShowFileCount * _vec2FileNodeSize.y;
                float remainHeight = rectWindow.height - rectWindow.y;
                _iShowFileCount = 0;

                //刷新左右窗口绘制区域
                if (rectWindow != _windowsRect)
                {
                    _dragLineSeparator.SetSplitWindow(rectWindow, 0.5f, 0.5f);
                    _windowsRect = rectWindow;
                }

                leftWindowRect = _dragLineSeparator.GetSplitWindowRect(0);
                rightWindowRect = _dragLineSeparator.GetSplitWindowRect(1);

                DrawSearchField(leftWindowRect);

                UpdateEvent();

                //draw left window
                var rectPosition = new Rect(leftWindowRect.x, leftWindowRect.y, leftWindowRect.width, remainHeight);
                var rectView = new Rect(leftWindowRect.x, leftWindowRect.y, rectPosition.width - _fScrollBarWidth, maxHeight);
                _vec2ScrollView = GUI.BeginScrollView(rectPosition, _vec2ScrollView, rectView);
                {
                    //Fixed the selection of objects from jumping up and down when using the mouse wheel without need scroll bars
                    if (rectPosition.width >= rectView.width && rectPosition.height >= rectView.height)
                    {
                        _vec2ScrollView = Vector2.zero;
                    }
                    if (string.IsNullOrEmpty(_searchName))
                    {
                        DrawFolder();
                    }
                    else
                    {
                        DrawSearchFolder();
                    }
                }
                GUI.EndScrollView();

                //draw separator line
                _dragLineSeparator.ForceUpdateWindowSize(rectWindow);
                _dragLineSeparator.Draw();

                //draw right window
                if (_isDragAndDelete)
                {
                    DrawDeleteDragAndDrop();
                }
                else
                {
                    rectPosition = new Rect(rightWindowRect.x, rightWindowRect.y, rightWindowRect.width + _fScrollBarWidth, remainHeight);
                    rectView = new Rect(rightWindowRect.x, rightWindowRect.y, rectPosition.width - _fScrollBarWidth, Mathf.Min(_currentSelectAllFiles.Count, MAX_SHOW_ITEM_COUNT) * _vec2FileNodeSize.y);
                    _vec2ScrollViewFiles = GUI.BeginScrollView(rectPosition, _vec2ScrollViewFiles, rectView);
                    {
                        DrawCurrentSelectFiles(_currentSelectAllFiles);
                    }
                    GUI.EndScrollView();
                }
            }
            GUILayout.EndArea();
        }

        public void AddCustomContextMenu(string description, System.Action<string, List<string>> callbackClick)
        {
            if (_customContextMenus.ContainsKey(description))
            {
                return;
            }
            var newItem = new CustomContextMenu();
            newItem.description = description;
            newItem.callbackClick = callbackClick;
            _customContextMenus.Add(description, newItem);
        }

        private void DrawFolder()
        {
            int drawCount = 0;
            for (int i = 0; i < _rootFile.child.Count; ++i)
            {
                var value = _rootFile.child[i];
                DrawFolder(value, 0, ref drawCount);
            }
        }

        private void DrawFolder(FileInfo fileInfo, int level, ref int prevDrawCount)
        {
            //draw me
            ++_iShowFileCount;
            Vector3 vec3PosButtonOpen = new Vector3(level * _vec2FileNodeSize.x + leftWindowRect.x, prevDrawCount * _vec2FileNodeSize.y + leftWindowRect.y, 0);

            Rect rectButtonOpen = new Rect(vec3PosButtonOpen.x, vec3PosButtonOpen.y, _vec2FileNodeSize.x, _vec2FileNodeSize.y);

            if (HasTypeInChildren(fileInfo, FileType.FOLDER))
            {
                var pivotPoint = new Vector2(rectButtonOpen.width / 2 + rectButtonOpen.x, rectButtonOpen.height / 2 + rectButtonOpen.y);
                EditorGUIUtility.RotateAroundPivot(fileInfo.isOpen ? 90 : 0, pivotPoint);
                if (fileInfo.fileType == FileType.FOLDER && GUI.Button(rectButtonOpen, string.Empty, EditorStyles.foldout))
                {
                    ChangeIsOpen(fileInfo, true);
                }
                EditorGUIUtility.RotateAroundPivot(fileInfo.isOpen ? -90 : 0, pivotPoint);
            }

            Vector3 vec3PosButtonFolder = vec3PosButtonOpen + new Vector3(_vec2FileNodeSize.x, 0, 0);
            Rect rectButtonFolder = new Rect(vec3PosButtonFolder.x, vec3PosButtonFolder.y, leftWindowRect.width - vec3PosButtonFolder.x, _vec2FileNodeSize.y);
            fileInfo.RectDraw = rectButtonFolder;

            if (fileInfo.fileType == FileType.FOLDER)
            {
                var rectPosButtonTmp = rectButtonFolder;
                rectPosButtonTmp.width = _vec2FileNodeSize.x;

                GUI.DrawTexture(rectPosButtonTmp, EditorGUIUtility.FindTexture(fileInfo.IsIgnoreFolder ? "PrefabNormal Icon" : "Folder Icon"));

                Vector3 vec3PosLabelName = vec3PosButtonFolder + new Vector3(_fIconAndLabelDistance, 0, 0);
                Rect rectLabelName = new Rect(vec3PosLabelName.x, vec3PosLabelName.y, leftWindowRect.width - vec3PosLabelName.x, _vec2FileNodeSize.y);
                GUI.Label(rectLabelName, fileInfo.name);
            }
            else if (fileInfo.fileType == FileType.FILE)
            {
                // Vector3 vec3PosFile = vec3PosButtonOpen;

                // if (fileInfo.file != null)
                // {
                //     Rect rectFile = new Rect(vec3PosFile.x, vec3PosFile.y, leftWindowRect.width - vec3PosFile.x, _vec2FileNodeSize.y);
                //     var oldFile = fileInfo.file;
                //     fileInfo.file = EditorGUI.ObjectField(rectFile, fileInfo.file, typeof(Object), true);

                //     if (oldFile != null && fileInfo.file == null)
                //     {
                //         fileInfo.file = oldFile;
                //         InvokeCallBack(OnDeleteCallBack, new List<FileInfo>() { fileInfo });
                //         fileInfo.file = null;

                //         ClearCurrentTmpList();
                //         RemoveFile(fileInfo);
                //     }

                //     if (oldFile != fileInfo.file && fileInfo.file != null)
                //     {
                //         fileInfo.name = FileHelper.GetLastFileName(EditorHelper.GetAssetPathLower(fileInfo.file), true);
                //         InvokeCallBack(OnChangedCallBack, oldFile, fileInfo);
                //     }
                // }
                // else
                // {
                //     Rect rectLabelName = new Rect(vec3PosButtonOpen.x, vec3PosButtonOpen.y, leftWindowRect.width - vec3PosFile.x, _vec2FileNodeSize.y);
                //     fileInfo.RectDraw = rectLabelName;
                //     GUI.Label(rectLabelName, fileInfo.name);
                // }
            }
            else
            {
                Debug.LogError("DrawFolder erorr: unsupport file type=" + fileInfo.fileType);
            }

            ++prevDrawCount;

            if (_isOpenDrawListRecord)
            {
                _currentDrawList.Add(fileInfo);
            }

            //draw child
            if (fileInfo.isOpen)
            {
                for (int i = 0; i < fileInfo.child.Count; ++i)
                {
                    var value = fileInfo.child[i];

                    if (value.fileType == FileType.FOLDER)
                    {
                        DrawFolder(value, level + 1, ref prevDrawCount);
                    }
                }
            }
        }

        private void DrawDeleteDragAndDrop()
        {
            var oldColor = GUI.color;

            if (Event.current != null && rightWindowRect.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.red;
            }
            else
            {
                GUI.color = Color.grey;
            }
            GUI.Box(rightWindowRect, string.Empty);
            GUI.color = oldColor;

            var oldSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 20;
            GUI.Label(new Rect(rightWindowRect.x, rightWindowRect.y + rightWindowRect.height / 2, rightWindowRect.width, rightWindowRect.height), "Drop here to delete");
            GUI.skin.label.fontSize = oldSize;

            if (Event.current != null)
            {
                var iconTmp = EditorGUIUtility.FindTexture("PrefabNormal Icon"/*"DefaultAsset Icon"*/);
                GUI.DrawTexture(new Rect(Event.current.mousePosition.x - iconTmp.width / 2, Event.current.mousePosition.y - iconTmp.height / 2, iconTmp.width, iconTmp.height), iconTmp);
            }
        }

        private void DrawSearchFolder()
        {
            var rectTmp = leftWindowRect;
            rectTmp.y += _vec2FileNodeSize.y;
            DrawDefaultFolderAndFiles(_currentSearchAllFolders, rectTmp, -1);
        }

        private List<FileInfo> GetAllChildren(List<FileInfo> fileInfos, FileType fileType, string searchName)
        {
            List<FileInfo> retValue = new List<FileInfo>();

            for (int i = 0; i < fileInfos.Count; ++i)
            {
                ForeachChidren(fileInfos[i], (FileInfo fileInfo) =>
                {

                    if (fileInfo.fileType == fileType || fileType == FileType.UNKNOWN)
                    {
                        if (string.IsNullOrEmpty(searchName) || (fileInfo.name.ToLower().Contains(searchName.ToLower())))
                        {
                            if (!retValue.Contains(fileInfo))
                            {
                                retValue.Add(fileInfo);
                            }
                        }
                    }

                });
            }

            return retValue;
        }

        private void DrawCurrentSelectFiles(List<FileInfo> allFiles)
        {
            DrawDefaultFolderAndFiles(allFiles, rightWindowRect, MAX_SHOW_ITEM_COUNT);
        }

        private void DrawDefaultFolderAndFiles(List<FileInfo> allFiles, Rect rectWindow, int maxShowItemsCount)
        {
            int drawIndexTmp = 0;
            var rectFirstItem = new Rect(rectWindow.x, rectWindow.y, rectWindow.width, _vec2FileNodeSize.y);

            int currentDrawFileCount = 0;//_currentSelectFilesCount

            if (_currentSelectFilesCount > 0)
            {
                GUI.Label(new Rect(rectFirstItem.x, rectFirstItem.y, rectFirstItem.width, 16), "Total(" + _currentSelectFilesCount + ")Show(" + Mathf.Min(_currentSelectFilesCount, maxShowItemsCount) + ")");
                rectFirstItem.y += 16;
            }

            for (int i = 0; i < allFiles.Count; ++i)
            {
                var fileTmp = allFiles[i];

                Rect rectFileTmp = rectFirstItem;
                rectFileTmp.y += (drawIndexTmp++) * _vec2FileNodeSize.y;

                if (fileTmp.fileType == FileType.FOLDER)
                {
                    var rectFolderIcon = new Rect(rectFileTmp.x, rectFileTmp.y, _vec2FileNodeSize.x, _vec2FileNodeSize.y);
                    GUI.DrawTexture(rectFolderIcon, EditorGUIUtility.FindTexture(fileTmp.IsIgnoreFolder ? "PrefabNormal Icon" : "Folder Icon"));
                    GUI.Label(new Rect(rectFileTmp.x + _vec2FileNodeSize.x, rectFileTmp.y, rectFileTmp.width - _vec2FileNodeSize.x, rectFileTmp.height), fileTmp.name);
                    fileTmp.RectDraw = rectFileTmp;
                }
                else if (fileTmp.fileType == FileType.FILE)
                {
                    //为了提高性能，只显示在数量控制范围内的组建
                    if (currentDrawFileCount++ < maxShowItemsCount)
                    {
                        //第一次显示检查资源是否需要加载
                        if (fileTmp.displayFile == null)
                        {
                            fileTmp.displayFile = AssetDatabase.LoadAssetAtPath(GetFullPath(fileTmp), typeof(Object));
                        }
                        if (fileTmp.displayFile != null)
                        {
                            var oldFile = fileTmp.displayFile;
                            fileTmp.displayFile = EditorGUI.ObjectField(rectFileTmp, fileTmp.displayFile, typeof(Object), true);

                            //删除
                            if (oldFile != null && fileTmp.displayFile == null)
                            {
                                fileTmp.displayFile = oldFile;
                                InvokeCallBack(OnDeleteCallBack, new List<FileInfo>() { fileTmp });
                                fileTmp.displayFile = null;

                                ClearCurrentTmpList();
                                RemoveFile(fileTmp);
                            }

                            //修改
                            if (oldFile != fileTmp.displayFile && fileTmp.displayFile != null)
                            {
                                var oldName = fileTmp.name;
                                fileTmp.name = FileHelper.GetLastFileName(EditorHelper.GetAssetPathLower(fileTmp.displayFile), true);
                                InvokeCallBack(OnChangedCallBack, oldName, fileTmp);
                            }
                        }
                        else
                        {
                            fileTmp.RectDraw = rectFileTmp;
                            GUI.Label(rectFileTmp, fileTmp.name);
                        }
                    }
                }
            }

            _currentSelectFilesCount = currentDrawFileCount;
        }

        private void UpdateEvent()
        {
            var eventTmp = Event.current;
            if (eventTmp == null || _dragLineSeparator.isDragSplitter)
                return;

            switch (eventTmp.type)
            {
                case EventType.MouseDown:
                    {
                        if (eventTmp.clickCount == 2 && string.IsNullOrEmpty(_searchName))
                        {
                            ExcuteSelectEvent(eventTmp);
                            if (_currentSelectFileInfo.Count > 0)
                            {
                                for (int i = 0; i < _currentSelectFileInfo.Count; ++i)
                                {
                                    ChangeIsOpen(_currentSelectFileInfo[i], true);
                                }
                            }
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (_isDragAndDelete)
                        {
                            if (rightWindowRect.Contains(eventTmp.mousePosition))
                            {
                                ExcuteDelete();
                            }
                            else
                            {
                                _isDragAndDelete = false;
                            }
                        }
                        else
                        {
                            //如果开始了拖拽，不允许通过鼠标抬起获取当前选中
                            if (_isFirstDragMove)
                            {
                                ExcuteSelectEvent(eventTmp);
                            }
                            if (eventTmp.button == 1 && leftWindowRect.Contains(eventTmp.mousePosition))
                            {
                                ShowContextMenu();
                            }
                        }
                        _isFirstDragMove = true;
                        break;
                    }
                case EventType.KeyDown:
                    {
                        //键盘上下移动选中光标
                        if (_currentSelectFileInfo.Count > 0)
                        {
                            if (eventTmp.keyCode == KeyCode.UpArrow)
                            {
                                SelectCurrentFileInfo(GetPrevOrNextFileInfo(_currentSelectFileInfo[_currentSelectFileInfo.Count - 1], false), !eventTmp.shift);
                            }
                            else if (eventTmp.keyCode == KeyCode.DownArrow)
                            {
                                SelectCurrentFileInfo(GetPrevOrNextFileInfo(_currentSelectFileInfo[_currentSelectFileInfo.Count - 1], true), !eventTmp.shift);
                            }
                        }

                        //键盘左右方向键展开或者收起文件夹
                        if (_currentSelectFileInfo.Count == 1)
                        {
                            if (eventTmp.keyCode == KeyCode.LeftArrow || eventTmp.keyCode == KeyCode.RightArrow)
                            {
                                ChangeIsOpen(_currentSelectFileInfo[0], true);
                            }
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (eventTmp.keyCode == KeyCode.Delete && string.IsNullOrEmpty(_searchName))
                        {
                            ExcuteDelete();
                        }
                        break;
                    }
                case EventType.DragUpdated:
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        if (!_isDragAndDelete)
                        {
                            if (_isFirstDragMove)
                            {
                                _isFirstDragMove = false;

                                var tmpNotTouchRect = rightWindowRect;
                                tmpNotTouchRect.x -= 10;
                                tmpNotTouchRect.width += 10;
                                if (_currentSelectAllFiles.Count > 0 && !tmpNotTouchRect.Contains(eventTmp.mousePosition))
                                {
                                    if (ExcuteSelectEvent(eventTmp))
                                    {
                                        _isDragAndDelete = true;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case EventType.DragPerform:
                    {
                        OnDragPerformEvent(eventTmp);
                        break;
                    }
                case EventType.Used:
                    {
                        break;
                    }
                default: break;
            }
        }

        private void OnDragPerformEvent(Event curEvent)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                if (_windowsRect.Contains(curEvent.mousePosition))
                {
                    Selection.objects = DragAndDrop.objectReferences;
                    ExcuteAddFile();
                }
            }
            _isDragAndDelete = false;
        }

        private void ShowContextMenu()
        {
            GenericMenu menuTmp = new GenericMenu();

            menuTmp.AddItem(new GUIContent("Delete"), false, (object context) =>
            {
                ExcuteDelete();
            }, null);

            List<string> currentSelectAllFilesTmp = null;
            if (_customContextMenus.Count > 0 && _currentSelectFileInfo.Count == 1)
            {
                currentSelectAllFilesTmp = new List<string>();
                foreach (var iter in _currentSelectAllFiles)
                {
                    currentSelectAllFilesTmp.Add(iter.name);
                }

                foreach (var iter in _customContextMenus)
                {
                    menuTmp.AddItem(new GUIContent(iter.Value.description), false, (object context) =>
                    {
                        var fullPath = GetFullPath(_currentSelectFileInfo[0]);
                        iter.Value.callbackClick(fullPath, currentSelectAllFilesTmp);
                    }, null);
                }
            }

            menuTmp.ShowAsContext();
        }

        private void SelectCurrentFileInfo(FileInfo current, bool onlyOne = true)
        {
            if (null == current)
            {
                return;
            }

            if (onlyOne)
            {
                _currentSelectFileInfo.Clear();
                _iShitMuiltySelectDirection = 0;
            }

            if (_currentSelectFileInfo.Contains(current))
            {
                _currentSelectFileInfo.Remove(current);
                if (_currentSelectFileInfo.Count == 1)
                {
                    _iShitMuiltySelectDirection = 0;
                }
            }
            else
            {
                _currentSelectFileInfo.Add(current);
            }

            _currentSelectAllFiles = GetAllChildren(_currentSelectFileInfo, FileType.FILE, string.Empty);
            _vec2ScrollViewFiles = Vector2.zero;
        }

        private FileInfo GetPrevOrNextFileInfo(FileInfo current, bool isNext)
        {
            FileInfo retValue = null;

            //绘制一次界面，重新排序
            _currentDrawList.Clear();
            _isOpenDrawListRecord = true;
            DrawFolder();
            _isOpenDrawListRecord = false;

            int findIndex = _currentDrawList.IndexOf(current);
            findIndex += isNext ? 1 : -1;
            if (findIndex >= 0 && findIndex < _currentDrawList.Count)
            {
                retValue = _currentDrawList[findIndex];
            }

            _currentDrawList.Clear();

            //确认是否需要需要返回反选的对象
            int directionTmp = isNext ? 1 : -1;
            if (_currentSelectFileInfo.Count >= 2 && _iShitMuiltySelectDirection != directionTmp)
            {
                retValue = _currentSelectFileInfo[_currentSelectFileInfo.Count - 1];
            }

            if (_iShitMuiltySelectDirection == 0)
            {
                _iShitMuiltySelectDirection = directionTmp;
            }

            return retValue;
        }

        private bool ExcuteSelectEvent(Event curEvent)
        {
            var realMousePosition = curEvent.mousePosition + _vec2ScrollView;
            FileInfo findInfo = null;

            //在右侧面板点击的时候，不响应选择物件事件
            if (rightWindowRect.Contains(curEvent.mousePosition))
            {
                return false;
            }
            else
            {
                findInfo = SelectFileInfo(realMousePosition);
            }

            //取消控件输入焦点
            GUI.FocusControl(string.Empty);

            if (findInfo != null)
            {
                if (EditorHelper.IsPressedControlButton())
                {
                    if (_currentSelectFileInfo.Contains(findInfo))
                    {
                        _currentSelectFileInfo.Remove(findInfo);
                    }
                    SelectCurrentFileInfo(findInfo, false);
                }
                else
                {
                    SelectCurrentFileInfo(findInfo, true);
                }

                //如果当前有选中对象，并且是鼠标左键按下的时候，高亮显示
                if (_currentSelectFileInfo.Count > 0 && curEvent.button == 0)
                {
                    for (int i = 0; i < _currentSelectFileInfo.Count; ++i)
                    {
                        var loadAssetTmp = AssetDatabase.LoadAssetAtPath(GetFullPath(_currentSelectFileInfo[i]), typeof(Object));
                        if (null != loadAssetTmp)
                        {
                            EditorGUIUtility.PingObject(loadAssetTmp);
                        }
                        else
                        {
                            Debug.LogWarning("FolderDrawerEditor ExcuteSelectEvent warning: can't ping object, name=" + _currentSelectFileInfo[i].name);
                        }
                    }
                }
            }
            else
            {
                if (curEvent.button == 0)
                {
                    ClearCurrentTmpList();
                }
            }

            return _currentSelectFileInfo.Count > 0;
        }

        private void ExcuteDelete()
        {
            if (_currentSelectFileInfo.Count == 0)
                return;

            _isDragAndDelete = false;

            InvokeCallBack(OnDeleteCallBack, _currentSelectFileInfo);

            for (int j = 0; j < _currentSelectFileInfo.Count; ++j)
            {
                var fileInfoTmp = _currentSelectFileInfo[j];
                RemoveFile(fileInfoTmp);
            }

            ClearCurrentTmpList();
        }

        private void ExcuteAddFile()
        {
            var listAddAssetTmp = DragAndDrop.objectReferences;

            if (listAddAssetTmp.IsNullOrEmpty())
            {
                return;
            }

            var listAddPathTmp = new List<string>();
            for (int i = 0; i < listAddAssetTmp.Length; ++i)
            {
                listAddPathTmp.Add(AssetDatabase.GetAssetPath(listAddAssetTmp[i]));
            }

            //如果有外部方法截获了add file操作，则立即跳出
            if (InvokeCallBack(OnNeedAddFolderCallBack, listAddPathTmp))
            {
                return;
            }

            List<FileInfo> listCurrentAdd = new List<FileInfo>();

            for (int i = 0; i < listAddPathTmp.Count; ++i)
            {
                var path = listAddPathTmp[i].ToLower();

                for (int j = 0; j < _currentSelectFileInfo.Count; ++j)
                {
                    var fileInfoTmp = _currentSelectFileInfo[j];
                    if (fileInfoTmp.fileType == FileType.FOLDER)
                    {
                        var newFile = AddFile(fileInfoTmp, path);
                        listCurrentAdd.Add(newFile);
                    }
                }
            }

            InvokeCallBack(OnAddFileCallBack, listCurrentAdd);
            ClearCurrentTmpList();
        }

        private FileInfo SelectFileInfo(Vector2 mousePos)
        {
            FileInfo retValue = null;
            if (!string.IsNullOrEmpty(_searchName))
            {
                for (int i = 0; i < _currentSearchAllFolders.Count; ++i)
                {
                    retValue = SelectFileInfo(mousePos, _currentSearchAllFolders[i], false);
                    if (retValue != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                retValue = SelectFileInfo(mousePos, _rootFile, true);
            }

            return retValue;
        }

        private FileInfo SelectFileInfo(Vector2 mousePos, FileInfo fileInfo, bool isDeep)
        {
            FileInfo ret = null;
            if (fileInfo.RectDraw.Contains(mousePos))
            {
                ret = fileInfo;
            }
            else if (fileInfo.isOpen && isDeep)
            {
                for (int i = 0; i < fileInfo.child.Count; ++i)
                {
                    ret = SelectFileInfo(mousePos, fileInfo.child[i], isDeep);
                    if (ret != null)
                        break;
                }
            }

            return ret;
        }

        private void DrawSearchField(Rect rectWindow)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(rectWindow.width / 3 * 2);
                var oldSearchNameTmp = _searchName;
                _searchName = GUILayoutHelper.SearchField(_searchName, GUILayout.Width(rectWindow.width / 3 * 1));

                if (oldSearchNameTmp != _searchName)
                {
                    if (string.IsNullOrEmpty(_searchName))
                    {
                        if (_currentSelectFileInfo.Count == 1)
                        {
                            LocationTarget(_currentSelectFileInfo[0]);
                            UpdateSort(_currentSelectFileInfo[0].parent);
                        }
                    }
                    else
                    {
                        ClearCurrentTmpList();
                        _currentSearchAllFolders = GetAllChildren(new List<FileInfo>() { _rootFile }, FileType.FOLDER, _searchName);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void LocationTarget(FileInfo fileInfo)
        {
            ChangeIsOpenInverseForeachParent(fileInfo, true);

            //绘制一次界面，然后重新排序
            DrawFolder();

            SelectCurrentFileInfo(fileInfo);

            _vec2ScrollView.y = fileInfo.RectDraw.y;
        }

        private void DrawCurrentSelectFolderSign()
        {
            if (_currentSelectFileInfo == null)
                return;

            var colorOld = GUI.color;
            GUI.color = _colorSelect;
            for (int j = 0; j < _currentSelectFileInfo.Count; ++j)
            {
                var fileInfoTmp = _currentSelectFileInfo[j];
                if (IsOpenNodeByDeep(fileInfoTmp) || !string.IsNullOrEmpty(_searchName))
                {
                    var drawRectTmp = new Rect(fileInfoTmp.RectDraw.x,
                        fileInfoTmp.RectDraw.y - _vec2ScrollView.y,
                        fileInfoTmp.RectDraw.width,
                        fileInfoTmp.RectDraw.height);
                    GUI.DrawTexture(drawRectTmp, Texture2D.whiteTexture);
                }
            }

            GUI.color = colorOld;
        }

        private bool IsOpenNodeByDeep(FileInfo fileInfo)
        {
            bool ret = true;
            var parent = fileInfo.parent;
            if (parent != null)
            {
                do
                {
                    ret = parent.isOpen;
                    if (!ret)
                        break;
                    else
                        parent = parent.parent;
                } while (parent != null);
            }

            return ret;
        }

        private void ChangeIsOpen(FileInfo fileInfo, bool changeValue)
        {
            if (changeValue)
            {
                fileInfo.isOpen = !fileInfo.isOpen;

                if (fileInfo.isOpen)
                {
                    UpdateSort(fileInfo);
                }

                SaveSetting(fileInfo);
            }
            if (_rootFile.child.Count == 1 && _rootFile.child[0] == fileInfo && !fileInfo.isOpen)
            {
                ChangeIsOpenForeachChildren(fileInfo, false);
            }
        }

        private void ChangeIsOpenForeachChildren(FileInfo fileInfo, bool isOpen)
        {
            ForeachChidren(fileInfo, (FileInfo fileInfoChild) =>
            {
                fileInfoChild.isOpen = isOpen;
            });
        }

        private void ChangeIsOpenInverseForeachParent(FileInfo fileInfo, bool isOpen)
        {
            if (null == fileInfo || fileInfo == _rootFile)
            {
                return;
            }

            if (fileInfo.fileType == FileType.FOLDER)
            {
                fileInfo.isOpen = isOpen;
            }

            ChangeIsOpenInverseForeachParent(fileInfo.parent, isOpen);
        }

        private void ClearCurrentTmpList()
        {
            for (int i = _currentSelectAllFiles.Count - 1; i >= 0; --i)
            {
                _currentSelectAllFiles[i].displayFile = null;
            }
            _currentSelectFileInfo.Clear();
            _currentSearchAllFolders.Clear();
            _currentSelectAllFiles.Clear();

            _currentSelectFilesCount = 0;
            _iShitMuiltySelectDirection = 0;
        }
    }
}
