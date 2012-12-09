using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace CannedBytes.Media.IO.SchemaAttributes
{
    /// <summary>
    /// Code attribute placed on a class.
    /// to indicate it is a representation of a chunk.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ChunkAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        public ChunkAttribute()
        { }

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="fourCharacterCode">The identification of the chunk in four characters.</param>
        public ChunkAttribute(string fourCharacterCode)
        {
            Contract.Requires(!String.IsNullOrEmpty(fourCharacterCode));
            Throw.IfArgumentNullOrEmpty(fourCharacterCode, "fourCharacterCode");

            ChunkTypeId = new FourCharacterCode(fourCharacterCode);
        }

        /// <summary>
        /// The identification of the chunk in four characters.
        /// </summary>
        public FourCharacterCode ChunkTypeId { get; set; }

        /// <summary>
        /// Returns the identification of a chunk declared in a ChunkAttribute on the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Returns null if not found.</returns>
        public static string GetChunkId(Type type)
        {
            Contract.Requires(type != null);
            Throw.IfArgumentNull(type, "type");

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
        public static string GetChunkId(object instance)
        {
            Contract.Requires(instance != null);
            Throw.IfArgumentNull(instance, "instance");

            return GetChunkId(instance.GetType());
        }
    }
}