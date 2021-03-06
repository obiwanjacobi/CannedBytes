namespace CannedBytes.Media.IO.Services
{
    using System;
    using System.IO;

    /// <summary>
    /// Implements the <see cref="INumberWriter"/> interface for a big-endian encoding.
    /// </summary>
//    [Export(typeof(INumberWriter))]
    public class BigEndianNumberWriter : INumberWriter
    {
        /// <inheritdocs/>
        public void WriteInt16(short value, Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            WriteUInt16((ushort)value, stream);
        }

        /// <inheritdocs/>
        public void WriteInt16(int value, Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            WriteUInt16((ushort)value, stream);
        }

        /// <inheritdocs/>
        public void WriteInt32(int value, Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            WriteUInt32((uint)value, stream);
        }

        /// <inheritdocs/>
        public void WriteInt32(long value, Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            WriteUInt32((uint)value, stream);
        }

        /// <inheritdocs/>
        public void WriteInt64(long value, Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);

            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes an unsigned integer to the <paramref name=nameof(stream)/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name=nameof(stream)>Must not be null.</param>
        private static void WriteUInt16(ushort value, Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);

            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes an unsigned integer to the <paramref name=nameof(stream)/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name=nameof(stream)>Must not be null.</param>
        private static void WriteUInt32(uint value, Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);

            stream.Write(buffer, 0, buffer.Length);
        }
    }
}