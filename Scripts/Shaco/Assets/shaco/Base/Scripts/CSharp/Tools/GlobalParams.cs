namespace shaco.Base
{
    public class GlobalParams
    {
        //shaco游戏框架版本号
        public const string SHACO_GAME_FRAMEWORK_VERSION = "1.0";

        static public bool OpenDebugLog
        {
            get { return _openDebugLog; }
        }


#if DEBUG_LOG
        static private bool _openDebugLog = true;
#else
        static private bool _openDebugLog = false;
#endif
        static readonly public char ENCRYPT_SECRET_CODE = (char)14;
        static readonly public string DATA_SAVE_EXTENSIONS = "sdata";
#if UNITY_EDITOR
        static public readonly string[] DEFAULT_ASSEMBLY = new string[] { "Assembly-CSharp", "Assembly-CSharp-Editor" };
#else
        static public readonly string[] DEFAULT_ASSEMBLY = new string[] { "Assembly-CSharp" };
#endif

        static public string GetShacoFrameworkRootPath()
        {
            var retValue = shaco.Base.FileHelper.GetCurrentSourceFolderPath().Substring("", "shaco/Base") + "shaco/Base";
            if (retValue.StartsWith("//"))
            {
                retValue = retValue.ReplaceFromBegin("//", "\\\\", 1);
            }
            return retValue;
        }
    }
}