using System.IO;

namespace CannedBytes.Media.IO.Services
{
    public interface INumberReader
    {
        short ReadInt16(Stream stream);

        int ReadInt32(Stream stream);

        long ReadInt64(Stream stream);

        int ReadUInt16AsInt32(Stream stream);

        long ReadUInt32AsInt64(Stream stream);
    }
}