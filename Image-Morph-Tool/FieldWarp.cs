using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Image_Morph_Tool.Drawing;
using Image_Morph_Tool.Utils;

namespace Image_Morph_Tool
{
    public class FieldWarp
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

        const float LINE_WEIGHT = 0.03f;

        public static unsafe void WarpImage(MarkerSet markerSet, ImageData inputImage, ImageData outputImage, bool isStartImage)
        {
            LineMarkerSet lineMarkerSet = (LineMarkerSet)markerSet;

            double xStep = 1.0 / outputImage.Width;

            WarpMarker[] markers = CreateWarpMarkers(lineMarkerSet, isStartImage);
            NormalizeWarpMarkers(markers);

            if (markers.Length == 0)
            {
                return;
            }

            Parallel.For(0, outputImage.Height, yi =>
            {
                ProcessOutputPixel(yi, xStep, (Color*)outputImage.Data, inputImage, markers, outputImage.Width, outputImage.Height);
            });
        }

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

                    // calc relative coordinates to line
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

                    // translation
                    Vector srcPoint = markers[markerIndex].destStart + u * markers[markerIndex].destDirNorm + v * markers[markerIndex].destPerpNorm;
                    displacement += (srcPoint - position) * weight;
                }

                displacement /= weightSum;
                position += displacement;
                position = position.ClampToImageArea();

                *outputDataPixel = inputImage.Sample(position.X, position.Y);
            }
        }
    }
}
