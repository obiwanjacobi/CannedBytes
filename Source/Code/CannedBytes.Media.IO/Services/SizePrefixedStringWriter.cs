namespace CannedBytes.Media.IO.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;

    [Export(typeof(IStringWriter))]
    public class SizePrefixedStringWriter : IStringWriter
    {
        public void WriteString(Stream stream, string value)
        {
            throw new NotImplementedException();
        }
    }
}