using CannedBytes.Media.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests.Services
{
    [TestClass]
    public class ChunkTypeFactoryTests
    {
        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_RIFF()
        {
            var factory = new ChunkTypeFactory();
            var fourcc = new FourCharacterCode("RIFF");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }

        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_WAVE()
        {
            var factory = new ChunkTypeFactory();
            var fourcc = new FourCharacterCode("WAVE");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }

        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_fmt()
        {
            var factory = new ChunkTypeFactory();
            var fourcc = new FourCharacterCode("fmt ");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }

        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_data()
        {
            var factory = new ChunkTypeFactory();
            var fourcc = new FourCharacterCode("data");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }
    }
}