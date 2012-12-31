namespace CannedBytes.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// The SubStream wraps an existing stream and only allows access to a part of that wrapped stream.
    /// </summary>
    public class SubStream : WrappedStream
    {
        /// <summary>The offset into the base stream where the sub-stream starts.</summary>
        private long streamOffset;

        /// <summary>The length of the sub-stream.</summary>
        private long subStreamLength;

        /// <summary>
        /// Instantiates a new instance from the start of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public SubStream(Stream stream, long length)
            : base(stream)
        {
            Check.IfArgumentNull(stream, "stream");

            this.streamOffset = stream.Position;
            this.SetSubLength(length);
        }

        /// <summary>
        /// Instantiates a new seekable instance from the start of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">False to prohibit seeking.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public SubStream(Stream stream, bool canSeek, long length)
            : base(stream, canSeek)
        {
            Check.IfArgumentNull(stream, "stream");

            this.streamOffset = stream.Position;
            this.SetSubLength(length);
        }

        /// <summary>
        /// Instantiates a new seekable instance from <paramref name="offset"/> of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="offset">The offset in bytes from the start of <paramref name="stream"/>.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        public SubStream(Stream stream, long offset, long length)
            : base(stream, true)
        {
            this.SetStreamOffset(offset);
            this.SetSubLength(length);
        }

        /// <summary>
        /// Instantiates a new instance from <paramref name="offset"/> of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">False to prohibit seeking.</param>
        /// <param name="offset">The offset in bytes from the start of <paramref name="stream"/>.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        public SubStream(Stream stream, bool canSeek, long offset, long length)
            : base(stream, canSeek)
        {
            this.SetStreamOffset(offset);
            this.SetSubLength(length);
        }

        /// <summary>
        /// Initializes the sub-stream offset.
        /// </summary>
        /// <param name="offset">Must be within range.</param>
        private void SetStreamOffset(long offset)
        {
            if (offset > 0 && offset >= this.InnerStream.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            this.streamOffset = offset;
        }

        /// <summary>
        /// Initializes the sub-stream length.
        /// </summary>
        /// <param name="length">Is adjusted to fit within range.</param>
        private void SetSubLength(long length)
        {
            if (length > 0 &&
                (this.streamOffset + length) > base.Length)
            {
                this.subStreamLength = base.Length - this.streamOffset;
            }
            else
            {
                this.subStreamLength = length;
            }
        }

        /// <summary>
        /// Adjusts the specified <paramref name="count"/> to stay within range.
        /// </summary>
        /// <param name="count">The parameter is in/out (ref).</param>
        /// <returns>Returns true if the (adjusted) count is greater than zero.</returns>
        private bool AdjustCount(ref int count)
        {
            if (this.subStreamLength > 0)
            {
                if ((base.Position + count) > (this.streamOffset + this.subStreamLength))
                {
                    count = (int)((this.streamOffset + this.subStreamLength) - base.Position);
                }
            }

            return count > 0;
        }

        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Check.IfArgumentNull(buffer, "buffer");

            if (!this.AdjustCount(ref count))
            {
                throw new ArgumentOutOfRangeException("count", count, "The count does not lie within range of this sub-stream's length.");
            }

            return base.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Check.IfArgumentNull(buffer, "buffer");

            if (!this.AdjustCount(ref count))
            {
                throw new ArgumentOutOfRangeException("count", count, "The count does not lie within range of this sub-stream's length.");
            }

            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public override int Read(byte[] buffer, int offset, int count)
        {
            Check.IfArgumentNull(buffer, "buffer");

            if (this.AdjustCount(ref count))
            {
                return base.Read(buffer, offset, count);
            }

            return 0;
        }

        /// <inheritdocs/>
        public override int ReadByte()
        {
            int length = 1;

            if (this.AdjustCount(ref length))
            {
                return base.ReadByte();
            }

            return -1;
        }

        /// <inheritdocs/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long absoluteOffset = 0;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    absoluteOffset = this.streamOffset + offset;
                    break;
                case SeekOrigin.Current:
                    absoluteOffset = base.Position + offset;
                    break;
                case SeekOrigin.End:
                    absoluteOffset = this.streamOffset + this.subStreamLength - Math.Abs(offset);
                    break;
            }

            if (absoluteOffset < this.streamOffset)
            {
                absoluteOffset = this.streamOffset;
            }
            else if (absoluteOffset > (this.streamOffset + this.subStreamLength))
            {
                absoluteOffset = this.streamOffset + this.subStreamLength;
            }

            return base.Seek(absoluteOffset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value">Not used.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        public override void Write(byte[] buffer, int offset, int count)
        {
            Check.IfArgumentNull(buffer, "buffer");

            if (this.AdjustCount(ref count))
            {
                InnerStream.Write(buffer, offset, count);
            }
        }

        /// <inheritdocs/>
        public override void WriteByte(byte value)
        {
            int length = 1;

            if (this.AdjustCount(ref length))
            {
                base.WriteByte(value);
            }
        }

        /// <inheritdocs/>
        public override long Length
        {
            get { return this.subStreamLength; }
        }

        /// <inheritdocs/>
        public override long Position
        {
            get
            {
                // maxed out
                if ((base.Position - this.streamOffset) >= this.subStreamLength)
                {
                    return this.subStreamLength;
                }

                return base.Position - this.streamOffset;
            }

            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }
    }
}