using System.Collections.Generic;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes.Avi
{
    [Chunk("movi")]
    public class AviMoviChunk
    {
        public AviRecChunk Record { get; set; }

        public IEnumerable<AviDataChunkBase> DataList { get; set; }
    }
}