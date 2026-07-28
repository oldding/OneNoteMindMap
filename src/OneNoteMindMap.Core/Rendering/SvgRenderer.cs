using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Core.Rendering
{
    public static class SvgRenderer
    {
        public static string Render(MindMapDocument doc, List<NodeLayout> layouts)
        {
            if (layouts.Count == 0) return "";

            double minX = layouts.Min(l => l.X);
            double minY = layouts.Min(l => l.Y);
            double maxX = layouts.Max(l => l.Right);
            double maxY = layouts.Max(l => l.Bottom);

            double pad = 40;
            double width = maxX - minX + pad * 2;
            double height = maxY - minY + pad * 2;
            double offsetX = pad - minX;
            double offsetY = pad - minY;

            var theme = ThemeManager.GetTheme(doc.Settings.Theme);
            string connectionStyle = doc.Settings?.ConnectionStyle ?? "Curved";
            string endpointStyle = doc.Settings?.EndpointStyle ?? "None";
            bool isClassicMindMap = connectionStyle == "ClassicMindMap";

            var sb = new StringBuilder();
            sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width:F0} {height:F0}\" width=\"{width:F0}\" height=\"{height:F0}\">");
            sb.AppendLine($"<rect width=\"{width:F0}\" height=\"{height:F0}\" fill=\"{theme.Background}\"/>");

            var layoutDict = layouts.ToDictionary(l => l.NodeId);

            foreach (var layout in layouts)
            {
                var node = doc.Root.FindById(layout.NodeId);
                if (node == null) continue;

                double x = layout.X + offsetX;
                double y = layout.Y + offsetY;
                double rx = 6;
                double ry = 6;
                string displayText = TruncateText(node.Text);
                if (isClassicMindMap)
                {
                    if (layout.Depth > 0)
                    {
                        string branchColor = ClassicMindMapStyle.GetBranchColor(doc.Root, node);
                        sb.AppendLine($"<path d=\"M{x:F1},{y + layout.Height:F1} L{x + layout.Width:F1},{y + layout.Height:F1}\" fill=\"none\" stroke=\"{branchColor}\" stroke-width=\"2\"/>");
                    }

                    double classicFontSize = layout.Depth == 0 ? 18 : 13;
                    string weight = layout.Depth <= 1 ? "600" : "400";
                    string anchor = layout.Depth == 0 ? "middle" : "start";
                    double textX = layout.Depth == 0 ? x + layout.Width / 2 : x + 4;
                    sb.AppendLine($"<text x=\"{textX:F1}\" y=\"{y + layout.Height - 7:F1}\" text-anchor=\"{anchor}\" font-family=\"{theme.FontFamily}\" font-size=\"{classicFontSize}\" font-weight=\"{weight}\" fill=\"{theme.NodeText}\">{EscapeXml(displayText)}</text>");
                }
                else
                {
                    string fill = GetNodeColor(node, layout.Depth, theme);
                    string textColor = layout.Depth == 0 ? theme.RootText : theme.NodeText;
                    double fontSize = layout.Depth == 0 ? 15 : 13;
                    sb.AppendLine($"<rect x=\"{x:F1}\" y=\"{y:F1}\" width=\"{layout.Width:F1}\" height=\"{layout.Height:F1}\" rx=\"{rx:F1}\" ry=\"{ry:F1}\" fill=\"{fill}\" stroke=\"{theme.Border}\" stroke-width=\"1\"/>");
                    sb.AppendLine($"<text x=\"{x + layout.Width / 2:F1}\" y=\"{y + layout.Height / 2:F1}\" text-anchor=\"middle\" dominant-baseline=\"central\" font-family=\"{theme.FontFamily}\" font-size=\"{fontSize}\" fill=\"{textColor}\">{EscapeXml(displayText)}</text>");
                }
            }

            foreach (var layout in layouts.Where(l => l.Depth > 0))
            {
                var parentNode = doc.Root.FindParentOf(layout.NodeId);
                if (parentNode == null || !layoutDict.TryGetValue(parentNode.Id, out var parentLayout)) continue;

                var anchors = connectionStyle == "ClassicMindMap"
                    ? ConnectorAnchorCalculator.CalculateClassicMindMap(parentLayout, layout)
                    : connectionStyle == "Curved"
                        ? ConnectorAnchorCalculator.CalculateClassicCurve(parentLayout, layout)
                        : ConnectorAnchorCalculator.Calculate(parentLayout, layout);
                double x1 = anchors.StartX + offsetX;
                double y1 = anchors.StartY + offsetY;
                double x2 = anchors.EndX + offsetX;
                double y2 = anchors.EndY + offsetY;
                string lineColor = isClassicMindMap
                    ? ClassicMindMapStyle.GetBranchColor(doc.Root, doc.Root.FindById(layout.NodeId))
                    : theme.Line;

                if (connectionStyle == "Orthogonal")
                {
                    if (anchors.IsVertical)
                    {
                        double middleY = y1 + (y2 - y1) * 0.5;
                        sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} L{x1:F1},{middleY:F1} L{x2:F1},{middleY:F1} L{x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{lineColor}\" stroke-width=\"2\"/>");
                    }
                    else
                    {
                        double middleX = x1 + (x2 - x1) * 0.5;
                        sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} L{middleX:F1},{y1:F1} L{middleX:F1},{y2:F1} L{x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{lineColor}\" stroke-width=\"2\"/>");
                    }
                }
                else if (connectionStyle == "Straight")
                {
                    sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} L{x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{lineColor}\" stroke-width=\"2\"/>");
                }
                else if (anchors.IsVertical)
                {
                    double middleY = y1 + (y2 - y1) * 0.5;
                    sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} C{x1:F1},{middleY:F1} {x2:F1},{middleY:F1} {x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{lineColor}\" stroke-width=\"2\"/>");
                }
                else
                {
                    double middleX = x1 + (x2 - x1) * 0.5;
                    sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} C{middleX:F1},{y1:F1} {middleX:F1},{y2:F1} {x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{lineColor}\" stroke-width=\"2\"/>");
                }

                AppendEndpoint(
                    sb,
                    x1,
                    y1,
                    x2,
                    y2,
                    anchors.IsVertical,
                    connectionStyle,
                    endpointStyle,
                    lineColor);
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        private static string GetNodeColor(MindMapNode node, int depth, Theme theme)
        {
            if (!string.IsNullOrEmpty(node.Color)) return node.Color;
            if (depth == 0) return theme.RootFill;
            if (depth == 1) return theme.Level1Fill;
            if (depth == 2) return theme.Level2Fill;
            return theme.NodeFill;
        }

        private static void AppendEndpoint(
            StringBuilder sb,
            double x1,
            double y1,
            double x2,
            double y2,
            bool isVertical,
            string connectionStyle,
            string endpointStyle,
            string color)
        {
            if (endpointStyle == "None" || connectionStyle == "ClassicMindMap") return;

            double dx;
            double dy;
            if (connectionStyle == "Straight")
            {
                dx = x2 - x1;
                dy = y2 - y1;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (length < 0.001) { dx = 1; dy = 0; }
                else { dx /= length; dy /= length; }
            }
            else if (isVertical)
            {
                dx = 0;
                dy = y2 >= y1 ? 1 : -1;
            }
            else
            {
                dx = x2 >= x1 ? 1 : -1;
                dy = 0;
            }

            if (endpointStyle == "DoubleArrow")
                AppendArrow(sb, x1, y1, -dx, -dy, color);

            if (endpointStyle == "Circle")
            {
                sb.AppendLine($"<circle cx=\"{x2:F1}\" cy=\"{y2:F1}\" r=\"4.5\" fill=\"{color}\"/>");
            }
            else if (endpointStyle == "Diamond")
            {
                double nx = -dy;
                double ny = dx;
                double cx = x2 - dx * 5;
                double cy = y2 - dy * 5;
                sb.AppendLine($"<polygon points=\"{x2:F1},{y2:F1} {cx + nx * 4.5:F1},{cy + ny * 4.5:F1} {x2 - dx * 10:F1},{y2 - dy * 10:F1} {cx - nx * 4.5:F1},{cy - ny * 4.5:F1}\" fill=\"{color}\"/>");
            }
            else
            {
                AppendArrow(sb, x2, y2, dx, dy, color);
            }
        }

        private static void AppendArrow(
            StringBuilder sb,
            double tipX,
            double tipY,
            double dx,
            double dy,
            string color)
        {
            double nx = -dy;
            double ny = dx;
            double bx = tipX - dx * 10;
            double by = tipY - dy * 10;
            sb.AppendLine($"<polygon points=\"{tipX:F1},{tipY:F1} {bx + nx * 5:F1},{by + ny * 5:F1} {bx - nx * 5:F1},{by - ny * 5:F1}\" fill=\"{color}\"/>");
        }

        private static string TruncateText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            string singleLine = text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
            if (singleLine.Length <= 20) return singleLine;
            return singleLine.Substring(0, 17) + "...";
        }

        private static string EscapeXml(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }
    }
}
