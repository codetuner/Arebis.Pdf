using System;
using System.IO;
using System.Text;

namespace Arebis.Pdf.Common
{
    public class PdfDeflateStreamFilter : PdfStreamFilter
    {
        public PdfDeflateStreamFilter()
            : base("/FlateDecode")
        { }

        public override byte[] Encode(byte[] bytes)
        {
            using (var inputstr = new MemoryStream(bytes))
            using (var outputstr = new MemoryStream())
            {
                using (var dstream = new System.IO.Compression.DeflateStream(outputstr, System.IO.Compression.CompressionLevel.Optimal))
                {
                    // http://stackoverflow.com/questions/9050260/what-does-a-zlib-header-look-like
                    outputstr.WriteByte(0x78);
                    outputstr.WriteByte(0xDA);

                    inputstr.CopyTo(dstream);
                }
                return outputstr.ToArray();
            }
        }

        public override byte[] Decode(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
