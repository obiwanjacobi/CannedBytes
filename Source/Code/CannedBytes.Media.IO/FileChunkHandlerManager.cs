namespace CannedBytes.Media.IO
{
    using CannedBytes.Media.IO.SchemaAttributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Manages the available <see cref="IFileChunkHandler"/> implementations.
    /// </summary>
    /// <remarks>Uses composition to retrieve the chunk handler implementations.
    /// Each chunk handler must be marked with the <see cref="FileChunkHandlerAttribute"/>.</remarks>
    public class FileChunkHandlerManager
    {
        private readonly List<KeyValuePair<string, IFileChunkHandler>> _chunkHandlers;

        public FileChunkHandlerManager(IEnumerable<IFileChunkHandler> handlers)
        {
#if NET4
            _chunkHandlers = handlers
                .Select(h => new KeyValuePair<string, IFileChunkHandler>(FileChunkHandlerAttribute.GetChunkId(h.GetType()), h))
                .ToList();
#else
            _chunkHandlers = handlers
                .Select(h => KeyValuePair.Create(FileChunkHandlerAttribute.GetChunkId(h.GetType()), h))
                .ToList();
#endif
        }

        /// <summary>
        /// The open wildcard for the default handler.
        /// </summary>
        public const string DefaultHandlerChunkId = "****";

        /// <summary>
        /// Retrieves a handler for the specified <paramref name="chunkId"/>.
        /// </summary>
        /// <param name="chunkId">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        /// <remarks>If no specific chunk handler could be found, the default handler is returned.</remarks>
        public IFileChunkHandler GetChunkHandler(FourCharacterCode chunkId)
        {
            Check.IfArgumentNull(chunkId, nameof(chunkId));
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
            Check.IfArgumentNullOrEmpty(chunkId, nameof(chunkId));
            if (chunkId.Length != 4)
            {
                throw new ArgumentException("A Chunk Id must be 4 characters exactly.", nameof(chunkId));
            }

            var handler = (from pair in _chunkHandlers
                           where pair.Key != DefaultHandlerChunkId
                           where chunkId.MatchesWith(pair.Key)
                           select pair.Value).FirstOrDefault();

            if (handler == null)
            {
                // retrieve default handler
                handler = (from pair in _chunkHandlers
                           where pair.Key == DefaultHandlerChunkId
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