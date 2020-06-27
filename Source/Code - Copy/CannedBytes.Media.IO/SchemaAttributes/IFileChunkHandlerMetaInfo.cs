namespace CannedBytes.Media.IO.SchemaAttributes
{
    /// <summary>
    /// Interface used by importer to access meta data.
    /// </summary>
    public interface IFileChunkHandlerMetaInfo
    {
        /// <summary>
        /// Gets the chunk id for the chunk handler. Can contain wildcards.
        /// </summary>
        string ChunkId { get; }
    }
}