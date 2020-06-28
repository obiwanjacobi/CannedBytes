namespace CannedBytes.Media.IO.Services
{
    using System.IO;

    /// <summary>
    /// Implements the <see cref="IStreamNavigator"/> interface.
    /// </summary>
//    [Export(typeof(IStreamNavigator))]
    public class StreamNavigator : IStreamNavigator
    {
        /// <summary>Backing field for the current stream position marker.</summary>
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
            this.ByteAlignment = DefaultByteAlignment;
        }

        /// <inheritdocs/>
        public int ByteAlignment { get; set; }

        /// <inheritdocs/>
        public long SetCurrentMarker(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            this.currentMarker = stream.Position;
            return this.currentMarker;
        }

        /// <inheritdocs/>
        public bool SeekToCurrentMarker(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            if (stream.CanSeek)
            {
                stream.Seek(this.currentMarker, SeekOrigin.Begin);
                return true;
            }

            return false;
        }

        /// <inheritdocs/>
        public int AlignPosition(Stream stream)
        {
            Check.IfArgumentNull(stream, nameof(stream));

            if (this.ByteAlignment <= 0)
            {
                return 0;
            }

            var rest = (int)(stream.Position % this.ByteAlignment);

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