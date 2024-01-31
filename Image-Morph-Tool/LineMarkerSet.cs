using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Image_Morph_Tool
{
    public class LineMarkerSet : MarkerSet
    {
        public class Line
        {
            public Vector Start;
            public Vector End;

            public static Line Lerp(Line a, Line b, float interp)
            {
                return new Line()
                {
                    Start = a.Start.Lerp(b.Start, interp),
                    End = a.End.Lerp(b.End, interp)
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

        private int _dragedEndPoint = -1;
        private int _dragedStartPoint = -1;
        private int _hoveredStartPoint = -1;
        private int _hoveredEndPoint = -1;
        private bool _dragBoth = false;

        private const float MIN_LINE_LENGTH = 0.05f;

        public override void OnLeftMouseButtonDown(MarkerSet.Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_hoveredStartPoint >= 0 || _hoveredEndPoint >= 0)
            {
                _dragedEndPoint = _hoveredEndPoint;
                _dragedStartPoint = _hoveredStartPoint;
                return;
            }

            _dragBoth = true;
            _dragedEndPoint = _markerList.Count;
            LineMarker newLine = new LineMarker()
            {
                StartMarker = new Line() { Start = imageCor, End = imageCor + new Vector(MIN_LINE_LENGTH, 0.0) },
                EndMarker = new Line() { Start = imageCor, End = imageCor + new Vector(MIN_LINE_LENGTH, 0.0) },
                InterpolatedMarker = new Line() { Start = imageCor, End = imageCor }
            };
            _markerList.Add(newLine);
        }

        public override void OnLeftMouseButtonUp()
        {
            _dragedStartPoint = -1;
            _dragedEndPoint = -1;
            _dragBoth = false;
        }

        public override void OnRightMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_dragedStartPoint >= 0)
                _markerList.RemoveAt(_dragedStartPoint);
            else if (_dragedEndPoint >= 0)
                _markerList.RemoveAt(_dragedEndPoint);
            else
            {
                int hit = PointHitTest(Lines.Select(x => x[clickLocation].End), imageCor, imageSizePixel);
                if (hit >= 0) _markerList.RemoveAt(hit);
                else
                {
                    hit = PointHitTest(Lines.Select(x => x[clickLocation].Start), imageCor, imageSizePixel);
                    if (hit >= 0) _markerList.RemoveAt(hit);
                }
            }

            _dragedStartPoint = -1;
            _dragedEndPoint = -1;
        }

        public override bool OnMouseMove(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_dragedStartPoint >= 0)
            {
                var marker = ((LineMarker)_markerList[_dragedStartPoint])[clickLocation];
                if ((marker.End - imageCor).Length > MIN_LINE_LENGTH)
                {
                    marker.Start = imageCor;
                }
                if (_dragBoth)
                {
                    marker = ((LineMarker)_markerList[_dragedStartPoint])[clickLocation == Location.START_IMAGE ? Location.END_IMAGE : Location.START_IMAGE];
                    if ((marker.End - imageCor).Length > MIN_LINE_LENGTH)
                        marker.Start = imageCor;
                }
                _markerList[_dragedStartPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else if (_dragedEndPoint >= 0)
            {
                var marker = ((LineMarker)_markerList[_dragedEndPoint])[clickLocation];
                if ((marker.Start - imageCor).Length > MIN_LINE_LENGTH)
                    marker.End = imageCor;
                if (_dragBoth)
                {
                    marker = ((LineMarker)_markerList[_dragedEndPoint])[clickLocation == Location.START_IMAGE ? Location.END_IMAGE : Location.START_IMAGE];
                    if ((marker.Start - imageCor).Length > MIN_LINE_LENGTH)
                        marker.End = imageCor;
                }
                _markerList[_dragedEndPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            else
            {
                _hoveredEndPoint = PointHitTest(Lines.Select(x => x[clickLocation].End), imageCor, imageSizePixel); ;
                if (_hoveredEndPoint < 0)
                    _hoveredStartPoint = PointHitTest(Lines.Select(x => x[clickLocation].Start), imageCor, imageSizePixel);
                else
                    _hoveredStartPoint = -1;
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
                if (markerIdx == _dragedEndPoint || markerIdx == _dragedStartPoint)
                    arrow.Stroke = new SolidColorBrush(Colors.Red);
                else if (markerIdx == _hoveredStartPoint || markerIdx == _hoveredEndPoint)
                    arrow.Stroke = new SolidColorBrush(Colors.DarkRed);
                else
                    arrow.Stroke = new SolidColorBrush(Colors.Black);
                arrow.StrokeThickness = 2;
                arrow.X1 = marker[location].Start.X * imageSizePixel.X;
                arrow.X2 = marker[location].End.X * imageSizePixel.X;
                arrow.Y1 = marker[location].Start.Y * imageSizePixel.Y;
                arrow.Y2 = marker[location].End.Y * imageSizePixel.Y;

                Canvas.SetLeft(arrow, imageOffsetPixel.X);
                Canvas.SetTop(arrow, imageOffsetPixel.Y);
                imageCanvas.Children.Add(arrow);

                if ((_hoveredStartPoint == markerIdx || _hoveredEndPoint == markerIdx) && !_dragBoth)
                {
                    AddPointsToCanvases(new List<Vector> { marker[location].Start, marker[location].End },
                                        -1, -1, imageCanvas, imageOffsetPixel, imageSizePixel);
                }
            }
        }
    }
}
