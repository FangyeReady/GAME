using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Base
{
    public static partial class Utility
    {
        /// <summary>
        /// 导入语言收集信息
        /// </summary>
        public class LocalizationCollectnfo
        {
            //文件路径
            public string path = string.Empty;

            //文本内容
            public string languageString = string.Empty;

            //额外参数
            public string parameter = "*";
        }

        /// <summary>
        /// 导出语言信息
        /// </summary>
        public class LocalizationExportInfo
        {
            //文件路径
            public string path = string.Empty;

            //原文，默认不允许为空字符串
            public string textOriginal = "*";

            //译文，默认不允许为空字符串
            public string textTranslation = "*";

            //原文所在行(不包含注释)
            public int lineOriginal = -1;

            //额外参数，默认不允许为空字符串
            public string parameter = "*";
        }

        /// <summary>
        /// 收集语言标记符号
        /// </summary>
        public class LocalizationCollectPairFlag
        {
            //起始符号
            public string startFlag = string.Empty;

            //终止符号
            public string endFlag = string.Empty;
        }

        /// <summary>
        /// 筛选出语言对应的文本内容
        /// <param name="value">需要筛选的文本内容</param>
        /// <param name="collectFlags">需要筛选的符号</param>
        /// <param name="ignoreCollectFlags">需要过滤筛选的符号，一般为注释内容</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static public Dictionary<string, LocalizationExportInfo> SelectChineseCharacter(string value, List<LocalizationCollectPairFlag> collectFlags)
        {
            var retValue = SelectLanguageCharacter(value, collectFlags, new int[] { 0x4e00 }, new int[] { 0x9fbb });

            //排除包含日文的筛选部分
            var removeKeys = new List<string>();
            foreach (var iter in retValue)
            {
                var strTmp = iter.Value.textOriginal;
                bool hasJapanese = false;

                for (int i = strTmp.Length - 1; i >= 0; --i)
                {
                    int codeTmp = (int)strTmp[i];

                    //文字中包含片假名或者平假名，则判断为日文，需要过滤掉
                    if ((codeTmp >= 0x3040 && codeTmp <= 0x3090) || codeTmp >= 0x30A0 && codeTmp <= 0x30FF)
                    {
                        hasJapanese = true;
                        break;
                    }
                }

                if (hasJapanese)
                {
                    removeKeys.Add(iter.Key);
                }
            }

            for (int i = 0; i < removeKeys.Count; ++i)
            {
                retValue.Remove(removeKeys[i]);
            }

            return retValue;
        }

        /// <summary>
        /// 获取其中的日文字符，只能判断文字中是否包含片假名或者平假名，无法判断日文中的汉字
        /// <param name="value">需要筛选的文本内容</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static public Dictionary<string, LocalizationExportInfo> SelectJapaneseCharacter(string value, List<LocalizationCollectPairFlag> collectFlags)
        {
            return SelectLanguageCharacter(value, collectFlags, new int[] { 0x3040, 0x30A0 }, new int[] { 0x3090, 0x30FF });
        }

        /// <summary>
        /// 获取其中的英文字符
        /// <param name="value">需要筛选的文本内容</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static public Dictionary<string, LocalizationExportInfo> SelectEnglishCharacter(string value, List<LocalizationCollectPairFlag> collectFlags)
        {
            return SelectLanguageCharacter(value, collectFlags, new int[] { 0x30, 0x61, 0x41 }, new int[] { 0x39, 0x7a, 0x5a });
        }

        /// <summary>
        /// 获取其中的中文字符
        /// <param name="value">字符串</param>
        /// <param name="startCode">字符开始编码</param>
        /// <param name="endCode">字符终止编码</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static private Dictionary<string, LocalizationExportInfo> SelectLanguageCharacter(string value, List<LocalizationCollectPairFlag> collectFlags, int[] startCode, int[] endCode)
        {
            Dictionary<string, LocalizationExportInfo> retValue = null;

            if (startCode.IsNull() || endCode.IsNull())
            {
                Debug.LogError("LocalizationReplaceInspector SelectLanguageCharacter error: invalid param");
                return retValue;
            }

            if (startCode.Length != endCode.Length)
            {
                Debug.LogError("LocalizationReplaceInspector SelectLanguageCharacter error: start code count must == end code count");
                return retValue;
            }

            if (!collectFlags.IsNullOrEmpty())
            {
                //筛选文本
                for (int i = 0; i < collectFlags.Count; ++i)
                {
                    var startSplitCode = collectFlags[i].startFlag;
                    var endSplitCode = collectFlags[i].endFlag;

                    retValue = SelectLanguageCharacterWithSplitCode(value, startCode, endCode, startSplitCode, endSplitCode);
                }
            }
            else
            {
                retValue = SelectLanguageCharacterWithSplitCode(value, startCode, endCode, string.Empty, string.Empty);
            }

            return retValue;
        }

        /// <summary>
        /// 根据指定的拆分符号筛选语言
        /// <param name="value">字符串</param>
        /// <param name="startCode">字符开始编码</param>
        /// <param name="endCode">字符终止编码</param>
        /// <param name="splitStartCode">拆分起始符号</param>
        /// <param name="splitEndCode">拆分终止符号</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static private Dictionary<string, LocalizationExportInfo> SelectLanguageCharacterWithSplitCode(string value, int[] startCode, int[] endCode, string splitStartCode, string splitEndCode)
        {
            Dictionary<string, LocalizationExportInfo> retValue = new Dictionary<string, LocalizationExportInfo>();

            var splitList = value.SplitWithoutTransfer(splitStartCode, splitEndCode);

            for (int i = splitList.Count - 1; i >= 0; --i)
            {
                var strTmp = splitList[i];
                bool isChecked = false;

                for (int j = strTmp.Length - 1; j >= 0; --j)
                {
                    int codeTmp = (int)strTmp[j];

                    for (int k = 0; k < startCode.Length; ++k)
                    {
                        if (codeTmp >= startCode[k] && codeTmp <= endCode[k])
                        {
                            //"ー"这个符号和日中片假名中的"ー"是同一个编码，需要特殊处理
                            if (strTmp[j] != 'ー')
                            {
                                isChecked = true;
                                break;
                            }
                        }
                    }
                }

                //如果不是目标语言，则从列表中删除
                if (!isChecked)
                {
                    splitList.RemoveAt(i);
                }
                //添加目标语言信息
                else
                {
                    if (!retValue.ContainsKey(strTmp))
                    {
                        retValue.Add(strTmp, new LocalizationExportInfo() { textOriginal = strTmp });
                    }
                }
            }

            return retValue;
        }
    }
}