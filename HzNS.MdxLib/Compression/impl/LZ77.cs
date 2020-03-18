using System;
using System.Diagnostics;
using System.IO;

namespace HzNS.MdxLib.Compression.impl
{
    public class LZ77
    {
        public LZ77()
        {
        }

        #region 压缩数据

        public static void ZipData(string sourceFilePath, string targetFilePath)
        {
            using var data = new FileStream(sourceFilePath, FileMode.Open);
            using var zippedData = new FileStream(targetFilePath, FileMode.Create);
            try
            {
                ZipData(data, zippedData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            finally
            {
                data.Close();
                zippedData.Close();
            }
        }

        public static void ZipData(Stream data, Stream zippedData)
        {
            /////////////////////////////////
            //       初始化变量
            /////////////////////////////////

            int p, wPos, wLen, tPos, tLen;
            var dataLength = (int) data.Length;
            var bufferLength = 0;
            var buffer = new byte[4096];
            var bufferZip = new byte[8092];
            var nextSamePoint = new int[4096];
            int bufferDP;
            bool f, ff;
            byte tByteLen = 0;
            var tInt = 0;

            /////////////////////////////////
            //     开始分组压缩
            /////////////////////////////////

            while ((bufferLength = data.Read(buffer, 0, 4096)) > 0)
            {
                // 如果buffer长度太短直接保存
                if (bufferLength < 6)
                {
                    for (var i = 0; i < Math.Min(3, bufferLength); i++)
                    {
                        tInt = (tInt << 8) + buffer[i];
                        zippedData.WriteByte((byte) (tInt >> tByteLen));
                        tInt = getRightByte(tInt, tByteLen);
                    }

                    for (var i = 3; i < bufferLength; i++)
                    {
                        tInt = (tInt << 9) + buffer[i];
                        tByteLen += 9;
                        while (tByteLen >= 8)
                        {
                            zippedData.WriteByte((byte) (tInt >> (tByteLen -= 8) << 24 >> 24));
                        }

                        tInt = getRightByte(tInt, tByteLen); // 记录剩余信息
                    }

                    break;
                }

                // 提前计算出下一个相同字节的位置
                for (var i = 0; i < bufferLength; i++)
                {
                    for (var j = i + 1; j < bufferLength; j++)
                    {
                        nextSamePoint[i] = 4096;
                        if (buffer[i] != buffer[j]) continue;
                        nextSamePoint[i] = j;
                        break;
                    }
                }

                // 初始化字典和滑块
                tInt = (tInt << 8) + buffer[0];
                bufferZip[0] = (byte) (tInt >> tByteLen);
                tInt = getRightByte(tInt, tByteLen);

                tInt = (tInt << 8) + buffer[1];
                bufferZip[1] = (byte) (tInt >> tByteLen);
                tInt = getRightByte(tInt, tByteLen);

                tInt = (tInt << 8) + buffer[2];
                bufferZip[2] = (byte) (tInt >> tByteLen);
                tInt = getRightByte(tInt, tByteLen);

                p = 3;
                wPos = 0;
                wLen = 3;
                tPos = -1;
                tLen = 0;
                bufferDP = 2;

                ////////////////////////////////
                //        开始编码
                ////////////////////////////////

                while (bufferLength - 3 >= p)
                {
                    while (wPos + wLen <= p && p + wLen <= bufferLength)
                    {
                        // 判断滑块是否匹配
                        f = true;
                        if (buffer[wPos] == buffer[p])
                        {
                            ff = true;
                            for (var i = 1; i < wLen; i++)
                            {
                                if (buffer[wPos + i] == buffer[p + i]) continue;
                                f = false;
                                break;
                            }
                        }
                        else
                        {
                            f = false;
                            ff = false;
                        }

                        // 匹配则增加滑块长度，否则移动
                        if (!f)
                        {
                            // 滑块移动
                            if (ff)
                            {
                                wPos = nextSamePoint[wPos];
                            }
                            else
                            {
                                wPos++;
                            }
                        }
                        else
                        {
                            // 增加滑块长度
                            while (wPos + wLen != p && p + wLen != bufferLength && wLen < 1024)
                            {
                                if (buffer[wPos + wLen] == buffer[p + wLen])
                                {
                                    wLen++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            tPos = wPos;
                            tLen = wLen;

                            // 滑块移动并增加1长度
                            wPos = nextSamePoint[wPos];
                            wLen++;
                        }
                    }

                    //////////////////////////////////
                    //     写入buffer_zip
                    //////////////////////////////////

                    if (tPos == -1)
                    {
                        // 单个字节
                        tInt = (tInt << 9) + buffer[p];
                        tByteLen += 9;
                        p++;
                    }
                    else
                    {
                        // 匹配的字节串
                        tInt = (((tInt << 1) + 1 << 12) + tPos << 11) + tLen;
                        tByteLen += 24;
                        p += tLen;
                    }

                    while (tByteLen >= 8)
                    {
                        bufferZip[++bufferDP] = (byte) (tInt >> (tByteLen -= 8) << 24 >> 24);
                    }

                    tInt = getRightByte(tInt, tByteLen); // 记录剩余信息

                    ////////////////////////////////////
                    //        初始化滑块
                    ////////////////////////////////////

                    wPos = 0;
                    wLen = 3;
                    tPos = -1;
                    tLen = 0;
                }

                // 写入剩余字节
                for (var i = p; i < bufferLength; i++)
                {
                    tInt = (tInt << 9) + buffer[i];
                    tByteLen += 9;
                    bufferZip[++bufferDP] = (byte) (tInt >> (tByteLen -= 8));
                    tInt = getRightByte(tInt, tByteLen);
                }

                // 写入Stream
                zippedData.Write(bufferZip, 0, bufferDP + 1);
            }

            if (tByteLen != 0) zippedData.WriteByte((byte) (tInt << 8 - tByteLen)); // 写入剩余信息
        }

        #endregion

        #region 解压数据

        public static void UnzipData(string zippedFilePath, string targetFilePath)
        {
            using var zippedData = new FileStream(zippedFilePath, FileMode.Open);
            using var data = new FileStream(targetFilePath, FileMode.Create);
            try
            {
                UnzipData(zippedData, data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            finally
            {
                data.Close();
                zippedData.Close();
            }
        }

        public static void UnzipData(Stream zippedData, Stream data)
        {
            /////////////////////////////////
            //       初始化变量
            /////////////////////////////////

            uint wPos = 0, wLen = 0;
            var bufferZipLength = (int) zippedData.Length;
            var buffer = new byte[4096];
            var bufferZip = new byte[bufferZipLength];
            var bufferZipDp = -1;
            var bufferDP = -1;
            byte tByteLen = 0;
            uint tInt = 0;
            zippedData.Read(bufferZip, 0, bufferZipLength);

            /////////////////////////////////
            //        开始解压
            /////////////////////////////////

            while (bufferZipDp < bufferZipLength - 3)
            {
                // 写入前3个字节
                tInt = (tInt << 8) + bufferZip[++bufferZipDp];
                buffer[0] = (byte) (tInt >> tByteLen);
                tInt = getRightByte(tInt, tByteLen);

                tInt = (tInt << 8) + bufferZip[++bufferZipDp];
                buffer[1] = (byte) (tInt >> tByteLen);
                tInt = getRightByte(tInt, tByteLen);

                tInt = (tInt << 8) + bufferZip[++bufferZipDp];
                buffer[2] = (byte) (tInt >> tByteLen);
                tInt = getRightByte(tInt, tByteLen);

                bufferDP = 2;

                while (bufferDP < 4095)
                {
                    // 读入充足数据
                    while (tByteLen < 24 && bufferZipDp < bufferZipLength - 1)
                    {
                        tInt = (tInt << 8) + bufferZip[++bufferZipDp];
                        tByteLen += 8;
                    }

                    // 写入buffer
                    if (tInt >> tByteLen - 1 == 0)
                    {
                        // 单个字节
                        tByteLen -= 1;
                        tInt = getRightByte(tInt, tByteLen);
                        buffer[++bufferDP] = (byte) (tInt << 32 - tByteLen >> 24);
                        tByteLen -= 8;
                        tInt = getRightByte(tInt, tByteLen);
                    }
                    else
                    {
                        // 字节串
                        tByteLen -= 1;
                        tInt = getRightByte(tInt, tByteLen);
                        wPos = tInt >> (tByteLen -= 12);
                        tInt = getRightByte(tInt, tByteLen);
                        wLen = tInt >> (tByteLen -= 11);
                        tInt = getRightByte(tInt, tByteLen);
                        for (var i = 0; i < wLen; i++)
                        {
                            buffer[++bufferDP] = buffer[wPos + i];
                        }
                    }

                    // 判断是否解压完成
                    if (bufferZipDp == bufferZipLength - 1 && tByteLen < 9) break;
                }

                // 写入Stream
                data.Write(buffer, 0, bufferDP + 1);
            }

            // 写入剩余的信息
            for (var i = bufferZipDp + 1; i < bufferZipLength; i++)
            {
                data.WriteByte(bufferZip[i]);
            }
        }

        #endregion

        #region private items

        private static int getLeftByte(int dataLine, int s)
        {
            return dataLine << 32 - s >> 24;
        }

        private static uint getLeftByte(uint dataLine, int s)
        {
            return dataLine << 32 - s >> 24;
        }

        private static int getRightByte(int dataLine, int l)
        {
            if (l == 0)
            {
                return 0;
            }
            else
            {
                return dataLine << (32 - l) >> (32 - l);
            }
        }

        private static uint getRightByte(uint dataLine, int l)
        {
            if (l == 0)
            {
                return 0;
            }
            else
            {
                return dataLine << (32 - l) >> (32 - l);
            }
        }

        #endregion
    }
}