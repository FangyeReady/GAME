﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace shaco.Base.ToolGoodWords
{
    public class IllegalWordsSearchResult
    {
        internal IllegalWordsSearchResult(string keyword, int start, int end, string srcText)
        {
            Keyword = keyword;
            Success = true;
            End = end;
            Start = start;
            SrcString = srcText.Substring(Start, end - Start + 1);
            BlacklistType = BlacklistType.None;
        }
        internal IllegalWordsSearchResult(string keyword, int start, int end, string srcText, BlacklistType type)
        {
            Keyword = keyword;
            Success = true;
            End = end;
            Start = start;
            SrcString = srcText.Substring(Start, end - Start + 1);
            BlacklistType = type;
        }

        private IllegalWordsSearchResult()
        {
            Success = false;
            Start = 0;
            End = 0;
            SrcString = null;
            Keyword = null;
        }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// 开始位置
        /// </summary>
        public int Start { get; private set; }
        /// <summary>
        /// 结束位置
        /// </summary>
        public int End { get; private set; }
        /// <summary>
        /// 原始文本
        /// </summary>
        public string SrcString { get; private set; }
        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; private set; }

        /// <summary>
        /// 黑名单类型
        /// </summary>
        public BlacklistType BlacklistType { get; private set; }

        public static IllegalWordsSearchResult Empty { get { return new IllegalWordsSearchResult(); } }


        public override string ToString()
        {
            return Start.ToString() + "|" + SrcString;
        }
    }
}
