namespace CannedBytes.Media.IO.Services
{
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// Code contracts for <see cref="INumberReader"/>.
    /// </summary>
    [ContractClassFor(typeof(INumberReader))]
    internal abstract class NumberReaderContract : INumberReader
    {
        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>No contract.</returns>
        short INumberReader.ReadInt16(Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>No contract.</returns>
        int INumberReader.ReadInt32(Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>No contract.</returns>
        long INumberReader.ReadInt64(Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>No contract.</returns>
        int INumberReader.ReadUInt16AsInt32(Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>No contract.</returns>
        long INumberReader.ReadUInt32AsInt64(Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }
    }
}