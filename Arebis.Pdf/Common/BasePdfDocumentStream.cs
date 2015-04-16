using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Common
{
    public abstract class BasePdfDocumentStream : IDisposable
    {
        public abstract void WriteRaw(string s);

        public abstract void WriteRaw(byte[] bytes, int offset, int length);
        
        public void WriteRaw(byte[] bytes)
        {
            this.WriteRaw(bytes, 0, bytes.Length);
        }

        public abstract void Flush();

        public abstract long Position { get; set; }

        public PdfStreamFilter Filter { get; set; }

        public virtual long WriteObject(PdfObject obj, PdfObjectRef objRef)
        {
            var offset = this.Position;

            var builder = new StringBuilder();
            builder.Append(objRef.ObjectId + " " + objRef.GenerationId + " obj\n");
            if (obj.Data != null || obj.Stream != null)
            {
                builder.Append("<<\n");
                if (obj.Data != null)
                {
                    foreach (var pair in obj.Data)
                    {
                        builder.Append('/');
                        builder.Append(pair.Key);
                        builder.Append(' ');
                        builder.Append(pair.Value.ToString());
                        builder.Append('\n');
                    }
                }
                if (obj.Stream == null)
                {
                    builder.Append(">>\n");
                }
                else if (obj.Stream is PdfBinaryStream)
                {
                    var s = ((PdfBinaryStream)obj.Stream);
                    builder.Append("/Length ");
                    builder.Append(s.Length);
                    builder.Append("\n/Filter [");
                    builder.Append(s.Filter);
                    builder.Append("]\n>>\n");
                    builder.Append("stream\n");

                    this.WriteRaw(builder.ToString());
                    builder.Length = 0;
                    this.WriteRaw(s.Content, 0, s.Content.Length);

                    builder.Append("\nendstream\n");
                }
                else
                {
                    byte[] bytes;
                    if ((obj.Stream is PdfTextStream) && (this.Filter != null))
                    {
                        bytes = this.Filter.EncodeString(((PdfTextStream)obj.Stream).Content.ToString());
                        builder.Append("/Filter [");
                        builder.Append(this.Filter.Name);
                        builder.Append("]\n");
                    }
                    else
                    {
                        bytes = Encoding.Default.GetBytes(((PdfTextStream)obj.Stream).Content.ToString());
                    }
                    builder.Append("/Length ");
                    builder.Append(bytes.Length);
                    builder.Append('\n');

                    builder.Append(">>\n");

                    builder.Append("stream\n");

                    this.WriteRaw(builder.ToString());
                    builder.Length = 0;
                    this.WriteRaw(bytes, 0, bytes.Length);

                    builder.Append("\nendstream\n");
                }
            }
            builder.Append("endobj\n");

            this.WriteRaw(builder.ToString());

            return offset;
        }

        public virtual void Close()
        {
            this.Flush();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
