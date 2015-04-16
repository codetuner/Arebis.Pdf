using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Common
{
    [Serializable]
    public abstract class PdfStream
    {
        public abstract int Length { get; }

        public abstract string Filter { get; }
    }
}
