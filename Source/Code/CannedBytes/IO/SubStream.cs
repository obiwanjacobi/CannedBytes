namespace CannedBytes.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// The SubStream wraps an existing stream and only allows access to a part of that wrapped stream.
    /// </summary>
    public class SubStream : WrappedStream
    {
        /// <summary>The offset into the base stream where the sub-stream starts.</summary>
        private long _streamOffset;

        /// <summary>The length of the sub-stream.</summary>
        private long _subStreamLength;

        /// <summary>
        /// For derived classes that wish to compute the offset and length of the sub-stream themselves.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">Pass true to allow seeking in the sub-stream.</param>
        /// <remarks>The offset is initialized to the current position of the <paramref name="stream"/>
        /// but can be overruled with a call to <see cref="SetStreamOffset"/>.</remarks>
        protected SubStream(Stream stream, bool canSeek)
            : base(stream, canSeek)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            _streamOffset = stream.Position;
        }

        /// <summary>
        /// Instantiates a new instance from the start of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        public SubStream(Stream stream, long length)
            : base(stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            _streamOffset = stream.Position;
            SetSubLength(length);
        }

        /// <summary>
        /// Instantiates a new instance from the start of the <paramref name="stream"/> for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="canSeek">False to prohibit seeking.</param>
        /// <param name="length">Must be greater or equal to zero.</param>
        public SubStream(Stream stream, bool canSeek, long length)
            : base(stream, canSeek)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            _streamOffset = stream.Position;
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
            SetStreamOffset(offset);
            SetSubLength(length);
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
            SetStreamOffset(offset);
            SetSubLength(length);
        }

        /// <summary>
        /// Initializes the sub-stream offset.
        /// </summary>
        /// <param name="offset">Must be within range.</param>
        protected long SetStreamOffset(long offset)
        {
            if (offset > 0 && offset >= InnerStream.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            _streamOffset = offset;

            return _streamOffset;
        }

        /// <summary>
        /// Initializes the sub-stream length.
        /// </summary>
        /// <param name="length">Is adjusted to fit within length of the base stream.</param>
        protected long SetSubLength(long length)
        {
            if (length > 0 && base.Length > 0 &&
                (_streamOffset + length) > base.Length)
            {
                _subStreamLength = base.Length - _streamOffset;
            }
            else
            {
                _subStreamLength = length;
            }

            return _subStreamLength;
        }

        /// <summary>
        /// Adjusts the specified <paramref name="count"/> to stay within range.
        /// </summary>
        /// <param name="count">The parameter is in/out (ref).</param>
        /// <returns>Returns true if the (adjusted) count is greater than zero.</returns>
        private bool AdjustCount(ref int count)
        {
            if (_subStreamLength > 0 &&
                (base.Position + count) > (_streamOffset + _subStreamLength))
            {
                count = (int)((_streamOffset + _subStreamLength) - base.Position);
            }

            return count > 0;
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Check.IfArgumentNull(buffer, nameof(buffer));

            if (!AdjustCount(ref count))
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, 
                    "The count does not lie within range of this sub-stream's length.");
            }

            return base.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Check.IfArgumentNull(buffer, nameof(buffer));

            if (!AdjustCount(ref count))
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, 
                    "The count does not lie within range of this sub-stream's length.");
            }

            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdocs/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Check.IfArgumentNull(buffer, nameof(buffer));

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
                    absoluteOffset = _streamOffset + offset;
                    break;
                case SeekOrigin.Current:
                    absoluteOffset = base.Position + offset;
                    break;
                case SeekOrigin.End:
                    absoluteOffset = _streamOffset + _subStreamLength - Math.Abs(offset);
                    break;
            }

            if (_subStreamLength > 0)
            {
                if (absoluteOffset < _streamOffset)
                {
                    absoluteOffset = _streamOffset;
                }
                else if (absoluteOffset > (_streamOffset + _subStreamLength))
                {
                    absoluteOffset = _streamOffset + _subStreamLength;
                }
            }

            return base.Seek(absoluteOffset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value">Not used.</param>
        public override void SetLength(long value)
        {
            base.SetLength(value);
        }

        /// <inheritdocs/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Check.IfArgumentNull(buffer, nameof(buffer));

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
            get
            {
                if (_subStreamLength == 0)
                {
                    return base.Length;
                }

                return _subStreamLength;
            }
        }

        /// <inheritdocs/>
        public override long Position
        {
            get
            {
                // maxed out
                if (_subStreamLength > 0 &&
                    (base.Position - _streamOffset) >= _subStreamLength)
                {
                    return _subStreamLength;
                }

                if ((base.Position - _streamOffset) >= 0)
                {
                    return base.Position - _streamOffset;
                }

                return 0;
            }

            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }
    }
}