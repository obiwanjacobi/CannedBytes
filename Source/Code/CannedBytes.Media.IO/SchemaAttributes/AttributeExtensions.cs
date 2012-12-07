using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace CannedBytes.Media.IO.SchemaAttributes
{
    public static class AttributeExtensions
    {
        public static bool IsChunk(this Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return (from attr in type.GetCustomAttributes(typeof(ChunkAttribute), false)
                    where attr != null
                    select attr).Any();
        }
    }
}