namespace SingleFileExtractor.Core.Legacy;

public interface IBundleExtractor
{
    Manifest ExtractToDirectory(string fileName, string outputDirectory);
}