using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.Services
{
    [Export(typeof(ChunkTypeFactory))]
    [Export(typeof(IChunkTypeFactory))]
    public class ChunkTypeFactory : IChunkTypeFactory
    {
        Dictionary<string, Type> chunkMap = new Dictionary<string, Type>();

        public ChunkTypeFactory()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            AddChunksFrom(assembly, true);
        }

        public virtual void AddChunksFrom(Assembly assembly, bool replace)
        {
            var result = from type in assembly.GetTypes()
                         from attr in type.GetCustomAttributes(typeof(ChunkAttribute), false)
                         where attr != null
                         select new { Type = type, Attribute = attr as ChunkAttribute };

            foreach (var item in result)
            {
                var chunkId = item.Attribute.ChunkTypeId.ToString();

                if (this.chunkMap.ContainsKey(chunkId))
                {
                    if (replace)
                    {
                        this.chunkMap[chunkId] = item.Type;
                    }
                }
                else
                {
                    this.chunkMap.Add(chunkId, item.Type);
                }
            }
        }

        public virtual object CreateChunkObject(FourCharacterCode chunkTypeId)
        {
            Type result = LookupChunkObjectType(chunkTypeId);

            if (result != null)
            {
                return Activator.CreateInstance(result);
            }

            return null;
        }

        public virtual Type LookupChunkObjectType(FourCharacterCode chunkTypeId)
        {
            Type result = null;

            string chunkId = chunkTypeId.ToString();

            if (!chunkId.HasWildcard())
            {
                // try fast lookup first if the requested type has no wildcards
                if (this.chunkMap.TryGetValue(chunkId, out result))
                {
                    return result;
                }
            }

            // now match wildcards
            foreach (var item in this.chunkMap)
            {
                if (chunkId.MatchesWith(item.Key))
                {
                    return item.Value;
                }
            }

            return null;
        }
    }
}