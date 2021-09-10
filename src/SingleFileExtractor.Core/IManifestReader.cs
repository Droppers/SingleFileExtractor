using System.IO.MemoryMappedFiles;

namespace SingleFileExtractor.Core
{
    public interface IManifestReader
    {
        Manifest Read(string fileName);

        Manifest Read(MemoryMappedViewAccessor viewAccessor);
    }
}