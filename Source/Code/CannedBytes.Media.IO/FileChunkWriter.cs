namespace CannedBytes.Media.IO
{
    using CannedBytes.Media.IO.ChunkTypes;
    using CannedBytes.Media.IO.SchemaAttributes;
    using CannedBytes.Media.IO.Services;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Implements the writing of chunks.
    /// </summary>
    public class FileChunkWriter
    {
        private readonly ChunkFileContext _context;
        private readonly IStreamNavigator _streamNavigator;
        private readonly FileChunkHandlerManager _handlerMgr;
        private readonly IStringWriter _stringWriter;
        private readonly INumberWriter _numberWriter;

        /// <summary>
        /// Constructs a new writer instance.
        /// </summary>
        /// <param name="context">Must not be null. Can be reused from a read operation.</param>
        public FileChunkWriter(ChunkFileContext context)
        {
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(context.Services, "context.CompositionContainer");

            _streamNavigator = context.Services.GetService<IStreamNavigator>();
            _handlerMgr = context.Services.GetService<FileChunkHandlerManager>();
            _stringWriter = context.Services.GetService<IStringWriter>();
            _numberWriter = context.Services.GetService<INumberWriter>();
            _context = context;

            context.Services.AddService(GetType(), this);
        }

        /// <summary>
        /// Pops a header from the header stack.
        /// </summary>
        /// <returns>Returns the header popped of the stack.</returns>
        private FileChunkHeader PopHeader()
        {
            return _context.HeaderStack.Pop();
        }

        /// <summary>
        /// Pushes a new initialized header onto the stack.
        /// </summary>
        /// <param name="chunkId">Must not be null.</param>
        /// <returns>Returns the header object. Never returns null.</returns>
        private FileChunkHeader PushNewHeader(FourCharacterCode chunkId)
        {
            var header = new FileChunkHeader
            {
                DataStream = new MemoryStream(),
                ChunkId = chunkId
            };

            _context.HeaderStack.Push(header);

            return header;
        }

        /// <summary>
        /// Writes the <paramref name="chunk"/> to the file.
        /// </summary>
        /// <param name="chunk">Must not be null.</param>
        /// <returns>Returns the header information for the chunk that has just been written.</returns>
        public FileChunkHeader WriteNextChunk(object chunk)
        {
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
            var chunkHandler = _handlerMgr.GetChunkHandler(chunkTypeId);

            if (!chunkHandler.CanWrite(chunk))
            {
                var msg = String.Format(
                          CultureInfo.InvariantCulture,
                          "The chunk handler '{0}' cannot write the specified runtime object.",
                          chunkHandler.ChunkId);

                throw new ArgumentException(msg);
            }

            int stackPos = _context.HeaderStack.Count;
            var header = PushNewHeader(chunkTypeId);

            chunkHandler.Write(_context, chunk);

            // wind down the stack to the level it was before we started.
            while (_context.HeaderStack.Count > stackPos)
            {
                var poppedHeader = PopHeader();
                WriteChunkHeader(poppedHeader);
            }

            return header;
        }

        /// <summary>
        /// Writes the chunk identified by the <paramref name="header"/> to the <see cref="P:CurrentStream"/>.
        /// </summary>
        /// <param name="header">Must not be null and must NOT be on the <see cref="P:ChunkFileContext.HeaderStack"/>.</param>
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
            _numberWriter.WriteInt32(header.DataStream.Position, stream);

            // write chunk body
            header.DataStream.Position = 0;
            header.DataStream.CopyTo(stream);

            // optionally align each chunk
            if (_streamNavigator != null)
            {
                _streamNavigator.AlignPosition(stream);
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

                            WriteNextChunk(listChunk);
                        }
                        else
                        {
                            foreach (var item in collection)
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
                if (_context.HeaderStack.Count > 0)
                {
                    return _context.HeaderStack.Peek().DataStream;
                }

                return _context.ChunkFile.BaseStream;
            }
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteByte(byte value)
        {
            var writer = new BinaryWriter(CurrentStream);

            writer.Write(value);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteChar(char value)
        {
            var writer = new BinaryWriter(CurrentStream);

            writer.Write(value);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteInt16(short value)
        {
            _numberWriter.WriteInt16(value, CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteInt32(int value)
        {
            _numberWriter.WriteInt32(value, CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteInt64(long value)
        {
            _numberWriter.WriteInt64(value, CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteString(string value)
        {
            _stringWriter.WriteString(CurrentStream, value);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteFourCharacterCode(FourCharacterCode value)
        {
            if (value != null)
            {
                value.WriteTo(CurrentStream);
            }
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteUInt16(ushort value)
        {
            _numberWriter.WriteInt16((short)value, CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteUInt32(uint value)
        {
            _numberWriter.WriteInt32((int)value, CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public void WriteUInt64(ulong value)
        {
            _numberWriter.WriteInt64((long)value, CurrentStream);
        }

        /// <summary>
        /// Writes the <paramref name="buffer"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="buffer">The value to be written.</param>
        public void WriteBuffer(byte[] buffer)
        {
            Check.IfArgumentNull(buffer, "value");

            CurrentStream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes the <paramref name="stream"/> to the <see cref="CurrentStream"/>.
        /// </summary>
        /// <param name="stream">The value to be written.</param>
        public void WriteStream(Stream stream)
        {
            Check.IfArgumentNull(stream, "value");
            if (!stream.CanRead)
            {
                throw new ArgumentException("Cannot read from stream.", nameof(stream));
            }
            if (!stream.CanSeek && stream.Position != 0)
            {
                throw new ArgumentException("Cannot seek in stream.", nameof(stream));
            }

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            stream.CopyTo(CurrentStream);
        }
    }
}