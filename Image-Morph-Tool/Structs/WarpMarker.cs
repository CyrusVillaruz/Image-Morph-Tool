using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Image_Morph_Tool.Structs
{
    struct WarpMarker
    {
        public Vector targetStart;
        public Vector targetDirNorm;
        public Vector targetPerpNorm;
        public double targetLineLength;

        public Vector destStart;
        public Vector destDirNorm;
        public Vector destPerpNorm;
    };
}
