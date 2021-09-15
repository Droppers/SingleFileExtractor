using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using SingleFileExtractor.Core;
using SingleFileExtractor.Core.Exceptions;

var app = new CommandLineApplication();
app.HelpOption();

var fileOption = app.Argument("executable", "The single file executable to be extracted").IsRequired();
var outputOption = app.Option("-o|--output <DIRECTORY>", "The directory to write the extracted files to.",
    CommandOptionType.SingleValue).IsRequired();

app.OnExecute(() =>
{
    if (fileOption.Value != null && outputOption.Value() != null)
    {
        RunExtractor(fileOption.Value, outputOption.Value()!);
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

return app.Execute(args);

static void RunExtractor(string fileName, string outputDirectory)
{
    try
    {
        var result = BundleExtractor.Extract(fileName, outputDirectory);
        if (result.StartupInfo.EntryPoint != null)
        {
            Console.WriteLine($"Entry point: {result.StartupInfo.EntryPoint}");
        }

        Console.WriteLine($"Bundle version: {result.MajorVersion}.{result.MinorVersion}");
        Console.WriteLine($"Extracted {result.Files.Count} files to \"{outputDirectory}\"");
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