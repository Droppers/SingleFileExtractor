namespace SingleFileExtractor.Core
{
    public record StartupInfo(string? EntryPoint, long ManifestOffset)
    {
        public bool IsBundle => ManifestOffset != 0;
    }
}