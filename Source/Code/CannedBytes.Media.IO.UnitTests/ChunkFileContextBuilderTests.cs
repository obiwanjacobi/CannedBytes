using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    public class ChunkFileContextBuilderTests
    {
        [TestMethod]
        public void Build_Empty()
        {
            var ctx = new ChunkFileContextBuilder()
                .Build();

            ctx.Should().NotBeNull();
        }
    }
}
