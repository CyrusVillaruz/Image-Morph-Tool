using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Image_Morph_Tool.Utils
{
    public static class FormUtils
    {
        public static RenderTargetBitmap CreateResizedImage(ImageSource source, int width, int height)
        {
            var group = new DrawingGroup();

            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);

            group.Children.Add(new ImageDrawing(source, new Rect(0, 0, width, height)));

            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(group);
            }

            var resizedImage = new RenderTargetBitmap(
                width, height,
                96, 96,
                PixelFormats.Default);
            resizedImage.Render(drawingVisual);

            return resizedImage;
        }
    }
}
