# About
This is a tool to extract the contents (assemblies, configuration, ...) of a single file application to a directory.

# Install
To use this tool install it as a global dotnet tool:
```
dotnet tool install -g sfextract
```

# Usage
After installation, the tool can be called by using the `sfextract` command using the following arguments:
```
sfextract [file] -o|--output [output-directory]
```

# Example
```
sfextract MyApplication.exe -o output/
```