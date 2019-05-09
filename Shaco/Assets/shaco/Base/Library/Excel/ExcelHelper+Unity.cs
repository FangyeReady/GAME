using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Base
{
    public partial class ExcelHelper
    {
        /// <summary>
        /// 从本地热更新下载目录打开excel，仅支持excel导出的txt或者csv格式
        /// <param name="path">excel路径</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return>表字典，用于读写excel数据</return>
        /// </summary>
        static public ExcelData OpenResourcesOrLocal(string path, string multiVersionControlRelativePath = "")
        {
            ExcelData retValue = null;
            var readTmp = shaco.ResourcesEx.LoadResourcesOrLocal(path, multiVersionControlRelativePath);
            if (!readTmp.IsNull())
            {
                var readString = readTmp.ToString();
                if (!string.IsNullOrEmpty(readString))
                {
                    retValue = new ExcelData();
                    retValue.excelPath = path;
                    retValue.InitWithString(readString);
                }
            }
            return retValue;
        }

        /// <summary>
        /// 从Resources目录或者本地下载目录异步打开excel表，仅支持excel导出的txt或者csv格式
        /// <param name="path">excel路径</param>
        /// <param name="callbackEnd">打开excel完毕回调</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return>表字典，用于读写excel数据</return>
        /// </summary>
        static public void OpenResourcesOrLocalAsync(string path, System.Action<ExcelData> callbackEnd, string multiVersionControlRelativePath = "")
        {
            if (null == callbackEnd)
            {
                Log.Error("ExcelHelper OpenResourcesOrLocalAsync error: callback function is invalid");
                return;
            }

            ExcelData retValue = null;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                retValue = OpenResourcesOrLocal(path, multiVersionControlRelativePath);
            }, () =>
            {
                callbackEnd(retValue);
            });
        }
    }
}