namespace CannedBytes.Media.IO
{
    using CannedBytes.IO;
    using CannedBytes.Media.IO.Services;
    using System;
    using System.IO;

    /// <summary>
    /// Implements the reading of file chunks, the creation of runtime objects and the serialization process.
    /// </summary>
    public class FileChunkReader
    {
        /// <summary>
        /// Private reference to the file context.
        /// </summary>
        private ChunkFileContext _context;

        ////warning CS0649: Field 'X' is never assigned to, and will always have its default value null
#pragma warning disable 0649
        /// <summary>
        /// Optional reference to the stream navigator.
        /// </summary>
//        [Import(AllowDefault = true, AllowRecomposition = true)]
        private IStreamNavigator _streamNavigator;

        /// <summary>
        /// Private reference to the chunk type factory.
        /// </summary>
//        [Import]
        private IChunkTypeFactory _chunkTypeFactory;

        /// <summary>
        /// Private reference to the chunk handler manager.
        /// </summary>
//        [Import]
        private FileChunkHandlerManager _handlerMgr;

        /// <summary>
        /// Private reference to the string reader.
        /// </summary>
        //[Import]
        private IStringReader _stringReader;

        /// <summary>
        /// Private reference to the number reader.
        /// </summary>
        //[Import]
        private INumberReader _numberReader;
#pragma warning restore 0649

        /// <summary>
        /// Constructs a new instance on the specified file <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        public FileChunkReader(ChunkFileContext context)
        {
            Check.IfArgumentNull(context, "context");
            Check.IfArgumentNull(context.Services, "context.CompositionContainer");

            //context.Container.ComposeParts(this);
            //context.Container.AddInstance(this);

            _context = context;
        }

        /// <summary>
        /// Reads the next chunk from the file.
        /// </summary>
        /// <returns>Returns null when there was no runtime chunk type found to represent the chunk read.</returns>
        public object ReadNextChunk()
        {
            var stream = CurrentStream;

            if (!CurrentStreamCanRead)
            {
                // TODO: should we try to pop the current chunk first?
                stream = _context.ChunkFile.BaseStream;
            }

            if (stream == null)
            {
                throw new ChunkFileException("No valid stream was found to read from.");
            }

            return ReadNextChunk(stream);
        }

        /// <summary>
        /// Reads the next chunk from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns null when there was no runtime chunk type found to represent the chunk read.</returns>
        public object ReadNextChunk(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            object chunkObject = null;

            var chunk = ReadChunkHeader(stream);

            if (chunk != null)
            {
                _context.ChunkStack.PushChunk(chunk);

                var chunkHandler = _handlerMgr.GetChunkHandler(chunk.ChunkId);

                if (chunkHandler.CanRead(chunk))
                {
                    chunkObject = chunkHandler.Read(_context);

                    chunk.RuntimeInstance = chunkObject;
                }

                // makes sure all of the chunk is 'read' (skipped)
                // and aligns the stream position ready for the next chunk.
                SkipChunk(chunk);

                var poppedChunk = _context.ChunkStack.PopChunk();

                if (poppedChunk != null &&
                    !Object.ReferenceEquals(poppedChunk, chunk))
                {
                    throw new InvalidOperationException("The Chunk Stack has been corrupted.");
                }
            }

            return chunkObject;
        }

        /// <summary>
        /// Gets the stream for the current file chunk.
        /// </summary>
        public Stream CurrentStream
        {
            get
            {
                Stream stream;

                if (_context.ChunkStack.CurrentChunk == null)
                {
                    stream = _context.ChunkFile.BaseStream;
                }
                else
                {
                    stream = _context.ChunkStack.CurrentChunk.DataStream;
                }

                return stream;
            }
        }

        /// <summary>
        /// Gets an indication if bytes can be read from the <see cref="P:CurrentStream"/>.
        /// </summary>
        public bool CurrentStreamCanRead
        {
            get { return CurrentStream.CanRead && (CurrentStream.Position < CurrentStream.Length); }
        }

        /// <summary>
        /// Skips reading any remaining data for the current chunk.
        /// </summary>
        public void SkipCurrentChunk()
        {
            SkipChunk(_context.ChunkStack.CurrentChunk);
        }

        /// <summary>
        /// Skips reading any remaining data for the <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk">Must not be null.</param>
        /// <remarks>The underlying file stream is aligned using the <see cref="IStreamNavigator"/> implementation if available.</remarks>
        protected void SkipChunk(FileChunk chunk)
        {
            Check.IfArgumentNull(chunk, "chunk");
            Check.IfArgumentNull(chunk.DataStream, "chunk.DataStream");

            EndStream(chunk.DataStream);

            if (_streamNavigator != null)
            {
                // after skipping the chunk length re-align position of the root stream.
                // sub streams may refuse to move if they are at their end.
                _streamNavigator.AlignPosition(_context.ChunkFile.BaseStream);
            }
        }

        /// <summary>
        /// Reads all bytes until the <paramref name="stream"/> is ended.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <remarks>If the <paramref name="stream"/> is not seekable
        /// the bytes are read which requires a buffer allocation.</remarks>
        protected void EndStream(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.End);
            }
            else
            {
                // wasteful!
                GetRemainingCurrentChunkBuffer();
            }
        }

        /// <summary>
        /// Reads the first (header) fields of a chunk from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns null when the stream position does not allow to read the header.</returns>
        /// <remarks>The following properties are set on the returned <see cref="FileChunk"/>.
        /// <see cref="P:FileChunk.ChunkId"/>, <see cref="P:FileChunk.DataLength"/>,
        /// <see cref="P:FileChunk.ParentPosition"/>, <see cref="P:FileChunk.FilePosition"/>
        /// and <see cref="P:FileChunk.DataStream"/>.</remarks>
        public FileChunk ReadChunkHeader(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            if (ValidateStreamPosition(8))
            {
                var chunk = new FileChunk();
                chunk.ChunkId = FourCharacterCode.ReadFrom(stream);
                chunk.DataLength = _numberReader.ReadUInt32AsInt64(stream);
                chunk.ParentPosition = stream.Position;
                chunk.FilePosition = _context.ChunkFile.BaseStream.Position;
                chunk.DataStream = new SubStream(stream, chunk.DataLength);

                return chunk;
            }

            return null;
        }

        /// <summary>
        /// Creates and fill the data members of the runtime object for the specified <paramref name="chunkId"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="chunkId">Must not be null.</param>
        /// <returns>Returns null when no runtime chunk type could be found for the <paramref name="chunkId"/>.</returns>
        /// <remarks>Note that chunks (types) are never mixed content. They are either containing other chunks
        /// or the contain data. This method is for reading data.
        /// Use the <see cref="M:ReadRuntimteContainerChunkType"/> to read sub chunks.</remarks>
        public object ReadRuntimeChunkType(Stream stream, FourCharacterCode chunkId)
        {
            Check.IfArgumentNull(stream, "stream");
            Check.IfArgumentNull(chunkId, "chunkId");

            var runtimeObject = _chunkTypeFactory.CreateChunkObject(chunkId);

            if (runtimeObject != null)
            {
                var writer = new ObjectMemberWriter(runtimeObject);

                writer.ReadFields(this);
            }

            return runtimeObject;
        }

        /// <summary>
        /// Creates and fill the data members of the runtime object for the specified <paramref name="chunkId"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="chunkId">Must not be null.</param>
        /// <returns>Returns null when no runtime chunk type could be found for the <paramref name="chunkId"/>.</returns>
        /// <remarks>Note that chunks (types) are never mixed content. They are either containing other chunks
        /// or the contain data. This method is for reading sub chunks.
        /// Use the <see cref="M:ReadRuntimteChunkType"/> to read data.</remarks>
        public object ReadRuntimeContainerChunkType(Stream stream, FourCharacterCode chunkId)
        {
            Check.IfArgumentNull(stream, "stream");
            Check.IfArgumentNull(chunkId, "chunkId");

            var runtimeObject = _chunkTypeFactory.CreateChunkObject(chunkId);

            if (runtimeObject != null)
            {
                var writer = new ObjectMemberWriter(runtimeObject);

                // read all of the stream
                while (stream.Position < stream.Length)
                {
                    object runtimeChildObj = ReadNextChunk(stream);

                    // if null means there was no runtime type found.
                    if (runtimeChildObj != null)
                    {
                        // if returns false means can't find member
                        if (!writer.WriteChunkObject(runtimeChildObj))
                        {
                            // just keep reading the stream...
                        }
                    }
                }
            }

            return runtimeObject;
        }

        /// <summary>
        /// Returns an indication if <paramref name="byteCount"/> can be read from the file stream.
        /// </summary>
        /// <param name="byteCount">Must be greater or equal than zero.</param>
        /// <returns>Returns true when there is enough room in the file stream.</returns>
        protected bool ValidateStreamPosition(long byteCount)
        {
            Check.IfArgumentOutOfRange(byteCount, 0, int.MaxValue, "byteCount");

            var stream = _context.ChunkFile.BaseStream;
            long curPos = stream.Position;
            long length = stream.Length;

            return byteCount <= (length - curPos);
        }

        /// <summary>
        /// Creates a new <see cref="Stream"/> instance for the remaining chunk.
        /// </summary>
        /// <returns>Never returns null.</returns>
        /// <remarks>Based on the <see cref="P:ChunkFileContext.CopyStreams"/> property
        /// the original file <see cref="T:Stream"/> is used or an in-memory copy.</remarks>
        public Stream GetRemainingCurrentChunkSubStream()
        {
            if (_context.CopyStreams)
            {
                var buffer = GetRemainingCurrentChunkBuffer();
                return new MemoryStream(buffer, false);
            }

            var chunk = _context.ChunkStack.CurrentChunk;
            var stream = chunk.DataStream;

            var curPos = stream.Position;
            var totalLength = chunk.DataLength;

            return new SubStream(stream, totalLength - curPos);
        }

        /// <summary>
        /// Creates a buffer with the remaining chunk info.
        /// </summary>
        /// <returns>Never returns null.</returns>
        public byte[] GetRemainingCurrentChunkBuffer()
        {
            var chunk = _context.ChunkStack.CurrentChunk;
            var stream = chunk.DataStream;

            var length = (int)(chunk.DataLength - stream.Position);
            var buffer = new byte[length];

            var read = CurrentStream.Read(buffer, 0, length);

            if (length != read)
            {
                throw new EndOfStreamException();
            }

            return buffer;
        }

        /// <summary>
        /// Reads a single byte.
        /// </summary>
        /// <returns>Returns the byte read.</returns>
        public byte ReadByte()
        {
            var stream = CurrentStream;

            var value = stream.ReadByte();

            if (value == -1)
            {
                throw new EndOfStreamException();
            }

            return (byte)value;
        }

        /// <summary>
        /// Reads a single character.
        /// </summary>
        /// <returns>Returns the character read.</returns>
        public char ReadChar()
        {
            var stream = CurrentStream;

            var value = stream.ReadByte();

            if (value == -1)
            {
                throw new EndOfStreamException();
            }

            return (char)value;
        }

        /// <summary>
        /// Reads a single short.
        /// </summary>
        /// <returns>Returns the short read.</returns>
        public short ReadInt16()
        {
            var stream = CurrentStream;

            return _numberReader.ReadInt16(stream);
        }

        /// <summary>
        /// Reads a single integer.
        /// </summary>
        /// <returns>Returns the integer read.</returns>
        public int ReadInt32()
        {
            var stream = CurrentStream;

            return _numberReader.ReadInt32(stream);
        }

        /// <summary>
        /// Reads a single long.
        /// </summary>
        /// <returns>Returns the long read.</returns>
        public long ReadInt64()
        {
            var stream = CurrentStream;

            return _numberReader.ReadInt64(stream);
        }

        /// <summary>
        /// Reads a single string.
        /// </summary>
        /// <returns>Returns the string read. Never returns null.</returns>
        public string ReadString()
        {
            var stream = CurrentStream;

            return _stringReader.ReadString(stream);
        }

        /// <summary>
        /// Reads a four character code.
        /// </summary>
        /// <returns>Returns the code read. Never returns null.</returns>
        public FourCharacterCode ReadFourCharacterCode()
        {
            var stream = CurrentStream;

            return FourCharacterCode.ReadFrom(stream);
        }

        /// <summary>
        /// Reads a single unsigned short.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        public ushort ReadUInt16()
        {
            return (ushort)ReadInt16();
        }

        /// <summary>
        /// Reads a single unsigned integer.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        public uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

        /// <summary>
        /// Reads a single unsigned long.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        public ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }
    }
}