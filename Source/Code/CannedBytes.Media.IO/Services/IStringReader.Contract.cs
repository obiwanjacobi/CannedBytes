using System.Diagnostics.Contracts;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Code contracts for <see cref="IStringReader"/>.
    /// </summary>
    [ContractClassFor(typeof(IStringReader))]
    internal abstract class StringReaderContract : IStringReader
    {
        /// <summary>
        /// Block instantiation.
        /// </summary>
        private StringReaderContract()
        { }

        /// <summary>
        /// Contracts.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Never returns null.</returns>
        string IStringReader.ReadString(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Ensures(Contract.Result<string>() != null);

            throw new System.NotImplementedException();
        }
    }
}