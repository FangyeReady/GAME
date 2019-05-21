using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;

namespace shaco.Base.ToolGoodWords.Test
{
    
    class SearchTextTest
    {
        
        public void test()
        {
            string s = "中国|国人|zg人";
            string test = "我是中国人";

            StringSearch iwords = new StringSearch();
            iwords.SetKeywords(s.Split('|'));

            var b = iwords.ContainsAny(test);
            shaco.Base.Utility.AssetAreEqual(true, b);


            var f = iwords.FindFirst(test);
            shaco.Base.Utility.AssetAreEqual("中国", f);



            var all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中国", all[0]);
            shaco.Base.Utility.AssetAreEqual("国人", all[1]);
            shaco.Base.Utility.AssetAreEqual(2, all.Count);

            var str = iwords.Replace(test, '*');
            shaco.Base.Utility.AssetAreEqual("我是***", str);


        }
        
        public void test2()
        {
            string s = "中国人|中国|国人|zg人|我是中国人|我是中国|是中国人";
            string test = "我是中国人";

            StringSearch iwords = new StringSearch();
            iwords.SetKeywords(s.Split('|'));



            var all = iwords.FindAll(test);

            shaco.Base.Utility.AssetAreEqual(6, all.Count);

            var str = iwords.Replace(test, '*');
            shaco.Base.Utility.AssetAreEqual("*****", str);


        }
        
        public void test3()
        {
            string s = "中国|国人|zg人";
            string test = "我是中国人";

            StringSearchEx iwords = new StringSearchEx();
            iwords.SetKeywords(s.Split('|').ToList());

            var b = iwords.ContainsAny(test);
            shaco.Base.Utility.AssetAreEqual(true, b);


            var f = iwords.FindFirst(test);
            shaco.Base.Utility.AssetAreEqual("中国", f);



            var all = iwords.FindAll(test);
            shaco.Base.Utility.AssetAreEqual("中国", all[0]);
            shaco.Base.Utility.AssetAreEqual("国人", all[1]);
            shaco.Base.Utility.AssetAreEqual(2, all.Count);

            var str = iwords.Replace(test, '*');
            shaco.Base.Utility.AssetAreEqual("我是***", str);


        }
        
        public void test4()
        {
            string s = "中国人|中国|国人|zg人|我是中国人|我是中国|是中国人";
            string test = "我是中国人";

            StringSearchEx iwords = new StringSearchEx();
            iwords.SetKeywords(s.Split('|').ToList());



            var all = iwords.FindAll(test);

            shaco.Base.Utility.AssetAreEqual(6, all.Count);

            var str = iwords.Replace(test, '*');
            shaco.Base.Utility.AssetAreEqual("*****", str);


        }
    }
}
