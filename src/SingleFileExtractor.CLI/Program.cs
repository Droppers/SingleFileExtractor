﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SingleFileExtractor.Core;
using SingleFileExtractor.Core.Exceptions;

var app = new CommandLineApplication();
app.HelpOption();

var fileOption = app.Argument("executable", "The single file executable to be extracted").IsRequired();
var outputOption = app.Option("-o|--output <DIRECTORY>",
    "(optional) The directory to write the extracted files to. Omit this option to only list the files.",
    CommandOptionType.SingleValue);

app.OnExecuteAsync(async cancellationToken =>
{
    if (fileOption.Value != null)
    {
        await RunExtractorAsync(fileOption.Value, outputOption.Value(), cancellationToken);
    }
    else
    {
        app.ShowHelp();
    }
});

app.OnValidationError(error =>
{
    app.ShowHelp();

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(error.ErrorMessage);
    Console.ResetColor();
});

return await app.ExecuteAsync(args);

static async Task RunExtractorAsync(string fileName, string? outputDirectory, CancellationToken cancellationToken)
{
    try
    {
        var reader = new ExecutableReader(fileName);
        if (!reader.IsSupported)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Is not a .NET Core 3.x or greater executable.");
            Console.ResetColor();
            return;
        }

        if (reader.StartupInfo.EntryPoint is not null)
        {
            Console.WriteLine($"Entry point: {reader.StartupInfo.EntryPoint}");
        }

        if (!reader.IsSingleFile)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                "Executable is not a single file executable, note that assembly files (*.dll) are not executables.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine($"Bundle version: {reader.Bundle.MajorVersion}.{reader.Bundle.MinorVersion}");

        if (outputDirectory is null)
        {
            Console.WriteLine($"Contains {reader.Bundle.Files.Count} files:");

            foreach (var file in reader.Bundle.Files)
            {
                Console.WriteLine($" - {file.RelativePath} ({HumanizeFileSize(file.Size)}, {file.Size} bytes)");
            }
        }
        else
        {
            await reader.ExtractToDirectoryAsync(outputDirectory, cancellationToken);
            Console.WriteLine($"Extracted {reader.Bundle.Files.Count} files to \"{outputDirectory}\"");
        }
    }
    catch (Exception e) when (e is FileNotFoundException or UnsupportedExecutableException)
    {
        Console.WriteLine($"Could not extract: {e.Message}");
    }
    catch (Exception e)
    {
        Console.WriteLine("Unexpected error while extracting:");
        Console.WriteLine(e);
    }
}

static string HumanizeFileSize(long size)
{
    var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
    var order = 0;
    while (size >= 1024 && order < sizes.Length - 1)
    {
        order++;
        size /= 1024;
    }

    return $"{size:0.##} {sizes[order]}";
}