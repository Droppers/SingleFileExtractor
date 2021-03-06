using System.IO.MemoryMappedFiles;

namespace SingleFileExtractor.Core
{
    public interface IBundleExtractor
    {
        Manifest ExtractToDirectory(string fileName, string outputDirectory);
    }
}