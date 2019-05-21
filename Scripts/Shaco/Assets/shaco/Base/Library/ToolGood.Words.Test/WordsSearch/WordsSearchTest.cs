using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;

namespace shaco.Base.ToolGoodWords.Test
{
    
    class WordsSearchTest
    {
        
        public void test()
        {
            string s = "中国|国人|zg人";
            string test = "我是中国人";

            WordsSearch wordsSearch = new WordsSearch();
            wordsSearch.SetKeywords(s.Split('|'));

            var b = wordsSearch.ContainsAny(test);
            shaco.Base.Utility.AssetAreEqual(true, b);


            var f = wordsSearch.FindFirst(test);
            shaco.Base.Utility.AssetAreEqual("中国", f.Keyword);

            var alls = wordsSearch.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中国", alls[0].Keyword);
            shaco.Base.Utility.AssetAreEqual(2, alls[0].Start);
            shaco.Base.Utility.AssetAreEqual(3, alls[0].End);
            shaco.Base.Utility.AssetAreEqual(0, alls[0].Index);//返回索引Index,默认从0开始
            shaco.Base.Utility.AssetAreEqual("国人", alls[1].Keyword);
            shaco.Base.Utility.AssetAreEqual(2, alls.Count);

            var t = wordsSearch.Replace (test,'*');
            shaco.Base.Utility.AssetAreEqual("我是***",t);


        }


        
        public void test2()
        {
            string s = "中国|国人|zg人";
            string test = "我是中国人";

            WordsSearchEx wordsSearch = new WordsSearchEx();
            wordsSearch.SetKeywords(s.Split('|').ToList());

            var b = wordsSearch.ContainsAny(test);
            shaco.Base.Utility.AssetAreEqual(true, b);


            var f = wordsSearch.FindFirst(test);
            shaco.Base.Utility.AssetAreEqual("中国", f.Keyword);

            var alls = wordsSearch.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中国", alls[0].Keyword);
            shaco.Base.Utility.AssetAreEqual(2, alls[0].Start);
            shaco.Base.Utility.AssetAreEqual(3, alls[0].End);
            shaco.Base.Utility.AssetAreEqual(0, alls[0].Index);//返回索引Index,默认从0开始
            shaco.Base.Utility.AssetAreEqual("国人", alls[1].Keyword);
            shaco.Base.Utility.AssetAreEqual(2, alls.Count);

            var t = wordsSearch.Replace(test, '*');
            shaco.Base.Utility.AssetAreEqual("我是***", t);


        }

    }
}
