using System;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO.ChunkTypes
{
    [FileChunkHandler("RIFF")]
    public class RiffChunkHandler : DefaultFileChunkHandler
    {
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
                    "The Four Character Code '{0}' specified in the RIFF (root) chunk could not be mapped to a CLR Type.",
                    riffChunk.FileType);

                throw new ChunkFileException(msg);
            }

            return riffChunk;
        }

        public override void Write(ChunkFileContext context, object instance)
        {
            throw new NotImplementedException();
        }
    }
}