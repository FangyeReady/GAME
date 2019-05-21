//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using PetaTest;
//using System.Diagnostics;

//namespace shaco.Base.ToolGoodWords.Test
//{
//    
//    class PinYinSearchExTest3
//    {
//        //
//        public void SetKeywords()
//        {
//            GC.Collect();
//            PinYinSearchEx3 search = new PinYinSearchEx3(PinYinSearchType.PinYin);
//            search.SetKeywords(ReadFiles());
//            search.SaveFile("keys.dat");
//        }

//        
//        public void Search()
//        {
//            GC.Collect();
//            PinYinSearchEx3 search = new PinYinSearchEx3(PinYinSearchType.PinYin);
//            search.LoadFile("keys.dat");

//            Stopwatch watch = new Stopwatch();
//            watch.Start();
//            for (int i = 0; i < 1000; i++) {
//                var ts = search.SearchTexts("程xy");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("程xuy");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("程xuyuan");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("程xyuan");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("cxy");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("chengxy");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("cxuy");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("cheng序y");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("c序y");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("c序yuan");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));

//                ts = search.SearchTexts("cx员");
//                shaco.Base.Utility.AssetAreEqual(true, ts.Contains("程序员"));
//            }
//            watch.Stop();
//            Trace.Write(watch.ElapsedMilliseconds + "ms");
//        }



//        public static List<string> ReadFiles()
//        {
//            var files = Directory.GetFiles("_texts");
//            HashSet<string> texts = new HashSet<string>();

//            foreach (var file in files) {
//                var ts = File.ReadAllLines(file);
//                var c = ts[0][0];
//                if (c < 0x4e00 || c > 0x9fa5) {
//                    ts = File.ReadAllLines(file, Encoding.Default);
//                }

//                foreach (var t in ts) {
//                    texts.Add(t.Trim());
//                }
//            }
//            return texts.ToList();
//        }
//    }
//}
