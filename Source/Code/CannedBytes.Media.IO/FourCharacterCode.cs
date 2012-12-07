using System;
using System.IO;
using System.Text;

namespace CannedBytes.Media.IO
{
    public class FourCharacterCode
    {
        private byte[] fourCC;

        private FourCharacterCode()
        { }

        public FourCharacterCode(byte[] fourCC)
        {
            if (fourCC == null)
            {
                throw new ArgumentNullException("fourCC");
            }
            if (fourCC.Length < 4)
            {
                throw new ArgumentOutOfRangeException("The length of the byte array is not 4.", "fourCC");
            }

            this.fourCC = new byte[4];
            fourCC.CopyTo(this.fourCC, 0);
        }

        public FourCharacterCode(string fourCC)
        {
            if (fourCC == null)
            {
                throw new ArgumentNullException("fourCC");
            }
            if (fourCC.Length != 4)
            {
                throw new ArgumentOutOfRangeException("The length of the string is not 4.", "fourCC");
            }

            this.fourCC = Encoding.ASCII.GetBytes(fourCC);
        }

        //public FourCharacterCode(int fourCC)
        //{
        //    this.fourCC = BitConverter.GetBytes(fourCC);

        //    if (this.fourCC.Length != 4)
        //    {
        //        throw new ArgumentOutOfRangeException("The argument did not result in 4 bytes.", "fourCC");
        //    }
        //}

        public static FourCharacterCode ReadFrom(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
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

        public void WriteTo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream does not support writing.", "stream");
            }

            stream.Write(this.fourCC, 0, 4);
        }

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