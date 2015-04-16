using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arebis.Pdf.Common;
using System.Globalization;
using System.Drawing;

namespace Arebis.Pdf.Writing
{
    public class PdfDocumentWriter : IDisposable
    {
        public string PdfVersion = "1.4";
        public string PdfCreator = "Arebis.Pdf .NET Library";

        private int CurrentGenerationId = 0;

        private List<PdfObjectRef> PageRefs = new List<PdfObjectRef>();
        private PdfObjectRef CatalogRef;
        private PdfObjectRef InfoRef;
        private PdfObjectRef PagesRef;
        private PdfObjectRef ResourcesRef;

        public PdfDocumentWriter(Stream destination, PdfDocumentOptions options = null)
        {
            this.Writer = new StreamWriter(destination, Encoding.ASCII);
            this.Options = options ?? new PdfDocumentOptions();
            this.Xref = new List<long>();
            this.Fonts = new Dictionary<string, PdfObjectRef>();
            this.XObjects = new Dictionary<string, PdfObjectRef>();
            this.XObjectsRev = new Dictionary<PdfObjectRef, string>();
            this.ImageRatios = new Dictionary<PdfObjectRef, double>();
            this.WriteDocumentStart();
        }

        public StreamWriter Writer { get; private set; }

        public PdfDocumentOptions Options { get; private set; }

        protected List<long> Xref { get; private set; }

        protected Dictionary<string, PdfObjectRef> Fonts { get; private set; }

        protected Dictionary<string, PdfObjectRef> XObjects { get; private set; }
        
        protected Dictionary<PdfObjectRef, string> XObjectsRev { get; private set; }

        protected Dictionary<PdfObjectRef, double> ImageRatios { get; private set; }

        protected void RegisterXObject(PdfObjectRef objRef, string name)
        {
            XObjects[name] = objRef;
            XObjectsRev[objRef] = name;
        }

        public string GetNameOfXObject(PdfObjectRef objRef)
        {
            return this.XObjectsRev[objRef];
        }

        public double GetImageAspectRatio(PdfObjectRef imageRef)
        {
            return this.ImageRatios[imageRef];
        }

        public PdfObjectRef RegisterFont(PdfFont font)
        {
            PdfObjectRef fontRef;
            if (this.Fonts.TryGetValue(font.Name, out fontRef))
            {
                return fontRef;
            }
            else
            {
                fontRef = this.Fonts[font.Name] = WriteObject(font);
                return fontRef;
            }
        }

        /// <summary>
        /// Starts a new page. Dispose page when done.
        /// </summary>
        public virtual PdfPageWriter NewPage(PdfPageFormat format)
        {
            if (format.Orientation == PdfPageOrientation.Portrait)
            {
                return this.NewPage(format.Height, format.Width);
            }
            else
            {
                return this.NewPage(format.Width, format.Height);
            }
        }

        public virtual PdfPageWriter NewPage(double width, double height)
        {
            return new PdfPageWriter(this, width, height);
        }

        protected virtual PdfObjectRef NewObjectRef()
        {
            Xref.Add(-1L);
            return new PdfObjectRef(CurrentGenerationId, Xref.Count);
        }

        public PdfObjectRef WriteObject(PdfObject obj)
        {
            var objRef = NewObjectRef();
            this.WriteObject(obj, objRef);
            return objRef;
        }

        public virtual void WriteObject(PdfObject obj, PdfObjectRef objRef)
        {
            this.Writer.Flush();
            Xref[objRef.ObjectId - 1] = this.Writer.BaseStream.Position;

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
                    this.Writer.Flush();
                    this.Writer.BaseStream.Write(s.Content, 0, s.Content.Length);

                    builder.Append("\nendstream\n");
                }
                else
                {
                    byte[] bytes;
                    if ((obj.Stream is PdfTextStream) && (Options.TextFilter != null))
                    {
                        bytes = Options.TextFilter.EncodeString(((PdfTextStream)obj.Stream).Content.ToString());
                        builder.Append("/Filter [");
                        builder.Append(Options.TextFilter.Name);
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
                    this.Writer.Flush();
                    this.Writer.BaseStream.Write(bytes, 0, bytes.Length);

                    builder.Append("\nendstream\n");
                }
            }
            builder.Append("endobj\n");
            
            this.WriteRaw(builder.ToString());
        }

        public PdfObjectRef AddImage(Image image)
        {
            var obj = new PdfObject();
            obj.Data["Subtype"] = "/Image";
            obj.Data["Width"] = image.Width;
            obj.Data["Height"] = image.Height;
            obj.Data["BitsPerComponent"] = "8";
            obj.Data["ColorSpace"] = "/DeviceRGB";
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                obj.Stream = new PdfBinaryStream("/DCTDecode", ms.ToArray());
            }

            var objRef = AddXObject(obj);
            ImageRatios[objRef] = (double)image.Height / image.Width;
            return objRef;
        }

        public PdfObjectRef AddXObject(PdfObject obj)
        {
            var objRef = NewObjectRef();
            var name = objRef.ToDefaultName();
            obj.Data["Type"] = "/XObject";
            if (!obj.Data.ContainsKey("Subtype")) obj.Data["Subtype"] = "/Form";
            obj.Data["Name"] = name;

            WriteObject(obj, objRef);
            RegisterXObject(objRef, name);
            return objRef;
        }

        public virtual void WriteRaw(string str)
        {
            this.Writer.Write(str);
        }

        public virtual void Dispose()
        {
            this.Close();
        }

        public virtual void Close()
        {
            if (this.Writer != null)
            {
                this.WriteDocumentEnd();
                this.Writer.Flush();
                this.Writer.Dispose();
                this.Writer = null;
            }
        }

        private void WriteDocumentStart()
        {
            var binarybytes = new byte[] { 0xE2, 0xE3, 0xCF, 0xD3 };
            WriteRaw("%PDF-" + PdfVersion + "\n%");
            Writer.Flush();
            Writer.BaseStream.Write(binarybytes, 0, binarybytes.Length);
            WriteRaw("\n%Arebis.Pdf .NET Library\n");

            CatalogRef = NewObjectRef();
            InfoRef = NewObjectRef();
            ResourcesRef = NewObjectRef();
            PagesRef = NewObjectRef();

            var catalog = new PdfObject();
            catalog.Data["Type"] = "/Catalog";
            catalog.Data["Version"] = "/" + PdfVersion;
            catalog.Data["Pages"] = PagesRef;
            WriteObject(catalog, CatalogRef);

            var info = new PdfObject();
            if (!String.IsNullOrWhiteSpace(this.Options.Title)) info.Data["Title"] = '(' + this.Options.Title + ')';
            if (!String.IsNullOrWhiteSpace(this.Options.Subject)) info.Data["Subject"] = '(' + this.Options.Subject + ')';
            if (!String.IsNullOrWhiteSpace(this.Options.Keywords)) info.Data["Keywords"] = '(' + this.Options.Keywords + ')';
            info.Data["Author"] = '(' + ((!String.IsNullOrWhiteSpace(this.Options.Author)) ? this.Options.Author : Environment.UserName) + ')';
            info.Data["Creator"] = "(" + PdfCreator + ")";
            var now = DateTime.Now;
            var timezone = TimeZone.CurrentTimeZone.GetUtcOffset(now);
            info.Data["CreationDate"] = "(D:" + now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + (timezone.Ticks >= 0 ? '+' : '-') + String.Format("{0:00}'{1:00}'", Math.Abs(timezone.Hours), Math.Abs(timezone.Minutes)) + ")";
            WriteObject(info, InfoRef);
        }

        internal void WritePage(PdfObject pageObj)
        {
            pageObj.Data["Type"] = "/Page";
            pageObj.Data["Parent"] = PagesRef;
            pageObj.Data["Resources"] = ResourcesRef;
            var pageObjRef = WriteObject(pageObj);
            PageRefs.Add(pageObjRef);
        }

        private void WriteDocumentEnd()
        {
            var resources = new PdfObject();
            if (this.Fonts.Count > 0)
                resources.Data["Font"] = "<< " + String.Join(" ", this.Fonts.Select(kv => kv.Key + " " + kv.Value)) + " >>";
            if (this.XObjects.Count > 0)
                resources.Data["XObject"] = "<< " + String.Join(" ", this.XObjects.Select(kv => kv.Key + " " + kv.Value)) + " >>";
            WriteObject(resources, ResourcesRef);

            var pages = new PdfObject();
            pages.Data["Type"] = "/Pages";
            pages.Data["Count"] = PageRefs.Count;
            pages.Data["Kids"] = "[" + String.Join(" ", PageRefs.Select(r => r.ToString())) + "]";
            WriteObject(pages, PagesRef);

            this.Writer.Flush();
            var xrefStart = this.Writer.BaseStream.Position;
            var builder = new StringBuilder();
            builder.Append("xref\r\n");
            builder.Append("0 ");
            builder.Append(Xref.Count + 1);
            builder.Append("\r\n");
            builder.Append("0000000000 65535 f\r\n");
            foreach (var offset in Xref)
            {
                if (offset < 0)
                {
                    builder.AppendFormat("{0:0000000000} {1:00000} {2}\r\n", 0L, CurrentGenerationId, 'f');
                }
                else
                {
                    builder.AppendFormat("{0:0000000000} {1:00000} {2}\r\n", offset, CurrentGenerationId, 'n');
                }
            }

            var fileId = Guid.NewGuid().ToString().Replace("-", "");
            builder.Append("trailer\n<<\n/Size ");
            builder.Append(Xref.Count + 1);
            builder.Append("\n/Root ");
            builder.Append(CatalogRef);
            builder.Append("\n/Info ");
            builder.Append(InfoRef);
            builder.Append("\n/ID [<");
            builder.Append(fileId);
            builder.Append("><");
            builder.Append(fileId);
            builder.Append(">]");
            builder.Append("\n>>\nstartxref\n");
            builder.Append(xrefStart);
            builder.Append("\n%%EOF\n");

            WriteRaw(builder.ToString());
        }
    }
}
