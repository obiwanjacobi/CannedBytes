using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using CannedBytes.IO;
using CannedBytes.Media.IO.Services;

namespace CannedBytes.Media.IO
{
    public class FileChunkReader
    {
        ChunkFileContext context;

        [Import]
        IStreamNavigator streamNavigator;
        [Import]
        IChunkTypeFactory chunkTypeFactory;
        [Import]
        FileChunkHandlerManager handlerMgr;
        [Import]
        IStringReader stringReader;
        [Import]
        INumberReader numberReader;

        public FileChunkReader(ChunkFileContext context)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<InvalidDataException>(context.CompositionContainer != null,
                "The CompositionContainer must be set on the FileContext.");

            context.CompositionContainer.ComposeParts(this);
            context.CompositionContainer.AddInstance(this);

            this.context = context;
        }

        public object ReadNextChunk()
        {
            var stream = CurrentStream;

            if (!CurrentStreamCanRead)
            {
                // TODO: should we try to pop the current chunk first?
                stream = this.context.ChunkFile.BaseStream;
            }

            return ReadNextChunk(stream);
        }

        public object ReadNextChunk(Stream stream)
        {
            object chunkObject = null;

            var chunk = ReadChunkHeader(stream);

            if (chunk != null)
            {
                this.context.ChunkStack.PushChunk(chunk);

                var chunkHandler = handlerMgr.GetChunkHandler(chunk.ChunkId);

                if (chunkHandler.CanRead(chunk))
                {
                    chunkObject = chunkHandler.Read(this.context);

                    chunk.RuntimeInstance = chunkObject;
                }

                // makes sure all of the chunk is 'read' (skipped)
                // and aligns the stream position ready for the next chunk.
                SkipChunk(chunk);

                var poppedChunk = this.context.ChunkStack.PopChunk();

                if (poppedChunk != null &&
                    !Object.ReferenceEquals(poppedChunk, chunk))
                {
                    throw new InvalidOperationException("The ChunkStack has been corrupted.");
                }
            }

            return chunkObject;
        }

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

        public bool CurrentStreamCanRead
        {
            get
            {
                return (CurrentStream.CanRead && (CurrentStream.Position < CurrentStream.Length));
            }
        }

        public void SkipCurrentChunk()
        {
            SkipChunk(this.context.ChunkStack.CurrentChunk);
        }

        protected void SkipChunk(FileChunk chunk)
        {
            EndStream(chunk.DataStream);
        }

        protected void EndStream(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.End);
            }
            else
            {
                // wasteful!
                GetRemainingCurrentChunkBuffer();
            }

            // after skipping the chunk length re-align position of the root stream.
            // sub streams may refuse to move if they are at their end.
            this.streamNavigator.AllignPosition(this.context.ChunkFile.BaseStream);
        }

        public FileChunk ReadChunkHeader(Stream stream)
        {
            if (ValidateStreamPosition(8))
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

        public object ReadRuntimeChunkType(Stream stream, FourCharacterCode chunkId, bool ignoreChunks)
        {
            var runtimeObject = this.chunkTypeFactory.CreateChunkObject(chunkId);

            if (runtimeObject != null)
            {
                var writer = new ObjectMemberWriter(runtimeObject);

                writer.ReadFields(this, ignoreChunks);
            }

            return runtimeObject;
        }

        public object ReadRuntimeContainerChunkType(Stream stream, FourCharacterCode chunkId)
        {
            var runtimeObject = this.chunkTypeFactory.CreateChunkObject(chunkId);

            if (runtimeObject != null)
            {
                var writer = new ObjectMemberWriter(runtimeObject);

                object rtChildObj = ReadNextChunk(stream);

                // read all of the stream
                while (stream.Position < stream.Length)
                {
                    // if null means there was no runtime type found.
                    if (rtChildObj != null)
                    {
                        // if returns false means can't find member
                        if (!writer.WriteNextChunkObject(rtChildObj))
                        {
                            // just keep reading the stream...
                        }
                    }

                    rtChildObj = ReadNextChunk(stream);
                }

                //while (stream.Position < stream.Length)
                //{
                //    object rtChildObj = null;
                //    do
                //    {
                //        rtChildObj = ReadNextChunk(stream);
                //        if (rtChildObj != null)
                //        {
                //            if (!writer.WriteNextChunkObject(rtChildObj))
                //            {
                //                // could not be written to object
                //                // skip the rest of current chunk
                //                EndStream(stream);
                //                // will break loops
                //            }
                //        }
                //    }
                //    while (rtChildObj == null && stream.Position < stream.Length);
                //}
            }

            return runtimeObject;
        }

        protected bool ValidateStreamPosition(long bytesToRead)
        {
            var stream = this.context.ChunkFile.BaseStream;
            long curPos = stream.Position;
            long length = stream.Length;

            return (bytesToRead <= (length - curPos));
        }

        public Stream GetRemainingCurrentChunkSubStream()
        {
            var chunk = this.context.ChunkStack.CurrentChunk;
            var stream = chunk.DataStream;

            var curPos = stream.Position;
            var totalLength = chunk.DataLength;

            return new SubStream(stream, totalLength - curPos);
        }

        public byte[] GetRemainingCurrentChunkBuffer()
        {
            var chunk = this.context.ChunkStack.CurrentChunk;
            var stream = chunk.DataStream;

            var length = (int)(chunk.DataLength - stream.Position);
            var buffer = new byte[length];

            var read = this.CurrentStream.Read(buffer, 0, length);

            return buffer;
        }

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

        public short ReadInt16()
        {
            var stream = CurrentStream;

            return numberReader.ReadInt16(stream);
        }

        public int ReadInt32()
        {
            var stream = CurrentStream;

            return numberReader.ReadInt32(stream);
        }

        public long ReadInt64()
        {
            var stream = CurrentStream;

            return numberReader.ReadInt64(stream);
        }

        public decimal ReadDecimal()
        {
            throw new NotImplementedException();
        }

        public double ReadDouble()
        {
            throw new NotImplementedException();
        }

        public float ReadSingle()
        {
            throw new NotImplementedException();
        }

        public string ReadString()
        {
            var stream = CurrentStream;

            return stringReader.ReadString(stream);
        }

        public FourCharacterCode ReadFourCharacterCode()
        {
            var stream = CurrentStream;

            return FourCharacterCode.ReadFrom(stream);
        }

        [CLSCompliant(false)]
        public UInt16 ReadUInt16()
        {
            return (UInt16)ReadInt16();
        }

        [CLSCompliant(false)]
        public UInt32 ReadUInt32()
        {
            return (UInt32)ReadInt32();
        }

        [CLSCompliant(false)]
        public UInt64 ReadUInt64()
        {
            return (UInt64)ReadInt64();
        }
    }
}