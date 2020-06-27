namespace CannedBytes.Media.IO
{

    using System.IO;

    /// <summary>
    /// The FileChunk class represents a single chunk in a (R)IIF file.
    /// </summary>
    /// <remarks>This info is independent of any runtime chunk types that might be registered.
    /// This allows for reconstructions of the chunk file even when some chunks weren't recognized by the application.</remarks>
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

        /// <summary>
        /// Gets an indication if this chunk has any children.
        /// </summary>
        public bool HasSubChunks
        {
            get { return _subChunks != null && _subChunks.Count > 0; }
        }

        /// <summary>
        /// Backing field for <see cref="P:Subchunks"/>.
        /// </summary>
        private FileChunkCollection _subChunks;

        /// <summary>
        /// Child chunks. Filled after they're parsed when reading.
        /// </summary>
        public FileChunkCollection SubChunks
        {
            get
            {
                if (_subChunks == null)
                {
                    _subChunks = new FileChunkCollection();
                }

                return _subChunks;
            }
        }
    }
}