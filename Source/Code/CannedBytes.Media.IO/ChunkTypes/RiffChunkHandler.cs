namespace CannedBytes.Media.IO.ChunkTypes
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using CannedBytes.Media.IO.SchemaAttributes;

    /// <summary>
    /// Handles serialization of a RIFF chunk.
    /// </summary>
    [FileChunkHandler("RIFF")]
    public class RiffChunkHandler : DefaultFileChunkHandler
    {
        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public override object Read(ChunkFileContext context)
        {
            // create instance and read type
            var riffChunk = base.Read(context) as RiffChunk;

            if (riffChunk == null)
            {
                throw new InvalidOperationException();
            }

            // read child chunk of 'type'
            var reader = context.CompositionContainer.GetService<FileChunkReader>();
            var stream = reader.CurrentStream;

            riffChunk.InnerChunk = reader.ReadRuntimeContainerChunkType(stream, riffChunk.FileType);

            // the 4CC FileType was not found as a runtime chunk type.
            if (riffChunk.InnerChunk == null)
            {
                // normally we'd skip an unknown chunk but at the root its no use.
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          "The Four Character Code '{0}' specified in the RIFF (root) chunk could not be mapped to a CLR Type.",
                          riffChunk.FileType);

                throw new ChunkFileException(msg);
            }

            return riffChunk;
        }

        /// <summary>
        /// Writes the <paramref name="instance"/> to the file stream.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        /// <param name="instance">Must not be null.</param>
        public override void Write(ChunkFileContext context, object instance)
        {
            Contract.Requires(context != null);
            Contract.Requires(instance != null);
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(instance, "instance");

            throw new NotImplementedException();
        }
    }
}