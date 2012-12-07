using System.IO;

namespace CannedBytes.Media.IO.Services
{
    public interface IStreamNavigator
    {
        int ByteAllignment { get; set; }

        long SetCurrentMarker(Stream stream);

        bool SeekToCurrentMarker(Stream stream);

        int AllignPosition(Stream stream);
    }
}