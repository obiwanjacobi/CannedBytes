using System;
using System.IO;

namespace CannedBytes.IO
{
    public class WriteOnlyStream : StreamWrapper
    {
        public WriteOnlyStream(Stream stream)
            : base(stream)
        {
            ValidateStreamIsWritable(stream);
        }

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

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return false; }
        }
    }
}