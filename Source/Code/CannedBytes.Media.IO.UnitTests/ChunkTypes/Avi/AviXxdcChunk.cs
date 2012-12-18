using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("##dc")]
    public class AviXxdcChunk : AviDataChunkBase
    {
        public byte[] Data { get; set; }
    }
}