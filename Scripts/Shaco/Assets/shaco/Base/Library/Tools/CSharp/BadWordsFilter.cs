using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace shaco.Base
{
    public class BadWordsFilter
    {
        static private List<shaco.Base.ToolGoodWords.IllegalWordsSearch> wordsSearchs = new List<shaco.Base.ToolGoodWords.IllegalWordsSearch>();
        static private readonly int PRE_LOAD_COUNT = 2000;

        static public void LoadFromFile(string path, System.Action<float> loadProgress = null, string splitFlag = "\n")
        {
            LoadFromString(FileHelper.ReadAllByUserPath(path.Replace("\\", FileDefine.PATH_FLAG_SPLIT)), loadProgress, splitFlag);
        }

        static public void LoadFromString(string readString, System.Action<float> loadProgress = null, string splitFlag = "\n")
        {
            if (string.IsNullOrEmpty(readString))
            {
                Log.Info("ShieldWords LoadFromString error: readString is empty");
                return;
            }

            wordsSearchs.Clear();
            readString = readString.Replace("\r", string.Empty);
            var splitStrings = readString.Split(splitFlag).ToArrayList();

            if (null == loadProgress)
            {
                for (int i = 0; i < splitStrings.Count;)
                {
                    i = UpdateSearchKeywords(splitStrings, i);
                }
            }
            else 
            {
                int index = 0;
                int count = splitStrings.Count / PRE_LOAD_COUNT + (splitStrings.Count % PRE_LOAD_COUNT != 0 ? 1 : 0);
                shaco.Base.Coroutine.ForeachCountAsync(count, ()=>
                {
                    index = UpdateSearchKeywords(splitStrings, index);
                    return true;
                }, loadProgress);
            }

            //屏蔽字库会消耗比较大的临时内存，需要最后回收一下
            System.GC.Collect();
        }

        /// <summary>
        /// 刷新并设置查找字典
        /// </summary>
        static private int UpdateSearchKeywords(List<string> splitStrings, int startIndex)
        {
            int endIndex = startIndex + PRE_LOAD_COUNT;
            if (endIndex > splitStrings.Count - 1)
            {
                endIndex = splitStrings.Count - 1;
            }

            var listTmp = splitStrings.GetRange(startIndex, endIndex - startIndex + 1);

            var newSearch = new shaco.Base.ToolGoodWords.IllegalWordsSearch();
            newSearch.SetKeywords(listTmp);
            wordsSearchs.Add(newSearch);
            return endIndex + 1;
        }

        /// <summary>
        /// 检查输入内容是否包含脏词（包含返回true）
        /// </summary>
        public static bool HasBadWords(string raw)
        {
            foreach (var iter in wordsSearchs)
            {
                if (iter.ContainsAny(raw))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 脏词替换成*号
        /// </summary>
        public static string Filter(string raw, char replacement = '*')
        {
            var retValue = raw;
            foreach (var iter in wordsSearchs)
            {
                retValue = iter.Replace(retValue);
            }
            return retValue;
        }

        /// <summary>
        /// 获取内容中含有的脏词
        /// </summary>
        public static List<shaco.Base.ToolGoodWords.IllegalWordsSearchResult> GetBadWords(string raw)
        {
            var retValue = new List<shaco.Base.ToolGoodWords.IllegalWordsSearchResult>();
            foreach (var iter in wordsSearchs)
            {
                retValue.AddRange(iter.FindAll(raw));
            }
            return retValue;
        }
    }
}