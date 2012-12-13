namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public FileChunkHandlerAttribute(string chunkId)
            : base(typeof(IFileChunkHandler))
        {
            Contract.Requires(!String.IsNullOrEmpty(chunkId));
            Contract.Requires(chunkId.Length == 4);
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