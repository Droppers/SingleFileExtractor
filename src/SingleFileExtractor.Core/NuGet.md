# About
This is a library to programmatically extract the contents (assemblies, symbols, configuration, ...) of a single file application to a directory.

# Install
Install the `SingleFileExtractor.Core` NuGet package for your project.

Types from version 1.x are available under the SingleFileExtractor.Core.Legacy namespace.

# Usage

## Read startup info

When you want to know what the entry point assembly is, you can read the startup info

```cs
var reader = new ExecutableReader("application.exe");
var startupInfo = reader.StartupInfo;
```

## Extract all files

```cs
var reader = new ExecutableReader("application.exe");

// Validate if executable is a single file executable, and can be extracted
var isSingleFile = reader.IsSingleFile;

if (isSingleFile)
{
    // Extract specific file entry
    await reader.Manifest.Entries[0].ExtractToFileAsync("example.dll");
    // , or create an in-memory stream of a specifc file entry
    var stream = await reader.Manifest.Entries[0].AsStreamAsync()
    
    // Extract all files to a directory
    await reader.ExtractToDirectoryAsync("path/to/output");
}
```

## Extract specific entry to file

```cs
var reader = new ExecutableReader("application.exe");
await reader.Manifest.Entries[0].ExtractToFileAsync("example.dll");
```

## Extract specific entry to stream

```cs
var reader = new ExecutableReader("application.exe");
var stream = await reader.Manifest.Entries[0].AsStreamAsync()
```