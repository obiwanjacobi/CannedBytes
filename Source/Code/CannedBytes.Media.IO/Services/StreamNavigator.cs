using System.ComponentModel.Composition;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    /// <summary>
    /// Implements the <see cref="IStreamNavigator"/> interface.
    /// </summary>
    [Export(typeof(IStreamNavigator))]
    public class StreamNavigator : IStreamNavigator
    {
        long currentMarker;

        /// <summary>
        /// The default alignment is 2 bytes.
        /// </summary>
        public const int DefaultByteAlignment = 2;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public StreamNavigator()
        {
            ByteAlignment = DefaultByteAlignment;
        }

        /// <inheritdocs/>
        public int ByteAlignment { get; set; }

        /// <inheritdocs/>
        public long SetCurrentMarker(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            this.currentMarker = stream.Position;
            return this.currentMarker;
        }

        /// <inheritdocs/>
        public bool SeekToCurrentMarker(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            if (stream.CanSeek)
            {
                stream.Seek(this.currentMarker, SeekOrigin.Begin);
                return true;
            }

            return false;
        }

        /// <inheritdocs/>
        public int AllignPosition(Stream stream)
        {
            Throw.IfArgumentNull(stream, "stream");

            if (ByteAlignment <= 0) return 0;

            var rest = (int)(stream.Position % ByteAlignment);

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