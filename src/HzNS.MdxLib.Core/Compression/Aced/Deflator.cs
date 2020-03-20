#region AcedDeflator Class Reference

/*
--------------------------------------------------------------------------------
public class AcedDeflator

  This class provides methods for compressing binary data with the LZ+Huffman
  method similar to the one described in RFC 1951. You should synchronize calls
  to the methods of this class in a multithreaded application.

--------------------------------------------------------------------------------
ctor
--------------------------------------------------------------------------------

public AcedDeflator()

  Creates a new instance of the AcedDeflator class. It is not recommended to
  call this constructor directly. When possible, please use a single cached
  instance of the AcedDeflator class which is returned by the Instance static
  property.

--------------------------------------------------------------------------------
Property
--------------------------------------------------------------------------------

public static AcedDeflator Instance { get; }

  Returns a cached instance of the AcedDeflator class. Such an instance
  occupies a lot of memory (a little more than 2 MB). So, a single instance
  is created and cached in an internal static field of this class. This
  instance can be used each time when you want to compress some data. The only
  exception may be a multithreaded application where you compress data in
  several threads simultaneously. If so, you should create each instance with
  the regular constructor.

--------------------------------------------------------------------------------
Methods
--------------------------------------------------------------------------------

public byte[] Compress(byte[] sourceBytes, int sourceIndex, int sourceLength,
  AcedCompressionLevel compressionLevel, int beforeGap, int afterGap)

  Compresses binary data passed in the "sourceBytes" array starting from
  "sourceIndex" with "sourceLength" bytes in length. The compression ratio
  depends on the "compressionLevel" parameter. This method creates a new byte
  array which is enough to store the compressed data. You may also reserve
  "beforeGap" bytes before the compressed data block and "afterGap" bytes
  after the compressed data block. This may be useful if you want to supply
  a custom header and/or tail to the compressed data and you don't want to
  reallocate memory unnecessarily. The result byte array consists of
  (beforeGap + Compressed_Data_Length + afterGap) bytes.

public int Compress(byte[] sourceBytes, int sourceIndex, int sourceLength,
  AcedCompressionLevel compressionLevel, byte[] destinationBytes,
  int destinationIndex)

  This method is similar to the previous one but it doesn't allocate the
  memory for the output array. The output array is passed as the
  "destinationBytes" argument. The compressed data will be stored in
  that array starting from the "destinationIndex". The maximum length
  of the compressed data is (sourceLength + 4) bytes. The output array
  must have enough space. This method returns the number of bytes stored
  in the output array. If you pass null in the "destinationBytes" argument
  the method performs "dummy" compression. In such a way you can find out
  the length of the output data. However, the "dummy" compression takes
  almost the same time as the regular compression.
  
public static void Release()

  Call this method to release an internal static reference to the cached
  instance of the AcedDeflator class. After that, you may call GC.Collect()
  to free memory which was occupied by that cached instance.

--------------------------------------------------------------------------------
*/

#endregion

using System;

namespace HzNS.MdxLib.Compression.Aced
{
    // AcedDeflator class (not thread-safe)

    public class AcedDeflator
    {
        private static AcedDeflator _instance;

        private const int
            len3MaxDist = 4096,
            len4MaxDist = 32768,
            diff1Min = 1025,
            diff2Min = 32769,
            lazyLimit = 32,
            hashSizeFastest = 0x1000,
            hashMaskFastest = 0x0FFF,
            prevSizeFastest = 0x2000,
            prevMaskFastest = 0x1FFF,
            chainFastest = 2,
            shiftFastest = 4,
            hashSizeFast = 0x8000,
            hashMaskFast = 0x7FFF,
            prevSizeFast = 0x10000,
            prevMaskFast = 0xFFFF,
            chainFast = 8,
            shiftFast = 5,
            hashSizeNormal = 0x40000,
            hashMaskNormal = 0x3FFFF,
            prevSizeNormal = 0x40000,
            prevMaskNormal = 0x3FFFF,
            chainNormal = 32,
            shiftNormal = 6,
            hashSizeMaximum = 0x40000,
            hashMaskMaximum = 0x3FFFF,
            prevSizeMaximum = 0x40000,
            prevMaskMaximum = 0x3FFFF,
            chainMaximum = 96,
            shiftMaximum = 6;

        private int _hashHead;
        private int _length;
        private int _distance;
        private int _prevSize;
        private int _prevMask;
        private int _shift;
        private int _hashSize;
        private int _hashMask;
        private int _maxChain;
        private int _prevLimit;
        private int _limit;
        private int _breakOffset;
        private int _chain;
        private int _maxCode;
        private int _heapLen;
        private int _r0;
        private int _r1;
        private int _r2;
        private int _prevR0;
        private int _prevR1;
        private int _prevR2;

        private unsafe int* _pHash;
        private unsafe int* _pPrev;
        private unsafe int* _pHeap;
        private unsafe int* _pWorkFreq;
        private unsafe int* _pWorkDad;
        private unsafe int* _pDepth;
        private unsafe int* _pTree;
        private unsafe int* _pNextCode;

        private int[] _hash;
        private int[] _prev;
        private int[] _heap;
        private int[] _depth;
        private int[] _tree;
        private int[] _nextCode;

        private int _dstIndex;
        private int _dstBreak;
        private int _chunkLength;
        private uint[] _chunk;
        private object[] _chunkList;
        private int _bits;
        private uint _hold;

        private unsafe byte* _pSrcBytes;
        private unsafe byte* _pDstBytes;

        private int _exBitCount;
        private int _blockStart;
        private int _blockEndIndex;
        private int _bufLength;

        private unsafe int* _pBuffer;
        private unsafe int* _pLen;
        private unsafe int* _pDist;
        private unsafe int* _pDistExBits;

        private int[] _buffer;
        private int[] _len;
        private int[] _dist;
        private int[] _distExBits;

        private int _charBitLenLen;
        private int _distBitLenLen;
        private int _charLenCount;
        private int _distLenCount;

        private unsafe int* _pCharBitCount;
        private unsafe int* _pCharFreqCode;
        private unsafe int* _pCharDadLen;
        private unsafe int* _pCharBitLen;
        private unsafe int* _pCharBitLenEx;
        private unsafe int* _pDistBitCount;
        private unsafe int* _pDistFreqCode;
        private unsafe int* _pDistDadLen;
        private unsafe int* _pDistBitLen;
        private unsafe int* _pDistBitLenEx;
        private unsafe int* _pChLenBitCount;
        private unsafe int* _pChLenFreqCode;
        private unsafe int* _pChLenDadLen;

        private int[] _charBitCount;
        private int[] _charFreqCode;
        private int[] _charDadLen;
        private int[] _charBitLen;
        private int[] _charBitLenEx;
        private int[] _distBitCount;
        private int[] _distFreqCode;
        private int[] _distDadLen;
        private int[] _distBitLen;
        private int[] _distBitLenEx;
        private int[] _chLenBitCount;
        private int[] _chLenFreqCode;
        private int[] _chLenDadLen;
        private int[] _chLenBitLen;

        private unsafe int* _pCharExBitLength;
        private unsafe int* _pCharExBitBase;
        private unsafe int* _pDistExBitLength;
        private unsafe int* _pDistExBitBase;
        private unsafe int* _pChLenExBitLength;

        private bool _growMode;
        private bool _tryMode;

        private static byte[] _fakeDestinationBytes = new byte[1];

        public static AcedDeflator Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                _instance = new AcedDeflator();
                return _instance;
            }
        }

        public AcedDeflator()
        {
            _heap = new int[AcedConsts.CharCount + 1];
            _depth = new int[AcedConsts.CharTreeSize];
            _tree = new int[AcedConsts.CharTreeSize];
            _nextCode = new int[AcedConsts.MaxBits + 1];
            _buffer = new int[AcedConsts.BlockSize];
            _len = new int[AcedConsts.BlockSize];
            _dist = new int[AcedConsts.BlockSize];
            _distExBits = new int[AcedConsts.BlockSize];
            _charBitCount = new int[AcedConsts.MaxBits + 1];
            _charFreqCode = new int[AcedConsts.CharTreeSize];
            _charDadLen = new int[AcedConsts.CharTreeSize];
            _charBitLen = new int[AcedConsts.CharCount];
            _charBitLenEx = new int[AcedConsts.CharCount];
            _distBitCount = new int[AcedConsts.MaxBits + 1];
            _distFreqCode = new int[AcedConsts.DistTreeSize];
            _distDadLen = new int[AcedConsts.DistTreeSize];
            _distBitLen = new int[AcedConsts.DistCount];
            _distBitLenEx = new int[AcedConsts.DistCount];
            _chLenBitCount = new int[AcedConsts.MaxChLenBits + 1];
            _chLenFreqCode = new int[AcedConsts.ChLenTreeSize];
            _chLenDadLen = new int[AcedConsts.ChLenTreeSize];
            _chLenBitLen = new int[AcedConsts.ChLenCount];
            _hash = new int[hashSizeMaximum];
            _prev = new int[prevSizeMaximum];
        }

        private void NextChunk()
        {
            if (_chunkList == null)
            {
                _chunkList = new object[16];
                _chunkList[0] = _chunk;
            }
            else
            {
                int index = (_dstIndex >> AcedConsts.ChunkShift) - 1;
                int capacity = _chunkList.Length;
                if (index == capacity)
                {
                    object[] newList = new object[capacity * 2];
                    Array.Copy(_chunkList, 0, newList, 0, capacity);
                    _chunkList = newList;
                }

                _chunkList[index] = _chunk;
            }

            _chunk = new uint[AcedConsts.ChunkCapacity];
            _chunkLength = 0;
        }

        private unsafe bool PutBit(int v)
        {
            if (_bits != 0)
            {
                _hold |= (uint) v << (32 - _bits);
                _bits--;
                return true;
            }

            _dstIndex += 4;
            if (_dstIndex <= _dstBreak)
            {
                if (_growMode)
                {
                    if (_chunkLength == AcedConsts.ChunkCapacity)
                        NextChunk();
                    _chunk[_chunkLength] = _hold;
                    _chunkLength++;
                }
                else if (!_tryMode)
                {
                    *((uint*) _pDstBytes) = _hold;
                    _pDstBytes += 4;
                }

                _hold = (uint) v;
                _bits = 31;
                return true;
            }

            return false;
        }

        private unsafe bool PutNBits(int n, uint v)
        {
            int bits = _bits;
            if (bits >= n)
            {
                _hold |= v << (32 - bits);
                _bits = bits - n;
                return true;
            }

            if (bits != 0)
            {
                _hold |= v << (32 - bits);
                v >>= bits;
            }

            _dstIndex += 4;
            if (_dstIndex <= _dstBreak)
            {
                if (_growMode)
                {
                    if (_chunkLength == AcedConsts.ChunkCapacity)
                        NextChunk();
                    _chunk[_chunkLength] = _hold;
                    _chunkLength++;
                }
                else if (!_tryMode)
                {
                    *((uint*) _pDstBytes) = _hold;
                    _pDstBytes += 4;
                }

                _hold = v;
                _bits = 32 + bits - n;
                return true;
            }

            return false;
        }

        private unsafe void InitHeap(int count)
        {
            int* pH = _pHeap + 1;
            int* pW = _pWorkFreq;
            _heapLen = 0;
            _maxCode = -1;
            int i = 0;
            while (i < count)
            {
                if (*pW != 0)
                {
                    *pH = i;
                    _maxCode = i;
                    _pDepth[i] = 0;
                    _heapLen++;
                    pH++;
                }
                else
                    _pWorkDad[i] = 0;

                i++;
                pW++;
            }

            if (_heapLen > 1)
                return;
            if (_heapLen > 0)
            {
                if (_maxCode == 0)
                    _maxCode = i = 1;
                else
                    i = 0;
                *pH = i;
                _pWorkFreq[i] = 1;
                _pDepth[i] = 0;
            }
            else
            {
                _maxCode = 1;
                pH[0] = 0;
                pH[1] = 1;
                _pWorkFreq[0] = 1;
                _pWorkFreq[1] = 1;
                _pDepth[0] = 0;
                _pDepth[1] = 0;
            }

            _heapLen = 2;
        }

        private unsafe void SortHeap(int L, int R)
        {
            int* pH = _pHeap;
            int* pW = _pWorkFreq;
            int I, J;
            do
            {
                I = L;
                J = R;
                int M = pW[pH[(L + R) >> 1]];
                do
                {
                    while (pW[pH[I]] < M)
                        I++;
                    while (M < pW[pH[J]])
                        J--;
                    if (I <= J)
                    {
                        int T = pH[I];
                        pH[I] = pH[J];
                        pH[J] = T;
                        I++;
                        J--;
                    }
                } while (I <= J);

                if (L < J)
                    SortHeap(L, J);
                L = I;
            } while (I < R);
        }

        private unsafe void PQDownHeap()
        {
            int* pH = _pHeap;
            int v = pH[1];
            int nV = _pWorkFreq[v];
            int k = 1;
            int j = 2;
            while (j <= _heapLen)
            {
                int p0 = pH[j];
                int n0 = _pWorkFreq[p0];
                if (j < _heapLen)
                {
                    int p1 = pH[j + 1];
                    int n1 = _pWorkFreq[p1];
                    if (n1 < n0 || (n1 == n0 && _pDepth[p1] <= _pDepth[p0]))
                    {
                        j++;
                        p0 = p1;
                        n0 = n1;
                    }
                }

                if (nV < n0 || (nV == n0 && _pDepth[v] <= _pDepth[p0]))
                    break;
                pH[k] = pH[j];
                k = j;
                j <<= 1;
            }

            pH[k] = v;
        }

        private unsafe int BuildTree(int nextNode)
        {
            int i = 0;
            int* pHeap1 = _pHeap + 1;
            int* pT = _pTree;
            do
            {
                int n = *pHeap1;
                *pHeap1 = _pHeap[_heapLen];
                _heapLen--;
                PQDownHeap();
                int m = *pHeap1;
                pT[0] = n;
                pT[1] = m;
                i += 2;
                pT += 2;
                _pWorkDad[n] = _pWorkDad[m] = nextNode;
                _pWorkFreq[nextNode] = _pWorkFreq[n] + _pWorkFreq[m];
                n = _pDepth[n];
                m = _pDepth[m];
                _pDepth[nextNode] = (n > m ? n : m) + 1;
                *pHeap1 = nextNode;
                nextNode++;
                PQDownHeap();
            } while (_heapLen > 1);

            _pWorkDad[*pHeap1] = 0;
            return i;
        }

        private unsafe int PrepareCharLengths()
        {
            _pWorkFreq = _pCharFreqCode;
            _pWorkDad = _pCharDadLen;
            InitHeap(AcedConsts.CharCount);
            SortHeap(1, _heapLen);
            int i = BuildTree(AcedConsts.CharCount);
            AcedUtils.Fill(0, _pCharBitCount, AcedConsts.MaxBits + 1);
            int overf = 0;
            int bitAmount = 0;
            int n, m;
            int* pT = _pTree + i;
            do
            {
                pT--;
                n = *pT;
                m = _pCharDadLen[_pCharDadLen[n]] + 1;
                if (m > AcedConsts.MaxBits)
                {
                    m = AcedConsts.MaxBits;
                    overf++;
                }

                _pCharDadLen[n] = m;
                if (n < AcedConsts.CharCount)
                {
                    bitAmount += m * _pCharFreqCode[n];
                    _pCharBitCount[m]++;
                }

                i--;
            } while (i > 0);

            if (overf == 0)
                return bitAmount;
            do
            {
                i = AcedConsts.MaxBits - 1;
                while (_pCharBitCount[i] == 0)
                    i--;
                _pCharBitCount[i]--;
                _pCharBitCount[i + 1] += 2;
                _pCharBitCount[AcedConsts.MaxBits]--;
                overf -= 2;
            } while (overf > 0);

            overf = AcedConsts.MaxBits;
            do
            {
                n = _pCharBitCount[overf];
                while (n != 0)
                {
                    m = *pT;
                    pT++;
                    if (m < AcedConsts.CharCount)
                    {
                        bitAmount += (overf - _pCharDadLen[m]) * _pCharFreqCode[m];
                        _pCharDadLen[m] = overf;
                        n--;
                    }
                }

                overf--;
            } while (overf != 0);

            return bitAmount;
        }

        private unsafe int PrepareDistLengths()
        {
            _pWorkFreq = _pDistFreqCode;
            _pWorkDad = _pDistDadLen;
            InitHeap(AcedConsts.DistCount);
            SortHeap(1, _heapLen);
            int i = BuildTree(AcedConsts.DistCount);
            AcedUtils.Fill(0, _pDistBitCount, AcedConsts.MaxBits + 1);
            int overf = 0;
            int bitAmount = 0;
            int n, m;
            int* pT = _pTree + i;
            do
            {
                pT--;
                n = *pT;
                m = _pDistDadLen[_pDistDadLen[n]] + 1;
                if (m > AcedConsts.MaxBits)
                {
                    m = AcedConsts.MaxBits;
                    overf++;
                }

                _pDistDadLen[n] = m;
                if (n < AcedConsts.DistCount)
                {
                    bitAmount += m * _pDistFreqCode[n];
                    _pDistBitCount[m]++;
                }

                i--;
            } while (i > 0);

            if (overf == 0)
                return bitAmount;
            do
            {
                i = AcedConsts.MaxBits - 1;
                while (_pDistBitCount[i] == 0)
                    i--;
                _pDistBitCount[i]--;
                _pDistBitCount[i + 1] += 2;
                _pDistBitCount[AcedConsts.MaxBits]--;
                overf -= 2;
            } while (overf > 0);

            overf = AcedConsts.MaxBits;
            do
            {
                n = _pDistBitCount[overf];
                while (n != 0)
                {
                    m = *pT;
                    pT++;
                    if (m < AcedConsts.DistCount)
                    {
                        bitAmount += (overf - _pDistDadLen[m]) * _pDistFreqCode[m];
                        _pDistDadLen[m] = overf;
                        n--;
                    }
                }

                overf--;
            } while (overf != 0);

            return bitAmount;
        }

        private unsafe int FillCharBitLengths()
        {
            int* pDad = _pCharDadLen;
            int* pLen = _pCharBitLen;
            _charBitLenLen = 0;
            int lastLen = 0;
            int j, i = 0;
            if (_maxCode > 255)
                _charLenCount = _maxCode + 1;
            else
            {
                _charLenCount = 257;
                _maxCode = 256;
            }

            AcedUtils.Fill(0, _pCharBitLenEx, _charLenCount);
            int breakOffset, result = 0;
            do
            {
                int m = *pDad;
                pDad++;
                if (m != lastLen || m != *pDad)
                {
                    *pLen = lastLen = m;
                    i++;
                }
                else
                {
                    if (i + 144 > _maxCode)
                        breakOffset = _charLenCount;
                    else
                        breakOffset = i + 144;
                    j = i;
                    i += 2;
                    pDad++;
                    while (i < breakOffset && m == *pDad)
                    {
                        pDad++;
                        i++;
                    }

                    j = i - j;
                    if (j == 2)
                        *pLen = 15;
                    else if (j < 5)
                    {
                        *pLen = 16;
                        _pCharBitLenEx[_charBitLenLen] = j - 3;
                        result += 1;
                    }
                    else if (j < 9)
                    {
                        *pLen = 17;
                        _pCharBitLenEx[_charBitLenLen] = j - 5;
                        result += 2;
                    }
                    else if (j < 17)
                    {
                        *pLen = 18;
                        _pCharBitLenEx[_charBitLenLen] = j - 9;
                        result += 3;
                    }
                    else
                    {
                        *pLen = 19;
                        _pCharBitLenEx[_charBitLenLen] = j - 17;
                        result += 7;
                    }
                }

                _charBitLenLen++;
                pLen++;
            } while (i < _maxCode);

            if (i == _maxCode)
            {
                *pLen = *pDad;
                _charBitLenLen++;
            }

            return result;
        }

        private unsafe int FillDistBitLengths()
        {
            int* pDad = _pDistDadLen;
            int* pLen = _pDistBitLen;
            _distBitLenLen = 0;
            int lastLen = 0;
            int j, i = 0;
            _distLenCount = _maxCode + 1;
            AcedUtils.Fill(0, _pDistBitLenEx, _distLenCount);
            int result = 0;
            do
            {
                int m = *pDad;
                pDad++;
                if (m != lastLen || m != *pDad)
                {
                    *pLen = lastLen = m;
                    i++;
                }
                else
                {
                    j = i;
                    i += 2;
                    pDad++;
                    while (i < _distLenCount && m == *pDad)
                    {
                        pDad++;
                        i++;
                    }

                    j = i - j;
                    if (j == 2)
                        *pLen = 15;
                    else if (j < 5)
                    {
                        *pLen = 16;
                        _pDistBitLenEx[_distBitLenLen] = j - 3;
                        result += 1;
                    }
                    else if (j < 9)
                    {
                        *pLen = 17;
                        _pDistBitLenEx[_distBitLenLen] = j - 5;
                        result += 2;
                    }
                    else if (j < 17)
                    {
                        *pLen = 18;
                        _pDistBitLenEx[_distBitLenLen] = j - 9;
                        result += 3;
                    }
                    else
                    {
                        *pLen = 19;
                        _pDistBitLenEx[_distBitLenLen] = j - 17;
                        result += 7;
                    }
                }

                _distBitLenLen++;
                pLen++;
            } while (i < _maxCode);

            if (i == _maxCode)
            {
                *pLen = *pDad;
                _distBitLenLen++;
            }

            return result;
        }

        private unsafe void FillChLenFreqCodes()
        {
            int* pFreq = _pChLenFreqCode;
            AcedUtils.Fill(0, pFreq, AcedConsts.ChLenCount);
            int i;
            int* pLen = _pCharBitLen;
            for (i = 7; i < _charBitLenLen; i += 8)
            {
                pFreq[pLen[0]]++;
                pFreq[pLen[1]]++;
                pFreq[pLen[2]]++;
                pFreq[pLen[3]]++;
                pFreq[pLen[4]]++;
                pFreq[pLen[5]]++;
                pFreq[pLen[6]]++;
                pFreq[pLen[7]]++;
                pLen += 8;
            }

            for (i -= 7; i < _charBitLenLen; i++)
            {
                pFreq[*pLen]++;
                pLen++;
            }

            pLen = _pDistBitLen;
            for (i = 7; i < _distBitLenLen; i += 8)
            {
                pFreq[pLen[0]]++;
                pFreq[pLen[1]]++;
                pFreq[pLen[2]]++;
                pFreq[pLen[3]]++;
                pFreq[pLen[4]]++;
                pFreq[pLen[5]]++;
                pFreq[pLen[6]]++;
                pFreq[pLen[7]]++;
                pLen += 8;
            }

            for (i -= 7; i < _distBitLenLen; i++)
            {
                pFreq[*pLen]++;
                pLen++;
            }
        }

        private unsafe int PrepareChLenLengths()
        {
            _pWorkFreq = _pChLenFreqCode;
            _pWorkDad = _pChLenDadLen;
            InitHeap(AcedConsts.ChLenCount);
            SortHeap(1, _heapLen);
            int i = BuildTree(AcedConsts.ChLenCount);
            AcedUtils.Fill(0, _pChLenBitCount, AcedConsts.MaxChLenBits + 1);
            int overf = 0;
            int bitAmount = 0;
            int n, m;
            int* pT = _pTree + i;
            do
            {
                pT--;
                n = *pT;
                m = _pChLenDadLen[_pChLenDadLen[n]] + 1;
                if (m > AcedConsts.MaxChLenBits)
                {
                    m = AcedConsts.MaxChLenBits;
                    overf++;
                }

                _pChLenDadLen[n] = m;
                if (n < AcedConsts.ChLenCount)
                {
                    bitAmount += m * _pChLenFreqCode[n];
                    _pChLenBitCount[m]++;
                }

                i--;
            } while (i > 0);

            if (overf == 0)
                return bitAmount;
            do
            {
                i = AcedConsts.MaxChLenBits - 1;
                while (_pChLenBitCount[i] == 0)
                    i--;
                _pChLenBitCount[i]--;
                _pChLenBitCount[i + 1] += 2;
                _pChLenBitCount[AcedConsts.MaxChLenBits]--;
                overf -= 2;
            } while (overf > 0);

            overf = AcedConsts.MaxChLenBits;
            do
            {
                n = _pChLenBitCount[overf];
                while (n != 0)
                {
                    m = *pT;
                    pT++;
                    if (m < AcedConsts.ChLenCount)
                    {
                        bitAmount += (overf - _pChLenDadLen[m]) * _pChLenFreqCode[m];
                        _pChLenDadLen[m] = overf;
                        n--;
                    }
                }

                overf--;
            } while (overf != 0);

            return bitAmount;
        }

        private unsafe bool WriteNonCompressedBlock(uint v, int blockStart, int blockEnd)
        {
            if (!PutBit(0))
                return false;
            int bits = _bits;
            while (true)
            {
                if (bits >= 8)
                {
                    _hold |= v << (32 - bits);
                    bits -= 8;
                }
                else
                {
                    if (bits != 0)
                    {
                        _hold |= v << (32 - bits);
                        v >>= bits;
                    }

                    _dstIndex += 4;
                    if (_dstIndex > _dstBreak)
                        return false;
                    if (_growMode)
                    {
                        if (_chunkLength == AcedConsts.ChunkCapacity)
                            NextChunk();
                        _chunk[_chunkLength] = _hold;
                        _chunkLength++;
                    }
                    else if (!_tryMode)
                    {
                        *((uint*) _pDstBytes) = _hold;
                        _pDstBytes += 4;
                    }

                    _hold = v;
                    bits += 24;
                }

                if (blockStart < blockEnd)
                    v = (uint) _pSrcBytes[blockStart];
                else
                    break;
                blockStart++;
            }

            _bits = bits;
            return true;
        }

        private unsafe bool WriteBlockData()
        {
            for (int i = 0; i < _bufLength; i++)
            {
                int c = _pBuffer[i];
                if (!PutNBits(_pCharDadLen[c], (uint) _pCharFreqCode[c]))
                    return false;
                if (c < AcedConsts.FirstLengthChar)
                    continue;
                c -= AcedConsts.FirstCharWithExBit;
                if (c >= 0)
                {
                    if (!PutNBits(_pCharExBitLength[c], (uint) (_pLen[i] - _pCharExBitBase[c])))
                        return false;
                }

                c = _pDist[i];
                int n = _pDistDadLen[c];
                if (c < AcedConsts.FirstDistWithExBit)
                {
                    if (!PutNBits(n, (uint) _pDistFreqCode[c]))
                        return false;
                }
                else if (!PutNBits(n + _pDistExBitLength[c], (uint) (_pDistFreqCode[c] | (_pDistExBits[i] << n))))
                    return false;
            }

            return true;
        }

        private unsafe void GenerateCodes()
        {
            int* p = _pNextCode;
            p[1] = 0;
            int n = _pChLenBitCount[1] << 1;
            p[2] = n;
            p += 3;
            int* pBD = _pChLenBitCount + 2;
            int i, m;
            for (i = AcedConsts.MaxChLenBits - 2; i > 0; i--)
            {
                *p = n = (n + *pBD) << 1;
                pBD++;
                p++;
            }

            p = _pNextCode;
            int* pFC = _pChLenFreqCode;
            pBD = _pChLenDadLen;
            for (i = AcedConsts.ChLenCount; i > 0; i--)
            {
                n = *pBD;
                if (n != 0)
                {
                    m = p[n];
                    *pFC = (int) AcedUtils.ReverseBits((uint) m, n);
                    p[n] = m + 1;
                }

                pBD++;
                pFC++;
            }

            p = _pNextCode;
            p[1] = 0;
            n = _pCharBitCount[1] << 1;
            p[2] = n;
            p += 3;
            pBD = _pCharBitCount + 2;
            for (i = AcedConsts.MaxBits - 2; i > 0; i--)
            {
                *p = n = (n + *pBD) << 1;
                pBD++;
                p++;
            }

            p = _pNextCode;
            pFC = _pCharFreqCode;
            pBD = _pCharDadLen;
            for (i = AcedConsts.CharCount; i > 0; i--)
            {
                n = *pBD;
                if (n != 0)
                {
                    m = p[n];
                    *pFC = (int) AcedUtils.ReverseBits((uint) m, n);
                    p[n] = m + 1;
                }

                pBD++;
                pFC++;
            }

            p = _pNextCode;
            p[1] = 0;
            n = _pDistBitCount[1] << 1;
            p[2] = n;
            p += 3;
            pBD = _pDistBitCount + 2;
            for (i = AcedConsts.MaxBits - 2; i > 0; i--)
            {
                *p = n = (n + *pBD) << 1;
                pBD++;
                p++;
            }

            p = _pNextCode;
            pFC = _pDistFreqCode;
            pBD = _pDistDadLen;
            for (i = AcedConsts.DistCount; i > 0; i--)
            {
                n = *pBD;
                if (n != 0)
                {
                    m = p[n];
                    *pFC = (int) AcedUtils.ReverseBits((uint) m, n);
                    p[n] = m + 1;
                }

                pBD++;
                pFC++;
            }
        }

        private unsafe bool WriteDynamicBlock()
        {
            GenerateCodes();
            if (!PutBit(1))
                return false;
            int i, m, n;
            for (i = 0; i < AcedConsts.ChLenCount; i++)
                if (!PutNBits(3, (uint) _pChLenDadLen[i]))
                    return false;
            if (!PutNBits(6, (uint) (_charLenCount - 257)))
                return false;
            for (i = 0; i < _charBitLenLen; i++)
            {
                m = _pCharBitLen[i];
                n = _pChLenDadLen[m];
                if (!PutNBits(n + _pChLenExBitLength[m], (uint) (_pChLenFreqCode[m] | (_pCharBitLenEx[i] << n))))
                    return false;
            }

            if (!PutNBits(6, (uint) (_distLenCount - 1)))
                return false;
            for (i = 0; i < _distBitLenLen; i++)
            {
                m = _pDistBitLen[i];
                n = _pChLenDadLen[m];
                if (!PutNBits(n + _pChLenExBitLength[m], (uint) (_pChLenFreqCode[m] | (_pDistBitLenEx[i] << n))))
                    return false;
            }

            return WriteBlockData();
        }

        private unsafe bool FlushBuffer()
        {
            int bitAmount = 16;
            bitAmount += PrepareCharLengths();
            bitAmount += FillCharBitLengths();
            bitAmount += PrepareDistLengths();
            bitAmount += FillDistBitLengths();
            FillChLenFreqCodes();
            bitAmount += PrepareChLenLengths() + AcedConsts.ChLenCount * 3;
            bitAmount += _exBitCount;
            int n = _blockEndIndex - _blockStart;
            if (n <= AcedConsts.BlockSize + 255 && (n << 3) + 10 <= bitAmount)
            {
                if (!WriteNonCompressedBlock(n < AcedConsts.BlockSize ? 0u : (uint) (n - AcedConsts.BlockSize),
                    _blockStart, _blockEndIndex))
                    return false;
                _r0 = _prevR0;
                _r1 = _prevR1;
                _r2 = _prevR2;
            }
            else
            {
                if (!WriteDynamicBlock())
                    return false;
                _prevR0 = _r0;
                _prevR1 = _r1;
                _prevR2 = _r2;
            }

            AcedUtils.Fill(0, _pCharFreqCode, AcedConsts.CharCount);
            AcedUtils.Fill(0, _pDistFreqCode, AcedConsts.DistCount);
            _exBitCount = 0;
            _blockStart = _blockEndIndex;
            _bufLength = 0;
            return true;
        }

        private int GetLenChar(int length)
        {
            if (length < 19)
                return length + 253;
            if (length < 115)
            {
                if (length < 51)
                {
                    _exBitCount += 1;
                    return ((length - 19) >> 1) + 272;
                }

                _exBitCount += 2;
                return ((length - 51) >> 2) + 288;
            }

            if (length < 243)
            {
                if (length < 179)
                {
                    _exBitCount += 3;
                    return ((length - 115) >> 3) + 304;
                }

                _exBitCount += 4;
                return ((length - 179) >> 4) + 312;
            }

            if (length < 371)
            {
                _exBitCount += 6;
                return ((length - 243) >> 6) + 316;
            }

            if (length < 627)
            {
                _exBitCount += 8;
                return 318;
            }

            _exBitCount += 14;
            return 319;
        }

        private int SplitDistance(int distance)
        {
            if (distance < 3)
                return distance + 2;
            if (distance == _r0)
                return 0;
            if (distance == _r1)
            {
                _r1 = _r0;
                _r0 = distance;
                return 1;
            }

            if (distance == _r2)
            {
                _r2 = _r0;
                _r0 = distance;
                return 2;
            }

            _r2 = _r1;
            _r1 = _r0;
            _r0 = distance;
            if (distance < 1025)
            {
                if (distance < 65)
                {
                    if (distance < 17)
                        return ((distance - 3) >> 1) + 5;
                    if (distance < 33)
                        return ((distance - 17) >> 2) + 12;
                    return ((distance - 33) >> 3) + 16;
                }

                if (distance < 257)
                {
                    if (distance < 129)
                        return ((distance - 65) >> 4) + 20;
                    return ((distance - 129) >> 5) + 24;
                }

                if (distance < 513)
                    return ((distance - 257) >> 6) + 28;
                return ((distance - 513) >> 7) + 32;
            }

            if (distance < 16385)
            {
                if (distance < 4097)
                {
                    if (distance < 2049)
                        return ((distance - 1025) >> 8) + 36;
                    return ((distance - 2049) >> 9) + 40;
                }

                if (distance < 8193)
                    return ((distance - 4097) >> 10) + 44;
                return ((distance - 8193) >> 11) + 48;
            }

            if (distance < 65537)
            {
                if (distance < 32769)
                    return ((distance - 16385) >> 12) + 52;
                return ((distance - 32769) >> 14) + 56;
            }

            if (distance < 131073)
                return ((distance - 65537) >> 15) + 58;
            if (distance < 262145)
                return ((distance - 131073) >> 16) + 60;
            return ((distance - 262145) >> 17) + 62;
        }

        private unsafe bool PutChar(int c)
        {
            if (_bufLength == AcedConsts.BlockSize)
                if (!FlushBuffer())
                    return false;
            _pCharFreqCode[c]++;
            _pBuffer[_bufLength] = c;
            _bufLength++;
            return true;
        }

        private unsafe bool PutLenDist()
        {
            if (_bufLength == AcedConsts.BlockSize)
                if (!FlushBuffer())
                    return false;
            int bufPos = _bufLength;
            int c = GetLenChar(_length);
            _pCharFreqCode[c]++;
            _pBuffer[bufPos] = c;
            _pLen[bufPos] = _length;
            c = SplitDistance(_distance);
            if (c >= AcedConsts.FirstDistWithExBit)
            {
                _pDistExBits[bufPos] = _distance - _pDistExBitBase[c];
                _exBitCount += _pDistExBitLength[c];
            }

            _pDistFreqCode[c]++;
            _pDist[bufPos] = c;
            _bufLength = bufPos + 1;
            return true;
        }

        private unsafe int CompressCore()
        {
            fixed (int* pPrev = &_prev[0], pHash = &_hash[0], pNextCode = &_nextCode[0],
                pHeap = &_heap[0], pDepth = &_depth[0], pTree = &_tree[0],
                pBuffer = &_buffer[0], pLen = &_len[0], pDist = &_dist[0],
                pDistExBits = &_distExBits[0], pCharBitCount = &_charBitCount[0],
                pCharFreqCode = &_charFreqCode[0], pCharDadLen = &_charDadLen[0],
                pCharBitLen = &_charBitLen[0], pCharBitLenEx = &_charBitLenEx[0],
                pDistBitCount = &_distBitCount[0], pDistFreqCode = &_distFreqCode[0],
                pDistDadLen = &_distDadLen[0], pDistBitLen = &_distBitLen[0],
                pDistBitLenEx = &_distBitLenEx[0], pChLenBitCount = &_chLenBitCount[0],
                pChLenFreqCode = &_chLenFreqCode[0], pChLenDadLen = &_chLenDadLen[0],
                pCharExBitLength = &AcedConsts.CharExBitLength[0],
                pCharExBitBase = &AcedConsts.CharExBitBase[0],
                pDistExBitLength = &AcedConsts.DistExBitLength[0],
                pDistExBitBase = &AcedConsts.DistExBitBase[0],
                pChLenExBitLength = &AcedConsts.ChLenExBitLength[0])
            {
                _pPrev = pPrev;
                _pHash = pHash;
                _pNextCode = pNextCode;
                _pHeap = pHeap;
                _pDepth = pDepth;
                _pTree = pTree;
                _pBuffer = pBuffer;
                _pLen = pLen;
                _pDist = pDist;
                _pDistExBits = pDistExBits;
                _pCharBitCount = pCharBitCount;
                _pCharFreqCode = pCharFreqCode;
                _pCharDadLen = pCharDadLen;
                _pCharBitLen = pCharBitLen;
                _pCharBitLenEx = pCharBitLenEx;
                _pDistBitCount = pDistBitCount;
                _pDistFreqCode = pDistFreqCode;
                _pDistDadLen = pDistDadLen;
                _pDistBitLen = pDistBitLen;
                _pDistBitLenEx = pDistBitLenEx;
                _pChLenBitCount = pChLenBitCount;
                _pChLenFreqCode = pChLenFreqCode;
                _pChLenDadLen = pChLenDadLen;
                _pCharExBitLength = pCharExBitLength;
                _pCharExBitBase = pCharExBitBase;
                _pDistExBitLength = pDistExBitLength;
                _pDistExBitBase = pDistExBitBase;
                _pChLenExBitLength = pChLenExBitLength;
                AcedUtils.Fill(AcedConsts.InitPosValue, _pHash, _hashSize);
                AcedUtils.Fill(0, _pCharFreqCode, AcedConsts.CharCount);
                AcedUtils.Fill(0, _pDistFreqCode, AcedConsts.DistCount);
                _blockStart = 0;
                _bufLength = 0;
                _exBitCount = 0;
                _r0 = 0;
                _r1 = 0;
                _r2 = 0;
                _prevR0 = 0;
                _prevR1 = 0;
                _prevR2 = 0;
                _pPrev[0] = AcedConsts.InitPosValue;
                int hv = ((_pSrcBytes[0] << (_shift + _shift)) ^ (_pSrcBytes[1] << _shift) ^ _pSrcBytes[2]) & _hashMask;
                _pHash[hv] = 0;
                _dstIndex = 0;
                PutChar(_pSrcBytes[0]);
                _bits = 32;
                _hold = 0;
                int srcIndex = 1;
                while (srcIndex < _breakOffset - 2)
                {
                    hv = ((hv << _shift) ^ _pSrcBytes[srcIndex + 2]) & _hashMask;
                    _pPrev[srcIndex & _prevMask] = _hashHead = _pHash[hv];
                    _pHash[hv] = srcIndex;
                    _blockEndIndex = srcIndex;
                    _chain = _maxChain;
                    _prevLimit = srcIndex - _prevSize;
                    if (_hashHead < (_limit = srcIndex - AcedConsts.MaxDistance) || !FindFirstFragment(srcIndex))
                    {
                        if (!PutChar(_pSrcBytes[srcIndex]))
                            return -1;
                        srcIndex++;
                    }
                    else
                    {
                        int nextOffset = srcIndex + _length;
                        if (_hashHead > _prevLimit && (_hashHead = _pPrev[_hashHead & _prevMask]) >= _limit &&
                            nextOffset < _breakOffset && _length < AcedConsts.MaxLength)
                        {
                            FindNextFragment(srcIndex);
                            nextOffset = srcIndex + _length;
                        }

                        while (_length < lazyLimit && nextOffset < _breakOffset)
                        {
                            srcIndex++;
                            hv = ((hv << _shift) ^ _pSrcBytes[srcIndex + 2]) & _hashMask;
                            _pPrev[srcIndex & _prevMask] = _hashHead = _pHash[hv];
                            _pHash[hv] = srcIndex;
                            _chain = _maxChain;
                            _prevLimit = srcIndex - _prevSize;
                            if (_hashHead < (_limit = srcIndex - AcedConsts.MaxDistance) || !FindNextFragment(srcIndex))
                                break;
                            if (!PutChar(_pSrcBytes[srcIndex - 1]))
                                return -1;
                            nextOffset = srcIndex + _length;
                            _blockEndIndex = srcIndex;
                        }

                        if (!PutLenDist())
                            return -1;
                        if (nextOffset >= _breakOffset - 2)
                            srcIndex = nextOffset;
                        else
                        {
                            srcIndex++;
                            do
                            {
                                hv = ((hv << _shift) ^ _pSrcBytes[srcIndex + 2]) & _hashMask;
                                _pPrev[srcIndex & _prevMask] = _hashHead = _pHash[hv];
                                _pHash[hv] = srcIndex;
                                srcIndex++;
                            } while (srcIndex < nextOffset);
                        }
                    }
                }

                while (srcIndex < _breakOffset)
                {
                    _blockEndIndex = srcIndex;
                    if (!PutChar(_pSrcBytes[srcIndex]))
                        return -1;
                    srcIndex++;
                }

                _blockEndIndex = srcIndex;
                if (!FlushBitTrail())
                    return -1;
            }

            return _dstIndex;
        }

        private unsafe bool FindFirstFragment(int srcIndex)
        {
            byte* p = _pSrcBytes;
            uint m = (uint) p[srcIndex] | ((uint) p[srcIndex + 1] << 8) | ((uint) p[srcIndex + 2] << 16);
            int head = _hashHead;
            int boff = _breakOffset;
            int xoff = boff - 8;
            int chain = _chain;
            do
            {
                if ((*((uint*) (p + head)) & 0xFFFFFF) == m)
                {
                    int m1 = head + 3;
                    int m2 = srcIndex + 3;
                    if (m2 < boff && p[m1] == p[m2])
                    {
                        m1++;
                        m2++;
                        if (m2 < boff && p[m1] == p[m2])
                        {
                            do
                            {
                            } while (m2 < xoff && p[++m1] == p[++m2] &&
                                     p[++m1] == p[++m2] && p[++m1] == p[++m2] &&
                                     p[++m1] == p[++m2] && p[++m1] == p[++m2] &&
                                     p[++m1] == p[++m2] && p[++m1] == p[++m2] &&
                                     p[++m1] == p[++m2]);

                            if (p[m1] == p[m2])
                            {
                                m1++;
                                m2++;
                                while (m2 < boff && p[m1] == p[m2])
                                {
                                    m1++;
                                    m2++;
                                }
                            }

                            m2 -= srcIndex;
                            if (m2 < AcedConsts.MaxLength)
                                _length = m2;
                            else
                                _length = AcedConsts.MaxLength;
                            _distance = srcIndex - head;
                            return true;
                        }
                        else if (srcIndex - head <= len4MaxDist)
                        {
                            _length = 4;
                            _distance = srcIndex - head;
                            return true;
                        }
                    }
                    else if (srcIndex - head <= len3MaxDist)
                    {
                        _length = 3;
                        _distance = srcIndex - head;
                        return true;
                    }
                }

                if (head <= _prevLimit)
                    return false;
                _hashHead = head = _pPrev[head & _prevMask];
                if (head < _limit)
                    return false;
                _chain = --chain;
            } while (chain != 0);

            return false;
        }

        private unsafe bool FindNextFragment(int srcIndex)
        {
            byte* p = _pSrcBytes;
            int head = _hashHead;
            int len = _length;
            int boff = _breakOffset;
            int xoff = boff - 8;
            int chain = _chain;
            uint bf = *((uint*) (p + srcIndex + len - 3));
            bool result = false;
            do
            {
                if (*((uint*) (p + head + len - 3)) == bf && p[srcIndex] == p[head] && p[srcIndex + 1] == p[head + 1])
                {
                    int m1 = head + 1;
                    int m2 = srcIndex + 1;
                    if (len > 4)
                    {
                        int n = (len - 1) >> 2;
                        do
                        {
                            if (*((uint*) (p + m1 + 1)) != *((uint*) (p + m2 + 1)))
                                goto SkipThisSequence;
                            m1 += 4;
                            m2 += 4;
                            n--;
                        } while (n > 0);
                    }

                    while (m2 < xoff && p[++m1] == p[++m2] &&
                           p[++m1] == p[++m2] && p[++m1] == p[++m2] &&
                           p[++m1] == p[++m2] && p[++m1] == p[++m2] &&
                           p[++m1] == p[++m2] && p[++m1] == p[++m2] &&
                           p[++m1] == p[++m2]) ;
                    if (p[m1] == p[m2])
                    {
                        m1++;
                        m2++;
                        while (m2 < boff && p[m1] == p[m2])
                        {
                            m1++;
                            m2++;
                        }
                    }

                    m1 = m2 - srcIndex;
                    srcIndex -= head;
                    if (m1 - len > 2 || (m1 - len == 1 && (srcIndex < diff1Min || Test1Difference(srcIndex, _distance)))
                                     || (m1 - len == 2 &&
                                         (srcIndex < diff2Min || Test2Difference(srcIndex, _distance))))
                    {
                        _length = len = m1;
                        _distance = srcIndex;
                        result = true;
                        if (len >= AcedConsts.MaxLength)
                        {
                            _length = AcedConsts.MaxLength;
                            break;
                        }

                        if (m2 == boff)
                            break;
                        bf = *((uint*) (p + m2 - 3));
                    }

                    srcIndex += head;
                }

                SkipThisSequence:
                if (head <= _prevLimit)
                    break;
                head = _pPrev[head & _prevMask];
                if (head < _limit)
                    break;
            } while (--chain != 0);

            return result;
        }

        private static bool Test1Difference(int newDistance, int oldDistance)
        {
            if (newDistance < 16385)
            {
                if (newDistance < 4097)
                {
                    if (newDistance < 2049)
                    {
                        if (oldDistance < 3)
                            return false;
                    }
                    else if (oldDistance < 17)
                        return false;
                }
                else if (newDistance < 8193)
                {
                    if (oldDistance < 33)
                        return false;
                }
                else if (oldDistance < 65)
                    return false;
            }
            else if (newDistance < 65537)
            {
                if (newDistance < 32769)
                {
                    if (oldDistance < 129)
                        return false;
                }
                else if (oldDistance < 513)
                    return false;
            }
            else if (newDistance < 131073)
            {
                if (oldDistance < 2049)
                    return false;
            }
            else if (newDistance < 262145)
            {
                if (oldDistance < 4097)
                    return false;
            }
            else if (oldDistance < 8193)
                return false;

            return true;
        }

        private static bool Test2Difference(int newDistance, int oldDistance)
        {
            if (newDistance < 131073)
            {
                if (newDistance < 65537)
                {
                    if (oldDistance < 3)
                        return false;
                }

                if (oldDistance < 17)
                    return false;
            }

            if (newDistance < 262145)
            {
                if (oldDistance < 33)
                    return false;
            }
            else if (oldDistance < 65)
                return false;

            return true;
        }

        private unsafe bool FlushBitTrail()
        {
            if (FlushBuffer())
            {
                _dstIndex += 4 - (_bits >> 3);
                if (_dstIndex <= _dstBreak)
                {
                    if (_growMode)
                    {
                        if (_chunkLength == AcedConsts.ChunkCapacity)
                            NextChunk();
                        _chunk[_chunkLength] = _hold;
                        _chunkLength++;
                    }
                    else if (!_tryMode)
                        switch (_bits >> 3)
                        {
                            case 0:
                                *((uint*) _pDstBytes) = _hold;
                                break;
                            case 1:
                                _pDstBytes[0] = (byte) _hold;
                                _pDstBytes[1] = (byte) (_hold >> 8);
                                _pDstBytes[2] = (byte) (_hold >> 16);
                                break;
                            case 2:
                                _pDstBytes[0] = (byte) _hold;
                                _pDstBytes[1] = (byte) (_hold >> 8);
                                break;
                            case 3:
                                *_pDstBytes = (byte) _hold;
                                break;
                        }

                    return true;
                }
            }

            return false;
        }

        public unsafe byte[] Compress(byte[] sourceBytes, int sourceIndex, int sourceLength,
            AcedCompressionLevel compressionLevel, int beforeGap, int afterGap)
        {
            if (sourceLength == 0)
                return new byte[beforeGap + afterGap + 4];
            if (sourceBytes == null)
                AcedMCException.ThrowArgumentNullException("sourceBytes");
            _growMode = true;
            _tryMode = false;
            _chunkList = null;
            _chunk = new uint[AcedConsts.ChunkCapacity];
            _chunkLength = 0;
            _dstBreak = sourceLength;
            _breakOffset = sourceLength;
            _pDstBytes = null;
            byte[] result;
            fixed (byte* pSrcBytes = &sourceBytes[sourceIndex])
            {
                _pSrcBytes = pSrcBytes;
                int outSize = -1;
                if (sourceLength > 3 && compressionLevel != AcedCompressionLevel.Store)
                {
                    SetupCompressionLevel(compressionLevel);
                    outSize = CompressCore();
                }

                if (outSize > 0)
                {
                    result = new byte[beforeGap + 4 + outSize + afterGap];
                    fixed (byte* pResult = &result[beforeGap])
                        *((int*) pResult) = sourceLength;
                    beforeGap += 4;
                    outSize--;
                    int n = outSize >> AcedConsts.ChunkShift;
                    for (int i = 0; i < n; i++)
                    {
                        Buffer.BlockCopy((uint[]) _chunkList[i], 0, result, beforeGap, AcedConsts.ChunkCapacity * 4);
                        beforeGap += AcedConsts.ChunkCapacity * 4;
                    }

                    Buffer.BlockCopy(_chunk, 0, result, beforeGap,
                        (outSize & ((AcedConsts.ChunkCapacity * 4) - 1)) + 1);
                    _chunkList = null;
                    _chunk = null;
                }
                else
                {
                    _chunkList = null;
                    _chunk = null;
                    result = new byte[beforeGap + 4 + sourceLength + afterGap];
                    fixed (byte* pResult = &result[beforeGap])
                        *((int*) pResult) = -sourceLength;
                    Buffer.BlockCopy(sourceBytes, sourceIndex, result, beforeGap + 4, sourceLength);
                }
            }

            return result;
        }

        public unsafe int Compress(byte[] sourceBytes, int sourceIndex, int sourceLength,
            AcedCompressionLevel compressionLevel, byte[] destinationBytes, int destinationIndex)
        {
            _tryMode = false;
            if (destinationBytes != null)
                _dstBreak = destinationBytes.Length - destinationIndex - 4;
            else
            {
                _tryMode = true;
                destinationBytes = _fakeDestinationBytes;
                destinationIndex = 0;
                _dstBreak = sourceLength;
            }

            if (sourceLength == 0)
            {
                if (!_tryMode)
                {
                    if (_dstBreak < 0)
                        AcedMCException.ThrowNoPlaceToStoreCompressedDataException();
                    fixed (byte* pDstBytes = &destinationBytes[destinationIndex])
                        *((int*) pDstBytes) = 0;
                }

                return 4;
            }

            if (sourceBytes == null)
                AcedMCException.ThrowArgumentNullException("sourceBytes");
            _growMode = false;
            if (sourceLength < _dstBreak)
                _dstBreak = sourceLength;
            _breakOffset = sourceLength;
            fixed (byte* pSrcBytes = &sourceBytes[sourceIndex], pDstBytes = &destinationBytes[destinationIndex])
            {
                _pSrcBytes = pSrcBytes;
                _pDstBytes = pDstBytes;
                int result = -1;
                if (sourceLength > 3 && compressionLevel != AcedCompressionLevel.Store)
                {
                    SetupCompressionLevel(compressionLevel);
                    if (!_tryMode)
                    {
                        if (_dstBreak < 0)
                            AcedMCException.ThrowNoPlaceToStoreCompressedDataException();
                        *((int*) _pDstBytes) = sourceLength;
                        _pDstBytes += 4;
                    }

                    result = CompressCore();
                }

                if (result < 0)
                {
                    if (!_tryMode)
                    {
                        if (_dstBreak < sourceLength)
                            AcedMCException.ThrowNoPlaceToStoreCompressedDataException();
                        *((int*) pDstBytes) = -sourceLength;
                        Buffer.BlockCopy(sourceBytes, sourceIndex, destinationBytes, destinationIndex + 4,
                            sourceLength);
                    }

                    result = sourceLength;
                }

                return result + 4;
            }
        }

        private void SetupCompressionLevel(AcedCompressionLevel compressionLevel)
        {
            switch (compressionLevel)
            {
                case AcedCompressionLevel.Fastest:
                    _hashSize = hashSizeFastest;
                    _hashMask = hashMaskFastest;
                    _prevSize = prevSizeFastest;
                    _prevMask = prevMaskFastest;
                    _maxChain = chainFastest;
                    _shift = shiftFastest;
                    break;
                case AcedCompressionLevel.Fast:
                    _hashSize = hashSizeFast;
                    _hashMask = hashMaskFast;
                    _prevSize = prevSizeFast;
                    _prevMask = prevMaskFast;
                    _maxChain = chainFast;
                    _shift = shiftFast;
                    break;
                case AcedCompressionLevel.Normal:
                    _hashSize = hashSizeNormal;
                    _hashMask = hashMaskNormal;
                    _prevSize = prevSizeNormal;
                    _prevMask = prevMaskNormal;
                    _maxChain = chainNormal;
                    _shift = shiftNormal;
                    break;
                case AcedCompressionLevel.Maximum:
                    _hashSize = hashSizeMaximum;
                    _hashMask = hashMaskMaximum;
                    _prevSize = prevSizeMaximum;
                    _prevMask = prevMaskMaximum;
                    _maxChain = chainMaximum;
                    _shift = shiftMaximum;
                    break;
            }
        }

        public static void Release()
        {
            _instance = null;
        }
    }
}