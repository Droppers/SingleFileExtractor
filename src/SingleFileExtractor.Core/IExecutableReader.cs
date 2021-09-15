using System.IO.MemoryMappedFiles;

namespace SingleFileExtractor.Core
{
    public interface IExecutableReader
    {
        Manifest ReadManifest(string fileName);

        Manifest ReadManifest(MemoryMappedViewAccessor viewAccessor);

        StartupInfo ReadStartupInfo(string fileName);

        StartupInfo ReadStartupInfo(MemoryMappedViewAccessor viewAccessor);
    }
}