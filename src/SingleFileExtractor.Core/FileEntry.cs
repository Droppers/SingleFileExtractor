﻿using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SingleFileExtractor.Core;

[PublicAPI]
public record FileEntry(
    ExecutableReader ExecutableReader,
    long Offset,
    long Size,
    long CompressedSize,
    FileType Type,
    string RelativePath)
{
    public bool IsCompressed => CompressedSize > 0;

    internal static FileEntry FromBinaryReader(
        ExecutableReader executableReader,
        BinaryReader reader,
        uint majorVersion)
    {
        var offset = reader.ReadInt64();
        var size = reader.ReadInt64();

        long compressedSize = 0;
        if (majorVersion >= 6)
        {
            compressedSize = reader.ReadInt64();
        }

        var type = (FileType)reader.ReadByte();
        var relativePath = reader.ReadString();

        return new FileEntry(executableReader, offset, size, compressedSize, type, relativePath);
    }

    [PublicAPI]
    public void ExtractToFile(string targetFileName)
    {
        using var destination = OpenDestinationStream(targetFileName);
        using var entryStream = AsStream();
        entryStream.CopyTo(destination);
    }

    [PublicAPI]
    public async Task ExtractToFileAsync(string targetFileName, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        using var destination = OpenDestinationStream(targetFileName);
        using var entryStream = await AsStreamAsync(cancellationToken);
        await entryStream.CopyToAsync(destination, 81920, cancellationToken);
#else
        await using var destination = OpenDestinationStream(targetFileName);
        await using var entryStream = await AsStreamAsync(cancellationToken);
        await entryStream.CopyToAsync(destination, cancellationToken);
#endif
    }

    [PublicAPI]
    public Stream AsStream()
    {
        if (!IsCompressed)
        {
            return new UnmanagedMemoryStream(ExecutableReader.ViewAccessor.SafeMemoryMappedViewHandle, Offset, Size);
        }

        using var compressedStream = new UnmanagedMemoryStream(ExecutableReader.ViewAccessor.SafeMemoryMappedViewHandle,
            Offset, CompressedSize);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        var decompressedStream = new MemoryStream((int)Size);
        deflateStream.CopyTo(decompressedStream);
        if (decompressedStream.Length != Size)
        {
            throw new InvalidDataException(
                $"Single file entry {RelativePath} with compressed size {CompressedSize}, was decompressed to size {decompressedStream.Length} but expected {Size}.");
        }

        decompressedStream.Seek(0, SeekOrigin.Begin);
        return decompressedStream;
    }

    [PublicAPI]
    public async Task<Stream> AsStreamAsync(CancellationToken cancellationToken = default)
    {
        if (!IsCompressed)
        {
            return new UnmanagedMemoryStream(ExecutableReader.ViewAccessor.SafeMemoryMappedViewHandle, Offset, Size);
        }

        var decompressedStream = new MemoryStream((int)Size);

#if NETSTANDARD2_0
        using var compressedStream = new UnmanagedMemoryStream(ExecutableReader.ViewAccessor.SafeMemoryMappedViewHandle,
            Offset, CompressedSize);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        await deflateStream.CopyToAsync(decompressedStream, 81920, cancellationToken);
#else
        await using var compressedStream = new UnmanagedMemoryStream(
            ExecutableReader.ViewAccessor.SafeMemoryMappedViewHandle, Offset, CompressedSize);
        await using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        await deflateStream.CopyToAsync(decompressedStream, cancellationToken);
#endif

        decompressedStream.Seek(0, SeekOrigin.Begin);
        return decompressedStream;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDecompressionFailed(Stream decompressedStream)
    {
        if (decompressedStream.Length != Size)
        {
            throw new InvalidDataException(
                $"Single file entry {RelativePath} with compressed size {CompressedSize}, was decompressed to size {decompressedStream.Length} but expected {Size}.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Stream OpenDestinationStream(string fileName)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
        return File.OpenWrite(fileName);
    }
}