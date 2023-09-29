using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using SingleFileExtractor.Core.Helpers;

namespace SingleFileExtractor.Core
{
    internal class BundleReader
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

        // A fast method of locating the bundle signature using KMP search
        // https://github.com/dotnet/runtime/blob/84de9b678613675e0444b265905c82d33dae33a8/src/installer/managed/Microsoft.NET.HostModel/AppHost/HostWriter.cs
        public static bool TryReadStartupInfo(ExecutableReader executableReader, [NotNullWhen(true)] out StartupInfo? startupInfo)
        {
            var position = BinaryKmpSearch.SearchInFile(executableReader.ViewAccessor, BundleSignature);
            if (position == -1)
            {
                startupInfo = default;
                return false;
            }

            var entryPointPath = ReadEntryPoint(executableReader.ViewAccessor, position + BundleSignature.Length);
            var manifestOffset = executableReader.ViewAccessor.ReadInt64(position - sizeof(long));
            startupInfo = new StartupInfo(entryPointPath, manifestOffset);
            return true;
        }

        // Extremely janky method for finding the relative path to the entry point managed DLL
        // Why? It saves me a lot of time
        private static unsafe string? ReadEntryPoint(UnmanagedMemoryAccessor memoryAccessor, long startOffset)
        {
            Span<byte> buffer = stackalloc byte[1024];
            
            foreach (var entryPointOffset in KnownEntryPointPathOffsets)
            {
                // Check if it is the start of a sequence of ASCII characters
                var previousByte = memoryAccessor.ReadByte(startOffset + entryPointOffset - 1);
                var firstByte = memoryAccessor.ReadByte(startOffset + entryPointOffset);
                if (firstByte is < 32 or > 126 || previousByte is not 0)
                {
                    continue;
                }

                startOffset += entryPointOffset; // Max file path is 1024 chars
                var index = 0;
                while (true)
                {
                    var character = memoryAccessor.ReadByte(startOffset + index);
                    if (character == 0)
                    {
                        break;
                    }

                    buffer[index] = character;
                    index++;
                }

                return Encoding.UTF8.GetString(buffer[..index]);
            }

            return null;
        }
    }
}