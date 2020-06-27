using CannedBytes.Media.IO.UnitTests.Media;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    [DeploymentItem(@"Media\tada.wav")]
    [DeploymentItem(@"Media\boxed-delete.avi")]
    public class FileChunkReaderTests
    {
        public TestContext TestContext { get; set; }

        internal static FileChunkReader CreateReader(string filePath, Endianness endianness)
        {
            var context = new ChunkFileContextBuilder()
                .Endianness(endianness)
                .ForReading(filePath)
                .Discover(typeof(FileChunkReaderTests).Assembly)
                .Build();

            var reader = new FileChunkReader(context);
            return reader;
        }

        [TestMethod]
        public void ReadChunk_ReadsToEnd_WaveFile()
        {
            var filePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.WaveFileName);
            var reader = CreateReader(filePath, Endianness.LittleEndian);

            var rtObj = reader.ReadNextChunk();
            rtObj.Should().NotBeNull();
        }

        [TestMethod]
        public void ReadChunk_ReadsToEnd_AviFile()
        {
            var filePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.AviFileName);
            var reader = CreateReader(filePath, Endianness.LittleEndian);

            var rtObj = reader.ReadNextChunk();
            rtObj.Should().NotBeNull();
        }
    }
}