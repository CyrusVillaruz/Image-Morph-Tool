using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Image_Morph_Tool.Utils;

namespace Image_Morph_Tool.Drawing
{
    public class LineMarkerSet : MarkerSet
    {
        public class Line
        {
            public Vector Start;
            public Vector End;
            public Vector Middle;

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
        { 
            get { return _markerList.Cast<LineMarker>(); } 
        }

        private int _draggedEndPoint = -1;
        private int _draggedStartPoint = -1;
        private int _draggedMiddlePoint = -1;
        private int _hoveredStartPoint = -1;
        private int _hoveredEndPoint = -1;
        private int _hoveredMiddlePoint = -1;
        private bool _dragBoth = false;

        private const float MIN_LINE_LENGTH = 0.05f;

        public override void OnLeftMouseButtonDown(Location clickLocation, Vector clickPos, Vector imageSizePixel)
        {
            if (_hoveredStartPoint >= 0 || _hoveredEndPoint >= 0 || _hoveredMiddlePoint >= 0)
            {
                _draggedEndPoint = _hoveredEndPoint;
                _draggedStartPoint = _hoveredStartPoint;
                _draggedMiddlePoint = _hoveredMiddlePoint;
                return;
            }

            _dragBoth = true;
            _draggedEndPoint = _markerList.Count;
            LineMarker newLine = new LineMarker()
            {
                StartMarker = new Line() 
                { 
                    Start = clickPos, 
                    End = clickPos + new Vector(MIN_LINE_LENGTH, 0.0),
                    Middle = clickPos + new Vector(MIN_LINE_LENGTH / 2, 0.0) 
                },

                EndMarker = new Line() 
                { 
                    Start = clickPos, 
                    End = clickPos + new Vector(MIN_LINE_LENGTH, 0.0), 
                    Middle = clickPos + new Vector(MIN_LINE_LENGTH / 2, 0.0) 
                },

                InterpolatedMarker = new Line() 
                { 
                    Start = clickPos, 
                    End = clickPos, 
                    Middle = clickPos 
                }
            };
            _markerList.Add(newLine);
        }

        public override void OnLeftMouseButtonUp()
        {
            _draggedStartPoint = -1;
            _draggedEndPoint = -1;
            _draggedMiddlePoint = -1;
            _dragBoth = false;
        }

        public override void OnRightMouseButtonDown(Location clickLocation, Vector clickPos, Vector imageSizePixel)
        {
            if (_draggedStartPoint >= 0)
            {
                _markerList.RemoveAt(_draggedStartPoint);
            }
            else if (_draggedEndPoint >= 0)
            {
                _markerList.RemoveAt(_draggedEndPoint);
            }
            else if (_draggedMiddlePoint >= 0)
            {
                _markerList.RemoveAt(_draggedMiddlePoint);
            }
            else
            {
                int hit = PointHitTest(Lines.Select(x => x[clickLocation].End), clickPos, imageSizePixel);
                if (hit >= 0)
                {
                    _markerList.RemoveAt(hit);
                }
                else
                {
                    hit = PointHitTest(Lines.Select(x => x[clickLocation].Start), clickPos, imageSizePixel);
                    if (hit >= 0)
                    {
                        _markerList.RemoveAt(hit);
                    }
                }
            }

            _draggedStartPoint = -1;
            _draggedEndPoint = -1;
            _draggedMiddlePoint = -1;
        }

        public override bool OnMouseMove(Location clickLocation, Vector clickPos, Vector imageSizePixel)
        {
            if (_draggedStartPoint >= 0)
            {
                var marker = ((LineMarker)_markerList[_draggedStartPoint])[clickLocation];
                if ((marker.End - clickPos).Length > MIN_LINE_LENGTH)
                {
                    marker.Start = clickPos;
                    marker.Middle = clickPos + (marker.End - clickPos) / 2;
                }
                if (_dragBoth)
                {
                    marker = ((LineMarker)_markerList[_draggedStartPoint])[clickLocation == Location.START_IMAGE ? Location.END_IMAGE : Location.START_IMAGE];
                    if ((marker.End - clickPos).Length > MIN_LINE_LENGTH)
                    {
                        marker.Start = clickPos;
                        marker.Middle = clickPos + (marker.End - clickPos) / 2;
                    }
                }
                _markerList[_draggedStartPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else if (_draggedEndPoint >= 0)
            {
                var marker = ((LineMarker)_markerList[_draggedEndPoint])[clickLocation];
                if ((marker.Start - clickPos).Length > MIN_LINE_LENGTH)
                {
                    marker.End = clickPos;
                    marker.Middle = clickPos + (marker.Start - clickPos) / 2;
                }
                if (_dragBoth)
                {
                    marker = ((LineMarker)_markerList[_draggedEndPoint])[clickLocation == Location.START_IMAGE ? Location.END_IMAGE : Location.START_IMAGE];
                    if ((marker.Start - clickPos).Length > MIN_LINE_LENGTH)
                    {
                        marker.End = clickPos;
                        marker.Middle = clickPos + (marker.Start - clickPos) / 2;
                    }
                }
                _markerList[_draggedEndPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else if (_draggedMiddlePoint >= 0)
            {
                var marker = ((LineMarker)_markerList[_draggedMiddlePoint])[clickLocation];
                var offset = clickPos - marker.Middle;
                marker.Start += offset;
                marker.End += offset;
                marker.Middle = clickPos;
                _markerList[_draggedMiddlePoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else
            {
                _hoveredEndPoint = PointHitTest(Lines.Select(x => x[clickLocation].End), clickPos, imageSizePixel); ;
                if (_hoveredEndPoint < 0)
                {
                    _hoveredStartPoint = PointHitTest(Lines.Select(x => x[clickLocation].Start), clickPos, imageSizePixel);
                }
                else
                {
                    _hoveredStartPoint = -1;
                }

                _hoveredMiddlePoint = PointHitTest(Lines.Select(x => x[clickLocation].Middle), clickPos, imageSizePixel);

                return false;
            }
        }

        public override void UpdateMarkerCanvas(Location location, Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel)
        {
            imageCanvas.Children.Clear();

            for (int markerIndex = 0; markerIndex < _markerList.Count; ++markerIndex)
            {
                LineMarker marker = (LineMarker)_markerList[markerIndex];

                var arrow = new Arrow();
                arrow.HeadHeight = MARKER_RENDER_SIZE / 2;
                arrow.HeadWidth = MARKER_RENDER_SIZE;
                arrow.Stretch = Stretch.None;

                if (markerIndex == _draggedEndPoint || markerIndex == _draggedStartPoint || markerIndex == _draggedMiddlePoint)
                {
                    arrow.Stroke = new SolidColorBrush(Colors.Red);
                }
                else if (markerIndex == _hoveredStartPoint || markerIndex == _hoveredEndPoint || markerIndex == _hoveredMiddlePoint)
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
            AddPointsToCanvases(Lines.Select(x => x[location].Middle), _draggedMiddlePoint, _hoveredMiddlePoint, imageCanvas, imageOffsetPixel, imageSizePixel);
        }
    }
}
