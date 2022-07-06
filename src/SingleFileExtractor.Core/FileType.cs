using JetBrains.Annotations;

namespace SingleFileExtractor.Core
{
    [PublicAPI]
    public enum FileType : byte
    {
        Unknown, // Type not determined.
        Assembly, // IL and R2R Assemblies
        NativeBinary, // NativeBinaries
        DepsJson, // .deps.json configuration file
        RuntimeConfigJson, // .runtimeconfig.json configuration file
        Symbols // PDB Files
    }
}