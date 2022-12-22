using System.IO;
using JetBrains.Annotations;
using SingleFileExtractor.Core.Helpers;

namespace SingleFileExtractor.Core.Legacy
{
    [PublicAPI]
    public record FileEntry(
        string ContainingFileName,
        long Offset,
        long Size,
        long CompressedSize,
        FileType Type,
        string RelativePath)
    {
        public bool IsCompressed => CompressedSize > 0;

        public static FileEntry FromBinaryReader(BinaryReader reader, uint majorVersion, string containingFileName)
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

            return new FileEntry(containingFileName, offset, size, compressedSize, type, relativePath);
        }

        public void Extract(string targetFileName)
        {
            using var accessor = MemoryMappedFileHelper.CreateViewAccessor(ContainingFileName);
            BundleExtractor.ExtractToFile(accessor, targetFileName, this);
        }
    }
}