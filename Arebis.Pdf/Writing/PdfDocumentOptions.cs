using Arebis.Pdf.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Writing
{
    [Serializable]
    public class PdfDocumentOptions
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string Subject { get; set; }

        public string Keywords { get; set; }

        public PdfStreamFilter TextFilter { get; set; }
    }
}
