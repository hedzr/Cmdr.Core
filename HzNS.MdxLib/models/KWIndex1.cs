using System.Collections.Generic;
using System.ComponentModel;

namespace HzNS.MdxLib.models
{
    [TypeConverter(typeof(KwIndexConverter)), Description("展开以查看应用程序的拼写选项。")]
    public class KwIndex1
    {
        /// <summary>
        /// 条目数量
        /// </summary>
        public ulong CountOfEntries { get; set; }

        //public string Keyword { get; set; }
        public ulong CompressedSize { get; set; }
        public ulong UncompressedSize { get; set; }

        /// <summary>
        /// 起始关键字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 结束关键字
        /// </summary>
        public string KeywordEnd { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}({4} items, {2}/{3} bytes)", Keyword, KeywordEnd, CompressedSize,
                UncompressedSize, CountOfEntries);
        }
    }

    public class KwIndexMap : Dictionary<string, KwIndex1>
    {
    }

    [TypeConverter(typeof(KwIndexConverter)), Description("展开以查看应用程序的拼写选项。")]
    public class KwIndexList : List<KwIndex1>
    {
    }
}