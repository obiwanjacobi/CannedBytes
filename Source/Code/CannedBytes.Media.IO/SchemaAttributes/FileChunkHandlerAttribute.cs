namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.Linq;

    /// <summary>
    /// A code attribute that indicates to the framework that the class is a chunk handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FileChunkHandlerAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new instance for the specified <paramref name="chunkId"/>.
        /// </summary>
        /// <param name="chunkId">Must be 4 characters long. Must not be null or empty.</param>
        public FileChunkHandlerAttribute(string chunkId)
        //            : base(typeof(IFileChunkHandler))
        {
            Check.IfArgumentNullOrEmpty(chunkId, nameof(chunkId));
            Check.IfArgumentOutOfRange(chunkId.Length, 4, 4, nameof(chunkId.Length));

            this.ChunkId = chunkId;
        }

        /// <summary>
        /// Gets the chunk id (four character code).
        /// </summary>
        public string ChunkId { get; private set; }

        /// <summary>
        /// Returns the identification of a chunk declared in a FileChunkHandlerAttribute on the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Returns null if not found.</returns>
        public static string GetChunkId(Type type)
        {
            Check.IfArgumentNull(type, nameof(type));

            var result = (from attr in type.GetCustomAttributes(typeof(FileChunkHandlerAttribute), false)
                          where attr != null
                          select ((FileChunkHandlerAttribute)attr).ChunkId.ToString()).FirstOrDefault();

            return result;
        }
    }
}