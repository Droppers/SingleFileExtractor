using JetBrains.Annotations;

namespace SingleFileExtractor.Core;

[PublicAPI]
public enum FileType : byte
{
    Unknown, // Type not determined.
    Assembly, // IL and R2R Assemblies
    NativeBinary, // Native binaries
    DepsJson, // deps.json configuration file
    RuntimeConfigJson, // runtimeconfig.json configuration file
    Symbols // Symbol (PDB) files
}