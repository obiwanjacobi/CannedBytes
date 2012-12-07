using System;
using System.IO;

namespace CannedBytes.IO
{
    public class StreamWrapper : Stream
    {
        private Stream stream;
        private bool canSeek;

        protected StreamWrapper(Stream stream)
            : this(stream, true)
        { }

        protected StreamWrapper(Stream stream, bool canSeek)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.stream = stream;
            this.canSeek = canSeek;
        }

        protected Stream InternalStream
        {
            get { return this.stream; }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            this.stream.Close();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.stream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return this.stream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this.canSeek)
            {
                throw new NotSupportedException();
            }

            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.stream.WriteByte(value);
        }

        public override bool CanRead
        {
            get { return this.stream.CanRead; }
        }

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

        public override bool CanWrite
        {
            get { return this.stream.CanWrite; }
        }

        public override long Length
        {
            get { return this.stream.Length; }
        }

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