namespace CannedBytes.Media.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;

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
            Check.IfArgumentNullOrEmpty(filePath, "filePath");

            this.FilePath = filePath;
            this.FileExtension = Path.GetExtension(filePath);
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
        public Stream BaseStream { get; set; }

        /// <summary>
        /// Opens a file for reading.
        /// </summary>
        /// <param name="filePath">Must not be null or empty and the file must exist.</param>
        /// <returns>Never returns null.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Implemented the suggested pattern.")]
        public static ChunkFileInfo OpenRead(string filePath)
        {
            Contract.Requires(!String.IsNullOrEmpty(filePath));
            Contract.Ensures(Contract.Result<ChunkFileInfo>() != null);
            Contract.Ensures(Contract.Result<ChunkFileInfo>().BaseStream != null);

            Check.IfArgumentNullOrEmpty(filePath, "filePath");

            ChunkFileInfo chunkFile = null;
            ChunkFileInfo cf = null;

            try
            {
                cf = new ChunkFileInfo(filePath);
                cf.BaseStream = File.OpenRead(filePath);

                chunkFile = cf;
                cf = null;
            }
            finally
            {
                if (cf != null)
                {
                    cf.Dispose();
                }
            }

            return chunkFile;
        }

        /// <summary>
        /// Opens a file for writing.
        /// </summary>
        /// <param name="filePath">Must not be null or empty.</param>
        /// <returns>Never returns null.</returns>
        /// <remarks>Not implemented yet.</remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Implemented suggested pattern.")]
        public static ChunkFileInfo OpenWrite(string filePath)
        {
            Contract.Requires(!String.IsNullOrEmpty(filePath));
            Contract.Ensures(Contract.Result<ChunkFileInfo>() != null);
            Contract.Ensures(Contract.Result<ChunkFileInfo>().BaseStream != null);

            Check.IfArgumentNullOrEmpty(filePath, "filePath");

            ChunkFileInfo chunkFile = null;
            ChunkFileInfo cf = null;

            try
            {
                cf = new ChunkFileInfo(filePath);
                cf.BaseStream = File.OpenWrite(filePath);

                chunkFile = cf;
                cf = null;
            }
            finally
            {
                if (cf != null)
                {
                    cf.Dispose();
                }
            }

            return chunkFile;
        }

        /// <inheritdocs/>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            if (disposeKind == DisposeObjectKind.ManagedAndUnmanagedResources)
            {
                if (this.BaseStream != null)
                {
                    this.BaseStream.Dispose();
                }
            }
        }
    }
}