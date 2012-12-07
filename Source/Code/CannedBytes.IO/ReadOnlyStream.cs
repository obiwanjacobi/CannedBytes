using System;
using System.IO;

namespace CannedBytes.IO
{
    public class ReadOnlyStream : StreamWrapper
    {
        public ReadOnlyStream(Stream stream)
            : base(stream)
        {
            ValidateStreamIsReadable(stream);
        }

        public ReadOnlyStream(Stream stream, bool canSeek)
            : base(stream, canSeek)
        {
            ValidateStreamIsReadable(stream);
        }

        private static void ValidateStreamIsReadable(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream cannot be read.", "stream");
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }
    }
}