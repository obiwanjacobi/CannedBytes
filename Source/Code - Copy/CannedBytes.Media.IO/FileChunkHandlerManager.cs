namespace CannedBytes.Media.IO
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using CannedBytes.Media.IO.SchemaAttributes;

    /// <summary>
    /// Manages the available <see cref="IFileChunkHandler"/> implementations.
    /// </summary>
    /// <remarks>Uses composition to retrieve the chunk handler implementations.
    /// Each chunk handler must be marked with the <see cref="FileChunkHandlerAttribute"/>.</remarks>
    [Export]
    public class FileChunkHandlerManager
    {
        /// <summary>
        /// The open wildcard for the default handler.
        /// </summary>
        public const string DefaultHandlerChunkId = "****";

        ////warning CS0649: Field 'X' is never assigned to, and will always have its default value null
#pragma warning disable 0649
        /// <summary>
        /// The list of chunk handlers.
        /// </summary>
        [ImportMany(AllowRecomposition = true)]
        private Lazy<IFileChunkHandler, IFileChunkHandlerMetaInfo>[] chunkHandlers;
#pragma warning restore 0649

        /// <summary>
        /// Retrieves a handler for the specified <paramref name="chunkId"/>.
        /// </summary>
        /// <param name="chunkId">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        /// <remarks>If no specific chunk handler could be found, the default handler is returned.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public IFileChunkHandler GetChunkHandler(FourCharacterCode chunkId)
        {
            Contract.Requires(chunkId != null);
            Contract.Ensures(Contract.Result<IFileChunkHandler>() != null);
            Check.IfArgumentNull(chunkId, "chunkId");

            return GetChunkHandler(chunkId.ToString());
        }

        /// <summary>
        /// Retrieves a handler for the specified <paramref name="chunkId"/>.
        /// </summary>
        /// <param name="chunkId">Must not be null or empty.</param>
        /// <returns>Never returns null.</returns>
        /// <remarks>If no specific chunk handler could be found, the default handler is returned.</remarks>
        public IFileChunkHandler GetChunkHandler(string chunkId)
        {
            Contract.Requires(!String.IsNullOrEmpty(chunkId));
            Contract.Ensures(Contract.Result<IFileChunkHandler>() != null);
            Check.IfArgumentNullOrEmpty(chunkId, "chunkId");

            var handler = (from pair in this.chunkHandlers
                           where pair.Metadata.ChunkId.ToString() != DefaultHandlerChunkId
                           where chunkId.MatchesWith(pair.Metadata.ChunkId.ToString())
                           select pair.Value).FirstOrDefault();

            if (handler == null)
            {
                // retrieve default handler
                handler = (from pair in this.chunkHandlers
                           where pair.Metadata.ChunkId.ToString() == DefaultHandlerChunkId
                           select pair.Value).FirstOrDefault();
            }

            if (handler == null)
            {
                throw new InvalidOperationException(
                    "No default chunk handler was found ('****').");
            }

            return handler;
        }
    }
}