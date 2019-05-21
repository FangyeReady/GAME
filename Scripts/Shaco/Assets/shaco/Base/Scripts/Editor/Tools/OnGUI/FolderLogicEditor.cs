using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;
using System.Linq;

namespace shacoEditor
{
    public partial class FolderDrawerEditor
    {
        public IComparer<FileInfo> sortCompareFuntion = null;

        public FileInfo AddFile(string path)
        {
            return Find(path, true, null);
        }

        public FileInfo AddFile(string path, string assetFilePath)
        {
            return Find(path, true, assetFilePath);
        }

        public FileInfo AddFile(FileInfo parent, string path)
        {
            if (parent.fileType != FileType.FOLDER)
            {
                Debug.LogError("add file error: not a folder type=" + parent.fileType);
                return null;
            }
            else
            {
                if (DragAndDrop.objectReferences.IsNullOrEmpty() && !parent.IsIgnoreFolder && !AllowedInvalidAsset)
                {
                    Debug.LogError("add file error: you cannot add a file in a folder that does not allow invalid asset and has not been ignored !");
                    return null;
                }
            }

            var pathFolder = GetFullPath(parent);
            var filename = FileHelper.GetLastFileName(path, true);
            var pathConvert = FileHelper.ContactPath(pathFolder, filename);
            return AddFile(pathConvert);
        }

        public void RemoveFile(string path)
        {
            FileInfo findInfo = Find(path);
            if (findInfo == null)
            {
                // Debug.LogError("remove file error: not find file by path=" + path);
            }
            else if (findInfo.fileType == FileType.ROOT)
            {
                Debug.LogError("remove file error: can't remove root path=" + path);
            }
            else
            {
                if (findInfo.parent == null)
                    Debug.LogError("remove file is error: parent is null path=" + path);
                else
                {
                    RemoveFileBase(findInfo);
                }
            }
        }

        public void RemoveFile(FileInfo fileInfo)
        {
            if (fileInfo.parent != null)
            {
                RemoveFileBase(fileInfo);
            }
            else
                Debug.LogError("remove file is error: parent is null name=" + fileInfo.name);
        }

        public FileInfo Find(string path)
        {
            return Find(path, false, null);
        }

        public void ClearFile()
        {
            _rootFile.child.Clear();
            _currentSelectFileInfo.Clear();
            _currentDrawList.Clear();
            _currentSearchAllFolders.Clear();
            _currentSelectAllFiles.Clear();
        }

        public string GetAssetPathLower(FileInfo fileInfo)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(fileInfo.name))
            {
                Debug.LogError("getAssetPath error: file is null !");
                return ret;
            }
            ret = fileInfo.name.ToLower();
            return ret;
        }

        public string GetFullPath(FileInfo fileInfo)
        {
            string ret = string.Empty;

            if (fileInfo == null)
                return ret;

            //如果是文件，本身就是全称路径
            if (fileInfo.fileType == FileType.FILE)
            {
                return fileInfo.name;
            }

            var parent = fileInfo;
            var listTmp = new List<string>();
            do
            {
                if (!string.IsNullOrEmpty(parent.name))
                {
                    listTmp.Add(parent.name);
                }
                parent = parent.parent;

            } while (parent != null);

            if (listTmp.Count <= 1)
            {
                return fileInfo.name;
            }
            else
            {
                //开始的路径必须是Assets否则会找不到正确的资源
                ret += "assets";
                ret += FileDefine.PATH_FLAG_SPLIT;

                //如果父节点文件夹名字带有后缀名，则会被过滤掉
                bool haveExtensionsFolder = shaco.Base.FileHelper.HasFileNameExtension(fileInfo.parent.name);

                for (int i = listTmp.Count - 2; i >= 0; --i)
                {
                    //如果上层文件夹包含后缀名，则特殊处理
                    if (haveExtensionsFolder)
                    {
                        if (null != fileInfo.parent && listTmp[i] == fileInfo.parent.name)
                        {
                            continue;
                        }
                    }
                    ret += listTmp[i];
                    if (i != 0)
                        ret += FileDefine.PATH_FLAG_SPLIT;
                }

                var fullPath = EditorHelper.GetFullPath(ret);
                if (!shaco.Base.FileHelper.ExistsDirectory(fullPath) && !shaco.Base.FileHelper.ExistsFile(fullPath))
                {
                    if (fileInfo.child.Count == 1)
                    {
                        ret = fileInfo.child[0].name;
                    }
                    else
                    {
                        ret = shaco.Base.FileHelper.RemoveExtension(ret);
                    }

                    //如果文件或者文件夹都找不到，可能是用户自定义的目录，则取它第一个子节点的路径
                    if (fileInfo.fileType == FileType.FOLDER && fileInfo.child.Count > 0)
                    {
                        if (!shaco.Base.FileHelper.ExistsDirectory(ret) && !shaco.Base.FileHelper.ExistsFile(ret))
                        {
                            ret = GetFullPath(fileInfo.child[0]);
                        }
                    }
                }
            }

            return ret;
        }

        public void AddIgnoreFolderTag(string tag)
        {
            if (!_listIgnoreFolderTag.Contains(tag))
                _listIgnoreFolderTag.Add(tag);
        }

        public void RemoveIgnoreFolderTag(string tag)
        {
            _listIgnoreFolderTag.Remove(tag);
        }

        public bool IsIgnoreFolder(string path)
        {
            return IndexIgnoreFolder(path) >= 0;
        }

        public int IndexIgnoreFolder(string path)
        {
            int ignoreListIndex = 0;
            return IndexIgnoreFolder(path, ref ignoreListIndex);
        }

        public int IndexIgnoreFolder(string path, ref int ignoreListIndex)
        {
            int ret = -1;
            ignoreListIndex = -1;
            for (int i = 0; i < _listIgnoreFolderTag.Count; ++i)
            {
                ret = path.LastIndexOf(_listIgnoreFolderTag[i]);
                if (ret >= 0)
                {
                    ignoreListIndex = i;
                    break;
                }
            }
            return ret;
        }

        public void SaveSetting(FileInfo fileInfo)
        {
            var fullPath = GetFullPath(fileInfo);
            shaco.DataSave.Instance.Write("FolderDrawerEditor_isOpen_" + fullPath, fileInfo.isOpen);
        }

        public void OpenAllFolder()
        {
            ChangeIsOpenForeachChildren(_rootFile, true);
        }

        public void CloseAllFolder()
        {
            ChangeIsOpenForeachChildren(_rootFile, false);
        }

        public void UpdateSort(FileInfo fileInfo)
        {
            if (null == fileInfo || !fileInfo.isOpen || fileInfo.fileType != FileType.FOLDER)
                return;

            if (null != sortCompareFuntion)
            {
                fileInfo.child.Sort(sortCompareFuntion);
            }
            else
            {
                fileInfo.child.Sort((FileInfo x, FileInfo y) =>
                {
                    char[] arr1 = x.name.ToCharArray();
                    char[] arr2 = y.name.ToCharArray();
                    int i = 0, j = 0;
                    while (i < arr1.Length && j < arr2.Length)
                    {
                        if (char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
                        {
                            string s1 = string.Empty, s2 = string.Empty;
                            while (i < arr1.Length && char.IsDigit(arr1[i]))
                            {
                                s1 += arr1[i];
                                ++i;
                            }
                            while (j < arr2.Length && char.IsDigit(arr2[j]))
                            {
                                s2 += arr2[j];
                                ++j;
                            }
                            if (int.Parse(s1) > int.Parse(s2))
                            {
                                return 1;
                            }
                            if (int.Parse(s1) < int.Parse(s2))
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            if (arr1[i] > arr2[j])
                            {
                                return 1;
                            }
                            if (arr1[i] < arr2[j])
                            {
                                return -1;
                            }
                            ++i;
                            ++j;
                        }
                    }
                    if (arr1.Length == arr2.Length)
                    {
                        return 0;
                    }
                    else
                    {
                        return arr1.Length > arr2.Length ? 1 : -1;
                    }
                });

            }
        }

        private void ForeachChidren(FileInfo root, System.Action<FileInfo> callfunc)
        {
            var child = root.child;
            for (int i = 0; i < child.Count; ++i)
            {
                callfunc(child[i]);
                ForeachChidren(child[i], callfunc);
            }
        }

        private void CheckLoadSettings(FileInfo fileInfo)
        {
            var fullPath = GetFullPath(fileInfo);
            fileInfo.isOpen = shaco.DataSave.Instance.ReadBool("FolderDrawerEditor_isOpen_" + fullPath);
        }

        private void RemoveFileBase(FileInfo fileInfo)
        {
            fileInfo.parent.child.Remove(fileInfo);
            UpdateSort(fileInfo.parent);
            fileInfo.parent = null;
        }

        private int GetChildCountDeep(FileInfo fileInfo)
        {
            int count = 1;
            GetChildCountDeep(fileInfo, ref count);
            return count;
        }

        private void GetChildCountDeep(FileInfo fileInfo, ref int count)
        {
            for (int i = 0; i < fileInfo.child.Count; ++i)
            {
                ++count;
                GetChildCountDeep(fileInfo.child[i], ref count);
            }
        }

        private FileInfo Find(string path, bool isAutoCreate, string assetFilePath)
        {
            FileInfo ret = null;
            var listSplitName = path.Split(FileDefine.PATH_FLAG_SPLIT.ToChar());

            FileInfo findChild = _rootFile;

            for (int i = 0; i < listSplitName.Length; ++i)
            {
                bool isLastName = (i == listSplitName.Length - 1);
                var findIndexTmp = FindFile(findChild.child, listSplitName[i]);

                if (isAutoCreate)
                {
                    if (findIndexTmp < 0)
                    {
                        FileInfo newFileInfo = new FileInfo();
                        newFileInfo.fileType = (isLastName ? FileType.FILE : FileType.FOLDER);
                        newFileInfo.name = newFileInfo.fileType == FileType.FOLDER ? listSplitName[i] : path;
                        newFileInfo.parent = findChild;
                        newFileInfo.IsIgnoreFolder = IsIgnoreFolder(listSplitName[i]);

                        if (newFileInfo.fileType == FileType.FOLDER)
                        {
                            newFileInfo.name = listSplitName[i];
                        }
                        else
                        {
                            newFileInfo.name = string.IsNullOrEmpty(assetFilePath) ? path : assetFilePath;
                        }

                        CheckLoadSettings(newFileInfo);

                        // switch (newFileInfo.fileType)
                        // {
                        //     case FileType.FOLDER:
                        //         {
                        //             var folderPath = string.Empty;
                        //             for (int j = 0; j <= i; ++j)
                        //             {
                        //                 folderPath = folderPath.ContactPath(listSplitName[j]);
                        //             }
                        //             if (shaco.Base.FileHelper.HasFileNameExtension(folderPath))
                        //             {
                        //                 folderPath = shaco.Base.FileHelper.RemoveExtension(folderPath);
                        //             }
                        //             newFileInfo.file = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));
                        //             if (null == newFileInfo.file)
                        //             {
                        //                 newFileInfo.file = LoadFileAsset(path, asset);
                        //             }
                        //             break;
                        //         }
                        //     case FileType.FILE:
                        //         {
                        //             newFileInfo.file = LoadFileAsset(path, asset);
                        //             break;
                        //         }
                        //     default: Log.Error("FolderLogicEditor Find error: unsupport file type=" + newFileInfo.fileType); break;
                        // }

                        findChild.child.Add(newFileInfo);
                        findChild = newFileInfo;

                        if (null != findChild.parent && findChild.parent.isOpen)
                        {
                            UpdateSort(findChild.parent);
                        }
                    }
                }
                else
                {
                    if (findIndexTmp < 0)
                    {
                        if (!IsIgnoreFolder(path))
                            Debug.LogWarning("not find file info by path=" + path);
                        break;
                    }
                }

                if (findIndexTmp >= 0)
                    findChild = findChild.child[findIndexTmp];

                if (isLastName)
                {
                    ret = findChild;
                }
            }

            if (ret == null && isAutoCreate)
            {
                Debug.LogError("find file info with auto create error: path=" + path);
            }

            return ret;
        }

        private Object LoadFileAsset(string path, Object defaultAsset)
        {
            Object retValue = null;

            if (defaultAsset == null)
            {
                var pathAsset = GetAssetRealPath(path, false);
                retValue = AssetDatabase.LoadAssetAtPath(pathAsset, typeof(Object));

                if (retValue == null)
                {
                    pathAsset = GetAssetRealPath(path, true);
                    retValue = AssetDatabase.LoadAssetAtPath(pathAsset, typeof(Object));
                }

                if (!AllowedInvalidAsset)
                {
                    if (retValue == null)
                    {
                        Debug.LogError("create new file error: is a invalid asset path=" + pathAsset);
                        return retValue;
                    }
                }
            }
            else
            {
                retValue = defaultAsset;
            }
            return retValue;
        }

        private string GetAssetRealPath(string path, bool onlyRemoveExtension)
        {
            string ret = path;

            if (onlyRemoveExtension)
            {
                for (int i = 0; i < _listIgnoreFolderTag.Count; ++i)
                {
                    ret = ret.Remove(_listIgnoreFolderTag[i]);
                }
            }
            else
            {
                for (int i = 0; i < _listIgnoreFolderTag.Count; ++i)
                {
                    ret = FileHelper.RemoveSubStringByFind(ret, _listIgnoreFolderTag[i]);
                }
            }

            return ret;
        }

        private int FindFile(List<FileInfo> listFile, string name)
        {
            int ret = -1;

            if (AllowedDuplicatePath)
            {
                for (int i = 0; i < listFile.Count; ++i)
                {
                    if (listFile[i].name == name && listFile[i].fileType == FileType.FOLDER)
                    {
                        ret = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < listFile.Count; ++i)
                {
                    var fileInfoTmp = listFile[i];
                    bool isBreak = false;
                    if (fileInfoTmp.fileType == FileType.FOLDER)
                    {
                        if (fileInfoTmp.name == name)
                        {
                            isBreak = true;
                        }
                    }
                    else
                    {
                        if (fileInfoTmp.name == name)
                        {
                            isBreak = true;
                        }
                    }
                    if (isBreak)
                    {
                        ret = i;
                        break;
                    }
                }
            }

            return ret;
        }

        private void InvokeCallBack(System.Action<List<FileInfo>> callback, List<FileInfo> fileInfo)
        {
            if (callback != null)
            {
                callback(fileInfo);
            }
        }

        private bool InvokeCallBack(System.Func<List<string>, bool> callback, List<string> listAsset)
        {
            if (callback != null)
            {
                return callback(listAsset);
            }
            return false;
        }

        private void InvokeCallBack(System.Action<string, FileInfo> callback, string oldAsset, FileInfo newFile)
        {
            if (callback != null)
            {
                callback(oldAsset, newFile);
            }
        }

        private bool HasTypeInChildren(FileInfo fileInfo, FileType type)
        {
            bool ret = false;
            for (int i = 0; i < fileInfo.child.Count; ++i)
            {
                if (fileInfo.child[i].fileType == type)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
    }
}
