using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Image_Morph_Tool.Drawing;
using Image_Morph_Tool.Utils;

namespace Image_Morph_Tool
{
    public class Morph
    {
        public MarkerSet MarkerSet
        {
            get { return _markerSet; }
        }

        private MarkerSet _markerSet = new LineMarkerSet();

        private ImageData _sourceImage;
        private ImageData _destinationImage;
        private ImageData _warpedSourceImage;
        private ImageData _warpedDestinationImage;

        private List<long> previousRuntimes = new List<long>();

        public void SetSourceImage(BitmapSource inputStartImage)
        {
            if (_sourceImage != null)
            {
                _sourceImage.Dispose();
            }

            if (_warpedSourceImage != null)
            { 
                _warpedSourceImage.Dispose(); 
            }

            _warpedSourceImage = new ImageData(inputStartImage.PixelWidth, inputStartImage.PixelHeight);
            _sourceImage = new ImageData(inputStartImage.PixelWidth, inputStartImage.PixelHeight);

            unsafe
            {
                inputStartImage.CopyPixels(Int32Rect.Empty, (IntPtr)_sourceImage.Data, _sourceImage.BufferSize, _sourceImage.Stride);
            }
        }

        public void SetDestinationImage(BitmapSource inputEndImage)
        {
            if (_destinationImage != null)
            {
                _destinationImage.Dispose();
            }

            if (_warpedDestinationImage != null)
            {
                _warpedDestinationImage.Dispose();
            }

            _warpedDestinationImage = new ImageData(inputEndImage.PixelWidth, inputEndImage.PixelHeight);
            _destinationImage = new ImageData(inputEndImage.PixelWidth, inputEndImage.PixelHeight);

            unsafe
            {
                inputEndImage.CopyPixels(Int32Rect.Empty, (IntPtr)_destinationImage.Data, _destinationImage.BufferSize, _destinationImage.Stride);
            }
        }

        public void MorphImages(float morphingProgress, WriteableBitmap outputImage, int numThreads)
        {
            _markerSet.UpdateInterpolation(morphingProgress);

            FieldWarp.WarpImage(_markerSet, _sourceImage, _warpedSourceImage, true, numThreads);
            FieldWarp.WarpImage(_markerSet, _destinationImage, _warpedDestinationImage, false, numThreads);

            CrossDissolve.DissolveImages(_warpedSourceImage, _warpedDestinationImage, morphingProgress, outputImage, numThreads);
        }

        public void BenchmarkMorph(float morphingProgress, WriteableBitmap outputImage, int maxThreads)
        {
            previousRuntimes.Clear();

            _markerSet.UpdateInterpolation(morphingProgress);

            StringBuilder resultBuilder = new StringBuilder();

            for (int numThreads = 1; numThreads <= maxThreads; numThreads++)
            {
                Stopwatch stopwatch = new Stopwatch();

                Debug.WriteLine($"Running Benchmark with {numThreads} Threads");

                stopwatch.Start();
                FieldWarp.WarpImage(_markerSet, _sourceImage, _warpedSourceImage, true, numThreads);
                FieldWarp.WarpImage(_markerSet, _destinationImage, _warpedDestinationImage, false, numThreads);
                CrossDissolve.DissolveImages(_warpedSourceImage, _warpedDestinationImage, morphingProgress, outputImage, numThreads);
                stopwatch.Stop();

                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                resultBuilder.AppendLine($"Morphing with {numThreads} threads runtime: {elapsedMilliseconds} milliseconds");

                previousRuntimes.Add(elapsedMilliseconds);
            }

            for (int i = 2; i <= maxThreads; i++)
            {
                double speedupFactor = (double)previousRuntimes[i - 2] / previousRuntimes[i - 1];
                resultBuilder.AppendLine($"Morphing with {i} threads is {speedupFactor:F2} times faster than morphing with {i - 1} threads");
            }

            MessageBox.Show(resultBuilder.ToString(), "Benchmark Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
