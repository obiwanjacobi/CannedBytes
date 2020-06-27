using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("strn")]
    public class AviStrnChunk
    {
        public byte[] Data { get; set; }
    }
}