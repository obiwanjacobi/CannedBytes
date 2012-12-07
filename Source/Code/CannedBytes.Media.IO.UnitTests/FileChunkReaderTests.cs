using System.IO;
using CannedBytes.Media.IO.UnitTests.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    [DeploymentItem(@"Media\tada.wav")]
    [DeploymentItem(@"Media\boxed-delete.avi")]
    [DeploymentItem(@"Media\town.mid")]
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

        //[TestMethod]
        //public void ReadChunk_ReadsToEnd_MidFile()
        //{
        //    var filePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.MidFileName);
        //    // note that Midi files use big endian.
        //    var reader = CreateReader(filePath, false);

        //    // because there is no root chunk, we need to call ReadNextChunk multiple times.
        //    var midiHdr = reader.ReadNextChunk() as MThdChunk;
        //    Assert.IsNotNull(midiHdr);

        //    var tracks = new List<MTrkChunk>();

        //    for (int i = 0; i < midiHdr.NumberOfTracks; i++)
        //    {
        //        var track = reader.ReadNextChunk() as MTrkChunk;
        //        Assert.IsNotNull(track);

        //        tracks.Add(track);
        //    }
        //}
    }
}