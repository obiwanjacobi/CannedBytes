namespace CannedBytes.Media.IO.Services
{
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Implements the <see cref="INumberReader"/> interface for little-endian encoding.
    /// </summary>
    [Export(typeof(INumberReader))]
    public class LittleEndianNumberReader : INumberReader
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public long ReadInt64(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            byte[] buffer = new byte[8];
            int bytesRead = stream.Read(buffer, 0, 8);

            if (bytesRead < 8)
            {
                throw new EndOfStreamException();
            }

            int lowWord = (((buffer[3] << 0x18) | (buffer[2] << 0x10)) | (buffer[1] << 8)) | buffer[0];
            int highWord = (((buffer[7] << 0x18) | (buffer[6] << 0x10)) | (buffer[5] << 8)) | buffer[4];
            return (long)(((ulong)lowWord) | (ulong)(highWord << 0x20));
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
        /// Reads an unsigned integer from the <paramref name="stream"/>.
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

            return (ushort)((buffer[1] << 8) | buffer[0]);
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

            return (uint)((((buffer[3] << 0x18) | (buffer[2] << 0x10)) | (buffer[1] << 8)) | buffer[0]);
        }
    }
}