using System.IO.MemoryMappedFiles;

namespace SingleFileExtractor.Core
{
    public interface IExecutableReader
    {
        Manifest ReadManifest(string fileName);
        
        StartupInfo ReadStartupInfo(string fileName);
    }
}