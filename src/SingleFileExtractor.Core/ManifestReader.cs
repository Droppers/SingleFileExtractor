using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using SingleFileExtractor.Core.Exceptions;
using SingleFileExtractor.Core.Helpers;

namespace SingleFileExtractor.Core
{
    public class ManifestReader : IManifestReader
    {
        private static readonly byte[] BundleSignature =
        {
            // 32 bytes represent the bundle signature: SHA-256 for ".net core bundle"
            0x8b, 0x12, 0x02, 0xb9, 0x6a, 0x61, 0x20, 0x38,
            0x72, 0x7b, 0x93, 0x02, 0x14, 0xd7, 0xa0, 0x32,
            0x13, 0xf5, 0xb9, 0xe6, 0xef, 0xae, 0x33, 0x18,
            0xee, 0x3b, 0x2d, 0xce, 0x24, 0xb3, 0x6a, 0xae
        };

        public Manifest Read(string fileName)
        {
            using var memoryMappedFile =
                MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using MemoryMappedViewAccessor accessor =
                memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

            var headerOffset = FindManifestHeaderOffset(accessor);
            return Read(accessor, headerOffset);
        }

        public Manifest Read(MemoryMappedViewAccessor viewAccessor)
        {
            var headerOffset = FindManifestHeaderOffset(viewAccessor);
            return Read(viewAccessor, headerOffset);
        }

        private static Manifest Read(MemoryMappedViewAccessor viewAccessor, long offset)
        {
            var stream = new UnmanagedMemoryStream(viewAccessor.SafeMemoryMappedViewHandle, offset,
                viewAccessor.Capacity - offset);
            using var br = new BinaryReader(stream, Encoding.ASCII);

            var majorVersion = br.ReadUInt32();
            var minorVersion = br.ReadUInt32();
            var fileCount = br.ReadInt32();
            var bundleHash = br.ReadString();

            if (majorVersion >= 2)
            {
                // We can skip these? They are included in file entries anyways.
                br.ReadInt64(); // depsOffset
                br.ReadInt64(); // depsSize
                br.ReadInt64(); // runtimeConfigOffset
                br.ReadInt64(); // runtimeConfigSize
                br.ReadUInt64(); // flags
            }

            var files = new List<FileEntry>();
            for (var i = 0; i < fileCount; i++)
            {
                files.Add(FileEntry.FromBinaryReader(br, majorVersion));
            }

            return new Manifest((int)majorVersion, (int)minorVersion, bundleHash, files);
        }

        // A fast method of locating the bundle signature using KMP search
        // https://github.com/dotnet/runtime/blob/84de9b678613675e0444b265905c82d33dae33a8/src/installer/managed/Microsoft.NET.HostModel/AppHost/HostWriter.cs
        private static long FindManifestHeaderOffset(MemoryMappedViewAccessor viewAccessor)
        {
            var position = BinaryKmpSearch.SearchInFile(viewAccessor, BundleSignature);
            if (position == -1)
            {
                throw new UnsupportedExecutableException("Is not a .NET Core executable.");
            }

            var headerOffset = viewAccessor.ReadInt64(position - sizeof(long));
            if (headerOffset == 0)
            {
                throw new UnsupportedExecutableException("Only single file executables can be extracted.");
            }

            return headerOffset;
        }
    }
}