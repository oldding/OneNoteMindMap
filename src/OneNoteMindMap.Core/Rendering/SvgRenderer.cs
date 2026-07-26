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
                string fill = GetNodeColor(node, layout.Depth, theme);
                string textColor = layout.Depth == 0 ? theme.RootText : theme.NodeText;
                double fontSize = layout.Depth == 0 ? 15 : 13;

                sb.AppendLine($"<rect x=\"{x:F1}\" y=\"{y:F1}\" width=\"{layout.Width:F1}\" height=\"{layout.Height:F1}\" rx=\"{rx:F1}\" ry=\"{ry:F1}\" fill=\"{fill}\" stroke=\"{theme.Border}\" stroke-width=\"1\"/>");

                string displayText = TruncateText(node.Text);
                sb.AppendLine($"<text x=\"{x + layout.Width / 2:F1}\" y=\"{y + layout.Height / 2:F1}\" text-anchor=\"middle\" dominant-baseline=\"central\" font-family=\"{theme.FontFamily}\" font-size=\"{fontSize}\" fill=\"{textColor}\">{EscapeXml(displayText)}</text>");
            }

            foreach (var layout in layouts.Where(l => l.Depth > 0))
            {
                var parentNode = doc.Root.FindParentOf(layout.NodeId);
                if (parentNode == null || !layoutDict.TryGetValue(parentNode.Id, out var parentLayout)) continue;

                var anchors = ConnectorAnchorCalculator.Calculate(parentLayout, layout);
                double x1 = anchors.StartX + offsetX;
                double y1 = anchors.StartY + offsetY;
                double x2 = anchors.EndX + offsetX;
                double y2 = anchors.EndY + offsetY;

                if (doc.Settings?.ConnectionStyle == "Orthogonal")
                {
                    if (anchors.IsVertical)
                    {
                        double middleY = y1 + (y2 - y1) * 0.5;
                        sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} L{x1:F1},{middleY:F1} L{x2:F1},{middleY:F1} L{x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{theme.Line}\" stroke-width=\"2\"/>");
                    }
                    else
                    {
                        double middleX = x1 + (x2 - x1) * 0.5;
                        sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} L{middleX:F1},{y1:F1} L{middleX:F1},{y2:F1} L{x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{theme.Line}\" stroke-width=\"2\"/>");
                    }
                }
                else if (doc.Settings?.ConnectionStyle == "Straight")
                {
                    sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} L{x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{theme.Line}\" stroke-width=\"2\"/>");
                }
                else if (anchors.IsVertical)
                {
                    double middleY = y1 + (y2 - y1) * 0.5;
                    sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} C{x1:F1},{middleY:F1} {x2:F1},{middleY:F1} {x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{theme.Line}\" stroke-width=\"2\"/>");
                }
                else
                {
                    double middleX = x1 + (x2 - x1) * 0.5;
                    sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} C{middleX:F1},{y1:F1} {middleX:F1},{y2:F1} {x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{theme.Line}\" stroke-width=\"2\"/>");
                }
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
