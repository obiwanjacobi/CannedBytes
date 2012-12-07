using System;
using System.ComponentModel.Composition.Hosting;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// Represents the (R)IFF file that is being parsed.
    /// </summary>
    public class ChunkFileContext : IDisposable
    {
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

        public static ChunkFileContext OpenFrom(string filePath)
        {
            var ctx = new ChunkFileContext();

            ctx.ChunkFile = ChunkFileInfo.OpenRead(filePath);

            return ctx;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                ChunkFile.Dispose();
                CompositionContainer.Dispose();
            }
        }
    }
}