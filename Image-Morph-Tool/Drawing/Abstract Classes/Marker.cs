using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image_Morph_Tool.Enums;

namespace Image_Morph_Tool.Drawing.Abstract_Classes
{
    public abstract class Marker<T> : IMarker
    {
        public T StartMarker;
        public T EndMarker;
        public T InterpolatedMarker;

        public T this[Location element]
        {
            get
            {
                switch (element)
                {
                    case Location.START_IMAGE:
                        return StartMarker;
                    case Location.END_IMAGE:
                        return EndMarker;
                    case Location.OUTPUT_IMAGE:
                        return InterpolatedMarker;
                }
                throw new Exception("PointMarker has only 3 Elements!");
            }
            set
            {
                switch (element)
                {
                    case Location.START_IMAGE:
                        StartMarker = value;
                        break;
                    case Location.END_IMAGE:
                        EndMarker = value;
                        break;
                    case Location.OUTPUT_IMAGE:
                        InterpolatedMarker = value;
                        break;
                }
            }
        }
    }
}
