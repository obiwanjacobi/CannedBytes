using System;
using System.IO;

namespace CannedBytes.IO
{
    /// <summary>
    /// The SubStream wraps an existing stream and only allows access to a part of that wrapped stream.
    /// </summary>
    public class SubStream : StreamWrapper
    {
        private long offset;
        private long length;

        /// <summary>
        /// Instantiates a new instance from the start of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        public SubStream(Stream stream, long length)
            : base(stream)
        {
            this.offset = stream.Position;
            SetSubLength(length);
        }

        /// <summary>
        /// Instantiates a new seekable instance from the start of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        /// <param name="canSeek">False to prohibit seeking.</param>
        public SubStream(Stream stream, bool canSeek, long length)
            : base(stream, canSeek)
        {
            this.offset = stream.Position;
            SetSubLength(length);
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
            SetOffset(offset);
            SetSubLength(length);
        }

        /// <summary>
        /// Instantiates a new instance from <paramref name="offset"/> of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="offset">The offset in bytes from the start of <paramref name="stream"/>.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        /// <param name="canSeek">False to prohibit seeking.</param>
        public SubStream(Stream stream, bool canSeek, long offset, long length)
            : base(stream, canSeek)
        {
            SetOffset(offset);
            SetSubLength(length);
        }

        private void SetOffset(long offset)
        {
            if (offset >= this.InnerStream.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            this.offset = offset;
        }

        private void SetSubLength(long length)
        {
            if ((this.offset + length) > base.Length)
            {
                this.length = base.Length - this.offset;
            }
            else
            {
                this.length = length;
            }
        }

        private bool AdjustCount(ref int count)
        {
            if ((base.Position + count) > (this.offset + this.length))
            {
                count = (int)((this.offset + this.length) - base.Position);
            }

            return (count > 0);
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!AdjustCount(ref count))
            {
                throw new ArgumentException("count");
            }

            return base.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!AdjustCount(ref count))
            {
                throw new ArgumentException("count");
            }

            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (AdjustCount(ref count))
            {
                return base.Read(buffer, offset, count);
            }

            return 0;
        }

        /// <inheritdocs/>
        public override int ReadByte()
        {
            int length = 1;

            if (AdjustCount(ref length))
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
                    absoluteOffset = this.offset + offset;
                    break;
                case SeekOrigin.Current:
                    absoluteOffset = base.Position + offset;
                    break;
                case SeekOrigin.End:
                    absoluteOffset = this.offset + this.length - Math.Abs(offset);
                    break;
            }

            if (absoluteOffset < this.offset)
            {
                absoluteOffset = this.offset;
            }
            else if (absoluteOffset > (this.offset + this.length))
            {
                absoluteOffset = this.offset + this.length;
            }

            return base.Seek(absoluteOffset, SeekOrigin.Begin);
        }

        /// <inheritdocs/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdocs/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (AdjustCount(ref count))
            {
                InnerStream.Write(buffer, offset, count);
            }
        }

        /// <inheritdocs/>
        public override void WriteByte(byte value)
        {
            int length = 1;

            if (AdjustCount(ref length))
            {
                base.WriteByte(value);
            }
        }

        /// <inheritdocs/>
        public override long Length
        {
            get { return this.length; }
        }

        /// <inheritdocs/>
        public override long Position
        {
            get
            {
                // maxed out
                if ((base.Position - this.offset) >= this.length)
                {
                    return this.length;
                }

                return base.Position - this.offset;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }
    }
}