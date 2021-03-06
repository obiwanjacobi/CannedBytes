namespace CannedBytes.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// Wraps an existing stream and makes it read-only.
    /// </summary>
    public class ReadOnlyStream : WrappedStream
    {
        /// <summary>
        /// Instantiates a new seekable read-only stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        public ReadOnlyStream(Stream stream)
            : base(stream)
        {
            ValidateStreamIsReadable(stream);
        }

        /// <summary>
        /// Instantiates a new read-only stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">False to prohibit seeking.</param>
        public ReadOnlyStream(Stream stream, bool canSeek)
            : base(stream, canSeek)
        {
            ValidateStreamIsReadable(stream);
        }

        /// <summary>
        /// Throws and exception id the <paramref name="stream"/> can not be read.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        private static void ValidateStreamIsReadable(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream cannot be read.", nameof(stream));
            }
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override bool CanWrite
        {
            get { return false; }
        }
    }
}