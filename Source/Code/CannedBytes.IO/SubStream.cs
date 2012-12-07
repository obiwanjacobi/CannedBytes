using System;
using System.IO;

namespace CannedBytes.IO
{
    public class SubStream : StreamWrapper
    {
        private long offset;
        private long count;

        public SubStream(Stream stream, long count)
            : base(stream)
        {
            this.offset = stream.Position;
            SetCount(count);
        }

        public SubStream(Stream stream, bool canSeek, long count)
            : base(stream, canSeek)
        {
            this.offset = stream.Position;
            SetCount(count);
        }

        public SubStream(Stream stream, long offset, long count)
            : base(stream)
        {
            SetOffset(offset);
            SetCount(count);
        }

        public SubStream(Stream stream, bool canSeek, long offset, long count)
            : base(stream, canSeek)
        {
            SetOffset(offset);
            SetCount(count);
        }

        private void SetOffset(long offset)
        {
            if (offset >= this.InternalStream.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.offset = offset;
        }

        private void SetCount(long count)
        {
            if ((this.offset + count) > base.Length)
            {
                this.count = base.Length - this.offset;
            }
            else
            {
                this.count = count;
            }
        }

        private bool AdjustCount(ref int count)
        {
            if ((base.Position + count) > (this.offset + this.count))
            {
                count = (int)((this.offset + this.count) - base.Position);
            }

            return (count > 0);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            AdjustCount(ref count);

            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            AdjustCount(ref count);

            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (AdjustCount(ref count))
            {
                return base.Read(buffer, offset, count);
            }

            return 0;
        }

        public override int ReadByte()
        {
            int count = 1;

            if (AdjustCount(ref count))
            {
                return base.ReadByte();
            }

            return -1;
        }

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
                    absoluteOffset = this.offset + this.count - Math.Abs(offset);
                    break;
            }

            if (absoluteOffset < this.offset)
            {
                absoluteOffset = this.offset;
            }
            else if (absoluteOffset > (this.offset + this.count))
            {
                absoluteOffset = this.offset + this.count;
            }

            return base.Seek(absoluteOffset, SeekOrigin.Begin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (AdjustCount(ref count))
            {
                InternalStream.Write(buffer, offset, count);
            }
        }

        public override void WriteByte(byte value)
        {
            int count = 1;

            if (AdjustCount(ref count))
            {
                base.WriteByte(value);
            }
        }

        public override long Length
        {
            get { return this.count; }
        }

        public override long Position
        {
            get { return base.Position - this.offset; }
            set { base.Position = this.offset + value; }
        }
    }
}