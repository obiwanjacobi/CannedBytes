namespace CannedBytes.Media.IO.Services
{
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// A general interface for writing strings to a <see cref="Stream"/>.
    /// </summary>
    [ContractClass(typeof(StringWriterContract))]
    public interface IStringWriter
    {
        /// <summary>
        /// Writes a string <paramref name="value"/> to a <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="value">Must not be null.</param>
        void WriteString(Stream stream, string value);
    }
}