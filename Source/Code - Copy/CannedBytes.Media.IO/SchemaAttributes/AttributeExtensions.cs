namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public static bool IsChunk(this Type type)
        {
            Contract.Requires(type != null);
            Check.IfArgumentNull(type, "type");

            return (from attr in type.GetCustomAttributes(typeof(ChunkAttribute), false)
                    where attr != null
                    select attr).Any();
        }
    }
}