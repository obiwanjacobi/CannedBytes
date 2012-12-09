using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// Represents a chunk file information.
    /// </summary>
    public class ChunkFileInfo : DisposableBase
    {
        /// <summary>
        /// Constructs a new instance for the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">Must not be null or empty and the file must exist.</param>
        public ChunkFileInfo(string filePath)
        {
            Contract.Requires(!String.IsNullOrEmpty(filePath));
            Throw.IfArgumentNullOrEmpty(filePath, "filePath");

            FilePath = filePath;
            FileExtension = Path.GetExtension(filePath);
        }

        /// <summary>
        /// Gets the extension of the file that is being processed.
        /// </summary>
        public string FileExtension { get; protected set; }

        /// <summary>
        /// Gets the complete file path.
        /// </summary>
        public string FilePath { get; protected set; }

        /// <summary>
        /// Gets the opened file stream.
        /// </summary>
        public Stream BaseStream { get; protected set; }

        /// <summary>
        /// Opens a file for reading.
        /// </summary>
        /// <param name="filePath">Must not be null or empty and the file must exist.</param>
        /// <returns>Never returns null.</returns>
        public static ChunkFileInfo OpenRead(string filePath)
        {
            Contract.Requires(!String.IsNullOrEmpty(filePath));
            Contract.Ensures(Contract.Result<ChunkFileInfo>() != null);
            Throw.IfArgumentNullOrEmpty(filePath, "filePath");

            var chunkFile = new ChunkFileInfo(filePath);

            chunkFile.BaseStream = File.OpenRead(filePath);

            return chunkFile;
        }

        /// <summary>
        /// Opens a file for writing.
        /// </summary>
        /// <param name="filePath">Must not be null or empty.</param>
        /// <returns>Never returns null.</returns>
        /// <remarks>Not implemented yet.</remarks>
        public static ChunkFileInfo OpenWrite(string filePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdocs/>
        protected override void Dispose(bool disposing)
        {
            if (BaseStream != null)
            {
                BaseStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}