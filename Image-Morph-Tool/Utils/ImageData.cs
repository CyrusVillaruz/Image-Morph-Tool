using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image_Morph_Tool.Structs;

namespace Image_Morph_Tool.Utils
{
    public unsafe class ImageData : IDisposable
    {
        public Color* Data { get; private set; }
        public readonly int Width;
        public readonly int Height;

        public readonly int BufferSize;
        public readonly int Stride;

        private readonly int widthSub1;
        private readonly int heightSub1;
        private readonly Color* lastValidAdress;

        public ImageData(int width, int height)
        {
            Stride = width * sizeof(Color);
            BufferSize = Stride * height;

            Data = (Color*)System.Runtime.InteropServices.Marshal.AllocHGlobal(BufferSize);
            Width = width;
            Height = height;

            widthSub1 = width - 1;
            heightSub1 = height - 1;
            lastValidAdress = Data + BufferSize - 1;
        }

        ~ImageData()
        {
            Dispose();
        }

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)Data);
            Data = null;
        }

        public Color Sample(double x, double y)
        {
            double xPosPixel = x * widthSub1;
            double yPosPixel = y * heightSub1;
            int xPosFloor = (int)xPosPixel;
            int yPosFloor = (int)yPosPixel;
            double fracX = xPosPixel - xPosFloor;
            double fracY = yPosFloor - yPosFloor;

            Color* upperLeft = Data + (yPosFloor * Width + xPosFloor);
            Color* upperRight = upperLeft;
            Color* lowerLeft = upperLeft + (yPosFloor != heightSub1 ? Width : 0);
            Color* lowerRight = lowerLeft;
            if (xPosFloor != widthSub1)
            {
                ++upperRight;
                ++lowerRight;
            }

            return Color.Lerp(Color.Lerp(*upperLeft, *upperRight, fracX),
                              Color.Lerp(*lowerLeft, *lowerRight, fracX), fracY);
        }
    };
}
