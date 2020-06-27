namespace CannedBytes.Media.IO.Services
{
    using CannedBytes.Media.IO.SchemaAttributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Implementation of the <see cref="IChunkTypeFactory"/> interface.
    /// </summary>
    [Export(typeof(ChunkTypeFactory))]
    [Export(typeof(IChunkTypeFactory))]
    public class ChunkTypeFactory : IChunkTypeFactory
    {
        /// <summary>
        /// The Type lookup table.
        /// </summary>
        private readonly Dictionary<string, Type> chunkMap = new Dictionary<string, Type>();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public ChunkTypeFactory()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            this.AddChunksFrom(assembly, true);
        }

        /// <summary>
        /// Scans the <paramref name="assembly"/> for types that have the
        /// <see cref="ChunkAttribute"/> applied and adds them to the factory.
        /// </summary>
        /// <param name="assembly">Must not be null.</param>
        /// <param name="replace">If true the Type found in the <paramref name="assembly"/> will replace
        /// an already registered type.</param>
        public void AddChunksFrom(Assembly assembly, bool replace)
        {
            Check.IfArgumentNull(assembly, "assembly");

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

        /// <inheritdocs/>
        public virtual object CreateChunkObject(FourCharacterCode chunkTypeId)
        {
            Check.IfArgumentNull(chunkTypeId, "chunkTypeId");

            Type result = this.LookupChunkObjectType(chunkTypeId);

            if (result != null)
            {
                return Activator.CreateInstance(result);
            }

            return null;
        }

        /// <inheritdocs/>
        public virtual Type LookupChunkObjectType(FourCharacterCode chunkTypeId)
        {
            Check.IfArgumentNull(chunkTypeId, "chunkTypeId");

            string chunkId = chunkTypeId.ToString();

            if (!chunkId.HasWildcard())
            {
                Type result = null;

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