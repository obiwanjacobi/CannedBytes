using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes
{
    [Chunk("RIFF")]
    public class RiffChunk
    {
        public FourCharacterCode FileType { get; set; }

        [Ignore]
        public object InnerChunk { get; set; }

        public T GetInnerChunk<T>()
        {
            return (T)InnerChunk;
        }
    }
}