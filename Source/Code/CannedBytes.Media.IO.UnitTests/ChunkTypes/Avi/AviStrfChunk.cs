using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("strf")]
    public class AviStrfChunk
    {
        public byte[] Data { get; set; }
    }
}