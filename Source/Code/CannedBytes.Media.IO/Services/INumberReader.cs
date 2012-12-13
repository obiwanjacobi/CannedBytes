namespace CannedBytes.Media.IO.Services
{
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// A general interface for reading numbers from a <see cref="Stream"/>.
    /// </summary>
    [ContractClass(typeof(NumberReaderContract))]
    public interface INumberReader
    {
        /// <summary>
        /// Reads a 16 bit integer from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the number value read.</returns>
        short ReadInt16(Stream stream);

        /// <summary>
        /// Reads a 32 bit integer from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the number value read.</returns>
        int ReadInt32(Stream stream);

        /// <summary>
        /// Reads a 64 bit integer from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the number value read.</returns>
        long ReadInt64(Stream stream);

        /// <summary>
        /// Reads a 16 bit integer from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the number value read as 32 bits.</returns>
        int ReadUInt16AsInt32(Stream stream);

        /// <summary>
        /// Reads a 32 bit integer from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the number value read as 64 bits.</returns>
        long ReadUInt32AsInt64(Stream stream);
    }
}