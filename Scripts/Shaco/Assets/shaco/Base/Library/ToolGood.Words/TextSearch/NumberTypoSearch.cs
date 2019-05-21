﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using shaco.Base.ToolGoodWords.internals;

namespace shaco.Base.ToolGoodWords
{
    /// <summary>
    /// 数字错字搜索
    /// 目前未针对❿这些符号进行转化,
    /// 建议：先对❿等符号强制转化成数字，然后使用本类
    /// </summary>
    public class NumberTypoSearch : StringTypoSearch
    {
        #region 静态方法
        private static Dictionary<char, string> toWordDict;

        /// <summary>
        /// 对❿等符号强制转化成数字
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ReplaceNumberSymbol(string src)
        {
            if (toWordDict == null) {
                toWordDict = new Dictionary<char, string>();
                var texts = "㊉ 10\r㈩ 10\r➉ 10\r➓ 10\r" +
                "❿ 10\r⓫ 11\r⓬ 12\r⓭ 13\r⓮ 14\r⓯ 15\r⓰ 16\r⓱ 17\r⓲ 18\r⓳ 19\r⓴ 20\r" +
                "⑽ 10\r⑾ 11\r⑿ 12\r⒀ 13\r⒁ 14\r⒂ 15\r⒃ 16\r⒄ 17\r⒅ 18\r⒆ 19\r⒇ 20\r" +
                "⒑ 10\r⒒ 11\r⒓ 12\r⒔ 13\r⒕ 14\r⒖ 15\r⒗ 16\r⒘ 17\r⒙ 18\r⒚ 19\r⒛ 20\r" +
                "⑩ 10\r⑪ 11\r⑫ 12\r⑬ 13\r⑭ 14\r⑮ 15\r⑯ 16\r⑰ 17\r⑱ 18\r⑲ 19\r⑳ 20\r" +
                "㉑ 21\r㉒ 22\r㉓ 23\r㉔ 24\r㉕ 25\r㉖ 26\r㉗ 27\r㉘ 28\r㉙ 29\r㉚ 30\r" +
                "㉛ 31\r㉜ 32\r㉝ 33\r㉞ 34\r㉟ 35\r㊱ 36\r㊲ 37\r㊳ 38\r㊴ 39\r㊵ 40\r" +
                "㊶ 41\r㊷ 42\r㊸ 43\r㊹ 44\r㊺ 45\r㊻ 46\r㊼ 47\r㊽ 48\r㊾ 49\r㊿ 50\r";
                var ts = texts.Split('\r');
                var splits = " \t".ToCharArray();
                foreach (var t in ts) {
                    var sp = t.Split(splits, StringSplitOptions.RemoveEmptyEntries);
                    if (sp.Length < 2) continue;
                    toWordDict[sp[0][0]] = sp[1];
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in src) {
                string t = null;
                if (toWordDict.TryGetValue(item, out t)) {
                    sb.Append(t);
                } else {
                    sb.Append(item);
                }
            }
            return sb.ToString();
        }


        #endregion

        private const string baseTypo =
          "0 0\r1 1\r2 2\r3 3\r4 4\r5 5\r6 6\r7 7\r8 8\r9 9\r" +
          "０ 0\r１ 1\r２ 2\r３ 3\r４ 4\r５ 5\r６ 6\r７ 7\r８ 8\r９ 9\r" +
          "o 0\rO 0\rl 1\r";

        private const string defaultTypo = "⓪ 0\r零 0\rº 0\r₀ 0\r⓿ 0\r○ 0\r〇 0\r" +
                                         "一 1\r二 2\r三 3\r四 4\r五 5\r六 6\r七 7\r八 8\r九 9\r" +
                                         "壹 1\r贰 2\r叁 3\r肆 4\r伍 5\r陆 6\r柒 7\r捌 8\r玖 9\r" +
                                          "— 1\r貮 2\r参 3\r陸 6\r" +
                                         "¹ 1\r² 2\r³ 3\r⁴ 4\r⁵ 5\r⁶ 6\r⁷ 7\r⁸ 8\r⁹ 9\r" +
                                         "₁ 1\r₂ 2\r₃ 3\r₄ 4\r₅ 5\r₆ 6\r₇ 7\r₈ 8\r₉ 9\r" +
                                         "① 1\r② 2\r③ 3\r④ 4\r⑤ 5\r⑥ 6\r⑦ 7\r⑧ 8\r⑨ 9\r" +
                                         "⑴ 1\r⑵ 2\r⑶ 3\r⑷ 4\r⑸ 5\r⑹ 6\r⑺ 7\r⑻ 8\r⑼ 9\r" +
                                         "⒈ 1\r⒉ 2\r⒊ 3\r⒋ 4\r⒌ 5\r⒍ 6\r⒎ 7\r⒏ 8\r⒐ 9\r" +
                                         "❶ 1\r❷ 2\r❸ 3\r❹ 4\r❺ 5\r❻ 6\r❼ 7\r❽ 8\r❾ 9\r" +
                                         "➀ 1\r➁ 2\r➂ 3\r➃ 4\r➄ 5\r➅ 6\r➆ 7\r➇ 8\r➈ 9\r" +
                                         "➊ 1\r➋ 2\r➌ 3\r➍ 4\r➎ 5\r➏ 6\r➐ 7\r➑ 8\r➒ 9\r" +
                                         "㈠ 1\r㈡ 2\r㈢ 3\r㈣ 4\r㈤ 5\r㈥ 6\r㈦ 7\r㈧ 8\r㈨ 9\r" +
                                         "⓵ 1\r⓶ 2\r⓷ 3\r⓸ 4\r⓹ 5\r⓺ 6\r⓻ 7\r⓼ 8\r⓽ 9\r" +
                                         "㊀ 1\r㊁ 2\r㊂ 3\r㊃ 4\r㊄ 5\r㊅ 6\r㊆ 7\r㊇ 8\r㊈ 9\r";

        /// <summary>
        ///  特殊数字符号转成常用数字
        ///  数字同音汉字转成常用数字
        ///  符号【点】转【.】
        /// --------------------以下为【数字同音汉字转成常用数字】-------------
        ///  "yi": "一以已意议义益亿易医艺食依移衣异伊仪宜射遗疑毅谊亦疫役忆抑尾乙译翼蛇溢椅沂泄逸蚁夷邑怡绎彝裔姨熠贻矣屹颐倚诣胰奕翌疙弈轶蛾驿壹猗臆弋铱旖漪迤佚翊诒怿痍懿饴峄揖眙镒仡黟肄咿翳挹缢呓刈咦嶷羿钇殪荑薏蜴镱噫癔苡悒嗌瘗衤佾埸圯舣酏劓",
        ///  "er": "而二尔儿耳迩饵洱贰铒珥佴鸸鲕",
        ///  "san": "三参散伞叁糁馓毵",
        ///  "shan": "山单善陕闪衫擅汕扇掺珊禅删膳缮赡鄯栅煽姗跚鳝嬗潸讪舢苫疝掸膻钐剡蟮芟埏彡骟",
        ///  "si": "司四思斯食私死似丝饲寺肆撕泗伺嗣祀厮驷嘶锶俟巳蛳咝耜笥纟糸鸶缌澌姒汜厶兕",
        ///  "shi": "是时实事市十使世施式势视识师史示石食始士失适试什泽室似诗饰殖释驶氏硕逝湿蚀狮誓拾尸匙仕柿矢峙侍噬嗜栅拭嘘屎恃轼虱耆舐莳铈谥炻豕鲥饣螫酾筮埘弑礻蓍鲺贳",
        ///  "wu": "务物无五武午吴舞伍污乌误亡恶屋晤悟吾雾芜梧勿巫侮坞毋诬呜钨邬捂鹜兀婺妩於戊鹉浯蜈唔骛仵焐芴鋈庑鼯牾怃圬忤痦迕杌寤阢",
        ///  "liu": "流刘六留柳瘤硫溜碌浏榴琉馏遛鎏骝绺镏旒熘鹨锍",
        ///  "qi": "企其起期气七器汽奇齐启旗棋妻弃揭枝歧欺骑契迄亟漆戚岂稽岐琦栖缉琪泣乞砌祁崎绮祺祈凄淇杞脐麒圻憩芪伎俟畦耆葺沏萋骐鳍綦讫蕲屺颀亓碛柒啐汔綮萁嘁蛴槭欹芑桤丌蜞",
        ///  "ba": "把八巴拔伯吧坝爸霸罢芭跋扒叭靶疤笆耙鲅粑岜灞钯捌菝魃茇",
        ///  "jiu": "就究九酒久救旧纠舅灸疚揪咎韭玖臼柩赳鸠鹫厩啾阄桕僦鬏",
        ///  "ling": "领令另零灵龄陵岭凌玲铃菱棱伶羚苓聆翎泠瓴囹绫呤棂蛉酃鲮柃",
        ///  "lin": "林临邻赁琳磷淋麟霖鳞凛拎遴蔺吝粼嶙躏廪檩啉辚膦瞵懔",
        /// 注：
        /// 多音字类型一：食（转成4）、栅（转成3）、似（转成4）、俟、耆
        /// 多音字类型二：射、尾、蛇、泄、疙、蛾、诒、嗌、参(转成3）、单、掺、禅、掸、剡、彡(转成3)、纟、糸、泽、殖、
        ///     硕、匙、峙（转成4）、嘘、酾、亡、恶、於、唔(转成5)、碌、揭、枝、亟、稽、缉、伎、啐、丌、伯、棱
        /// 特列汉字：删（不转化）、十、拾
        /// </summary>
        private const string miniPinyin =
                "以 1\r已 1\r意 1\r议 1\r义 1\r益 1\r亿 1\r易 1\r医 1\r艺 1\r依 1\r移 1\r衣 1\r异 1\r伊 1\r仪 1\r宜 1\r遗 1\r疑 1\r毅 1\r谊 1\r亦 1\r疫 1\r役 1\r忆 1\r抑 1\r乙 1\r译 1\r翼 1\r溢 1\r椅 1\r沂 1\r逸 1\r蚁 1\r夷 1\r邑 1\r绎 1\r彝 1\r裔 1\r姨 1\r矣 1\r屹 1\r颐 1\r倚 1\r诣 1\r胰 1\r翌 1\r臆 1\r铱 1\r揖 1\r肄 1\r" +
                "而 2\r尔 2\r儿 2\r耳 2\r饵 2\r洱 2\r" +
                "散 3\r伞 3\r山 3\r善 3\r陕 3\r闪 3\r衫 3\r擅 3\r汕 3\r扇 3\r珊 3\r删 3\r膳 3\r缮 3\r赡 3\r煽 3\r苫 3\r" +
                "司 4\r思 4\r斯 4\r私 4\r死 4\r丝 4\r饲 4\r寺 4\r撕 4\r伺 4\r嗣 4\r嘶 4\r巳 4\r是 4\r时 4\r实 4\r事 4\r市 4\r使 4\r世 4\r施 4\r式 4\r势 4\r视 4\r识 4\r师 4\r史 4\r示 4\r石 4\r始 4\r士 4\r失 4\r适 4\r试 4\r什 4\r室 4\r诗 4\r饰 4\r释 4\r驶 4\r氏 4\r逝 4\r湿 4\r蚀 4\r狮 4\r誓 4\r尸 4\r仕 4\r柿 4\r矢 4\r侍 4\r噬 4\r嗜 4\r拭 4\r屎 4\r恃 4\r虱 4\r" +
                "务 5\r物 5\r无 5\r武 5\r午 5\r吴 5\r舞 5\r污 5\r乌 5\r误 5\r屋 5\r晤 5\r悟 5\r吾 5\r雾 5\r芜 5\r梧 5\r勿 5\r巫 5\r侮 5\r坞 5\r毋 5\r诬 5\r呜 5\r钨 5\r捂 5\r戊 5\r" +
                "流 6\r刘 6\r留 6\r柳 6\r瘤 6\r硫 6\r溜 6\r榴 6\r琉 6\r馏 6\r" +
                "企 7\r其 7\r起 7\r期 7\r气 7\r器 7\r汽 7\r奇 7\r齐 7\r启 7\r旗 7\r棋 7\r妻 7\r弃 7\r歧 7\r欺 7\r骑 7\r契 7\r迄 7\r漆 7\r戚 7\r岂 7\r栖 7\r泣 7\r乞 7\r砌 7\r祁 7\r崎 7\r祈 7\r凄 7\r脐 7\r畦 7\r沏 7\r讫 7\r" +
                "把 8\r巴 8\r拔 8\r吧 8\r坝 8\r爸 8\r霸 8\r罢 8\r芭 8\r跋 8\r扒 8\r叭 8\r靶 8\r疤 8\r笆 8\r耙 8\r" +
                "就 9\r究 9\r酒 9\r久 9\r救 9\r旧 9\r纠 9\r舅 9\r灸 9\r疚 9\r揪 9\r咎 9\r韭 9\r臼 9\r厩 9\r" +
                "领 0\r令 0\r另 0\r灵 0\r龄 0\r陵 0\r岭 0\r凌 0\r玲 0\r铃 0\r菱 0\r伶 0\r羚 0\r林 0\r临 0\r邻 0\r赁 0\r琳 0\r磷 0\r淋 0\r霖 0\r鳞 0\r凛 0\r拎 0\r吝 0\r";

        private const string pinyinAppend = "勼 9\r食 4\r栅 3\r似 4\r彡 3\r峙 4\r唔 5\r" +
                "怡 1\r熠 1\r贻 1\r奕 1\r弈 1\r轶 1\r驿 1\r猗 1\r弋 1\r旖 1\r漪 1\r迤 1\r佚 1\r翊 1\r怿 1\r痍 1\r懿 1\r饴 1\r峄 1\r眙 1\r镒 1\r仡 1\r黟 1\r咿 1\r翳 1\r挹 1\r缢 1\r呓 1\r刈 1\r咦 1\r嶷 1\r羿 1\r钇 1\r殪 1\r荑 1\r薏 1\r蜴 1\r镱 1\r噫 1\r癔 1\r苡 1\r悒 1\r瘗 1\r衤 1\r佾 1\r埸 1\r圯 1\r舣 1\r酏 1\r劓 1\r" +
                "迩 2\r铒 2\r珥 2\r佴 2\r鸸 2\r鲕 2\r" +
                "糁 3\r馓 3\r毵 3\r鄯 3\r姗 3\r跚 3\r鳝 3\r嬗 3\r潸 3\r讪 3\r舢 3\r疝 3\r膻 3\r钐 3\r蟮 3\r芟 3\r埏 3\r骟 3\r" +
                "泗 4\r祀 4\r厮 4\r驷 4\r锶 4\r蛳 4\r咝 4\r耜 4\r笥 4\r鸶 4\r缌 4\r澌 4\r姒 4\r汜 4\r厶 4\r兕 4\r轼 4\r舐 4\r莳 4\r铈 4\r谥 4\r炻 4\r豕 4\r鲥 4\r饣 4\r螫 4\r筮 4\r埘 4\r弑 4\r礻 4\r蓍 4\r鲺 4\r贳 4\r" +
                "邬 5\r鹜 5\r兀 5\r婺 5\r妩 5\r鹉 5\r浯 5\r蜈 5\r骛 5\r仵 5\r焐 5\r芴 5\r鋈 5\r庑 5\r鼯 5\r牾 5\r怃 5\r圬 5\r忤 5\r痦 5\r迕 5\r杌 5\r寤 5\r阢 5\r" +
                "浏 6\r遛 6\r鎏 6\r骝 6\r绺 6\r镏 6\r旒 6\r熘 6\r鹨 6\r锍 6\r" +
                "岐 7\r琦 7\r琪 7\r绮 7\r祺 7\r淇 7\r杞 7\r麒 7\r圻 7\r憩 7\r芪 7\r葺 7\r萋 7\r骐 7\r鳍 7\r綦 7\r蕲 7\r屺 7\r颀 7\r亓 7\r碛 7\r汔 7\r綮 7\r萁 7\r嘁 7\r蛴 7\r槭 7\r欹 7\r芑 7\r桤 7\r蜞 7\r" +
                "鲅 8\r粑 8\r岜 8\r灞 8\r钯 8\r菝 8\r魃 8\r茇 8\r" +
                "柩 9\r赳 9\r鸠 9\r鹫 9\r啾 9\r阄 9\r桕 9\r僦 9\r鬏 9\r" +
                "苓 0\r聆 0\r翎 0\r泠 0\r瓴 0\r囹 0\r绫 0\r呤 0\r棂 0\r蛉 0\r酃 0\r鲮 0\r柃 0\r麟 0\r遴 0\r蔺 0\r粼 0\r嶙 0\r躏 0\r廪 0\r檩 0\r啉 0\r辚 0\r膦 0\r瞵 0\r懔 0\r"
            ;

        protected override string GetTypos()
        {
            return baseTypo + defaultTypo + miniPinyin + pinyinAppend;
        }


    }
}
