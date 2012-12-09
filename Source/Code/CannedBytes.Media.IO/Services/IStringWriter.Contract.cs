using System.Diagnostics.Contracts;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Contracts class for <see cref="IStringWriter"/>.
    /// </summary>
    [ContractClassFor(typeof(IStringWriter))]
    internal abstract class StringWriterContract : IStringWriter
    {
        /// <summary>
        /// Block instantiation.
        /// </summary>
        private StringWriterContract()
        { }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="value">Must not be null.</param>
        void IStringWriter.WriteString(Stream stream, string value)
        {
            Contract.Requires(stream != null);
            Contract.Requires(value != null);

            throw new System.NotImplementedException();
        }
    }
}