using JetBrains.Annotations;

namespace SingleFileExtractor.Core;

[PublicAPI]
public record StartupInfo(string? EntryPoint, long ManifestOffset)
{
    public bool IsSingleFile => ManifestOffset is not 0;
}