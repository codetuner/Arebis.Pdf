using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Common
{
    [Serializable]
    public class PdfObject
    {
        public PdfObject()
        {
            this.Data = new Dictionary<string, object>();
        }

        public Dictionary<String, Object> Data { get; set; }

        public PdfStream Stream { get; set; }
    }
}
