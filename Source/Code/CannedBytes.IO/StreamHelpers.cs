namespace CannedBytes.IO
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// Extension method helpers for <see cref="Stream"/>s.
    /// </summary>
    public static class StreamHelpers
    {
        /// <summary>
        /// Copies the complete content of the <paramref name="sourceStream"/>
        /// to the <paramref name="destinationStream"/> in chunks of <paramref name="blockSize"/> bytes.
        /// </summary>
        /// <param name="sourceStream">Must not be null.</param>
        /// <param name="destinationStream">Must not be null.</param>
        /// <param name="blockSize">The size of how many bytes are read and written at a time.</param>
        /// <returns>Returns the total number of bytes transferred.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Check is not recognized.")]
        public static long CopyTo(this Stream sourceStream, Stream destinationStream, int blockSize)
        {
            Contract.Requires(sourceStream != null);
            Contract.Requires(destinationStream != null);
            Contract.Requires(blockSize >= 0);
            Check.IfArgumentNull(sourceStream, "sourceStream");
            Check.IfArgumentNull(destinationStream, "destinationStream");
            Check.IfArgumentOutOfRange(blockSize, 0, int.MaxValue, "blockSize");

            if (blockSize <= 0 || (long)blockSize > sourceStream.Length)
            {
                blockSize = (int)sourceStream.Length;
            }

            byte[] buffer = new byte[blockSize];

            long totalBytesRead = 0;

            int bytesRead = sourceStream.Read(buffer, 0, blockSize);

            while (bytesRead == blockSize)
            {
                totalBytesRead += bytesRead;

                destinationStream.Write(buffer, 0, bytesRead);

                bytesRead = sourceStream.Read(buffer, 0, blockSize);
            }

            if (bytesRead > 0)
            {
                totalBytesRead += bytesRead;

                destinationStream.Write(buffer, 0, bytesRead);
            }

            return totalBytesRead;
        }
    }
}