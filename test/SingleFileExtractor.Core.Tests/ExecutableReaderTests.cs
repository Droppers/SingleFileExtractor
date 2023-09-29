using System;
using System.IO;
using System.Linq;
using SingleFileExtractor.Core.Exceptions;
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
        [InlineData("NoCompression.exe", "Compression.dll")] // Same project but with compression disabled
        [InlineData("RoslynPad.exe", "RoslynPad.dll")]
        [InlineData("DotNetPad.exe", "DotNetPad.dll")]
        [InlineData("LINQPad6.exe", "LINQPad.GUI.dll")]
        public void StartupInfoTests(string executable, string expectedEntryPointPath)
        {
            var startupInfo = new ExecutableReader(GetPath(executable)).StartupInfo;
            Assert.Equal(expectedEntryPointPath, startupInfo.EntryPoint);
        }

        [Fact]
        public void Read_And_ExtractSpecificFile()
        {
            const string fileName = "output/Compression.dll";

            var bundle = new ExecutableReader(GetPath("Compression.exe")).Bundle;
            var file = bundle.Files.Single(x => x.RelativePath == "Compression.dll");
            file.ExtractToFile(fileName);

            Assert.True(File.Exists(fileName));
        }

        [Fact]
        public void Extract_NotBundle()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new ExecutableReader(GetPath("paintdotnet.exe")).ExtractToDirectory("output/paintdotnet");
            });
        }

        [Fact]
        public void Extract_NotDotNetCore()
        {
            Assert.Throws<UnsupportedExecutableException>(() =>
            {
                new ExecutableReader(GetPath("ShareX.exe")).ExtractToDirectory("output/sharex");
            });
        }
        
        [Fact]
        public void Extract_WithCompression()
        {
            var bundle = new ExecutableReader(GetPath("Compression.exe"));
            bundle.ExtractToDirectory("output/compression");
            Assert.Equal("Compression.dll", bundle.StartupInfo.EntryPoint);
        }

        [Fact]
        public void Extract_WithoutCompression()
        {
            var bundle = new ExecutableReader(GetPath("NoCompression.exe"));
            bundle.ExtractToDirectory("output/no-compression");
            Assert.Equal("Compression.dll", bundle.StartupInfo.EntryPoint);
        }

        [Fact]
        public void Extract_Utf8Encoding()
        {
            var bundle = new ExecutableReader(GetPath("FunkyCharacters.exe"));
            bundle.ExtractToDirectory("output/funky-characters");
            Assert.Equal("FunkyCharacters.dll", bundle.StartupInfo.EntryPoint);
            Assert.Equal(1, bundle.Bundle.Files.Count(a => a.RelativePath == "えのさにへひいけくはわもむもてむけものりら.dll"));
        }

        private static string GetPath(string name) => Path.Combine(AppContext.BaseDirectory, "TestFiles", name);
    }
}