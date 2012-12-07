using System.ComponentModel.Composition;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    [Export(typeof(IStreamNavigator))]
    public class StreamNavigator : IStreamNavigator
    {
        long currentMarker;
        public const int DefaultByteAllignment = 2;

        public StreamNavigator()
        {
            ByteAllignment = DefaultByteAllignment;
        }

        public int ByteAllignment { get; set; }

        public long SetCurrentMarker(Stream stream)
        {
            this.currentMarker = stream.Position;
            return this.currentMarker;
        }

        public bool SeekToCurrentMarker(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(this.currentMarker, SeekOrigin.Begin);
                return true;
            }

            return false;
        }

        public int AllignPosition(Stream stream)
        {
            if (ByteAllignment <= 0) return 0;

            var rest = (int)(stream.Position % ByteAllignment);

            if (rest > 0)
            {
                if (stream.CanSeek)
                {
                    rest = (int)stream.Seek(rest, SeekOrigin.Current);
                }
                else
                {
                    var buffer = new byte[rest];
                    stream.Read(buffer, 0, rest);
                }
            }

            return rest;
        }
    }
}