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
        public static PathGeometry Build(NodeLayout parent, NodeLayout child, double offsetX, double offsetY)
        {
            double x1, y1, x2, y2;
            bool vertical = false;

            if (child.X >= parent.Right - 1)
            {
                // Child on the right side
                x1 = parent.Right; y1 = parent.CenterY;
                x2 = child.X; y2 = child.CenterY;
            }
            else if (child.Right <= parent.X + 1)
            {
                // Child on the left side
                x1 = parent.X; y1 = parent.CenterY;
                x2 = child.Right; y2 = child.CenterY;
            }
            else
            {
                // Child below parent (org chart)
                vertical = true;
                x1 = parent.CenterX; y1 = parent.Bottom;
                x2 = child.CenterX; y2 = child.Y;
            }

            x1 += offsetX; y1 += offsetY;
            x2 += offsetX; y2 += offsetY;

            var figure = new PathFigure { StartPoint = new Point(x1, y1) };

            if (vertical)
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
