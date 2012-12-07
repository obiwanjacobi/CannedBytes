using System.IO;

namespace CannedBytes.IO
{
    public static class StreamUtil
    {
        public static long CopyTo(this Stream sourceStream, Stream destinationStream, int blockSize)
        {
            if (blockSize == 0 || (long)blockSize > sourceStream.Length)
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