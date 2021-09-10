using System.IO;

namespace SingleFileExtractor.Core
{
    public record FileEntry(long Offset, long Size, long CompressedSize, FileType Type, string RelativePath)
    {
        public bool IsCompressed => CompressedSize > 0;

        public static FileEntry FromBinaryReader(BinaryReader reader, uint majorVersion)
        {
            var offset = reader.ReadInt64();
            var size = reader.ReadInt64();

            long compressedSize = 0;
            if (majorVersion >= 6)
            {
                compressedSize = reader.ReadInt64();
            }

            var type = (FileType)reader.ReadByte();
            var relativePath = reader.ReadString();

            return new FileEntry(offset, size, compressedSize, type, relativePath);
        }
    }
}