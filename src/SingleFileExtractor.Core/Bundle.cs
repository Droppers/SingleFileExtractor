using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace SingleFileExtractor.Core;

[PublicAPI]
public record Bundle(
    int MajorVersion,
    int MinorVersion,
    string Hash,
    IReadOnlyList<FileEntry> Files,
    ulong? Flags)
{
    internal int BundleOffset { get; init; }

    internal static Bundle FromExecutableReader(ExecutableReader executableReader)
    {
        if (executableReader.StartupInfo.ManifestOffset == 0)
        {
            throw new InvalidOperationException("Only single file executables can be extracted.");
        }

        var stream = new UnmanagedMemoryStream(executableReader.ViewAccessor.SafeMemoryMappedViewHandle,
            executableReader.StartupInfo.ManifestOffset,
            executableReader.ViewAccessor.Capacity - executableReader.StartupInfo.ManifestOffset);
        using var br = new BinaryReader(stream, Encoding.UTF8);

        var majorVersion = br.ReadUInt32();
        var minorVersion = br.ReadUInt32();
        var fileCount = br.ReadInt32();
        var bundleHash = br.ReadString();

        ulong? flags = null;
        if (majorVersion >= 2)
        {
            // We can skip these, they are included in file entries anyways.
            br.ReadInt64(); // depsOffset
            br.ReadInt64(); // depsSize
            br.ReadInt64(); // runtimeConfigOffset
            br.ReadInt64(); // runtimeConfigSize

            flags = br.ReadUInt64(); // flags
        }

        var files = new FileEntry[fileCount];
        for (var i = 0; i < fileCount; i++)
        {
            files[i] = FileEntry.FromBinaryReader(executableReader, br, majorVersion);
        }

        return new Bundle((int)majorVersion, (int)minorVersion, bundleHash, files, flags);
    }
}