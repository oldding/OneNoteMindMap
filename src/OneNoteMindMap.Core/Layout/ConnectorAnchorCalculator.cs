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
