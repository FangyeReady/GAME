using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using shaco.Base.ToolGoodWords;

namespace shaco.Base.ToolGoodWords.Test
{
    
    class WordHelperTest
    {
        
        public void GetPinYin()
        {
            var t = WordsHelper.GetAllPinYin('芃');
            shaco.Base.Utility.AssetAreEqual("Peng", t[0]);

            // var a = WordsHelper.GetPinYinFast("阿");
            // shaco.Base.Utility.AssetAreEqual("A", a);


            var b = WordsHelper.GetPinYin("摩擦棒");
            shaco.Base.Utility.AssetAreEqual("MoCaBang", b);

            b = WordsHelper.GetPinYin("秘鲁");
            shaco.Base.Utility.AssetAreEqual("BiLu", b);



            // var py = WordsHelper.GetPinYinFast("我爱中国");
            // shaco.Base.Utility.AssetAreEqual("WoAiZhongGuo", py);



            var py = WordsHelper.GetPinYin("快乐，乐清");
            shaco.Base.Utility.AssetAreEqual("KuaiLe，YueQing", py);

            py = WordsHelper.GetPinYin("我爱中国");
            shaco.Base.Utility.AssetAreEqual("WoAiZhongGuo", py);

            py = WordsHelper.GetFirstPinYin("我爱中国");
            shaco.Base.Utility.AssetAreEqual("WAZG", py);

            var pys = WordsHelper.GetAllPinYin('传');
            shaco.Base.Utility.AssetAreEqual("Chuan", pys[0]);
            shaco.Base.Utility.AssetAreEqual("Zhuan", pys[1]);


        }


        
        public void HasChinese()
        {
            var b = WordsHelper.HasChinese("xhdsf");
            shaco.Base.Utility.AssetAreEqual(false, b);

            var c = WordsHelper.HasChinese("我爱中国");
            shaco.Base.Utility.AssetAreEqual(true, c);

            var d = WordsHelper.HasChinese("I爱中国");
            shaco.Base.Utility.AssetAreEqual(true, d);
        }
        
        public void ToChineseRMB()
        {
            var t = WordsHelper.ToChineseRMB(12345678901.12);
            shaco.Base.Utility.AssetAreEqual("壹佰贰拾叁億肆仟伍佰陆拾柒萬捌仟玖佰零壹元壹角贰分", t);
        }
        
        public void ToNumber()
        {
            var t = WordsHelper.ToNumber("壹佰贰拾叁億肆仟伍佰陆拾柒萬捌仟玖佰零壹元壹角贰分");
            shaco.Base.Utility.AssetAreEqual((decimal)12345678901.12, t);
        }

        
        public void ToSimplifiedChinese()
        {
            var tw = WordsHelper.ToSimplifiedChinese("壹佰贰拾叁億肆仟伍佰陆拾柒萬捌仟玖佰零壹元壹角贰分");

            shaco.Base.Utility.AssetAreEqual("壹佰贰拾叁亿肆仟伍佰陆拾柒万捌仟玖佰零壹元壹角贰分", tw);
        }
        
        public void ToTraditionalChinese()
        {
            var tw = WordsHelper.ToTraditionalChinese("壹佰贰拾叁亿肆仟伍佰陆拾柒万捌仟玖佰零壹元壹角贰分");
            shaco.Base.Utility.AssetAreEqual("壹佰貳拾叁億肆仟伍佰陸拾柒萬捌仟玖佰零壹元壹角貳分", tw);
        }


        
        public void ToSBC_ToDBC()
        {
            var s = WordsHelper.ToSBC("abcABC123");
            var t = WordsHelper.ToDBC(s);
            shaco.Base.Utility.AssetAreEqual("ａｂｃＡＢＣ１２３", s);
            shaco.Base.Utility.AssetAreEqual("abcABC123", t);


        }

    }
}
