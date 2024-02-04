﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Image_Morph_Tool.Utils;

namespace Image_Morph_Tool.Drawing
{
    public abstract class MarkerSet
    {
        public abstract class IMarker
        {
            public abstract void UpdateInterpolatedMarker(float interp);
        }

        public abstract class Marker<T> : IMarker
        {
            public T StartMarker;
            public T EndMarker;
            public T InterpolatedMarker;

            public T this[Location element]
            {
                get
                {
                    switch (element)
                    {
                        case Location.START_IMAGE:
                            return StartMarker;
                        case Location.END_IMAGE:
                            return EndMarker;
                        case Location.OUTPUT_IMAGE:
                            return InterpolatedMarker;
                    }
                    throw new Exception("PointMarker has only 3 Elements!");
                }
                set
                {
                    switch (element)
                    {
                        case Location.START_IMAGE:
                            StartMarker = value;
                            break;
                        case Location.END_IMAGE:
                            EndMarker = value;
                            break;
                        case Location.OUTPUT_IMAGE:
                            InterpolatedMarker = value;
                            break;
                    }
                }
            }
        }

        protected List<IMarker> _markerList = new List<IMarker>();
        protected float _lastInterpolationFactor;

        public enum Location
        {
            START_IMAGE = 0,
            END_IMAGE = 1,
            OUTPUT_IMAGE = 2,
            NONE = -1
        }

        public abstract void OnLeftMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel);
        public abstract void OnLeftMouseButtonUp();
        public abstract void OnRightMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel);
        public abstract bool OnMouseMove(Location clickLocation, Vector imageCor, Vector imageSizePixel);

        public virtual void UpdateInterpolation(float interpolation)
        {
            _lastInterpolationFactor = interpolation;
            foreach (var marker in _markerList)
            {
                marker.UpdateInterpolatedMarker(interpolation);
            }
        }

        public abstract void UpdateMarkerCanvas(Location location, Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel);

        protected const int MARKER_RENDER_SIZE = 10;

        protected void AddPointsToCanvases(IEnumerable<Vector> pointsPerCanvas, int selectedPoint, int hoveredPoint,
                                            Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel)
        {
            int markerIdx = 0;
            foreach (Vector point in pointsPerCanvas)
            {
                var markerRect = new Ellipse();
                markerRect.Width = MARKER_RENDER_SIZE;
                markerRect.Height = MARKER_RENDER_SIZE;
                markerRect.StrokeThickness = 2;

                if (selectedPoint == markerIdx)
                {
                    markerRect.Stroke = new SolidColorBrush(Colors.Red);
                    markerRect.Fill = new SolidColorBrush(Colors.Wheat);
                }
                else if (hoveredPoint == markerIdx)
                {
                    markerRect.Stroke = new SolidColorBrush(Colors.DarkRed);
                    markerRect.Fill = new SolidColorBrush(Colors.Wheat);
                }
                else
                {
                    markerRect.Stroke = new SolidColorBrush(Colors.Black);
                    markerRect.Fill = new SolidColorBrush(Colors.White);
                }

                markerRect.Width = MARKER_RENDER_SIZE;
                markerRect.Height = MARKER_RENDER_SIZE;

                Canvas.SetLeft(markerRect, point.X * imageSizePixel.X + imageOffsetPixel.X - MARKER_RENDER_SIZE / 2);
                Canvas.SetTop(markerRect, point.Y * imageSizePixel.Y + imageOffsetPixel.Y - MARKER_RENDER_SIZE / 2);

                imageCanvas.Children.Add(markerRect);

                ++markerIdx;
            }
        }

        protected int PointHitTest(IEnumerable<Vector> points, Vector imageCor, Vector imageSizePixel)
        {
            Vector halfMarkerSize = new Vector(MARKER_RENDER_SIZE / imageSizePixel.X, MARKER_RENDER_SIZE / imageSizePixel.Y) * 0.5f;

            int i = 0;
            foreach (Vector point in points)
            {
                if (imageCor.IsInRectangle(point - halfMarkerSize, point + halfMarkerSize))
                {
                    return i;
                }
                ++i;
            }

            return -1;
        }
    }
}
