using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class DataSave : IDataSave
    {
        static protected readonly string FORMAT_BEGIN = "[";
        static protected readonly string FORMAT_END = "]";
        static protected readonly string SPLIT_FLAG = "##";
        static protected readonly string ARRAY_SPLIT_FLAG = ",";

        protected bool autoEncrypt = true;
        protected string _savePath = string.Empty;
        protected Dictionary<string, string> _mapDatas = new Dictionary<string, string>();

        private System.Threading.Mutex _mutex = new System.Threading.Mutex();

        static public DataSave Instance { get { return GameEntry.GetInstance<DataSave>(); } }

        //Write functions ------ basic type
        public void Write(string key, char value) { WriteString(key, value.ToString(), false); }
        public void Write(string key, bool value) { WriteString(key, value.ToString(), false); }
        public void Write(string key, int value) { WriteString(key, value.ToString(), false); }
        public void Write(string key, float value) { WriteString(key, value.ToString(), false); }
        public void Write(string key, double value) { WriteString(key, value.ToString(), false); }
        public void Write(string key, long value) { WriteString(key, value.ToString(), false); }
        public void Write(string key, string value) { WriteString(key, value.ToString(), true); }
        public void Write(string key, string[] values) { Write(key, values.ToArrayList()); }
        public void Write(string key, List<string> values)
        {
            var appendStr = new System.Text.StringBuilder();
            for (int i = 0; i < values.Count; ++i)
            {
                appendStr.Append(values[i]);
                appendStr.Append(ARRAY_SPLIT_FLAG);
            }
            if (appendStr.Length > 0)
            {
                appendStr.Remove(appendStr.Length - 1, 1);
            }
            WriteString(key, appendStr.ToString(), true);
        }

        public void WriteEnum<T>(string key, T value)
        {
            Write(key, value.ToString());
        }

        public void WriteArguments(string key, params object[] args)
        {
            string strWrite = GetStringBySplitFlag(args);
            WriteString(key, strWrite, false);
        }

        //Read functions ------ basic type
        public char ReadChar(string key, char defaultValue = ' ') { return char.Parse(ReadString(key, defaultValue.ToString())); }
        public bool ReadBool(string key, bool defaultValue = false) { return bool.Parse(ReadString(key, defaultValue.ToString())); }
        public int ReadInt(string key, int defaultValue = 0) { return int.Parse(ReadString(key, defaultValue.ToString())); }
        public float ReadFloat(string key, float defaultValue = 0) { return float.Parse(ReadString(key, defaultValue.ToString())); }
        public double ReadDouble(string key, double defaultValue = 0) { return double.Parse(ReadString(key, defaultValue.ToString())); }
        public long ReadLong(string key, long defaultValue = 0) { return long.Parse(ReadString(key, defaultValue.ToString())); }
        public T ReadEnum<T>(string key, T defaultValue = default(T)) { return Utility.ToEnum<T>(ReadString(key, defaultValue.ToString())); }
        public List<string> ReadStringList(string key, string[] defaultValue = null)
        {
            var retValue = ReadString(key).Split(ARRAY_SPLIT_FLAG).ToArrayList();
            if (null != retValue && retValue.Count > 0)
            {
                if (string.IsNullOrEmpty(retValue[retValue.Count - 1]))
                {
                    retValue.RemoveAt(retValue.Count - 1);
                }
            }
            return retValue;
        }
        public string[] ReadStringArray(string key, string[] defaultValue = null) { return ReadStringList(key, defaultValue).ToArray(); }

        public string ReadString(string key, string defaultValue = "")
        {
            this.CheckInit();
            if (!this._mapDatas.ContainsKey(key))
                return defaultValue;
            else
                return this._mapDatas[key];
        }

        public T ReadUserType<T>(string key, System.Func<string[], T> callbackConvert, T defaultValue)
        {
            var strTmp = ReadString(key);
            if (string.IsNullOrEmpty(strTmp))
            {
                return defaultValue;
            }
            else
            {
                var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, SPLIT_FLAG);
                return (T)callbackConvert(splitTmp);
            }
        }

        public void Remove(string key)
        {
            if (ContainsKey(key))
            {
                this._mapDatas.Remove(key);
                this.SaveMapData();
            }
        }

        public void RemoveStartWith(string keyPrefix)
        {
            var listRemoveKeys = new List<string>();
            foreach (var iter in this._mapDatas)
            {
                if (iter.Key.Contains(keyPrefix))
                {
                    listRemoveKeys.Add(iter.Key);
                }
            }

            for (int i = listRemoveKeys.Count - 1; i >= 0; --i)
            {
                this._mapDatas.Remove(listRemoveKeys[i]);
            }

            if (listRemoveKeys.Count > 0)
            {
                listRemoveKeys.Clear();
                this.SaveMapData();
            }
        }

        public bool ContainsKey(string key)
        {
            return this._mapDatas.ContainsKey(key);
        }

        //other functions 
        public void Clear()
        {
            this._mapDatas.Clear();
            this._savePath = string.Empty;
        }

        public string GetFormatString()
        {
            string strPrint = string.Empty;
            foreach (var key in this._mapDatas.Keys)
            {
                var value = this._mapDatas[key];

                strPrint += "key:[" + key + "]" + "  value:[" + value + "]" + "\n";
            }

            if (strPrint.Length > 0)
            {
                strPrint = strPrint.Remove(strPrint.Length - 1, 1);
            }
            return strPrint;
        }

        /// <summary>
        /// realoda all save config from path
        /// </summary>
        /// <param name="path">if path is null or empty, will auto set it as default path</param>
        /// <param name="autoEncrypt">if true, will encrypt all config data</param>
        public void ReloadFromFile(string path = null, bool autoEncrypt = true)
        {
            this.autoEncrypt = autoEncrypt;
            this._savePath = path;
            this._mapDatas.Clear();
            this.InitMapData();
            this.SaveMapData();
        }

        protected string GetStringBySplitFlag(params object[] args)
        {
            string ret = string.Empty;
            if (args.Length == 0)
            {
                Log.Exception("GetStringBySplitFlag error: args length is " + args.Length);
            }
            int lenTmp = args.Length - 1;
            for (int i = 0; i < lenTmp; ++i)
            {
                ret += args[i].ToString();
                ret += SPLIT_FLAG;
            }

            ret += args[lenTmp];

            return ret;
        }

        protected string GetSavePath()
        {
            if (string.IsNullOrEmpty(FileDefine.persistentDataPath))
            {
                Log.Exception("FileDefine.persistentDataPath is empty");
            }

            if (string.IsNullOrEmpty(this._savePath))
            {
                this._savePath = FileDefine.persistentDataPath + FileDefine.PATH_FLAG_SPLIT + "DataSave2_0." + GlobalParams.DATA_SAVE_EXTENSIONS;
            }
            return this._savePath;
        }

        protected System.Text.StringBuilder Format(string source)
        {
            var ret = new System.Text.StringBuilder();
            if (source.Contains(FORMAT_BEGIN) || source.Contains(FORMAT_END))
            {
                Log.Exception("DataSave format erorr: can't use flag " + FORMAT_BEGIN + " and " + FORMAT_END + " source=" + source);
            }

            ret.Append(FORMAT_BEGIN);
            ret.Append(source);
            ret.Append(FORMAT_END);
            return ret;
        }

        protected List<string> GetSourceByLine(string readString, ref int startIndex)
        {
            List<string> ret = new List<string>();

            while (ret.Count < 2)
            {
                var strSub = readString.Substring(FORMAT_BEGIN, FORMAT_END, startIndex);
                ret.Add(strSub);
                startIndex += strSub.Length + FORMAT_BEGIN.Length + FORMAT_END.Length;

                if (startIndex >= readString.Length)
                {
                    break;
                }
            }

            startIndex += "\n".Length;
            return ret;
        }

        protected void CheckInit()
        {
            InitMapData();
        }

        protected void InitMapData()
        {
            if (_mapDatas.Count > 0)
                return;

            lock (_mutex)
            {
                var path = GetSavePath();
                if (!System.IO.File.Exists(path))
                    return;

                _savePath = path;
                int readStartIndex = 0;
                var readBytes = FileHelper.ReadAllByteByUserPath(path);

                if (null == readBytes || readBytes.Length == 0)
                    return;

                var readString = EncryptDecrypt.Decrypt(readBytes).ToStringArray();

                while (readStartIndex < readString.Length)
                {
                    var listStr = GetSourceByLine(readString, ref readStartIndex);
                    if (listStr.Count < 2)
                    {
                        Log.Error("Data Save initMapData error: file length=" + listStr.Count);
                        break;
                    }

                    //key 
                    var strKey = listStr[0];

                    //value
                    var strValue = listStr[1];

                    if (string.IsNullOrEmpty(strKey))
                    {
                        Log.Error("DataSave InitMapData error: key is empty");
                    }
                    else
                    {
                        if (_mapDatas.ContainsKey(strKey))
                        {
                            Log.Error("DataSave InitMapData error: has duplicate key=" + strKey + " value=" + strValue);
                        }
                        else
                        {
                            _mapDatas.Add(strKey, strValue);
                        }
                    }
                }
            }
        }

        protected void CheckMapDataModify(string key, string value)
        {
            lock (_mutex)
            {
                bool isChanged = true;
                CheckInit();

                if (_mapDatas.ContainsKey(key))
                {
                    if (_mapDatas[key] != value)
                        _mapDatas[key] = value;
                    else
                        isChanged = false;
                }
                else
                {
                    _mapDatas.Add(key, value);
                }

                //reWrite all file
                if (isChanged || !System.IO.File.Exists(GetSavePath()))
                {
                    SaveMapData();
                }
            }
        }

        protected void SaveMapData()
        {
            if (_mapDatas.Count == 0)
                return;

            lock (_mutex)
            {
                var WriteContent = new System.Text.StringBuilder();

                foreach (var key in _mapDatas.Keys)
                {
                    var key2 = key;
                    var value = _mapDatas[key2];

                    WriteContent.Append(Format(key2));
                    WriteContent.Append(Format(value));
                    WriteContent.Append('\n');
                }

                var writeStringTmp = WriteContent.ToString();
                if (writeStringTmp.Length > 0)
                {
                    writeStringTmp = writeStringTmp.Remove(writeStringTmp.Length - 1, 1);
                }

                if (autoEncrypt)
                {
                    writeStringTmp = EncryptDecrypt.Encrypt(writeStringTmp.ToString().ToByteArray(), Utility.Random(5, 10)).ToStringArray();
                }
                System.IO.File.WriteAllText(GetSavePath(), writeStringTmp);
            }
        }

        protected void WriteString(string key, string value, bool checkValueValid)
        {
            if (checkValueValid)
            {
                if (value.Contains(SPLIT_FLAG))
                {
                    Log.Exception("DataSave Write string error: value can't contain flag " + SPLIT_FLAG + " \nkey=" + key + " value=" + value);
                }
            }
            this.CheckMapDataModify(key, value);
        }
    }
}
