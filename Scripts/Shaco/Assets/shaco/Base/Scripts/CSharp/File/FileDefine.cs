using System.Collections;

namespace shaco.Base
{
    public class FileDefine
    {
        public enum FileExtension
        {
            None = -1,
            AssetBundle = 0,
            Prefab,
            Png,
            Jpg,
            Txt,
            Json,
            Xml,
            Lua,
            Bytes,
        }

#if UNITY_5_3_OR_NEWER
        static public string persistentDataPath = string.Empty;
#else
        static public string persistentDataPath = UnityEngine.Application.persistentDataPath;
#endif        
        public const string PATH_FLAG_SPLIT = "/";
        public const string DOT_SPLIT = ".";
        public const long ONE_KB = 1024;
        public const long ONE_MB = 1024 * 1024;
        public const long ONE_GB = 1024 * 1024 * 1024;
    }
}
