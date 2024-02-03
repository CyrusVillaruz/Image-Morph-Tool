using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Morph_Tool.Drawing
{
    public struct Color
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;

        public Color(byte B, byte G, byte R)
        {
            this.B = B;
            this.G = G;
            this.R = R;
            A = 255;
        }

        public static Color Lerp(Color a, Color b, float interp)
        {
            return new Color((byte)(a.B + (b.B - a.B) * interp),
                             (byte)(a.G + (b.G - a.G) * interp),
                             (byte)(a.R + (b.R - a.R) * interp));
        }

        public static Color Lerp(Color a, Color b, double interp)
        {
            return new Color((byte)(a.B + (b.B - a.B) * interp),
                             (byte)(a.G + (b.G - a.G) * interp),
                             (byte)(a.R + (b.R - a.R) * interp));
        }
    };
}
