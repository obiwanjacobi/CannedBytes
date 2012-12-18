namespace CannedBytes.Media.IO.ChunkTypes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using CannedBytes.Media.IO.SchemaAttributes;
    using CannedBytes.Media.IO.Services;

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
        /// <returns>Returns the runtime object instance or null when no type was found.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public override object Read(ChunkFileContext context)
        {
            Contract.Requires(context.CompositionContainer != null);
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(context.ChunkStack, "context.ChunkStack");
            Check.IfArgumentNull(context.ChunkStack.CurrentChunk, "context.ChunkStack.CurrentChunk");
            Check.IfArgumentNull(context.CompositionContainer, "context.CompositionContainer");

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
                var runtimeObj = reader.ReadRuntimeContainerChunkType(stream, listChunk.ItemType);

                // check if CLR type could be found for 'ItemType'.
                if (runtimeObj != null && children != null)
                {
                    children.Add(runtimeObj);
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
        private static Type LookupItemType(ChunkFileContext context, FourCharacterCode chunkId)
        {
            var factory = context.CompositionContainer.GetService<IChunkTypeFactory>();

            return factory.LookupChunkObjectType(chunkId);
        }

        public override bool CanWrite(object instance)
        {
            return base.CanWrite(instance) && instance is ListChunk;
        }

        public override void Write(ChunkFileContext context, object instance)
        {
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(instance, "instance");
            Check.IfArgumentNotOfType<ListChunk>(instance, "instance");

            var listChunk = (ListChunk)instance;

            if (listChunk.InnerChunks == null)
            {
                throw new ArgumentException("No LIST content found.", "instance.InnerChunks");
            }

            listChunk.ItemType = new FourCharacterCode(GetCollectionItemChunkId(listChunk.InnerChunks));

            // write out LIST chunk
            base.Write(context, instance);

            var writer = context.CompositionContainer.GetService<FileChunkWriter>();

            foreach (var item in listChunk.InnerChunks)
            {
                writer.WriteRuntimeChunkType(item);
            }
        }

        private string GetCollectionItemChunkId(object instance)
        {
            Type type = instance.GetType();
            Type dataType = null;

            if (type.IsGenericType)
            {
                var genType = type.GetGenericTypeDefinition();

                if (genType.FullName.StartsWith("System.Collections.Generic.") &&
                    genType.FullName.EndsWith("`1"))
                {
                    dataType = (from typeArg in type.GetGenericArguments()
                                select typeArg).FirstOrDefault();
                }
                else
                {
                    var msg = String.Format(
                              CultureInfo.InvariantCulture,
                              "The generic type '{0}' is not supported. Use IEnumerable<T> for collections.",
                              genType.FullName);

                    throw new NotSupportedException(msg);
                }
            }
            else
            {
                dataType = type;
            }

            string chunkId = null;

            if (dataType != null)
            {
                chunkId = ChunkAttribute.GetChunkId(dataType);
            }

            return chunkId;
        }
    }
}