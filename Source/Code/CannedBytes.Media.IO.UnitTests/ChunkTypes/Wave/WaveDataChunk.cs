using System.IO;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes.Wave
{
    [Chunk("data")]
    class WaveDataChunk
    {
        public Stream DataStream { get; set; }
    }
}