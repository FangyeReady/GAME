using UnityEngine;
using System.Collections;
using UnityEditor;
using shaco.Base;

namespace shacoEditor
{
    static public class HotUpdateVersionControlDrawerEditor
    {
        static public void DrawCurrentSelectAssets(HotUpdateExportEditor.SelectFile.FileInfo[] selectAsset, int maxShowAssetCount, System.Action<HotUpdateExportEditor.SelectFile.FileInfo[]> callbackNewAssetBundle, System.Action<HotUpdateExportEditor.SelectFile.FileInfo[]> callbackNewAssetBundleDeepAssets)
        {
            // if (!selectAsset.IsNullOrEmpty())
            // {
                // GUILayout.BeginHorizontal();
                //				if (GUILayout.Button("New"))
                //				{
                //					OpenFileDialog(false, false);
                //				}
                // if (GUILayout.Button("NewAssetBundle(Assets)"))
                // {
                //     callbackNewAssetBundle(new Object[0]);
                // }
                // if (GUILayout.Button("NewAssetBundle(Recursive Directory)"))
                // {
                //     callbackNewAssetBundleDeepAssets(new Object[0]);
                // }
                // GUILayout.EndHorizontal();

                //不再显示当前选中对象了
                // int loopCountTmp = selectAsset.Length < maxShowAssetCount ? selectAsset.Length : maxShowAssetCount;
                // for (int i = 0; i < loopCountTmp; ++i)
                // {
                //     EditorGUILayout.ObjectField(selectAsset[i], typeof(Object), true);
                // }

                // if (selectAsset.Length > maxShowAssetCount)
                // {
                //     GUILayout.Label("total " + selectAsset.Length + " count select asset ...");
                // }
            // }
        }

        static public bool DrawLocalConfig(HotUpdateExportEditor target, bool isAutoEncryptOriginalFile)
        {
            //draw keep folder
            if (target.MapAssetbundlePath.Count > 0)
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayoutHelper.DrawList(target.LocalConfigAsset.ListKeepFolder, "Keep Original Folder", (value) =>
                    {
                        var valueObject = value as Object;
                        var pathCheckFolder = EditorHelper.GetAssetPathLower(valueObject);
                        if (!FileHelper.ExistsDirectory(pathCheckFolder) && null != valueObject)
                        {
                            Debug.LogError("please select a folder rather than a asset");
                            return false;
                        }
                        return true;
                    });
                }
                GUILayout.EndVertical();
            }

            //draw ignore meta extensios
            if (target.MapAssetbundlePath.Count > 0)
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayoutHelper.DrawList(target.LocalConfigAsset.ListIgnoreMetaModel, "Ignore File Modal     ", (value) =>
                    {
                        var valueObject = value as Object;
                        var pathAssetTmp = EditorHelper.GetAssetPathLower(valueObject);
                        if (string.IsNullOrEmpty(pathAssetTmp) || !pathAssetTmp.Contains(FileDefine.DOT_SPLIT))
                        {
                            Debug.LogError("only can select a asset in 'Project' window !");
                            return false;
                        }
                        else if (FileHelper.ExistsDirectory(pathAssetTmp))
                        {
                            Debug.LogError("please select a asset rather than a folder");
                            return false;
                        }
                        else
                        {
                            //check duplicate extensions
                            int sameCountTmp = 0;
                            for (int j = 0; j < target.LocalConfigAsset.ListIgnoreMetaModel.Count; ++j)
                            {
                                var pathAssetTmp2 = EditorHelper.GetAssetPathLower(target.LocalConfigAsset.ListIgnoreMetaModel[j]);
                                if (FileHelper.GetFilNameExtension(pathAssetTmp2) == FileHelper.GetFilNameExtension(pathAssetTmp))
                                {
                                    ++sameCountTmp;
                                    if (sameCountTmp > 1)
                                        break;
                                }
                            }
                            if (sameCountTmp > 1)
                            {
                                Debug.LogError("has same extensions=" + pathAssetTmp);
                                return false;
                            }
                        }
                        return true;
                    });
                }
                GUILayout.EndVertical();
            }

            return isAutoEncryptOriginalFile;
        }
    }
}
