namespace CannedBytes.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Wraps an existing stream and makes it write-only.
    /// </summary>
    public class WriteOnlyStream : WrappedStream
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

        /// <summary>
        /// Throws an exception if the <paramref name="stream"/> can not be written to.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        private static void ValidateStreamIsWritable(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream cannot be written.", "stream");
            }
        }

        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Not used.")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Not used.")]
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