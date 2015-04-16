﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arebis.Pdf.Common
{
    /// <summary>
    /// A PdfFont.
    /// </summary>
    [Serializable]
    public abstract class PdfFont : PdfObject
    {
        protected PdfFont()
        { }

        public PdfFont(string subType, string name, string baseFont, string encoding)
        {
            this.Data = new Dictionary<string, object>();
            this.Data["Type"] = "/Font";
            this.Data["Subtype"] = subType;
            this.Data["Name"] = name;
            this.Data["BaseFont"] = baseFont;
            this.Data["Encoding"] = encoding;
        }

        /// <summary>
        /// Name of the font.
        /// </summary>
        public string Name 
        {
            get
            {
                return this.Data["Name"].ToString();
            }
            set
            {
                this.Data["Name"] = value;
            }
        }

        /// <summary>
        /// Returns the width (in 1000ths of a point) of the given character.
        /// </summary>
        /// <param name="c">Character to return the width of.</param>
        public abstract int GetRawCharWidth(char c);

        /// <summary>
        /// Gets the width needed to render the given string in the given size
        /// of the current font.
        /// </summary>
        public double GetStringWidth(string str, double fontSize)
        {
            var maxWidth = 0;
            var width = 0;
            foreach (var c in str)
            {
                switch (c)
                { 
                    case '\n':
                    case '\r':
                        if (width > maxWidth)
                        {
                            maxWidth = width;
                            width = 0;
                        }
                        break;
                    default:
                        width += GetRawCharWidth(c);
                        break;
                }
            }

            if (width > maxWidth) maxWidth =width;

            return (maxWidth * fontSize);
        }

        /// <summary>
        /// Adds linefeeds to split the text to limit width.
        /// </summary>
        public string SplitText(string text, double fontSize, double width)
        {
            text = text.Replace("\t", "    ");
            var sb = new StringBuilder(text.Length + 200);
            var usedWidth = 0;
            var maxWidth = (int)(1000.0 * width / fontSize);
            var nonSpaceCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\r' || c == '\n')
                {
                    usedWidth = 0;
                    nonSpaceCount = 0;
                    sb.Append(c);
                    continue;
                }
                else if (c == ' ')
                {
                    usedWidth += GetRawCharWidth(c);
                    nonSpaceCount = 0;
                    sb.Append(' ');
                }
                else
                {
                    usedWidth += GetRawCharWidth(c);
                    if (usedWidth > maxWidth)
                    {
                        sb.Length = sb.Length - nonSpaceCount;
                        sb.Append('\n');
                        i -= nonSpaceCount + 1;

                        nonSpaceCount = 0;
                        usedWidth = 0;
                        continue;
                    }
                    sb.Append(c);
                    nonSpaceCount++;
                }
            }


            return sb.ToString();
        }
    }
}
