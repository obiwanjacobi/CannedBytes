namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using CannedBytes.Media.IO.ChunkTypes;
    using CannedBytes.Media.IO.SchemaAttributes;
    using CannedBytes.Media.IO.Services;

    /// <summary>
    /// Implements the writing of chunks.
    /// </summary>
    public class FileChunkWriter
    {
        /// <summary>
        /// Private reference to the file context.
        /// </summary>
        private ChunkFileContext context;

        ////warning CS0649: Field 'X' is never assigned to, and will always have its default value null
#pragma warning disable 0649
        /// <summary>
        /// Optional reference to the stream navigator.
        /// </summary>
        [Import(AllowDefault = true, AllowRecomposition = true)]
        private IStreamNavigator streamNavigator;

        /// <summary>
        /// Private reference to the chunk handler manager.
        /// </summary>
        [Import]
        private FileChunkHandlerManager handlerMgr;

        /// <summary>
        /// Private reference to the string writer.
        /// </summary>
        [Import]
        private IStringWriter stringWriter;

        /// <summary>
        /// Private reference to the number writer.
        /// </summary>
        [Import]
        private INumberWriter numberWriter;
#pragma warning restore 0649

        /// <summary>
        /// Constructs a new writer instance.
        /// </summary>
        /// <param name="context">Must not be null. Can be reused from a read operation.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public FileChunkWriter(ChunkFileContext context)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.CompositionContainer != null);
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(context.CompositionContainer, "context.CompositionContainer");

            context.CompositionContainer.ComposeParts(this);
            context.CompositionContainer.AddInstance(this);

            this.context = context;
        }

        /// <summary>
        /// Pops a header from the header stack.
        /// </summary>
        /// <returns>Returns the header popped of the stack.</returns>
        private FileChunkHeader PopHeader()
        {
            return this.context.HeaderStack.Pop();
        }

        /// <summary>
        /// Pushes a new initialized header onto the stack.
        /// </summary>
        /// <param name="chunkId">Must not be null.</param>
        /// <returns>Returns the header object. Never returns null.</returns>
        private FileChunkHeader PushNewHeader(FourCharacterCode chunkId)
        {
            var header = new FileChunkHeader();
            header.DataStream = new MemoryStream();
            header.ChunkId = chunkId;

            this.context.HeaderStack.Push(header);

            return header;
        }

        /// <summary>
        /// Writes the <paramref name="chunk"/> to the file.
        /// </summary>
        /// <param name="chunk">Must not be null.</param>
        /// <returns>Returns the header information for the chunk that has just been written.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public FileChunkHeader WriteNextChunk(object chunk)
        {
            Contract.Requires(chunk != null);
            Check.IfArgumentNull(chunk, "chunk");

            // find chunk id
            var chunkId = ChunkAttribute.GetChunkId(chunk);

            if (String.IsNullOrEmpty(chunkId))
            {
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          "No Chunk Attribute was found for the specified runtime object of type '{0}'.",
                          chunk.GetType().FullName);

                throw new ArgumentException(msg);
            }

            if (chunkId.HasWildcard())
            {
                // TODO: this must be pluggable in some way
                // for now only digits
                chunkId = chunkId.Merge("0000");
            }

            var chunkTypeId = new FourCharacterCode(chunkId);
            var chunkHandler = this.handlerMgr.GetChunkHandler(chunkTypeId);

            if (!chunkHandler.CanWrite(chunk))
            {
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          "The chunk handler '{0}' cannot write the specified runtime object.",
                          chunkHandler.ChunkId);

                throw new ArgumentException(msg);
            }

            int stackPos = this.context.HeaderStack.Count;
            var header = this.PushNewHeader(chunkTypeId);

            chunkHandler.Write(this.context, chunk);

            // wind down the stack to the level it was before we started.
            while (this.context.HeaderStack.Count > stackPos)
            {
                var poppedHeader = this.PopHeader();
                this.WriteChunkHeader(poppedHeader);
            }

            return header;
        }

        /// <summary>
        /// Writes the chunk identified by the <paramref name="header"/> to the <see cref="P:CurrentStream"/>.
        /// </summary>
        /// <param name="header">Must not be null and must NOT be on the <see cref="P:ChunkFileContext.HeaderStack"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public void WriteChunkHeader(FileChunkHeader header)
        {
            Check.IfArgumentNull(header, "header");

            var stream = this.CurrentStream;

            if (Object.ReferenceEquals(header.DataStream, stream))
            {
                throw new ArgumentException("Specified header is still on the stack.", "header");
            }

            // write chunk header
            header.ChunkId.WriteTo(stream);

            // data length
            this.numberWriter.WriteInt32(header.DataStream.Position, stream);

            // write chunk body
            header.DataStream.Position = 0;
            header.DataStream.CopyTo(stream);

            // optionally align each chunk
            if (this.streamNavigator != null)
            {
                this.streamNavigator.AlignPosition(stream);
            }
        }

        /// <summary>
        /// Writes the content of the chunk <paramref name="instance"/> to the <see cref="P:CurrentStream"/>.
        /// </summary>
        /// <param name="instance">Must not be null.</param>
        /// <remarks>The <paramref name="instance"/> either has data fields or sub-chunks. Mixed content is not supported.</remarks>
        public void WriteRuntimeChunkType(object instance)
        {
            var objectReader = new ObjectMemberReader(instance);

            if (objectReader.IsChunkContainer)
            {
                object childObject = null;

                while (objectReader.GetNextChunkObject(out childObject))
                {
                    // property value is null, skip it.
                    if (childObject == null)
                    {
                        continue;
                    }

                    if (objectReader.CurrentMemberIsCollection)
                    {
                        var collection = (IEnumerable<object>)childObject;

                        if (objectReader.CurrentMemberIsListChunk)
                        {
                            // insert a LIST chunk.
                            var listChunk = new ListChunk();
                            listChunk.InnerChunks = collection;

                            this.WriteNextChunk(listChunk);
                        }
                        else
                        {
                            foreach (var item in collection)
                            {
                                this.WriteNextChunk(item);
                            }
                        }
                    }
                    else
                    {
                        this.WriteNextChunk(childObject);
                    }
                }
            }
            else
            {
                objectReader.WriteFields(this);
            }
        }

        /// <summary>
        /// Gets the (file) stream for writing.
        /// </summary>
        public Stream CurrentStream
        {
            get
            {
                if (this.context.HeaderStack.Count > 0)
                {
                    return this.context.HeaderStack.Peek().DataStream;
                }

                return this.context.ChunkFile.BaseStream;
            }
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteByte(byte value)
        {
            var writer = new BinaryWriter(this.CurrentStream);

            writer.Write(value);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteChar(char value)
        {
            var writer = new BinaryWriter(this.CurrentStream);

            writer.Write(value);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteInt16(short value)
        {
            this.numberWriter.WriteInt16(value, this.CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteInt32(int value)
        {
            this.numberWriter.WriteInt32(value, this.CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteInt64(long value)
        {
            this.numberWriter.WriteInt64(value, this.CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteString(string value)
        {
            this.stringWriter.WriteString(this.CurrentStream, value);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteFourCharacterCode(FourCharacterCode value)
        {
            if (value != null)
            {
                value.WriteTo(this.CurrentStream);
            }
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        [CLSCompliant(false)]
        public void WriteUInt16(ushort value)
        {
            this.numberWriter.WriteInt16((short)value, this.CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        [CLSCompliant(false)]
        public void WriteUInt32(uint value)
        {
            this.numberWriter.WriteInt32((int)value, this.CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        [CLSCompliant(false)]
        public void WriteUInt64(ulong value)
        {
            this.numberWriter.WriteInt64((long)value, this.CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public void WriteBuffer(byte[] value)
        {
            Check.IfArgumentNull(value, "value");

            this.CurrentStream.Write(value, 0, value.Length);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public void WriteStream(Stream value)
        {
            Check.IfArgumentNull(value, "value");
            if (!value.CanRead)
            {
                throw new ArgumentException("Cannot read from stream.", "value");
            }
            if (!value.CanSeek && value.Position != 0)
            {
                throw new ArgumentException("Cannot seek in stream.", "value");
            }

            if (value.CanSeek)
            {
                value.Position = 0;
            }

            value.CopyTo(this.CurrentStream);
        }
    }
}