namespace CannedBytes.Media.IO
{
    using System;
    using System.Collections.Generic;

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

        //#if NET4
        //        private CompositionContainer _compositionContainer;

        //        /// <summary>
        //        /// A container used for satisfying (external) object references.
        //        /// </summary>
        //        public CompositionContainer Container
        //        {
        //            get
        //            {
        //                return _compositionContainer;
        //            }

        //            set
        //            {
        //                _compositionContainer = value;

        //                if (_compositionContainer != null)
        //                {
        //                    _compositionContainer.AddInstance(this);
        //                }
        //            }
        //        }
        //#else
        //        private CompositionHost _compositionContainer;

        //        /// <summary>
        //        /// A container used for satisfying (external) object references.
        //        /// </summary>
        //        public CompositionHost Container
        //        {
        //            get
        //            {
        //                return _compositionContainer;
        //            }

        //            set
        //            {
        //                _compositionContainer = value;

        //                if (_compositionContainer != null)
        //                {
        //                    _compositionContainer.AddInstance(this);
        //                }
        //            }
        //        }
        //#endif

        /// <summary>
        /// Services from the container.
        /// </summary>
        public IServiceProvider Services
        {
            get
            {
                //return _compositionContainer;
                return null;
            }
        }

        /// <inheritdocs/>
        protected override void Dispose(DisposeObjectKind disposeKind)
        {
            if (disposeKind == DisposeObjectKind.ManagedAndUnmanagedResources)
            {
                ChunkFile.Dispose();

                //if (_compositionContainer != null)
                //{
                //    _compositionContainer.Dispose();
                //}
            }
        }
    }
}