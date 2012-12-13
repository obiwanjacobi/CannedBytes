using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes.Wave
{
    [Chunk("fmt ")]
    class WaveFormatChunk
    {
        public short AudioFormat { get; set; }

        public short NumberOfChannels { get; set; }

        public int SampleRate { get; set; }

        public int ByteRate { get; set; }

        public short BlockAllign { get; set; }

        public short BitsPerSample { get; set; }
    }
}