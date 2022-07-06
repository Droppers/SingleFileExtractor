# About
This is a library to programmatically extract the contents (assemblies, symbols, configuration, ...) of a single file application to a directory.

# Install
Install the `SingleFileExtractor.Core` NuGet package for your project.

# Usage

## Extract all files

```cs
BundleExtractor.Extract("Application.exe", "path/to/output");
```

## Extract specific file

```cs
var manifest = new ExecutableReader().Read("Application.exe");
var file = manifest.Files.Single(x => x.RelativePath == "Example.dll");
file.Extract("Example.dll"); // or file.AsStream()
```