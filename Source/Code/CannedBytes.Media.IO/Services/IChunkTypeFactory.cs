namespace CannedBytes.Media.IO.Services
{
    using System;

    /// <summary>
    /// Implemented by a factory that knows how to create runtime types for chunk identifiers.
    /// </summary>
    public interface IChunkTypeFactory
    {
        /// <summary>
        /// Creates a new instance for the specified <paramref name="chunkTypeId"/>.
        /// </summary>
        /// <param name="chunkTypeId">Must not be null.</param>
        /// <returns>Returns null when not found.</returns>
        object CreateChunkObject(FourCharacterCode chunkTypeId);

        /// <summary>
        /// Looks up the <see cref="Type"/> for the specified <paramref name="chunkTypeId"/>.
        /// </summary>
        /// <param name="chunkTypeId">Must not be null.</param>
        /// <returns>Returns null when not found.</returns>
        Type LookupChunkObjectType(FourCharacterCode chunkTypeId);
    }
}