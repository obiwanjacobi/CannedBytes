namespace CannedBytes.Media.IO.Services
{
#if NET4
    using System.ComponentModel.Composition;
#else
    using System.Composition;
#endif

    using System.IO;

    /// <summary>
    /// An implementation of the <see cref="INumberReader"/> for big-endian encoding.
    /// </summary>
    [Export(typeof(INumberReader))]
    public class BigEndianNumberReader : INumberReader
    {
        /// <inheritdocs/>
        public short ReadInt16(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            return (short)ReadUInt16(stream);
        }

        /// <inheritdocs/>
        public int ReadInt32(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            return (int)ReadUInt32(stream);
        }

        /// <inheritdocs/>
        public long ReadInt64(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            byte[] buffer = new byte[8];
            int bytesRead = stream.Read(buffer, 0, 8);

            if (bytesRead < 8)
            {
                throw new EndOfStreamException();
            }

            ulong highWord = (ulong)((((buffer[0] << 0x18) | (buffer[1] << 0x10)) | (buffer[2] << 8)) | buffer[3]);
            ulong lowWord = (ulong)((((buffer[4] << 0x18) | (buffer[5] << 0x10)) | (buffer[6] << 8)) | buffer[7]);
            return (long)(lowWord | (highWord << 0x20));
        }

        /// <inheritdocs/>
        public int ReadUInt16AsInt32(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            return (int)ReadUInt16(stream);
        }

        /// <inheritdocs/>
        public long ReadUInt32AsInt64(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            return (long)ReadUInt32(stream);
        }

        /// <summary>
        /// Reads an unsigned integer value from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the value read.</returns>
        private static ushort ReadUInt16(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            byte[] buffer = new byte[2];
            int bytesRead = stream.Read(buffer, 0, 2);

            if (bytesRead < 2)
            {
                throw new EndOfStreamException();
            }

            return (ushort)((buffer[0] << 8) | buffer[1]);
        }

        /// <summary>
        /// Reads an unsigned integer from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the value read.</returns>
        private static uint ReadUInt32(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");
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