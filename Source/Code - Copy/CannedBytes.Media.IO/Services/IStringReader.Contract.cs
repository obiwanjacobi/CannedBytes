namespace CannedBytes.Media.IO.Services
{
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// Code contracts for <see cref="IStringReader"/>.
    /// </summary>
    [ContractClassFor(typeof(IStringReader))]
    internal abstract class StringReaderContract : IStringReader
    {
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