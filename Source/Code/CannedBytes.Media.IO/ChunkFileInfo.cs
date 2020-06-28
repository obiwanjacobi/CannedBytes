namespace CannedBytes.Media.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents a chunk file information.
    /// </summary>
    public class ChunkFileInfo : DisposableBase
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <remarks><see cref="FilePath"/> and <see cref="FileExtension"/> will be empty.</remarks>
        public ChunkFileInfo()
        {
            FilePath = String.Empty;
            FileExtension = String.Empty;
        }

        /// <summary>
        /// Constructs a new instance for the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">Must not be null or empty and the file must exist.</param>
        public ChunkFileInfo(string filePath)
        {
            Check.IfArgumentNullOrEmpty(filePath, nameof(filePath));

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
        /// File Stream for reading or writing?
        /// </summary>
        public FileAccess FileAccess { get; protected set; }

        /// <summary>
        /// Opens a file for reading.
        /// </summary>
        /// <param name="filePath">Must not be null or empty and the file must exist.</param>
        /// <returns>Never returns null.</returns>
        public static ChunkFileInfo OpenRead(string filePath)
        {
            Check.IfArgumentNullOrEmpty(filePath, nameof(filePath));

            return new ChunkFileInfo(filePath)
            {
                BaseStream = File.OpenRead(filePath),
                FileAccess = FileAccess.Read
            };
        }

        public static ChunkFileInfo CreateRead(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream not opened for reading.", nameof(stream));
            }

            return new ChunkFileInfo()
            {
                BaseStream = stream,
                FileAccess = FileAccess.Read
            };
        }

        /// <summary>
        /// Opens a file for writing.
        /// </summary>
        /// <param name="filePath">Must not be null or empty.</param>
        /// <returns>Never returns null.</returns>
        /// <remarks>Not implemented yet.</remarks>
        public static ChunkFileInfo OpenWrite(string filePath)
        {
            Check.IfArgumentNullOrEmpty(filePath, nameof(filePath));

            return new ChunkFileInfo(filePath)
            {
                BaseStream = File.OpenWrite(filePath),
                FileAccess = FileAccess.Write
            };
        }

        public static ChunkFileInfo CreateWrite(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream not opened for writing.", nameof(stream));
            }

            return new ChunkFileInfo()
            {
                BaseStream = stream,
                FileAccess = FileAccess.Write
            };
        }

        /// <inheritdocs/>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            if (disposeKind == DisposeObjectKind.ManagedAndUnmanagedResources &&
                BaseStream != null)
            {
                BaseStream.Dispose();
            }
        }
    }
}