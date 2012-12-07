using System;

namespace CannedBytes.Media.IO
{
    [Serializable]
    public class ChunkFileException : Exception
    {
        public ChunkFileException()
        { }

        public ChunkFileException(string message)
            : base(message)
        { }

        public ChunkFileException(string message, Exception inner)
            : base(message, inner)
        { }

        protected ChunkFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}