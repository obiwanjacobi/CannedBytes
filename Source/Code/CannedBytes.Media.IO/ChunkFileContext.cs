namespace CannedBytes.Media.IO
{
    using System.Collections.Generic;
    using System.ComponentModel.Design;

    /// <summary>
    /// Represents the context for the (R)IFF file that is being parsed.
    /// </summary>
    public class ChunkFileContext : DisposableBase
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public ChunkFileContext()
        {
            ChunkStack = new FileChunkStack();
            HeaderStack = new Stack<FileChunkHeader>();
        }

        /// <summary>
        /// Gets or sets the file (stream) object of the file currently being processed.
        /// </summary>
        public ChunkFileInfo ChunkFile { get; set; }

        /// <summary>
        /// Gets the stack of FileChunks used during file parsing.
        /// </summary>
        /// <remarks>Populated while reading, optionally used for writing.</remarks>
        public FileChunkStack ChunkStack { get; private set; }

        /// <summary>
        /// Gets the stack of streams used for writing.
        /// </summary>
        public Stack<FileChunkHeader> HeaderStack { get; private set; }

        /// <summary>
        /// Gets or sets an indication whether to copy data from/to the file streams or reuse the existing (sub)stream.
        /// </summary>
        public bool CopyStreams { get; set; }

        private ServiceContainer _services;

        /// <summary>
        /// Services from the container.
        /// </summary>
        public ServiceContainer Services
        {
            get
            {
                return _services;
            }

            set
            {
                _services = value;

                if (_services != null)
                {
                    _services.AddService(GetType(), this);
                }
            }
        }

        /// <inheritdocs/>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            if (disposeKind == DisposeObjectKind.ManagedAndUnmanagedResources)
            {
                ChunkFile.Dispose();

                if (_services != null)
                {
                    _services.Dispose();
                }
            }
        }
    }
}