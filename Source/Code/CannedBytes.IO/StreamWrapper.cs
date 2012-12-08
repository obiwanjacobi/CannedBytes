using System;
using System.IO;

namespace CannedBytes.IO
{
    /// <summary>
    /// The StreamWrapper class can be used by derived classes to implement
    /// specific functionality over an existing stream instance.
    /// </summary>
    /// <remarks>Most method implementations pass the call to the <see cref="P:InnerStream"/>.</remarks>
    public abstract class StreamWrapper : Stream
    {
        private Stream stream;
        private bool canSeek;

        /// <summary>
        /// Creates a seekable stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        protected StreamWrapper(Stream stream)
            : this(stream, true)
        { }

        /// <summary>
        /// Creates a new stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">Overrides if the stream will allow seeking.</param>
        /// <remarks>Passing true to <paramref name="canSeek"/> will not guarantee the stream can seek -
        /// which also depends on the wrapped <paramref name="stream"/>- but it will not prohibit it.</remarks>
        protected StreamWrapper(Stream stream, bool canSeek)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.stream = stream;
            this.canSeek = canSeek;
        }

        /// <summary>
        /// Gets a reference to the wrapped stream.
        /// </summary>
        protected Stream InnerStream
        {
            get { return this.stream; }
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override void Close()
        {
            this.stream.Close();
        }

        /// <inheritdocs/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.stream.EndRead(asyncResult);
        }

        /// <inheritdocs/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.stream.EndWrite(asyncResult);
        }

        /// <inheritdocs/>
        public override void Flush()
        {
            this.stream.Flush();
        }

        /// <inheritdocs/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.Read(buffer, offset, count);
        }

        /// <inheritdocs/>
        public override int ReadByte()
        {
            return this.stream.ReadByte();
        }

        /// <inheritdocs/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this.canSeek)
            {
                throw new NotSupportedException();
            }

            return this.stream.Seek(offset, origin);
        }

        /// <inheritdocs/>
        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        /// <inheritdocs/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.Write(buffer, offset, count);
        }

        /// <inheritdocs/>
        public override void WriteByte(byte value)
        {
            this.stream.WriteByte(value);
        }

        /// <inheritdocs/>
        public override bool CanRead
        {
            get { return this.stream.CanRead; }
        }

        /// <inheritdocs/>
        public override bool CanSeek
        {
            get
            {
                if (this.canSeek)
                {
                    return this.stream.CanSeek;
                }

                return false;
            }
        }

        /// <inheritdocs/>
        public override bool CanWrite
        {
            get { return this.stream.CanWrite; }
        }

        /// <inheritdocs/>
        public override long Length
        {
            get { return this.stream.Length; }
        }

        /// <inheritdocs/>
        public override long Position
        {
            get { return this.stream.Position; }
            set
            {
                if (this.canSeek == false)
                {
                    throw new NotSupportedException();
                }

                this.stream.Position = value;
            }
        }
    }
}