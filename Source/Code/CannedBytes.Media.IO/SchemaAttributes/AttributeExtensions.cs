using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace CannedBytes.Media.IO.SchemaAttributes
{
    /// <summary>
    /// Extension methods for the schema attributes.
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// Indicates if the <paramref name="type"/> (this) is a chunk type.
        /// </summary>
        /// <param name="type">Must not be null.</param>
        /// <returns>Returns true when the <see cref="ChunkAttribute"/> is applied to the <paramref name="type"/>.</returns>
        public static bool IsChunk(this Type type)
        {
            Contract.Requires(type != null);
            Throw.IfArgumentNull(type, "type");

            return (from attr in type.GetCustomAttributes(typeof(ChunkAttribute), false)
                    where attr != null
                    select attr).Any();
        }
    }
}