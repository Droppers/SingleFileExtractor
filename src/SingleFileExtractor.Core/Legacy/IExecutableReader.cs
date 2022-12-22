namespace SingleFileExtractor.Core.Legacy
{
    public interface IExecutableReader
    {
        Manifest ReadManifest(string fileName);
        
        StartupInfo ReadStartupInfo(string fileName);
    }
}