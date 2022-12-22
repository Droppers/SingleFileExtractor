using System.Collections.Generic;
using JetBrains.Annotations;

namespace SingleFileExtractor.Core
{
    [PublicAPI]
    public record Bundle(
        int MajorVersion,
        int MinorVersion,
        string Hash,
        IReadOnlyList<FileEntry> Files);
}