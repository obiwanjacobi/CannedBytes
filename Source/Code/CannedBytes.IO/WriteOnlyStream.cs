using System;
using System.IO;

namespace CannedBytes.IO
{
    /// <summary>
    /// Wraps an existing stream and makes it write-only.
    /// </summary>
    public class WriteOnlyStream : StreamWrapper
    {
        /// <summary>
        /// Instantiates a new seekable write-only stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        public WriteOnlyStream(Stream stream)
            : base(stream)
        {
            ValidateStreamIsWritable(stream);
        }

        /// <summary>
        /// Instantiates a new write-only stream.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">False to prohibit seeking.</param>
        public WriteOnlyStream(Stream stream, bool canSeek)
            : base(stream, canSeek)
        {
            ValidateStreamIsWritable(stream);
        }

        private static void ValidateStreamIsWritable(Stream stream)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream cannot be written.", "stream");
            }
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override bool CanRead
        {
            get { return false; }
        }
    }
}