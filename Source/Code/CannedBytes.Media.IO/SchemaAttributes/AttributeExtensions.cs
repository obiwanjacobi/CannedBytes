namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.Linq;

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
            Check.IfArgumentNull(type, nameof(type));
            return type.GetCustomAttributes(typeof(ChunkAttribute), false)
                .Any(a => a != null);
        }
    }
}