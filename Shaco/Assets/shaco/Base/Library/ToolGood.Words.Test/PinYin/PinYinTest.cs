using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using shaco.Base.ToolGoodWords;

namespace shaco.Base.ToolGoodWords.Test.PinYin
{
    
    public class PinYinTest
    {
        
        public void GetPinYin()
        {
            var list = WordsHelper.GetAllPinYin('㘄');
            list = WordsHelper.GetAllPinYin('䉄');
            list = WordsHelper.GetAllPinYin('䬋');
            list = WordsHelper.GetAllPinYin('䮚');
            list = WordsHelper.GetAllPinYin('䚏');
            list = WordsHelper.GetAllPinYin('㭁');
            list = WordsHelper.GetAllPinYin('䖆');

            foreach (var iter in list)
            {
                Log.Info(iter);
            }
        }


    }
}
