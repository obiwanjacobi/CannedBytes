namespace CannedBytes.Media.IO
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Declaring contracts for the <see cref="IFileChunkHandler"/> interface.
    /// </summary>
    [ContractClassFor(typeof(IFileChunkHandler))]
    internal abstract class FileChunkHandlerContract : IFileChunkHandler
    {
        /// <summary>
        /// Ensures the returned value is not null.
        /// </summary>
        FourCharacterCode IFileChunkHandler.ChunkId
        {
            get
            {
                Contract.Ensures(Contract.Result<FourCharacterCode>() != null);

                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Contract specification.
        /// </summary>
        /// <param name="chunk">Must not be null.</param>
        /// <returns>No Contract.</returns>
        bool IFileChunkHandler.CanRead(FileChunk chunk)
        {
            Contract.Requires(chunk != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract specification.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        /// <returns>No contract.</returns>
        object IFileChunkHandler.Read(ChunkFileContext context)
        {
            Contract.Requires(context != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract specification.
        /// </summary>
        /// <param name="chunk">Must not be null.</param>
        /// <returns>No contract.</returns>
        bool IFileChunkHandler.CanWrite(FileChunk chunk)
        {
            Contract.Requires(chunk != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract specification.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        /// <param name="instance">Must not be null.</param>
        void IFileChunkHandler.Write(ChunkFileContext context, object instance)
        {
            Contract.Requires(context != null);
            Contract.Requires(instance != null);

            throw new System.NotImplementedException();
        }
    }
}