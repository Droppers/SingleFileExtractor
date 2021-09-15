using System;
using System.IO;
using SingleFileExtractor.Core.Exceptions;
using Xunit;

namespace SingleFileExtractor.Core.Tests
{
    public class BundleExtractorTests
    {
        private readonly IBundleExtractor _extractor;

        public BundleExtractorTests()
        {
            _extractor = new BundleExtractor();
        }

        [Fact]
        public void Extract_NotBundle()
        {
            Assert.Throws<UnsupportedExecutableException>(() =>
            {
                _extractor.ExtractToDirectory(GetPath("paintdotnet.exe"), "output/paintdotnet");
            });
        }

        [Fact]
        public void Extract_NotDotNetCore()
        {
            Assert.Throws<UnsupportedExecutableException>(() =>
            {
                _extractor.ExtractToDirectory(GetPath("ShareX.exe"), "output/sharex");
            });
        }

        [Fact]
        public void Extract_WithCompression()
        {
            var result = _extractor.ExtractToDirectory(GetPath("Compression.exe"), "output/compression");
            Assert.Equal("Compression.dll", result.StartupInfo.EntryPoint);
        }

        [Fact]
        public void Extract_WithoutCompression()
        {
            var result = _extractor.ExtractToDirectory(GetPath("NoCompression.exe"), "output/no-compression");
            Assert.Equal("Compression.dll", result.StartupInfo.EntryPoint);
        }

        private static string GetPath(string name) => Path.Combine(AppContext.BaseDirectory, "TestFiles", name);
    }
}