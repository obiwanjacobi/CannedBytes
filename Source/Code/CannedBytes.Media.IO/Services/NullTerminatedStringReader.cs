using System.IO;

namespace CannedBytes.Media.IO.Services
{
    public class NullTerminatedStringReader : IStringReader
    {
        public string ReadString(Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}