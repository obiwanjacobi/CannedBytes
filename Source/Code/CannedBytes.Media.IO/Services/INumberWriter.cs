namespace CannedBytes.Media.IO.Services
{
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// General interface for writing numbers to a <see cref="Stream"/>.
    /// </summary>
    [ContractClass(typeof(NumberWriterContract))]
    public interface INumberWriter
    {
        /// <summary>
        /// Writes the <paramref name="value"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        /// <param name="stream">Must not be null.</param>
        void WriteInt16(short value, Stream stream);

        /// <summary>
        /// Writes the <paramref name="value"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        /// <param name="stream">Must not be null.</param>
        void WriteInt16(int value, Stream stream);

        /// <summary>
        /// Writes the <paramref name="value"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        /// <param name="stream">Must not be null.</param>
        void WriteInt32(int value, Stream stream);

        /// <summary>
        /// Writes the <paramref name="value"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        /// <param name="stream">Must not be null.</param>
        void WriteInt32(long value, Stream stream);

        /// <summary>
        /// Writes the <paramref name="value"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        /// <param name="stream">Must not be null.</param>
        void WriteInt64(long value, Stream stream);
    }
}