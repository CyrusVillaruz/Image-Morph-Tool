using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void SetSourceImage(BitmapSource inputStartImage)
        {
            if (_sourceImage != null)
                _sourceImage.Dispose();
            if (_warpedSourceImage != null)
                _warpedSourceImage.Dispose();

            _warpedSourceImage = new ImageData(inputStartImage.PixelWidth, inputStartImage.PixelHeight);
            _sourceImage = new ImageData(inputStartImage.PixelWidth, inputStartImage.PixelHeight);
            unsafe
            {
                inputStartImage.CopyPixels(System.Windows.Int32Rect.Empty, (IntPtr)_sourceImage.Data, _sourceImage.BufferSize, _sourceImage.Stride);
            }
        }

        public void SetDestinationImage(BitmapSource inputEndImage)
        {
            if (_destinationImage != null)
                _destinationImage.Dispose();
            if (_warpedDestinationImage != null)
                _warpedDestinationImage.Dispose();

            _warpedDestinationImage = new ImageData(inputEndImage.PixelWidth, inputEndImage.PixelHeight);
            _destinationImage = new ImageData(inputEndImage.PixelWidth, inputEndImage.PixelHeight);
            unsafe
            {
                inputEndImage.CopyPixels(System.Windows.Int32Rect.Empty, (IntPtr)_destinationImage.Data, _destinationImage.BufferSize, _destinationImage.Stride);
            }
        }

        public void MorphImages(float morphingProgress, WriteableBitmap outputImage)
        {
            MorphReverse(morphingProgress, outputImage);
        }

        private void MorphForward(float morphingProgress, WriteableBitmap outputImage)
        {
            _markerSet.UpdateInterpolation(morphingProgress);

            FieldWarp.WarpImage(_markerSet, _sourceImage, _warpedSourceImage, true);
            FieldWarp.WarpImage(_markerSet, _destinationImage, _warpedDestinationImage, false);

            CrossDissolve.DissolveImages(_warpedSourceImage, _warpedDestinationImage, morphingProgress, outputImage);
        }

        private void MorphReverse(float morphingProgress, WriteableBitmap outputImage)
        {
            // TODO: update markers so that warp goes from destination to source instead
            _markerSet.UpdateInterpolation(morphingProgress);

            FieldWarp.WarpImage(_markerSet, _sourceImage, _warpedSourceImage, false);
            FieldWarp.WarpImage(_markerSet, _destinationImage, _warpedDestinationImage, true);

            CrossDissolve.DissolveImages(_warpedDestinationImage, _warpedSourceImage, morphingProgress, outputImage);
        }
    }
}
