using System.Diagnostics.Contracts;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Contract for <see cref="IStreamNavigator"/>.
    /// </summary>
    [ContractClassFor(typeof(IStreamNavigator))]
    internal abstract class StreamNavigatorContract : IStreamNavigator
    {
        /// <summary>
        /// Block instantiation.
        /// </summary>
        private StreamNavigatorContract()
        { }

        /// <summary>
        /// Value must be greater or equal to  zero.
        /// </summary>
        int IStreamNavigator.ByteAlignment
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                throw new System.NotImplementedException();
            }
            set
            {
                Contract.Requires(value >= 0);

                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns greater or equal to zero.</returns>
        long IStreamNavigator.SetCurrentMarker(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Ensures(Contract.Result<long>() >= 0);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>No contract.</returns>
        bool IStreamNavigator.SeekToCurrentMarker(Stream stream)
        {
            Contract.Requires(stream != null);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Contract.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns greater or equal to zero.</returns>
        int IStreamNavigator.AllignPosition(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            throw new System.NotImplementedException();
        }
    }
}