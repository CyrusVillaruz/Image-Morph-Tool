using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image_Morph_Tool.Utils;
using Image_Morph_Tool.Enums;

namespace Image_Morph_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Morph morph = new Morph();

        private BitmapSource _originalSourceImage;
        private BitmapSource _originalDestinationImage;

        private int _selectedNumThreads = 1;

        private bool _isBenchmarking = false;

        public static bool IsReverseChecked { get; set; }

        public const int IMG_WIDTH = 300;
        public const int IMG_HEIGHT = 300;

        public MainWindow()
        {
            InitializeComponent();
            _animPlayer.Tick += AnimationPlayerTimeElapsed;
        }

        #region UI Event Handlers
        private void Image_MarkerDeselect(object sender, MouseEventArgs e)
        {
            morph.MarkerSet.OnLeftMouseButtonUp();
            UpdateMarkerCanvases();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Location location = sender == SourceImage ? Location.START_IMAGE : Location.END_IMAGE;
            morph.MarkerSet.OnLeftMouseButtonDown(location, ComputeRelativeImagePositionFromMouseEvent(sender, e),
                                                        new Vector(((Image)sender).ActualWidth, ((Image)sender).ActualHeight));
            UpdateOutputImageContent();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Location location = sender == SourceImage ? Location.START_IMAGE : Location.END_IMAGE;
            bool changedMarker = morph.MarkerSet.OnMouseMove(location, ComputeRelativeImagePositionFromMouseEvent(sender, e),
                                                        new Vector(((Image)sender).ActualWidth, ((Image)sender).ActualHeight));
            if (!_animPlayer.IsEnabled && changedMarker)
            {
                UpdateOutputImageContent();
            }
            else
            {
                UpdateMarkerCanvases(location);
            }
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Location location = sender == SourceImage ? Location.START_IMAGE : Location.END_IMAGE;
            morph.MarkerSet.OnRightMouseButtonDown(location, ComputeRelativeImagePositionFromMouseEvent(sender, e),
                                                        new Vector(((Image)sender).ActualWidth, ((Image)sender).ActualHeight));
            UpdateOutputImageContent();
        }

        private void LoadDestinationImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = LoadImageFileDialog();
            if (image != null)
            {
                DestinationImage.Source = _originalDestinationImage = image;
                AdaptInputOutputImages();
            }
        }

        private void LoadSourceImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = LoadImageFileDialog();
            if (image != null)
            {
                SourceImage.Source = _originalSourceImage = image;
                AdaptInputOutputImages();
            }
        }

        private void OnProgressChange(object senender, RoutedPropertyChangedEventArgs<double> e)
        {
            int frame = (int)e.NewValue;
            CurrentFrameTextBox.Text = frame.ToString();

            UpdateOutputImageContent();
        }

        private void ReverseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IsReverseChecked = (bool)((CheckBox)sender).IsChecked;
        }

        private void NumThreadsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedNumThreads = NumThreadsSelector.SelectedIndex + 1;
        }

        private void Benchmark_Checked(object sender, RoutedEventArgs e)
        {
            _isBenchmarking = !_isBenchmarking;
            Debug.WriteLine("Benchmarking Enabled: " + _isBenchmarking);
        }

        #endregion

        #region Image Handling
        private BitmapImage? LoadImageFileDialog()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select a picture";
            openDialog.Filter = "All supported images|*.jpg;*.jpeg;*.png;*.gif;*.tiff;*.bmp|" +
                                 "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                 "Portable Network Graphic (*.png)|*.png|" +
                                 "Graphics Interchange Format (*.gif)|*.gif|" +
                                 "Bitmap (*.bmp)|*.bmp|" +
                                 "Tagged Image File Format (*.tiff)|*.tiff";
            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    return new BitmapImage(new Uri(openDialog.FileName));
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        private void AdaptInputOutputImages()
        {
            if (_originalSourceImage == null || _originalDestinationImage == null)
            {
                return;
            }

            SourceImage.Source = _originalSourceImage;
            SourceImage.UpdateLayout();
            SourceImage.Source = FormUtils.CreateResizedImage(_originalSourceImage, IMG_WIDTH, IMG_HEIGHT);
            DestinationImage.Source = _originalDestinationImage;
            DestinationImage.UpdateLayout();
            DestinationImage.Source = FormUtils.CreateResizedImage(_originalDestinationImage, IMG_WIDTH, IMG_HEIGHT);

            OutputImage.Source = new WriteableBitmap(IMG_WIDTH, IMG_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            OutputImage.UpdateLayout();
            SourceImage.UpdateLayout();
            DestinationImage.UpdateLayout();

            morph.SetSourceImage((BitmapSource)SourceImage.Source);
            morph.SetDestinationImage((BitmapSource)DestinationImage.Source);

            if (!_animPlayer.IsEnabled)
            {
                UpdateOutputImageContent();
            }

            UpdateMarkerCanvases();
        }

        private void UpdateOutputImageContent()
        {
            UpdateMarkerCanvases();

            if (OutputImage.Source == null)
            {
                return;
            }

            float progress = IsReverseChecked
                ? 1.0f - (float)((ProgressBar.Value - ProgressBar.Minimum) / (ProgressBar.Maximum - ProgressBar.Minimum))
                : (float)((ProgressBar.Value - ProgressBar.Minimum) / (ProgressBar.Maximum - ProgressBar.Minimum));

            morph.MorphImages(progress, (WriteableBitmap)OutputImage.Source, _selectedNumThreads);
        }

        private void UpdateMarkerCanvases()
        {
            if (morph.MarkerSet != null)
            {
                UpdateMarkerCanvases(Location.START_IMAGE);
                UpdateMarkerCanvases(Location.END_IMAGE);
                UpdateMarkerCanvases(Location.OUTPUT_IMAGE);
            }
        }

        private void UpdateMarkerCanvases(Location location)
        {
            if (morph.MarkerSet != null)
            {
                switch (location)
                {
                    case Location.END_IMAGE:
                    case Location.START_IMAGE:
                        morph.MarkerSet.UpdateMarkerCanvas(Location.START_IMAGE, StartImageMarkerCanvas,
                                                                    new Vector((StartImageMarkerCanvas.ActualWidth - SourceImage.ActualWidth) / 2, (StartImageMarkerCanvas.ActualHeight - SourceImage.ActualHeight) / 2),
                                                                    new Vector(SourceImage.ActualWidth, SourceImage.ActualHeight));
                        morph.MarkerSet.UpdateMarkerCanvas(Location.END_IMAGE, EndImageMarkerCanvas,
                        new Vector((EndImageMarkerCanvas.ActualWidth - DestinationImage.ActualWidth) / 2, (EndImageMarkerCanvas.ActualHeight - DestinationImage.ActualHeight) / 2),
                                                                    new Vector(DestinationImage.ActualWidth, DestinationImage.ActualHeight));
                        break;
                    case Location.OUTPUT_IMAGE:
                        morph.MarkerSet.UpdateMarkerCanvas(Location.OUTPUT_IMAGE, OutputImageMarkerCanvas,
                                                                    new Vector((OutputImageMarkerCanvas.ActualWidth - OutputImage.ActualWidth) / 2, (OutputImageMarkerCanvas.ActualHeight - OutputImage.ActualHeight) / 2),
                                                                    new Vector(OutputImage.ActualWidth, OutputImage.ActualHeight));
                        break;
                }
            }
        }

        private Vector ComputeRelativeImagePositionFromMouseEvent(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(sender as IInputElement);
            return new Vector((float)(pos.X / ((Image)sender).ActualWidth), (float)(pos.Y / ((Image)sender).ActualHeight));
        }

        #endregion
    }
}
