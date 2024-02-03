using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Image_Morph_Tool.Utils;

namespace Image_Morph_Tool
{
    public class LineMarkerSet : MarkerSet
    {
        public class Line
        {
            public Vector Start;
            public Vector End;
            public Vector Middle; // New middle point

            public static Line Lerp(Line a, Line b, float interp)
            {
                return new Line()
                {
                    Start = a.Start.Lerp(b.Start, interp),
                    End = a.End.Lerp(b.End, interp),
                    Middle = a.Middle.Lerp(b.Middle, interp)
                };
            }
        }

        public class LineMarker : Marker<Line>
        {
            public override void UpdateInterpolatedMarker(float interp)
            {
                InterpolatedMarker = Line.Lerp(StartMarker, EndMarker, interp);
            }
        }

        public IEnumerable<LineMarker> Lines
        { get { return _markerList.Cast<LineMarker>(); } }

        private int _draggedEndPoint = -1;
        private int _draggedStartPoint = -1;
        private int _draggedMiddlePoint = -1; // New variable for tracking the dragged middle point
        private int _hoveredStartPoint = -1;
        private int _hoveredEndPoint = -1;
        private int _hoveredMiddlePoint = -1; // New variable for tracking the hovered middle point
        private bool _dragBoth = false;

        private const float MIN_LINE_LENGTH = 0.05f;

        public override void OnLeftMouseButtonDown(MarkerSet.Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_hoveredStartPoint >= 0 || _hoveredEndPoint >= 0 || _hoveredMiddlePoint >= 0)
            {
                _draggedEndPoint = _hoveredEndPoint;
                _draggedStartPoint = _hoveredStartPoint;
                _draggedMiddlePoint = _hoveredMiddlePoint; // Start dragging the middle point
                return;
            }

            _dragBoth = true;
            _draggedEndPoint = _markerList.Count;
            LineMarker newLine = new LineMarker()
            {
                StartMarker = new Line() { Start = imageCor, End = imageCor + new Vector(MIN_LINE_LENGTH, 0.0), Middle = imageCor + new Vector(MIN_LINE_LENGTH / 2, 0.0) }, // Initialize the middle point
                EndMarker = new Line() { Start = imageCor, End = imageCor + new Vector(MIN_LINE_LENGTH, 0.0), Middle = imageCor + new Vector(MIN_LINE_LENGTH / 2, 0.0) },
                InterpolatedMarker = new Line() { Start = imageCor, End = imageCor, Middle = imageCor } // Initialize the middle point
            };
            _markerList.Add(newLine);
        }

        public override void OnLeftMouseButtonUp()
        {
            _draggedStartPoint = -1;
            _draggedEndPoint = -1;
            _draggedMiddlePoint = -1; // Stop dragging the middle point
            _dragBoth = false;
        }

        public override void OnRightMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_draggedStartPoint >= 0)
            {
                _markerList.RemoveAt(_draggedStartPoint);
            }
            else if (_draggedEndPoint >= 0)
            {
                _markerList.RemoveAt(_draggedEndPoint);
            }
            else if (_draggedMiddlePoint >= 0) // Remove the middle point if right-clicked
            {
                _markerList.RemoveAt(_draggedMiddlePoint);
            }
            else
            {
                int hit = PointHitTest(Lines.Select(x => x[clickLocation].End), imageCor, imageSizePixel);
                if (hit >= 0)
                {
                    _markerList.RemoveAt(hit);
                }
                else
                {
                    hit = PointHitTest(Lines.Select(x => x[clickLocation].Start), imageCor, imageSizePixel);
                    if (hit >= 0)
                    {
                        _markerList.RemoveAt(hit);
                    }
                }
            }

            _draggedStartPoint = -1;
            _draggedEndPoint = -1;
            _draggedMiddlePoint = -1; // Stop dragging the middle point
        }

        public override bool OnMouseMove(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_draggedStartPoint >= 0)
            {
                var marker = ((LineMarker)_markerList[_draggedStartPoint])[clickLocation];
                if ((marker.End - imageCor).Length > MIN_LINE_LENGTH)
                {
                    marker.Start = imageCor;
                    marker.Middle = imageCor + (marker.End - imageCor) / 2; // Update the middle point when moving the start point
                }
                if (_dragBoth)
                {
                    marker = ((LineMarker)_markerList[_draggedStartPoint])[clickLocation == Location.START_IMAGE ? Location.END_IMAGE : Location.START_IMAGE];
                    if ((marker.End - imageCor).Length > MIN_LINE_LENGTH)
                    {
                        marker.Start = imageCor;
                        marker.Middle = imageCor + (marker.End - imageCor) / 2; // Update the middle point when moving the start point
                    }
                }
                _markerList[_draggedStartPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else if (_draggedEndPoint >= 0)
            {
                var marker = ((LineMarker)_markerList[_draggedEndPoint])[clickLocation];
                if ((marker.Start - imageCor).Length > MIN_LINE_LENGTH)
                {
                    marker.End = imageCor;
                    marker.Middle = imageCor + (marker.Start - imageCor) / 2; // Update the middle point when moving the end point
                }
                if (_dragBoth)
                {
                    marker = ((LineMarker)_markerList[_draggedEndPoint])[clickLocation == Location.START_IMAGE ? Location.END_IMAGE : Location.START_IMAGE];
                    if ((marker.Start - imageCor).Length > MIN_LINE_LENGTH)
                    {
                        marker.End = imageCor;
                        marker.Middle = imageCor + (marker.Start - imageCor) / 2; // Update the middle point when moving the end point
                    }
                }
                _markerList[_draggedEndPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else if (_draggedMiddlePoint >= 0) // Move the entire line when dragging the middle point
            {
                var marker = ((LineMarker)_markerList[_draggedMiddlePoint])[clickLocation];
                var offset = imageCor - marker.Middle; // Calculate the offset from the original middle point
                marker.Start += offset;
                marker.End += offset;
                marker.Middle = imageCor; // Update the middle point
                _markerList[_draggedMiddlePoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else
            {
                _hoveredEndPoint = PointHitTest(Lines.Select(x => x[clickLocation].End), imageCor, imageSizePixel); ;
                if (_hoveredEndPoint < 0)
                {
                    _hoveredStartPoint = PointHitTest(Lines.Select(x => x[clickLocation].Start), imageCor, imageSizePixel);
                }
                else
                {
                    _hoveredStartPoint = -1;
                }

                // Check for the middle point
                _hoveredMiddlePoint = PointHitTest(Lines.Select(x => x[clickLocation].Middle), imageCor, imageSizePixel);

                return false;
            }
        }

        public override void UpdateMarkerCanvas(Location location, Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel)
        {
            imageCanvas.Children.Clear();

            for (int markerIdx = 0; markerIdx < _markerList.Count; ++markerIdx)
            {
                LineMarker marker = (LineMarker)_markerList[markerIdx];

                var arrow = new Arrow();
                arrow.HeadHeight = MarkerSet.MARKER_RENDER_SIZE / 2;
                arrow.HeadWidth = MarkerSet.MARKER_RENDER_SIZE;
                arrow.Stretch = Stretch.None;

                if (markerIdx == _draggedEndPoint || markerIdx == _draggedStartPoint || markerIdx == _draggedMiddlePoint)
                {
                    arrow.Stroke = new SolidColorBrush(Colors.Red);
                }
                else if (markerIdx == _hoveredStartPoint || markerIdx == _hoveredEndPoint || markerIdx == _hoveredMiddlePoint)
                {
                    arrow.Stroke = new SolidColorBrush(Colors.DarkRed);
                }
                else
                {
                    arrow.Stroke = new SolidColorBrush(Colors.Black);
                }

                arrow.StrokeThickness = 2;
                arrow.X1 = marker[location].Start.X * imageSizePixel.X;
                arrow.X2 = marker[location].End.X * imageSizePixel.X;
                arrow.Y1 = marker[location].Start.Y * imageSizePixel.Y;
                arrow.Y2 = marker[location].End.Y * imageSizePixel.Y;

                Canvas.SetLeft(arrow, imageOffsetPixel.X);
                Canvas.SetTop(arrow, imageOffsetPixel.Y);
                imageCanvas.Children.Add(arrow);
            }

            AddPointsToCanvases(Lines.Select(x => x[location].Start), _draggedStartPoint, _hoveredStartPoint, imageCanvas, imageOffsetPixel, imageSizePixel);
            AddPointsToCanvases(Lines.Select(x => x[location].End), _draggedEndPoint, _hoveredEndPoint, imageCanvas, imageOffsetPixel, imageSizePixel);
            AddPointsToCanvases(Lines.Select(x => x[location].Middle), _draggedMiddlePoint, _hoveredMiddlePoint, imageCanvas, imageOffsetPixel, imageSizePixel); // Add middle point
        }
    }
}
