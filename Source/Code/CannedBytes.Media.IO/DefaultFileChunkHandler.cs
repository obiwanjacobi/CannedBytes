using System;
using System.Diagnostics.Contracts;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// This chunk handler matches any chunk and handles it in a generic way.
    /// </summary>
    [FileChunkHandler("****")]
    public class DefaultFileChunkHandler : FileChunkHandler
    {
        /// <summary>
        /// Indicates if the specified <paramref name="chunk"/> can be read by the handler.
        /// </summary>
        /// <param name="chunk">Must not be null.</param>
        /// <returns>Returns true if the <paramref name="chunk"/> can be read.</returns>
        public override bool CanRead(FileChunk chunk)
        {
            Throw.IfArgumentNull(chunk, "chunk");

            return (chunk.DataStream != null && chunk.DataStream.CanRead);
        }

        /// <summary>
        /// Reads the current chunk and creates a runtime type for it, which is then returned.
        /// </summary>
        /// <param name="context">The context of the chunk file being read. Must not be null.</param>
        /// <returns>Returns null if there was no runtime type found for the current chunk.</returns>
        public override object Read(ChunkFileContext context)
        {
            Contract.Requires(context.ChunkStack != null);
            Contract.Requires(context.ChunkStack.CurrentChunk != null);
            Contract.Requires(context.ChunkFile != null);
            Contract.Requires(context.ChunkFile.BaseStream != null);
            Throw.IfArgumentNull(context, "context");
            Throw.IfArgumentNull(context.ChunkStack, "context.ChunkStack");
            Throw.IfArgumentNull(context.ChunkStack.CurrentChunk, "context.ChunkStack.CurrentChunk");
            Throw.IfArgumentNull(context.ChunkFile, "context.ChunkFile");
            Throw.IfArgumentNull(context.ChunkFile.BaseStream, "context.ChunkFile.BaseStream");

            var reader = context.CompositionContainer.GetService<FileChunkReader>();
            var stream = context.ChunkFile.BaseStream;

            var chunk = context.ChunkStack.CurrentChunk;

            chunk.RuntimeInstance = reader.ReadRuntimeChunkType(stream, chunk.ChunkId);

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

        /// <inheritdocs/>
        public override bool CanWrite(FileChunk chunk)
        {
            return (chunk.DataStream != null && chunk.DataStream.CanWrite);
        }

        /// <inheritdocs/>
        public override void Write(ChunkFileContext context, object instance)
        {
            throw new NotImplementedException();
        }
    }
}