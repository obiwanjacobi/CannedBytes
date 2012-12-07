using System;
using System.Diagnostics.Contracts;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO
{
    [FileChunkHandler("****")]
    public class DefaultFileChunkHandler : FileChunkHandler
    {
        public override bool CanRead(FileChunk chunk)
        {
            return (chunk.DataStream != null && chunk.DataStream.CanRead);
        }

        public override object Read(ChunkFileContext context)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentException>(context.ChunkStack.CurrentChunk != null,
                "The FileContext must have the CurrentChunk property set.");

            var reader = context.CompositionContainer.GetService<FileChunkReader>();
            var stream = context.ChunkFile.BaseStream;
            var chunk = context.ChunkStack.CurrentChunk;

            chunk.RuntimeInstance = reader.ReadRuntimeChunkType(stream, chunk.ChunkId, true);

            // extra check if type returned is correct for chunk.
            if (chunk.RuntimeInstance != null &&
                !ChunkAttribute.GetChunkId(chunk.RuntimeInstance).MatchesWith(chunk.ChunkId.ToString()))
            {
                var msg = String.Format(
                    "The type '{0}' tagged with '{1}' was returned when '{2}' was requested.",
                    chunk.RuntimeInstance.GetType(), ChunkAttribute.GetChunkId(chunk.RuntimeInstance), chunk.ChunkId);

                throw new ChunkFileException(msg);
            }

            return chunk.RuntimeInstance;
        }

        public override bool CanWrite(FileChunk chunk)
        {
            return (chunk.DataStream != null && chunk.DataStream.CanWrite);
        }

        public override void Write(ChunkFileContext context, object instance)
        {
            throw new NotImplementedException();
        }
    }
}