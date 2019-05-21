
using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco.Base
{
    public partial class ExcelHelper
    {
        private Dictionary<string, IExcelData> _excelDatas = new Dictionary<string, IExcelData>();

		/// <summary>
		/// 从文件路径打开excel表
		/// <param name="path">excel路径</param>
		/// <return>表字典，用于读写excel数据</return>
		/// </summary>
        static public ExcelData OpenWithFile(string path)
		{
			var retValue = new ExcelData();
            retValue.excelPath = path;
            retValue.Init(path);
			return retValue;
		}

        /// <summary>
        /// 从文件路径异步 打开excel表
        /// <param name="path">excel路径</param>
        /// <param name="callbackEnd">打开excel完毕回调</param>
        /// <return>表字典，用于读写excel数据</return>
        /// </summary>
        static public void OpenWithFileAsync(string path, System.Action<ExcelData> callbackEnd)
        {
            if (null == callbackEnd)
            {
                Log.Error("ExcelHelper OpenWithFileAsync error: callback function is invalid");
                return;
            }

            ExcelData retValue = null;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                retValue = OpenWithFile(path);
            }, () =>
            {
                callbackEnd(retValue);
            });
        }

        /// <summary>
        /// 获取excel数据管理类，用于获取excel数据
        /// <param name="T">数据类类型</param>
        /// <return>excel数据类</return>
        /// </summary>
        static public T GetExcelData<T>() where T : IExcelData, new()
        {
            var instanceTmp = GameEntry.GetInstance<ExcelHelper>();
            string key = typeof(T).FullName.ToString();
            T retValue = default(T);
            IExcelData findValue = null;

            if (instanceTmp._excelDatas.TryGetValue(key, out findValue))
            {
                retValue = (T)findValue;
            }
            else 
            {
                retValue = new T();
                instanceTmp._excelDatas.Add(key, retValue);
            }
           
            return retValue;
        }

        /// <summary>
        /// 销毁一个excel数据类
        /// <param name="T">数据类类型</param>
        /// </summary>
        static public void UnloadExcelData<T>()
        {
            var instanceTmp = GameEntry.GetInstance<ExcelHelper>();
            string key = typeof(T).FullName.ToString();

            if (!instanceTmp._excelDatas.ContainsKey(key))
            {
                Log.Error("ExcelHelper UnloadExcelData error: not found data, key=" + key);
            }
            else 
            {
                instanceTmp._excelDatas.Remove(key);
            }
        }

        /// <summary>
        /// 清理所有excel数据类，释放内存
        /// </summary>
        static public void ClearExcelData()
        {
            var instanceTmp = GameEntry.GetInstance<ExcelHelper>();
            instanceTmp._excelDatas.Clear();

            System.GC.Collect();
        }
    }
}

