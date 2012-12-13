namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Code attribute placed on a class.
    /// to indicate it is a representation of a chunk.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Got that and it still complains.")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ChunkAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="chunkTypeId">The identification of the chunk in four characters.</param>
        public ChunkAttribute(string chunkTypeId)
        {
            Contract.Requires(!String.IsNullOrEmpty(chunkTypeId));
            Check.IfArgumentNullOrEmpty(chunkTypeId, "chunkTypeId");

            this.ChunkTypeId = new FourCharacterCode(chunkTypeId);
        }

        /// <summary>
        /// The identification of the chunk in four characters.
        /// </summary>
        public FourCharacterCode ChunkTypeId { get; private set; }

        /// <summary>
        /// Returns the identification of a chunk declared in a ChunkAttribute on the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Returns null if not found.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public static string GetChunkId(Type type)
        {
            Contract.Requires(type != null);
            Check.IfArgumentNull(type, "type");

            var result = (from attr in type.GetCustomAttributes(typeof(ChunkAttribute), false)
                          where attr != null
                          select ((ChunkAttribute)attr).ChunkTypeId.ToString()).FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Returns the identification of a chunk declared in a ChunkAttribute on the specified <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Must not be null.</param>
        /// <returns>Returns null if not found.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public static string GetChunkId(object instance)
        {
            Contract.Requires(instance != null);
            Check.IfArgumentNull(instance, "instance");

            return GetChunkId(instance.GetType());
        }
    }
}