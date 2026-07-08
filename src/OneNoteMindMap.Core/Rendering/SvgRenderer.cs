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
                var node = FindNode(doc.Root, layout.NodeId);
                if (node == null) continue;

                double x = layout.X + offsetX;
                double y = layout.Y + offsetY;
                double rx = 6;
                double ry = 6;
                string fill = GetNodeColor(node, layout.Depth, theme);
                string textColor = layout.Depth == 0 ? theme.RootText : theme.NodeText;
                double fontSize = layout.Depth == 0 ? 15 : 13;

                sb.AppendLine($"<rect x=\"{x:F1}\" y=\"{y:F1}\" width=\"{layout.Width:F1}\" height=\"{layout.Height:F1}\" rx=\"{rx:F1}\" ry=\"{ry:F1}\" fill=\"{fill}\" stroke=\"{theme.Border}\" stroke-width=\"1\"/>");

                string displayText = TruncateText(node.Text, layout.Width - 16);
                sb.AppendLine($"<text x=\"{x + layout.Width / 2:F1}\" y=\"{y + layout.Height / 2:F1}\" text-anchor=\"middle\" dominant-baseline=\"central\" font-family=\"{theme.FontFamily}\" font-size=\"{fontSize}\" fill=\"{textColor}\">{EscapeXml(displayText)}</text>");
            }

            foreach (var layout in layouts.Where(l => l.Depth > 0))
            {
                var parentId = FindParentId(doc.Root, layout.NodeId);
                if (parentId == null || !layoutDict.TryGetValue(parentId, out var parentLayout)) continue;

                double x1 = parentLayout.AnchorRightX + offsetX;
                double y1 = parentLayout.AnchorRightY + offsetY;
                double x2 = layout.AnchorLeftX + offsetX;
                double y2 = layout.AnchorLeftY + offsetY;
                double cx1 = x1 + (x2 - x1) * 0.5;
                double cy1 = y1;
                double cx2 = cx1;
                double cy2 = y2;

                sb.AppendLine($"<path d=\"M{x1:F1},{y1:F1} C{cx1:F1},{cy1:F1} {cx2:F1},{cy2:F1} {x2:F1},{y2:F1}\" fill=\"none\" stroke=\"{theme.Line}\" stroke-width=\"2\"/>");
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        private static MindMapNode FindNode(MindMapNode root, string id)
        {
            if (root.Id == id) return root;
            foreach (var child in root.Children)
            {
                var found = FindNode(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static string FindParentId(MindMapNode root, string childId)
        {
            foreach (var child in root.Children)
            {
                if (child.Id == childId) return root.Id;
                var found = FindParentId(child, childId);
                if (found != null) return found;
            }
            return null;
        }

        private static string GetNodeColor(MindMapNode node, int depth, Theme theme)
        {
            if (!string.IsNullOrEmpty(node.Color)) return node.Color;
            if (depth == 0) return theme.RootFill;
            if (depth == 1) return theme.Level1Fill;
            if (depth == 2) return theme.Level2Fill;
            return theme.NodeFill;
        }

        private static string TruncateText(string text, double maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length <= 20) return text;
            return text.Substring(0, 17) + "...";
        }

        private static string EscapeXml(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }
    }

    public class Theme
    {
        public string Name { get; set; } = "Default";
        public string Background { get; set; } = "#FFFFFF";
        public string RootFill { get; set; } = "#7719AA";
        public string RootText { get; set; } = "#FFFFFF";
        public string Level1Fill { get; set; } = "#FFFFFF";
        public string Level2Fill { get; set; } = "#FFFFFF";
        public string NodeFill { get; set; } = "#FFFFFF";
        public string NodeText { get; set; } = "#333333";
        public string Border { get; set; } = "#CCCCCC";
        public string Line { get; set; } = "#AAAAAA";
        public string FontFamily { get; set; } = "Microsoft YaHei, Arial, sans-serif";
    }

    public static class ThemeManager
    {
        private static readonly Theme DefaultTheme = new Theme();
        private static readonly Theme PurpleTheme = new Theme
        {
            Name = "Purple",
            RootFill = "#7719AA",
            RootText = "#FFFFFF",
            Level1Fill = "#E8D5F5",
            Level2Fill = "#F3E8FB",
            NodeFill = "#FFFFFF",
            Line = "#7719AA"
        };
        private static readonly Theme MinimalTheme = new Theme
        {
            Name = "Minimal",
            Background = "#FFFFFF",
            RootFill = "#333333",
            RootText = "#FFFFFF",
            Level1Fill = "#F5F5F5",
            Level2Fill = "#FAFAFA",
            NodeFill = "#FFFFFF",
            Border = "#CCCCCC",
            Line = "#999999"
        };
        private static readonly Theme FreshTheme = new Theme
        {
            Name = "Fresh",
            RootFill = "#0F766E",
            RootText = "#FFFFFF",
            Level1Fill = "#CCFBF1",
            Level2Fill = "#ECFDF5",
            NodeFill = "#FFFFFF",
            Border = "#99F6E4",
            Line = "#14B8A6"
        };
        private static readonly Theme WarmTheme = new Theme
        {
            Name = "Warm",
            RootFill = "#C2410C",
            RootText = "#FFFFFF",
            Level1Fill = "#FFEDD5",
            Level2Fill = "#FFF7ED",
            NodeFill = "#FFFFFF",
            Border = "#FED7AA",
            Line = "#FB923C"
        };

        public static Theme GetTheme(string name)
        {
            return name switch
            {
                "Purple" => PurpleTheme,
                "Minimal" => MinimalTheme,
                "Fresh" => FreshTheme,
                "Warm" => WarmTheme,
                _ => DefaultTheme
            };
        }
    }
}
