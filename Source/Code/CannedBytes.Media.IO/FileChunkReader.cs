namespace CannedBytes.Media.IO
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using CannedBytes.IO;
    using CannedBytes.Media.IO.Services;

    /// <summary>
    /// Implements the reading of file chunks, the creation of runtime objects and the serialization process.
    /// </summary>
    public class FileChunkReader
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
        /// Private reference to the chunk type factory.
        /// </summary>
        [Import]
        private IChunkTypeFactory chunkTypeFactory;

        /// <summary>
        /// Private reference to the chunk handler manager.
        /// </summary>
        [Import]
        private FileChunkHandlerManager handlerMgr;

        /// <summary>
        /// Private reference to the string reader.
        /// </summary>
        [Import]
        private IStringReader stringReader;

        /// <summary>
        /// Private reference to the number reader.
        /// </summary>
        [Import]
        private INumberReader numberReader;
#pragma warning restore 0649

        /// <summary>
        /// Constructs a new instance on the specified file <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Must not be null.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public FileChunkReader(ChunkFileContext context)
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
        /// Reads the next chunk from the file.
        /// </summary>
        /// <returns>Returns null when there was no runtime chunk type found to represent the chunk read.</returns>
        public object ReadNextChunk()
        {
            var stream = this.CurrentStream;

            if (!this.CurrentStreamCanRead)
            {
                // TODO: should we try to pop the current chunk first?
                stream = this.context.ChunkFile.BaseStream;
            }

            if (stream == null)
            {
                throw new ChunkFileException("No valid stream was found to read from.");
            }

            return this.ReadNextChunk(stream);
        }

        /// <summary>
        /// Reads the next chunk from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns null when there was no runtime chunk type found to represent the chunk read.</returns>
        public object ReadNextChunk(Stream stream)
        {
            Contract.Requires(stream != null);
            Check.IfArgumentNull(stream, "stream");

            object chunkObject = null;

            var chunk = this.ReadChunkHeader(stream);

            if (chunk != null)
            {
                this.context.ChunkStack.PushChunk(chunk);

                var chunkHandler = this.handlerMgr.GetChunkHandler(chunk.ChunkId);

                if (chunkHandler.CanRead(chunk))
                {
                    chunkObject = chunkHandler.Read(this.context);

                    chunk.RuntimeInstance = chunkObject;
                }

                // makes sure all of the chunk is 'read' (skipped)
                // and aligns the stream position ready for the next chunk.
                this.SkipChunk(chunk);

                var poppedChunk = this.context.ChunkStack.PopChunk();

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

                if (this.context.ChunkStack.CurrentChunk == null)
                {
                    stream = this.context.ChunkFile.BaseStream;
                }
                else
                {
                    stream = this.context.ChunkStack.CurrentChunk.DataStream;
                }

                return stream;
            }
        }

        /// <summary>
        /// Gets an indication if bytes can be read from the <see cref="P:CurrentStream"/>.
        /// </summary>
        public bool CurrentStreamCanRead
        {
            get { return this.CurrentStream.CanRead && (this.CurrentStream.Position < this.CurrentStream.Length); }
        }

        /// <summary>
        /// Skips reading any remaining data for the current chunk.
        /// </summary>
        public void SkipCurrentChunk()
        {
            this.SkipChunk(this.context.ChunkStack.CurrentChunk);
        }

        /// <summary>
        /// Skips reading any remaining data for the <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk">Must not be null.</param>
        /// <remarks>The underlying file stream is aligned using the <see cref="IStreamNavigator"/> implementation if available.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        protected void SkipChunk(FileChunk chunk)
        {
            Contract.Requires(chunk != null);
            Contract.Requires(chunk.DataStream != null);
            Check.IfArgumentNull(chunk, "chunk");
            Check.IfArgumentNull(chunk.DataStream, "chunk.DataStream");

            this.EndStream(chunk.DataStream);

            if (this.streamNavigator != null)
            {
                // after skipping the chunk length re-align position of the root stream.
                // sub streams may refuse to move if they are at their end.
                this.streamNavigator.AlignPosition(this.context.ChunkFile.BaseStream);
            }
        }

        /// <summary>
        /// Reads all bytes until the <paramref name="stream"/> is ended.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <remarks>If the <paramref name="stream"/> is not seekable
        /// the bytes are read which requires a buffer allocation.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        protected void EndStream(Stream stream)
        {
            Contract.Requires(stream != null);
            Check.IfArgumentNull(stream, "stream");

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.End);
            }
            else
            {
                // wasteful!
                this.GetRemainingCurrentChunkBuffer();
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public FileChunk ReadChunkHeader(Stream stream)
        {
            Contract.Requires(stream != null);
            Check.IfArgumentNull(stream, "stream");

            if (this.ValidateStreamPosition(8))
            {
                var chunk = new FileChunk();
                chunk.ChunkId = FourCharacterCode.ReadFrom(stream);
                chunk.DataLength = this.numberReader.ReadUInt32AsInt64(stream);
                chunk.ParentPosition = stream.Position;
                chunk.FilePosition = this.context.ChunkFile.BaseStream.Position;
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
            Contract.Requires(stream != null);
            Contract.Requires(chunkId != null);
            Check.IfArgumentNull(stream, "stream");
            Check.IfArgumentNull(chunkId, "chunkId");

            var runtimeObject = this.chunkTypeFactory.CreateChunkObject(chunkId);

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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public object ReadRuntimeContainerChunkType(Stream stream, FourCharacterCode chunkId)
        {
            Contract.Requires(stream != null);
            Contract.Requires(chunkId != null);
            Check.IfArgumentNull(stream, "stream");
            Check.IfArgumentNull(chunkId, "chunkId");

            var runtimeObject = this.chunkTypeFactory.CreateChunkObject(chunkId);

            if (runtimeObject != null)
            {
                var writer = new ObjectMemberWriter(runtimeObject);

                // read all of the stream
                while (stream.Position < stream.Length)
                {
                    object runtimeChildObj = this.ReadNextChunk(stream);

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
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "The method validates the number of bytes.")]
        protected bool ValidateStreamPosition(long byteCount)
        {
            Contract.Requires(byteCount >= 0);
            Check.IfArgumentOutOfRange(byteCount, 0, int.MaxValue, "byteCount");

            var stream = this.context.ChunkFile.BaseStream;
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The MemoryStream is not an issue.")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creating a sub-stream is not something for in a property getter.")]
        public Stream GetRemainingCurrentChunkSubStream()
        {
            Contract.Ensures(Contract.Result<Stream>() != null);

            if (this.context.CopyStreams)
            {
                var buffer = this.GetRemainingCurrentChunkBuffer();
                return new MemoryStream(buffer, false);
            }

            var chunk = this.context.ChunkStack.CurrentChunk;
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
            Contract.Ensures(Contract.Result<byte[]>() != null);

            var chunk = this.context.ChunkStack.CurrentChunk;
            var stream = chunk.DataStream;

            var length = (int)(chunk.DataLength - stream.Position);
            var buffer = new byte[length];

            var read = this.CurrentStream.Read(buffer, 0, length);

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
            var stream = this.CurrentStream;

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
            var stream = this.CurrentStream;

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
            var stream = this.CurrentStream;

            return this.numberReader.ReadInt16(stream);
        }

        /// <summary>
        /// Reads a single integer.
        /// </summary>
        /// <returns>Returns the integer read.</returns>
        public int ReadInt32()
        {
            var stream = this.CurrentStream;

            return this.numberReader.ReadInt32(stream);
        }

        /// <summary>
        /// Reads a single long.
        /// </summary>
        /// <returns>Returns the long read.</returns>
        public long ReadInt64()
        {
            var stream = this.CurrentStream;

            return this.numberReader.ReadInt64(stream);
        }

        /// <summary>
        /// Reads a single string.
        /// </summary>
        /// <returns>Returns the string read. Never returns null.</returns>
        public string ReadString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            var stream = this.CurrentStream;

            return this.stringReader.ReadString(stream);
        }

        /// <summary>
        /// Reads a four character code.
        /// </summary>
        /// <returns>Returns the code read. Never returns null.</returns>
        public FourCharacterCode ReadFourCharacterCode()
        {
            Contract.Ensures(Contract.Result<FourCharacterCode>() != null);

            var stream = this.CurrentStream;

            return FourCharacterCode.ReadFrom(stream);
        }

        /// <summary>
        /// Reads a single unsigned short.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        [CLSCompliant(false)]
        public ushort ReadUInt16()
        {
            return (ushort)this.ReadInt16();
        }

        /// <summary>
        /// Reads a single unsigned integer.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        [CLSCompliant(false)]
        public uint ReadUInt32()
        {
            return (uint)this.ReadInt32();
        }

        /// <summary>
        /// Reads a single unsigned long.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        [CLSCompliant(false)]
        public ulong ReadUInt64()
        {
            return (ulong)this.ReadInt64();
        }
    }
}