using System.IO;

namespace CannedBytes.Media.IO.Services
{
    public interface IStringWriter
    {
        void WriteString(Stream stream, string value);
    }
}