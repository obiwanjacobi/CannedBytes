using System.Diagnostics.Contracts;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Contract class for <see cref="INumberWriter"/>.
    /// </summary>
    [ContractClassFor(typeof(INumberWriter))]
    internal abstract class NumberWriterContract : INumberWriter
    {
        /// <summary>
        /// Block instantiation.
        /// </summary>
        private NumberWriterContract()
        { }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="value">No contract.</param>
        /// <param name="stream">Must not be null.</param>
        void INumberWriter.WriteInt16(short value, Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="value">No contract.</param>
        /// <param name="stream">Must not be null.</param>
        void INumberWriter.WriteInt16(int value, Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="value">No contract.</param>
        /// <param name="stream">Must not be null.</param>
        void INumberWriter.WriteInt32(int value, Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="value">No contract.</param>
        /// <param name="stream">Must not be null.</param>
        void INumberWriter.WriteInt32(long value, Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="value">No contract.</param>
        /// <param name="stream">Must not be null.</param>
        void INumberWriter.WriteInt64(long value, Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }
    }
}