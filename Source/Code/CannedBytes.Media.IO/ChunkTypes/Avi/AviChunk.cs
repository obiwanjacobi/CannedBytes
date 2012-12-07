using System.Collections.Generic;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes.Avi
{
    [Chunk("AVI ")]
    public class AviChunk
    {
        public IEnumerable<AviHdrlChunk> HeaderList { get; set; }

        public IEnumerable<AviMoviChunk> MovieData { get; set; }

        public AviIdx1Chunk Index { get; set; }
    }
}