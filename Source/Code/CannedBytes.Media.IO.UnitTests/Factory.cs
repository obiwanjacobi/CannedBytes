﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CannedBytes.Media.IO.UnitTests
{
    static class Factory
    {
        public static ChunkFileContext CreateFileContextForReading(string filePath, bool littleEndian)
        {
            Assert.IsNotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));

            var context = new ChunkFileContext
            {
                ChunkFile = ChunkFileInfo.OpenRead(filePath)
            };

            Assert.IsNotNull(context.ChunkFile);
            Assert.IsNotNull(context.ChunkFile.BaseStream);

            //context.CompositionContainer = CreateCompositionContext(littleEndian);
            //Assert.IsNotNull(context.CompositionContainer);

            return context;
        }

        public static ChunkFileContext CreateFileContextForWriting(string filePath, bool littleEndian)
        {
            Assert.IsNotNull(filePath);
            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(filePath)));

            var context = new ChunkFileContext
            {
                ChunkFile = ChunkFileInfo.OpenWrite(filePath)
            };

            Assert.IsNotNull(context.ChunkFile);
            Assert.IsNotNull(context.ChunkFile.BaseStream);

            //context.CompositionContainer = CreateCompositionContext(littleEndian);
            //Assert.IsNotNull(context.CompositionContainer);

            return context;
        }

        //public static CompositionContainer CreateCompositionContext(bool littleEndian)
        //{
        //    var factory = new CompositionContainerFactory();

        //    factory.AddMarkedTypesInAssembly(null, typeof(IFileChunkHandler));

        //    factory.AddTypes(
        //        littleEndian ? typeof(LittleEndianNumberReader) : typeof(BigEndianNumberReader),
        //        littleEndian ? typeof(LittleEndianNumberWriter) : typeof(BigEndianNumberWriter),
        //        typeof(SizePrefixedStringReader),
        //        typeof(SizePrefixedStringWriter),
        //        typeof(StreamNavigator),
        //        typeof(ChunkTypeFactory),
        //        typeof(FileChunkHandlerManager));

        //    var container = factory.CreateNew();

        //    var chunkFactory = container.GetService<ChunkTypeFactory>();
        //    // add test chunks
        //    chunkFactory.AddChunksFrom(typeof(WaveChunk).Assembly, AddMode.OverwriteExisting);

        //    return container;
        //}
    }
}