<h1 align="center"><img src="https://svgur.com/i/_YV.svg" height="45" valign="text-bottom">&nbsp;&nbsp;RefScout - Reference Analyzer</h1></br>
<p align="center">
  RefScout is a desktop and command-line application that assists you with identifying conflicting assembly references in your .NET Framework and .NET Core applications.<br><br>
  <img src="https://i.imgur.com/SzQCpyY.png" height="500">
</p>

<h6 align="center">
  <img src="https://img.shields.io/badge/License-MIT-yellow.svg" height="24" valign="middle">&nbsp;&nbsp;
  <a href="https://dotnet.microsoft.com/download" alt=".NET target"><img alt=".NET target" src="https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=target&query=%2F%2FTargetFramework%5B1%5D&url=https%3A%2F%2Fraw.githubusercontent.com%2FZacharyPatten%2FTowel%2Fmain%2FSources%2FTowel%2FTowel.csproj&logo=.net" title="Go To .NET Download" height="24" valign="middle"></a>&nbsp;&nbsp;
  <img src= "https://joery.nl/static/vector/logo.svg" height="24" valign="middle">&nbsp;&nbsp;By <a href="https://joery.nl">Joery Droppers</a>
</h6>

# Install
To use this tool install it as a tool using dotnet:
```

dotnet tool install -g SingleFileExtractor
```

# Usage
## Command-line


```
sfextract [file] -o [output-directory]
```

## Programmatically

Install the `SingleFileExtractor.Core` NuGet package to use it programmatically:
```csharp
BundleExtractor.Extract("application.exe", "path/to/output");
```
