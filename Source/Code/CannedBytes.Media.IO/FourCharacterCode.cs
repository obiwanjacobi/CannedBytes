using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace CannedBytes.Media.IO
{
    /// <summary>
    /// Represents a four character code.
    /// </summary>
    public class FourCharacterCode
    {
        /// <summary>
        /// Backing field for the 4cc.
        /// </summary>
        private byte[] fourCC;

        /// <summary>
        /// Block default constructor.
        /// </summary>
        private FourCharacterCode()
        { }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="fourCC">A byte buffer of (at least) 4 characters.</param>
        public FourCharacterCode(byte[] fourCC)
        {
            Contract.Requires(fourCC != null);
            Contract.Requires(fourCC.Length >= 4);
            Throw.IfArgumentNull(fourCC, "fourCC");
            Throw.IfArgumentOutOfRange(fourCC.Length, 4, int.MaxValue, "fourCC.Length");

            this.fourCC = new byte[4];
            fourCC.CopyTo(this.fourCC, 0);
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="fourCC">A string of exactly 4 characters long.</param>
        public FourCharacterCode(string fourCC)
        {
            Contract.Requires(fourCC != null);
            Contract.Requires(fourCC.Length == 4);
            Throw.IfArgumentNull(fourCC, "fourCC");
            Throw.IfArgumentOutOfRange(fourCC.Length, 4, 4, "fourCC.Length");

            this.fourCC = Encoding.ASCII.GetBytes(fourCC);
        }

        /// <summary>
        /// Reads a four character code from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <returns>Never return null.</returns>
        public static FourCharacterCode ReadFrom(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanRead);
            Contract.Ensures(Contract.Result<FourCharacterCode>() != null);
            Throw.IfArgumentNull(stream, "stream");

            if (!stream.CanRead)
            {
                throw new ArgumentException("The stream does not support reading.", "stream");
            }

            var fourCC = new FourCharacterCode();
            fourCC.fourCC = new byte[4];

            int bytesRead = stream.Read(fourCC.fourCC, 0, 4);

            if (bytesRead < 4)
            {
                throw new EndOfStreamException("End of stream while reading fourCC.");
            }

            return fourCC;
        }

        /// <summary>
        /// Writes the four character code to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        public void WriteTo(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanWrite);
            Throw.IfArgumentNull(stream, "stream");

            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream does not support writing.", "stream");
            }

            stream.Write(this.fourCC, 0, 4);
        }

        /// <summary>
        /// Returns the four character code as a string.
        /// </summary>
        /// <returns>Never returns null.</returns>
        public override string ToString()
        {
            if (this.fourCC != null)
            {
                return Encoding.ASCII.GetString(this.fourCC);
            }

            return String.Empty;
        }
    }
}