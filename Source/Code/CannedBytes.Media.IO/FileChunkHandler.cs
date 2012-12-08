using System;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// abstract base implementation for a chunk handler class.
    /// </summary>
    /// <remarks>All chunk handler implementation classes must have a <see cref="FileChunkHandlerAttribute"/>.</remarks>
    public abstract class FileChunkHandler : IFileChunkHandler
    {
        /// <summary>
        /// Retrieves the <see cref="FileChunkHandlerAttribute"/> and initializes the <see cref="P:ChunkId"/> property.
        /// </summary>
        protected FileChunkHandler()
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(FileChunkHandlerAttribute), true);

            if (attrs == null || attrs.Length == 0)
            {
                throw new InvalidOperationException(
                    "A FileChunkHandler derived class must be marked with the FileChunkHandlerExportAttribute.");
            }

            ChunkId = new FourCharacterCode(((FileChunkHandlerAttribute)attrs[0]).ChunkId);
        }

        /// <summary>
        /// The chunk id this handler can manage. Can contain wildcards.
        /// </summary>
        public FourCharacterCode ChunkId { get; private set; }

        /// <summary>
        /// Returns an indication if the <paramref name="chunk"/> can be read by the handler.
        /// </summary>
        /// <param name="chunk">File chunk info. Must not be null.</param>
        /// <returns>Returns true if the chunk can be read by this handler.</returns>
        public virtual bool CanRead(FileChunk chunk)
        {
            return (chunk.ChunkId.ToString() == ChunkId.ToString() &&
                chunk.DataStream != null && chunk.DataStream.CanRead);
        }

        /// <inheritdocs/>
        public abstract object Read(ChunkFileContext context);

        /// <summary>
        /// Returns an indication if the <paramref name="chunk"/> can be written by the handler.
        /// </summary>
        /// <param name="chunk">File chunk info. Must not be null.</param>
        /// <returns>Returns true if the chunk can be written by this handler.</returns>
        public virtual bool CanWrite(FileChunk chunk)
        {
            return (chunk.ChunkId.ToString() == ChunkId.ToString() &&
                chunk.DataStream != null && chunk.DataStream.CanWrite);
        }

        /// <inheritdocs/>
        public abstract void Write(ChunkFileContext context, object instance);
    }
}