using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Base
{
    public partial class ExcelData
	{
        /// <summary>
        /// 将excel数据序列化并自动创建对应的c#脚本，并自动读取
        /// <param name="exportCSharpPath">导出的c#脚本路径</param>
        /// </summary>
        public void SerializableAsCSharpScript(string exportCSharpPath)
        {
            if (dataList.IsNullOrEmpty())
            {
                Log.Error("ExcelData SerializableAsCSharpScript error: not init");
                return;
            }

            var writeString = new System.Text.StringBuilder();

            //获取excel导出相对路径
            var fileNameTmp = FileHelper.GetLastFileName(exportCSharpPath);
            if (string.IsNullOrEmpty(fileNameTmp))
            {
                Log.Error("ExcelData SerializableAsCSharpScript error: missing file name");
                return;
            }
            fileNameTmp = FileHelper.RemoveExtension(fileNameTmp);
            var excelRelativePath = FileHelper.GetLastFileName(excelPath);

            //获取数据类型
            var rowTypes = GetTypeRow();
            if (rowTypes.IsNullOrEmpty())
            {
                Log.Error("ExcelData SerializableAsCSharpScript error: can't get data type");
                return;
            }

            //获取参数名字和数据类型(_dataType)
            var rowValueNames = GetValueNameRow();

            //写入using
            writeString.Append("using System.Collections;\n");
            writeString.Append("using System.Collections.Generic;\n\n");

            //开始写入类
            writeString.Append("namespace shaco.ExcelData\n");
            writeString.Append("{\n");
            writeString.Append("\tpublic class " + fileNameTmp + " : shaco.Base.IExcelData\n");
            writeString.Append("\t{\n");
            writeString.Append("\t\tpublic class RowData\n");
            writeString.Append("\t\t{\n");

            for (int j = 0; j < dataList[0].values.Count; ++j)
            {
                //第一行数据为属性名字，如果没有设置第一行为属性名字，则以Item为默认属性名字
                var valueName = GetValueName(j, rowValueNames);

                //写入参数
                writeString.Append("\t\t\tpublic ");

                var typeTmp = rowTypes[j];
                if (typeof(bool) == typeTmp)
                {
                    writeString.Append("bool " + valueName + " = false;");
                }
                else if (typeof(int) == typeTmp)
                {
                    writeString.Append("int " + valueName + " = 0;");
                }
                else if (typeof(float) == typeTmp)
                {
                    writeString.Append("float " + valueName + " = 0.0f;");
                }
                else if (typeof(string) == typeTmp)
                {
                    writeString.Append("string " + valueName + " = string.Empty;");
                }
                else
                {
                    Log.Error("ExcelData SerializableAsCSharpScript error: unsupport type=" + typeTmp);
                }

                writeString.Append("\n");
            }

            //写入数据结构体结束
            writeString.Append("\t\t}\n");

            //写入私有变量 - multiVersionControlRelativePath
            writeString.Append("\t\tprivate string multiVersionControlRelativePath = string.Empty;\n");

            //写入公开变量 - Count
            writeString.Append("\t\tstatic public int Count { get {return shaco.Base.ExcelHelper.GetExcelData<" + fileNameTmp + ">()._rowDatas.Count;} }\n");

            //写入公开变量 - Instance
            writeString.Append("\t\tstatic public " + fileNameTmp + " Instance { get {return shaco.Base.ExcelHelper.GetExcelData<" + fileNameTmp + ">();} }\n");

            //写入公开方法 - SetMultiVersionPath
            writeString.Append("\t\tprivate void SetMultiVersionPath(string multiVersionControlRelativePath)\n");
            writeString.Append("\t\t{\n");
            writeString.Append("\t\t\tif (this.multiVersionControlRelativePath != multiVersionControlRelativePath)\n");
            writeString.Append("\t\t\t{\n");
            writeString.Append("\t\t\t\tthis.multiVersionControlRelativePath = multiVersionControlRelativePath;\n");
            writeString.Append("\t\t\t\tthis.UpdateData(multiVersionControlRelativePath);\n");
            writeString.Append("\t\t\t}\n");
            writeString.Append("\t\t}\n");

            switch (_dataType)
            {
                case DataType.List:
                    {
                        //写入公开方法 - Get(List)
                        writeString.Append("\t\tstatic public RowData Get(int index, string multiVersionControlRelativePath = \"\")\n");
                        writeString.Append("\t\t{\n");
                        writeString.Append("\t\t\tvar data = shaco.Base.ExcelHelper.GetExcelData<" + fileNameTmp + ">();\n");
                        writeString.Append("\t\t\tdata.SetMultiVersionPath(multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\tif (0 == data._rowDatas.Count) data.UpdateData(data.multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\tif (index < 0 || index > data._rowDatas.Count - 1)\n");
                        writeString.Append("\t\t\t{\n");
                        writeString.Append("\t\t\t\tshaco.Log.Error(\"" + excelRelativePath + " Get error: out of range, index=\" + index + \" count=\" + data._rowDatas.Count + \" + muiltyVersion=\" + data.multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\t\treturn null;\n");
                        writeString.Append("\t\t\t}\n");
                        writeString.Append("\t\t\treturn data._rowDatas[index];\n");
                        writeString.Append("\t\t}\n");

                        //写入私有参数 - _rowDatas(List)
                        writeString.Append("\t\tprivate List<RowData> _rowDatas = new List<RowData>();\n");

                        //写入公开方法 - GetEnumerator(List)
                        writeString.Append("\t\tpublic IEnumerator<RowData> GetEnumerator()\n");
                        writeString.Append("\t\t{\n");
                        writeString.Append("\t\t\tif (0 == _rowDatas.Count) UpdateData(multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\tfor (int i = 0; i < _rowDatas.Count; ++i)\n");
                        writeString.Append("\t\t\t{\n");
                        writeString.Append("\t\t\t\tyield return _rowDatas[i];\n");
                        writeString.Append("\t\t\t}\n");
                        writeString.Append("\t\t}\n");
                        break;
                    }
                case DataType.Dictionary:
                    {
                        //获取key值的代码类型字符串
                        if (_dataDictionaryKeyTypeColIndex < 0 || _dataDictionaryKeyTypeColIndex > rowTypes.Length - 1)
                        {
                            Log.Error("ExcelData SerializableAsCSharpScript error: invalid type index=" + _dataDictionaryKeyTypeColIndex + " count=" + rowTypes.Length);
                            return;
                        }
                        var keyValueTypeName = TypeToScriptTypeString(rowTypes[_dataDictionaryKeyTypeColIndex]);

                        //写入公开方法 - Get(Dictionary)
                        writeString.Append("\t\tstatic public RowData Get(" + keyValueTypeName + " key, string multiVersionControlRelativePath = \"\")\n");
                        writeString.Append("\t\t{\n");
                        writeString.Append("\t\t\tvar data = shaco.Base.ExcelHelper.GetExcelData<" + fileNameTmp + ">();\n");
                        writeString.Append("\t\t\tdata.SetMultiVersionPath(multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\tif (0 == data._rowDatas.Count) data.UpdateData(data.multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\tif (!data._rowDatas.ContainsKey(key))\n");
                        writeString.Append("\t\t\t{\n");
                        writeString.Append("\t\t\t\tshaco.Log.Error(\"" + excelRelativePath + " Get error: not found, key=\" + key + \" count=\" + data._rowDatas.Count + \" + muiltyVersion=\" + data.multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\t\treturn null;\n");
                        writeString.Append("\t\t\t}\n");
                        writeString.Append("\t\t\treturn data._rowDatas[key];\n");
                        writeString.Append("\t\t}\n");

                        //写入私有参数 - _rowDatas(Dictionary)
                        writeString.Append("\t\tprivate Dictionary<" + keyValueTypeName + ", RowData> _rowDatas = new Dictionary<" + keyValueTypeName + ", RowData>();\n");

                        //写入公开方法 - GetEnumerator(Dictionary)
                        writeString.Append("\t\tpublic IEnumerator<KeyValuePair<" + keyValueTypeName + ", RowData>> GetEnumerator()\n");
                        writeString.Append("\t\t{\n");
                        writeString.Append("\t\t\tif (0 == _rowDatas.Count) UpdateData(multiVersionControlRelativePath);\n");
                        writeString.Append("\t\t\tforeach (var iter in _rowDatas)\n");
                        writeString.Append("\t\t\t{\n");
                        writeString.Append("\t\t\t\tyield return iter;\n");
                        writeString.Append("\t\t\t}\n");
                        writeString.Append("\t\t}\n");
                        break;
                    }
                default: Log.Error("ExcelData SerializableAsCSharpScript: unsupport data type=" + _dataType); break;
            }

            //写入私有方法 - UpdateData
            writeString.Append("\t\tprivate void UpdateData(string multiVersionControlRelativePath)\n");
            writeString.Append("\t\t{\n");
            writeString.Append("\t\t\tvar excelData = shaco.Base.ExcelHelper.OpenResourcesOrLocal(\"" + excelRelativePath + "\", multiVersionControlRelativePath);\n");
            writeString.Append("\t\t\tif (null != excelData)\n");
            writeString.Append("\t\t\t{\n");
            writeString.Append("\t\t\t\tfor (int i = 1; i < excelData.dataList.Count; ++i)\n");
            writeString.Append("\t\t\t\t{\n");
            writeString.Append("\t\t\t\t\tvar dataTmp = excelData.dataList[i];\n");
            writeString.Append("\t\t\t\t\tif (!dataTmp.isIgnoreData)\n");
            writeString.Append("\t\t\t\t\t{\n");
            writeString.Append("\t\t\t\t\t\tvar newData = new RowData()\n");
            writeString.Append("\t\t\t\t\t\t{\n");
            for (int j = 0; j < dataList[0].values.Count; ++j)
            {
                var valueName = GetValueName(j, rowValueNames);

                writeString.Append("\t\t\t\t\t\t\t");
                writeString.Append(valueName + " = dataTmp.values[" + j + "]");

                var typeTmp = rowTypes[j];
                if (typeof(bool) == typeTmp)
                {
                    writeString.Append(".ToBool(false, false)");
                }
                else if (typeof(int) == typeTmp)
                {
                    writeString.Append(".ToInt(0, false)");
                }
                else if (typeof(float) == typeTmp)
                {
                    writeString.Append(".ToFloat(0, false)");
                }
                else if (typeof(string) == typeTmp)
                {
                    //do nothing
                }
                else
                {
                    Log.Error("ExcelData SerializableAsCSharpScript error: unsupport type=" + typeTmp);
                }
                writeString.Append(",\n");
            }
            writeString.Append("\t\t\t\t\t\t};\n");

            switch (_dataType)
            {
                case DataType.List:
                    {
                        writeString.Append("\t\t\t\t\t\t_rowDatas.Add(newData);\n");
                        break;
                    }
                case DataType.Dictionary:
                    {
                        //获取key值的参数名字字符串
                        if (_dataDictionaryKeyTypeColIndex < 0 || _dataDictionaryKeyTypeColIndex > rowValueNames.Length - 1)
                        {
                            Log.Error("ExcelData SerializableAsCSharpScript error: invalid type index=" + _dataDictionaryKeyTypeColIndex + " count=" + rowValueNames.Length);
                            return;
                        }
                        var keyParameterName = rowValueNames[_dataDictionaryKeyTypeColIndex];
                        writeString.Append("\t\t\t\t\t\tif (_rowDatas.ContainsKey(newData." + keyParameterName + "))\n");
                        writeString.Append("\t\t\t\t\t\t\tshaco.Log.Error(\"" + excelRelativePath + " UpdateData error: has dulicate key=\" + newData." + keyParameterName + ");\n");
                        writeString.Append("\t\t\t\t\t\telse\n");
                        writeString.Append("\t\t\t\t\t\t\t_rowDatas.Add(newData." + keyParameterName + ", newData);\n");
                        break;
                    }
                default: Log.Error("ExcelData SerializableAsCSharpScript: unsupport data type=" + _dataType); break;
            }
            writeString.Append("\t\t\t\t\t}\n");
            writeString.Append("\t\t\t\t}\n");
            writeString.Append("\t\t\t}\n");
            writeString.Append("\t\t}\n");

            //写入结束
            writeString.Append("\t}\n");
            writeString.Append("}");

            //写入文件
            if (!FileHelper.HasFileNameExtension(exportCSharpPath))
            {
                exportCSharpPath = FileHelper.AddExtensions(exportCSharpPath, "cs");
            }

            FileHelper.WriteAllByUserPath(exportCSharpPath, writeString.ToString());
        }

        /// <summary>
        /// 获取每行的数据类型，以第一行有效数据类型为准，如果行与行之间的数据类型不一致，会导致读取出错
        /// <return>每行数据类型</return>
        /// </summary>
        private System.Type[] GetTypeRow()
        {
            System.Type[] retValue = null;

            if (dataList.Count == 0)
            {
                Log.Error("ExcelData GetTypeRow: no data");
                return retValue;
            }

            //忽略注释的行，获取第一行有效数据在第几行
            int typeRowIndexTmp = -1;
            for (int i = 0; i < dataList.Count; ++i)
            {
                if (!dataList[i].isIgnoreData)
                {
                    typeRowIndexTmp = i;
                    break;
                }
            }

            if (typeRowIndexTmp < 0)
            {
                Log.Error("ExcelData GetTypeRow error: not found type index");
                return retValue;
            }

            //获取数据类型
            retValue = new System.Type[dataList[0].values.Count];
            for (int i = 0; i < retValue.Length; ++i)
            {
                var valueTmp = dataList[typeRowIndexTmp].values[i];
                retValue[i] = DataStringToType(valueTmp);
            }

            if (dataList.Count <= 1)
            {
                Log.Error("ExcelData GetTypeRow error: not enough data");
                return retValue;
            }

            return retValue;
        }

        /// <summary>
        /// 通过字符串判断数据类型
        /// <param name="str">数据字符串</param>
        /// <return>数据类型</return>
        /// </summary>
        private System.Type DataStringToType(string str)
        {
            System.Type retValue = typeof(string);

            if (str == "true" || str == "false")
            {
                retValue = typeof(bool);
            }
            else if (shaco.Base.Utility.IsNumber(str))
            {
                if (str.Contains("."))
                    retValue = typeof(float);
                else
                    retValue = typeof(int);
            }
            else
            {
                retValue = typeof(string);
            }
            return retValue;
        }

        /// <summary>
        /// 类型转换类型代码字符串，并非Type.FullName
        /// <param name="type">类型</param>
        /// <return>类型代码字符串</return>
        /// </summary>
        private string TypeToScriptTypeString(System.Type type)
        {
            if (type == typeof(bool)) return "bool";
            else if (type == typeof(float)) return "float";
            else if (type == typeof(int)) return "int";
            else return "string";
        }

        /// <summary>
        /// 获取所有参数名字
        /// <return>返回一行的参数名字</return>
        /// </summary>
        private string[] GetValueNameRow()
        {
            if (dataList.Count == 0)
            {
                Log.Error("ExcelData GetValueNameRow: no data");
                return null;
            }

            int colums = dataList[0].values.Count;
            string[] retValue = new string[colums];
            var findIndex = -1;
            for (int i = 0; i < dataList.Count; ++i)
            {
                var dataRowTmp = dataList[i];

                //找到第一个铺满整行数据的，同时注释掉的行作为属性名称行
                if (dataRowTmp.isIgnoreData && !string.IsNullOrEmpty(dataRowTmp.values[dataRowTmp.values.Count - 1]))
                {
                    //获取当前行首列是否有设置数据标记
                    if (dataRowTmp.values.Count > 0)
                    {
                        for (int j = 0; j < dataRowTmp.values.Count; ++j)
                        {
                            var valueTmp = dataRowTmp.values[j];
                            if (valueTmp.Contains(LIST_KEY))
                            {
                                //删除数据标记
                                dataRowTmp.values[j] = dataRowTmp.values[j].Remove(LIST_KEY);
                                findIndex = i;

                                //设置数据类型
                                _dataType = DataType.List;
                                break;
                            }
                            else if (valueTmp.Contains(DICTIONARY_KEY))
                            {
                                //删除数据标记
                                dataRowTmp.values[j] = dataRowTmp.values[j].Remove(DICTIONARY_KEY);
                                findIndex = i;

                                //设置数据类型
                                _dataType = DataType.Dictionary;

                                //设置数据key值类型所在行下标
                                _dataDictionaryKeyTypeColIndex = j;
                                break;
                            }
                        }

                        //找到数据类型行，退出循环
                        if (findIndex >= 0)
                        {
                            break;
                        }
                    }
                }
            }

            //获取一行的参数名字
            if (findIndex >= 0)
            {
                for (int i = 0; i < colums; ++i)
                {
                    retValue[i] = dataList[findIndex].values[i];

                    //删除注释标记
                    if (retValue[i].StartsWith(IGNORE_FLAG))
                    {
                        retValue[i] = retValue[i].Remove(0, IGNORE_FLAG.Length);
                    }
                }
            }
            //没有找到参数名字行，用默认参数名字代替
            else
            {
                for (int i = 0; i < colums; ++i)
                {
                    retValue[i] = "Item" + i;
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取参数的名字
        /// <param name="indexCol">纵向数据下标</param>
        /// <param name="rowValueNames">一行参数名字</param>
        /// <return>参数名字</return>
        /// </summary>
        private string GetValueName(int indexCol, string[] rowValueNames)
        {
            var retValue = string.Empty;
            if (dataList.IsNullOrEmpty())
            {
                Log.Error("ExcelData GetValueName error: not init data");
                return retValue;
            }

            if (indexCol < 0 || indexCol > rowValueNames.Length - 1)
            {
                Log.Error("ExcelData GetValueName error: out of range, indexCol=" + indexCol + " count=" + rowValueNames.Length);
                return retValue;
            }

            retValue = rowValueNames[indexCol];

            //参数明明不允许空格，默认替换空格为下划线
            retValue = retValue.Replace(" ", "_");
            return retValue;
        }
	}
}
