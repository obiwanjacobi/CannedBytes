namespace CannedBytes.Media.IO.ChunkTypes
{
    using CannedBytes.Media.IO.SchemaAttributes;

    /// <summary>
    /// A chunk class that represent the 'RIFF' chunk in a RIFF file.
    /// </summary>
    /// <remarks>The RIFF chunk is the root of the file.</remarks>
    [Chunk("RIFF")]
    public class RiffChunk
    {
        /// <summary>
        /// An identification of the type of file.
        /// </summary>
        /// <remarks>CLR Types marked with this code are used for serialization.</remarks>
        public FourCharacterCode FileType { get; set; }

        /// <summary>
        /// All child chunks can be accessed here.
        /// </summary>
        /// <remarks>This member is filled by the <see cref="RiffChunkHandler"/>.</remarks>
        [Ignore]
        public object InnerChunk { get; set; }
    }
}