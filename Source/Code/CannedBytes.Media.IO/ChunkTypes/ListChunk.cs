using System.Collections.Generic;
using System.Linq;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes
{
    [Chunk("LIST")]
    public class ListChunk
    {
        public FourCharacterCode ItemType { get; set; }

        [Ignore]
        public IEnumerable<object> InnerChunks { get; set; }

        public IEnumerable<T> GetInnerChunks<T>()
        {
            if (InnerChunks == null) return null;

            return InnerChunks.Cast<T>();
        }
    }
}