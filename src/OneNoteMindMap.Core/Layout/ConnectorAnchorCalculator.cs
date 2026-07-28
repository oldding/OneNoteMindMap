using System;

namespace OneNoteMindMap.Core.Layout
{
    public struct ConnectorAnchors
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public bool IsVertical { get; set; }
    }

    /// <summary>
    /// Finds connector points on the edges of two node rectangles.
    /// The calculation works for nodes placed on any side of their parent,
    /// including nodes that have been moved manually.
    /// </summary>
    public static class ConnectorAnchorCalculator
    {
        /// <summary>
        /// Uses the classic connector anchors for curved links: the midpoint of
        /// the facing node edges. This preserves the cleaner fan-out used before
        /// free-form node dragging introduced direction-based edge intersections.
        /// </summary>
        public static ConnectorAnchors CalculateClassicCurve(NodeLayout parent, NodeLayout child)
        {
            if (child.X >= parent.Right - 1)
            {
                return new ConnectorAnchors
                {
                    StartX = parent.Right,
                    StartY = parent.CenterY,
                    EndX = child.X,
                    EndY = child.CenterY,
                    IsVertical = false
                };
            }

            if (child.Right <= parent.X + 1)
            {
                return new ConnectorAnchors
                {
                    StartX = parent.X,
                    StartY = parent.CenterY,
                    EndX = child.Right,
                    EndY = child.CenterY,
                    IsVertical = false
                };
            }

            bool childIsBelow = child.CenterY >= parent.CenterY;
            return new ConnectorAnchors
            {
                StartX = parent.CenterX,
                StartY = childIsBelow ? parent.Bottom : parent.Y,
                EndX = child.CenterX,
                EndY = childIsBelow ? child.Y : child.Bottom,
                IsVertical = true
            };
        }

        /// <summary>
        /// Connects classic mind-map branches at the node baseline so the curve
        /// flows directly into the colored line beneath each topic label.
        /// </summary>
        public static ConnectorAnchors CalculateClassicMindMap(NodeLayout parent, NodeLayout child)
        {
            if (child.CenterX >= parent.CenterX)
            {
                return new ConnectorAnchors
                {
                    StartX = parent.Right,
                    StartY = parent.Bottom,
                    EndX = child.X,
                    EndY = child.Bottom,
                    IsVertical = false
                };
            }

            return new ConnectorAnchors
            {
                StartX = parent.X,
                StartY = parent.Bottom,
                EndX = child.Right,
                EndY = child.Bottom,
                IsVertical = false
            };
        }

        public static ConnectorAnchors Calculate(NodeLayout parent, NodeLayout child)
        {
            double deltaX = child.CenterX - parent.CenterX;
            double deltaY = child.CenterY - parent.CenterY;

            if (Math.Abs(deltaX) < 0.001 && Math.Abs(deltaY) < 0.001)
                deltaX = 1;

            double parentScale = BoundaryScale(
                deltaX,
                deltaY,
                parent.Width / 2,
                parent.Height / 2);
            double childScale = BoundaryScale(
                deltaX,
                deltaY,
                child.Width / 2,
                child.Height / 2);

            double normalizedX = Math.Abs(deltaX) /
                Math.Max(1, parent.Width / 2 + child.Width / 2);
            double normalizedY = Math.Abs(deltaY) /
                Math.Max(1, parent.Height / 2 + child.Height / 2);

            return new ConnectorAnchors
            {
                StartX = parent.CenterX + deltaX * parentScale,
                StartY = parent.CenterY + deltaY * parentScale,
                EndX = child.CenterX - deltaX * childScale,
                EndY = child.CenterY - deltaY * childScale,
                IsVertical = normalizedY > normalizedX
            };
        }

        private static double BoundaryScale(
            double deltaX,
            double deltaY,
            double halfWidth,
            double halfHeight)
        {
            halfWidth = Math.Max(1, halfWidth);
            halfHeight = Math.Max(1, halfHeight);

            double ratio = Math.Max(
                Math.Abs(deltaX) / halfWidth,
                Math.Abs(deltaY) / halfHeight);
            return ratio < 0.001 ? 0 : 1 / ratio;
        }
    }
}
