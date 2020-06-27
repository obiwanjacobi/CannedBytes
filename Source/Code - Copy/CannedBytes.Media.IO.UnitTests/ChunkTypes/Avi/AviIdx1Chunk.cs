using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("idx1")]
    public class AviIdx1Chunk
    {
        public byte[] Data { get; set; }
    }
}