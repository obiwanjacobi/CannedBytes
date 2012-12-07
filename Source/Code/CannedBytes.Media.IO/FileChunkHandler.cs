using System;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO
{
    public abstract class FileChunkHandler : IFileChunkHandler
    {
        protected FileChunkHandler()
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(FileChunkHandlerAttribute), true);

            if (attrs == null || attrs.Length == 0)
            {
                throw new InvalidOperationException(
                    "A FileChunkHandler derived class must be marked with the FileChunkHandlerExportAttribute.");
            }

            ChunkId = new FourCharacterCode(((FileChunkHandlerAttribute)attrs[0]).ChunkId);
        }

        public FourCharacterCode ChunkId { get; private set; }

        public virtual bool CanRead(FileChunk chunk)
        {
            return (chunk.ChunkId.ToString() == ChunkId.ToString() &&
                chunk.DataStream != null && chunk.DataStream.CanRead);
        }

        public abstract object Read(ChunkFileContext context);

        public virtual bool CanWrite(FileChunk chunk)
        {
            return (chunk.ChunkId.ToString() == ChunkId.ToString() &&
                chunk.DataStream != null && chunk.DataStream.CanWrite);
        }

        public abstract void Write(ChunkFileContext context, object instance);
    }
}