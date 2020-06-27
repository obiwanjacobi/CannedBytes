namespace CannedBytes.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// The WrappedStream class can be used by derived classes to implement
    /// specific functionality over an existing stream instance.
    /// </summary>
    /// <remarks>Most method implementations pass the call to the <see cref="P:InnerStream"/>.</remarks>
    public abstract class WrappedStream : Stream
    {
        /// <summary>The wrapped stream.</summary>
        private readonly Stream _stream;

        /// <summary>Indication if the stream is seekable.</summary>
        private readonly bool _canSeek;

        /// <summary>
        /// Creates a seekable stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        protected WrappedStream(Stream stream)
            : this(stream, true)
        {
        }

        /// <summary>
        /// Creates a new stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">Overrides if the stream will allow seeking.</param>
        /// <remarks>Passing true to <paramref name="canSeek"/> will not guarantee the stream can seek -
        /// which also depends on the wrapped <paramref name="stream"/>- but it will not prohibit it.</remarks>
        protected WrappedStream(Stream stream, bool canSeek)
        {
            Check.IfArgumentNull(stream, "stream");

            _stream = stream;
            _canSeek = canSeek;
        }

        /// <summary>
        /// Gets a reference to the wrapped stream.
        /// </summary>
        protected Stream InnerStream
        {
            get { return _stream; }
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Check.IfArgumentNull(buffer, "buffer");

            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Check.IfArgumentNull(buffer, "buffer");

            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override void Close()
        {
            _stream.Close();
        }

        /// <inheritdocs/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return _stream.EndRead(asyncResult);
        }

        /// <inheritdocs/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            _stream.EndWrite(asyncResult);
        }

        /// <inheritdocs/>
        public override void Flush()
        {
            _stream.Flush();
        }

        /// <inheritdocs/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Check.IfArgumentNull(buffer, "buffer");

            return _stream.Read(buffer, offset, count);
        }

        /// <inheritdocs/>
        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        /// <inheritdocs/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_canSeek)
            {
                throw new NotSupportedException();
            }

            return _stream.Seek(offset, origin);
        }

        /// <inheritdocs/>
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        /// <inheritdocs/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Check.IfArgumentNull(buffer, "buffer");

            _stream.Write(buffer, offset, count);
        }

        /// <inheritdocs/>
        public override void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        /// <inheritdocs/>
        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        /// <inheritdocs/>
        public override bool CanSeek
        {
            get
            {
                if (_canSeek)
                {
                    return _stream.CanSeek;
                }

                return false;
            }
        }

        /// <inheritdocs/>
        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        /// <inheritdocs/>
        public override long Length
        {
            get { return _stream.Length; }
        }

        /// <inheritdocs/>
        public override long Position
        {
            get
            {
                return _stream.Position;
            }

            set
            {
                if (!_canSeek)
                {
                    throw new NotSupportedException();
                }

                _stream.Position = value;
            }
        }
    }
}