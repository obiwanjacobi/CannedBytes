using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("strd")]
    public class AviStrdChunk
    {
        public byte[] Data { get; set; }
    }
}