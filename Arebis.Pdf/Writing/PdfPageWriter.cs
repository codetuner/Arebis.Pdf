using Arebis.Pdf.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Writing
{
    public class PdfPageWriter : IDisposable
    {
        internal protected PdfPageWriter(PdfDocumentWriter writer, double height, double width)
        {
            this.DocumentWriter = writer;
            this.Content = new List<PdfObjectRef>();
            this.Width = width;
            this.Height = height;
            this.PageObject = this.InitializePageObject(new PdfObject(), height, width);
        }

        protected virtual PdfObject InitializePageObject(PdfObject pageObj, double height, double width)
        {
            pageObj.Data["Type"] = "/Page";
            pageObj.Data["MediaBox"] = String.Format("[0 0 {0:0.###} {1:0.###}]", this.Width, this.Height);
            //pageObj.Data["CropBox"] = String.Format("[0 0 {0:0.###} {1:0.###}]", this.Width, this.Height);
            //pageObj.Data["Rotate"] = "0";
            //pageObj.Data["ProcSet"] = "[/PDF /Text /ImageC]";
            return pageObj;
        }

        public PdfDocumentWriter DocumentWriter { get; private set; }

        public PdfObject PageObject { get; private set; }

        public double Width { get; private set; }

        public double Height { get; private set; }

        public List<PdfObjectRef> Content { get; private set; }

        protected virtual PdfScriptObject CreateNewScriptObject()
        {
            return new PdfScriptObject();
        }

        public PdfObjectRef DrawCircle(double x, double y, double ray, PdfGraphicsOptions options)
        {
            var obj = CreateNewScriptObject();
            obj.BeginGraphicsState();
            options.Apply(obj);
            obj.DrawCircle(x, y, ray);
            obj.EndPath(true, true, true);
            obj.EndGraphicsState();
            return WriteObject(obj);
        }

        public PdfObjectRef DrawLine(double x1, double y1, double x2, double y2, PdfGraphicsOptions options)
        {
            var obj = CreateNewScriptObject();
            obj.BeginGraphicsState();
            options.Apply(obj);
            obj.BeginPath(x1, y1);
            obj.DrawLine(x2, y2);
            obj.EndPath(false, true, true);
            obj.EndGraphicsState();
            return WriteObject(obj);
        }

        public PdfObjectRef DrawRectangle(double x, double y, double width, double height, PdfGraphicsOptions options)
        {
            var obj = CreateNewScriptObject();
            obj.BeginGraphicsState();
            options.Apply(obj);
            obj.DrawRectangle(x, y, width, height);
            obj.EndPath(true, true, true);
            obj.EndGraphicsState();
            return WriteObject(obj);
        }

        public PdfObjectRef DrawRectangle2(double x1, double y1, double x2, double y2, PdfGraphicsOptions options)
        {
            var obj = CreateNewScriptObject();
            obj.BeginGraphicsState();
            options.Apply(obj);
            obj.DrawRectangle2(x1, y1, x2, y2);
            obj.EndPath(true, true, true);
            obj.EndGraphicsState();
            return WriteObject(obj);
        }

        public PdfObjectRef DrawOval(double x, double y, double width, double height, PdfGraphicsOptions options)
        {
            var obj = CreateNewScriptObject();
            obj.BeginGraphicsState();
            options.Apply(obj);
            obj.DrawOval(x, y, width, height);
            obj.EndPath(true, true, true);
            obj.EndGraphicsState();
            return WriteObject(obj);
        }

        public PdfObjectRef DrawOval2(double x1, double y1, double x2, double y2, PdfGraphicsOptions options)
        {
            var obj = CreateNewScriptObject();
            obj.BeginGraphicsState();
            options.Apply(obj);
            obj.DrawOval2(x1, y1, x2, y2);
            obj.EndPath(true, true, true);
            obj.EndGraphicsState();
            return WriteObject(obj);
        }

        /// <summary>
        /// Draws an image previously added to the DocumentWriter, scaled to the given width and height.
        /// </summary>
        public PdfObjectRef DrawImageRef(double x, double y, PdfObjectRef imageRef, double width, double height, PdfImagePlacement imagePlacement)
        {
            if (imagePlacement != PdfImagePlacement.Stretch)
            {
                var boxar = height / width;
                var imgar = DocumentWriter.GetImageAspectRatio(imageRef);
                if (imgar < boxar)
                {
                    var imgh = height / boxar * imgar;
                    switch (imagePlacement)
                    { 
                        case PdfImagePlacement.LeftOrTop:
                            y = y + (height - imgh);
                            break;
                        case PdfImagePlacement.Center:
                            y = y + (height - imgh) / 2.0;
                            break;
                    }
                    height = imgh;
                }
                else
                {
                    var imgw = width * boxar / imgar;
                    switch (imagePlacement)
                    { 
                        case PdfImagePlacement.RightOrBottom:
                            x = x + (width - imgw);
                            break;
                        case PdfImagePlacement.Center:
                            x = x + (width - imgw) / 2.0;
                            break;
                    }
                    width = imgw;
                }
            }
            var obj = CreateNewScriptObject();
            obj.DrawImageByName(x, y, width, height, DocumentWriter.GetNameOfXObject(imageRef));
            return WriteObject(obj);
        }

        /// <summary>
        /// Draws an image previously added to the DocumentWriter, scaled to the given width and same aspect ratio.
        /// </summary>
        public PdfObjectRef DrawImageRef(double x, double y, PdfObjectRef imageRef, double width)
        {
            return DrawImageRef(x, y, imageRef, width, DocumentWriter.GetImageAspectRatio(imageRef) * width, PdfImagePlacement.Stretch);
        }

        /// <summary>
        /// Draws an image scaled to the given width and height.
        /// </summary>
        public PdfObjectRef DrawImage(double x, double y, Image image, double width, double height, PdfImagePlacement imagePlacement)
        {
            var imageRef = DocumentWriter.AddImage(image);
            return DrawImageRef(x, y, imageRef, width, height, imagePlacement);
        }

        /// <summary>
        /// Draws an image scaled to the given width and same aspect ratio.
        /// </summary>
        public PdfObjectRef DrawImage(double x, double y, Image image, double width)
        {
            var imageRef = DocumentWriter.AddImage(image);
            return DrawImageRef(x, y, imageRef, width);
        }

        /// <summary>
        /// Draws a reference to an XObject (External Object) on the page.
        /// </summary>
        public PdfObjectRef DrawXObjectRef(double x, double y, PdfObjectRef xObjRef, double width)
        {
            var obj = CreateNewScriptObject();
            obj.DrawExternalObject(DocumentWriter.GetNameOfXObject(xObjRef));
            return WriteObject(obj);
        }
        
        /// <summary>
        /// Writes text to the page.
        /// </summary>
        public PdfObjectRef DrawText(double x, double y, string str, PdfTextOptions options)
        {
            var obj = CreateNewScriptObject();
            obj.BeginGraphicsState();
            obj.BeginText();
            options.Apply(obj, x, y);
            obj.DrawText(str);
            obj.EndText();
            obj.EndGraphicsState();
            return WriteObject(obj);
        }

        public PdfObjectRef DrawTextblock(double x, double y, string str, double blockWidth, PdfTextOptions options)
        {
            return this.DrawText(x, y, options.Font.SplitText(str, options.FontSize, blockWidth), options);
        }

        public virtual PdfObjectRef WriteObject(PdfObject obj)
        {
            if (obj is PdfScriptObject)
            {
                foreach (var font in ((PdfScriptObject)obj).ReferencedFonts)
                    DocumentWriter.RegisterFont(font);
            }
            var objRef = DocumentWriter.WriteObject(obj);
            Content.Add(objRef);
            return objRef;
        }

        public virtual void Dispose()
        {
            this.Close();
        }

        public virtual void Close()
        {
            if(DocumentWriter != null)
            {
                // Add contents information to PageObject:
                if (this.Content.Count > 0)
                    this.PageObject.Data["Contents"] = "[" + String.Join(" ", this.Content.Select(c => c.ToString())) + "]";

                // Write the PageObject:
                DocumentWriter.WritePage(this.PageObject);

                // Page is now closed:
                DocumentWriter = null;
            }
        }
    }
}
