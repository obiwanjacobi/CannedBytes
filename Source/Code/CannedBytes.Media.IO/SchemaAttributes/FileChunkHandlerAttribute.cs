namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    /// A code attribute that indicates to the framework that the class is a chunk handler.
    /// </summary>
    /// <remarks>This is a MEF custom export attribute.</remarks>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FileChunkHandlerAttribute : ExportAttribute, IFileChunkHandlerMetaInfo
    {
        /// <summary>
        /// Constructs a new instance for the specified <paramref name="chunkId"/>.
        /// </summary>
        /// <param name="chunkId">Must be 4 characters long. Must not be null or empty.</param>
        public FileChunkHandlerAttribute(string chunkId)
            : base(typeof(IFileChunkHandler))
        {
            Check.IfArgumentNullOrEmpty(chunkId, "chunkId");
            Check.IfArgumentOutOfRange(chunkId.Length, 4, 4, "chunkId.Length");

            this.ChunkId = chunkId;
        }

        /// <summary>
        /// Gets the chunk id (four character code).
        /// </summary>
        public string ChunkId { get; private set; }
    }
}