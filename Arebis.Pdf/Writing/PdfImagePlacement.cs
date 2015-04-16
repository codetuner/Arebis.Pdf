using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Writing
{
    [Serializable]
    public enum PdfImagePlacement
    {
        Stretch = 0,
        Center = 1,
        LeftOrTop = 2,
        RightOrBottom = 3,
    }
}
