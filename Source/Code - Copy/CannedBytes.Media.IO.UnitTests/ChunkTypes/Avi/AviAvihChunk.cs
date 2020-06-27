using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("avih")]
    public class AviAvihChunk
    {
        public byte[] Data { get; set; }
    }
}