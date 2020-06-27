namespace CannedBytes.Media.IO.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;

    /// <summary>
    /// A string writer implementation that prefixes the length of the string before the string is written.
    /// </summary>
    [Export(typeof(IStringWriter))]
    public class SizePrefixedStringWriter : IStringWriter
    {
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="stream">Must not be null.</param>
        /// <param name="value">Must not be null.</param>
        public void WriteString(Stream stream, string value)
        {
            throw new NotImplementedException();
        }
    }
}