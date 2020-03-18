using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * ManagedQLZ is a fully ported C# implementation of QuickLZ by Lasse Mikkel Reinhold
 *
 * @Author Shane Bryldt, Copyright (C) 2006
 *
 * ManagedQLZ follows the same public GPL liscensing as the original QuickLZ
 * Commercial liscensing is unavailable for the C# port
 */

/*                    QuickLZ 1.10 data compression library
                  Copyright (C) 2006 by Lasse Mikkel Reinhold

QuickLZ can be used for free under the GPL license (where anything released into
public must be open source) or under a commercial license if such has been 
acquired (see http://www.quicklz.com/order.html).
*/

namespace HzNS.MdxLib.Compression.impl
{
    /// <summary>
    /// http://www.codeproject.com/KB/recipes/ManagedQLZ.aspx
    /// 
    /// QuickLZ压缩率约在61%上下，但吞吐量很有优势。
    /// 压缩率：
    /// QuickLZ 1.10 （61%) > LZF 1.6 (60.7) > LZO 1X 2.02 (60.2) > LZP-1 (59.9) > ZLIB-1 1.2.3 (46.4%)
    /// 压缩速度：
    /// QuickLZ 1.10 （148MB/sec) > LZO 1X 2.02 (81.8) > LZF 1.6 (60.9) > LZP-1 (60.4) > ZLIB-1 1.2.3 (7.45MB/sec)
    /// 解压速度：
    /// QuickLZ 1.10 （380MB/sec) > LZO 1X 2.02 (307) > LZF 1.6 (198) > ZLIB-1 1.2.3 (120) > LZP-1 (89.3)
    /// 
    /// </summary>
    public static class QuickLZ
    {
        private enum HeaderFields
        {
            QCLZ = 0,
            VERSION = 1,
            COMPSIZE = 2,
            UNCOMPSIZE = 3,
            COMPRESSIBLE = 4,
            RESERVED1 = 5,
            RESERVED2 = 6,
            RESERVED3 = 7
        }

        private static unsafe uint FastRead(void* src, uint bytes)
        {
            uint val = 0;
            if (BitConverter.IsLittleEndian)
                val = *((uint*) src);
            else
            {
                var p = (byte*) src;
                switch (bytes)
                {
                    case 4:
                        val = (uint) (*p) | (uint) (*(p + 1)) << 8 | (uint) (*(p + 2)) << 16 |
                              (uint) (*(p + 3)) << 24;
                        break;
                    case 3:
                        val = (uint) (*p) | (uint) (*(p + 1)) << 8 | (uint) (*(p + 2)) << 16;
                        break;
                    case 2:
                        val = (uint) (*p) | (uint) (*(p + 1)) << 8;
                        break;
                    case 1:
                        val = (uint) (*p);
                        break;
                    default: break;
                }
            }

            return val;
        }

        private static unsafe void FastWrite(uint f, void* dst, uint bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                switch (bytes)
                {
                    case 4:
                    case 3:
                    case 2:
                        *((UInt32*) dst) = f;
                        break;
                    case 1:
                        *((Byte*) dst) = (Byte) f;
                        break;
                    default: break;
                }
            }
            else
            {
                Byte* p = (Byte*) dst;
                switch (bytes)
                {
                    case 4:
                        p[0] = (Byte) f;
                        p[1] = (Byte) (f >> 8);
                        p[2] = (Byte) (f >> 16);
                        p[3] = (Byte) (f >> 24);
                        break;
                    case 3:
                        p[0] = (Byte) f;
                        p[1] = (Byte) (f >> 8);
                        p[2] = (Byte) (f >> 16);
                        break;
                    case 2:
                        p[0] = (Byte) f;
                        p[1] = (Byte) (f >> 8);
                        break;
                    case 1:
                        p[0] = (Byte) f;
                        break;
                    default: break;
                }
            }
        }

        private static unsafe Boolean MemCompare(void* m1, void* m2, UInt32 size)
        {
            Boolean match = true;
            for (UInt32 i = 0; match && i < size; ++i) match = ((Byte*) m1)[i] == ((Byte*) m2)[i];
            return match;
        }

        private static unsafe Boolean MemCompare(void* m1, String str)
        {
            Byte[] source = Encoding.ASCII.GetBytes(str);
            fixed (Byte* m2 = source) return MemCompare(m1, m2, (UInt32) source.Length);
        }

        private static unsafe void MemSet(void* dst, Byte val, UInt32 n)
        {
            for (UInt32 i = 0; i < n; ++i) ((Byte*) dst)[i] = val;
        }

        private static unsafe void MemCopy(void* dst, void* src, UInt32 n)
        {
            for (UInt32 i = 0; i < n; ++i) *((Byte*) dst + i) = *((Byte*) src + i);
        }

        private static unsafe void MemCopy(void* dst, String str)
        {
            Byte[] source = Encoding.ASCII.GetBytes(str);
            fixed (Byte* src = source) MemCopy(dst, src, (UInt32) source.Length);
        }

        private static unsafe void MemCopyUP(Byte* dst, Byte* src, UInt32 n)
        {
            if (BitConverter.IsLittleEndian)
            {
                if (n < 5)
                    *((UInt32*) dst) = *((UInt32*) src);
                else
                {
                    Byte* end = dst + n;
                    while (dst < end)
                    {
                        *((UInt32*) dst) = *((UInt32*) src);
                        dst += 4;
                        src += 4;
                    }
                }
            }
            else
            {
                if (n > 8 && src + n < dst)
                    MemCopy(dst, src, n);
                else
                {
                    Byte* end = dst + n;
                    while (dst < end)
                    {
                        *dst = *src;
                        ++dst;
                        ++src;
                    }
                }
            }
        }

        private static unsafe UInt32 GetHeaderField(Byte[] source, UInt32 start, HeaderFields field)
        {
            UInt32 val = 0;
            fixed (Byte* src = source)
                if (MemCompare(src + start, "QCLZ"))
                    val = ((UInt32*) (src + start))[(Int32) field];
            return val;
        }

        private static UInt32 GetCompressedSize(Byte[] source)
        {
            return GetHeaderField(source, 0, HeaderFields.COMPSIZE);
        }

        public static UInt32 GetDecompressedSize(Byte[] source, UInt32 start)
        {
            return GetHeaderField(source, start, HeaderFields.UNCOMPSIZE);
        }

        private static unsafe UInt32 UnsafeCompress(Byte[] source, UInt32 start, Byte[] destination, UInt32 size)
        {
            fixed (Byte* source_f = source, destination_c = destination)
            {
                Byte* source_c = source_f + start;
                UInt32 headerlen = 8 * 4;
                Byte* last_byte = source_c + size - 1;
                Byte* src = source_c + 1;
                Byte** hashtable = (Byte**) (destination_c + size + 36000 - sizeof(Byte*) * 4096 -
                                             (((Int64) (destination_c + size)) % 8));
                const UInt32 SEQLEN = 2 + (1 << 11);
                Byte* cword_ptr = destination_c + headerlen;
                Byte* dst = destination_c + headerlen + 4 + 1;
                Byte* prev_dst = dst;
                Byte* prev_src = src;
                UInt32 cword_val = 1 << 30;
                UInt32 hash;
                UInt32* header = (UInt32*) destination_c;
                Byte* guarentee_uncompressed = last_byte - 4 * 4;

                MemCopy(&header[(Int32) HeaderFields.QCLZ], "QCLZ");
                destination_c[0] = (Byte) 'Q';
                destination_c[1] = (Byte) 'C';
                destination_c[2] = (Byte) 'L';
                destination_c[3] = (Byte) 'Z';
                FastWrite(3, &header[(Int32) HeaderFields.VERSION], 4);
                FastWrite(size, &header[(Int32) HeaderFields.UNCOMPSIZE], 4);

                for (hash = 0; hash < 4096; ++hash) hashtable[hash] = source_c;

                *(destination_c + headerlen + 4) = *source_c;

                while (src < guarentee_uncompressed - SEQLEN)
                {
                    UInt32 fetch;
                    if ((cword_val & 1) == 1)
                    {
                        if (dst + SEQLEN + 128 > destination_c + size + SEQLEN + 256)
                        {
                            MemCopy(destination_c + headerlen, source_c, size);
                            FastWrite(0, &header[(Int32) HeaderFields.COMPRESSIBLE], 4);
                            FastWrite(size + headerlen + 4, &header[(Int32) HeaderFields.COMPSIZE], 4);
                            MemCopy(destination_c + FastRead(&header[(Int32) HeaderFields.COMPSIZE], 4) - 4, "QCLZ");
                            return FastRead(&header[(Int32) HeaderFields.COMPSIZE], 4);
                        }

                        FastWrite((UInt32) ((cword_val >> 1) | (1 << (4 * 8 - 1))), cword_ptr, 4);
                        cword_ptr = dst;
                        dst += 4;
                        cword_val = 0x80000000;

                        if (dst - prev_dst > src - prev_src && src + 2 * SEQLEN < guarentee_uncompressed)
                        {
                            while (src < prev_src + SEQLEN - 4 * 8)
                            {
                                FastWrite(0x80000000, dst - 4, 4);
                                MemCopyUP(dst, src, 4 * 8 - 1);
                                dst += 4 * 8 - 1 + 4;
                                src += 4 * 8 - 1;
                            }

                            prev_src = src;
                            prev_dst = dst;
                            cword_ptr = dst - 4;
                        }
                    }

                    if (FastRead(src, 4) == FastRead(src + 1, 4))
                    {
                        Byte* orig_src;
                        fetch = FastRead(src, 4);
                        orig_src = src;
                        src += 4 + 1;
                        while (fetch == FastRead(src, 4) && src < orig_src + SEQLEN - 4) src += 4;
                        FastWrite(((fetch & 0xFF) << 16) | (UInt32) ((src - orig_src) << 4) | 15, dst, 4);
                        dst += 3;
                        cword_val = (cword_val >> 1) | 0x80000000;
                    }
                    else
                    {
                        Byte* o;
                        fetch = FastRead(src, 4);
                        hash = ((fetch >> 12) ^ fetch) & 0x0FFF;
                        o = hashtable[hash];
                        hashtable[hash] = src;

                        Boolean tmp;
                        if (BitConverter.IsLittleEndian)
                            tmp = src - o <= 131071 && src - o > 3 &&
                                  (((*(UInt32*) o) ^ (*(UInt32*) src)) & 0xFFFFFF) == 0;
                        else
                            tmp = src - o <= 131071 && src - o > 3 && *src == *o && *(src + 1) == *(o + 1) &&
                                  *(src + 2) == *(o + 2);
                        if (tmp)
                        {
                            UInt32 offset = (UInt32) (src - o);
                            UInt32 matchlen;

                            if (BitConverter.IsLittleEndian) tmp = (*(UInt32*) o) != (*(UInt32*) src);
                            else tmp = *(o + 3) != *(src + 3);
                            if (tmp)
                            {
                                if (offset <= 63)
                                {
                                    *dst = (Byte) (offset << 2);
                                    ++dst;
                                    cword_val = (cword_val >> 1) | 0x80000000;
                                    src += 3;
                                }
                                else if (offset <= 16383)
                                {
                                    UInt32 f = (offset << 2) | 1;
                                    FastWrite(f, dst, 2);
                                    dst += 2;
                                    cword_val = (cword_val >> 1) | 0x80000000;
                                    src += 3;
                                }
                                else
                                {
                                    *dst = *src;
                                    ++dst;
                                    ++src;
                                    cword_val = (cword_val >> 1);
                                }
                            }
                            else
                            {
                                cword_val = (cword_val >> 1) | 0x80000000;
                                matchlen = 3;
                                while (*(o + matchlen) == *(src + matchlen) && matchlen < SEQLEN) ++matchlen;

                                src += matchlen;
                                if (matchlen <= 18 && offset <= 1023)
                                {
                                    UInt32 f = ((matchlen - 3) << 2) | (offset << 6) | 2;
                                    FastWrite(f, dst, 2);
                                    dst += 2;
                                }
                                else if (matchlen <= 34 && offset <= 65535)
                                {
                                    UInt32 f = ((matchlen - 3) << 3) | (offset << 8) | 3;
                                    FastWrite(f, dst, 3);
                                    dst += 3;
                                }
                                else if (matchlen >= 3)
                                {
                                    UInt32 f = ((matchlen - 3) << 4) | (offset << 15) | 7;
                                    FastWrite(f, dst, 4);
                                    dst += 4;
                                }
                            }
                        }
                        else
                        {
                            *dst = *src;
                            ++dst;
                            ++src;
                            cword_val = (cword_val >> 1);
                        }
                    }
                }

                while (src <= last_byte)
                {
                    if ((cword_val & 1) == 1)
                    {
                        FastWrite((cword_val >> 1) | 0x80000000, cword_ptr, 4);
                        cword_ptr = dst;
                        dst += 4;
                        cword_val = 0x80000000;
                    }

                    *dst = *src;
                    ++dst;
                    ++src;
                    cword_val = (cword_val >> 1);
                }

                while ((cword_val & 1) != 1) cword_val = (cword_val >> 1);

                FastWrite((cword_val >> 1) | 0x80000000, cword_ptr, 4);
                dst += 4;


                FastWrite(1, &header[(Int32) HeaderFields.COMPRESSIBLE], 4);
                FastWrite((UInt32) (dst - destination_c - 1 + 4), &header[(Int32) HeaderFields.COMPSIZE], 4);
                MemCopy(destination_c + FastRead(&header[(Int32) HeaderFields.COMPSIZE], 4) - 4, "QCLZ");
            }

            return GetCompressedSize(destination);
        }

        private static unsafe UInt32 UnsafeDecompress(Byte[] source, UInt32 start, Byte[] destination)
        {
            fixed (Byte* source_f = source, destination_c = destination)
            {
                Byte* source_c = source_f + start;
                UInt32 headerlen = 8 * 4;
                Byte* src = source_c + headerlen;
                Byte* dst = destination_c;
                UInt32* header = (UInt32*) source_c;
                Byte* last_byte = destination_c + FastRead(&header[(Int32) HeaderFields.UNCOMPSIZE], 4);
                UInt32 cword_val = 1;
                UInt32[] bitlut = new UInt32[16] {4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0};
                Byte* guaranteed_uncompressed = last_byte - 4;

                if (!MemCompare(&header[(Int32) HeaderFields.QCLZ], "QCLZ"))
                    return 0;
                if (FastRead(&header[(Int32) HeaderFields.VERSION], 4) != 3)
                    return 0;
                if (!MemCompare(source_c + FastRead(&header[(Int32) HeaderFields.COMPSIZE], 4) - 4, "QCLZ"))
                    return 0;

                if (FastRead(&header[(Int32) HeaderFields.COMPRESSIBLE], 4) != 1)
                {
                    MemCopy(destination_c, source_c + headerlen, FastRead(&header[(Int32) HeaderFields.UNCOMPSIZE], 4));
                    return FastRead(&header[(Int32) HeaderFields.UNCOMPSIZE], 4);
                }

                if (dst >= guaranteed_uncompressed)
                {
                    src += 4;
                    while (dst < last_byte)
                    {
                        *dst = *src;
                        ++dst;
                        ++src;
                    }

                    return (UInt32) (dst - destination_c);
                }

                while (true)
                {
                    UInt32 fetch;
                    if (cword_val == 1)
                    {
                        cword_val = FastRead(src, 4);
                        src += 4;
                    }

                    fetch = FastRead(src, 4);

                    if ((cword_val & 1) == 1)
                    {
                        UInt32 offset;
                        UInt32 matchlen;

                        cword_val = (cword_val >> 1);
                        if ((fetch & 3) == 0)
                        {
                            offset = (fetch & 0xFF) >> 2;
                            MemCopyUP(dst, dst - offset, 3);
                            dst += 3;
                            ++src;
                        }
                        else if ((fetch & 2) == 0)
                        {
                            offset = (fetch & 0xFFFF) >> 2;
                            MemCopyUP(dst, dst - offset, 3);
                            dst += 3;
                            src += 2;
                        }
                        else if ((fetch & 1) == 0)
                        {
                            offset = (fetch & 0xFFFF) >> 6;
                            matchlen = ((fetch >> 2) & 15) + 3;
                            MemCopyUP(dst, dst - offset, matchlen);
                            dst += matchlen;
                            src += 2;
                        }
                        else if ((fetch & 4) == 0)
                        {
                            offset = (fetch & 0xFFFFFF) >> 8;
                            matchlen = ((fetch >> 3) & (4 * 8 - 1)) + 3;
                            MemCopyUP(dst, dst - offset, matchlen);
                            dst += matchlen;
                            src += 3;
                        }
                        else if ((fetch & 8) == 0)
                        {
                            offset = (fetch >> 15);
                            matchlen = ((fetch >> 4) & 2047) + 3;
                            MemCopyUP(dst, dst - offset, matchlen);
                            dst += matchlen;
                            src += 4;
                        }
                        else
                        {
                            Byte rle_byte;
                            rle_byte = (Byte) (fetch >> 16);
                            matchlen = ((fetch >> 4) & 0x0FFF);
                            MemSet(dst, rle_byte, matchlen);
                            dst += matchlen;
                            src += 3;
                        }
                    }
                    else
                    {
                        MemCopyUP(dst, src, 4);
                        dst += bitlut[cword_val & 0x0F];
                        src += bitlut[cword_val & 0x0F];
                        cword_val = cword_val >> (Byte) (bitlut[cword_val & 0xf]);

                        if (dst >= guaranteed_uncompressed)
                        {
                            while (dst < last_byte)
                            {
                                if (cword_val == 1)
                                {
                                    src += 4;
                                    cword_val = 0x80000000;
                                }

                                *dst = *src;
                                ++dst;
                                ++src;
                                cword_val = cword_val >> 1;
                            }

                            return (UInt32) (dst - destination_c);
                        }
                    }
                }
            }
        }

        public static Byte[] Compress(Byte[] source, UInt32 start, UInt32 size)
        {
            Byte[] destination = new Byte[size + 36000];
            UInt32 used = UnsafeCompress(source, start, destination, size);
            Byte[] compressed = new Byte[used];
            for (UInt32 i = 0; i < used; ++i) compressed[i] = destination[i];
            return compressed;
        }

        public static Byte[] Decompress(Byte[] source, UInt32 start)
        {
            UInt32 size = GetDecompressedSize(source, start);
            Byte[] destination = null;
            if (size != 0)
            {
                destination = new Byte[size];
                UnsafeDecompress(source, start, destination);
            }

            return destination;
        }
    }
}