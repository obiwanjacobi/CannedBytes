using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace CannedBytes.Media.IO
{
    public class ChunkFileInfo : IDisposable
    {
        public ChunkFileInfo(string filePath)
        {
            Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(filePath));

            FilePath = filePath;
            FileExtension = Path.GetExtension(filePath);
        }

        /// <summary>
        /// The extension of the file that is being processed.
        /// </summary>
        public string FileExtension { get; protected set; }

        public string FilePath { get; protected set; }

        public Stream BaseStream { get; protected set; }

        public static ChunkFileInfo OpenRead(string filePath)
        {
            Contract.Ensures(Contract.Result<ChunkFileInfo>() != null);

            var chunkFile = new ChunkFileInfo(filePath);

            chunkFile.BaseStream = File.OpenRead(filePath);

            return chunkFile;
        }

        public static ChunkFileInfo OpenWrite(string filePath)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                BaseStream.Dispose();
            }
        }
    }
}