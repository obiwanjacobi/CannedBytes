using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace CannedBytes.Media.IO.SchemaAttributes
{
    /// <summary>
    /// Placed on a field or property that represents a collection, it indicates what types (chunks)
    /// can be used as items.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ChunkTypeAttribute : Attribute
    {
        public ChunkTypeAttribute()
        { }

        public ChunkTypeAttribute(string fourCharacterCode)
        {
            ChunkTypeId = new FourCharacterCode(fourCharacterCode);
        }

        public FourCharacterCode ChunkTypeId { get; set; }

        public static bool HasChunkTypes(MemberInfo memberInfo)
        {
            var types = GetChunkTypes(memberInfo);

            return (types != null && types.Length > 0);
        }

        public static string[] GetChunkTypes(MemberInfo memberInfo)
        {
            Contract.Requires(memberInfo != null);

            var result = (from attr in memberInfo.GetCustomAttributes(typeof(ChunkTypeAttribute), false)
                          where attr != null
                          select ((ChunkTypeAttribute)attr).ChunkTypeId.ToString());

            return result.ToArray();
        }
    }
}