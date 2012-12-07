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
        public ChunkAttribute()
        { }

        public ChunkAttribute(string fourCharacterCode)
        {
            ChunkTypeId = new FourCharacterCode(fourCharacterCode);
        }

        public FourCharacterCode ChunkTypeId { get; set; }

        public static string GetChunkId(Type type)
        {
            Contract.Requires(type != null);

            var result = (from attr in type.GetCustomAttributes(typeof(ChunkAttribute), false)
                          where attr != null
                          select ((ChunkAttribute)attr).ChunkTypeId.ToString()).FirstOrDefault();

            return result;
        }

        public static string GetChunkId(object instance)
        {
            Contract.Requires(instance != null);

            return GetChunkId(instance.GetType());
        }
    }
}