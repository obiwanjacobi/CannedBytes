namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.Linq;

    /// <summary>
    /// Code attribute placed on a class.
    /// to indicate it is a representation of a chunk.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ChunkAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="chunkId">The identification of the chunk in four characters.</param>
        public ChunkAttribute(string chunkId)
        {
            Check.IfArgumentNullOrEmpty(chunkId, nameof(chunkId));

            this.ChunkId = new FourCharacterCode(chunkId);
        }

        /// <summary>
        /// The identification of the chunk in four characters.
        /// </summary>
        public FourCharacterCode ChunkId { get; private set; }

        /// <summary>
        /// Returns the identification of a chunk declared in a ChunkAttribute on the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Returns null if not found.</returns>
        public static string GetChunkId(Type type)
        {
            Check.IfArgumentNull(type, nameof(type));

            var result = (from attr in type.GetCustomAttributes(typeof(ChunkAttribute), false)
                          where attr != null
                          select ((ChunkAttribute)attr).ChunkId.ToString()).FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Returns the identification of a chunk declared in a ChunkAttribute on the specified <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Must not be null.</param>
        /// <returns>Returns null if not found.</returns>
        public static string GetChunkId(object instance)
        {
            Check.IfArgumentNull(instance, nameof(instance));

            return GetChunkId(instance.GetType());
        }
    }
}