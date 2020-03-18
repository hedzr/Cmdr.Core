#region Format of the Data Block

/*
-------------------------------------------------------------------

  Encoded data block consists of sequences of symbols drawn from
  three conceptually distinct alphabets: either literal bytes,
  from the alphabet of byte values (0..255), or <length, backward
  distance> pairs, where the length is drawn from (3..17010) and
  the distance is drawn from (1..524,288). In fact, the literal
  and length alphabets are merged into a single alphabet (0..319),
  where values 0..255 represent literal bytes, and values
  256..319 represent length codes (possibly in conjunction with
  extra bits following the symbol code) as follows:

               Extra                  Extra
          Code  Bits  Length(s)  Code  Bits  Length(s)
          ----  ----  -------    ----  ----  -------
           256    0      3        288    2    51-54
           257    0      4        289    2    55-58
           258    0      5        290    2    59-62
           259    0      6        291    2    63-66
           260    0      7        292    2    67-70
           261    0      8        293    2    71-74
           262    0      9        294    2    75-78
           263    0     10        295    2    79-82
           264    0     11        296    2    83-86
           265    0     12        297    2    87-90
           266    0     13        298    2    91-94
           267    0     14        299    2    95-98
           268    0     15        300    2    99-102
           269    0     16        301    2   103-106
           270    0     17        302    2   107-110
           271    0     18        303    2   111-114
           272    1    19,20      304    3   115-122
           273    1    21,22      305    3   123-130
           274    1    23,24      306    3   131-138
           275    1    25,26      307    3   139-146
           276    1    27,28      308    3   147-154
           277    1    29,30      309    3   155-162
           278    1    31,32      310    3   163-170
           279    1    33,34      311    3   171-178
           280    1    35,36      312    4   179-194
           281    1    37,38      313    4   195-210
           282    1    39,40      314    4   211-226
           283    1    41,42      315    4   227-242
           284    1    43,44      316    6   243-306
           285    1    45,46      317    6   307-370
           286    1    47,48      318    8   371-626
           287    1    49,50      319   14   627-17010
	 
  Distance codes 0-63 are represented by codes of variable length
  with possible additional bits.

              Extra                    Extra
          Code Bits   Distance    Code  Bits    Distance
          ---- ----   --------    ----  ----    ---------
            0    0       R0        32     7      513-640
            1    0       R1        33     7      641-768
            2    0       R2        34     7      769-896
            3    0        1        35     7      897-1024
            4    0        2        36     8     1025-1280
            5    1       3,4       37     8     1281-1536
            6    1       5,6       38     8     1537-1792
            7    1       7,8       39     8     1793-2048
            8    1       9,10      40     9     2049-2560
            9    1      11,12      41     9     2561-3072
           10    1      13,14      42     9     3073-3584
           11    1      15,16      43     9     3585-4096
           12    2      17-20      44    10     4097-5120
           13    2      21-24      45    10     5121-6144
           14    2      25-28      46    10     6145-7168
           15    2      29-32      47    10     7169-8192
           16    3      33-40      48    11     8193-10240
           17    3      41-48      49    11    10241-12288
           18    3      49-56      50    11    12289-14336
           19    3      57-64      51    11    14337-16384
           20    4      65-80      52    12    16385-20480
           21    4      81-96      53    12    20481-24576
           22    4      97-112     54    12    24577-28672
           23    4     113-128     55    12    28673-32768
           24    5     129-160     56    14    32769-49152
           25    5     161-192     57    14    49153-65536
           26    5     193-224     58    15    65537-98304
           27    5     225-256     59    15    98305-131072
           28    6     257-320     60    16   131073-196608
           29    6     321-384     61    16   196609-262144
           30    6     385-448     62    17   262145-393216
           31    6     449-512     63    17   393217-524288

  The alphabet for code lengths is as follows:

        0 - 14: Represent code lengths of 0 - 14 bits.
          15:   Copy previous code length twice.
          16:   Copy previous code length 3 or 4 times
                (1 bits of length: 0 = 3, 1 = 4).
          17:   Copy previous code length 5 - 8 times
                (2 bits of length: 0 = 5, ..., 3 = 8).
          18:   Copy previous code length 9 - 16 times
                (3 bits of length 0 = 9, ..., 7 = 16).
          19:   Copy previous code length 17 - 144 times
                (7 bits of length).

  We can now define the format of the block:

       1 Bit: X, 1 means using of dynamic Huffman codes
                 0 - non-compressed block of data

       8 Bits (if X = 0): # of Bytes in this block:
              # - 8192, 0 if this block is the last

       if X = 1

       20 x 3 bits: code lengths for the code length alphabet

          These code lengths are interpreted as 3-bit integers
          (0-7); as above, a code length of 0 means the
          corresponding symbol (literal/length or distance code
          length) is not used.

       6 Bits: HLIT, # of Literal/Length codes - 257 (257 - 320)

       HLIT + 256 code lengths for the literal/length alphabet,
          encoded using the code length Huffman code

       6 Bits: HDIST, # of Distance codes - 1 (1 - 64)

       HDIST + 1 code lengths for the distance alphabet,
          encoded using the code length Huffman code

       1..8192..8447 literal/length and distance Huffman codes

-------------------------------------------------------------------
*/

#endregion

using System;

// http://www.codeproject.com/KB/cs/IMCompressor.aspx
// Better Than Zip Algorithm For Compressing In-Memory Data
//      By Andrey Dryazgov | 24 May 2006
namespace HzNS.MdxLib.Compression.Aced
{
    // AcedCompressionLevel enumeration

    public enum AcedCompressionLevel
    {
        Store,
        Fastest,
        Fast,
        Normal,
        Maximum
    }

    // AcedConsts internal class

    internal sealed class AcedConsts
    {
        internal const int
            ChunkShift = 17,
            ChunkCapacity = 32768,
            BlockSize = 8192,
            MaxLength = 17010,
            MaxDistance = 524288,
            InitPosValue = -(MaxDistance + 1),
            CharCount = 320,
            FirstLengthChar = 256,
            FirstCharWithExBit = 272,
            CharTreeSize = CharCount * 2,
            DistCount = 64,
            FirstDistWithExBit = 5,
            DistTreeSize = DistCount * 2,
            MaxBits = 14,
            ChLenCount = 20,
            ChLenTreeSize = ChLenCount * 2,
            MaxChLenBits = 7;

        internal static readonly int[] CharExBitLength = new int[]
        {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 6, 6, 8, 14
        };

        internal static readonly int[] CharExBitBase = new int[]
        {
            19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39, 41, 43, 45, 47, 49, 51, 55, 59, 63,
            67, 71, 75, 79, 83, 87, 91, 95, 99, 103, 107, 111, 115, 123, 131, 139, 147, 155,
            163, 171, 179, 195, 211, 227, 243, 307, 371, 627
        };

        internal static readonly int[] DistExBitLength = new int[]
        {
            0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5,
            6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 12,
            12, 12, 12, 14, 14, 15, 15, 16, 16, 17, 17
        };

        internal static readonly int[] DistExBitBase = new int[]
        {
            0, 0, 0, 1, 2, 3, 5, 7, 9, 11, 13, 15, 17, 21, 25, 29, 33, 41, 49, 57, 65, 81, 97, 113,
            129, 161, 193, 225, 257, 321, 385, 449, 513, 641, 769, 897, 1025, 1281, 1537, 1793,
            2049, 2561, 3073, 3585, 4097, 5121, 6145, 7169, 8193, 10241, 12289, 14337, 16385, 20481,
            24577, 28673, 32769, 49153, 65537, 98305, 131073, 196609, 262145, 393217
        };

        internal static readonly int[] ChLenExBitLength = new int[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 7
        };

        private AcedConsts()
        {
        }
    }

    // AcedMCException exception

    public class AcedMCException : Exception
    {
        internal static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        internal static void ThrowNoPlaceToStoreCompressedDataException()
        {
            throw new AcedMCException("Destination byte array is not enough to store compressed data.");
        }

        internal static void ThrowNoPlaceToStoreDecompressedDataException()
        {
            throw new AcedMCException("Destination byte array is not enough to store decompressed data.");
        }

        internal static void ThrowReadBeyondTheEndException()
        {
            throw new AcedMCException("An attempt to read beyond the end of the source byte array.");
        }

        private AcedMCException(string message)
            : base(message)
        {
        }
    }
}