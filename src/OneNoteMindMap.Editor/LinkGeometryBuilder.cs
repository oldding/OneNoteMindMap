using System.Windows;
using System.Windows.Media;
using OneNoteMindMap.Core.Layout;

namespace OneNoteMindMap.Editor
{
    /// <summary>
    /// Builds parent-child connector geometry shared by the editor canvas
    /// and the PNG exporter, so on-screen and inserted images always match.
    /// Chooses anchors based on the child position relative to its parent:
    /// right side, left side (BothSides layout) or below (OrgChart layout).
    /// </summary>
    public static class LinkGeometryBuilder
    {
        public static PathGeometry Build(
            NodeLayout parent,
            NodeLayout child,
            double offsetX,
            double offsetY,
            string connectionStyle = "Curved")
        {
            var anchors = ConnectorAnchorCalculator.Calculate(parent, child);
            double x1 = anchors.StartX;
            double y1 = anchors.StartY;
            double x2 = anchors.EndX;
            double y2 = anchors.EndY;

            x1 += offsetX; y1 += offsetY;
            x2 += offsetX; y2 += offsetY;

            var figure = new PathFigure { StartPoint = new Point(x1, y1) };

            if (connectionStyle == "Orthogonal")
            {
                var segment = new PolyLineSegment { IsStroked = true };
                if (anchors.IsVertical)
                {
                    double middleY = y1 + (y2 - y1) * 0.5;
                    segment.Points.Add(new Point(x1, middleY));
                    segment.Points.Add(new Point(x2, middleY));
                }
                else
                {
                    double middleX = x1 + (x2 - x1) * 0.5;
                    segment.Points.Add(new Point(middleX, y1));
                    segment.Points.Add(new Point(middleX, y2));
                }
                segment.Points.Add(new Point(x2, y2));
                figure.Segments.Add(segment);
            }
            else if (connectionStyle == "Straight")
            {
                figure.Segments.Add(new LineSegment(new Point(x2, y2), true));
            }
            else if (anchors.IsVertical)
            {
                double my = y1 + (y2 - y1) * 0.5;
                figure.Segments.Add(new BezierSegment(
                    new Point(x1, my), new Point(x2, my), new Point(x2, y2), true));
            }
            else
            {
                double mx = x1 + (x2 - x1) * 0.5;
                figure.Segments.Add(new BezierSegment(
                    new Point(mx, y1), new Point(mx, y2), new Point(x2, y2), true));
            }

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }
    }
}
