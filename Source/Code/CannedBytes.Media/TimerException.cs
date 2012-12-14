namespace CannedBytes.Media
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception used to report timer-related errors.
    /// </summary>
    [Serializable]
    public class TimerException : Exception
    {
        /// <summary>
        /// Constructs a new default instance.
        /// </summary>
        public TimerException()
        {
        }

        /// <summary>
        /// Constructs a new instance with a <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Must not be null or empty.</param>
        public TimerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs a new instance with a <paramref name="message"/> and an <paramref name="inner"/> exception.
        /// </summary>
        /// <param name="message">Must not be null or empty.</param>
        /// <param name="inner">Must not be null.</param>
        public TimerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Must not be null.</param>
        /// <param name="context">Must not be null.</param>
        protected TimerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}