using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class HotUpdateDefine
    {
        //导出assetbundle配置信息
        [SerializeField]
        public class ExportAssetBundle
        {
            public class ExportFile
            {
                //文件路径
                public string Key;
            }
            public string AssetBundleName = "BundleName";
            public string AssetBundleMD5 = string.Empty;
            public long fileSize = 0;
            public List<ExportAssetBundle.ExportFile> ListFiles = new List<ExportAssetBundle.ExportFile>();
        }

        //下载的assetbundle信息
        [SerializeField]
        public class DownloadAssetBundle
        {
            public ExportAssetBundle ExportInfo = null;
            public shaco.Base.HttpHelper HttpDel = null;
            public HotUpdateImportWWW HotUpdateDel = null;

            public DownloadAssetBundle(ExportAssetBundle exportInfo)
            {
                this.ExportInfo = exportInfo;
            }
            public DownloadAssetBundle() { }
        }

        //序列化的版本控制描述文件
        [SerializeField]
        public class SerializeVersionControl
        {
            //文件是否为自动加密状态
            public bool AutoEncryt = true;
            //是否自动打包修改内容(包含本次更新和删除的文件与目录 )
            public bool AutoBuildModifyPackage = false;
            //文件标记符(预留位置)
            public string FileTag = string.Empty;
            //Unity版本号
            public string UnityVersion;
            //资源版本号
            public string Version;
            //现在的MD5Main字段被MainMD5.txt文件所替代了，该字段作为API字段保留
            public string MD5Main;
            //所有Assetbundle和原始文件数据大小
            public long TotalDataSize;
            //版本中所有assetbundle
            public List<ExportAssetBundle> ListAssetBundles = new List<ExportAssetBundle>();
            //需要保留为原始文件的目录，该目录下文件只作为原始文件来加载，支持纯文本和图片
            public List<string> ListKeepFileFolders = new List<string>();
            //导出资源时候，需要在计算md5时候忽略的meta文件后缀名
            public List<string> ListIgnoreMetaExtensions = new List<string>();

            public SerializeVersionControl() {}
            public SerializeVersionControl(SerializeVersionControl other)
            {
                this.AutoEncryt = other.AutoEncryt;
                this.AutoBuildModifyPackage = other.AutoBuildModifyPackage;
                this.FileTag = other.FileTag;
                this.UnityVersion = other.UnityVersion;
                this.Version = other.Version;
                this.MD5Main = other.MD5Main;
                this.TotalDataSize = other.TotalDataSize;
                this.ListAssetBundles.Clear(); this.ListAssetBundles.AddRange(other.ListAssetBundles);
                this.ListKeepFileFolders.Clear(); this.ListKeepFileFolders.AddRange(other.ListKeepFileFolders);
                this.ListIgnoreMetaExtensions.Clear(); this.ListIgnoreMetaExtensions.AddRange(other.ListIgnoreMetaExtensions);
            }
        }

        //版本控制配置文件(仅在编辑器使用)
        public class VersionControlLocalConfigEditor
        {
            //导出资源时候需要被过滤的文件模板，和该模板相同的文件类型也会被过滤掉
            public List<Object> ListIgnoreMetaModel = new List<Object>();
            //导出资源时候需要作为原始文件保留的文件目录
            public List<Object> ListKeepFolder = new List<Object>();

            public void Clear()
            {
                ListIgnoreMetaModel.Clear();
            }
        }

        public delegate void CALL_FUNC_READ_OBJECT(UnityEngine.Object value);
        public delegate void CALL_FUNC_READ_OBJECTS(UnityEngine.Object[] values);
        public delegate void CALL_FUNC_READ_STRING(string value);
        public delegate void CALL_FUNC_READ_BYTE(byte[] value);
        public delegate void CALL_FUNC_READ_PROGRESS(float percent);

        public enum ResourceCreateMode
        {
            Memory,
            MemoryAsync
        }

        public enum Platform
        {
            None,
            Android,
            iOS
        }

        public enum ExportFileAPI
        {
            None,
            DeleteFile,
            DeleteUnUseFiles,
        }

        //资源配置文件数量
        static public readonly int ALL_VERSION_CONTROL_FILE_COUNT = 4;
        //计算下载百分比的时候，下载资源配置文件所占的百分比(范围0~1)
        static public readonly float CHECK_VERSION_PROGRESS_RATIO = 0.1f;

        //file tag
        static public readonly string SIGN_FLAG = "@@";
        static public readonly string PATH_RELATIVE_FLAG = "##";
        static public readonly string FILENAME_TAG_ANDROID = SIGN_FLAG + "android";
        static public readonly string FILENAME_TAG_IOS = SIGN_FLAG + "ios";
        // static public readonly string FILENAME_TAG_UNITY_VERSION = SIGN_FLAG;
        // static public readonly string PATH_TAG_UNITY_VERSION = SIGN_FLAG;
        static public readonly string FILE_DELETE_FLAG = "(Delete)";
        static public readonly string FILE_UPDATE_FLAG = "(Update)";
        static public readonly string EXTENSION_ASSETBUNDLE = ".assetbundle";
        static public readonly string EXTENSION_VERSION_CONTROL = ".json";
        static public readonly string EXTENSION_MANIFEST = ".manifest";
        static public readonly string EXTENSION_META = ".meta";
        static public readonly string EXTENSION_MAIN_MD5 = ".txt";
        static public readonly string EXTENSION_ENCRPYT = ".txt";
        static public readonly string VERSION_CONTROL = "VersionControl";
        static public readonly string VERSION_CONTROL_MAIN_MD5 = VERSION_CONTROL + "_MainMD5";
        static public readonly string PATH_TAG_ANDROID = VERSION_CONTROL + SIGN_FLAG + "Android";
        static public readonly string PATH_TAG_IOS = VERSION_CONTROL + SIGN_FLAG + "iOS";
        static public readonly string PATH_TAG_ORIGINAL = "(original)";
        
        static public readonly string ORIGINAL_FILE_TAG = "origianl";
    }
}
