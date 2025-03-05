namespace SingleFileExtractor.Core.Legacy;

public record StartupInfo(string? EntryPoint, long ManifestOffset)
{
    public bool IsBundle => ManifestOffset != 0;
}