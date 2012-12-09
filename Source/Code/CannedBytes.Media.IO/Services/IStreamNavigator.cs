using System.Diagnostics.Contracts;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// A general interface for navigating <see cref="Stream"/>s.
    /// </summary>
    [ContractClass(typeof(StreamNavigatorContract))]
    public interface IStreamNavigator
    {
        /// <summary>
        /// Gets or sets the alignment in bytes.
        /// </summary>
        int ByteAlignment { get; set; }

        /// <summary>
        /// Stores (and returns) the current <paramref name="stream"/> position.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the current <paramref name="stream"/> position.</returns>
        long SetCurrentMarker(Stream stream);

        /// <summary>
        /// Positions the <paramref name="stream"/> on the current marker set by <see cref="M:SetCurrentMarker"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns true if positioning the <paramref name="stream"/> was successful.</returns>
        /// <remarks>If the <see cref="M:SetCurrentMarker"/> method was not called, the <paramref name="stream"/> is rewound.
        /// The current marker is NOT cleared.</remarks>
        bool SeekToCurrentMarker(Stream stream);

        /// <summary>
        /// Alligns the position of the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Returns the number of bytes that were skipped in order to align the stream.</returns>
        /// <remarks>The alignment value must be set through the implementation class.</remarks>
        int AllignPosition(Stream stream);
    }
}