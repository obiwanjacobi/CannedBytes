using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Implements a <see cref="IStringReader"/> for a null terminated string.
    /// </summary>
    public class NullTerminatedStringReader : IStringReader
    {
        /// <summary>
        /// Reads the string from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the string that is read.</returns>
        public string ReadString(Stream stream)
        {
            var reader = new BinaryReader(stream);

            return reader.ReadString();
        }
    }
}