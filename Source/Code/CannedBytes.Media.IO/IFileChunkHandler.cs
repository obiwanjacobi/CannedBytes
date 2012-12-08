using System.Diagnostics.Contracts;

namespace CannedBytes.Media.IO
{
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
        /// <param name="chunk"></param>
        /// <returns></returns>
        bool CanWrite(FileChunk chunk);

        /// <summary>
        /// Called to write the <paramref name="instance"/> to the chunk file.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        /// <param name="instance">The runtime chunk type that contains the data.</param>
        void Write(ChunkFileContext context, object instance);
    }

    /// <summary>
    /// Declaring contracts for the <see cref="IFileChunkHandler"/> interface.
    /// </summary>
    [ContractClassFor(typeof(IFileChunkHandler))]
    internal abstract class FileChunkHandlerContract : IFileChunkHandler
    {
        private FileChunkHandlerContract()
        { }

        FourCharacterCode IFileChunkHandler.ChunkId
        {
            get
            {
                Contract.Ensures(Contract.Result<FourCharacterCode>() != null);

                throw new System.NotImplementedException();
            }
        }

        bool IFileChunkHandler.CanRead(FileChunk chunk)
        {
            Contract.Requires(chunk != null);

            throw new System.NotImplementedException();
        }

        object IFileChunkHandler.Read(ChunkFileContext context)
        {
            Contract.Requires(context != null);

            throw new System.NotImplementedException();
        }

        bool IFileChunkHandler.CanWrite(FileChunk chunk)
        {
            Contract.Requires(chunk != null);

            throw new System.NotImplementedException();
        }

        void IFileChunkHandler.Write(ChunkFileContext context, object instance)
        {
            Contract.Requires(context != null);
            Contract.Requires(instance != null);

            throw new System.NotImplementedException();
        }
    }
}