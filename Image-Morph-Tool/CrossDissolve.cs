using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Image_Morph_Tool.Structs;
using Image_Morph_Tool.Utils;

namespace Image_Morph_Tool
{
    /**
     * Contains static methods for blending two images together via cross dissolve.
     */
    public class CrossDissolve
    {
        public static unsafe void DissolveImages(ImageData startImage, ImageData endImage, float percentage, WriteableBitmap outputImage, int numThreads)
        {
            outputImage.Lock();

            int width = outputImage.PixelWidth;
            int height = outputImage.PixelHeight;
            float xStep = 1.0f / width;

            Color* outputData = (Color*)outputImage.BackBuffer;

            int chunkSize = height / numThreads;

            Task[] tasks = new Task[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                int start = i * chunkSize;
                int end = (i == numThreads - 1) ? height : (i + 1) * chunkSize;

                tasks[i] = Task.Factory.StartNew(() =>
                {
                    ProcessImageRows(start, end, xStep, outputData, startImage, endImage, percentage, width, height);
                });
            }

            Task.WaitAll(tasks);

            outputImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, outputImage.PixelWidth, outputImage.PixelHeight));
            outputImage.Unlock();
        }

        private static unsafe void ProcessImageRows(int startY, int endY, float xStep, Color* outputData, ImageData startImage, ImageData endImage, float percentage, int width, int height)
        {
            for (int yi = startY; yi < endY; yi++)
            {
                ProcessImageRow(yi, xStep, outputData, startImage, endImage, percentage, width, height);
            }
        }

        private static unsafe void ProcessImageRow(int yi, float xStep, Color* outputData, ImageData startImage, ImageData endImage, float percentage, int width, int height)
        {
            Color* outputDataPixel = outputData + yi * width;
            Color* lastOutputDataPixel = outputDataPixel + width;
            float y = (float)yi / height;

            for (float x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
            {
                *outputDataPixel = Color.Lerp(startImage.Sample(x, y), endImage.Sample(x, y), percentage);
            }
        }
    }
}
