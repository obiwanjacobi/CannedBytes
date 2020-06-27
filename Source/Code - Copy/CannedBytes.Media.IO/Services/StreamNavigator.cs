namespace CannedBytes.Media.IO.Services
{
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Implements the <see cref="IStreamNavigator"/> interface.
    /// </summary>
    [Export(typeof(IStreamNavigator))]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public long SetCurrentMarker(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            this.currentMarker = stream.Position;
            return this.currentMarker;
        }

        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public bool SeekToCurrentMarker(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            if (stream.CanSeek)
            {
                stream.Seek(this.currentMarker, SeekOrigin.Begin);
                return true;
            }

            return false;
        }

        /// <inheritdocs/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check is not recognized")]
        public int AlignPosition(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

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