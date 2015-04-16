using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Common
{
    [Serializable]
    public class PdfTextStream : PdfStream
    {
        public PdfTextStream()
        {
            this.Content = new StringBuilder();
        }

        public StringBuilder Content { get; set; }

        public override int Length
        {
            get { return Content.Length; }
        }

        public override string Filter
        {
            get { return null; }
        }
    }
}
