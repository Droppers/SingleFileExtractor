using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SingleFileExtractor.Core.Tests
{
    public class ExecutableReaderTests
    {
        [Theory]
        [InlineData("paintdotnet.exe", "paintdotnet.dll")]
        [InlineData("dnSpy.exe", @"bin\dnSpy.dll")]
        [InlineData("dnSpy.Console.exe", @"bin\dnSpy.Console.dll")]
        [InlineData("Console31.exe", "Console31.dll")]
        [InlineData("Compression.exe", "Compression.dll")]
        [InlineData("NoCompression.exe", "Compression.dll")] // Same project but with compression disabled oops
        [InlineData("RoslynPad.exe", "RoslynPad.dll")]
        [InlineData("DotNetPad.exe", "DotNetPad.dll")]
        [InlineData("LINQPad6.exe", "LINQPad.GUI.dll")]
        public void StartupInfoTests(string executable, string expectedEntryPointPath)
        {
            var reader = new ExecutableReader();
            var startupInfo = reader.ReadStartupInfo(GetPath(executable));

            Assert.Equal(expectedEntryPointPath, startupInfo.EntryPoint);
        }

        [Fact]
        public void Read_And_ExtractSpecificFile()
        {
            const string fileName = "output/Compression.dll";

            var manifest = new ExecutableReader().ReadManifest(GetPath("Compression.exe"));
            var file = manifest.Files.Single(x => x.RelativePath == "Compression.dll");
            file.Extract(fileName);

            Assert.True(File.Exists(fileName));
        }


        private static string GetPath(string name) => Path.Combine(AppContext.BaseDirectory, "TestFiles", name);
    }
}