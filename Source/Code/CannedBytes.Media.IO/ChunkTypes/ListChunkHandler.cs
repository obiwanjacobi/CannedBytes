using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using CannedBytes.Media.IO.SchemaAttributes;
using CannedBytes.Media.IO.Services;

namespace CannedBytes.Media.IO.ChunkTypes
{
    /// <summary>
    /// Called by the <see cref="FileChunkReader"/> when a 'LIST' chunk is encountered.
    /// </summary>
    [FileChunkHandler("LIST")]
    public class ListChunkHandler : DefaultFileChunkHandler
    {
        /// <summary>
        /// Reads the chunk file until the complete LIST of chunks are read.
        /// </summary>
        /// <param name="context">The context of the file being read. Must not be null.</param>
        /// <returns></returns>
        public override object Read(ChunkFileContext context)
        {
            Contract.Requires(context.CompositionContainer != null);
            Throw.IfArgumentNull(context, "context");
            Throw.IfArgumentNull(context.ChunkStack, "context.ChunkStack");
            Throw.IfArgumentNull(context.ChunkStack.CurrentChunk, "context.ChunkStack.CurrentChunk");
            Throw.IfArgumentNull(context.CompositionContainer, "context.CompositionContainer");

            // create instance and read type
            var listChunk = base.Read(context) as ListChunk;

            if (listChunk == null)
            {
                throw new InvalidOperationException();
            }

            // read child chunk of 'type'
            var reader = context.CompositionContainer.GetService<FileChunkReader>();
            var chunk = context.ChunkStack.CurrentChunk;

            var stream = chunk.DataStream;
            var itemType = LookupItemType(context, listChunk.ItemType);
            IList children = null;

            if (itemType != null)
            {
                // create a generic list with the correct item type.
                var listType = typeof(List<>).MakeGenericType(new[] { itemType });
                children = (IList)Activator.CreateInstance(listType);
            }

            // while there is still data in the stream
            while (stream.Position < stream.Length)
            {
                var rtObj = reader.ReadRuntimeContainerChunkType(stream, listChunk.ItemType);

                // check if CLR type could be found for 'ItemType'.
                if (rtObj != null && children != null)
                {
                    children.Add(rtObj);
                }
            }

            if (children != null)
            {
                listChunk.InnerChunks = children.Cast<object>();
            }

            return children;
        }

        /// <summary>
        /// Helper to find the runtime Type for a chunk.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        /// <param name="chunkId">Must not be null.</param>
        /// <returns>Returns null when not found.</returns>
        private Type LookupItemType(ChunkFileContext context, FourCharacterCode chunkId)
        {
            var factory = context.CompositionContainer.GetService<IChunkTypeFactory>();

            return factory.LookupChunkObjectType(chunkId);
        }

        /// <inheritdocs/>
        public override void Write(ChunkFileContext context, object instance)
        {
            throw new NotImplementedException();
        }
    }
}