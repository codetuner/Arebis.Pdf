using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Common
{
    public class PdfDocumentStream : BasePdfDocumentStream
    {
        public PdfDocumentStream(Stream stream)
        {
            this.InnerStream = stream;
            this.InnerWriter = new StreamWriter(stream, Encoding.ASCII);
        }

        public Stream InnerStream { get; private set; }

        public StreamWriter InnerWriter { get; private set; }

        public override void WriteRaw(string s)
        {
            this.InnerWriter.Write(s);
        }

        public override void WriteRaw(byte[] bytes, int offset, int count)
        {
            this.InnerWriter.Flush();
            this.InnerStream.Write(bytes, offset, count);
        }

        public override void Flush()
        {
            if (this.InnerWriter != null) this.InnerWriter.Flush();
            if (this.InnerStream != null) this.InnerStream.Flush();
        }

        public override long Position
        {
            get
            {
                this.Flush();
                return this.InnerStream.Position;
            }
            set
            {
                this.Flush();
                this.InnerStream.Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Close()
        {
            base.Close();

            if (this.InnerWriter != null)
            {
                this.InnerWriter.Dispose();
                this.InnerWriter = null;
            }
        }
    }
}
