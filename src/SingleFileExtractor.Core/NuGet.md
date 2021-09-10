# About
This is a library to programmatically extract the contents (assemblies, symbols, configuration, ...) of a single file application to a directory.

# Install
Install the `SingleFileExtractor.Core` NuGet package for your project.

# Usage
```csharp
BundleExtractor.Extract("Application.exe", "path/to/output");
```