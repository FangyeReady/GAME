using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PetaTest;
//using shaco.Base.ToolGoodWords.TextSearch;

namespace shaco.Base.ToolGoodWords.Test
{
    class IllegalWordsTest
    {
        public void IllegalWordsSearchTest()
        {
            string s = "中国|国人|zg人|fuck|all|as|19|http://|ToolGood|assert|zgasser|共产党";
            int[] bl = new int[] {7, 4, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7};
            string test = "我是中国人";


            var iwords = new IllegalWordsSearch();
            iwords.SetKeywords(s.Split('|'));


            var b = iwords.ContainsAny(test);
            shaco.Base.Utility.AssetAreEqual(true, b);


            var f = iwords.FindFirst(test);
            shaco.Base.Utility.AssetAreEqual(true, f.Success);
            shaco.Base.Utility.AssetAreEqual("中国", f.Keyword);
            shaco.Base.Utility.AssetAreEqual(2, f.Start);
            shaco.Base.Utility.AssetAreEqual(3, f.End);



            var all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中国", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("国人", all[1].SrcString);

            test = "共产党";
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("共产党", all[0].SrcString);


            test = "我是中国zg人";
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中国", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("zg人", all[1].SrcString);

            test = "中间国zg人";
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("zg人", all[0].SrcString);

            test = "fuck al[]l"; //未启用跳词
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("fuck", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual(1, all.Count);


            test = "fuck al[]l";
            iwords.UseSkipWordFilter = true; //启用跳词
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("fuck", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("al[]l", all[1].SrcString);
            shaco.Base.Utility.AssetAreEqual(2, all.Count);

            test = "http://ToolGood.com";
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("toolgood", all[0].Keyword); //关键字ToolGood默认转小写
            shaco.Base.Utility.AssetAreEqual("ToolGood", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual(1, all.Count);

            test = "asssert all";
            all = iwords.FindAll(test); //未启用重复词
            shaco.Base.Utility.AssetAreEqual("all", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual(1, all.Count);

            test = "asssert all";
            iwords.UseDuplicateWordFilter = true; //启用重复词
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("asssert", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("assert", all[0].Keyword);
            shaco.Base.Utility.AssetAreEqual("all", all[1].SrcString);
            shaco.Base.Utility.AssetAreEqual(2, all.Count);

            test = "asssert allll"; //重复词匹配到末尾
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("asssert", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("assert", all[0].Keyword);
            shaco.Base.Utility.AssetAreEqual("allll", all[1].SrcString);
            shaco.Base.Utility.AssetAreEqual(2, all.Count);

            test = "zgasssert aallll"; //不会匹配zgasser 或 assert
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("aallll", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("all", all[0].Keyword);
            shaco.Base.Utility.AssetAreEqual(1, all.Count);

            test = "我是【中]国【人";
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中]国", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("国【人", all[1].SrcString);

            test = "我是【中国【人";
            all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中国", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual("国【人", all[1].SrcString);
            shaco.Base.Utility.AssetAreEqual(2, all.Count);


            var ss = iwords.Replace(test, '*');
            shaco.Base.Utility.AssetAreEqual("我是【****", ss);

            test = "我是中国人"; //使用黑名单
            iwords.SetBlacklist(bl);
            iwords.UseBlacklistFilter = true;
            all = iwords.FindAll(test, 1);
            shaco.Base.Utility.AssetAreEqual("中国", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual(1, all.Count);

        }

        
        public void IllegalWordsSearchTest2()
        {
           var t= IllegalWordsVerify.ContainsAny("共产党");
            shaco.Base.Utility.AssetAreEqual(true, t);
        }


        
        public void NumberTypoSearchTest()
        {
            string s = "123456|778899|11";
            // string test = "123456789";

            NumberTypoSearch search = new NumberTypoSearch();
            search.SetKeywords(s.Split('|'));

            var all = search.FindAll("依依");
            shaco.Base.Utility.AssetAreEqual("11", all[0].Keyword);
            shaco.Base.Utility.AssetAreEqual("依依", all[0].SrcString);
            shaco.Base.Utility.AssetAreEqual(1, all.Count);

        }

        
        public void StringTypoSearchTest()
        {
            string s = "http://fanyi.baidu.com/|778899|11";

            StringTypoSearch search = new StringTypoSearch();
            search.SetKeywords(s.Split('|'));

            var all = search.FindAll(" http://fanyi删.bai除du.com/");
            shaco.Base.Utility.AssetAreEqual("http://fanyi.baidu.com/", all[0].Keyword);
            shaco.Base.Utility.AssetAreEqual(1, all.Count);


        }
    }

    public static class IllegalWordsVerify
    {
        private const string keywordsPath = "_Illegal/IllegalKeywords.txt";
        private const string urlsPath = "_Illegal/IllegalUrls.txt";
        private const string infoPath = "_Illegal/IllegalInfo.txt";
        private const string bitPath = "_Illegal/IllegalBit.iws";

        #region GetIllegalWordsSearch

        private static IllegalWordsSearch _search;

        private static IllegalWordsSearch GetIllegalWordsSearch()
        {
            if (_search == null) {
                var ipath = Path.GetFullPath(infoPath);
                if (File.Exists(ipath) == false) {
                    _search = CreateIllegalWordsSearch();
                } else {
                    var texts = File.ReadAllText(ipath).Split('|');
                    if (new FileInfo(Path.GetFullPath(keywordsPath)).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") !=
                        texts[0] ||
                        new FileInfo(Path.GetFullPath(urlsPath)).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") !=
                        texts[1]
                    ) {
                        _search = CreateIllegalWordsSearch();
                    } else {
                        var s = new IllegalWordsSearch();
                        s.Load(Path.GetFullPath(bitPath));
                        _search = s;
                    }
                }
            }
            return _search;
        }

        private static IllegalWordsSearch CreateIllegalWordsSearch()
        {
            var words1 = File.ReadAllLines(Path.GetFullPath(keywordsPath), Encoding.UTF8);
            var words2 = File.ReadAllLines(Path.GetFullPath(urlsPath), Encoding.UTF8);
            var words = new List<string>();
            foreach (var item in words1) {
                words.Add(item.Trim());
            }
            foreach (var item in words2) {
                words.Add(item.Trim());
            }

            var search = new IllegalWordsSearch();
            search.SetKeywords(words);

            search.Save(Path.GetFullPath(bitPath));

            var text = new FileInfo(Path.GetFullPath(keywordsPath)).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") + "|"
                       + new FileInfo(Path.GetFullPath(urlsPath)).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText(Path.GetFullPath(infoPath), text);

            return search;
        }

        #endregion

        public static List<IllegalWordsSearchResult> FindAll(string text)
        {
            var search = GetIllegalWordsSearch();
            return search.FindAll(text);
        }

        public static IllegalWordsSearchResult FindFirst(string text)
        {
            var search = GetIllegalWordsSearch();
            return search.FindFirst(text);
        }

        public static bool ContainsAny(string text)
        {
            var search = GetIllegalWordsSearch();
            return search.ContainsAny(text);
        }

        public static string Replace(string text, char replaceChar = '*')
        {
            var search = GetIllegalWordsSearch();
            return search.Replace(text, replaceChar);
        }

    }
}