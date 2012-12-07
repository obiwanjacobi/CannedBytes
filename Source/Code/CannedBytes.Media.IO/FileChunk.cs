using System.IO;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// The FileChunk class represents a single chunk in a (R)IIF file.
    /// </summary>
    public class FileChunk
    {
        /// <summary>
        /// The position in the main file stream where this chunk starts.
        /// </summary>
        public long FilePosition { get; set; }

        /// <summary>
        /// The position in the parent chunk stream where this chunk starts.
        /// </summary>
        public long ParentPosition { get; set; }

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

        /// <summary>
        /// A reference to the runtime type that holds the chunk data.
        /// </summary>
        public object RuntimeInstance { get; set; }

        public bool HasSubChunks
        {
            get { return (this.subChunks != null && this.subChunks.Count > 0); }
        }

        private FileChunkCollection subChunks;

        /// <summary>
        /// Child chunks. Filled after they're parsed when reading.
        /// </summary>
        public FileChunkCollection SubChunks
        {
            get
            {
                if (this.subChunks == null)
                {
                    this.subChunks = new FileChunkCollection();
                }

                return this.subChunks;
            }
        }
    }
}