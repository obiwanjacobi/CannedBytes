namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Placed on a field or property that represents a collection, it indicates what types (chunks)
    /// can be used as items.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ChunkTypeAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="chunkTypeId">The chunk id of the chunk type.</param>
        public ChunkTypeAttribute(string chunkTypeId)
        {
            Check.IfArgumentNullOrEmpty(chunkTypeId, nameof(chunkTypeId));

            this.ChunkTypeId = new FourCharacterCode(chunkTypeId);
        }

        /// <summary>
        /// Gets the chunk id.
        /// </summary>
        public FourCharacterCode ChunkTypeId { get; private set; }

        /// <summary>
        /// Indicates if any chunk type attributes are declared on the <paramref name="member"/>.
        /// </summary>
        /// <param name="member">Must not be null.</param>
        /// <returns>Returns true if any chunk types are found.</returns>
        public static bool HasChunkTypes(MemberInfo member)
        {
            Check.IfArgumentNull(member, nameof(member));

            var types = GetChunkTypes(member);

            return types != null && types.Length > 0;
        }

        /// <summary>
        /// Returns the chunk types for the <paramref name="member"/>.
        /// </summary>
        /// <param name="member">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        public static string[] GetChunkTypes(MemberInfo member)
        {
            Check.IfArgumentNull(member, nameof(member));

            var result = from attr in member.GetCustomAttributes(typeof(ChunkTypeAttribute), false)
                         where attr != null
                         select ((ChunkTypeAttribute)attr).ChunkTypeId.ToString();

            return result.ToArray();
        }
    }
}