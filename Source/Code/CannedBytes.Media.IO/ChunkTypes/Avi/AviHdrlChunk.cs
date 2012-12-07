using System.Collections.Generic;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes.Avi
{
    [Chunk("hdrl")]
    public class AviHdrlChunk
    {
        public AviAvihChunk AviHeader { get; set; }

        public IEnumerable<AviStrlChunk> StreamList { get; set; }
    }
}