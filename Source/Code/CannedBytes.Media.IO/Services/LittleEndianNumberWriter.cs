using System;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    public class LittleEndianNumberWriter : INumberWriter
    {
        public void WriteInt16(short value, Stream stream)
        {
            WriteUInt16((ushort)value, stream);
        }

        public void WriteInt16(int value, Stream stream)
        {
            WriteUInt16((ushort)value, stream);
        }

        public void WriteInt32(int value, Stream stream)
        {
            WriteUInt32((uint)value, stream);
        }

        public void WriteInt32(long value, Stream stream)
        {
            WriteUInt32((uint)value, stream);
        }

        public void WriteInt64(long value, Stream stream)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            stream.Write(buffer, 0, buffer.Length);
        }

        private void WriteUInt16(UInt16 value, Stream stream)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            stream.Write(buffer, 0, buffer.Length);
        }

        private void WriteUInt32(UInt32 value, Stream stream)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            stream.Write(buffer, 0, buffer.Length);
        }
    }
}