using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Morph_Tool.Drawing.Abstract_Classes
{
    /**
     * Represents an interface for markers for interpolation.
     */
    public abstract class IMarker
    {
        public abstract void UpdateInterpolatedMarker(float interp);
    }
}
