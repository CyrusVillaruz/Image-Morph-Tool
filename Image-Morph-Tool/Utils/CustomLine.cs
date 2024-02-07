using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Morph_Tool.Utils
{
    /**
     * Represents a drawn line in the image.
     */
    public class CustomLine
    {
        public Vector Start;
        public Vector End;
        public Vector Middle;

        public static CustomLine Lerp(CustomLine a, CustomLine b, float interp)
        {
            return new CustomLine()
            {
                Start = a.Start.Lerp(b.Start, interp),
                End = a.End.Lerp(b.End, interp),
                Middle = a.Middle.Lerp(b.Middle, interp)
            };
        }
    }
}
