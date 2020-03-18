using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using HzNS.MdxLib.MDict.Tool;

namespace HzNS.MdxLib.models
{
    public enum RegisterByEnum
    {
        EMail,
        DeviceId
    }

    /// <summary>
    /// MDict字典文件mdx,mdd使用了这个Unicode Text的Xml文本头部
    /// </summary>
    [XmlRoot("Dictionary")]
    [TypeConverter(typeof(DictionaryXmlHeaderConverter)), Description("展开以查看应用程序的拼写选项。")]
    public class DictionaryXmlHeader
    {
        /// <summary>
        /// 2.0 / 1.2
        /// </summary>
        [XmlAttribute]
        public string GeneratedByEngineVersion { get; set; }

        /// <summary>
        /// 2.0 / 1.2
        /// </summary>
        [XmlAttribute]
        public string RequiredEngineVersion { get; set; }

        /// <summary>
        /// “Html”
        /// </summary>
        [XmlAttribute]
        public string Format { get; set; }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string KeyCaseSensitive
        {
            get => _keyCaseSensitive ? "Yes" : "No"; /*internal*/
            set => _keyCaseSensitive = (value == "Yes");
        }

        private bool _keyCaseSensitive;

        [XmlIgnore]
        public bool KeyCaseSensitiveBool
        {
            get => _keyCaseSensitive;
            set => _keyCaseSensitive = value;
        }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string StripKey
        {
            get => _stripKey ? "Yes" : "No"; /*internal*/
            set => _stripKey = (value == "Yes");
        }

        private bool _stripKey;

        [XmlIgnore]
        public bool StripKeyBool
        {
            get => _stripKey;
            set => _stripKey = value;
        }

        /// <summary>
        /// 0:
        /// 1:
        /// 2: 
        /// 3: 使用一个字典创建者密钥来生成字典时，将设定此值。字典发布者利用此机制可以选择注册码方式发放字典。
        /// </summary>
        [XmlAttribute]
        public string Encrypted { get; set; }

        [XmlIgnore]
        public int EncryptedInt
        {
            get
            {
                if (Encrypted.ToLower() == "yes") throw new Exception("加密方式为Yes，但我们并不能支持该模式！");
                int.TryParse(Encrypted, out var r);
                return r;
            }
        }

        [XmlIgnore] public bool EncryptedBool => EncryptedInt == 3 || EncryptedInt == 2;

        /// <summary>
        /// 字典用户应该采用的注册方式
        /// RegisterBy: "Email", ""
        /// </summary>
        [XmlAttribute]
        public RegisterByEnum RegisterBy { get; set; }

        //[XmlIgnore]
        //public bool RegisterByEmail { get { return RegisterBy == "EMail"; } }
        [XmlAttribute] public string Description { get; set; }
        [XmlAttribute] public string Title { get; set; }

        /// <summary>
        /// "GBK" is default, ...
        /// 
        /// LangMode int:
        /// 1: utf-8
        /// 2: unicode "utf-16LE"
        /// 3: big5
        /// others: gbk
        /// </summary>
        [XmlAttribute]
        public string Encoding { get; set; }

        public Encoding LanguageMode => System.Text.Encoding.GetEncoding(Encoding);

        /// <summary>
        /// 2011-4-11
        /// </summary>
        [XmlAttribute]
        public string CreationDate { get; set; }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string Compact
        {
            get => _compact ? "Yes" : "No"; /*internal*/
            set => _compact = (value == "Yes");
        }

        private bool _compact;

        [XmlIgnore]
        public bool CompactBool
        {
            get => _compact;
            set => _compact = value;
        }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string Compat
        {
            get => _compat ? "Yes" : "No"; /*internal*/
            set => _compat = (value == "Yes");
        }

        private bool _compat;

        [XmlIgnore]
        public bool CompatBool
        {
            get => _compat;
            set => _compat = value;
        }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string Left2Right
        {
            get => _left2Right ? "Yes" : "No"; /*internal*/
            set => _left2Right = (value == "Yes");
        }

        private bool _left2Right;

        [XmlIgnore]
        public bool Left2RightBool
        {
            get => _left2Right;
            set => _left2Right = value;
        }

        [XmlAttribute] public int DataSourceFormat { get; set; }

        [XmlAttribute]
        public string StyleSheet
        {
            get => _styleSheet;
            set => SetStyleSheet(value);
        }

        private string _styleSheet;
        private List<CssEntry> _cssList;

        [XmlIgnore] public List<CssEntry> CssList => _cssList;

        public void SetStyleSheet(string s)
        {
            _styleSheet = s;
            _cssList = new List<CssEntry>();
            if (string.IsNullOrEmpty(s))
                return;

            var lines = s.Split(new[] {"\r\n", "\n", "\r"}, StringSplitOptions.None);
            int ln;
            for (ln = 0; ln < lines.Length;)
            {
                CssEntry e = new CssEntry();
                if (!int.TryParse(lines[ln], out e._index)) break;
                ln++;
                e.Begin = lines[ln++];
                e.End = lines[ln++];
                _cssList.Add(e);
            }
        }
    }

    #region DictionaryXmlHeaderConverter

    public class DictionaryXmlHeaderConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DictionaryXmlHeader))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is DictionaryXmlHeader)
            {
                DictionaryXmlHeader so = (DictionaryXmlHeader) value;
                return so.GeneratedByEngineVersion;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion

    public class CssEntry
    {
        public int _index;

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public string Begin { get; set; }
        public string End { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    public class MDictIndex
    {
        public uint maybeCRC32 { get; set; }
        public ulong CountOfSeg1Indexes { get; set; }
        public ulong TotalEntries { get; set; }
        public ulong Seg1UncompressedSize { get; set; }

        /// <summary>
        /// Seg1Length - 8 是正确的长度，注意此长度应该是小于330000的
        /// Seg1是一组索引：解密解压后为一级索引表。
        /// 紧随Seg1结尾的数据块由一级索引表进行索引，该数据块内容为全部关键字。
        /// </summary>
        public ulong Seg1Length { get; set; }

        /// <summary>
        /// Seg2相对于Seg1结束位置的偏移量。
        /// Seg2是一组索引：二级索引表。
        /// 二级索引表所指向的数据块紧随Seg2的末端，该数据块内容为全部关键字所关联的正文内容。
        /// </summary>
        public ulong Seg2RelOffset { get; set; }

        //public List<long> Values { get; set; }
        //public long Seg1Length { get; set; }
        //public int DataBlockLength { get; set; }
        public uint i8NeverUsed { get; set; }
        public uint MagicNumber { get; set; } //33554422, magic number
        public byte[] Seed { get; set; }

        //[Browsable(false)]
        //public int Count { get { return Values == null ? -1 : Values.Count; } }

        //public long TotalEntries { get { return Values == null || Values.Count != 5 ? 0 : Values[1]; } }
        //public long __s { get { return Values == null || Values.Count != 5 ? 0 : Values[2]; } }
        ///// <summary>
        ///// Seg1Length - 8 是正确的长度，注意此长度应该是小于330000的
        ///// Seg1是一组索引：解密解压后为一级索引表。
        ///// 紧随Seg1结尾的数据块由一级索引表进行索引，该数据块内容为全部关键字。
        ///// </summary>
        //public long Seg1Length { get { return Values == null || Values.Count != 5 ? 0 : Values[3]; } }
        ///// <summary>
        ///// Seg2相对于Seg1结束位置的偏移量。
        ///// Seg2是一组索引：二级索引表。
        ///// 二级索引表所指向的数据块紧随Seg2的末端，该数据块内容为全部关键字所关联的正文内容。
        ///// </summary>
        //public long Seg2Offset { get { return Values == null || Values.Count != 5 ? 0 : Values[4]; } }
    }

    [TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    public class MDictKwIndexTable
    {
        /// <summary>
        /// 加密后的索引块首先被缓存于此；随后解密并解压后的索引块仍然放在这里。
        /// </summary>
        public List<byte> IndexesRawData { get; set; }

        [Browsable(false)] public int RawCount => IndexesRawData?.Count ?? -1;

        public KwIndex1[] IndexList { get; set; }
        public KwIndexMap KwIndexMap { get; set; }
        public KwIndex2[] IndexList2 { get; set; }

        public int TotalEntries => IndexList2?.Length ?? 0;

        /// <summary>
        /// 这个数据结构用于进行快速的起始字符串匹配。
        /// 当用户键入一个字符串序列时，Matcher能够快速地匹配到最接近的一条关键字(从KWIndex2中)，并返回其下标值
        /// </summary>
        public FastRobustMatcher<int> Matcher { get; set; }
    }

    [TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    public class MDictContentIndexTable
    {
        /// <summary>
        /// 决定了解压缩采用什么算法
        /// </summary>
        public uint MagicNumber { get; set; }

        public List<byte> IndexesRawData { get; set; }

        [Browsable(false)] public int RawCount => IndexesRawData?.Count ?? -1;

        /// <summary>
        /// 索引表，每两个long值表示一个索引入口，指向一条正文内容。
        /// </summary>
        //public long[] Indexes { get; set; }
        public ContentIndex[] Indexes { get; set; }

        /// <summary>
        /// 索引个数(Indexes每两个long值代表一个索引项目)
        /// </summary>
        public ulong Count { get; set; }

        public ulong L2 { get; set; }

        /// <summary>
        /// 索引表本身的块长度
        /// </summary>
        public ulong IndexTableLength { get; set; }

        public ulong L4 { get; set; }

        /// <summary>
        /// 索引表尾部的文件偏移量，也即内容块的起始文件偏移
        /// </summary>
        public ulong Seg2ContentBlockOffset { get; set; }
    }

    ///// <summary>
    ///// 压缩块的压缩信息
    ///// </summary>
    //[TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    //public class DictionarySeg3
    //{
    //    public List<int> Values { get; set; }

    //    [Browsable(false)]
    //    public int Count { get { return Values == null ? -1 : Values.Count; } }

    //    public int ZippedLength { get { return Values == null || Values.Count < 13 ? 0 : Values[Values.Count - 4]; } }
    //    public int UnzippedLength { get { return Values == null || Values.Count < 13 ? 0 : Values[Values.Count - 2]; } }
    //}
}