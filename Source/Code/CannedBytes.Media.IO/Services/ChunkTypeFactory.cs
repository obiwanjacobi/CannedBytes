namespace CannedBytes.Media.IO.Services
{
    using CannedBytes.Media.IO.SchemaAttributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Implementation of the <see cref="IChunkTypeFactory"/> interface.
    /// </summary>
    public class ChunkTypeFactory : IChunkTypeFactory
    {
        /// <summary>
        /// The Type lookup table.
        /// </summary>
        private readonly Dictionary<string, Type> _chunkMap = new Dictionary<string, Type>();

        public void AddChunkType(Type chunkType, AddMode addMode)
        {
            var chunkId = ChunkAttribute.GetChunkId(chunkType);
            AddChunkType(chunkId, chunkType, addMode);
        }

        /// <summary>
        /// Scans the <paramref name="assembly"/> for types that have the
        /// <see cref="ChunkAttribute"/> applied and adds them to the factory.
        /// </summary>
        /// <param name="assembly">Must not be null.</param>
        /// <param name="replace">If true the Type found in the <paramref name="assembly"/> will replace
        /// an already registered type.</param>
        public void AddChunksFrom(Assembly assembly, AddMode addMode)
        {
            Check.IfArgumentNull(assembly, "assembly");

            var result = from type in assembly.GetTypes()
                         where type.IsClass && type.IsChunk()
                         select new { ChunkId = ChunkAttribute.GetChunkId(type), Type = type };

            foreach (var item in result)
            {
                AddChunkType(item.ChunkId, item.Type, addMode);
            }
        }

        private void AddChunkType(string chunkId, Type chunkType, AddMode addMode)
        {
            Check.IfArgumentNullOrEmpty(chunkId, nameof(chunkId));
            Check.IfArgumentNull(chunkType, nameof(chunkType));

            if (_chunkMap.ContainsKey(chunkId))
            {
                switch (addMode)
                {
                    case AddMode.ThrowIfExists:
                        throw new ChunkFileException($"The Chunk Id {chunkId} is already registered.");
                    case AddMode.SkipIfExists:
                        break;
                    case AddMode.OverwriteExisting:
                        _chunkMap[chunkId] = chunkType;
                        break;
                    default:
                        throw new NotImplementedException("Illegal AddMode Enum value.");
                }
            }
            else
            {
                _chunkMap.Add(chunkId, chunkType);
            }
        }

        /// <inheritdocs/>
        public virtual object CreateChunkObject(FourCharacterCode chunkTypeId)
        {
            Check.IfArgumentNull(chunkTypeId, "chunkTypeId");

            Type result = LookupChunkObjectType(chunkTypeId);

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

            var chunkId = chunkTypeId.ToString();

            if (!chunkId.HasWildcard())
            {
                // try fast lookup first if the requested type has no wildcards
                if (!_chunkMap.TryGetValue(chunkId, out Type result))
                {
                    return null;
                }
                return result;
            }

            // now match wildcards
            foreach (var item in _chunkMap)
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