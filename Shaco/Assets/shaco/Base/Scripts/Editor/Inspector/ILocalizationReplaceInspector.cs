using System.Collections;
using System.Collections.Generic;

namespace shacoEditor
{
    public interface ILocalizationReplace
    {
        /// <summary>
        /// 获取所有语言文本
        /// <param name="importPath">导入路径</param>
        /// <param name="callbackCollectInfo">加载完毕后语言文本收集信息</param>
        /// <param name="collectExtensions">需要收集的文件后缀名</param>
        /// </summary>
        void GetAllLanguageString(string importPath, System.Action<List<shaco.Base.Utility.LocalizationCollectnfo>> callbackCollectInfo, params string[] collectExtensions);

        /// <summary>
        /// 替换语言文本信息
        /// <param name="path">路径</param>
        /// <param name="exportInfo">导出的语言包信息</param>
        /// </summary>
        void RepalceLanguageString(string path, List<shaco.Base.Utility.LocalizationExportInfo> exportInfos);

        /// <summary>
        /// 绘制编辑器
        /// </summary>
        void DrawInspector();

        /// <summary>
        /// 是否只在主线程下替换文件
        /// </summary>
        bool OnlyReplaceInMainThread();
    }
}