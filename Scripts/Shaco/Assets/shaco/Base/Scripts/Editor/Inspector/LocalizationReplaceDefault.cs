using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class LocalizationReplaceDefault : ILocalizationReplace
    {
        /// <summary>
        /// 获取所有语言文本
        /// <param name="importPath">导入路径</param>
        /// <param name="callbackCollectInfo">加载完毕后语言文本收集信息</param>
        /// <param name="collectExtensions">需要收集的文件后缀名</param>
        /// </summary>
        public void GetAllLanguageString(string importPath, System.Action<List<shaco.Base.Utility.LocalizationCollectnfo>> callbackCollectInfo, params string[] collectExtensions)
        {
            if (string.IsNullOrEmpty(importPath) || null == callbackCollectInfo)
            {
                Debug.LogError("LocalizationReplaceInspector CollectAndExportLanguage erorr: invalid param");
                return;
            }

            var allFiles = new List<string>();

            //打开进度条(其实是假的，暂时没法做遍历文件夹的进度)
            EditorUtility.DisplayProgressBar("Seeking all files in directory", "please wait", 0);

            //该路径不允许在子线程访问，所以需要提前获取
            var applicationPathTmp = Application.dataPath;

            shaco.Base.ThreadPool.RunThreadSafeCallBack(() =>
            {
                //遍历目录，获取所有文本文件
                shaco.Base.FileHelper.GetSeekPath(importPath, ref allFiles, false, collectExtensions);

                //过滤shaco框架中的文本内容
                var shacoBaseRelativePath = shaco.Base.GlobalParams.GetShacoFrameworkRootPath().Remove(applicationPathTmp);
                for (int i = allFiles.Count - 1; i >= 0; --i)
                {
                    if (allFiles[i].Contains(shacoBaseRelativePath))
                    {
                        allFiles.RemoveAt(i);
                    }
                }
            }, () =>
            {
                //关闭进度条
                EditorUtility.ClearProgressBar();

                //读取所有文件内容，并准备筛选导出语言包
                bool userCancel = false;
                var retValue = new List<shaco.Base.Utility.LocalizationCollectnfo>();
                shaco.Base.Coroutine.ForeachAsync(allFiles, (object value) =>
                {
                    retValue.Add(new shaco.Base.Utility.LocalizationCollectnfo()
                    {
                        path = value.ToString(),
                        languageString = shaco.Base.FileHelper.ReadAllByUserPath((value.ToString()))
                    });

                    return !userCancel;
                }, (float progress) =>
                {
                    if (userCancel)
                        return;
                    else
                    {
                        EditorUtility.DisplayProgressBar("loading all seeking files", "please wait", progress);
                    }

                    if (progress >= 1.0f)
                    {
                        if (null != callbackCollectInfo)
                        {
                            callbackCollectInfo(retValue);
                        }
                    }
                });
            });
        }

        /// <summary>
        /// 替换语言文本信息
        /// <param name="path">路径</param>
        /// <param name="exportInfo">导出的语言包信息</param>
        /// </summary>
        public void RepalceLanguageString(string path, List<shaco.Base.Utility.LocalizationExportInfo> exportInfos)
        {
            //打开源文件
            var readOriginalString = shaco.Base.FileHelper.ReadAllByUserPath(path);
            if (string.IsNullOrEmpty(readOriginalString))
            {
                Debug.LogError("LocalizationReplaceInspector RepalceLanguageString error: not found path=" + path);
            }
            //替换对应文本
            else
            {
                for (int i = exportInfos.Count - 1; i >= 0; --i)
                {
                    var infoTmp = exportInfos[i];
                    if (readOriginalString.Contains(infoTmp.textOriginal))
                    {
                        readOriginalString = readOriginalString.Replace(infoTmp.textOriginal, infoTmp.textTranslation);
                    }
                    else
                    {
                        Debug.LogWarning("LocalizationReplaceInspector RepalceLanguageString error: not found orginal text=" + infoTmp.textOriginal + "\npath=" + path);
                    }
                }

                //重写源文件
                shaco.Base.FileHelper.WriteAllByUserPath(path, readOriginalString);
            }
        }

        /// <summary>
        /// 绘制编辑器
        /// </summary>
        public void DrawInspector()
        {

        }

        /// <summary>
        /// 是否只在主线程下替换文件
        /// </summary>
        public bool OnlyReplaceInMainThread() { return false; }
    }
}