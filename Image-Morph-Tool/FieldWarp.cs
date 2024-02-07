using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Image_Morph_Tool.Drawing;
using Image_Morph_Tool.Utils;
using Image_Morph_Tool.Structs;

namespace Image_Morph_Tool
{
    public class FieldWarp
    {
        const float LINE_WEIGHT = 0.03f;

        /**
         * Generates an array of warp markers by iterating through each line marker in the provided line marker set.
         * For each line marker, it constructs a corresponding warp marker with the following properties:
         *
         * @param lineMarkerSet The set of line markers
         * @param isStartImage True if the start image is being warped, false if the end image is being warped
         * @return a list of initialized warp markers
         */
        private static WarpMarker[] CreateWarpMarkers(LineMarkerSet lineMarkerSet, bool isStartImage)
        {
            return lineMarkerSet.Lines.Select(x => new WarpMarker
            {
                targetStart = x.InterpolatedMarker.Start,
                targetDirNorm = x.InterpolatedMarker.End - x.InterpolatedMarker.Start,
                targetPerpNorm = (x.InterpolatedMarker.End - x.InterpolatedMarker.Start).Perpendicular(),
                targetLineLength = (x.InterpolatedMarker.End - x.InterpolatedMarker.Start).Length,

                destStart = isStartImage ? x.StartMarker.Start : x.EndMarker.Start,
                destDirNorm = isStartImage ? x.StartMarker.End - x.StartMarker.Start : x.EndMarker.End - x.EndMarker.Start,
                destPerpNorm = isStartImage ? (x.StartMarker.End - x.StartMarker.Start).Perpendicular() : (x.EndMarker.End - x.EndMarker.Start).Perpendicular(),
            }).ToArray();
        }

        /**
         * Iterates through each warp marker in the array and normalizes its target and destination directional vectors.
         * Normalization ensures that the directional vectors have a unit length, preserving their direction while scaling them to a magnitude of 1.
         * 
         * @param warpMarkers list of warp markers
         */
        private static void NormalizeWarpMarkers(WarpMarker[] warpMarkers)
        {
            for (int markerIndex = 0; markerIndex < warpMarkers.Length; ++markerIndex)
            {
                warpMarkers[markerIndex].targetPerpNorm.Normalize();
                warpMarkers[markerIndex].targetDirNorm.Normalize();
                warpMarkers[markerIndex].destPerpNorm.Normalize();
                warpMarkers[markerIndex].destDirNorm.Normalize();
            }
        }


        /**
         * Iterates through each output pixel along the y-axis, calculating its corresponding position in the input image
         * based on the warp markers and copmutes displacement of the pixel based on the warp markers and their influence,
         * then samples the input image at the computed position to determine the color of the output pixel.
         * 
         * @param yi The y-index of the pixel to process.
         * @param xStep The step size in the x-direction.
         * @param outputData A pointer to the output image data.
         * @param inputImage The input image data.
         * @param markers An array of warp markers.
         * @param outputImageWidth The width of the output image.
         * @param outputImageHeight The height of the output image.
         */
        private static unsafe void ProcessOutputPixel(int yi, double xStep, Color* outputData, ImageData inputImage, WarpMarker[] markers, int outputImageWidth, double outputImageHeight)
        {
            Color* outputDataPixel = outputData + yi * outputImageWidth;
            Color* lastOutputDataPixel = outputDataPixel + outputImageWidth;
            double y = (double)yi / outputImageHeight;

            for (double x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
            {
                Vector position = new Vector(x, y);
                Vector displacement = new Vector(0, 0);
                double weightSum = 0.0f;

                for (int markerIndex = 0; markerIndex < markers.Length; ++markerIndex)
                {
                    Vector toStart = position - markers[markerIndex].targetStart;

                    double u = toStart.Dot(markers[markerIndex].targetDirNorm);
                    double v = toStart.Dot(markers[markerIndex].targetPerpNorm);
                    double weight;

                    if (u < 0)
                    {
                        weight = toStart.LengthSquared;
                    }
                    else if (u > 1)
                    {
                        weight = (toStart + markers[markerIndex].targetDirNorm * markers[markerIndex].targetLineLength).LengthSquared;
                    }
                    else
                    {
                        weight = v * v;
                    }
                    weight = Math.Exp(-weight / LINE_WEIGHT);
                    weightSum += weight;

                    Vector srcPoint = markers[markerIndex].destStart + u * markers[markerIndex].destDirNorm + v * markers[markerIndex].destPerpNorm;
                    displacement += (srcPoint - position) * weight;
                }

                displacement /= weightSum;
                position += displacement;
                position = position.ClampToImageArea();

                *outputDataPixel = inputImage.Sample(position.X, position.Y);
            }
        }

        /**
         * Warps the input image to the output image using the provided marker set, iterating through each pixel in the output image by
         * calculating its corresponding position in the input image based on the warp markers. It then samples the input image at the computed position
         * to determine the color of the output pixel.
         * 
         * @param markerSet The marker set containing warp markers.
         * @param inputImage The input image data.
         * @param outputImage The output image data.
         * @param isStartImage A boolean indicating whether the input image is the start image.
         * @param numThreads The number of threads to use for parallel processing.
         */
        public static unsafe void WarpImage(MarkerSet markerSet, ImageData inputImage, ImageData outputImage, bool isStartImage, int numThreads)
        {
            LineMarkerSet lineMarkerSet = (LineMarkerSet)markerSet;

            double xStep = 1.0 / outputImage.Width;

            WarpMarker[] markers = CreateWarpMarkers(lineMarkerSet, isStartImage);
            NormalizeWarpMarkers(markers);

            if (markers.Length == 0)
            {
                return;
            }

            int height = outputImage.Height;
            int chunkSize = height / numThreads;

            Task[] tasks = new Task[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                int start = i * chunkSize;
                int end = (i == numThreads - 1) ? height : (i + 1) * chunkSize;

                tasks[i] = Task.Factory.StartNew(() =>
                {
                    ProcessOutputPixels(start, end, xStep, (Color*)outputImage.Data, inputImage, markers, outputImage.Width, height);
                });
            }

            Task.WaitAll(tasks);
        }

        /**
         * Processes output pixels in a specified range, warping each pixel based on the provided marker set and input image.
         * Each pixel is calculated via the ProcessOutputPixel function.
         * 
         * @param startY The starting y-index of the range of pixels to process.
         * @param endY The ending y-index (exclusive) of the range of pixels to process.
         * @param xStep The step size in the x-direction.
         * @param outputData A pointer to the output image data.
         * @param inputImage The input image data.
         * @param markers An array of warp markers.
         * @param width The width of the output image.
         * @param height The height of the output image.
         */
        private static unsafe void ProcessOutputPixels(int startY, int endY, double xStep, Color* outputData, ImageData inputImage, WarpMarker[] markers, int width, int height)
        {
            for (int yi = startY; yi < endY; yi++)
            {
                ProcessOutputPixel(yi, xStep, outputData, inputImage, markers, width, height);
            }
        }
    }
}
