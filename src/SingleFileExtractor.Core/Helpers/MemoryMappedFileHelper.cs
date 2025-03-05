using System.IO;
using System.IO.MemoryMappedFiles;

namespace SingleFileExtractor.Core.Helpers;

internal static class MemoryMappedFileHelper
{
    public static MemoryMappedViewAccessor CreateViewAccessor(string fileName)
    {
        using var memoryMappedFile =
            MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        return memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
    }
}