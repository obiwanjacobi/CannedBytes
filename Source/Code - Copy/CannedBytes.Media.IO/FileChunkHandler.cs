namespace CannedBytes.Media.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CannedBytes.Media.IO.SchemaAttributes;

    /// <summary>
    /// An abstract base implementation for a chunk handler class.
    /// </summary>
    /// <remarks>All chunk handler implementation classes must have a <see cref="FileChunkHandlerAttribute"/>.</remarks>
    public abstract class FileChunkHandler : IFileChunkHandler
    {
        /// <summary>
        /// Retrieves the <see cref="FileChunkHandlerAttribute"/> and initializes the <see cref="P:ChunkId"/> property.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "FileChunkHandlerExportAttribute", Justification = "We want to indicate the type.")]
        protected FileChunkHandler()
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(FileChunkHandlerAttribute), true);

            if (attrs == null || attrs.Length == 0)
            {
                throw new InvalidOperationException(
                    "A File Chunk Handler derived class must be marked with the FileChunkHandlerExportAttribute.");
            }

            this.ChunkId = new FourCharacterCode(((FileChunkHandlerAttribute)attrs[0]).ChunkId);
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public virtual bool CanRead(FileChunk chunk)
        {
            Check.IfArgumentNull(chunk, "chunk");

            return (chunk.ChunkId.ToString() == this.ChunkId.ToString() &&
                chunk.DataStream != null && chunk.DataStream.CanRead);
        }

        /// <inheritdocs/>
        public abstract object Read(ChunkFileContext context);

        /// <summary>
        /// Returns an indication if the <paramref name="instance"/> can be written by the handler.
        /// </summary>
        /// <param name="instance">The runtime object containing the chunk data.</param>
        /// <returns>Returns true if the chunk can be written by this handler.</returns>
        public virtual bool CanWrite(object instance)
        {
            return instance != null;
        }

        /// <inheritdocs/>
        public abstract void Write(ChunkFileContext context, object instance);
    }
}