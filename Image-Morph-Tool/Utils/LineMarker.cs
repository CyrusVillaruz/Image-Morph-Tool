using Image_Morph_Tool.Drawing.Abstract_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Morph_Tool.Utils
{
    /**
     * Represents a marker on a line for interpolation.
     */
    public class LineMarker : Marker<CustomLine>
    {
        public override void UpdateInterpolatedMarker(float interp)
        {
            InterpolatedMarker = CustomLine.Lerp(StartMarker, EndMarker, interp);
        }
    }
}
