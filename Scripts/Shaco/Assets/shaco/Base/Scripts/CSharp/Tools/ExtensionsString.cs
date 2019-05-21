using System.Collections;

static public class shaco_ExtensionsString
{
    static public char ToChar(this string str, char defaultValue = ' ', bool throwError = true)
    {
        char ret = ' ';
        if (!char.TryParse(str, out ret))
        {
            ret = defaultValue;

            if (throwError)
                shaco.Base.Log.Exception("ToChar: parse error!");
        }

        return ret;
    }

    static public bool ToBool(this string str, bool defaultValue = false, bool throwError = true)
    {
        bool ret = false;
        if (!bool.TryParse(str, out ret))
        {
            ret = defaultValue;

            if (throwError)
                shaco.Base.Log.Exception("ToBool: parse error!");
        }

        return ret;
    }

    static public int ToInt(this string str, int defaultValue = 0, bool throwError = true)
    {
        int ret = 0;
        if (!int.TryParse(str, out ret))
        {
            ret = defaultValue;

            if (throwError)
                shaco.Base.Log.Exception("ToInt: parse error!");
        }

        return ret;
    }

    static public long ToLong(this string str, long defaultValue = 0, bool throwError = true)
    {
        long ret = 0;
        if (!long.TryParse(str, out ret))
        {
            ret = defaultValue;

            if (throwError)
                shaco.Base.Log.Exception("ToLong: parse error!");
        }

        return ret;
    }

    static public float ToFloat(this string str, float defaultValue = 0, bool throwError = true)
    {
        float ret = 0;
        if (!float.TryParse(str, out ret))
        {
            ret = defaultValue;

            if (throwError)
                shaco.Base.Log.Exception("ToFloat: parse error!");
        }

        return ret;
    }

    static public System.Type ToType(this string fullTypeName)
    {
        var retValue = System.Type.GetType(fullTypeName);
        if (null == retValue)
        {
            shaco.Base.Log.Exception("ExtensionUtility ToType error: can't find class by full type name=" + fullTypeName);
            return retValue;
        }
        return retValue;
    }

    static public T ToEnum<T>(this string value)
    {
        return (T)System.Enum.Parse(typeof(T), value, true);
    }

    static public byte[] ToByteArray(this string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }

    static public string ToStringArray(this byte[] buf)
    {
        return System.Text.Encoding.UTF8.GetString(buf);
    }

    static public string ToStringArrayUTF8(this string str)
    {
        return System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(str));
    }

    /// <summary>
    /// parse str until symbol
    /// </summary>
    /// <returns>parse string</returns>
    /// <param name="str">String.</param>
    /// <param name="symbol">Symbol.</param>
    /// <param name="MoveOffsetIndex">Move Offset Index.</param>
    static public string ParseUntilSymbols(this string str, out int MoveOffsetIndex, params string[] symbols)
    {
        string retValue = string.Empty;
        MoveOffsetIndex = -1;
        var findSymbol = string.Empty;

        for (int i = 0; i < symbols.Length; ++i)
        {
            MoveOffsetIndex = str.IndexOf(symbols[i]);
            if (MoveOffsetIndex >= 0)
            {
                findSymbol = symbols[i];
                break;
            }
        }

        if (MoveOffsetIndex == -1)
        {
            return retValue;
        }
        retValue = str.Substring(0, MoveOffsetIndex);
        MoveOffsetIndex += findSymbol.Length;

        return retValue;
    }
    static public string ParseUntilSymbol(this string str, string symbol)
    {
        int outIndex = 0;
        return ParseUntilSymbols(str, out outIndex, new string[] { symbol });
    }

    static public string ParseUntilSymbols(this string str, params string[] symbols)
    {
        int outIndex = 0;
        return ParseUntilSymbols(str, out outIndex, symbols);
    }

    static public string Remove(this string str, string removeStr, bool printError = false)
    {
        var ret = str;
        int findIndex = ret.IndexOf(removeStr);
        if (findIndex != -1)
        {
            ret = ret.Remove(findIndex, removeStr.Length);
        }
        else if (printError)
        {
            shaco.Log.Error("ExtensionsString Remove error: not found removeStr=" + removeStr + " in=" + str);
        }
        return ret;
    }

    static public string RemoveAll(this string str, string removeStr)
    {
        var ret = str;

        while (true)
        {
            int findIndex = ret.IndexOf(removeStr);
            if (findIndex != -1)
            {
                ret = ret.Remove(findIndex, removeStr.Length);
            }
            else
            {
                break;
            }
        }
        return ret;
    }

    static public string RemoveSubstring(this string str, string start, string end, int startIndex = 0)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var indexFindStart = string.IsNullOrEmpty(start) ? 0 : str.IndexOf(start, startIndex);
        var indexFindEnd = string.IsNullOrEmpty(end) ? str.Length - 1 : str.LastIndexOf(end);
        if (indexFindStart < 0 || indexFindEnd < 0 || indexFindEnd < indexFindStart || indexFindStart == indexFindEnd)
        {
            return str;
        }

        return str.Remove(indexFindStart, indexFindEnd - indexFindStart + 1);
    }

    //获取字符串所在下标，过滤转义符
    static private int IndexOfWithoutTransfer(this string str, string find, int moveOffset = 0)
    {
        int retValue = -1;

        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(find))
        {
            return retValue;
        }

        do
        {
            if (moveOffset > str.Length - 1)
            {
                break;
            }

            retValue = str.IndexOf(find, moveOffset);
            if (retValue <= 0)
            {
                break;
            }
            else
            {
                //查看前面是否包含转义符号
                //统计前方转义符数量
                int transferCount = 0;
                int calculateIndex = retValue - 1;
                do
                {
                    if (str[calculateIndex] == '\\')
                    {
                        ++transferCount;
                    }
                    else
                    {
                        break;
                    }
                }
                while (--calculateIndex >= 0);

                //如果转义符数量为奇数，表示当前转义符生效的
                bool haveTransfer = transferCount % 2 != 0;

                if (haveTransfer)
                {
                    moveOffset = retValue + 1;
                }
                else
                {
                    break;
                }
            }
        }
        while (true);
        return retValue;
    }

    private enum AnnoationFlag
    {
        None,
        Collect,
        Ignore,
    }

    private enum AnnoationFlagSortMode
    {
        MoreFront,
        MoreBehind
    }

    /// <summary>
    /// 注释信息
    /// </summary>
    private class AnnotationPriorityResult
    {
        //是否为收集标记
        public AnnoationFlag flag = AnnoationFlag.None;

        //查找到的标记下标
        public int findIndex = -1;

    }

    /// <summary>
    /// 获取注释标记开始位置，根据注释和过滤标记分别筛选优先级
    /// <param name="source">原文本</param>
    /// <param name="collects">注释标记</param>
    /// <param name="ignores">忽略标记</param>
    /// <param name="offsetIndex">原文本字符串偏移下标</param>
    /// <return>查找到的注释开始信息</return>
    /// </summary>
    static private AnnotationPriorityResult IndexOfAnnotationPriority(this string source, string[] collects, string[] ignores, int offsetIndex, AnnoationFlagSortMode mode)
    {
        var retValue = new AnnotationPriorityResult();

        var indexCollect = int.MaxValue;
        var indexIgnore = int.MaxValue;

        for (int i = 0; i < collects.Length; ++i)
        {
            var indexFindTmp = source.IndexOfWithoutTransfer(collects[i], offsetIndex);
            if (indexFindTmp >= 0 && indexFindTmp < indexCollect)
            {
                indexCollect = indexFindTmp;
            }
        }
        for (int i = 0; i < ignores.Length; ++i)
        {
            var indexFindTmp = source.IndexOfWithoutTransfer(ignores[i], offsetIndex);
            if (indexFindTmp >= 0 && indexFindTmp < indexCollect)
            {
                indexIgnore = indexFindTmp;
            }
        }

        switch (mode)
        {
            //取值更靠前的标记
            case AnnoationFlagSortMode.MoreFront:
                {
                    if (indexIgnore != int.MaxValue)
                    {
                        if (indexIgnore < indexCollect || indexCollect == int.MaxValue)
                        {
                            retValue.flag = AnnoationFlag.Ignore;
                            retValue.findIndex = indexIgnore;
                        }
                    }
                    if (indexCollect != int.MaxValue)
                    {
                        if (indexCollect < indexIgnore || indexIgnore == int.MaxValue)
                        {
                            retValue.flag = AnnoationFlag.Collect;
                            retValue.findIndex = indexCollect;
                        }
                    }
                    break;
                }
            //取值更靠后的标记
            case AnnoationFlagSortMode.MoreBehind:
                {
                    if (indexIgnore != int.MaxValue)
                    {
                        if (indexIgnore > indexCollect || indexCollect == int.MaxValue)
                        {
                            retValue.flag = AnnoationFlag.Ignore;
                            retValue.findIndex = indexIgnore;
                        }
                    }
                    if (indexCollect != int.MaxValue)
                    {
                        if (indexCollect > indexIgnore || indexIgnore == int.MaxValue)
                        {
                            retValue.flag = AnnoationFlag.Collect;
                            retValue.findIndex = indexCollect;
                        }
                    }
                    break;
                }
            default: shaco.Log.Error("ExtensionsString IndexOfAnnotationPriority error: unsupport type=" + mode); break;
        }

        if (retValue.flag == AnnoationFlag.None)
        {
            if (indexCollect == indexIgnore && (indexIgnore != int.MaxValue && indexCollect != int.MaxValue))
            {
                shaco.Log.Error("ExtensionsString IndexOfAnnotationPriority error: have overlap flag" + " offsetIndex=" + offsetIndex + "\nsource=" + source);
            }
            else if (indexIgnore == int.MaxValue && indexCollect == int.MaxValue)
            {
                retValue.flag = AnnoationFlag.None;
                retValue.findIndex = -1;
            }
            else
            {
                shaco.Log.Error("ExtensionsString IndexOfAnnotationPriority error: unkown codition... indexIgnore=" + indexIgnore + " indexCollect=" + indexCollect + " offsetIndex=" + offsetIndex + "\nsource=" + source);
            }
        }

        return retValue;
    }

    /// <summary>
    /// 移除注释
    /// <param name="str">字符串</param>
    /// <param name="start">注释开始标志</param>
    /// <param name="end">注释结束标志</param>
    /// <param name="ignoreFlag">忽略标志，不会计算为注释内容</param>
    /// <return>忽略注释后的字符串</return>
    /// </summary>
    static public string RemoveAnnotation(this string str, string start, string end, params shaco.Base.Utility.LocalizationCollectPairFlag[] ignoreFlags)
    {
        var retValue = str;
        int offsetIndex = 0;

        if (string.IsNullOrEmpty(retValue))
        {
            return retValue;
        }

        //如果匹配字符串为空，则默认以换行符作为匹配
        if (string.IsNullOrEmpty(start))
            start = "\n";

        if (string.IsNullOrEmpty(end))
            end = "\n";

        var ignoreStartFlagsTmp = new string[ignoreFlags.Length];
        var ignoreEndFlagsTmp = new string[ignoreFlags.Length];
        AnnotationPriorityResult prevResultTmp = null;
        bool shouldStopRemove = false;
        bool forceIgnoreResult = false;

        for (int i = 0; i < ignoreStartFlagsTmp.Length; ++i)
        {
            ignoreStartFlagsTmp[i] = ignoreFlags[i].startFlag;
        }
        for (int i = 0; i < ignoreEndFlagsTmp.Length; ++i)
        {
            ignoreEndFlagsTmp[i] = ignoreFlags[i].endFlag;
        }

        do
        {
            AnnotationPriorityResult resultTmp = null;

            //如果有上个标记对象，则需要找到后者配对内容
            if (null == prevResultTmp)
            {
                resultTmp = retValue.IndexOfAnnotationPriority(new string[] { start }, ignoreStartFlagsTmp, offsetIndex, AnnoationFlagSortMode.MoreFront);

                //如果没有找到标记，则跳出循环
                if (resultTmp.flag == AnnoationFlag.None)
                {
                    shouldStopRemove = true;
                }
                else
                {
                    //下标跳转
                    offsetIndex = resultTmp.findIndex + 1;
                }
            }
            else
            {
                forceIgnoreResult = false;
                resultTmp = retValue.IndexOfAnnotationPriority(new string[] { end }, ignoreEndFlagsTmp, offsetIndex, AnnoationFlagSortMode.MoreFront);

                //只处理相同类型标记，不同类型标记会直接跳过
                //没有上个标记对象，继续搜寻新的标记对象
                if (resultTmp.flag == prevResultTmp.flag)
                {
                    switch (resultTmp.flag)
                    {
                        case AnnoationFlag.Collect:
                            {
                                //发现注释，删除掉
                                int startIndex = prevResultTmp.findIndex;
                                int endIndex = resultTmp.findIndex;

                                //如果是换行符则保留，同时偏移下标要倒退1
                                int removeLength = endIndex - startIndex + (end == "\n" ? 0 : end.Length);
                                retValue = retValue.Remove(startIndex, removeLength);

                                //因为删除了注释，所有本地找到的下标重置
                                offsetIndex = startIndex + 1;
                                break;
                            }
                        case AnnoationFlag.Ignore:
                            {
                                //下标跳转
                                offsetIndex = resultTmp.findIndex + 1;
                                break;
                            }
                        default: shaco.Log.Error("ExtensionsString RemoveAnnotation error: unsupport type=" + resultTmp.flag); shouldStopRemove = true; break;
                    }

                    //匹配结束，重置标记
                    resultTmp = null;
                }
                else
                {
                    //检查上一次匹配状态
                    switch (prevResultTmp.flag)
                    {
                        case AnnoationFlag.Collect:
                            {
                                //上次是最后一行注释标记，则默认删除后面的所有内容
                                if (end == "\n" && retValue.IndexOfWithoutTransfer("\n", offsetIndex) < 0)
                                {
                                    retValue = retValue.Remove(prevResultTmp.findIndex);
                                    shouldStopRemove = true;
                                }
                                //本次匹配失败，退出匹配
                                else if (resultTmp.flag == AnnoationFlag.None)
                                {
                                    shouldStopRemove = true;
                                }
                                //当前注释标记结尾不匹配，回滚查看位置到上一次标记+1上
                                else
                                {
                                    offsetIndex = resultTmp.findIndex + 1;
                                    resultTmp.flag = AnnoationFlag.None;

                                    //强制过滤本次的搜索结果，进行下次匹配
                                    forceIgnoreResult = true;
                                }

                                //本次匹配结束，重置标记
                                resultTmp = null;
                                break;
                            }
                        case AnnoationFlag.Ignore:
                            {
                                //上一次是忽略标记，跳过它
                                offsetIndex = prevResultTmp.findIndex + 1;

                                //本次匹配结束，重置标记
                                resultTmp = null;
                                break;
                            }
                        case AnnoationFlag.None:
                            {
                                //上一次和本次都没有匹配到对象直接退出
                                shouldStopRemove = true;
                                break;
                            }
                        default: shaco.Log.Error("ExtensionsString RemoveAnnotation error: unsupport type=" + resultTmp.flag); shouldStopRemove = true; break;
                    }
                }
            }

            //如果刚好到最后一个字符结束，还没有进行结束标记查找，则默认后面的内容都是要删除的
            if (offsetIndex == retValue.Length && prevResultTmp == null)
            {
                retValue = retValue.Remove(resultTmp.findIndex);
                shouldStopRemove = true;
            }

            //记录上一次匹配标记
            if (null == resultTmp || resultTmp.flag != AnnoationFlag.None)
            {
                //当有强制跳过标记的时候，不重新设置上次查找的标记，因为需要跳过不匹配的标记
                if (!forceIgnoreResult)
                {
                    prevResultTmp = resultTmp;
                }
            }

        } while (!shouldStopRemove && (offsetIndex >= 0 && offsetIndex < retValue.Length));

        return retValue;
    }

    static public System.Collections.Generic.List<string> SplitWithoutTransfer(this string str, string start, string end)
    {
        var retValue = new System.Collections.Generic.List<string>();

        int moveOffset = 0;
        int indexStart = -1;
        int indexEnd = -1;
        string findString = string.Empty;

        if (null == start)
            start = string.Empty;
        if (null == end)
            end = string.Empty;

        do
        {
            indexStart = string.IsNullOrEmpty(start) ? 0 : str.IndexOfWithoutTransfer(start, moveOffset);
            if (indexStart < 0 || indexStart >= str.Length - 1)
                break;

            indexEnd = string.IsNullOrEmpty(end) ? str.Length - 1 : str.IndexOfWithoutTransfer(end, indexStart + 1);
            if (indexEnd < 0)
                break;

            findString = str.Substring(indexStart + start.Length, indexEnd - indexStart - start.Length + (!string.IsNullOrEmpty(end) ? 0 : 1));
            retValue.Add(findString);

            moveOffset = indexEnd + 1;

        } while (moveOffset < str.Length);

        return retValue;
    }

    /// <summary>
    /// 将字符串拆分为长度相等的多段
    /// <param name="str">字符串对象</param>
    /// <param name="perLength">每段字符串长度</param>
    /// <return>等长拆分后的字符串组</return>
    /// </summary>
    static public string[] Split(this string str, int perLength)
    {
        var retValue = new string[0];
        if (string.IsNullOrEmpty(str))
            return retValue;

        int splitCount = str.Length / perLength;
        if (str.Length % perLength != 0)
            splitCount += 1;

        if (splitCount <= 1)
        {
            retValue = new string[] { str };
        }
        else
        {
            retValue = new string[splitCount];

            int splitCountFront = splitCount - 1;
            for (int i = 0; i < splitCountFront; ++i)
            {
                retValue[i] = str.Substring(i * perLength, perLength);
            }
            retValue[splitCountFront] = str.Substring(splitCountFront * perLength, str.Length - splitCountFront * perLength);
        }

        return retValue;
    }

    static public string RemoveRangeIfHave(this string str, int start, int end, string find)
    {
        //safe param
        if (start > end)
        {
            shaco.Base.Log.Exception("string RemoveRangeIfHave exception: start > end, start=" + start + " end=" + end);
            return str;
        }

        if (start < 0) start = 0;
        if (end > str.Length - 1) end = str.Length - 1;

        int findIndex = start;
        while (true)
        {
            findIndex = str.IndexOf(find, findIndex);
            if (findIndex < 0 || findIndex > end)
                break;
            else
            {
                str = str.Remove(findIndex, find.Length);
            }
        }
        return str;
    }

    static public string[] Split(this string str, string pattern)
    {
        if (pattern.Length == 1)
            return str.Split(pattern[0]);
        else
            return str.Split(new string[] { pattern }, System.StringSplitOptions.None);
    }

    static public string Replace(this string str, string newStr, int startIndex, int length)
    {
        str = str.Remove(startIndex, length);
        return str.Insert(startIndex, newStr);
    }

    static public string ReplaceFromBegin(this string str, string oldValue, string newValue, int maxReplaceCount)
    {
        int replaceCountTmp = 0;
        int findIndex = 0;
        do
        {

            findIndex = str.IndexOf(oldValue, findIndex);
            if (findIndex < 0)
                break;
            else
            {
                str = str.Remove(findIndex, oldValue.Length);
                str = str.Insert(findIndex, newValue);
                ++replaceCountTmp;
            }

        } while (replaceCountTmp < maxReplaceCount);

        return str;
    }

    static public string ReplaceFromEnd(this string str, string oldValue, string newValue, int maxReplaceCount)
    {
        int replaceCountTmp = 0;
        int findIndex = str.Length;
        do
        {

            findIndex = str.LastIndexOf(oldValue, findIndex);
            if (findIndex < 0)
                break;
            else
            {
                str = str.Remove(findIndex, oldValue.Length);
                str = str.Insert(findIndex, newValue);
                ++replaceCountTmp;
            }

        } while (replaceCountTmp < maxReplaceCount);

        return str;
    }

    static public string RemoveFront(this string str, string find)
    {
        if (string.IsNullOrEmpty(find))
            return str;

        int findIndex = str.IndexOf(find);
        if (findIndex >= 0)
        {
            str = str.Substring(findIndex + find.Length);
        }

        return str;
    }

    static public string RemoveBehind(this string str, string find)
    {
        if (string.IsNullOrEmpty(find))
            return str;

        int findIndex = str.LastIndexOf(find);
        if (findIndex >= 0)
        {
            str = str.Substring(0, findIndex);
        }
        return str;
    }

    static public string ToSplit(this string[] strs, string split)
    {
        var retValue = string.Empty;
        int length = strs.Length - 1;
        for (int i = 0; i < length; ++i)
        {
            retValue += (strs[i] + split);
        }

        retValue += (strs[strs.Length - 1]);
        return retValue;
    }

    /// <summary>
    /// 连接路径，自动添加路径分隔符
    /// <param name="path">路径</param>
    /// <param name="other">连接路径</param>
    /// <return>连接后的路径</return>
    /// </summary>
    static public string ContactPath(this string path, string other)
    {
        return shaco.Base.FileHelper.ContactPath(path, other);
    }

    /// <summary>
    /// 移除路径层级
    /// <param name="path">路径</param>
    /// <param name="pathLevel">移除层级</param>
    /// <return>移除层级后的路径</return>
    /// </summary>
    static public string RemoveLastPathByLevel(this string path, int pathLevel)
    {
        return shaco.Base.FileHelper.RemoveLastPathByLevel(path, pathLevel);
    }

    /// <summary>
    /// 拆分字符串
    /// <param name="str">字符串</param>
    /// <param name="start">拆分起始标志</param>
    /// <param name="end">拆分终止标志</param>
    /// <param name="offsetIndex">下标偏移位置</param>
    /// <return>获取拆分后的字符串，不包含起始和终止标志</return>
    /// </summary>
    static public string Substring(this string str, string start, string end, int offsetIndex = 0)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var indexFindStart = string.IsNullOrEmpty(start) ? 0 : str.IndexOf(start, offsetIndex);
        if (indexFindStart < 0)
        {
            return string.Empty;
        }

        var indexFindEnd = string.IsNullOrEmpty(end) ? str.Length - 1 : str.IndexOf(end, indexFindStart + start.Length);
        if (indexFindEnd < 0)
        {
            return string.Empty;
        }

        //查找下标超出范围
        if (indexFindStart > indexFindEnd)
            return string.Empty;

        int subLength = indexFindEnd - indexFindStart - start.Length + (string.IsNullOrEmpty(end) ? 1 : 0);
        if (subLength < 0)
        {
            throw new System.Exception("ExtensionsString Substring error: str=" + str + " startIndex=" + indexFindStart + " endIndex=" + indexFindEnd + " start=" + start + " end=" + end);
        }
        else if (subLength == 0)
        {
            return string.Empty;
        }
        else
        {
            return str.Substring(indexFindStart + start.Length, subLength);
        }
    }

    /// <summary>
    /// 如果要添加在字符串在目标字符串的尾部没有，则添加上
    /// <param name="str">字符串</param>
    /// <param name="addString">准备添加到末尾的字符串</param>
    /// <return></return>
    /// </summary>
    static public string AddBehindNotContains(this string str, string addString)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }
        else
        {
            int findIndex = str.LastIndexOf(addString);
            if (findIndex == str.Length - addString.Length)
            {
                return str;
            }
            else
            {
                return str + addString;
            }
        }
    }

    /// <summary>
    /// 递归分割字符串，直到使用完毕分割符
    /// <param name="str">字符串</param>
    /// <param name="flag">分隔符号</param>
    /// <return>分割完毕的字符串列表</return>
    /// </summary>
    static public System.Collections.Generic.List<string> SplitLineStringRecursive(this string str, System.Collections.Generic.List<string> flag)
    {
        var retValue = new System.Collections.Generic.List<string>();

        //当没有分割符的时候，默认lineString就是最终的字符串了
        if (flag.IsNullOrEmpty())
        {
            retValue.Add(str);
            return retValue;
        }

        bool isNoSplitString = true;
        for (int i = flag.Count - 1; i >= 0; --i)
        {
            var splitTmp = str.Split(flag[i]).ToArrayList();

            //去除只有空格的文本
            for (int j = 0; j < splitTmp.Count; ++j)
            {
                if (string.IsNullOrEmpty(splitTmp[j].Trim()))
                {
                    splitTmp.RemoveAt(j);
                }
            }

            //还分割出多余的字符串的时候，继续进行递归分割
            if (splitTmp.Count > 1)
            {
                //删除已经使用过的分隔符
                var flagTmp = new System.Collections.Generic.List<string>(flag);

                //递归分割
                for (int j = 0; j < splitTmp.Count; ++j)
                {
                    retValue.AddRange(SplitLineStringRecursive(splitTmp[j], flagTmp));
                }

                isNoSplitString = false;
            }
        }

        //所有分隔符用完了，也只分割出了1个字符串，则为最终字符串
        if (isNoSplitString)
        {
            retValue.Add(str);
        }
        return retValue;
    }
}