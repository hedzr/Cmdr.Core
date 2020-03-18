using System.Collections.Generic;
using System.ComponentModel;

namespace HzNS.MdxLib.models
{
    [TypeConverter(typeof(KwIndexConverter)), Description("展开以查看应用程序的拼写选项。")]
    public class KwIndex2
    {
        public ulong RelOffsetUL { get; set; }

        //public ulong X2 { get; set; }
        public string Keyword { get; set; }
        public int CIIndex { get; set; }
        public ulong CIUncompOffset { get; set; }

        public ulong CIUncompLength { get; set; }

        //public int ContentBlockIndex { get; set; }
        //public ulong RelOffset { get; set; }
        //public ulong Length { get; set; }
        public KwIndex2()
        {
            CIIndex = -1;
        }

        public override string ToString()
        {
            return string.Format("{0}({1}/0x{1:X06}). #{3}, ofs={4}, len={5}", Keyword, RelOffsetUL, 0, CIIndex,
                CIUncompOffset, CIUncompLength);
        }
    }


    [TypeConverter(typeof(KwIndexConverter)), Description("展开以查看应用程序的拼写选项。")]
    public class KwIndex2List : List<KwIndex2>
    {
    }
}