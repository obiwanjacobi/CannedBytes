using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("##db")]
    public class AviXxdbChunk : AviDataChunkBase
    {
        public byte[] Data { get; set; }
    }
}