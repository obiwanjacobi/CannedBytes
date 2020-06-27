﻿namespace CannedBytes.Media.IO.Services
{
    using System.ComponentModel.Composition;
    using System.IO;

    /// <summary>
    /// Implements the <see cref="IStringReader"/> interface for a size-prefixed string.
    /// </summary>
    [Export(typeof(IStringReader))]
    public class SizePrefixedStringReader : IStringReader
    {
        /// <inheritdocs/>
        public string ReadString(Stream stream)
        {
            Check.IfArgumentNull(stream, "stream");

            throw new System.NotImplementedException();
        }
    }
}