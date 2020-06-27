using System.IO;
using CannedBytes.Media.IO.UnitTests.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    [DeploymentItem(@"Media\tada.wav")]
    [DeploymentItem(@"Media\boxed-delete.avi")]
    public class FileChunkReaderTests
    {
        public TestContext TestContext { get; set; }

        internal static FileChunkReader CreateReader(string filePath, bool littleEndian)
        {
            var context = Factory.CreateFileContextForReading(filePath, littleEndian);
            Assert.IsNotNull(context);

            var reader = context.CompositionContainer.CreateFileChunkReader();

            Assert.IsNotNull(reader);

            return reader;
        }

        [TestMethod]
        public void ReadChunk_ReadsToEnd_WaveFile()
        {
            var filePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.WaveFileName);
            var reader = CreateReader(filePath, true);

            var rtObj = reader.ReadNextChunk();

            Assert.IsNotNull(rtObj);
        }

        [TestMethod]
        public void ReadChunk_ReadsToEnd_AviFile()
        {
            var filePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.AviFileName);
            var reader = CreateReader(filePath, true);

            var rtObj = reader.ReadNextChunk();

            Assert.IsNotNull(rtObj);
        }
    }
}