using System;
using System.Collections.Generic;

namespace CannedBytes.Media.IO
{
    public class FileChunkStack
    {
        private Stack<FileChunk> chunkStack = new Stack<FileChunk>();

        public void Clear()
        {
            this.chunkStack.Clear();
        }

        /// <summary>
        /// Root chunk of the file.
        /// </summary>
        public FileChunk RootChunk
        {
            get
            {
                if (this.chunkStack.Count >= 1)
                {
                    return this.chunkStack.ToArray()[0];
                }

                // no root set yet
                return null;
            }
            set
            {
                if (this.chunkStack.Count != 0)
                    throw new InvalidOperationException("Root Chunk has already been set.");

                PushChunk(value);
            }
        }

        /// <summary>
        /// Current chunk being processed.
        /// </summary>
        public FileChunk CurrentChunk
        {
            get
            {
                if (this.chunkStack.Count == 0) return null;

                return this.chunkStack.Peek();
            }
        }

        public void PushChunk(FileChunk chunk)
        {
            var parentChunk = CurrentChunk;

            // maintain child-chunks on parent.
            if (parentChunk != null)
            {
                parentChunk.SubChunks.Add(chunk);
            }

            this.chunkStack.Push(chunk);
        }

        public FileChunk PopChunk()
        {
            // never pop the root.
            if (this.chunkStack.Count > 1)
            {
                return this.chunkStack.Pop();
            }

            return null;
        }
    }
}