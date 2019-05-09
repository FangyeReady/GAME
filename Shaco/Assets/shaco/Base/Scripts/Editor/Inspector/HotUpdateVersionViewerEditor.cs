using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;

namespace shacoEditor
{
    public class HotUpdateVersionViewerEditor
    {
        private HotUpdateExportEditor _windowExport = null;
        private FolderDrawerEditor _folderDrawer = new FolderDrawerEditor();

        public void Init(HotUpdateExportEditor target)
        {
            _windowExport = target;

            _folderDrawer.OnAddFileCallBack = OnAddCallBack;
            _folderDrawer.OnChangedCallBack = OnChangedCallBack;
            _folderDrawer.OnDeleteCallBack = OnDeleteCallBack;
            _folderDrawer.OnNeedAddFolderCallBack = OnNeedAddFolderCallBack;
            _folderDrawer.AddIgnoreFolderTag(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
        }

        public void ClearDrawFolder()
        {
            _folderDrawer.ClearFile();
        }

        public void UpdateDrawFolder()
        {
            if (null != _windowExport)
            {
                var removeKeys = new List<string>();
                foreach (var iter in _windowExport.MapAssetbundlePath)
                {
                    if (!UpdateDrawFolder(iter.Key, iter.Value, false))
                    {
                        removeKeys.Add(iter.Key);
                    }
                }

                for (int i = removeKeys.Count - 1; i >= 0; --i)
                {
                    _windowExport.MapAssetbundlePath.Remove(removeKeys[i]);
                }
            }
        }

        public bool AddFile(string assetbundleName, HotUpdateExportEditor.SelectFile.FileInfo asset)
        {
            var pathTmp = asset.Asset.ToLower();
            var pathFolder = assetbundleName;
            var filename = FileHelper.GetLastFileName(pathTmp, true);
            var pathConvert = FileHelper.ContactPath(pathFolder, filename);

            var fullPathCheck = shacoEditor.EditorHelper.GetFullPath(pathTmp);
            if (!shaco.Base.FileHelper.ExistsFile(fullPathCheck))
            {
                Debug.LogError("HotUpdateVersionViewerEditor AddFile error: not found path=" + pathTmp);
                return false;
            }
            else 
            {
                var fileInfo = _folderDrawer.AddFile(pathConvert, asset.Asset);
                _folderDrawer.UpdateSort(fileInfo.parent);
                return true;
            }
        }

        public bool UpdateDrawFolder(string assetbundleName, HotUpdateExportEditor.SelectFile selectInfo, bool forceUpdateAll)
        {
            if (selectInfo.ListAsset.Count == 0)
                return false;

            string pathFolder = string.Empty;
            foreach (var value1 in selectInfo.ListAsset.Values)
            {
                pathFolder = assetbundleName;
                break;
            }

            if (forceUpdateAll)
                _folderDrawer.RemoveFile(pathFolder);

            var removeKeys = new List<string>();
            foreach (var iter in selectInfo.ListAsset)
            {
                if (!this.AddFile(assetbundleName, iter.Value))
                {
                    removeKeys.Add(iter.Key);
                    _windowExport._mapAllExportAsset.Remove(iter.Value.Asset);
                }
            }

            for (int i = removeKeys.Count - 1; i >= 0; --i)
            {
                selectInfo.ListAsset.Remove(removeKeys[i]);
            }

            return selectInfo.ListAsset.Count > 0;
        }

        public void DrawInspector(Rect rect)
        {
            _folderDrawer.DrawFolder(rect);
        }

        private string GetPathFolderByAsset(string assetbundleName, HotUpdateExportEditor.SelectFile.FileInfo asset)
        {
            var pathTmp = asset.Asset.ToLower();

            var pathSplit1 = assetbundleName.Split(FileDefine.PATH_FLAG_SPLIT.ToChar());
            var pathSplit2 = pathTmp.Split(FileDefine.PATH_FLAG_SPLIT.ToChar());
            var pathSplitReal = new List<string>();

            int index1 = 0;
            int index2 = 0;
            for (; index1 < pathSplit1.Length && index2 < pathSplit2.Length; ++index1, ++index2)
            {
                if (pathSplit1[index1] == pathSplit2[index2])
                {
                    pathSplitReal.Add(pathSplit2[index2]);
                }
                else
                {
                    ++index2;
                }
            }

            if (pathSplitReal.Count == 0)
            {
                Debug.LogError("can't get path folder by asset bundle name=" + assetbundleName + " asset=" + asset);
                return pathTmp;
            }

            pathTmp = string.Empty;
            for (int i = 0; i < pathSplitReal.Count; ++i)
            {
                pathTmp += pathSplitReal[i] + FileDefine.PATH_FLAG_SPLIT;
            }
            pathTmp = pathTmp.Remove(pathTmp.Length - FileDefine.PATH_FLAG_SPLIT.Length);

            var pathFolderTmp = FileHelper.GetFolderNameByPath(pathTmp);
            var pathConvert = FileHelper.ContactPath(pathFolderTmp, FileHelper.GetLastFileName(assetbundleName, true));
            return pathConvert;
        }

        // private void RemoveFile(string assetbundleName, List<Object> listAsset)
        // {
        //     for (int i = 0; i < listAsset.Count; ++i)
        //     {
        //         var pathFolder = GetPathFolderByAsset(assetbundleName, listAsset[i]);
        //         var filename = FileHelper.GetLastFileName(EditorHelper.GetAssetPathLower(listAsset[i]), true);
        //         var pathConvert = FileHelper.ContactPath(pathFolder, filename);

        //         _folderDrawer.RemoveFile(pathConvert);
        //     }
        // }

        private void OnAddCallBack(List<FolderDrawerEditor.FileInfo> fileInfo)
        {
            if (fileInfo.Count == 0)
                return;

            var assetbundleName = string.Empty;

            var findValue = FindAssetBundleSelectInfo(fileInfo[0], _windowExport.MapAssetbundlePath, ref assetbundleName);
            if (findValue == null)
                return;

            var selectAssets = new HotUpdateExportEditor.SelectFile.FileInfo[fileInfo.Count];
            for (int i = 0; i < fileInfo.Count; ++i)
            {
                selectAssets[i] = new HotUpdateExportEditor.SelectFile.FileInfo(fileInfo[i].name);
            }

            List<HotUpdateExportEditor.SelectFile.FileInfo> listSameAsset = new List<HotUpdateExportEditor.SelectFile.FileInfo>();
            _windowExport.SelectCurrentAssetBundleFiles(assetbundleName, findValue, selectAssets, ref listSameAsset);

            UpdateDrawFolder(assetbundleName, findValue, true);
        }

        private void OnChangedCallBack(string oldAsset, FolderDrawerEditor.FileInfo fileInfo)
        {
            var pathAssetTmp = _folderDrawer.GetAssetPathLower(fileInfo);
            var assetbundleName = string.Empty;

            var findValue = FindAssetBundleSelectInfo(fileInfo, _windowExport.MapAssetbundlePath, ref assetbundleName);

            //check changed
            if (findValue != null && !string.IsNullOrEmpty(fileInfo.name))
            {
                if (findValue.ListAsset.ContainsKey(pathAssetTmp))
                {
                    Debug.LogWarning("has same item=" + pathAssetTmp + " in assetbundle, can't changed asset");
                }
                else
                {
                    if (_windowExport._mapAllExportAsset.ContainsKey(pathAssetTmp))
                    {
                        Debug.LogWarning("has same item=" + pathAssetTmp + " in export asset map, can't changed asset");

                    }
                    else
                    {
                        _windowExport._mapAllExportAsset[fileInfo.name] = pathAssetTmp;

                        _windowExport._mapAllExportAsset.Remove(oldAsset);
                        if (!_windowExport._mapAllExportAsset.ContainsKey(fileInfo.name))
                            _windowExport._mapAllExportAsset.Add(fileInfo.name, null);

                        findValue.ListAsset.Remove(oldAsset.ToLower());
                        findValue.ListAsset.Add(pathAssetTmp, new HotUpdateExportEditor.SelectFile.FileInfo(pathAssetTmp));
                    }
                }
            }
            else
            {
                Debug.LogError("changed asset error: findValue=" + findValue + " fileInfo.file=" + fileInfo.name);
            }

            UpdateDrawFolder(assetbundleName, findValue, true);
        }

        private void OnDeleteCallBack(List<FolderDrawerEditor.FileInfo> listFileInfo)
        {
            for (int i = 0; i < listFileInfo.Count; ++i)
            {
                OnDeleteCallBack(listFileInfo[i]);
            }
        }

        private void OnDeleteCallBack(FolderDrawerEditor.FileInfo fileInfo)
        {
            var pathAssetTmp = fileInfo.name.ToLower();
            var assetbundleName = string.Empty;
            var findValue = FindAssetBundleSelectInfo(fileInfo, _windowExport.MapAssetbundlePath, ref assetbundleName);

            if (findValue == null && fileInfo.fileType != FolderDrawerEditor.FileType.FOLDER)
            {
                findValue = FindAssetBundleSelectInfo(fileInfo.parent, _windowExport.MapAssetbundlePath, ref assetbundleName);
            }

            if (findValue == null)
            {
                if (fileInfo.fileType != FolderDrawerEditor.FileType.FOLDER)
                {
                    Debug.LogError("delete folder error: not a folder type! name=" + fileInfo.name);
                }
                else
                {
                    for (int i = 0; i < fileInfo.child.Count; ++i)
                    {
                        OnDeleteCallBack(fileInfo.child[i]);
                    }
                }
            }
            else
            {
                _windowExport._mapAllExportAsset.Remove(_folderDrawer.GetFullPath(fileInfo));

                if (fileInfo.fileType == FolderDrawerEditor.FileType.FOLDER)
                {
                    _windowExport.CheckAssetBundleValid(new List<string>() { assetbundleName });
                    findValue.ListAsset.Clear();
                }
                else
                {
                    if (findValue.ListAsset.Count == 1)
                        _windowExport.CheckAssetBundleValid(new List<string>() { assetbundleName });
                    findValue.ListAsset.Remove(pathAssetTmp);
                }

                if (findValue.ListAsset.Count == 0)
                {
                    if (fileInfo.fileType == FolderDrawerEditor.FileType.FILE)
                        _folderDrawer.RemoveFile(fileInfo.parent);
                }
                UpdateDrawFolder(assetbundleName, findValue, true);
            }
        }

        private bool OnNeedAddFolderCallBack(List<string> listAsset)
        {
            bool hasDirectory = false;
            for (int i = 0; i < listAsset.Count; ++i)
            {
                if (shaco.Base.FileHelper.ExistsDirectory(listAsset[i]))
                {
                    hasDirectory = true;
                    break;
                }
            }

            var listConvertAssetTmp = new HotUpdateExportEditor.SelectFile.FileInfo[listAsset.Count];

            for (int i = 0; i < listAsset.Count; ++i)
            {
                listConvertAssetTmp[i] = new HotUpdateExportEditor.SelectFile.FileInfo(listAsset[i]);
            }

            //如果包含文件夹，需要用户确认遍历文件夹还是直接对文件夹打包
            if (hasDirectory)
            {
                var selectedObjectsPath = new System.Text.StringBuilder();
                if (null != DragAndDrop.objectReferences)
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                    {
                        selectedObjectsPath.Append("Path: ");
                        selectedObjectsPath.Append(AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[i]));
                        selectedObjectsPath.Append("\n");
                    }
                }

                int select = EditorUtility.DisplayDialogComplex("New AssetBundle", selectedObjectsPath.ToString(), "One", "Cancel", "Multiple");
                if (select == 0)
                {
                    _windowExport.NewAssetBundle(listConvertAssetTmp);
                }
                else if (select == 2)
                {
                    _windowExport.NewAssetBundleDeepAssets(listConvertAssetTmp);
                }
                else
                {
                    //user cancel
                    return false;
                }
            }
            //如果都是文件则统一1个文件打成1个assetbundle
            else
            {
                _windowExport.NewAssetBundle(listConvertAssetTmp);
            }
            return true;
        }

        private HotUpdateExportEditor.SelectFile FindAssetBundleSelectInfo(
            FolderDrawerEditor.FileInfo fileInfo,
            Dictionary<string, HotUpdateExportEditor.SelectFile> mapAssetBundlePath,
            ref string assetbundleName)
        {
            HotUpdateExportEditor.SelectFile ret = null;
            var pathFolder = _folderDrawer.GetFullPath(fileInfo);
            var pathLowerTmp = pathFolder;

            if (fileInfo == null)
                return ret;

            if (fileInfo.fileType != FolderDrawerEditor.FileType.FOLDER)
                pathFolder = FileHelper.GetFolderNameByPath(pathFolder);
            pathLowerTmp = pathFolder;

            if (!string.IsNullOrEmpty(pathLowerTmp) && pathLowerTmp[pathLowerTmp.Length - 1].ToString() == FileDefine.PATH_FLAG_SPLIT)
            {
                pathLowerTmp = pathLowerTmp.Remove(pathLowerTmp.Length - 1);
            }

            assetbundleName = shaco.Base.FileHelper.ReplaceExtension(pathLowerTmp, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);

            if (_windowExport.MapAssetbundlePath.ContainsKey(assetbundleName))
            {
                ret = _windowExport.MapAssetbundlePath[assetbundleName];
            }

            return ret;
        }
    }
}