using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Image_Morph_Tool
{
    public class CrossDissolve
    {
        public static unsafe void DissolveImages(ImageData startImage, ImageData endImage, float percentage, WriteableBitmap outputImage)
        {
            outputImage.Lock();

            int width = outputImage.PixelWidth;
            int height = outputImage.PixelHeight;
            float xStep = 1.0f / width;

            Color* outputData = (Color*)outputImage.BackBuffer;
            Parallel.For(0, outputImage.PixelHeight, yi =>
            {
                Color* outputDataPixel = outputData + yi * width;
                Color* lastOutputDataPixel = outputDataPixel + width;
                float y = (float)yi / height;
                for (float x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
                {
                    *outputDataPixel = Color.Lerp(startImage.Sample(x, y), endImage.Sample(x, y), percentage);
                }
            });

            outputImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, outputImage.PixelWidth, outputImage.PixelHeight));
            outputImage.Unlock();
        }
    }
}
