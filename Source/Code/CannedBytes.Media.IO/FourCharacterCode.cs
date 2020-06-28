namespace CannedBytes.Media.IO
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a four character code.
    /// </summary>
    public class FourCharacterCode
    {
        /// <summary>
        /// Backing field for the 4cc.
        /// </summary>
        private byte[] _fourCC;

        /// <summary>
        /// Block default constructor.
        /// </summary>
        private FourCharacterCode()
        {
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="fourCC">A byte buffer of (at least) 4 characters.</param>
        public FourCharacterCode(byte[] fourCC)
        {
            Check.IfArgumentNull(fourCC, nameof(fourCC));
            Check.IfArgumentOutOfRange(fourCC.Length, 4, int.MaxValue, nameof(fourCC.Length));

            _fourCC = new byte[4];
            fourCC.CopyTo(_fourCC, 0);
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="fourCC">A string of exactly 4 characters long.</param>
        public FourCharacterCode(string fourCC)
        {
            Check.IfArgumentNull(fourCC, nameof(fourCC));
            Check.IfArgumentOutOfRange(fourCC.Length, 4, 4, nameof(fourCC.Length));

            _fourCC = Encoding.ASCII.GetBytes(fourCC);
        }

        /// <summary>
        /// Reads a four character code from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Never return null.</returns>
        public static FourCharacterCode ReadFrom(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            if (!stream.CanRead)
            {
                throw new ArgumentException("The stream does not support reading.", nameof(stream));
            }

            var fourCC = new FourCharacterCode
            {
                _fourCC = new byte[4]
            };

            int bytesRead = stream.Read(fourCC._fourCC, 0, 4);

            if (bytesRead < 4)
            {
                throw new EndOfStreamException();
            }

            return fourCC;
        }

        /// <summary>
        /// Writes the four character code to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        public void WriteTo(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream does not support writing.", nameof(stream));
            }

            stream.Write(_fourCC, 0, 4);
        }

        /// <summary>
        /// Returns the four character code as a string.
        /// </summary>
        /// <returns>Never returns null.</returns>
        public override string ToString()
        {
            if (_fourCC != null)
            {
                return Encoding.ASCII.GetString(_fourCC);
            }

            return String.Empty;
        }
    }
}