﻿using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Image_Morph_Tool
{
    /**
     * Part of the MainWindow, separated purely for handling the morphing animation and benchmarking.
     */
    public partial class MainWindow
    {
        private DispatcherTimer _animPlayer = new DispatcherTimer();
        private System.Diagnostics.Stopwatch _animStopWatch = new System.Diagnostics.Stopwatch();

        private void AnimationPlayerTimeElapsed(object sender, EventArgs e)
        {
            double progress = _animStopWatch.Elapsed.TotalSeconds / (double)Duration.Value;

            ProgressBar.Value = Math.Min(ProgressBar.Maximum, ProgressBar.Minimum + (ProgressBar.Maximum - ProgressBar.Minimum) * progress);

            if (progress >= 1.0)
            {
                StopAutoAnimation();
            }
        }

        private void StopAutoAnimation()
        {
            _animPlayer.Stop();
            ProgressBar.IsEnabled = true;
            PlayAnimationButton.Content = "Start";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBenchmarking)
            {
                BenchmarkTextBox.Text = "Benchmarking...";

                float progress = IsReverseChecked
                    ? 1.0f - (float)((ProgressBar.Value - ProgressBar.Minimum) / (ProgressBar.Maximum - ProgressBar.Minimum))
                    : (float)((ProgressBar.Value - ProgressBar.Minimum) / (ProgressBar.Maximum - ProgressBar.Minimum));
                morph.BenchmarkMorph(progress, (WriteableBitmap)OutputImage.Source, _selectedNumThreads);

                BenchmarkTextBox.Text = "No Benchmark in Progress...";
            }
            else if (!_animPlayer.IsEnabled)
            {
                ProgressBar.IsEnabled = false;
                PlayAnimationButton.Content = "Stop";
                _animPlayer.Interval = new TimeSpan(0, 0, 0, 0, (int)((double)Duration.Value / (double)NumFrames.Value * 1000.0));
                _animStopWatch.Restart();
                _animPlayer.Start();
            }
            else
            {
                StopAutoAnimation();
            }
        }

        private void NumberOfFrames_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateOutputImageContent();
        }
    }
}
