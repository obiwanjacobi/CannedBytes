using System;
using System.ComponentModel.Composition;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Implements the <see cref="INumberReader"/> interface for little-endian encoding.
    /// </summary>
    [Export(typeof(INumberReader))]
    public class LittleEndianNumberReader : INumberReader
    {
        /// <inheritdocs/>
        public short ReadInt16(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            return (short)ReadUInt16(stream);
        }

        /// <inheritdocs/>
        public int ReadInt32(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            return (int)ReadUInt32(stream);
        }

        /// <inheritdocs/>
        public long ReadInt64(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            byte[] buffer = new byte[8];
            int bytesRead = stream.Read(buffer, 0, 8);

            if (bytesRead < 8)
            {
                throw new EndOfStreamException();
            }

            int loWord = (((buffer[3] << 0x18) | (buffer[2] << 0x10)) | (buffer[1] << 8)) | buffer[0];
            int hiWord = (((buffer[7] << 0x18) | (buffer[6] << 0x10)) | (buffer[5] << 8)) | buffer[4];
            return (long)(((ulong)loWord) | (ulong)(hiWord << 0x20));
        }

        /// <inheritdocs/>
        public int ReadUInt16AsInt32(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");
            return (int)ReadUInt16(stream);
        }

        /// <inheritdocs/>
        public long ReadUInt32AsInt64(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            return (long)ReadUInt32(stream);
        }

        private UInt16 ReadUInt16(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            byte[] buffer = new byte[2];
            int bytesRead = stream.Read(buffer, 0, 2);

            if (bytesRead < 2)
            {
                throw new EndOfStreamException();
            }

            return (ushort)((buffer[1] << 8) | buffer[0]);
        }

        private UInt32 ReadUInt32(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            byte[] buffer = new byte[4];
            int bytesRead = stream.Read(buffer, 0, 4);

            if (bytesRead < 4)
            {
                throw new EndOfStreamException();
            }

            return (uint)((((buffer[3] << 0x18) | (buffer[2] << 0x10)) | (buffer[1] << 8)) | buffer[0]);
        }
    }
}