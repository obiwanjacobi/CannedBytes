using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.UnitTests.ChunkTypes.Avi
{
    [Chunk("strl")]
    public class AviStrlChunk
    {
        public AviStrhChunk StreamHeader { get; set; }

        public AviStrfChunk StreamFormat { get; set; }

        public AviStrdChunk StreamData { get; set; }

        public AviStrnChunk StreamName { get; set; }
    }
}