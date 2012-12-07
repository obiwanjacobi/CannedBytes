using System.ComponentModel.Composition;
using System.IO;

namespace CannedBytes.Media.IO.Services
{
    [Export(typeof(IStringReader))]
    public class SizePrefixedStringReader : IStringReader
    {
        public string ReadString(Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}