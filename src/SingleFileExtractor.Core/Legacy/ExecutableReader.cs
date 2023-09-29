using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using SingleFileExtractor.Core.Exceptions;
using SingleFileExtractor.Core.Helpers;

namespace SingleFileExtractor.Core.Legacy
{
    public class ExecutableReader : IExecutableReader
    {
        private static readonly int[] KnownEntryPointPathOffsets =
        {
            56, // 6.0
            48, 64, // 5.0
            200 // 3.x
        };


        private static readonly byte[] BundleSignature =
        {
            // 32 bytes represent the bundle signature: SHA-256 for ".net core bundle"
            0x8b, 0x12, 0x02, 0xb9, 0x6a, 0x61, 0x20, 0x38,
            0x72, 0x7b, 0x93, 0x02, 0x14, 0xd7, 0xa0, 0x32,
            0x13, 0xf5, 0xb9, 0xe6, 0xef, 0xae, 0x33, 0x18,
            0xee, 0x3b, 0x2d, 0xce, 0x24, 0xb3, 0x6a, 0xae
        };

        public Manifest ReadManifest(string fileName)
        {
            using var accessor = MemoryMappedFileHelper.CreateViewAccessor(fileName);

            var startupInfo = ReadStartupInfo(accessor);
            return Read(fileName, accessor, startupInfo);
        }

        public StartupInfo ReadStartupInfo(string fileName)
        {
            using var accessor = MemoryMappedFileHelper.CreateViewAccessor(fileName);
            return ReadStartupInfo(accessor);
        }

        // A fast method of locating the bundle signature using KMP search
        // https://github.com/dotnet/runtime/blob/84de9b678613675e0444b265905c82d33dae33a8/src/installer/managed/Microsoft.NET.HostModel/AppHost/HostWriter.cs
        private static StartupInfo ReadStartupInfo(MemoryMappedViewAccessor viewAccessor)
        {
            var position = BinaryKmpSearch.SearchInFile(viewAccessor, BundleSignature);
            if (position == -1)
            {
                throw new UnsupportedExecutableException("Is not a .NET Core 3.x, 5.0 or 6.0 executable.");
            }

            var entryPointPath = ReadEntryPoint(viewAccessor, position + BundleSignature.Length);
            var manifestOffset = viewAccessor.ReadInt64(position - sizeof(long));
            return new StartupInfo(entryPointPath, manifestOffset);
        }

        private static Manifest Read(string fileName, MemoryMappedViewAccessor viewAccessor, StartupInfo startupInfo)
        {
            if (startupInfo.ManifestOffset == 0)
            {
                throw new UnsupportedExecutableException("Only single file executables can be extracted.");
            }

            var stream = new UnmanagedMemoryStream(viewAccessor.SafeMemoryMappedViewHandle, startupInfo.ManifestOffset,
                viewAccessor.Capacity - startupInfo.ManifestOffset);
            using var br = new BinaryReader(stream, Encoding.UTF8);

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
                files.Add(FileEntry.FromBinaryReader(br, majorVersion, fileName));
            }

            return new Manifest(startupInfo, (int)majorVersion, (int)minorVersion, bundleHash, files);
        }

        // Extremely janky method for finding the relative path to the entry point managed DLL
        // Why? It saves me a lot of time
        private static string? ReadEntryPoint(MemoryMappedViewAccessor viewAccessor, long startOffset)
        {
            foreach (var entryPointOffset in KnownEntryPointPathOffsets)
            {
                // Check if it is the start of a sequence of ASCII characters
                var previousByte = viewAccessor.ReadByte(startOffset + entryPointOffset - 1);
                var firstByte = viewAccessor.ReadByte(startOffset + entryPointOffset);
                if (firstByte < 32 || firstByte > 126 || previousByte != 0)
                {
                    continue;
                }

                startOffset += entryPointOffset;

                var buffer = new byte[1024]; // Max file path is 1024 chars
                var index = 0;
                while (true)
                {
                    var character = viewAccessor.ReadByte(startOffset + index);
                    if (character == 0)
                    {
                        break;
                    }

                    buffer[index] = character;
                    index++;
                }

                return Encoding.ASCII.GetString(buffer, 0, index);
            }

            return null;
        }
    }
}