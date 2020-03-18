using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using HzNS.MdxLib.Compression.Aced;
using HzNS.MdxLib.Compression.impl;
using ICSharpCode.SharpZipLib.BZip2;
using Ionic.Zlib;

//using ICSharpCode.SharpZipLib.LZW;


namespace HzNS.MdxLib.Compression
{
    /// <summary>
    /// GZIP: http://blog.lugru.com/2010/06/compressing-decompressing-web-gzip-stream/
    ///   Reference:
    ///     [1] RFC 1952
    ///     [2] ZLIB – http://www.zlib.net/
    ///     [3] QUAZIP – http://quazip.sourceforge.net/
    /// .
    /// </summary>
    public static class CompressUtil
    {
        public static string stripPathExt(string pathName)
        {
            return pathName.Remove(pathName.Length - Path.GetExtension(pathName).Length);
        }


        private const int BufSize = 32768;


        #region gzipCompress

        public static bool gzipCompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return gzipCompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool gzipCompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return gzipCompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool gzipCompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];
            try
            {
                using (System.IO.Compression.GZipStream compressing =
                    new System.IO.Compression.GZipStream(fsOut, System.IO.Compression.CompressionMode.Compress, false))
                {
                    while (true)
                    {
                        count = fsIn.Read(buf, 0, BufSize);
                        if (count != 0)
                            compressing.Write(buf, 0, count);
                        if (count != BufSize)
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

        #region gzipDecompress

        public static bool gzipDecompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return gzipDecompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool gzipDecompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return gzipDecompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool gzipDecompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];

            try
            {
                using (System.IO.Compression.GZipStream uncompressed =
                    new System.IO.Compression.GZipStream(fsIn, System.IO.Compression.CompressionMode.Decompress, true))
                {
                    while (true)
                    {
                        count = uncompressed.Read(buf, 0, BufSize);
                        if (count != 0)
                            fsOut.Write(buf, 0, count);
                        if (count != BufSize)
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion


        #region Deflate Compress In Memory

        /// <summary>
        /// C#自身的deflate压缩功能
        /// </summary>
        /// <param name="memoryData"></param>
        /// <param name="compressedData"></param>
        /// <returns></returns>
        public static bool deflateCompressInMemory(string memoryData, out byte[] compressedData)
        {
            try
            {
                byte[] b = Encoding.Default.GetBytes(memoryData);
                return deflateCompressInMemory(b, 0, b.Length, out compressedData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            compressedData = null;
            return false;
        }

        public static bool deflateCompressInMemory(byte[] memoryData, int offset, int length, out byte[] compressedData)
        {
            using (MemoryStream ms = new MemoryStream(memoryData, offset, length))
            {
                try
                {
                    System.IO.Compression.DeflateStream ds =
                        new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress);
                    ds.Write(ms.GetBuffer(), 0, (int) ms.Length);
                    ds.Close();
                    compressedData = ms.ToArray();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            compressedData = null;
            return false;
        }

        #endregion

        #region Deflate Compress

        /// <summary>
        /// C#自身的压缩功能
        /// </summary>
        /// <param name="inName"></param>
        /// <param name="outName"></param>
        /// <returns></returns>
        public static bool deflateCompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return deflateCompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool deflateCompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return deflateCompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool deflateCompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];
            try
            {
                using (System.IO.Compression.DeflateStream compressing =
                    new System.IO.Compression.DeflateStream(fsOut, System.IO.Compression.CompressionMode.Compress,
                        false))
                {
                    while (true)
                    {
                        count = fsIn.Read(buf, 0, BufSize);
                        if (count != 0)
                            compressing.Write(buf, 0, count);
                        if (count != BufSize)
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

        #region DeflateDecompress

        /// <summary>
        /// C#自身的deflate/inflate解压缩功能
        /// </summary>
        /// <param name="inFilename"></param>
        /// <param name="outFilename"></param>
        /// <returns></returns>
        public static bool deflateDecompress(string inFilename, string outFilename)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inFilename, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return deflateDecompress(fsIn, outFilename);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool deflateDecompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return deflateDecompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool deflateDecompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];

            try
            {
                using (System.IO.Compression.DeflateStream uncompressed =
                    new System.IO.Compression.DeflateStream(fsIn, System.IO.Compression.CompressionMode.Decompress,
                        true))
                {
                    while (true)
                    {
                        count = uncompressed.Read(buf, 0, BufSize);
                        if (count != 0)
                            fsOut.Write(buf, 0, count);
                        if (count != BufSize)
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

        private const int BufferSize = 4096;

        #region IonicZip/IonicZlib Deflate

        // /// <summary>
        // /// IonicZlib库所实现的压缩功能
        // /// </summary>
        // /// <param name="ins"></param>
        // /// <param name="outs"></param>
        // /// <param name="level"></param>
        // public static void DeflateBufferWithIonicZlib(Stream ins, Stream outs, Ionic.Zlib.CompressionLevel level)
        // {
        //     int cnt;
        //     byte[] buffer = new byte[BufferSize];
        //     while ((cnt = ins.Read(buffer, 0, BufferSize)) > 0)
        //     {
        //         DeflateBufferWithIonicZlib(buffer, cnt, outs, level);
        //     }
        // }
        //
        // public static void DeflateBufferWithIonicZlib(byte[] uncompressedBytes, int length, Stream outs,
        //     Ionic.Zlib.CompressionLevel level)
        // {
        //     //int bufferSize = 1024;
        //     byte[] buffer = new byte[BufferSize];
        //     ZlibCodec compressor = new ZlibCodec();
        //     int rc = compressor.InitializeDeflate(level);
        //
        //     compressor.InputBuffer = uncompressedBytes;
        //     compressor.AvailableBytesIn = length;
        //     compressor.NextIn = 0;
        //
        //     compressor.OutputBuffer = buffer;
        //
        //     // pass 1: deflate 
        //     do
        //     {
        //         compressor.NextOut = 0;
        //         compressor.AvailableBytesOut = buffer.Length;
        //         rc = compressor.Deflate(FlushType.None);
        //
        //         if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
        //             throw new Exception("deflating: " + compressor.Message);
        //
        //         outs.Write(compressor.OutputBuffer, 0, buffer.Length - compressor.AvailableBytesOut);
        //     } while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);
        //
        //     // pass 2: finish and flush
        //     do
        //     {
        //         compressor.NextOut = 0;
        //         compressor.AvailableBytesOut = buffer.Length;
        //         rc = compressor.Deflate(FlushType.Finish);
        //
        //         if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
        //             throw new Exception("deflating: " + compressor.Message);
        //
        //         if (buffer.Length - compressor.AvailableBytesOut > 0)
        //             outs.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);
        //     } while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);
        //
        //     compressor.EndDeflate();
        // }
        //
        // private static byte[] DeflateBuffer(byte[] uncompressedBytes, Ionic.Zlib.CompressionLevel level)
        // {
        //     const int bufferSize = 1024;
        //     var buffer = new byte[bufferSize];
        //     var compressor = new ZlibCodec();
        //
        //     Console.WriteLine("\n============================================");
        //     Console.WriteLine("Size of Buffer to Deflate: {0} bytes.", uncompressedBytes.Length);
        //     var ms = new MemoryStream();
        //
        //     var rc = compressor.InitializeDeflate(level);
        //
        //     compressor.InputBuffer = uncompressedBytes;
        //     compressor.AvailableBytesIn = uncompressedBytes.Length;
        //     compressor.NextIn = 0;
        //
        //     compressor.OutputBuffer = buffer;
        //
        //     // pass 1: deflate 
        //     do
        //     {
        //         compressor.NextOut = 0;
        //         compressor.AvailableBytesOut = buffer.Length;
        //         rc = compressor.Deflate(FlushType.None);
        //
        //         if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
        //             throw new Exception("deflating: " + compressor.Message);
        //
        //         ms.Write(compressor.OutputBuffer, 0, buffer.Length - compressor.AvailableBytesOut);
        //     } while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);
        //
        //     // pass 2: finish and flush
        //     do
        //     {
        //         compressor.NextOut = 0;
        //         compressor.AvailableBytesOut = buffer.Length;
        //         rc = compressor.Deflate(FlushType.Finish);
        //
        //         if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
        //             throw new Exception("deflating: " + compressor.Message);
        //
        //         if (buffer.Length - compressor.AvailableBytesOut > 0)
        //             ms.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);
        //     } while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);
        //
        //     compressor.EndDeflate();
        //
        //     ms.Seek(0, SeekOrigin.Begin);
        //     var compressedBytes = new byte[compressor.TotalBytesOut];
        //     ms.Read(compressedBytes, 0, compressedBytes.Length);
        //     return compressedBytes;
        // }

        #endregion

        #region IonicZip/IonicZlib Inflate

        // /// <summary>
        // /// IonicZlib库所实现的解压缩功能
        // /// </summary>
        // /// <param name="ins"></param>
        // /// <param name="outs"></param>
        // public static void InflateBufferWithIonicZlib(Stream ins, Stream outs)
        // {
        //     int cnt;
        //     byte[] buffer = new byte[BufferSize];
        //     while ((cnt = ins.Read(buffer, 0, BufferSize)) > 0)
        //     {
        //         InflateBufferWithIonicZlib(buffer, cnt, outs);
        //     }
        // }
        //
        // public static void InflateBufferWithIonicZlib(byte[] CompressedBytes, int length, Stream outs)
        // {
        //     //int bufferSize = 1024;
        //     byte[] buffer = new byte[BufferSize];
        //     ZlibCodec decompressor = new ZlibCodec();
        //
        //     int rc = decompressor.InitializeInflate();
        //
        //     decompressor.InputBuffer = CompressedBytes;
        //     decompressor.NextIn = 0;
        //     decompressor.AvailableBytesIn = length;
        //
        //     decompressor.OutputBuffer = buffer;
        //
        //     // pass 1: inflate 
        //     do
        //     {
        //         decompressor.NextOut = 0;
        //         decompressor.AvailableBytesOut = buffer.Length;
        //         rc = decompressor.Inflate(FlushType.None);
        //
        //         if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
        //             throw new Exception("inflating: " + decompressor.Message);
        //
        //         outs.Write(decompressor.OutputBuffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        //     }
        //     while (decompressor.AvailableBytesIn > 0 || decompressor.AvailableBytesOut == 0);
        //
        //     // pass 2: finish and flush
        //     do
        //     {
        //         decompressor.NextOut = 0;
        //         decompressor.AvailableBytesOut = buffer.Length;
        //         rc = decompressor.Inflate(FlushType.Finish);
        //
        //         if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
        //             throw new Exception("inflating: " + decompressor.Message);
        //
        //         if (buffer.Length - decompressor.AvailableBytesOut > 0)
        //             outs.Write(buffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        //     }
        //     while (decompressor.AvailableBytesIn > 0 || decompressor.AvailableBytesOut == 0);
        //
        //     decompressor.EndInflate();
        // }
        //
        // private void InflateBuffer(byte[] CompressedBytes, byte[] DecompressedBytes)
        // {
        //     //int bufferSize = 1024;
        //     byte[] buffer = new byte[BufferSize];
        //     ZlibCodec decompressor = new ZlibCodec();
        //
        //     //Console.WriteLine("\n============================================");
        //     //Console.WriteLine("Size of Buffer to Inflate: {0} bytes.", CompressedBytes.Length);
        //     MemoryStream ms = new MemoryStream(DecompressedBytes);
        //
        //     int rc = decompressor.InitializeInflate();
        //
        //     decompressor.InputBuffer = CompressedBytes;
        //     decompressor.NextIn = 0;
        //     decompressor.AvailableBytesIn = CompressedBytes.Length;
        //
        //     decompressor.OutputBuffer = buffer;
        //
        //     // pass 1: inflate 
        //     do
        //     {
        //         decompressor.NextOut = 0;
        //         decompressor.AvailableBytesOut = buffer.Length;
        //         rc = decompressor.Inflate(FlushType.None);
        //
        //         if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
        //             throw new Exception("inflating: " + decompressor.Message);
        //
        //         ms.Write(decompressor.OutputBuffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        //     }
        //     while (decompressor.AvailableBytesIn > 0 || decompressor.AvailableBytesOut == 0);
        //
        //     // pass 2: finish and flush
        //     do
        //     {
        //         decompressor.NextOut = 0;
        //         decompressor.AvailableBytesOut = buffer.Length;
        //         rc = decompressor.Inflate(FlushType.Finish);
        //
        //         if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
        //             throw new Exception("inflating: " + decompressor.Message);
        //
        //         if (buffer.Length - decompressor.AvailableBytesOut > 0)
        //             ms.Write(buffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        //     }
        //     while (decompressor.AvailableBytesIn > 0 || decompressor.AvailableBytesOut == 0);
        //
        //     decompressor.EndInflate();
        // }

        #endregion

        /// <summary>
        /// InflateBufferWithPureZlib
        /// </summary>
        /// <param name="compressedBytes"></param>
        /// <param name="length"></param>
        /// <param name="outs"></param>
        /// <exception cref="Exception"></exception>
        public static void InflateBufferWithPureZlib(byte[] compressedBytes, int length, Stream outs)
        {
            //int bufferSize = 1024;
            var buffer = new byte[BufferSize];
            var decompressor = new ZlibCodec();

            var rc = decompressor.InitializeInflate();

            decompressor.InputBuffer = compressedBytes;
            decompressor.NextIn = 0;
            decompressor.AvailableBytesIn = length;

            decompressor.OutputBuffer = buffer;

            // pass 1: inflate 
            do
            {
                decompressor.NextOut = 0;
                decompressor.AvailableBytesOut = buffer.Length;
                rc = decompressor.Inflate(FlushType.None);

                if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                    throw new Exception("inflating: " + decompressor.Message);

                outs.Write(decompressor.OutputBuffer, 0, buffer.Length - decompressor.AvailableBytesOut);
            } while (decompressor.AvailableBytesIn > 0 && decompressor.AvailableBytesOut == 0);

            // pass 2: finish and flush
            do
            {
                decompressor.NextOut = 0;
                decompressor.AvailableBytesOut = buffer.Length;
                rc = decompressor.Inflate(FlushType.Finish);

                if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                    throw new Exception("inflating: " + decompressor.Message);

                if (buffer.Length - decompressor.AvailableBytesOut > 0)
                    outs.Write(buffer, 0, buffer.Length - decompressor.AvailableBytesOut);
            } while (decompressor.AvailableBytesIn > 0 && decompressor.AvailableBytesOut == 0);

            decompressor.EndInflate();
        }

        /// <summary>
        /// Inflater解压 (不能证明此解压功能是正确的，有待测试，使用的 ICSharpZip/Zlib.Portable.Core 的相关功能)
        /// </summary>
        /// <param name="baseBytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="noHeader"></param>
        /// <returns></returns>
        public static byte[] InflaterDecompress(byte[] baseBytes, int offset, int count, bool noHeader = true)
        {
            var resultStr = string.Empty;
            using var memoryStream = new MemoryStream(baseBytes, offset, count);
            using var buffer = new MemoryStream();
            InflaterDecompress(memoryStream, offset, count, noHeader, buffer);
            return buffer.ToArray();
        }

        private static void InflaterDecompress(Stream stream, int offset, int count, bool noHeader, Stream outStream)
        {
            var inflater = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater(noHeader);
            {
                using var inf =
                    new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(stream, inflater);

                var result = new byte[1024];
                int resLen;

                while ((resLen = inf.Read(result, 0, result.Length)) > 0)
                {
                    outStream.Write(result, 0, resLen);
                }

                //resultStr = Encoding.Default.GetString(result);
                return;
            }
        }

        public static void InflaterDecompressBuffer(byte[] baseBytes, int offset, int count, Stream outStream,
            bool noHeader = true)
        {
            using var memoryStream = new MemoryStream(baseBytes, offset, count);
            try
            {
                InflaterDecompressBuffer(memoryStream, outStream, noHeader);
            }
            finally
            {
                memoryStream.Close();
            }
        }

        public static void InflaterDecompressBuffer(Stream inStream, Stream outStream, bool noHeader = true)
        {
            var inflater = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater(noHeader);
            var result = new byte[1024];
            using var inf = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(inStream, inflater);

            try
            {
                int resLen;
                while ((resLen = inf.Read(result, 0, result.Length)) > 0)
                {
                    outStream.Write(result, 0, resLen);
                }

                //resultStr = Encoding.Default.GetString(result);
            }
            finally
            {
                inf.Close();
            }
        }

        #region Aced Inflator/Deflator Compress

        public static bool acedCompress(string inName, string outName)
        {
            try
            {
                using var fsIn = new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize);
                return acedCompress(fsIn, outName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool acedCompress(Stream fsIn, string outName)
        {
            try
            {
                using var fsOut = File.Create(outName);
                return acedCompress(fsIn, fsOut);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool acedCompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];
            AcedCompressionLevel level = AcedCompressionLevel.Fast;
            try
            {
                while (true)
                {
                    count = fsIn.Read(buf, 0, BufSize);
                    if (count != 0)
                    {
                        byte[] _compData = AcedDeflator.Instance.Compress(buf, 0, count, level, 0, 0);
                        fsOut.Write(_compData, 0, _compData.Length);
                        _compData = null;
                    }

                    if (count != BufSize)
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

        #region Aced Inflator/Deflator Decompress

        public static bool acedDecompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return acedDecompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool acedDecompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return acedDecompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool acedDecompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];
            _checkSum = 0;

            try
            {
                while (true)
                {
                    count = fsIn.Read(buf, 0, BufSize);
                    if (count != 0)
                    {
                        if (count < BufSize)
                        {
                            byte[] x = new byte[count];
                            Array.Copy(buf, x, count);
                            buf = null;
                            buf = x;
                        }

                        byte[] _decompData = AcedInflator.Instance.Decompress(buf, 0, 0, 0);
                        _checkSum += AcedUtils.Adler32(_decompData, 0, _decompData.Length);
                        fsOut.Write(_decompData, 0, _decompData.Length);
                        _decompData = null;
                    }

                    if (count != BufSize)
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion


        private static int _checkSum;

        public static int acedLastDecompressCheckSum
        {
            get { return _checkSum; }
        }


        #region MiniLZO Compress

        public static bool miniLzoCompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return miniLzoCompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool miniLzoCompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return miniLzoCompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool miniLzoCompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];
            byte[] _compData;
            byte b;
            try
            {
                while (true)
                {
                    count = fsIn.Read(buf, 0, BufSize);
                    if (count != 0)
                    {
                        //compressing.Write(buf, 0, count);
                        if (count < BufSize)
                        {
                            byte[] x = new byte[count];
                            Array.Copy(buf, x, count);
                            buf = null;
                            buf = x;
                        }

                        MiniLZO.Compress(buf, out _compData);
                        b = (byte) (_compData.Length & 0xff);
                        fsOut.WriteByte(b);
                        b = (byte) ((_compData.Length >> 8) & 0xff);
                        fsOut.WriteByte(b);
                        fsOut.Write(_compData, 0, _compData.Length);
                        _compData = null;
                    }

                    if (count != BufSize)
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

        #region MiniLZO Decompress

        public static bool miniLzoDecompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return miniLzoDecompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool miniLzoDecompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return miniLzoDecompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool miniLzoDecompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf;
            //_checkSum = 0;
            byte[] _decompData = new byte[BufSize];
            //byte b;

            try
            {
                while (true)
                {
                    count = (int) fsIn.ReadByte();
                    count += (fsIn.ReadByte() << 8);
                    buf = new byte[count];
                    count = fsIn.Read(buf, 0, count);
                    if (count != 0)
                    {
                        int _decompSize = MiniLZO.Decompress(buf, _decompData);
                        //_checkSum += AcedUtils.Adler32(_decompData, 0, _decompData.Length);
                        fsOut.Write(_decompData, 0, _decompSize);
                        _decompData = null;
                    }

                    buf = null;

                    if (count != BufSize)
                        break;
                    if (count == 0)
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion


        #region QuickLZ Compress

        public static bool quickLzCompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return quickLzCompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool quickLzCompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return quickLzCompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool quickLzCompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];
            byte[] _compData;
            byte b;
            try
            {
                while (true)
                {
                    count = fsIn.Read(buf, 0, BufSize);
                    if (count != 0)
                    {
                        //if (count < BufSize)
                        //{
                        //    byte[] x = new byte[count];
                        //    Array.Copy(buf, x, count);
                        //    buf = null;
                        //    buf = x;
                        //}
                        _compData = QuickLZ.Compress(buf, 0u, (uint) count);

                        b = (byte) (_compData.Length & 0xff);
                        fsOut.WriteByte(b);
                        b = (byte) ((_compData.Length >> 8) & 0xff);
                        fsOut.WriteByte(b);
                        fsOut.Write(_compData, 0, _compData.Length);
                        _compData = null;
                    }

                    if (count != BufSize)
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

        #region QuickLZ Decompress

        public static bool quickLzDecompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return quickLzDecompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool quickLzDecompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return quickLzDecompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool quickLzDecompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf;
            //_checkSum = 0;
            //byte b;

            try
            {
                while (true)
                {
                    count = (int) fsIn.ReadByte();
                    count += (fsIn.ReadByte() << 8);
                    buf = new byte[count];
                    count = fsIn.Read(buf, 0, count);
                    if (count != 0)
                    {
                        byte[] _decompData = QuickLZ.Decompress(buf, 0);
                        //_checkSum += AcedUtils.Adler32(_decompData, 0, _decompData.Length);
                        fsOut.Write(_decompData, 0, _decompData.Length);
                        _decompData = null;
                    }
                    else
                        break;

                    buf = null;

                    //if (count != BufSize)
                    //    break;
                    //if (count == 0)
                    //    break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion


        #region SharpZIP.LZW Decompress

        //public static bool sharpLzwDecompress(string inName, string outName)
        //{
        //    try
        //    {
        //        using (FileStream fsIn = new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
        //        {
        //            return sharpLzwDecompress(fsIn, outName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }
        //    return false;
        //}
        //public static bool sharpLzwDecompress(Stream fsIn, string outName)
        //{
        //    try
        //    {
        //        using (FileStream fsOut = File.Create(outName))
        //        {
        //            return sharpLzwDecompress(fsIn, fsOut);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }
        //    return false;
        //}
        //public static bool sharpLzwDecompress(Stream fsIn, Stream fsOut)
        //{
        //    int count;
        //    byte[] buf = new byte[BufSize];
        //    try
        //    {
        //        using (LzwInputStream lzwis = new LzwInputStream(fsIn))
        //        {
        //            while (true)
        //            {
        //                count = lzwis.Read(buf, 0, BufSize);
        //                if (count != 0)
        //                {
        //                    fsOut.Write(buf, 0, count);
        //                }

        //                if (count != BufSize)
        //                    break;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }
        //    return false;
        //}

        #endregion

        #region SharpZIP.BZIP2 Decompress

        public static bool sharpBzip2Decompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return sharpBzip2Decompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool sharpBzip2Decompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return sharpBzip2Decompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool sharpBzip2Decompress(Stream fsIn, Stream fsOut)
        {
            int count;
            byte[] buf = new byte[BufSize];
            try
            {
                using (BZip2InputStream lzwis = new BZip2InputStream(fsIn))
                {
                    while (true)
                    {
                        count = lzwis.Read(buf, 0, BufSize);
                        if (count != 0)
                        {
                            fsOut.Write(buf, 0, count);
                        }

                        if (count != BufSize)
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion


        #region LZ77, LZ78, LZW,

        public static bool lz77Decompress(string inName, string outName)
        {
            try
            {
                using (FileStream fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
                {
                    return lz77Decompress(fsIn, outName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool lz77Decompress(Stream fsIn, string outName)
        {
            try
            {
                using (FileStream fsOut = File.Create(outName))
                {
                    return lz77Decompress(fsIn, fsOut);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        /// <summary>
        /// TODO!
        /// </summary>
        /// <param name="fsIn"></param>
        /// <param name="fsOut"></param>
        /// <returns></returns>
        public static bool lz77Decompress(Stream fsIn, Stream fsOut)
        {
            lz77DeCompression oi = new lz77DeCompression();
            //byte[] buf = new byte[32768];
            //int cnt;
            //while((cnt=fsIn.Read(buf, 0, buf.Length)>0){
            //    ;
            //    string outbuf = oi.decompress(new string(buf));
            //}
            return false;
        }

        #endregion

        #region lz77DeCompression class

        public class lz77DeCompression
        {
            private char ReferencePrefix;
            private int ReferencePrefixCode;
            private int ReferenceIntBase;
            private int ReferenceIntFloorCode;
            private int ReferenceIntCeilCode;
            private int MaxStringDistance;
            private int MinStringLength;
            private int MaxStringLength;
            private int MaxWindowLength;

            public lz77DeCompression()
            {
                this.ReferencePrefix = '`';
                this.ReferencePrefixCode = (int) this.ReferencePrefix;
                this.ReferenceIntBase = 96;
                this.ReferenceIntFloorCode = (int) ' ';
                this.ReferenceIntCeilCode = this.ReferenceIntFloorCode + this.ReferenceIntBase - 1;
                this.MaxStringDistance = (int) Math.Pow(this.ReferenceIntBase, 2) - 1;
                this.MinStringLength = 5;
                this.MaxStringLength = (int) Math.Pow(this.ReferenceIntBase, 1) - 1 + this.MinStringLength;
                this.MaxWindowLength = this.MaxStringDistance + this.MinStringLength;
            }

            private int decodeReferenceInt(string words, int width)
            {
                int value;
                int i;
                int charcode;
                value = 0;

                for (i = 0; i < width; i++)
                {
                    value *= this.ReferenceIntBase;
                    charcode = (int) words[i];

                    if ((charcode >= this.ReferenceIntFloorCode) && (charcode <= this.ReferenceIntCeilCode))
                    {
                        value += charcode - this.ReferenceIntFloorCode;
                    }


                    /* else
                    {
                    Response.Write ( "<script type=\"javascript\">alert("+ charcode+")<//script>");
                    }*/
                }

                return value;
            }

            private int decodeReferenceInt(char words, int width)
            {
                int value;
                int i;
                int charcode;
                value = 0;

                for (i = 0; i < width; i++)
                {
                    value *= this.ReferenceIntBase;
                    charcode = (int) words;

                    if ((charcode >= this.ReferenceIntFloorCode) && (charcode <= this.ReferenceIntCeilCode))
                    {
                        value += charcode - this.ReferenceIntFloorCode;
                    }


                    /* else
                    {
                    Response.Write ( "<script type=\"javascript\">alert("+ charcode+")<//script>");
                    }*/
                }

                return value;
            }

            private int decodeReferenceLength(char words)
            {
                return decodeReferenceInt(words, 1) + this.MinStringLength;
            }

            public string decompress(string words)
            {
                string decompressed;
                int pos, distance, length;
                int getSubString;
                char currentChar;
                char nextChar;
                decompressed = "";
                pos = 0;
                while (pos < words.Length)
                {
                    currentChar = words[pos];
                    if (currentChar != this.ReferencePrefix)
                    {
                        decompressed += currentChar;
                        pos++;
                    }
                    else
                    {
                        nextChar = words[pos + 1];
                        if (nextChar != this.ReferencePrefix)
                        {
                            distance = decodeReferenceInt(words.Substring(pos + 1, 2), 2);
                            length = decodeReferenceLength(words[pos + 3]);
                            getSubString = decompressed.Length - distance - length;
                            decompressed += decompressed.Substring(getSubString, length);
                            pos += this.MinStringLength - 1;
                        }
                        else
                        {
                            decompressed += this.ReferencePrefix;
                            pos += 2;
                        }
                    }
                }

                return decompressed;
            }
        }

        //and this the JSCRIPT code 
        ////lz77 class
        //ReferencePrefix = "`";
        //ReferencePrefixCode = ReferencePrefix.charCodeAt(0);
        //
        //ReferenceIntBase = 96;
        //ReferenceIntFloorCode = " ".charCodeAt(0);
        //ReferenceIntCeilCode = ReferenceIntFloorCode + ReferenceIntBase - 1;
        //
        //MaxStringDistance = Math.pow(ReferenceIntBase, 2) - 1;
        //MinStringLength = 5;
        //MaxStringLength = Math.pow(ReferenceIntBase, 1) - 1 + MinStringLength;
        //
        //MaxWindowLength = MaxStringDistance + MinStringLength;
        //
        //function decodeReferenceInt(data, width) {
        //var value = 0;
        //for (var i = 0; i < width; i++) {
        //value *= ReferenceIntBase;
        //var charCode = data.charCodeAt(i);
        //if ((charCode >= ReferenceIntFloorCode) && (charCode <= ReferenceIntCeilCode)) {
        //value += charCode - ReferenceIntFloorCode;
        //} else {
        //throw "Invalid char code in reference int: " + charCode;
        //}
        //}
        //return value;
        //}
        //
        //function decodeReferenceLength(data) {
        //return decodeReferenceInt(data, 1) + MinStringLength;
        //}
        //
        //function decompress(data) {
        //var decompressed = "";
        //var pos = 0;
        //while (pos < data.length) {
        //var currentChar = data.charAt(pos);
        //if (currentChar != ReferencePrefix) {
        //decompressed += currentChar;
        //pos++;
        //} else {
        //var nextChar = data.charAt(pos + 1);
        //if (nextChar != ReferencePrefix) {
        //var distance = decodeReferenceInt(data.substr(pos + 1, 2), 2);
        //var length = decodeReferenceLength(data.charAt(pos + 3));
        //decompressed += decompressed.substr(decompressed.length - distance - length, length);
        //pos += MinStringLength - 1;
        //} else {
        //decompressed += ReferencePrefix;
        //pos += 2;
        //}
        //}
        //}
        //return decompressed;
        //}
        //end class

        #endregion

        //Huffman

        //bzip2
    }
}