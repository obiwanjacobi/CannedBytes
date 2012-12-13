namespace CannedBytes.Media.IO.SchemaAttributes
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Placed on a field or property that represents a collection, it indicates what types (chunks)
    /// can be used as items.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Got that and it still complains.")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ChunkTypeAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="chunkTypeId">The chunk id of the chunk type.</param>
        public ChunkTypeAttribute(string chunkTypeId)
        {
            Contract.Requires(!String.IsNullOrEmpty(chunkTypeId));
            Check.IfArgumentNullOrEmpty(chunkTypeId, "chunkTypeId");

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
            Contract.Requires(member != null);
            Check.IfArgumentNull(member, "member");

            var types = GetChunkTypes(member);

            return types != null && types.Length > 0;
        }

        /// <summary>
        /// Returns the chunk types for the <paramref name="member"/>.
        /// </summary>
        /// <param name="member">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public static string[] GetChunkTypes(MemberInfo member)
        {
            Contract.Requires(member != null);
            Contract.Ensures(Contract.Result<string[]>() != null);
            Check.IfArgumentNull(member, "member");

            var result = from attr in member.GetCustomAttributes(typeof(ChunkTypeAttribute), false)
                         where attr != null
                         select ((ChunkTypeAttribute)attr).ChunkTypeId.ToString();

            return result.ToArray();
        }
    }
}