namespace CannedBytes.Media.IO
{
    using System.IO;

    /// <summary>
    /// Basic file chunk information
    /// </summary>
    public class FileChunkHeader
    {
        /// <summary>
        /// The four character code identifying the file chunk.
        /// </summary>
        public FourCharacterCode ChunkId { get; set; }

        /// <summary>
        /// The length in bytes of the file chunk data.
        /// </summary>
        public long DataLength { get; set; }

        /// <summary>
        /// A (sub) stream for the current chunk (only).
        /// </summary>
        public Stream DataStream { get; set; }
    }
}