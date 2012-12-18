namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
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

        private FileChunkHeader PopHeader()
        {
            return this.context.HeaderStack.Pop();
        }

        private FileChunkHeader PushNewHeader(FourCharacterCode chunkId)
        {
            var header = new FileChunkHeader();
            header.DataStream = new MemoryStream();
            header.ChunkId = chunkId;

            this.context.HeaderStack.Push(header);

            return header;
        }

        public FileChunkHeader WriteNextChunk(object runtimeObject)
        {
            Contract.Requires(runtimeObject != null);
            Check.IfArgumentNull(runtimeObject, "runtimeObject");

            // find chunk id
            var chunkId = ChunkAttribute.GetChunkId(runtimeObject);

            if (String.IsNullOrEmpty(chunkId))
            {
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          "No Chunk Attribute was found for the specified runtime object of type '{0}'.",
                          runtimeObject.GetType().FullName);

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

            if (!chunkHandler.CanWrite(runtimeObject))
            {
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          "The chunk handler '{0}' cannot write the specified runtime object.",
                          chunkHandler.ChunkId);

                throw new ArgumentException(msg);
            }

            int stackPos = this.context.HeaderStack.Count;
            var header = PushNewHeader(chunkTypeId);

            chunkHandler.Write(this.context, runtimeObject);

            // wind down the stack to the level it was before we started.
            while (this.context.HeaderStack.Count > stackPos)
            {
                var poppedHeader = PopHeader();
                WriteChunkHeader(poppedHeader);
            }

            return header;
        }

        public void WriteChunkHeader(FileChunkHeader header)
        {
            Check.IfArgumentNull(header, "header");

            var stream = CurrentStream;

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
                        if (objectReader.CurrentMemberIsListChunk)
                        {
                            // insert a LIST chunk.
                            var listChunk = new ListChunk();
                            listChunk.InnerChunks = (IEnumerable<object>)childObject;

                            WriteNextChunk(listChunk);
                        }
                        else
                        {
                            foreach (var item in (IEnumerable<object>)childObject)
                            {
                                WriteNextChunk(item);
                            }
                        }
                    }
                    else
                    {
                        WriteNextChunk(childObject);
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

        public void WriteByte(byte value)
        {
            var writer = new BinaryWriter(CurrentStream);

            writer.Write(value);
        }

        public void WriteChar(char value)
        {
            var writer = new BinaryWriter(CurrentStream);

            writer.Write(value);
        }

        public void WriteInt16(short value)
        {
            this.numberWriter.WriteInt16(value, CurrentStream);
        }

        public void WriteInt32(int value)
        {
            this.numberWriter.WriteInt32(value, CurrentStream);
        }

        public void WriteInt64(long value)
        {
            this.numberWriter.WriteInt64(value, CurrentStream);
        }

        public void WriteString(string value)
        {
            this.stringWriter.WriteString(CurrentStream, value);
        }

        public void WriteFourCharacterCode(FourCharacterCode fourCC)
        {
            if (fourCC != null)
            {
                fourCC.WriteTo(CurrentStream);
            }
        }

        [CLSCompliant(false)]
        public void WriteUInt16(ushort value)
        {
            this.numberWriter.WriteInt16((short)value, CurrentStream);
        }

        [CLSCompliant(false)]
        public void WriteUInt32(uint value)
        {
            this.numberWriter.WriteInt32((int)value, CurrentStream);
        }

        [CLSCompliant(false)]
        public void WriteUInt64(ulong value)
        {
            this.numberWriter.WriteInt64((long)value, CurrentStream);
        }

        public void WriteBuffer(byte[] buffer)
        {
            Check.IfArgumentNull(buffer, "buffer");

            CurrentStream.Write(buffer, 0, buffer.Length);
        }

        public void WriteStream(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");
            if (!stream.CanRead)
            {
                throw new ArgumentException("Cannot read from stream.", "stream");
            }
            if (!stream.CanSeek && stream.Position != 0)
            {
                throw new ArgumentException("Cannot seek in stream.", "stream");
            }

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            stream.CopyTo(CurrentStream);
        }
    }
}