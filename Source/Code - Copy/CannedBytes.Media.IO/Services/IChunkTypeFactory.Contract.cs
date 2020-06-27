namespace CannedBytes.Media.IO.Services
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Contract for <see cref="IChunkTypeFactory"/>.
    /// </summary>
    [ContractClassFor(typeof(IChunkTypeFactory))]
    internal abstract class ChunkTypeFactoryContract : IChunkTypeFactory
    {
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