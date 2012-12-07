using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes.Wave
{
    [Chunk("WAVE")]
    class WaveChunk
    {
        public WaveFormatChunk Format { get; set; }

        public WaveDataChunk Data { get; set; }
    }
}