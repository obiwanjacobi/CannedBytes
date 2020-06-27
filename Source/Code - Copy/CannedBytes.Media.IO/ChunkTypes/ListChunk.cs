namespace CannedBytes.Media.IO.ChunkTypes
{
    using System.Collections.Generic;
    using CannedBytes.Media.IO.SchemaAttributes;

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
    }
}