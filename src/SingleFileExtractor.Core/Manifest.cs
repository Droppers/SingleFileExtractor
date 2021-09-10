using System.Collections.Generic;

namespace SingleFileExtractor.Core
{
    public record Manifest(int MajorVersion, int MinorVersion, string Hash, IReadOnlyList<FileEntry> Files);
}