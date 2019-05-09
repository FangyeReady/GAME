using System.Collections;
using System.Collections.Generic;
using System.IO;
using Excel;
using System.Data;

namespace shaco.Base
{
    public partial class ExcelData
    {
        private enum DataType
        {
            List,
            Dictionary
        }

        public class RowInfo
        {
            //是否忽略掉数据
            public bool isIgnoreData = false;
            //每行数据
            public List<string> values = new List<string>();
        }

        /// <summary>
        /// excel表中每行每列数据<行<列>>
        /// </summary>
        public List<RowInfo> dataList { get; private set; }

        /// <summary>
        /// 打开excel文件数据时候传入的路径
        /// </summary>
        public string excelPath = string.Empty;

        /// <summary>
        /// excel数据类型
        /// </summary>
        private DataType _dataType = DataType.List;

        /// <summary>
        /// excel数据类型为Dictionary的时候的key值所在列下标
        /// </summary>
        private int _dataDictionaryKeyTypeColIndex = -1;

        /// <summary>
        /// 表中数据注释用符号
        /// </summary>
        private readonly string IGNORE_FLAG = "//";

        /// <summary>
        /// 列表类型数据标记，可以更改excel导出的数据类型
        /// </summary>
        private readonly string LIST_KEY = "[List]";

        /// <summary>
        /// 字典类型数据标记，可以更改excel导出的数据类型
        /// </summary>
        private readonly string DICTIONARY_KEY = "[Dictionary]";

        /// <summary>
        /// 初始化excel表字典
        /// <param name="bytes">excel表二进制数据</param>
        /// </summary>
        public void Init(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("ExcelData Init error: path is empty");
                return;
            }

            if (null == dataList)
            {
                dataList = new List<RowInfo>();
            }
            else
            {
                dataList.Clear();
            }

            excelPath = path;

			string extensionsTmp = FileHelper.GetFilNameExtension(path);
			if (extensionsTmp == "xlsx")
			{
                InitWithXlsxOrXls(path, false);
			}
            else if (extensionsTmp == "xls")
            {
                InitWithXlsxOrXls(path, true);
            }
			else 
			{
                InitWithCsv(path);
			}
        }

        /// <summary>
        /// 通过字符串获取excel数据，仅支持excel导出的txt或者csv格式
        /// <param name="value">csv文件读取的字符串内容</param>
        /// </summary>
        public void InitWithString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Log.Error("ExcelData Init error: path is empty");
                return;
            }
            
            if (null == dataList)
            {
                dataList = new List<RowInfo>();
            }
            else
            {
                dataList.Clear();
            }

            InitWithCsvString(value);
        }

        /// <summary>
        /// 将excel以txt制表符的文本形式保存，以方便快速读取和修改
		/// *注意*：仅能导出第一张工作表(sheet)的内容，如果一个excel存在多张工作表，请手动拆分开来使用
        /// <param name="savePath">保存路径</param>
        /// </summary>
        public void SaveAsTxt(string savePath)
        {
			if (dataList.IsNullOrEmpty())
			{
				Log.Error("ExcelData SaveAsTxt error: not init");
				return;
			}

            var writeString = new System.Text.StringBuilder();

            for (int i = 0; i < dataList.Count; ++i)
            {
                int ColCount = dataList[i].values.Count;
                for (int j = 0; j < ColCount; ++j)
                {
                    string valueTmp = dataList[i].values[j].ToString();
                    writeString.Append(valueTmp);

                    if (j < ColCount - 1)
                    {
                        writeString.Append('\t');
                    }
                }
                writeString.Append('\n');
            }

            if (writeString.Length > 0)
            {
                writeString.Remove(writeString.Length - 1, 1);
            }

            if (!FileHelper.HasFileNameExtension(savePath))
            {
                savePath = FileHelper.AddExtensions(savePath, "txt");
            }
			FileHelper.WriteAllByUserPath(savePath, writeString.ToString());
        }

		/// <summary>
		/// 通过.xlsx或者.xls方式初始化excel数据
		/// <param name="path">路径</param>
		/// <return></return>
		/// </summary>
		private void InitWithXlsxOrXls(string path, bool isXls)
		{
			FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);

            try
            {
                IExcelDataReader excelReader = null;
                if (isXls)
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else 
                {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                
                if (!string.IsNullOrEmpty(excelReader.ExceptionMessage))
                {
                    Log.Error("ExcelData Init exception: " + excelReader.ExceptionMessage);
                    return;
                }

                var dataSet = excelReader.AsDataSet();

                if (dataSet.Tables.Count > 1)
                {
                	Log.Warning("ExcelData InitWithXlsx warning: contains more than one table, but only the first table can be read");
                }

                int columns = dataSet.Tables[0].Columns.Count;
                int rows = dataSet.Tables[0].Rows.Count;

                for (int i = 0; i < rows; ++i)
                {
                    //拷贝数据到数组
                    string[] lineStrings = new string[columns];
                    for (int j = 0; j < columns; ++j)
                    {
                        lineStrings[j] = dataSet.Tables[0].Rows[i][j].ToString();
                    }

                    //添加一行excel数据
                    AddLineData(lineStrings.ToArrayList(), columns);
				}
            }
            catch (System.Exception e)
            {
                Log.Error("ExcelData Init e: " + e);
            }
            finally
            {
                stream.Close();
            }
		}

		/// <summary>
		/// 从csv中初始化excel数据
		/// <param name="path">路径</param>
		/// </summary>
		private void InitWithCsv(string path)
		{
			var readString = FileHelper.ReadAllByUserPath(path);
			if (string.IsNullOrEmpty(readString))
			{
				Log.Info("ExcelData InitWithCsv error: missing path=" + path);
				return;
			}

            InitWithCsvString(readString);
		}

        /// <summary>
        /// 从csv中初始化excel数据
        /// <param name="value">csv字符串数据</param>
        /// </summary>
        private void InitWithCsvString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Log.Info("ExcelData InitWithCsv error: bytes is empty");
                return;
            }

            var splitRowStrings = value.Split('\n');

            if (splitRowStrings.IsNullOrEmpty())
            {
                Log.Info("ExcelData InitWithCsv error: no data");
                return;
            }

            //每行数据数量，以第一行数据数量为准
            int dataCountInRow = splitRowStrings[0].Split('\t').Length;

            for (int i = 0; i < splitRowStrings.Length; ++i)
            {
                var splitColStrings = splitRowStrings[i].Split('\t');
                AddLineData(splitColStrings.ToArrayList(), dataCountInRow);
            }
        }

        /// <summary>
        /// 添加一行数据
        /// <param name="lineReadDatas">读取到的一行excel的数据</param>
        /// <param name="currentRowIndex">当前所在行</param>
        /// <param name="dataCountInRow">默认一行的excel数据量</param>
        /// <param name="defaultValue">默认填充的数据</param>
        /// </summary>
        private void AddLineData(List<string> lineReadDatas, int dataCountInRow, string defaultValue = "0")
        {
            RowInfo rowInfo = null;

            //过滤一行前面的空白
            int indexSkipFront = -1;
            for (int i = 0; i < lineReadDatas.Count; ++i)
            {
                if (string.IsNullOrEmpty(lineReadDatas[i]))
                {
                    indexSkipFront = i;
                }
                else 
                    break;
            }
            if (indexSkipFront >= 0)
            {
                lineReadDatas.RemoveRange(0, indexSkipFront + 1);
                dataCountInRow -= (indexSkipFront + 1);
            }

            //如果当前行没有数据，则忽略该行其他设置了
            if (lineReadDatas.Count > 0)
            {
                dataList.Add(new RowInfo());
                rowInfo = dataList[dataList.Count - 1];
            }

            if (null == rowInfo)
            {
                return;
            }

            //如果第一列就存在//符号则视为注释，在读取数据的时候会自动过滤掉该行
            rowInfo.isIgnoreData = lineReadDatas[0].StartsWith(IGNORE_FLAG);

            //如果不是第一列出现//符号，但是在后面列出现了//符号，则过滤后面的列，自动设置为defaultValue，但是也会被当做数据读取
            if (!rowInfo.isIgnoreData)
            {
                int indexSkipBehind = -1;
                for (int i = 1; i < lineReadDatas.Count; ++i)
                {
                    if (lineReadDatas[i].StartsWith(IGNORE_FLAG))
                    {
                        indexSkipBehind = i;
                        break;
                    }
                }
                if (indexSkipBehind >= 0)
                {
                    for (int i = indexSkipBehind; i < lineReadDatas.Count; ++i)
                    {
                        lineReadDatas[i] = defaultValue;
                    }
                }
            }
            
            //添加筛选完毕后的数据到缓存中
            for (int j = 0; j < lineReadDatas.Count; ++j)
            {
                var valueTmp = lineReadDatas[j];
                rowInfo.values.Add(valueTmp);
            }

            //当前行数据不足，使用默认数据补齐
            for (int j = lineReadDatas.Count; j < dataCountInRow; ++j)
            {
                Log.Error("ExcelData InitWithCsv error: Row:" + (dataList.Count - 1) + " Colmn:" + j + " missing data, use default data instead of");
                rowInfo.values.Add(defaultValue);
            }

            var lastItem = rowInfo.values[rowInfo.values.Count - 1];
            if (lastItem.Contains("\r"))
                rowInfo.values[rowInfo.values.Count - 1] = lastItem.RemoveBehind("\r");
        }
    }
}