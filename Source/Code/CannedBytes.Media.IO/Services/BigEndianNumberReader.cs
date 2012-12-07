using System;
using System.ComponentModel.Composition;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    [Export(typeof(INumberReader))]
    public class BigEndianNumberReader : INumberReader
    {
        public short ReadInt16(Stream stream)
        {
            return (short)ReadUInt16(stream);
        }

        public int ReadInt32(Stream stream)
        {
            return (int)ReadUInt32(stream);
        }

        public long ReadInt64(Stream stream)
        {
            byte[] buffer = new byte[8];
            int bytesRead = stream.Read(buffer, 0, 8);

            if (bytesRead < 8)
            {
                throw new EndOfStreamException();
            }

            int hiWord = (((buffer[0] << 0x18) | (buffer[1] << 0x10)) | (buffer[2] << 8)) | buffer[3];
            int loWord = (((buffer[4] << 0x18) | (buffer[5] << 0x10)) | (buffer[6] << 8)) | buffer[7];
            return (long)(((ulong)loWord) | (ulong)(hiWord << 0x20));
        }

        public int ReadUInt16AsInt32(Stream stream)
        {
            return (int)ReadUInt16(stream);
        }

        public long ReadUInt32AsInt64(Stream stream)
        {
            return (long)ReadUInt32(stream);
        }

        private UInt16 ReadUInt16(Stream stream)
        {
            byte[] buffer = new byte[2];
            int bytesRead = stream.Read(buffer, 0, 2);

            if (bytesRead < 2)
            {
                throw new EndOfStreamException();
            }

            return (ushort)((buffer[0] << 8) | buffer[1]);
        }

        private UInt32 ReadUInt32(Stream stream)
        {
            byte[] buffer = new byte[4];
            int bytesRead = stream.Read(buffer, 0, 4);

            if (bytesRead < 4)
            {
                throw new EndOfStreamException();
            }

            return (uint)((((buffer[0] << 0x18) | (buffer[1] << 0x10)) | (buffer[2] << 8)) | buffer[3]);
        }
    }
}