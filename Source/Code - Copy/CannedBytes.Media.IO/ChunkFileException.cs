namespace CannedBytes.Media.IO
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The root of all chunk file specific exceptions.
    /// </summary>
    [Serializable]
    public class ChunkFileException : Exception
    {
        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        public ChunkFileException()
        {
        }

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="message">A exception text message. Must not be null.</param>
        public ChunkFileException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="message">A exception text message. Must not be null.</param>
        /// <param name="inner">A reference to a caught exception that lead to creating this instance. Must not be null.</param>
        public ChunkFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Serialization ctor.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        protected ChunkFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}