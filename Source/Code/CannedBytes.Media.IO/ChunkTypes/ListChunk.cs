using System.Collections.Generic;
using System.Linq;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes
{
    /// <summary>
    /// A chunk class that represent the 'LIST' chunk in a RIFF file.
    /// </summary>
    [Chunk("LIST")]
    public class ListChunk
    {
        /// <summary>
        /// An identification of the type of items in the list.
        /// </summary>
        /// <remarks>CLR Types marked with this code are used for serialization.</remarks>
        public FourCharacterCode ItemType { get; set; }

        /// <summary>
        /// All child chunks can be accessed here.
        /// </summary>
        /// <remarks>This member is filled by the <see cref="ListChunkHandler"/>.</remarks>
        [Ignore]
        public IEnumerable<object> InnerChunks { get; set; }

        /// <summary>
        /// Retrieves the child chunks typed to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The class of the item chunks.</typeparam>
        /// <returns>Can return null.</returns>
        public IEnumerable<T> GetInnerChunks<T>() where T : class
        {
            if (InnerChunks == null) return null;

            return InnerChunks.Cast<T>();
        }
    }
}