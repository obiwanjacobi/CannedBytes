using CannedBytes.Media.IO.ChunkTypes.Wave;
using CannedBytes.Media.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests.Services
{
    [TestClass]
    public class ChunkTypeFactoryTests
    {
        private static ChunkTypeFactory CreateFactory()
        {
            var factory = new ChunkTypeFactory();

            factory.AddChunksFrom(typeof(WaveChunk).Assembly, true);

            return factory;
        }

        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_RIFF()
        {
            var factory = CreateFactory();
            var fourcc = new FourCharacterCode("RIFF");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }

        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_WAVE()
        {
            var factory = CreateFactory();
            var fourcc = new FourCharacterCode("WAVE");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }

        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_fmt()
        {
            var factory = CreateFactory();
            var fourcc = new FourCharacterCode("fmt ");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }

        [TestMethod]
        public void CtorPopulates_CreatChunk_RetrievesInternalChunkObjects_data()
        {
            var factory = CreateFactory();
            var fourcc = new FourCharacterCode("data");

            var chunk = factory.CreateChunkObject(fourcc);

            Assert.IsNotNull(chunk);
        }
    }
}