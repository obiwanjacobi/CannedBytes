namespace CannedBytes.Media.IO
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Stateless object that knows how to process a specific type of file chunk.
    /// </summary>
    [ContractClass(typeof(FileChunkHandlerContract))]
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
        /// <param name="context">The context the handler can use to retrieve services. Must not be null.</param>
        /// <returns>Returns the runtime chunk type for this file chunk.</returns>
        object Read(ChunkFileContext context);

        /// <summary>
        /// Called to ensure the handler is able to write out the <paramref name="chunk"/>.
        /// </summary>
        /// <param name="instance">The runtime object containing the chunk data.</param>
        /// <returns>Returns true when the handler can write the chunk.</returns>
        bool CanWrite(object instance);

        /// <summary>
        /// Called to write the <paramref name="instance"/> to the chunk file.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        /// <param name="instance">The runtime chunk type that contains the data.</param>
        void Write(ChunkFileContext context, object instance);
    }
}