namespace CannedBytes.Media.IO
{
    using CannedBytes.Media.IO.SchemaAttributes;
    using System;
    using System.Globalization;

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
            Check.IfArgumentNull(chunk, "chunk");

            return chunk.DataStream != null && chunk.DataStream.CanRead;
        }

        /// <summary>
        /// Reads the current chunk and creates a runtime type for it, which is then returned.
        /// </summary>
        /// <param name="context">The context of the chunk file being read. Must not be null.</param>
        /// <returns>Returns null if there was no runtime type found for the current chunk.</returns>
        public override object Read(ChunkFileContext context)
        {
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(context.ChunkStack, "context.ChunkStack");
            Check.IfArgumentNull(context.ChunkStack.CurrentChunk, "context.ChunkStack.CurrentChunk");
            Check.IfArgumentNull(context.ChunkFile, "context.ChunkFile");
            Check.IfArgumentNull(context.ChunkFile.BaseStream, "context.ChunkFile.BaseStream");

            var reader = context.Services.GetService<FileChunkReader>();
            var stream = context.ChunkFile.BaseStream;

            var chunk = context.ChunkStack.CurrentChunk;

            chunk.RuntimeInstance = reader.ReadRuntimeChunkType(stream, chunk.ChunkId);

            // extra check if type returned is correct for chunk.
            if (chunk.RuntimeInstance != null &&
                !ChunkAttribute.GetChunkId(chunk.RuntimeInstance).MatchesWith(chunk.ChunkId.ToString()))
            {
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          "The type '{0}' tagged with '{1}' was returned when '{2}' was requested.",
                          chunk.RuntimeInstance.GetType(),
                          ChunkAttribute.GetChunkId(chunk.RuntimeInstance),
                          chunk.ChunkId);

                throw new ChunkFileException(msg);
            }

            return chunk.RuntimeInstance;
        }

        /// <inheritdocs/>
        public override void Write(ChunkFileContext context, object instance)
        {
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(instance, "instance");

            var writer = context.Services.GetService<FileChunkWriter>();

            writer.WriteRuntimeChunkType(instance);
        }
    }
}