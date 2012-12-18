using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("##wb")]
    public class AviXxwbChunk : AviDataChunkBase
    {
        public byte[] Data { get; set; }
    }
}