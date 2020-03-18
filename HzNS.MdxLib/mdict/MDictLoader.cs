using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using HzNS.MdxLib.Compression;
using HzNS.MdxLib.Compression.impl;
using HzNS.MdxLib.MDict.Tool;
using HzNS.MdxLib.models;

namespace HzNS.MdxLib.MDict
{
    public class MDictLoader : Loader
    {
        public MDictLoader()
        {
        }

        public MDictLoader(string dictFileName)
        {
            this.doOpen(dictFileName);
        }

        #region Log File

        private FileStream _fsLog;

        protected override void Log(string s)
        {
            if (_fsLog != null)
            {
                var b = Encoding.UTF8.GetBytes(s);
                _fsLog.Write(b, 0, b.Length);
                _fsLog.WriteByte(0x0d);
                _fsLog.WriteByte(0x0a);
            }

            Debug.WriteLine(s);
        }

        protected override void LogString(string s)
        {
            if (_fsLog != null)
            {
                var b = Encoding.UTF8.GetBytes(s);
                _fsLog.Write(b, 0, b.Length);
            }

            Debug.Write(s);
        }

        protected override void ErrorLog(string s)
        {
            if (_fsLog != null)
            {
                var b = Encoding.UTF8.GetBytes(s);
                _fsLog.Write(b, 0, b.Length);
                _fsLog.WriteByte(0x0d);
                _fsLog.WriteByte(0x0a);
            }

            Debug.WriteLine(s);
        }

        #endregion

        public override Loader Open(string dictFileName)
        {
            base.Open(dictFileName);
            this.doOpen(dictFileName);
            return this;
        }

        private void doOpen(string dictFileName)
        {
            base.Open(dictFileName);

            _fsLog?.Dispose();
            _fsLog = File.Create(dictFileName + ".log");

            // this.Process();
        }

        protected override Loader Shutdown()
        {
            base.Shutdown();

            _fsLog?.Dispose();
            _fsLog = null;

            return this;
        }

        private MDictIndex _seg0;
        private MDictKwIndexTable _seg1;
        private MDictContentIndexTable _seg2;

        public override MDictIndex DictIndex
        {
            get => _seg0;
            set => _seg0 = value;
        }

        public override MDictKwIndexTable DictKwIndexTable
        {
            get => _seg1;
            set => _seg1 = value;
        }

        public override MDictContentIndexTable DictLargeContentIndexTable
        {
            get => _seg2;
            set => _seg2 = value;
        }

        /// <summary>
        /// 通过关键字访问条目正文内容时，使用Items中计算出来的索引信息，
        /// 这些信息经过了整合或折算，可以以最快速度访问到目标。
        /// </summary>
        public KwIndex2[] Items => _seg1?.IndexList2;

        /// <summary>
        /// hash排序的快速搜索表，从关键字定位到Items的下标
        /// </summary>
        public Dictionary<string, int> KeywordToItemIndex { get; set; }

        //public override Dictionary<string, DictItem> DictItems { get; set; }

        bool verifyNow = false;

        public override bool TestIntegrity()
        {
            const int bufSize = 32768;
            using var fs = new FileStream(this.DictFileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize);
            try
            {
                Log($"---------- Loading MDX File: {this.DictFileName}.");

                //Xml Header
                LoadXmlHeader(fs);

                if (this.DictHeader.GeneratedByEngineVersion != "2.0" &&
                    this.DictHeader.GeneratedByEngineVersion != "1.2")
                {
                    ErrorLog("这是暂不支持的MDX文件格式版本：" + this.DictHeader.GeneratedByEngineVersion + "。不再继续处理。");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
                throw;
            }

            return true;
        }

        public override bool Process()
        {
            const int count = 32768;
            const int bufSize = 32768;
            //int offset = 0;
            //int readBytes;
            var buf = new byte[count];

            using var fs = new FileStream(this.DictFileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize);
            try
            {
                Log($"---------- Loading MDX File: {this.DictFileName}.");

                //Xml Header
                LoadXmlHeader(fs);

                if (this.DictHeader.GeneratedByEngineVersion != "2.0" &&
                    this.DictHeader.GeneratedByEngineVersion != "1.2")
                {
                    ErrorLog("这是暂不支持的MDX文件格式版本：" + this.DictHeader.GeneratedByEngineVersion + "。不再继续处理。");
                    return false;
                }

                Log($"             ---- LangMode: {this.DictHeader.LanguageMode.EncodingName}");
                Log($"             ---- EngineVersion: {this.DictHeader.GeneratedByEngineVersion}");

                //MDict的根索引表
                LoadKmDictIndex(fs, fs.Name);

                // segment 1
                Log("");
                //读取和解压缩一级索引表
                LoadKwIndex1Table(fs, fs.Name);

                if (walkOnContentIndexTable)
                {
                    var posSeg1BlockStart = fs.Position;
                    //二级索引表
                    if ( //this.DictHeader.GeneratedByEngineVersion != "1.2" &&
                        this.DictIndex.TotalEntries < 630000)
                    {
                        LoadKwIndex2Table(fs, fs.Name);

                        // segment 2
                        LoadContentIndexTable(fs, fs.Name, posSeg1BlockStart);
                    }
                    else
                    {
                        ErrorLog($"   没有装载二级索引表(因条目数太多): {this.DictIndex.TotalEntries} entries.");
                    }
                }

                return true;

                #region ref codes

                //if (DictionaryXmlHeader.EncryptedBool)
                //{
                //    Log("由于MDX注册机制已被启用，此文件无法按照常规方式解码。");
                //}
                //else
                {
                    #region segment 2 error

                    ////seg2len = 38;//example-6.mdx
                    ////seg2len = 41-4;//example-5.mdx
                    ////seg2len = 655-0x48+4;//WIKI-1.mdx
                    ////seg2len = 0;
                    //int seg2pos = (int)fs.Position;
                    //int seg2tag = readInt32(fs);
                    //byte seg2tag_B0 = (byte)(seg2tag >> 24);
                    //MemoryStream msSeg2 = new MemoryStream();
                    //if (_seg2 != null) _seg2 = null;
                    //_seg2 = new MDictContentIndexTable();
                    //_seg2.IndexesRawData = new List<byte>();
                    //while (true)
                    //{
                    //    b1 = (byte)fs.ReadByte();
                    //    if (b1 == seg2tag_B0)
                    //    {
                    //        fs.Seek(-1, SeekOrigin.Current);
                    //        int tag = readInt32(fs);//seg2的结束标记等于seg2的前4个字节
                    //        if (tag == seg2tag)
                    //        {
                    //            msSeg2.WriteByte((byte)(tag >> 24));
                    //            msSeg2.WriteByte((byte)((tag >> 16) & 0xff));
                    //            msSeg2.WriteByte((byte)((tag >> 8) & 0xff));
                    //            msSeg2.WriteByte((byte)(tag & 0xff));

                    //            //tag = readInt(fs);//seg2包含真正的终结符0x02 0x00 0x00 0x00
                    //            //msSeg2.WriteByte((byte)(tag >> 24));
                    //            //msSeg2.WriteByte((byte)((tag >> 16) & 0xff));
                    //            //msSeg2.WriteByte((byte)((tag >> 8) & 0xff));
                    //            //msSeg2.WriteByte((byte)(tag & 0xff));
                    //            break;
                    //        }
                    //        fs.Seek(-3, SeekOrigin.Current);
                    //    }
                    //    msSeg2.WriteByte(b1);
                    //}
                    //Log(string.Format("             ---- SEG #2 Found, {0}/0x{0:X} bytes begin(filepos={1}/0x{1:X}):", msSeg2.Length, seg2pos));
                    //_seg2.IndexesRawData.AddRange(msSeg2.ToArray());
                    //for (x = 0; x < _seg2.RawCount; x++)
                    //{
                    //    b1 = _seg2.IndexesRawData[x];
                    //    int mod8 = x % wrappos;
                    //    if (mod8 == 0) LogString(" ");
                    //    LogString(string.Format(" {0:X2}", b1));
                    //    if (mod8 == (wrappos - 1)) Log("");
                    //}
                    //Log("");

                    #endregion

                    #region segment 3, trunk 1

                    //Log("");
                    //Log(string.Format("             ---- compression information begin(filepos={1}/0x{1:X}):", 0, fs.Position));
                    ////int xMax = 13;//b1 = (byte)fs.ReadByte();
                    //if (_seg3 != null) _seg3 = null;
                    //_seg3 = new DictionarySeg3();
                    //_seg3.Values = new List<int>();
                    ////Log(string.Format("  byte : 0x{0:X2}/{0}", b1));
                    //for (x = 0; ; i++, x++)
                    //{
                    //    int j = readInt32(fs);
                    //    _seg3.Values.Add(j);
                    //    Log(string.Format("  int32 #{0:D2}/#{1:D2}: 0x{2:X8}/{2}", i, x, j));
                    //    //if (x == xMax - 6)
                    //    //    Log(string.Format(" |  Zipped Size: {0}", j));
                    //    //else if (x == xMax - 4)
                    //    //    Log(string.Format(" |  Zipped Size: {0}({1}KB)", j, j / 1024));
                    //    //else if (x == xMax - 2)
                    //    //    Log(string.Format(" |  Unzipped Size: {0}({1}KB)", j, j / 1024));
                    //    //else
                    //    //    Log("");
                    //    if (j == 0x02000000)
                    //        break;
                    //}

                    #endregion

                    #region temp

                    //byte[] trunkData;
                    //string trunkDataFN = this.DictFileName + ".trunk1.ddd";
                    //if (_seg3.ZippedLength > 0 && _seg3.ZippedLength < 65536*2)
                    //{
                    //    trunkData = new byte[_seg3.ZippedLength];
                    //    fs.Read(trunkData, 0, _seg3.ZippedLength);
                    //    using(FileStream fst=File.Create(trunkDataFN)){
                    //        byte[] xxx = new byte[] {
                    //            0x1f, 0x8b, 0x08, 00, //GZIP Magic, CM('DEFLATE'), Flags,
                    //            00, 00, 00, 00, //MTIME
                    //            00, //XFL
                    //            03, //OS:
                    //        };
                    //        byte[] bzip2hdr = new byte[] {
                    //            0x42, 0x5a, 0x68, 0x39, //'BZh9'
                    //            0x31, 0x41, 0x59, 0x26, 0x53, 0x59, //XFL
                    //            0,0,0,0//0x18, //OS:
                    //        };
                    //        //fst.Write(xxx, 0, xxx.Length);
                    //        fst.Write(bzip2hdr, 0, bzip2hdr.Length);
                    //        fst.Write(trunkData, 0, _seg3.ZippedLength);
                    //        //fst.WriteByte((byte)((_seg3.UnzippedLength) & 0xff));
                    //        //fst.WriteByte((byte)((_seg3.UnzippedLength>>8) & 0xff));
                    //        //fst.WriteByte((byte)((_seg3.UnzippedLength>>16) & 0xff));
                    //        //fst.WriteByte((byte)((_seg3.UnzippedLength>>24) & 0xff));
                    //    }
                    //    //CompressUtil.lz77Decompress(trunkDataFN, trunkDataFN + ".un");
                    //    //dictimp.Compression.impl.LZ77.unzipData(trunkDataFN, trunkDataFN + ".un");
                    //    //
                    //    ////ICSharpCode.SharpZipLib.LZW.LzwInputStream;
                    //    //LZWCompression lzw = new LZWCompression();
                    //    //byte[] unzipped = lzw.Decompress(trunkData);
                    //    //unzipped = null;
                    //    //
                    //    //CompressUtil.sharpLzwDecompress(trunkDataFN, trunkDataFN + ".un");
                    //    //
                    //    //CompressUtil.gzipDecompress(trunkDataFN, trunkDataFN + ".ungz");
                    //    //
                    //    #region (7-zip) LZMA解压使用下面的方式可以直接解压内存流(不过如果预定的字典尺寸不适当的话也可能解不开)
                    //    //using (FileStream inStream = new FileStream(trunkDataFN, FileMode.Open, FileAccess.Read, FileShare.Read, 32768))
                    //    //{
                    //    //    using (FileStream outStream = File.Create(trunkDataFN + ".un"))
                    //    //    {
                    //    //        byte[] properties = new byte[5] { 0x5d, 0, 0, 0x80, 0 };
                    //    //        SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
                    //    //        decoder.SetDecoderProperties(properties);
                    //    //        long compressedSize = _seg3.ZippedLength;
                    //    //        long outSize = _seg3.UnzippedLength;

                    //    //        decoder.Code(inStream, outStream, compressedSize, outSize, null);
                    //    //    }
                    //    //}
                    //    #endregion

                    //    using (FileStream inStream = new FileStream(trunkDataFN, FileMode.Open, FileAccess.Read, FileShare.Read, 32768))
                    //    {
                    //        using (FileStream outStream = File.Create(trunkDataFN + ".un"))
                    //        {
                    //            dictimp.Compression.BZip2.BZip2.Decompress(inStream, outStream, false);
                    //        }
                    //    }

                    //    trunkData = null;
                    //}

                    #endregion

                    #region temp zips codes

                    //string origName = CompressUtil.stripPathExt(fs.Name) + ".out";
                    //using (FileStream fsOut = File.Create(origName))
                    //{
                    //    #region old test codes with DeflateStream, failure
                    //    //using (System.IO.Compression.DeflateStream uncompressed = new System.IO.Compression.DeflateStream(fs, System.IO.Compression.CompressionMode.Decompress, true))
                    //    //{
                    //    //    while (true)
                    //    //    {
                    //    //        count = uncompressed.Read(buf, 0, bufSize);
                    //    //        if (count != 0)
                    //    //            fsOut.Write(buf, 0, count);
                    //    //        if (count != bufSize)
                    //    //            break;
                    //    //    }
                    //    //}
                    //    #endregion
                    //    #region old test codes with sharpBzip2, sharpLzw
                    //    //if (!CompressUtil.sharpBzip2Decompress(fs, fsOut)){
                    //    //    throw new Exception("failure lzw decompress.");
                    //    //}
                    //    #endregion
                    //    //LZ77.unzipData(fs, fsOut);
                    //}
                    //#region old codes with simple fs, no use
                    ////while ((readBytes = fs.Read(buf, 0, count)) > 0)
                    ////{
                    ////    Log(string.Format("reading from offset {0}, and {1} bytes read.", offset, readBytes));
                    ////    offset += readBytes;
                    ////}
                    //#endregion
                    //MessageBox.Show(null, "Completed.", "OK");

                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
                throw;
            }
            finally
            {
                // fs.Dispose();
            }
        }

        #region loadXmlHeader

        private void LoadXmlHeader(Stream fs)
        {
            try
            {
                var xmlBytes = xreadInt32(fs);
                if (xmlBytes < 0 || xmlBytes >= 1000000)
                    throw new Exception(string.Format("不合法的头字节长度“{0}(0x{0:X})”. 这不是正确的MDict格式。", xmlBytes));

                var xml = readString(fs, xmlBytes);
                if ("Library_Data".IndexOf(xml, StringComparison.Ordinal) >= 0)
                {
                    this.IsLibraryData = true;
                }

                this.SetDictionaryXmlHeader(xml.TrimEnd('\x0').Replace("Library_Data", "Dictionary"));
                //this.DictHeader.setStyleSheet(this.DictHeader.StyleSheet);
                Log(string.Format("  {0}(0x{0:X}) bytes read for XmlHeader.", xmlBytes));
                //this.DictionaryXmlHeader.GeneratedByEngineVersion = "2.0";
                //string tmp = Util.ToXml(this.DictHeader, typeof(DictionaryXmlHeader));
                //Debug.WriteLine(xml);
                //Debug.WriteLine(string.Format("\n\n"));

                if (this.DictHeader.GeneratedByEngineVersion != "2.0" &&
                    this.DictHeader.GeneratedByEngineVersion != "1.2")
                {
                    ErrorLog("这是暂不支持的MDX文件格式版本：“" + this.DictHeader.GeneratedByEngineVersion + "”。不再继续处理。");
                    throw new Exception("这是暂不支持的MDX文件格式版本：“" + this.DictHeader.GeneratedByEngineVersion + "”。不再继续处理。");
                }

                this.HeaderIsValidate = true;

                if (string.IsNullOrEmpty(this.DictHeader.Encoding))
                    this.DictHeader.Encoding = this.IsLibraryData ? "utf-16LE" : "UTF-8";
                //if (this.DictHeader.LanguageMode == null)
                //    this.DictHeader.LanguageMode = Encoding.UTF8;
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
                throw;
            }
        }

        #endregion

        #region loadMDictIndex

        /// <summary>
        /// MDict 的根索引表
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fsName"></param>
        private void LoadKmDictIndex(Stream fs, string fsName)
        {
            _seg0 = null;
            _seg0 = new MDictIndex {Seed = new byte[4], maybeCRC32 = readUInt32(fs)};

            //_seg0.Values = new List<long>(5);

            //现在知道，13个int是这样组织的：
            //  int     i7          | no used.
            //  long[5] q,r,s,t,u,  |
            //  int     i8,         |
            //  int     magicNumber,| = 0x02000000
            //  byte[4] i10,        | MD4 seed

            if (this.DictHeader.GeneratedByEngineVersion == "2.0")
            {
                this._seg0.CountOfSeg1Indexes = readUInt64(fs);
                this._seg0.TotalEntries = readUInt64(fs);
                this._seg0.Seg1UncompressedSize = readUInt64(fs);
                this._seg0.Seg1Length = readUInt64(fs);
                this._seg0.Seg2RelOffset = readUInt64(fs);
                Log(string.Format("  int32 #00: 0x{1:X16}/{1}", 0, this._seg0.CountOfSeg1Indexes));
                Log(string.Format("  int32 #01: 0x{1:X16}/{1} | TotalEntries", 0, this._seg0.TotalEntries));
                Log(string.Format("  int32 #02: 0x{1:X16}/{1} | Seg1 Uncompressed Size", 0,
                    this._seg0.Seg1UncompressedSize));
                Log(string.Format("  int32 #03: 0x{1:X16}/{1} | Seg1 Compressed Size", 0, this._seg0.Seg1Length));
                Log(string.Format("  int32 #04: 0x{1:X16}/{1} | Seg2 relative Offset", 0, this._seg0.Seg2RelOffset));
                _seg0.i8NeverUsed = readUInt32(fs);
                _seg0.MagicNumber = readUInt32(fs);
                fs.Read(_seg0.Seed, 0, 4);
            }
            else
            {
                this._seg0.CountOfSeg1Indexes = readUInt32(fs);
                this._seg0.TotalEntries = readUInt32(fs);
                this._seg0.Seg1UncompressedSize = readUInt32(fs);
                this._seg0.Seg1Length = 0;
                this._seg0.Seg2RelOffset = readUInt32(fs);
                LogString(string.Format("  int32 #00: 0x{1:X16}/{1}\n", 0, this._seg0.CountOfSeg1Indexes));
                LogString(string.Format("  int32 #01: 0x{1:X16}/{1} | TotalEntries\n", 0, this._seg0.TotalEntries));
                LogString(string.Format("  int32 #02: 0x{1:X16}/{1} | Seg1 Uncompressed Size\n", 0,
                    this._seg0.Seg1UncompressedSize));
                //LogString(string.Format("  int32 #03: 0x{1:X16}/{1} | Seg1 Compressed Size\n", 0, this._seg0.Seg1Length));
                LogString(string.Format("  int32 #04: 0x{1:X16}/{1} | Seg2 relative Offset\n", 0,
                    this._seg0.Seg2RelOffset));
                _seg0.i8NeverUsed = 0;
                _seg0.MagicNumber = 0;
            }

            if (_seg0.MagicNumber == 0x02000000 || this.DictHeader.GeneratedByEngineVersion == "1.2") return;

            ErrorLog($"MDX文件格式错误：期待Magic Number #33554432, 但只有{_seg0.MagicNumber}(0x{_seg0.MagicNumber:8X})");
            throw new Exception(
                $"MDX文件格式错误：期待Magic Number #33554432, 但只有{_seg0.MagicNumber}(0x{_seg0.MagicNumber:8X})");
        }

        #endregion

        #region loadKWIndexTable

        /// <summary>
        /// 根级索引表/一级索引表。
        /// 一级索引表指示每个二级索引表的索引位置，每个二级索引表包含一个关键字集合，从起始关键字到结束关键字。
        /// 因此，一级索引表表项包含：起始关键字，结束关键字，尺寸（压缩前、后），条目数量。
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fsName"></param>
        private void LoadKwIndex1Table(Stream fs, string fsName)
        {
            byte[] tmpIdx;
            if (DictHeader.GeneratedByEngineVersion == "2.0")
            {
                var j1 = (int) (_seg0.Seg1Length - 8); //i1
                var i1 = j1; //i2
                if (j1 < 33000)
                {
                    i1 = 33000; //label1045, @DONE
                }

                Log(string.Format(
                    ">> 提取一级索引表, {0} bytes begin (filepos={1}/0x{1:X}) / Real Length = {2}/0x{2:X}:\r\n\tStartKeyword-EndKeyword,  Count,  Compress Size,  Uncompressed Size",
                    _seg0.Seg1Length, fs.Position, j1));

                _seg1 = null;
                _seg1 = new MDictKwIndexTable {IndexesRawData = new List<byte>()};

                tmpIdx = new byte[j1];
                fs.Read(tmpIdx, 0, j1);

                _seg1.IndexesRawData.AddRange(tmpIdx);

                #region debug

                //{//debug
                //    //for (x = 0; x < j1; x++)
                //    //{
                //    //    b1 = _seg1.Values[x];
                //    //    int mod8 = x % 16;
                //    //    if (mod8 == 0) LogString(" ");
                //    //    //LogString(string.Format(" 0x{0:X2}/{0:D3}", b1));
                //    //    LogString(string.Format(" {0:X2}", b1));
                //    //    if (mod8 == 15) Log("");
                //    //}
                //    write_file(CompressUtil.stripPathExt(fsName) + ".seg1.out.dmp", _seg1.IndexesRawData.ToArray(), j1);
                //}
                //Log("");

                #endregion

                //i += j1;

                //DOING: a(this._seg1Buffer, i2, arrayOfByte1);
                //a(this._seg1Buffer, seg1_RealLength, _seg0.i10);
                if (DictHeader.EncryptedInt == 2)
                {
                    Log("解密根级索引表数据块。");
                    //解密seg1，如果必要的话
                    this.DecryptSeg1(j1);
                }

                #region debug

                if (debugModeEnable)
                {
                    for (var x = 0; x < j1; x++)
                    {
                        var b1 = _seg1.IndexesRawData[x];
                        var mod8 = x % 16;
                        if (mod8 == 0) LogString(" ");
                        //LogString(string.Format(" 0x{0:X2}/{0:D3}", b1));
                        LogString($" {b1:X2}");
                        if (mod8 == 15) Log("");
                    }

                    writeFile(CompressUtil.stripPathExt(fsName) + ".seg1.out.dmp", _seg1.IndexesRawData.ToArray(), j1);
                }

                Log("");

                #endregion

                {
                    var ok = false;

                    #region 解压seg1的index部分

                    var arrayOfByte7 = new byte[_seg0.Seg1UncompressedSize];
                    using (var outs = new MemoryStream(arrayOfByte7))
                    {
                        // CompressUtil.InflaterDecompressBuffer(_seg1.IndexesRawData.ToArray(), 0, j1, outs);
                        CompressUtil.InflateBufferWithPureZlib(_seg1.IndexesRawData.ToArray(), j1, outs);
                    }

                    ok = arrayOfByte7.Length == (int) _seg0.Seg1UncompressedSize;
                    if (!ok)
                    {
                        ErrorLog("对seg1解压(Inflater.inflate)失败，此文件无法按照常规方式解码。");
                        throw new Exception("对seg1解压(Inflater.inflate)失败，此文件无法按照常规方式解码。");
                    }

                    _seg1.IndexesRawData.Clear();
                    _seg1.IndexesRawData.AddRange(arrayOfByte7);

                    #endregion

                    #region debug //DEBUG

                    if (debugModeEnable)
                    {
                        writeFile(CompressUtil.stripPathExt(fsName) + ".seg1.out.decomp.dmp", arrayOfByte7);
                    }

                    #endregion

                    #region 处理解压后的seg1的index部分

                    var list = new KwIndexList();
                    _seg1.KwIndexMap = new KwIndexMap();
                    ParseKwIndexes(_seg1.IndexesRawData, list, _seg1.KwIndexMap);
                    _seg1.IndexList = list.ToArray();

                    #endregion
                }

                Log("");
            }
            else
            {
                Log(string.Format(
                    ">> 提取一级索引表, {0} bytes begin (filepos={1}/0x{1:X}) / Real Length = {2}/0x{2:X}:\r\n\tStartKeyword-EndKeyword,  Count,  Compress Size,  Uncompress Size",
                    _seg0.Seg1Length, fs.Position, _seg0.Seg1UncompressedSize));

                tmpIdx = new byte[_seg0.Seg1UncompressedSize];
                fs.Read(tmpIdx, 0, tmpIdx.Length);

                _seg1 = null;
                _seg1 = new MDictKwIndexTable {IndexesRawData = new List<byte>()};
                _seg1.IndexesRawData.AddRange(tmpIdx);

                #region 处理解压后的seg1的index部分

                var list = new KwIndexList();
                _seg1.KwIndexMap = new KwIndexMap();
                ParseKwIndexes(_seg1.IndexesRawData, list, _seg1.KwIndexMap);
                _seg1.IndexList = list.ToArray();

                #endregion
            }
        }

        #endregion

        #region decryptSeg1

        private void DecryptSeg1(int seg1RealLength)
        {
            //解密seg1，如果必要的话
            //MD4 md4 = new MD4();
            //byte[] arrayOfByte1 = new byte[8];
            //arrayOfByte1[0] = _seg0.i10[0];
            //arrayOfByte1[1] = _seg0.i10[1];
            //arrayOfByte1[2] = _seg0.i10[2];
            //arrayOfByte1[3] = _seg0.i10[3];
            //arrayOfByte1[4] = (byte)0x95;// 0xff95;// 65429 // -107;
            //arrayOfByte1[5] = 0x36;
            //arrayOfByte1[6] = 0;
            //arrayOfByte1[7] = 0;
            //
            //byte[] seeds = md4.GetByteHashFromBytes(arrayOfByte1);
            //
            //seeds = new byte[] { (byte) 0xc5, (byte) 0xde, (byte) 0xeb, (byte) 0xdb, 0x31, (byte) 0xe5, (byte) 0xc6,
            //	(byte) 0xa8, (byte) 0xda, (byte) 0x98, (byte) 0xf0, (byte) 0xb4, (byte) 0xa7, (byte) 0xbd, (byte) 0xc0,
            //	(byte) 0xe9};
            //seeds[0] = 0x47;

            var arrayOfInt1 = new uint[16];

            var n = (uint) (_seg0.Seed[3] & 0xFF) << 24;
            var i1 = (uint) (_seg0.Seed[2] & 0xFF) << 16;
            var i2 = n | i1;
            var i3 = (uint) (_seg0.Seed[1] & 0xFF) << 8;
            var i4 = i2 | i3;
            var i5 = (uint) (_seg0.Seed[0] & 0xFF);
            var i6 = i4 | i5;

            arrayOfInt1[0] = i6;
            arrayOfInt1[1] = 13973; // 0x00003695
            arrayOfInt1[2] = 128; // 0x00000080
            arrayOfInt1[14] = 64; // 0x00000040
            // MD4_d localb = new cn.edu.ustc.qdict.a.b();// MD4算法
            var arrayOfInt2 = MD4_d.mdfour_i(arrayOfInt1);
            //5e1a0447, 0d520bd2, c84bbc15, 2b9ad58f.

            var arrayOfByte1 = new byte[16];
            var i = 0; // j=0
            var i8 = 0; // j4=0
            while (i8 < 4)
            {
                // _L6:
                var byte0 = (byte) (arrayOfInt2[i8] & 0xff);
                arrayOfByte1[i++] = byte0;
                var byte1 = (byte) (arrayOfInt2[i8] >> 8 & 0xff);
                arrayOfByte1[i++] = byte1;
                var byte2 = (byte) (arrayOfInt2[i8] >> 16 & 0xff);
                arrayOfByte1[i++] = byte2;
                var byte3 = (byte) (arrayOfInt2[i8] >> 24 & 0xff);
                arrayOfByte1[i++] = byte3;
                i8++;
            }

            i = 54;
            i4 = 0; //8
            // _L9:
            // k4 = flag.length;
            while (i4 < _seg1.IndexesRawData.Count)
            {
                // break MISSING_BLOCK_LABEL_822;
                var i18 = _seg1.IndexesRawData[(int) i4];
                var i19 = (uint) i4; // -8;
                var i20 = (uint) i ^ i19;
                var i21 = ((uint) i4 /* - 8*/) % 16;
                var i22 = arrayOfByte1[i21];
                var i23 = i20 ^ i22;
                var i24 = (i18 & 0xF0) >> 4;
                var i25 = (i18 & 0xF) * 16;
                var i26 = i24 | i25;
                _seg1.IndexesRawData[(int) i4] = (byte) (i23 ^ i26);
                i4++;
                i = (int) i18;
            }

            i4 = 0; //8

            //int i = 0;
            //byte last_cb = 0x36;
            ////while (j1 < seg1_RealLength)
            ////{
            //do
            //{
            //    if (i >= seg1_RealLength)
            //        return;
            //    
            //    byte cb = _seg1.Values[i];
            //    int k1 = cb << 4 & 0xf0;
            //    byte cb_exchg = (byte)(cb >> 4 & 0xf | k1);
            //
            //    byte seed = seeds[i % 16];
            //    //90 -> 09 ^ seed ^ 36 = 78
            //    byte origin_text = (byte)((byte)((byte)(cb_exchg ^ seed) ^ i) ^ last_cb);
            //    //byte byte10 = _seg1.Values[j1];
            //    last_cb = _seg1.Values[i];
            //    _seg1.Values[i] = origin_text;
            //
            //    i++;
            //    //byte4 = byte10;
            //} while (true);
            //int i5 = 0;
            //int i14;
            //for (int i6 = 54; ; i6 = i14)
            //{
            //    if (i5 >= seg1_RealLength)
            //        break;
            //    int i7 = _seg1.Values[i5];
            //    int i8 = i7 << 4 & 0xF0;
            //    int i9 = (byte)(i7 >> 4 & 0xF | i8);
            //    int x10 = i5 % 16;
            //    int i11 = arrayOfByte2[x10];
            //    int i12 = (byte)((byte)(i9 ^ i11) ^ i5);
            //    int i13 = (byte)(i6 ^ i12);
            //    i14 = _seg1.Values[i5];
            //    _seg1.Values[i5] = (byte)i13;
            //    i5++;
            //}
        }

        #endregion

        #region parseKWIndexes

        /// <summary>
        /// 处理一级索引(TwoString格式的)
        /// </summary>
        /// <param name="list"></param>
        /// <param name="kwIndexes"></param>
        /// <param name="kwIndexMap"></param>
        /// <returns></returns>
        private bool ParseKwIndexes(List<byte> list, ICollection<KwIndex1> kwIndexes, KwIndexMap kwIndexMap)
        {
            var kwIndexTableLength = list.Count;
            var kwIndexTable = list.ToArray();
            var e_g = this.DictHeader.LanguageMode;
            var engineVersion = float.Parse(this.DictHeader.GeneratedByEngineVersion);

            var index = 0;
            while (index < kwIndexTableLength)
            {
                var kwi = new KwIndex1();
                if (this.DictHeader.GeneratedByEngineVersion == "2.0")
                {
                    kwi.CountOfEntries = readUInt64(kwIndexTable, index);
                    index += 8;
                    var i16 = readUInt16(kwIndexTable, index);
                    index += 2;
                    if (i16 > 0)
                    {
                        if (Equals(e_g, Encoding.Unicode))
                        {
                            i16 *= 2;
                        }

                        kwi.Keyword = e_g.GetString(kwIndexTable, index, i16).TrimEnd('\0');
                        index += i16;
                    }

                    index++;
                    if (Equals(e_g, Encoding.Unicode)) index++;
                    i16 = readUInt16(kwIndexTable, index);
                    index += 2;
                    if (i16 > 0)
                    {
                        if (Equals(e_g, Encoding.Unicode))
                        {
                            i16 *= 2;
                        }

                        kwi.KeywordEnd = e_g.GetString(kwIndexTable, index, i16).TrimEnd('\0');
                        index += i16;
                    }

                    index++;
                    if (Equals(e_g, Encoding.Unicode)) index++;
                    kwi.CompressedSize = readUInt64(kwIndexTable, index);
                    index += 8;
                    kwi.UncompressedSize = readUInt64(kwIndexTable, index);
                    index += 8;
                }
                else
                {
                    kwi.CountOfEntries = readUInt32(kwIndexTable, index);
                    index += 4;
                    int i16 = kwIndexTable[index++];
                    if (i16 > 0)
                    {
                        if (Equals(e_g, Encoding.Unicode))
                        {
                            i16 *= 2;
                        }

                        kwi.Keyword = e_g.GetString(kwIndexTable, index, i16).TrimEnd('\0');
                        index += i16;
                    }

                    i16 = kwIndexTable[index++];
                    if (i16 > 0)
                    {
                        if (Equals(e_g, Encoding.Unicode))
                        {
                            i16 *= 2;
                        }

                        kwi.KeywordEnd = e_g.GetString(kwIndexTable, index, i16).TrimEnd('\0');
                        index += i16;
                    }

                    kwi.CompressedSize = readUInt32(kwIndexTable, index);
                    index += 4;
                    kwi.UncompressedSize = readUInt32(kwIndexTable, index);
                    index += 4;
                }

                kwIndexes.Add(kwi); //kwIndexMap.Add(kwi);
                kwIndexMap.Add(string.IsNullOrEmpty(kwi.Keyword) ? "" : kwi.Keyword, kwi);
                if (KeywordToItemIndex == null) KeywordToItemIndex = new Dictionary<string, int>();
                KeywordToItemIndex.Add(string.IsNullOrEmpty(kwi.Keyword) ? "" : kwi.Keyword,
                    kwIndexes.Count - 1);

                Log(string.Format("    \"{0}\"-\"{1}\": {2}/0x{2:X8}, {3}/0x{3:X8}, {4}/0x{4:X8}.",
                    kwi.Keyword, kwi.KeywordEnd, kwi.CountOfEntries, kwi.CompressedSize, kwi.UncompressedSize));
            }

            Log("");

            return true;
        }

        #endregion

        #region loadKWIndexTable2ndLevel

        /// <summary>
        /// 二级索引表。
        /// 二级索引表被分为多个块，每个二级索引表块包含一组连续的关键字（标记为起始关键字-结束关键字），
        /// 同时也包含每个关键字所指向的内容块的相关索引信息。
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fsName"></param>
        private void LoadKwIndex2Table(Stream fs, string fsName)
        {
            #region segment 1: Content Block 部分: 二级索引表

            var list2 = new KwIndex2List();
            //foreach (string keyword in _seg1.KWIndexMap.Keys)
            for (var kwiIdx = 0; kwiIdx < _seg1.IndexList.Length; kwiIdx++)
            {
                var startPos = (ulong) fs.Position;

                KwIndex1 kwi = _seg1.IndexList[kwiIdx]; // _seg1.KWIndexMap[keyword];
                //Log("");
                Log(string.Format(
                    ">> KWI II - Block #{3}: startPos={0:X}H, len={4}/{4:X}H, endPos={1:X}H, unzip len={5}/{5:X}H, '{6}' - '{7}', {8} entries.",
                    startPos, startPos + kwi.CompressedSize, 0, kwiIdx, kwi.CompressedSize, kwi.UncompressedSize,
                    kwi.Keyword, kwi.KeywordEnd, kwi.CountOfEntries));

                var magicNum = readUInt32(fs);
                var j2 = readUInt32(fs);
                //不必进行校验，没有确定的标志。if ((kwi_idx==0 && (uint)j2 == 0xfd35fc4dU) || kwi_idx > 0 )
                {
                    var rawData = new byte[kwi.CompressedSize - 8];
                    fs.Read(rawData, 0, rawData.Length);

                    #region debug

                    //if (kwi_idx < 6)
                    //{
                    //    //DEBUG
                    //    //string path = string.Format("{0}.seg1.kwi.{1:D5}.raw", CompressUtil.stripPathExt(fsName), kwi_idx);
                    //    //write_file(path, rawdata);
                    //    //write_file(CompressUtil.stripPathExt(fsName) + string.Format(".seg1.kwi.{0:D5}.bin", kwi_idx), rawdata);
                    //}

                    #endregion

                    if (magicNum == 0x02000000)
                    {
                        #region InflaterDecompress

                        try
                        {
                            var txt = CompressUtil.InflaterDecompress(rawData, 0, rawData.Length, false);

                            #region debug

                            //if (kwi_idx < 1)
                            //{
                            //    string path = string.Format("{0}.seg1.kwi.{1:D5}.unz", CompressUtil.stripPathExt(fsName), kwi_idx);
                            //    write_file(path, txt);
                            //    //write_file(CompressUtil.stripPathExt(fsName) + ".seg1.kwi." + kwi_idx + ".unz", txt);
                            //}

                            #endregion

                            #region log & trying to parse 2nd level indexes

                            //Log("");
                            //Log(string.Format(">> 提取二级索引表 #{0}...", kwi_idx));
                            ulong x1;
                            uint x9;
                            int ofs = 0;
                            while (ofs < txt.Length)
                            {
                                KwIndex2 kwi2 = new KwIndex2();
                                kwi2.RelOffsetUL = x1 = readUInt64(txt, ofs);
                                ofs += 8;
                                //kwi2.X2 = x2 = readUInt32(txt, ofs); ofs += 4;
                                int ofs0 = ofs;
                                if (DictHeader.LanguageMode == Encoding.Unicode)
                                {
                                    x9 = 1;
                                    while (x9 != 0)
                                    {
                                        x9 = readUInt16(txt, ofs);
                                        ofs += 2;
                                    }
                                }
                                else
                                {
                                    while (txt[ofs] != 0) ofs++;
                                    ofs++;
                                }

                                kwi2.Keyword = DictHeader.LanguageMode.GetString(txt, ofs0, ofs - ofs0).TrimEnd('\0');
                                list2.Add(kwi2);
                                if (kwiIdx == 0 || kwiIdx == _seg1.IndexList.Length - 1)
                                    Log($"    > {kwi2}");
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            ErrorLog(ex.ToString());
                            var path = $"{CompressUtil.stripPathExt(fsName)}.seg1.kwi.{kwiIdx:D5}.raw.v2.0.prb";
                            writeFile(path, rawData);
                            //write_file(CompressUtil.stripPathExt(fsName) + string.Format(".seg1.kwi.{0:D5}.bin.v2.0.prb", kwi_idx), rawdata);
                            //throw ex;
                            continue;
                        }

                        #endregion
                    }
                    else if (magicNum == 0x01000000)
                    {
                        // 二级索引表解压后的块结构为：
                        //   item0.x1[dword] item0.x2[dword] item0.keyword[c-style string with zero-tail]
                        //   item1.x1[dword] item1.x2[dword] item1.keyword[c-style string with zero-tail]
                        //   ...

                        #region LZO.Decompress (V2.0 & V1.2)

                        if (DictHeader.GeneratedByEngineVersion == "2.0")
                        {
                            Log("永远不应该进入到这个分支来才是对的");

                            #region Lzo1x解压缩

                            #region 测试MiniLZO (废弃)

                            //if (false)
                            //{
                            //    seg1name1 = fs.Name + ".ini";
                            //    byte[] b000 = new byte[bufSize];
                            //    byte[] dst000;
                            //    byte[] src000;
                            //    using (FileStream fs1 = new FileStream(seg1name1, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize))
                            //    {
                            //        fs1.Read(b000, 0, bufSize);
                            //    }
                            //    MiniLZO.Compress(b000, out dst000);
                            //    using (FileStream fsOut = File.Create(seg1name1 + ".comp"))
                            //    {
                            //        fsOut.Write(dst000, 0, dst000.Length);
                            //    }
                            //    src000 = new byte[65536];
                            //    int srcSize = MiniLZO.Decompress(dst000, src000);
                            //    using (FileStream fsOut = File.Create(seg1name1 + ".comp.decomp"))
                            //    {
                            //        fsOut.Write(src000, 0, srcSize);
                            //    }
                            //}

                            #endregion

                            var decompData = new byte[kwi.UncompressedSize];
                            var decompSize = 0;
                            try
                            {
                                decompSize = MiniLZO.Decompress(rawData, decompData);
                            }
                            catch (Exception ex)
                            {
                                var b = false;
                                if (b == false)
                                {
                                    Log(ex.ToString());
                                    var path = $"{CompressUtil.stripPathExt(fsName)}.seg1.kwi.{kwiIdx:D5}.raw.v1.x.prb";
                                    writeFile(path, rawData);
                                    //write_file(CompressUtil.stripPathExt(fsName) + string.Format(".seg1.kwi.{0:D5}.bin.v1.x.prb", kwi_idx), rawdata);
                                    //throw ex;
                                    continue;
                                }
                            }

                            #region debug

                            //if (kwi_idx < 1)
                            //{
                            //    string path = string.Format("{0}.seg1.kwi.{1:D5}.unz", CompressUtil.stripPathExt(fsName), kwi_idx);
                            //    write_file(path, _decompData);
                            //    //write_file(CompressUtil.stripPathExt(fsName) + ".seg1.kwi." + kwi_idx + ".unz", _decompData, _decompSize);
                            //}

                            #endregion

                            #endregion

                            #region 提取二级索引表

                            Log("");
                            Log($">> 提取二级索引表 #{kwiIdx}...");
                            // ulong x1;
                            var ofs = 0;
                            while (ofs < decompSize)
                            {
                                KwIndex2 kwi2 = new KwIndex2 {RelOffsetUL = readUInt64(decompData, ofs)};
                                ofs += 8;
                                //kwi2.X2 = x2 = readUInt32(decompData, ofs); ofs += 4;
                                var ofs0 = ofs;
                                if (Equals(DictHeader.LanguageMode, Encoding.Unicode))
                                {
                                    uint x9 = 1;
                                    while (x9 != 0)
                                    {
                                        x9 = readUInt16(decompData, ofs);
                                        ofs += 2;
                                    }
                                }
                                else
                                {
                                    while (decompData[ofs] != 0) ofs++;
                                    ofs++;
                                }

                                kwi2.Keyword = DictHeader.LanguageMode.GetString(decompData, ofs0, ofs - ofs0)
                                    .TrimEnd('\0');
                                list2.Add(kwi2);
                                //Debug.WriteLine(string.Format("    > {0} - {1}, {2}", x1, x2, ss));
                                //Log(string.Format("    > {0}", kwi2));
                            }

                            #endregion
                        }
                        else
                        {
                            //Ver 1.2中，二级索引表是被压缩过的。
                            //证实: 数据被压缩，压缩算法是一种字典压缩算法，可能为LZW的变种，但应该不是LZO1x

                            #region v1.2

                            #region deleted codes

                            //ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf;
                            //inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater(true);
                            //inf.SetInput(rawdata);
                            //byte[] txt = new byte[65536 * 2];
                            //int decompSize = inf.Inflate(txt, 0, txt.Length);


                            // 二级索引表解压后的块结构为：
                            //   item0.x1[word] item0.x2[word] item0.keyword[c-style string with zero-tail]
                            //   item1.x1[word] item1.x2[word] item1.keyword[c-style string with zero-tail]
                            //   ...
                            /*int x1, x2, ofs = 0;
                            x1 = readInt16(rawdata, ofs); ofs += 2;
                            Log(string.Format(">>>>>>>>>>>>> #{2}: filepos={1}/0x{1:X8}, {0} bytes (Ver 1.2中，二级索引表):", x1, startPos,kwi_idx));
                            Debug.WriteLine(string.Format(">>>>>>>>>>>>> #{2}: filepos={1}/0x{1:X8}, {0} bytes (Ver 1.2中，二级索引表):", x1, startPos,kwi_idx));
                            try
                            {
                                while (ofs < rawdata.Length)
                                {
                                    x2 = readInt16(rawdata, ofs); ofs += 2;
                                    int len = rawdata[ofs++] + 2 + 1;
                                    string ss = DictHeader.LanguageMode.GetString(rawdata, ofs, len);
                                    ofs += len;
                                    if (kwi_idx < 5)
                                    {
                                        Debug.WriteLine(string.Format("    > {1}/0x{1:X2}, {2}", x1, x2, ss));
                                        Log(string.Format("    > {1}/0x{1:X2}, {2}", x1, x2, ss));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                                Log(ex.ToString());
                            }*/

                            #endregion

                            #region 提取二级索引表

                            //Log(string.Format(">>>> 提取二级索引表 #{0}...", kwi_idx));
                            // uint x1;
                            var ofs = 0;

                            var cZipped = rawData[ofs]; //预览一个word

                            var zipped = cZipped != (byte) 0; //如果头一个byte为0，则实际上该块未压缩。

                            byte[] decompData;

                            if (!zipped)
                            {
                                decompData = rawData;
                            }
                            else
                            {
                                #region TODO: unzip

                                //string path = string.Format("{0}.seg1.kwi.{1:D5}.unz", CompressUtil.stripPathExt(fsName), kwi_idx);

                                decompData = new byte[kwi.UncompressedSize];
                                //int _decompSize = 0;
                                var ok = false;
                                //Simplicit.Net.Lzo.LZOCompressor lzo = new Simplicit.Net.Lzo.LZOCompressor();
                                var in_len = rawData.Length - 3;
                                int out_len = BitConverter.ToInt16(rawData, 1);

                                if (!ok)
                                {
                                    try
                                    {
                                        //byte[] xyz = new byte[rawdata.Length+8];
                                        //Array.Copy(BitConverter.GetBytes(magicNum), xyz, BitConverter.GetBytes(magicNum).Length);
                                        //Array.Copy(BitConverter.GetBytes(j2), 0, xyz, BitConverter.GetBytes(j2).Length, BitConverter.GetBytes(j2).Length);
                                        //Array.Copy(rawdata, 0, xyz, BitConverter.GetBytes(j2).Length*2, rawdata.Length);
                                        //_decompData = LZOHelper.LZOCompressor.Decompress(xyz, 0, (int)kwi.UncompressedSize);

                                        MiniLZO.Decompress(rawData, 0, (int) kwi.CompressedSize, decompData);
                                        // decompData = LZOHelper.LZOCompressor.Decompress(rawdata, 0, (int) kwi.UncompressedSize);
                                        //if (kwi_idx < 3)
                                        //    write_file(string.Format("{0}.seg1.kwi.{1:D5}.unz", CompressUtil.stripPathExt(fsName), kwi_idx), _decompData, _decompData.Length);
                                        ok = decompData.Length == (int) kwi.UncompressedSize;
                                        Debug.Assert(ok, "Mdict 1.2 lzo decomp failed.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.ToString());
                                    }
                                }

                                //if (!ok)
                                //{
                                //    try
                                //    {
                                //        _decompData = new byte[kwi.UncompressedSize];
                                //        _decompSize = MiniLZO.Decompress(rawdata, _decompData);
                                //        ok = _decompSize == (int)kwi.UncompressedSize;
                                //    }
                                //    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
                                //}

                                if (!ok)
                                {
                                    ErrorLog("BAD");
                                    var path = $"{CompressUtil.stripPathExt(fsName)}.seg1.kwi.{kwiIdx:D5}.raw.v1.2.prb";
                                    writeFile(path, rawData);
                                    //write_file(CompressUtil.stripPathExt(fsName) + string.Format(".seg1.kwi.{0:D5}.bin.v1.2.prb", kwi_idx), rawdata);
                                }
                                else
                                {
                                    #region debug

                                    //if (kwi_idx < 1)
                                    //{
                                    //    string path = string.Format("{0}.seg1.kwi.{1:D5}.unz", CompressUtil.stripPathExt(fsName), kwi_idx);
                                    //    write_file(path, _decompData);
                                    //    //write_file(CompressUtil.stripPathExt(fsName) + ".seg1.kwi." + kwi_idx + ".unz", _decompData, _decompSize);
                                    //}

                                    #endregion
                                }

                                #endregion
                            }

                            #region 解释二级索引表

                            while (ofs < (int) kwi.UncompressedSize)
                            {
                                var kwi2 = new KwIndex2 {RelOffsetUL = readUInt32(decompData, ofs)};
                                ofs += 4; // if (x1 < 0) { Debug.WriteLine(">>> ???"); }
                                //kwi2.X2 = x2 = readUInt16(_decompData, ofs); ofs += 2;// if (x2 < 0) { Debug.WriteLine(">>> ???"); }
                                var ofs0 = ofs;
                                if (DictHeader.LanguageMode == Encoding.Unicode)
                                {
                                    uint x9 = 1;
                                    while (x9 != 0)
                                    {
                                        x9 = readUInt16(decompData, ofs);
                                        ofs += 2;
                                    }
                                }
                                else
                                {
                                    while (decompData[ofs] != 0) ofs++;
                                    ofs++;
                                }

                                kwi2.Keyword = DictHeader.LanguageMode.GetString(decompData, ofs0, ofs - ofs0)
                                    .TrimEnd('\0');
                                list2.Add(kwi2);
                                //Debug.WriteLine(string.Format("    > {0} - {1}, {2}", x1, x2, kwi2.Keyword));
                                //Log(string.Format("    > {0}", kwi2));
                            }

                            #endregion

                            #endregion

                            #endregion
                        }

                        #endregion
                    }
                    else if (magicNum == 0)
                    {
                        #region log & trying to parse 2nd level indexes

                        Log("");
                        Log($">> 提取二级索引表 #{kwiIdx}...");
                        // uint x1;
                        var ofs = 0;
                        while (ofs < rawData.Length)
                        {
                            var kwi2 = new KwIndex2 {RelOffsetUL = readUInt32(rawData, ofs)};
                            ofs += 4;
                            //kwi2.X2 = x2 = (uint)readUInt16(rawData, ofs); ofs += 2;
                            var ofs0 = ofs;
                            if (Equals(DictHeader.LanguageMode, Encoding.Unicode))
                            {
                                uint x9 = 1;
                                while (x9 != 0)
                                {
                                    x9 = readUInt16(rawData, ofs);
                                    ofs += 2;
                                }
                            }
                            else
                            {
                                while (rawData[ofs] != 0) ofs++;
                                ofs++;
                            }

                            kwi2.Keyword = DictHeader.LanguageMode.GetString(rawData, ofs0, ofs - ofs0).TrimEnd('\0');
                            list2.Add(kwi2);
                            Log($"    > {kwi2}");
                        }

                        #endregion
                    }
                    else
                    {
                        throw new Exception(
                            $"提取KWIndex2[]时，期望正确的算法标志0x2000000/0x1000000，然而遇到了{magicNum}/0x{magicNum:X}");
                    }

                    //break;
                }
                //else
                //{
                //    throw new Exception(string.Format("提取KWIndex2[]时，期望正确的辅助魔术号码0xFD 35 FC 4D序列，然而遇到了{0}/0x{0:X}", j2));
                //}
            }

            _seg1.IndexList2 = list2.ToArray();

            #endregion
        }

        #endregion

        #region loadContentIndexTable

        private void LoadContentIndexTable(Stream fs, string fsName, long posSeg1BlockStart)
        {
            _seg2 = new MDictContentIndexTable();

            #region seg2 index table

            Log("");
            Log("");
            Log(">> 提取内容块中的索引表...");
            fs.Seek((long) DictIndex.Seg2RelOffset + posSeg1BlockStart, SeekOrigin.Begin); //position = 0x928fc
            Log(string.Format("   filepos={0}/0x{0:X8}", fs.Position));
            if (DictHeader.GeneratedByEngineVersion == "2.0")
            {
                DictLargeContentIndexTable.Count = readUInt64(fs); //0x0260
                DictLargeContentIndexTable.L2 = readUInt64(fs); //0x0133a0
                DictLargeContentIndexTable.IndexTableLength = readUInt64(fs); //0x2600
                DictLargeContentIndexTable.L4 = readUInt64(fs); //0x5cd933
            }
            else
            {
                DictLargeContentIndexTable.Count = readUInt32(fs); //0x0260
                DictLargeContentIndexTable.L2 = readUInt32(fs); //0x0133a0
                DictLargeContentIndexTable.IndexTableLength = readUInt32(fs); //0x2600
                DictLargeContentIndexTable.L4 = readUInt32(fs); //0x5cd933
            }

            Log(string.Format(
                "   Index Table metadata: [Count={0}/0x{0:X8}, L2={1}/0x{1:X8}, Length={2}/0x{2:X8}, L4={3}/0x{3:X8}]",
                DictLargeContentIndexTable.Count, DictLargeContentIndexTable.L2,
                DictLargeContentIndexTable.IndexTableLength, DictLargeContentIndexTable.L4));

            var tmpseg2 = new byte[DictLargeContentIndexTable.IndexTableLength];
            fs.Read(tmpseg2, 0, tmpseg2.Length);

            DictLargeContentIndexTable.IndexesRawData = new List<byte>();
            DictLargeContentIndexTable.IndexesRawData.AddRange(tmpseg2);

            DictLargeContentIndexTable.Seg2ContentBlockOffset = (ulong) fs.Position;
            DictLargeContentIndexTable.Indexes =
                new ContentIndex[(int) (DictLargeContentIndexTable.IndexTableLength /
                                        (DictHeader.GeneratedByEngineVersion == "2.0" ? 8UL : 4UL) / 2)];
            //List<ContentIndex> list = new List<ContentIndex>();
            ulong sum1 = 0, sum2 = 0;
            for (int ix = 0, k = 0; ix < (int) DictLargeContentIndexTable.IndexTableLength; k++)
            {
                ContentIndex ci = new ContentIndex();
                if (DictHeader.GeneratedByEngineVersion == "2.0")
                {
                    ci.CompressedSize = readUInt64(tmpseg2, ix);
                    ci.UncompressedSize = readUInt64(tmpseg2, ix + 8);
                    ix += 16;
                }
                else
                {
                    ci.CompressedSize = readUInt32(tmpseg2, ix);
                    ci.UncompressedSize = readUInt32(tmpseg2, ix + 4);
                    ix += 8;
                }

                ci.Offset = sum1;
                ci.OffsetUncomp = sum2;

                DictLargeContentIndexTable.Indexes[k] = ci;

                sum1 += ci.CompressedSize;
                sum2 += ci.UncompressedSize;

                Log($"      > {k:D4} : {ci}");
            }

            #endregion

            //计算偏移量，并为seg1中二级索引表建立快速索引
            BuildFastRefPointers(fs, fsName);

            //DEBUG
            //if (k < 5)
            //write_file(CompressUtil.stripPathExt(fsName) + ".seg2.to.cti.idx.dmp", tmpseg2);

            if (!verifyNow)
            {
                _seg2.MagicNumber = readUInt32(fs);
                var j2 = readUInt32(fs); //CRC32?
            }
            else
            {
                Log(">> 提取内容块索引表指向的正文内容块进行测试和校验...");
                VerifyContentBlocks(fs, fsName);
            }
        }

        #endregion

        #region buildFastRefPointers

        /// <summary>
        /// 计算偏移量，并为seg1中二级索引表建立快速索引
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fsName"></param>
        private void BuildFastRefPointers(Stream fs, string fsName)
        {
            if (_seg1.IndexList2.Length == 0)
                return;

            //ContentIndex ci0 = _seg2.Indexes[0];
            //KWIndex2 kwi0 = _seg1.IndexList2[0];
            //KWIndex2 kwi_last = null;
            _seg1.Matcher = new FastRobustMatcher<int> {NoCase = true};

            var ic = 0;
            var ci = _seg2.Indexes[ic];
            var end_pos = ci.OffsetUncomp + ci.UncompressedSize;
            for (var iz = 0; iz < _seg1.IndexList2.Length; iz++)
            {
                var kwi2 = _seg1.IndexList2[iz];
                _seg1.Matcher.Add(kwi2.Keyword, iz);

                //ContentIndex ci = LocateContentIndex(kwi2);
                //if (kwi2.CILength == 0 && iz < _seg1.IndexList2.Length - 1)
                //{
                //    kwi2.CILength = _seg1.IndexList2[iz + 1].RelOffsetUL - kwi2.RelOffsetUL;
                //}

                if (kwi2.RelOffsetUL >= ci.OffsetUncomp && kwi2.RelOffsetUL < end_pos)
                {
                    kwi2.CIIndex = ic;
                    kwi2.CIUncompOffset = kwi2.RelOffsetUL - ci.OffsetUncomp;
                    kwi2.CIUncompLength = iz < _seg1.IndexList2.Length - 1
                        ? _seg1.IndexList2[iz + 1].RelOffsetUL - kwi2.RelOffsetUL
                        : end_pos - kwi2.RelOffsetUL;
                }
                else
                {
                    ic++;
                    if (ic >= _seg2.Indexes.Length)
                    {
                        ErrorLog("ic >= _seg2.Indexes.Length, Rebuild fast-ref failed. 这表示二级索引表和内容块索引表相互不匹配");
                    }

                    ci = _seg2.Indexes[ic];
                    end_pos = ci.OffsetUncomp + ci.UncompressedSize;
                    kwi2.CIIndex = ic;
                    kwi2.CIUncompOffset = kwi2.RelOffsetUL - ci.OffsetUncomp;
                    kwi2.CIUncompLength = iz < _seg1.IndexList2.Length - 1
                        ? _seg1.IndexList2[iz + 1].RelOffsetUL - kwi2.RelOffsetUL
                        : end_pos - kwi2.RelOffsetUL;
                }

                //Log(string.Format("   > Rebuilt fast-ref: kwi2 {3:D4} - ofs 0x{4:X08}: CI-idx={0}, CI-Ofs=0x{1:X06}, CI-Len=0x{2:X04}. [0x{5:X08}..0x{6:X08}]", kwi2.CIIndex, kwi2.CIUncompOffset, kwi2.CIUncompLength, iz, kwi2.RelOffsetUL, ci.OffsetUncomp, end_pos));
            }
        }

        private ContentIndex LocateContentIndex(KwIndex2 kwi2)
        {
            if (_seg2.Indexes.Length < 1)
                return null;

            for (var i = 0; i < _seg2.Indexes.Length; ++i)
            {
                var ci = _seg2.Indexes[i];
                if (kwi2.RelOffsetUL < ci.Offset)
                    continue;
                kwi2.CIIndex = i;
                kwi2.CIUncompOffset = kwi2.RelOffsetUL - ci.Offset;
                kwi2.CIUncompLength = ci.Offset + ci.CompressedSize - kwi2.RelOffsetUL;
                return ci;
            }

            return null;
        }

        #endregion

        #region verifyContentBlocks

        private void VerifyContentBlocks(Stream fs, string fsName)
        {
            for (var k = 0; k < DictLargeContentIndexTable.Indexes.Length; k++)
            {
                var ci = DictLargeContentIndexTable.Indexes[k];
                var startPos = (ulong) fs.Position;
                _seg2.MagicNumber = readUInt32(fs);
                //if (DictionaryXmlHeader.GeneratedByEngineVersion == "2.0")
                var j2 = readUInt32(fs); //CRC32?
                Log(string.Format(
                    "   > cti#{2}: start={3:X08}H, end={4:X08}H, len={5:X08}H, unzip len={6:X08}H, #magic={0:X08}H, #j2={1:X08}H",
                    _seg2.MagicNumber, j2, k, startPos, startPos + ci.CompressedSize, ci.CompressedSize,
                    ci.UncompressedSize));

                var rawData = new byte[ci.CompressedSize - 8];
                fs.Read(rawData, 0, rawData.Length);
                if (_seg2.MagicNumber == 0x02000000)
                {
                    #region InflaterDecompress

                    try
                    {
                        var txt = CompressUtil.InflaterDecompress(rawData, 0, rawData.Length);
                        //if (k < 1)
                        //{
                        //    string seg2name1 = CompressUtil.stripPathExt(fsName) + ".seg2.cti." + k + ".unz";
                        //    using (FileStream fsOut = File.Create(seg2name1))
                        //    {
                        //        fsOut.Write(txt, 0, txt.Length);
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        ErrorLog(ex.ToString());
                        throw;
                    }

                    #endregion
                }
                else if (_seg2.MagicNumber == 0x01000000)
                {
                    #region LZO 1x Decompress

                    //if (DictionaryXmlHeader.GeneratedByEngineVersion == "2.0")
                    {
                        //Lzo1x解压缩
                        var ok = false;
                        var _decompData = new byte[ci.UncompressedSize];
                        var _decompSize = 0;

                        if (!ok)
                        {
                            try
                            {
                                MiniLZO.Decompress(rawData, 0, (int) ci.CompressedSize, _decompData);
                                // _decompData = LZOHelper.LZOCompressor.Decompress1x(rawdata, 0, (int) ci.UncompressedSize);
                                ok = true;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                                var path = $"{CompressUtil.stripPathExt(fsName)}.seg2.cti.{k:D5}.prb";
                                writeFile(path, rawData, rawData.Length);
                            }
                        }

                        if (!ok)
                        {
                            try
                            {
                                _decompSize = MiniLZO.Decompress(rawData, 0, rawData.Length, _decompData);
                                ok = true;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }

                        if (!ok)
                        {
                            try
                            {
                                MiniLZO.Decompress(rawData, 0, (int) ci.CompressedSize, _decompData);
                                // _decompData = LZOHelper.LZOCompressor.Decompress(rawdata, 0, (int) ci.UncompressedSize);
                                //write_file(path, _decompData, _decompData.Length);
                                ok = _decompData.Length == (int) ci.UncompressedSize;
                            }
                            catch (Exception ex1)
                            {
                                ErrorLog(ex1.ToString());

                                //string seg2name1 = string.Format("{0}.seg2.cti.{1:D5}.prb", CompressUtil.stripPathExt(fsName), k);
                                //using (FileStream fsOut = File.Create(seg2name1))
                                //{
                                //    fsOut.Write(rawdata, 0, rawdata.Length);
                                //}

                                //throw ex1;
                            }
                        }

                        #region debug

                        //if (k < 1)
                        //{
                        //    //string seg2name1 = CompressUtil.stripPathExt(fsName) + ".seg2.cti." + k + ".unz";
                        //    string seg2name1 = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fsName), k);
                        //    using (FileStream fsOut = File.Create(seg2name1))
                        //    {
                        //        fsOut.Write(_decompData, 0, _decompSize);
                        //    }
                        //}

                        #endregion
                    }
                    //else
                    //{
                    //    //Version 1.2: 数据内容并未压缩故可直接取用。
                    //    if (k < 5)
                    //    {
                    //        seg2name1 = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fsName), k);
                    //        using (FileStream fsOut = File.Create(seg2name1))
                    //        {
                    //            fsOut.Write(rawdata, 0, rawdata.Length);
                    //        }
                    //    }
                    //}

                    #endregion
                }
                else
                {
                    // 如果是一个图像文件（或者类似的其他二进制文件），则通常直接嵌入为内容块，又或者是被抽取为mdd的一部分。
                    // 当直接嵌入为内容块时，则magicNumber为0.
                    // 如果内容未被压缩，那么也不必解压缩，这种情况下magicNumber也为0.
                    //string seg2name1 = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fsName), k);
                    //using (FileStream fsOut = File.Create(seg2name1))
                    //{
                    //    fsOut.Write(rawdata, 0, rawdata.Length);
                    //}
                    ////throw new Exception(string.Format("提取KWIndex时，期望正确的算法标志0x2000000/0x1000000，然而遇到了{0}", _seg2.MagicNumber));
                }
            }
        }

        #endregion

        #region loadContentByKeyword

        public override byte[] LoadContentBytesByKeyword(KwIndex2 kwi2)
        {
            Log($"-- LoadContentBytesByKeyword : Load KWIndex2 ({kwi2})");

            const int bufSize = 32768;
            //int k = int.MaxValue;
            //string seg2name1;
            var ci = _seg2.Indexes[kwi2.CIIndex];
            var magicNumber = _seg2.MagicNumber;
            Log($"   _seg2.MagicNumber={_seg2.MagicNumber:X}, ContentIndex: {ci}");

            using var fs = new FileStream(this.DictFileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize);

            fs.Seek((long) _seg2.Seg2ContentBlockOffset + 8 + (long) ci.Offset, SeekOrigin.Begin);
            Debug.Assert(ci.CompressedSize <= ci.UncompressedSize);
            byte[] rawData;
            if (_seg2.MagicNumber == 0)
            {
                rawData = new byte[ci.CompressedSize];
                fs.Read(rawData, 0, rawData.Length);
                if (ci.CompressedSize == ci.UncompressedSize)
                    return rawData;
                magicNumber = 0x01000000;
            }
            else
            {
                rawData = new byte[ci.CompressedSize];
                fs.Read(rawData, 0, rawData.Length);
            }

            if (magicNumber == 0x02000000)
            {
                #region InflaterDecompress, Passed

                try
                {
                    var txt = CompressUtil.InflaterDecompress(rawData, 0, rawData.Length);
                    //if (k < 1)
                    //{
                    //    seg2name1 = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fs.Name), k); 
                    //    //seg2name1 = CompressUtil.stripPathExt(fs.Name) + ".seg2.cti." + k + ".unz";
                    //    using (FileStream fsOut = File.Create(seg2name1))
                    //    {
                    //        fsOut.Write(txt, 0, txt.Length);
                    //    }
                    //}

                    var b = new byte[kwi2.CIUncompLength];
                    Array.Copy(txt, (int) kwi2.CIUncompOffset, b, 0, (int) kwi2.CIUncompLength);
                    return b;
                }
                catch (Exception ex)
                {
                    ErrorLog(ex.ToString());
                    throw;
                }

                #endregion
            }
            else if (magicNumber == 0x01000000)
            {
                //对于1.2版本格式，可能并未压缩，故如果LZO失败，则将rawdata原样返回

                #region 试图进行 LZO Decompress。

                //Lzo1x解压缩
                var ok = false;
                var _decompData = new byte[ci.UncompressedSize];
                var _decompSize = 0;

                if (!ok)
                {
                    try
                    {
                        MiniLZO.Decompress(rawData, 0, (int) ci.CompressedSize - 8, _decompData);
                        // _decompData = LZOHelper.LZOCompressor.Decompress(rawdata, 0, (int) ci.CompressedSize - 8, (int) ci.UncompressedSize);
                        //string path = string.Format("{0}.ctt.blk.ofs.{1}.unz", CompressUtil.stripPathExt(fs.Name), kwi2.RelOffsetUL);
                        //write_file(path, _decompData, _decompData.Length);
                        ok = _decompData.Length == (int) ci.UncompressedSize;
                        Debug.Assert(ok, "Mdict 1.2 decomp failed. LoadContentBytesByKeyword()");

                        var b = new byte[kwi2.CIUncompLength];
                        Array.Copy(_decompData, (int) kwi2.CIUncompOffset, b, 0, (int) kwi2.CIUncompLength);
                        return b;
                    }
                    catch (Exception ex1)
                    {
                        ErrorLog(ex1.ToString());
                    }
                }

                if (!ok)
                {
                    try
                    {
                        // 1.2版本格式，cti数据块的长度为CompressedSize-8，数据块的末尾8个字节含义暂时未知。
                        //byte[] xyz = new byte[rawdata.Length - 8];
                        //Array.Copy(rawdata, xyz, xyz.Length);
                        MiniLZO.Decompress(rawData, 0, rawData.Length - 8, _decompData);
                        // _decompSize = MiniLZO.Decompress(rawdata, 0, rawdata.Length - 8, _decompData);
                        //xyz = new byte[8];
                        //byte[] _decompData1 = new byte[100];
                        //Array.Copy(rawdata, ci.CompressedSize-8, xyz, 0, xyz.Length);
                        //_decompSize = MiniLZO.Decompress(xyz, _decompData1);

                        #region debug

                        //if (k < 1)
                        //{
                        //    string path = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fs.Name), k);
                        //    write_file(path, _decompData, _decompSize);
                        //    //write_file(CompressUtil.stripPathExt(fs.Name) + ".seg2.cti." + k + ".unz", _decompData, _decompSize);
                        //}

                        #endregion

                        if (_decompSize == (int) ci.UncompressedSize)
                            return _decompData;

                        var b = new byte[kwi2.CIUncompLength];
                        Array.Copy(_decompData, (int) kwi2.CIUncompOffset, b, 0, (int) kwi2.CIUncompLength);
                        return b;
                        //byte[] b = new byte[_decompSize];
                        //Array.Copy(_decompData, b, _decompSize);
                        //return b;
                    }
                    catch (Exception ex)
                    {
                        //if (this.DictHeader.GeneratedByEngineVersion == "2.0")
                        {
                            ErrorLog(ex.ToString());
                        }
                    }
                }

                if (!ok)
                {
                    try
                    {
                        MiniLZO.Decompress(rawData, 0, (int) ci.CompressedSize, _decompData);
                        // _decompData = LZOHelper.LZOCompressor.Decompress(rawdata, 0, (int) ci.UncompressedSize);
                        //write_file(path, _decompData, _decompData.Length);
                        ok = _decompData.Length == (int) ci.UncompressedSize;
                    }
                    catch (Exception ex1)
                    {
                        ErrorLog(ex1.ToString());

                        //byte[] txt = CompressUtil.InflaterDecompress(rawdata, 0, rawdata.Length);

                        var path = $"{CompressUtil.stripPathExt(fs.Name)}.ctt.{kwi2.CIIndex:D5}.raw";
                        writeFile(path, rawData);
                        //seg2name1 = CompressUtil.stripPathExt(fs.Name) + ".ctt.blk." + kwi2.ContentBlockIndex + ".bin";
                        //using (FileStream fsOut = File.Create(seg2name1))
                        //{
                        //    fsOut.Write(rawdata, 0, rawdata.Length);
                        //}

                        throw;
                    }
                }

                #endregion

                //此外，1.2版本格式也可能采用了第二种压缩方式，这种情况下ci.UncompressedSize!=ci.CompressedSize
                //if (this.DictHeader.GeneratedByEngineVersion == "1.2")
                if (ci.UncompressedSize == ci.CompressedSize)
                {
                    //Version 1.2: 数据内容并未压缩故可直接取用。
                    return rawData;
                }
                else
                {
                    //TODO: LZW变种算法
                    //string path = string.Format("{0}.ctt.{1:D5}.raw", CompressUtil.stripPathExt(fs.Name), kwi2.CIIndex);
                    //write_file(path, rawdata);
                    ////this.write_file(CompressUtil.stripPathExt(fs.Name) + ".ctt.blk." + kwi2.ContentBlockIndex + ".unz", rawdata);
                    return rawData;
                }
            }
            else
            {
                throw new Exception($"提取KWIndex时，期望正确的算法标志0x2000000/0x1000000，然而遇到了{_seg2.MagicNumber}");
            }
        }

        public override string LoadContentByKeyword(KwIndex2 kwi2)
        {
            var data = LoadContentBytesByKeyword(kwi2);
            if (data == null || data.Length <= 0) return string.Empty;

            var html = DictHeader.LanguageMode.GetString(data, 0, data.Length);
            return html;
        }

        public string LoadContentByKeywordXXYY(KwIndex2 kwi2)
        {
            Log($"-- LoadContentByKeyword : Load KWIndex2 ({kwi2})");

            const int bufSize = 32768;
            var html = string.Empty;

            //string seg2name1;
            var ci = _seg2.Indexes[kwi2.CIIndex];
            //int k = int.MaxValue;

            using (var fs = new FileStream(this.DictFileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize))
            {
                fs.Seek((long) _seg2.Seg2ContentBlockOffset + 8 + (long) ci.Offset, SeekOrigin.Begin);

                var rawData = new byte[ci.CompressedSize];
                fs.Read(rawData, 0, rawData.Length);

                if (_seg2.MagicNumber == 0x02000000)
                {
                    #region InflaterDecompress

                    try
                    {
                        byte[] txt = CompressUtil.InflaterDecompress(rawData, 0, rawData.Length);
                        //if (k < 1)
                        //{
                        //    string path = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fs.Name), k);
                        //    write_file(path, txt);
                        //    //seg2name1 = CompressUtil.stripPathExt(fs.Name) + ".seg2.cti." + k + ".unz";
                        //    //using (FileStream fsOut = File.Create(seg2name1))
                        //    //{
                        //    //    fsOut.Write(txt, 0, txt.Length);
                        //    //}
                        //}

                        //KWIndex2 kwiPost = null;
                        html = DictHeader.LanguageMode.GetString(txt, (int) kwi2.CIUncompOffset,
                            //(int)(kwi2.Length < 0 ? txt.Length - kwi2.RelOffset : kwi2.Length)
                            (int) kwi2.CIUncompLength);
                    }
                    catch (Exception ex)
                    {
                        ErrorLog(ex.ToString());
                        throw;
                    }

                    #endregion
                }
                else if (_seg2.MagicNumber == 0x01000000)
                {
                    #region LZO Decompress

                    //if (DictionaryXmlHeader.GeneratedByEngineVersion == "2.0")
                    {
                        //Lzo1x解压缩
                        var _decompData = new byte[ci.UncompressedSize];
                        var _decompSize = 0;
                        try
                        {
                            _decompSize = MiniLZO.Decompress(rawData, _decompData);
                            html = DictHeader.LanguageMode.GetString(_decompData, 0, _decompSize);
                        }
                        catch (Exception ex)
                        {
                            ErrorLog(ex.ToString());
                            throw;
                        }

                        #region debug

                        //if (k < 1)
                        //{
                        //    string path = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fs.Name), k);
                        //    write_file(path, _decompData);
                        //    //write_file(CompressUtil.stripPathExt(fs.Name) + ".seg2.cti." + k + ".unz", _decompData, _decompSize);
                        //}

                        #endregion
                    }
                    //else
                    //{
                    //    //Version 1.2: 数据内容并未压缩故可直接取用。
                    //    if (k < 5)
                    //    {
                    //        seg2name1 = CompressUtil.stripPathExt(fs.Name) + ".seg2.cti." + k + ".unz";
                    //        using (FileStream fsOut = File.Create(seg2name1))
                    //        {
                    //            fsOut.Write(rawdata, 0, rawdata.Length);
                    //        }
                    //    }
                    //}

                    #endregion
                }
                else
                {
                    throw new Exception($"提取KWIndex时，期望正确的算法标志0x2000000/0x1000000，然而遇到了{_seg2.MagicNumber}");
                }
            }

            return html;
        }

        #endregion


        #region small helpers,

        private static int xreadInt32(Stream fs)
        {
            uint x = 0;
            x |= (uint) fs.ReadByte() << 24;
            x |= (uint) fs.ReadByte() << 16;
            x |= (uint) fs.ReadByte() << 8;
            x |= (uint) fs.ReadByte();
            return (int) x;
        }

        private static uint readUInt32(Stream fs)
        {
            uint x = 0;
            x |= (uint) fs.ReadByte() << 24;
            x |= (uint) fs.ReadByte() << 16;
            x |= (uint) fs.ReadByte() << 8;
            x |= (uint) fs.ReadByte();
            return x;
        }

        private static int xreadInt32(byte[] b, int offset)
        {
            uint x = 0;
            x |= (uint) b[offset++] << 24;
            x |= (uint) b[offset++] << 16;
            x |= (uint) b[offset++] << 8;
            x |= (uint) b[offset];
            return (int) x;
        }

        private static uint readUInt32(byte[] b, int offset)
        {
            uint x = 0;
            x |= (uint) b[offset++] << 24;
            x |= (uint) b[offset++] << 16;
            x |= (uint) b[offset++] << 8;
            x |= (uint) b[offset];
            return x;
        }

        private static int xreadInt32Intel(Stream fs)
        {
            uint x = 0;
            x |= (uint) fs.ReadByte();
            x |= (uint) fs.ReadByte() << 8;
            x |= (uint) fs.ReadByte() << 16;
            x |= (uint) fs.ReadByte() << 24;
            return (int) x;
        }

        private static uint readUInt32Intel(Stream fs)
        {
            uint x = 0;
            x |= (uint) fs.ReadByte();
            x |= (uint) fs.ReadByte() << 8;
            x |= (uint) fs.ReadByte() << 16;
            x |= (uint) fs.ReadByte() << 24;
            return x;
        }

        private static long xreadInt64(Stream fs)
        {
            ulong x = 0;
            x |= ((ulong) (uint) fs.ReadByte() << 56);
            x |= ((ulong) (uint) fs.ReadByte() << 48);
            x |= ((ulong) (uint) fs.ReadByte() << 40);
            x |= ((ulong) (uint) fs.ReadByte() << 32);
            x |= ((ulong) (uint) fs.ReadByte() << 24);
            x |= ((ulong) (uint) fs.ReadByte() << 16);
            x |= ((ulong) (uint) fs.ReadByte() << 8);
            x |= ((ulong) (uint) fs.ReadByte());
            return (long) x;
        }

        private static ulong readUInt64(Stream fs)
        {
            ulong x = 0;
            x |= ((ulong) (uint) fs.ReadByte() << 56);
            x |= ((ulong) (uint) fs.ReadByte() << 48);
            x |= ((ulong) (uint) fs.ReadByte() << 40);
            x |= ((ulong) (uint) fs.ReadByte() << 32);
            x |= ((ulong) (uint) fs.ReadByte() << 24);
            x |= ((ulong) (uint) fs.ReadByte() << 16);
            x |= ((ulong) (uint) fs.ReadByte() << 8);
            x |= ((ulong) (uint) fs.ReadByte());
            return x;
        }

        private static long xreadInt64(byte[] b, int offset)
        {
            ulong x = 0;
            x |= ((ulong) (uint) b[offset++] << 56);
            x |= ((ulong) (uint) b[offset++] << 48);
            x |= ((ulong) (uint) b[offset++] << 40);
            x |= ((ulong) (uint) b[offset++] << 32);
            x |= ((ulong) (uint) b[offset++] << 24);
            x |= ((ulong) (uint) b[offset++] << 16);
            x |= ((ulong) (uint) b[offset++] << 8);
            x |= ((ulong) (uint) b[offset]);
            return (long) x;
        }

        private static ulong readUInt64(byte[] b, int offset)
        {
            ulong x = 0;
            x |= ((ulong) (uint) b[offset++] << 56);
            x |= ((ulong) (uint) b[offset++] << 48);
            x |= ((ulong) (uint) b[offset++] << 40);
            x |= ((ulong) (uint) b[offset++] << 32);
            x |= ((ulong) (uint) b[offset++] << 24);
            x |= ((ulong) (uint) b[offset++] << 16);
            x |= ((ulong) (uint) b[offset++] << 8);
            x |= ((ulong) (uint) b[offset]);
            return x;
        }

        private static short xreadInt16(byte[] b, int offset)
        {
            ushort x = 0;
            x |= (ushort) ((uint) b[offset++] << 8);
            x |= (ushort) ((uint) b[offset]);
            return (short) x;
        }

        private static ushort readUInt16(byte[] b, int offset)
        {
            ushort x = 0;
            x |= (ushort) ((uint) b[offset++] << 8);
            x |= (ushort) ((uint) b[offset]);
            return x;
        }

        private static DateTime readFileTime(Stream fs)
        {
            long x64;
            uint lo = readUInt32Intel(fs);
            uint hi = readUInt32Intel(fs);
            //int hi = readInt(fs);
            //int lo = readInt(fs);
            x64 = (long) (uint) hi;
            x64 <<= 32;
            x64 |= (long) (uint) lo;
            return DateTime.FromFileTime(x64);
        }

        private static string readString(Stream fs, int bytes)
        {
            byte[] buf = new byte[bytes];
            int size = fs.Read(buf, 0, bytes);
            Debug.Assert(size == bytes);
            return Encoding.Unicode.GetString(buf);
        }

        private static void writeFile(string name, byte[] content)
        {
            //string seg2name1 = CompressUtil.stripPathExt(fs.Name) + ".seg2.idx.dmp";
            using var fsOut = File.Create(name);
            fsOut.Write(content, 0, content.Length);
        }

        private static void writeFile(string name, byte[] content, int length)
        {
            using var fsOut = File.Create(name);
            fsOut.Write(content, 0, length);
        }

        #endregion

        // public new void Dispose()
        // {
        //     Console.WriteLine("MDictLoader.Dispose()");
        //     Shutdown();
        // }

        public bool debugModeEnable = false;
        public bool walkOnContentIndexTable = false;
    }
}