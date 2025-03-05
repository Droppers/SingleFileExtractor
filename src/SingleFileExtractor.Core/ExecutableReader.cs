using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SingleFileExtractor.Core.Exceptions;
using SingleFileExtractor.Core.Helpers;

namespace SingleFileExtractor.Core;

public class ExecutableReader : IDisposable
{
    private StartupInfo? _startupInfo;
    private Bundle? _bundle;
    private bool _isSupported;

    [PublicAPI]
    public ExecutableReader(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("Path to single file executable does not exist.", fileName);
        }


        FileName = fileName;
        ViewAccessor = MemoryMappedFileHelper.CreateViewAccessor(fileName);
    }

    internal MemoryMappedViewAccessor ViewAccessor { get; }

    [PublicAPI] public string FileName { get; }

    [PublicAPI]
    public bool IsSupported
    {
        get
        {
            ReadStartupInfo();
            return _isSupported;
        }
    }

    [PublicAPI]
    public StartupInfo StartupInfo => ReadStartupInfo() ??
                                      throw new UnsupportedExecutableException(
                                          "Is not a .NET Core 3.x, 5.0 or 6.0 executable.");

    [PublicAPI] public bool IsSingleFile => StartupInfo.IsSingleFile;

    [PublicAPI] public Bundle Bundle => _bundle ??= Bundle.FromExecutableReader(this);

    [PublicAPI]
    public void ExtractToDirectory(string outputDirectory)
    {
        GuardAgainstNotSingleFile();

        // Maintain a list of extracted files to be able to remove them if extraction fails
        var fileNames = new List<string>();
        try
        {
            foreach (var file in Bundle.Files)
            {
                var targetFileName = Path.Combine(outputDirectory, file.RelativePath);
                fileNames.Add(targetFileName);

                file.ExtractToFile(targetFileName);
            }
        }
        catch
        {
            CleanupFiles(fileNames);
            throw;
        }
    }

    [PublicAPI]
    public async Task ExtractToDirectoryAsync(string outputDirectory, CancellationToken cancellationToken = default)
    {
        GuardAgainstNotSingleFile();

        // Maintain a list of extracted files to be able to remove them if extraction fails
        var extractedFileNames = new List<string>();
        try
        {
            foreach (var file in Bundle.Files)
            {
                var targetFileName = Path.Combine(outputDirectory, file.RelativePath);
                extractedFileNames.Add(targetFileName);

                await file.ExtractToFileAsync(targetFileName, cancellationToken);
            }
        }
        catch
        {
            CleanupFiles(extractedFileNames);
            throw;
        }
    }

    private StartupInfo? ReadStartupInfo()
    {
        if (_startupInfo is not null)
        {
            return _startupInfo;
        }

        if (BundleReader.TryReadStartupInfo(this, out var startupInfo))
        {
            _isSupported = true;
            return _startupInfo = startupInfo;
        }

        _isSupported = false;
        return null;
    }

    private void GuardAgainstNotSingleFile()
    {
        if (!IsSingleFile)
        {
            throw new InvalidOperationException("Only single file executables can be extracted.");
        }
    }

    private static void CleanupFiles(IEnumerable<string> fileNames)
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

    public void Dispose()
    {
        ViewAccessor.Dispose();
    }
}