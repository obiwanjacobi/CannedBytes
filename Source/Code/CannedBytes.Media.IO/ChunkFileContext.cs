using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;

namespace CannedBytes.Media.IO
{
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
        }

        /// <summary>
        /// The file (stream) object of the file currently being processed.
        /// </summary>
        public ChunkFileInfo ChunkFile { get; set; }

        /// <summary>
        /// Stack of FileChunks used during file parsing.
        /// </summary>
        public FileChunkStack ChunkStack { get; protected set; }

        /// <summary>
        /// Backing field for <see cref="P:CompositionContainer"/>.
        /// </summary>
        private CompositionContainer compositionContainer;

        /// <summary>
        /// A container used for satisfying (external) object references.
        /// </summary>
        public CompositionContainer CompositionContainer
        {
            get { return this.compositionContainer; }
            set
            {
                this.compositionContainer = value;

                if (this.compositionContainer != null)
                {
                    this.compositionContainer.AddInstance(this);
                }
            }
        }

        /// <summary>
        /// Creates a new context based on the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">Must not be null or empty and the file pointed to must exist.</param>
        /// <returns></returns>
        public static ChunkFileContext OpenFrom(string filePath)
        {
            Contract.Requires(!String.IsNullOrEmpty(filePath));
            Throw.IfArgumentNullOrEmpty(filePath, "filePath");

            var ctx = new ChunkFileContext();

            ctx.ChunkFile = ChunkFileInfo.OpenRead(filePath);

            return ctx;
        }

        /// <inheritdocs/>
        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                ChunkFile.Dispose();

                if (this.compositionContainer != null)
                {
                    this.compositionContainer.Dispose();
                }
            }

            base.Dispose(disposeManagedResources);
        }
    }
}