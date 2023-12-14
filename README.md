<h1 align="center">📁 SingleFileExtractor</h1></br>
<p align="center">
  A tool for extracting contents (assemblies, configuration, etc.) from a single-file application to a directory, suitable for purposes like malware analysis.
</p>

<h6 align="center">
  <img src="https://img.shields.io/badge/License-MIT-yellow.svg" height="20" valign="middle">&nbsp;&nbsp;
  <a href="https://dotnet.microsoft.com/download" alt=".NET target"><img alt=".NET target" src="https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=target&query=%2F%2FTargetFramework%5B1%5D&url=https%3A%2F%2Fraw.githubusercontent.com%2FZacharyPatten%2FTowel%2Fmain%2FSources%2FTowel%2FTowel.csproj&logo=.net" title="Go To .NET Download" height="20" valign="middle"></a>&nbsp;&nbsp;
  <img src= "https://joery.nl/static/vector/logo.svg" height="20" valign="middle">&nbsp;&nbsp;By <a href="https://joery.nl">Joery Droppers</a>
</h6>

# Install
To use this tool install it as a global dotnet tool:
```
dotnet tool install -g sfextract
```

# Usage
## Command-line
```
sfextract [file] -o|--output [directory]
```

<b>Extract files</b><br>
```
sfextract Application.exe -o path/to/output/
```

<b>List files</b><br>
Omitting the output directory will list all files in the single file executable.
```
sfextract Application.exe
```

## Programmatically

Install the `SingleFileExtractor.Core` NuGet package to use it programmatically:
```csharp
var reader = new ExecutableReader("application.exe");
```

### Read startup info

When you want to know what the entry point assembly is, you can read the startup info

```cs
var reader = new ExecutableReader("application.exe");
var startupInfo = reader.StartupInfo;
```

### Extract all files

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

### Extract specific entry to file

```cs
var reader = new ExecutableReader("application.exe");
await reader.Manifest.Entries[0].ExtractToFileAsync("example.dll");
```

### Extract specific entry to stream

```cs
var reader = new ExecutableReader("application.exe");
var stream = await reader.Manifest.Entries[0].AsStreamAsync()
```

## Why
Another application I'm working on requires me to extract the contents of a single file. Since I've also seen people asking for a way to do this, I decided to turn it into a dotnet tool and NuGet package.

## Credits
- [GitHub: dotnet/runtime](https://github.com/dotnet/runtime) for inventing single file applications.

## License
```
MIT License

Copyright (c) 2021 Joery Droppers (https://github.com/Droppers)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
