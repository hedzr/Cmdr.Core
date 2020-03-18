namespace HzNS.MdxLib.models
{
    public class ContentIndex
    {
        public ulong CompressedSize { get; set; }
        public ulong UncompressedSize { get; set; }
        public ulong Offset { get; set; }
        public ulong OffsetUncomp { get; set; }

        public override string ToString()
        {
            return string.Format("OFS: {0:D7}(0x{0:X08})/{1}(0x{1:X}) - LEN: {2:D7}(0x{2:X})/uncomp:{3}(0x{3:X})",
                Offset, OffsetUncomp, CompressedSize, UncompressedSize);
        }
    }
}