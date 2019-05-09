using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace shaco
{
    public class DataSave : shaco.Base.DataSave, shaco.IDataSave
    {
        static public new shaco.DataSave Instance { get { return GameEntry.GetInstance<shaco.DataSave>(); } }

        //write functions ------ unity type
        public void Write(string key, Vector2 value) { WriteArguments(key, value.x, value.y); }
        public void Write(string key, Vector3 value) { WriteArguments(key, value.x, value.y, value.z); }
        public void Write(string key, Vector4 value) { WriteArguments(key, value.x, value.y, value.z, value.w); }
        public void Write(string key, Color value) { WriteArguments(key, value.r, value.g, value.b, value.a); }
        public void Write(string key, Rect value) { WriteArguments(key, value.x, value.y, value.width, value.height); }

        //read functions ------ unity type
        public Vector2 ReadVector2(string key) { return ReadVector2(key, Vector2.zero); }
        public Vector2 ReadVector2(string key, Vector2 defaultValue)
        {
            var strTmp = ReadString(key);
            if (string.IsNullOrEmpty(strTmp))
            {
                return defaultValue;
            }
            else
            {
                var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, SPLIT_FLAG);
                return new Vector2(float.Parse(splitTmp[0]), float.Parse(splitTmp[0]));
            }
        }

        public Vector3 ReadVector3(string key) { return ReadVector3(key, Vector3.zero); }
        public Vector3 ReadVector3(string key, Vector3 defaultValue)
        {
            var strTmp = ReadString(key);
            if (string.IsNullOrEmpty(strTmp))
            {
                return defaultValue;
            }
            else
            {
                var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, SPLIT_FLAG);
                return new Vector3(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]));
            }
        }

        public Vector4 ReadVector4(string key) { return ReadVector4(key, Vector3.zero); }
        public Vector4 ReadVector4(string key, Vector3 defaultValue)
        {
            var strTmp = ReadString(key);
            if (string.IsNullOrEmpty(strTmp))
            {
                return defaultValue;
            }
            else
            {
                var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, SPLIT_FLAG);
                return new Vector4(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]), float.Parse(splitTmp[3]));
            }
        }

        public Color ReadColor(string key) { return ReadColor(key, Color.black); }
        public Color ReadColor(string key, Color defaultValue)
        {
            var strTmp = ReadString(key);
            if (string.IsNullOrEmpty(strTmp))
            {
                return defaultValue;
            }
            else
            {
                var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, SPLIT_FLAG);
                return new Color(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]), float.Parse(splitTmp[3]));
            }
        }


        public Rect ReadRect(string key) { return ReadRect(key, new Rect()); }
        public Rect ReadRect(string key, Rect defaultValue)
        {
            var strTmp = ReadString(key);
            if (string.IsNullOrEmpty(strTmp))
            {
                return defaultValue;
            }
            else
            {
                var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, SPLIT_FLAG);
                return new Rect(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]), float.Parse(splitTmp[3]));
            }
        }
    }
}