rmdir "./.nupkg" /s /q
dotnet pack ./src/SingleFileExtractor.CLI/SingleFileExtractor.CLI.csproj --configuration Release --output ./.nupkg
dotnet pack ./src/SingleFileExtractor.Core/SingleFileExtractor.Core.csproj --configuration Release --output ./.nupkg