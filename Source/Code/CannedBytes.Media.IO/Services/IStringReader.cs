using System.IO;

namespace CannedBytes.Media.IO.Services
{
    public interface IStringReader
    {
        string ReadString(Stream stream);
    }
}