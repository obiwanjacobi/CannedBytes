using CannedBytes.Media.IO.Services;
using CannedBytes.Media.IO.UnitTests.Media;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    [DeploymentItem(@"Media\tada.wav")]
    public class ChunkFileContextBuilderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Build_ReturnsContext()
        {
            var ctx = new ChunkFileContextBuilder()
                .Build();

            ctx.Should().NotBeNull();
            ctx.Services.Should().NotBeNull();

            // we did not call ForReading/Writing
            ctx.ChunkFile.Should().BeNull();
        }


        [TestMethod]
        public void Build_HasDefaultServices()
        {
            var ctx = new ChunkFileContextBuilder()
                .Build();

            // default services
            ctx.Services.GetService<FileChunkHandlerManager>().Should().NotBeNull();
            ctx.Services.GetService<IChunkTypeFactory>().Should().NotBeNull();
            ctx.Services.GetService<INumberReader>().Should().NotBeNull();
            ctx.Services.GetService<INumberWriter>().Should().NotBeNull();
            ctx.Services.GetService<IStringReader>().Should().NotBeNull();
            ctx.Services.GetService<IStringWriter>().Should().NotBeNull();
        }

        [TestMethod]
        public void Build_HasFile()
        {
            var filePath = Path.Combine(TestContext.DeploymentDirectory, TestMedia.WaveFileName);

            var ctx = new ChunkFileContextBuilder()
                .ForReading(filePath)
                .Build();

            ctx.ChunkFile.Should().NotBeNull();
            ctx.ChunkFile.FilePath.Should().NotBeNullOrEmpty();
            ctx.ChunkFile.BaseStream.Should().NotBeNull();
            ctx.ChunkFile.FileAccess.Should().Be(FileAccess.Read);
        }
    }
}
