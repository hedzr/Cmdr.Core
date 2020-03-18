using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace HzNS.MdxLib.Compression
{
    /// <summary> 
    /// zip 压缩 
    /// </summary> 
    public class Zip
    {
        /// <summary> 
        /// 字符串压缩到字节数组 
        /// 返回：已压缩的字节数组 
        /// </summary> 
        /// <param name="stringToCompress">待压缩的字符串</param> 
        /// <returns></returns> 
        public static byte[] Compress(string stringToCompress)
        {
            var bytData = Encoding.UTF8.GetBytes(stringToCompress);
            var compressedData = CompressBytes(bytData);
            return compressedData;
        }

        /// <summary> 
        /// 字节数组解压缩到字符串 
        /// 返回：已压缩的字符串 
        /// </summary> 
        /// <param name="bytData">待解压缩的字节数组</param> 
        /// <returns></returns> 
        public static string DeCompress(byte[] bytData)
        {
            var decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        /// 字符串压缩 
        /// 返回：已压缩的字符串 
        /// </summary> 
        /// <param name="stringToCompress">待压缩的字符串</param> 
        /// <returns></returns> 
        public static string CompressString(string stringToCompress)
        {
            var bytData = Encoding.UTF8.GetBytes(stringToCompress);
            var compressedData = CompressBytes(bytData);
            return Convert.ToBase64String(compressedData);
        }

        /// <summary> 
        /// 字符串解压缩 
        /// 返回：已压缩的字符串 
        /// </summary> 
        /// <param name="compressTostring">待解压缩的字符串</param> 
        /// <returns></returns> 
        public static string DeCompressString(string compressTostring)
        {
            var bytData = Convert.FromBase64String(compressTostring);
            var decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        /// 字节数组压缩 
        /// 返回：已压缩的字节数组 
        /// </summary> 
        /// <param name="data">待压缩的字节数组</param> 
        /// <returns></returns> 
        public static byte[] CompressBytes(byte[] data)
        {
            var f = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(ICSharpCode.SharpZipLib.Zip.Compression
                .Deflater.BEST_COMPRESSION);
            f.SetInput(data);
            f.Finish();

            var o = new MemoryStream(data.Length);

            try
            {
                var buf = new byte[1024];
                while (!f.IsFinished)
                {
                    var got = f.Deflate(buf);
                    o.Write(buf, 0, got);
                }
            }
            finally
            {
                o.Close();
            }

            return o.ToArray();
        }

        /// <summary> 
        /// 字节数组解压缩 
        /// 返回：已解压缩的字节数组 
        /// </summary> 
        /// <param name="data">待解压缩的字节数组</param> 
        /// <returns></returns> 
        public static byte[] DecompressBytes(byte[] data)
        {
            var f = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
            f.SetInput(data);

            var o = new MemoryStream(data.Length);
            try
            {
                var buf = new byte[1024];
                while (!f.IsFinished)
                {
                    var got = f.Inflate(buf);
                    o.Write(buf, 0, got);
                }
            }
            finally
            {
                o.Close();
            }

            return o.ToArray();
        }
    }

    /// <summary> 
    /// gzip 的摘要说明。 
    /// </summary> 
    public class Gzip
    {
        // /// <summary> 
        // /// 实时压缩目标流，WEB输出专用功能 
        // /// </summary> 
        // public static Stream BaseStream => HttpContext.Current.Items["OutStream"] as Stream;
        //
        // /// <summary> 
        // /// 将字节数组写入目标压缩流，WEB输出专用功能 
        // /// </summary> 
        // /// <param name="buffer">要写入的字节数组</param> 
        // public static void Write(byte[] buffer)
        // {
        //     if (HttpContext.Current.Response.ContentType.IndexOf(';') == -1 && HttpContext.Current.Response.ContentType.StartsWith("text", true, null))
        //     {
        //         HttpContext.Current.Response.ContentType += "; charset=" + HttpContext.Current.Response.ContentEncoding.WebName;
        //     }
        //     ((Stream)HttpContext.Current.Items["OutStream"]).Write(buffer, 0, buffer.Length);
        // }
        //
        // /// <summary> 
        // /// 清除当前压缩流中的所有内容，WEB输出专用功能 
        // /// </summary> 
        // public static void Clear()
        // {
        //     string acceptEncoding = HttpContext.Current.Request.Headers.Get("Accept-Encoding");
        //     if (acceptEncoding != null && acceptEncoding.ToLower().IndexOf("gzip", StringComparison.Ordinal) > -1)
        //     {
        //         ((Stream)HttpContext.Current.Items["OutStream"]).Close();
        //         HttpContext.Current.Response.ClearContent();
        //         HttpContext.Current.Items["OutStream"] = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(HttpContext.Current.Response.OutputStream);
        //     }
        //     else
        //     {
        //         HttpContext.Current.Response.ClearContent();
        //     }
        // }
        //
        // /// <summary> 
        // /// 将XML对象写入目标压缩流，WEB输出专用功能 
        // /// </summary> 
        // /// <param name="xml">XmlDocument对象</param> 
        // public static void Write(XmlDocument xml)
        // {
        //     xml.Save(BaseStream);
        // }
        //
        // /// <summary> 
        // /// 将一个可转换为字符串的object对象写入目标压缩流，WEB输出专用功能 
        // /// </summary> 
        // /// <param name="num">要写入的对象</param> 
        // public static void Write(object num)
        // {
        //     Write(num.ToString());
        // }
        //
        // /// <summary> 
        // /// 将一个字符串写入目标压缩流，WEB输出专用功能 
        // /// </summary> 
        // /// <param name="str">要写入的字符串</param> 
        // public static void Write(string str)
        // {
        //     Write(HttpContext.Current.Response.ContentEncoding.GetBytes(str));
        // }
        //
        // /// <summary> 
        // /// 将一个流压缩到目标压缩流，WEB输出专用功能 
        // /// </summary> 
        // /// <param name="stream">要写入的流</param> 
        // public static void Write(Stream stream)
        // {
        //     var len = (int)stream.Length;
        //     var data = new byte[len];
        //     stream.Read(data, 0, len);
        //     Write(data);
        // }
        //
        // /// <summary> 
        // /// 将一个文件压缩到目标压缩流，WEB输出专用功能 
        // /// </summary> 
        // /// <param name="filepath">要写入的文件路径</param> 
        // public static void WriteFile(string filepath)
        // {
        //     if (File.Exists(filepath))
        //     {
        //         FileStream fStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        //         Write(fStream);
        //         fStream.Close();
        //     }
        //     else
        //     {
        //         Debug.WriteLine("文件" + Path.GetFileName(filepath) + "不存在");
        //     }
        // }

        /// <summary> 
        /// 将字符串压缩为字节数组 
        /// 返回：已压缩的字节数组 
        /// </summary> 
        /// <param name="stringToCompress">待压缩的字符串</param>
        /// <returns></returns> 
        public static byte[] Compress(string stringToCompress)
        {
            byte[] bytData = Encoding.UTF8.GetBytes(stringToCompress);
            return CompressBytes(bytData);
        }

        /// <summary> 
        /// 解压缩字节数组到字符串 
        /// 返回：已解压的字符串（慎用） 
        /// </summary> 
        /// <param name="bytData">待解压缩的字节数组</param> 
        /// <returns></returns> 
        public static string DeCompress(byte[] bytData)
        {
            byte[] decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        /// 压缩字符串 
        /// 返回：已压缩的字符串 
        /// </summary> 
        /// <param name="stringToCompress">待压缩的字符串组</param> 
        /// <returns></returns> 
        public static string CompressString(string stringToCompress)
        {
            byte[] bytData = Encoding.UTF8.GetBytes(stringToCompress);
            byte[] compressedData = CompressBytes(bytData);
            return Convert.ToBase64String(compressedData);
        }

        /// <summary> 
        /// 解压缩字符串 
        /// 返回：已解压的字符串 
        /// </summary> 
        /// <param name="compressToString">待解压缩的字符串</param> 
        /// <returns></returns> 
        public static string DeCompressString(string compressToString)
        {
            var bytData = Convert.FromBase64String(compressToString);
            var decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        /// 压缩字节数组 
        /// 返回：已压缩的字节数组 
        /// </summary> 
        /// <param name="byteData">待压缩的字节数组</param>
        /// <returns></returns> 
        public static byte[] CompressBytes(byte[] byteData)
        {
            var o = new MemoryStream();
            var s = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(o);
            s.Write(byteData, 0, byteData.Length);
            s.Close();
            o.Flush();
            o.Close();
            return o.ToArray();
        }

        /// <summary> 
        /// 解压缩字节数组 
        /// 返回：已解压的字节数组 
        /// </summary> 
        /// <param name="data">待解压缩的字节数组</param> 
        /// <returns></returns> 
        public static byte[] DecompressBytes(byte[] data)
        {
            var o = new MemoryStream();
            var s = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(new MemoryStream(data));
            try
            {
                var size = 0;
                var buf = new byte[1024];
                while ((size = s.Read(buf, 0, buf.Length)) > 0)
                {
                    o.Write(buf, 0, size);
                }
            }
            finally
            {
                o.Close();
            }

            return o.ToArray();
        }
    }

    /// <summary> 
    /// tar 的摘要说明。 
    /// </summary> 
    public class Tar
    {
        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="stringToCompress"></param> 
        /// <returns></returns> 
        public static byte[] Compress(string stringToCompress)
        {
            var bytData = Encoding.UTF8.GetBytes(stringToCompress);
            return CompressBytes(bytData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="bytData"></param> 
        /// <returns></returns> 
        public static string DeCompress(byte[] bytData)
        {
            var decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="stringToCompress"></param> 
        /// <returns></returns> 
        public static string CompressString(string stringToCompress)
        {
            var bytData = Encoding.UTF8.GetBytes(stringToCompress);
            var compressedData = CompressBytes(bytData);
            return Convert.ToBase64String(compressedData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="compressToString"></param> 
        /// <returns></returns> 
        public static string DeCompressString(string compressToString)
        {
            var bytData = Convert.FromBase64String(compressToString);
            var decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="data"></param> 
        /// <returns></returns> 
        public static byte[] CompressBytes(byte[] data)
        {
            var o = new MemoryStream();
            var s = new ICSharpCode.SharpZipLib.Tar.TarOutputStream(o);
            s.Write(data, 0, data.Length);
            s.Close();
            o.Flush();
            o.Close();
            return o.ToArray();
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="data"></param> 
        /// <returns></returns> 
        public static byte[] DecompressBytes(byte[] data)
        {
            var o = new MemoryStream();
            var s = new ICSharpCode.SharpZipLib.Tar.TarInputStream(new MemoryStream(data));
            try
            {
                var size = 0;
                var buf = new byte[1024];
                while ((size = s.Read(buf, 0, buf.Length)) > 0)
                {
                    o.Write(buf, 0, size);
                }
            }
            finally
            {
                o.Close();
            }

            return o.ToArray();
        }
    }

    /// <summary> 
    /// bzip 的摘要说明。 
    /// </summary> 
    public class Bzip
    {
        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="stringToCompress"></param> 
        /// <returns></returns> 
        public static byte[] Compress(string stringToCompress)
        {
            var bytData = Encoding.UTF8.GetBytes(stringToCompress);
            return CompressBytes(bytData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="bytData"></param> 
        /// <returns></returns> 
        public static string DeCompress(byte[] bytData)
        {
            var decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="stringToCompress"></param> 
        /// <returns></returns> 
        public static string CompressString(string stringToCompress)
        {
            var bytData = Encoding.UTF8.GetBytes(stringToCompress);
            var compressedData = CompressBytes(bytData);
            return Convert.ToBase64String(compressedData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="CompressTostring"></param> 
        /// <returns></returns> 
        public static string DeCompressString(string CompressTostring)
        {
            var bytData = Convert.FromBase64String(CompressTostring);
            var decompressedData = DecompressBytes(bytData);
            return Encoding.UTF8.GetString(decompressedData);
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="data"></param> 
        /// <returns></returns> 
        public static byte[] CompressBytes(byte[] data)
        {
            var o = new MemoryStream();
            var s = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(o);
            s.Write(data, 0, data.Length);
            s.Close();
            o.Flush();
            o.Close();
            return o.ToArray();
        }

        /// <summary> 
        ///  
        /// </summary> 
        /// <param name="data"></param> 
        /// <returns></returns> 
        public static byte[] DecompressBytes(byte[] data)
        {
            var o = new MemoryStream();
            var s = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(new MemoryStream(data));
            try
            {
                var size = 0;
                var buf = new byte[1024];
                while ((size = s.Read(buf, 0, buf.Length)) > 0)
                {
                    o.Write(buf, 0, size);
                }
            }
            finally
            {
                o.Close();
            }

            return o.ToArray();
        }
    }
}