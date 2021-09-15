using System.Collections.Generic;

namespace SingleFileExtractor.Core
{
    public record Manifest(
        StartupInfo StartupInfo,
        int MajorVersion,
        int MinorVersion,
        string Hash,
        IReadOnlyList<FileEntry> Files);
}