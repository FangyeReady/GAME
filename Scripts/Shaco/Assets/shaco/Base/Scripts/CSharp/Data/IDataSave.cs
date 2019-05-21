using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    interface IDataSave
    {
        //Write functions ------ basic type
        void Write(string key, char value);
        void Write(string key, bool value);
        void Write(string key, int value);
        void Write(string key, float value);
        void Write(string key, double value);
        void Write(string key, long value);
        void Write(string key, string value);
        void WriteEnum<T>(string key, T value);
        void Write(string key, string[] values);
        void WriteArguments(string key, params object[] args);

        //Read functions ------ basic type
        char ReadChar(string key, char defaultValue = ' ');
        bool ReadBool(string key, bool defaultValue = false);
        int ReadInt(string key, int defaultValue = 0);
        float ReadFloat(string key, float defaultValue = 0);
        double ReadDouble(string key, double defaultValue = 0);
        long ReadLong(string key, long defaultValue = 0);
        T ReadEnum<T>(string key, T defaultValue = default(T));
        string ReadString(string key, string defaultValue = "");
        string[] ReadStringArray(string key, string[] defaultValue = null);
        List<string> ReadStringList(string key, string[] defaultValue = null);
        T ReadUserType<T>(string key, System.Func<string[], T> callbackConvert, T defaultValue = default(T));
        void Remove(string key);
        void RemoveStartWith(string keyPrefix);
        bool ContainsKey(string key);
        void Clear();
        string GetFormatString();
        void ReloadFromFile(string path = null, bool autoEncrypt = true);
    }
}