using System;
using System.ComponentModel.Composition;

namespace CannedBytes.Media.IO.SchemaAttributes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class FileChunkHandlerAttribute : ExportAttribute, IFileChunkHandlerMetaInfo
    {
        public FileChunkHandlerAttribute(string chunkId)
            : base(typeof(IFileChunkHandler))
        {
            ChunkId = chunkId;
        }

        public string ChunkId { get; set; }
    }

    /// <summary>
    /// Interface used by importer to access meta data.
    /// </summary>
    public interface IFileChunkHandlerMetaInfo
    {
        string ChunkId { get; }
    }
}