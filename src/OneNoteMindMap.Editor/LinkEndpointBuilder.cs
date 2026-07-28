using System;
using System.Windows;
using System.Windows.Media;
using OneNoteMindMap.Core.Layout;

namespace OneNoteMindMap.Editor
{
    public static class LinkEndpointBuilder
    {
        public static Geometry Build(
            NodeLayout parent,
            NodeLayout child,
            double offsetX,
            double offsetY,
            string connectionStyle,
            string endpointStyle)
        {
            var group = new GeometryGroup();
            if (endpointStyle == "None" || connectionStyle == "ClassicMindMap")
                return group;

            var anchors = LinkGeometryBuilder.GetAnchors(parent, child, connectionStyle);
            var start = new Point(anchors.StartX + offsetX, anchors.StartY + offsetY);
            var end = new Point(anchors.EndX + offsetX, anchors.EndY + offsetY);
            Vector forward = GetDirection(start, end, anchors.IsVertical, connectionStyle);

            switch (endpointStyle)
            {
                case "DoubleArrow":
                    AddArrow(group, start, -forward);
                    AddArrow(group, end, forward);
                    break;
                case "Circle":
                    group.Children.Add(new EllipseGeometry(end, 4.5, 4.5));
                    break;
                case "Diamond":
                    AddDiamond(group, end, forward);
                    break;
                default:
                    AddArrow(group, end, forward);
                    break;
            }

            return group;
        }

        private static Vector GetDirection(
            Point start,
            Point end,
            bool isVertical,
            string connectionStyle)
        {
            Vector direction;
            if (connectionStyle == "Straight")
                direction = end - start;
            else if (isVertical)
                direction = new Vector(0, end.Y >= start.Y ? 1 : -1);
            else
                direction = new Vector(end.X >= start.X ? 1 : -1, 0);

            if (direction.Length < 0.001)
                direction = new Vector(1, 0);
            direction.Normalize();
            return direction;
        }

        private static void AddArrow(GeometryGroup group, Point tip, Vector direction)
        {
            Vector normal = new Vector(-direction.Y, direction.X);
            Point baseCenter = tip - direction * 10;
            AddPolygon(group, tip, baseCenter + normal * 5, baseCenter - normal * 5);
        }

        private static void AddDiamond(GeometryGroup group, Point tip, Vector direction)
        {
            Vector normal = new Vector(-direction.Y, direction.X);
            Point center = tip - direction * 5;
            AddPolygon(
                group,
                tip,
                center + normal * 4.5,
                tip - direction * 10,
                center - normal * 4.5);
        }

        private static void AddPolygon(GeometryGroup group, params Point[] points)
        {
            var figure = new PathFigure { StartPoint = points[0], IsClosed = true };
            for (int i = 1; i < points.Length; i++)
                figure.Segments.Add(new LineSegment(points[i], true));
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            group.Children.Add(geometry);
        }
    }
}
