using System.Collections.Generic;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes.Avi
{
    [Chunk("rec ")]
    public class AviRecChunk
    {
        // these are expected to be read all in one go.
        public IEnumerable<AviDataChunkBase> DataList { get; set; }
    }
}