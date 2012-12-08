using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using CannedBytes.Media.IO.SchemaAttributes;

namespace CannedBytes.Media.IO
{
    [Export]
    public class FileChunkHandlerManager
    {
        //warning CS0649: Field 'X' is never assigned to, and will always have its default value null
#pragma warning disable 0649
        [ImportMany(AllowRecomposition = true)]
        Lazy<IFileChunkHandler, IFileChunkHandlerMetaInfo>[] chunkHandlers;
#pragma warning restore 0649

        public IFileChunkHandler GetChunkHandler(FourCharacterCode chunkId)
        {
            Contract.Requires<ArgumentNullException>(chunkId != null);
            Contract.Ensures(Contract.Result<IFileChunkHandler>() != null);

            var chunk4cc = chunkId.ToString();

            var handler = (from pair in this.chunkHandlers
                           where pair.Metadata.ChunkId.ToString() != "****"
                           where chunk4cc.MatchesWith(pair.Metadata.ChunkId.ToString())
                           select pair.Value).FirstOrDefault();

            if (handler == null)
            {
                // retrieve default handler
                handler = (from pair in this.chunkHandlers
                           where pair.Metadata.ChunkId.ToString() == "****"
                           select pair.Value).FirstOrDefault();
            }

            if (handler == null)
            {
                throw new InvalidOperationException(
                    "No default chunk handler was found ('****').");
            }

            return handler;
        }
    }
}