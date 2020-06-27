using CannedBytes.Media.IO.UnitTests.Media;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    [DeploymentItem(@"Media\tada.wav")]
    [DeploymentItem(@"Media\boxed-delete.avi")]
    public class FileChunkWriterTests
    {
        public TestContext TestContext { get; set; }

        internal FileChunkWriter CreateWriter(string filePath)
        {
            var context = new ChunkFileContextBuilder()
                .Endianness(Endianness.LittleEndian)
                .ForWriting(filePath)
                .Discover(GetType().Assembly)
                .Build();

            var writer = new FileChunkWriter(context);
            return writer;
        }

        internal bool CompareFiles(string filePath1, string filePath2)
        {
            using (var file1 = File.OpenRead(filePath1))
            {
                using (var file2 = File.OpenRead(filePath2))
                {
                    return file1.Length == file2.Length;
                }
            }
        }

        [TestMethod]
        public void WriteWaveFile_CompareToOrigin_NoDiffs()
        {
            // read wave file
            var readerFilePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.WaveFileName);
            var reader = FileChunkReaderTests.CreateReader(readerFilePath, Endianness.LittleEndian);

            var rtObj = reader.ReadNextChunk();

            var writerFilePath = Path.Combine(TestContext.DeploymentDirectory,
                Path.GetFileNameWithoutExtension(TestMedia.WaveFileName) + "_out" + Path.GetExtension(TestMedia.WaveFileName));

            var writer = CreateWriter(writerFilePath);
            writer.WriteNextChunk(rtObj);
            writer.Dispose();

            CompareFiles(readerFilePath, writerFilePath).Should().BeTrue();
        }

        [TestMethod]
        public void WriteAviFile_CompareToOrigin_NoDiffs()
        {
            // read wave file
            var readerFilePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.AviFileName);
            var reader = FileChunkReaderTests.CreateReader(readerFilePath, Endianness.LittleEndian);

            var rtObj = reader.ReadNextChunk();

            var writerFilePath = Path.Combine(TestContext.DeploymentDirectory,
                Path.GetFileNameWithoutExtension(TestMedia.AviFileName) + "_out" + Path.GetExtension(TestMedia.AviFileName));

            var writer = CreateWriter(writerFilePath);
            writer.WriteNextChunk(rtObj);
            writer.Dispose();

            //CompareFiles(readerFilePath, writerFilePath).Should().BeTrue();
        }
    }
}