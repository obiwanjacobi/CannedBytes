using System.IO;

namespace CannedBytes.Media.IO.Services
{
    public interface INumberWriter
    {
        void WriteInt16(short value, Stream stream);

        void WriteInt16(int value, Stream stream);

        void WriteInt32(int value, Stream stream);

        void WriteInt32(long value, Stream stream);

        void WriteInt64(long value, Stream stream);
    }
}