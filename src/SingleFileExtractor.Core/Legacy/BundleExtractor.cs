using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using JetBrains.Annotations;
using SingleFileExtractor.Core.Helpers;

namespace SingleFileExtractor.Core.Legacy;

public class BundleExtractor : IBundleExtractor
{
    private readonly IExecutableReader _executableReader;

    [PublicAPI]
    public BundleExtractor() : this(new ExecutableReader()) { }

    [PublicAPI]
    public BundleExtractor(IExecutableReader executableReader)
    {
        _executableReader = executableReader;
    }

    [PublicAPI]
    public Manifest ExtractToDirectory(string fileName, string outputDirectory)
    {
        if (!File.Exists(fileName))
        {
            Console.WriteLine(fileName);
            throw new FileNotFoundException("Path to single file executable does not exist.", fileName);
        }


        var manifest = _executableReader.ReadManifest(fileName);
        using var accessor = MemoryMappedFileHelper.CreateViewAccessor(fileName);

        // Maintain a list of extracted files to be able to remove them if extraction fails
        var fileNames = new List<string>();
        try
        {
            foreach (var file in manifest.Files)
            {
                var targetFileName = Path.Combine(outputDirectory, file.RelativePath);
                fileNames.Add(targetFileName);

                ExtractToFile(accessor, targetFileName, file);
            }
        }
        catch
        {
            RemoveFiles(fileNames);
            throw;
        }

        return manifest;
    }

    [PublicAPI]
    public static Manifest Extract(string fileName, string outputDirectory) =>
        new BundleExtractor().ExtractToDirectory(fileName, outputDirectory);

    internal static void ExtractToFile(MemoryMappedViewAccessor viewAccessor, string targetFileName, FileEntry file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(targetFileName)!);
        using var destination = File.OpenWrite(targetFileName);
        using var entryStream = GetStreamForFileEntry(viewAccessor, file);
        entryStream.CopyTo(destination);
    }

    private static Stream GetStreamForFileEntry(MemoryMappedViewAccessor viewAccessor, FileEntry file)
    {
        if (!file.IsCompressed)
        {
            return new UnmanagedMemoryStream(viewAccessor.SafeMemoryMappedViewHandle, file.Offset, file.Size);
        }

        using var compressedStream = new UnmanagedMemoryStream(viewAccessor.SafeMemoryMappedViewHandle, file.Offset,
            file.CompressedSize);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        var decompressedStream = new MemoryStream((int)file.Size);
        deflateStream.CopyTo(decompressedStream);
        if (decompressedStream.Length != file.Size)
        {
            throw new InvalidDataException(
                $"Single file entry {file.RelativePath} with compressed size {file.CompressedSize}, was decompressed to size {decompressedStream.Length} but expected {file.Size}.");
        }

        decompressedStream.Seek(0, SeekOrigin.Begin);
        return decompressedStream;
    }

    private static void RemoveFiles(IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            if (!File.Exists(fileName))
            {
                continue;
            }

            try
            {
                File.Delete(fileName);
            }
            catch
            {
                // ignored
            }
        }
    }
}