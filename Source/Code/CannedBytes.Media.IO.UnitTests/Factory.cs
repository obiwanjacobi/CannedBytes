using System.ComponentModel.Composition.Hosting;
using System.IO;
using CannedBytes.ComponentModel.Composition;
using CannedBytes.Media.IO.ChunkTypes.Wave;
using CannedBytes.Media.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests
{
    static class Factory
    {
        public static ChunkFileContext CreateFileContextForReading(string filePath, bool littleEndian)
        {
            Assert.IsNotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));

            var context = ChunkFileContext.OpenFrom(filePath);

            Assert.IsNotNull(context);

            context.CompositionContainer = CreateCompositionContextForReading(littleEndian);

            Assert.IsNotNull(context.ChunkFile);
            Assert.IsNotNull(context.ChunkFile.BaseStream);
            Assert.IsNotNull(context.CompositionContainer);

            return context;
        }

        public static CompositionContainer CreateCompositionContextForReading(bool littleEndian)
        {
            var factory = new CompositionContainerFactory();

            factory.AddMarkedTypesInAssembly(null, typeof(IFileChunkHandler));

            factory.AddTypes(
                littleEndian ? typeof(LittleEndianNumberReader) : typeof(BigEndianNumberReader),
                typeof(SizePrefixedStringReader),
                typeof(StreamNavigator),
                typeof(ChunkTypeFactory),
                typeof(FileChunkHandlerManager));

            var container = factory.CreateNew();

            var chunkFactory = container.GetService<ChunkTypeFactory>();
            // add test chunks
            chunkFactory.AddChunksFrom(typeof(WaveChunk).Assembly, true);

            return container;
        }
    }
}