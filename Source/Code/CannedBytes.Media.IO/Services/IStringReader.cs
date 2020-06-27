namespace CannedBytes.Media.IO.Services
{
    using System.IO;

    /// <summary>
    /// Generic string reader interface.
    /// </summary>
    public interface IStringReader
    {
        /// <summary>
        /// Reads a string from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        string ReadString(Stream stream);
    }
}