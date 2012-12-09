using System;
using System.Diagnostics.Contracts;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Contract for <see cref="IChunkTypeFactory"/>.
    /// </summary>
    [ContractClassFor(typeof(IChunkTypeFactory))]
    internal abstract class ChunkTypeFactoryContract : IChunkTypeFactory
    {
        /// <summary>
        /// Block instantiation.
        /// </summary>
        private ChunkTypeFactoryContract()
        { }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="chunkTypeId">Must not be null.</param>
        /// <returns>Can return null.</returns>
        object IChunkTypeFactory.CreateChunkObject(FourCharacterCode chunkTypeId)
        {
            Contract.Requires(chunkTypeId != null);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="chunkTypeId">Must not be null.</param>
        /// <returns>Can return null.</returns>
        Type IChunkTypeFactory.LookupChunkObjectType(FourCharacterCode chunkTypeId)
        {
            Contract.Requires(chunkTypeId != null);

            throw new NotImplementedException();
        }
    }
}