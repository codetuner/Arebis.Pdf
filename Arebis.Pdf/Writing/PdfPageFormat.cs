using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Writing
{
    [Serializable]
    public class PdfPageFormat
    {
        /// <summary>
        /// Instantiates a new page format.
        /// Use predefined static instances for A4 and Letter.
        /// </summary>
        /// <param name="height">Height of the page in points (1 inch = 72 points).</param>
        /// <param name="width">Width of the page in points (1 inch = 72 points).</param>
        /// <param name="orientation">Page orientation.</param>
        public PdfPageFormat(double height, double width, PdfPageOrientation orientation = PdfPageOrientation.Portrait)
        {
            this.Height = height;
            this.Width = width;
            this.Orientation = orientation;
        }

        /// <summary>
        /// Height of the page in points (1 inch = 72 points).
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Width of the page in points (1 inch = 72 points).
        /// </summary>
        public double Width { get; private set; }
        
        /// <summary>
        /// Page orientation.
        /// </summary>
        public PdfPageOrientation Orientation { get; private set; }

        /// <summary>
        /// A4 paper format in portrait (841x595).
        /// </summary>
        public static readonly PdfPageFormat A4Portrait = new PdfPageFormat(841.0, 595.0, PdfPageOrientation.Portrait);

        /// <summary>
        /// A4 paper format in landscape (595x841).
        /// </summary>
        public static readonly PdfPageFormat A4Landscape = new PdfPageFormat(841.0, 595.0, PdfPageOrientation.Landscape);

        /// <summary>
        /// Letter paper format in portrait (792x612).
        /// </summary>
        public static readonly PdfPageFormat LetterPortrait = new PdfPageFormat(792, 612.0, PdfPageOrientation.Portrait);

        /// <summary>
        /// Letter paper format in landscape (612x792).
        /// </summary>
        public static readonly PdfPageFormat LetterLandscape = new PdfPageFormat(792, 612.0, PdfPageOrientation.Landscape);
    }

    [Serializable]
    public enum PdfPageOrientation
    { 
        Portrait = 0,
        Landscape = 1,
    }
}
