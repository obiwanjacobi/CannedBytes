using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CannedBytes.Media.IO.SchemaAttributes;
using CannedBytes.Media.IO.Services;

namespace CannedBytes.Media.IO.ChunkTypes
{
    [FileChunkHandler("LIST")]
    public class ListChunkHandler : DefaultFileChunkHandler
    {
        public override object Read(ChunkFileContext context)
        {
            // create instance and read type
            var listChunk = base.Read(context) as ListChunk;

            if (listChunk == null)
            {
                throw new InvalidOperationException();
            }

            // read child chunk of 'type'
            var reader = context.CompositionContainer.GetService<FileChunkReader>();
            var chunk = context.ChunkStack.CurrentChunk;
            var stream = reader.CurrentStream;
            var itemType = LookupItemType(context, listChunk.ItemType);

            if (itemType != null)
            {
                // create a generic list with the correct item type.
                var listType = typeof(List<>).MakeGenericType(new[] { itemType });
                var children = (IList)Activator.CreateInstance(listType);

                // while there is still data in the stream
                while (chunk.DataStream.Position < chunk.DataStream.Length)
                {
                    var rtObj = reader.ReadRuntimeContainerChunkType(stream, listChunk.ItemType);

                    // check if CLR type could be found for 'ItemType'.
                    if (rtObj != null)
                    {
                        children.Add(rtObj);
                    }
                }

                listChunk.InnerChunks = children.Cast<object>();
                return children;
            }

            return null;
        }

        private Type LookupItemType(ChunkFileContext context, FourCharacterCode chunkId)
        {
            var factory = context.CompositionContainer.GetService<IChunkTypeFactory>();

            return factory.LookupChunkObjectType(chunkId);
        }

        public override void Write(ChunkFileContext context, object instance)
        {
            throw new NotImplementedException();
        }
    }
}