namespace CannedBytes.Media.IO
{
    /// <summary>
    /// Stateless object that knows how to process a specific type of file chunk.
    /// </summary>
    public interface IFileChunkHandler
    {
        /// <summary>
        /// The Id of the type of chunk this handler can manage.
        /// </summary>
        FourCharacterCode ChunkId { get; }

        /// <summary>
        /// Called to ensure the handler is ok with reading the chunk.
        /// </summary>
        /// <param name="chunk">File chunk information. Must not be null.</param>
        /// <returns>Returns true when the handler can read the chunk.</returns>
        bool CanRead(FileChunk chunk);

        /// <summary>
        /// Called to read in the complete file chunk, including any of its children.
        /// </summary>
        /// <param name="context">The context the handler can use to retrieve services.</param>
        /// <returns>Returns the runtime chunk type for this file chunk.</returns>
        object Read(ChunkFileContext context);

        bool CanWrite(FileChunk chunk);

        void Write(ChunkFileContext context, object instance);
    }
}