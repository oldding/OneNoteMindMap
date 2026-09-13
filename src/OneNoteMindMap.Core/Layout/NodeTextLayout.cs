using System;
using System.Collections.Generic;
using System.Linq;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Core.Parsing;

namespace OneNoteMindMap.Core.Layout
{
    public struct NodeSize
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public static class NodeTextLayout
    {
        private const double HorizontalPadding = 24;
        private const double VerticalPadding = 14;

        public static string GetDisplayText(MindMapNode node)
        {
            string text = node?.Text ?? "";
            return string.IsNullOrWhiteSpace(node?.Icon)
                ? text
                : node.Icon.Trim() + "  " + text;
        }

        public static NodeSize Measure(MindMapNode node, LayoutOptions options, bool isRoot)
        {
            double minimumHeight = isRoot ? Math.Max(50, options.NodeHeight) : options.NodeHeight;
            if (!string.Equals(options.NodeSizeMode, "AutoFit", StringComparison.OrdinalIgnoreCase))
            {
                return new NodeSize
                {
                    Width = options.NodeWidth,
                    Height = minimumHeight
                };
            }

            double fontSize = isRoot && options.ConnectionStyle == "ClassicMindMap" ? 18 : isRoot ? 15 : 13;
            double halfWidthPixels = fontSize * 0.56;
            string displayText = GetDisplayText(node);
            var sourceLines = NormalizeLines(displayText);
            int longestLineWidth = Math.Max(1, sourceLines.Max(TextDisplayWidthHelper.DisplayWidth));
            double naturalWidth = longestLineWidth * halfWidthPixels + HorizontalPadding;
            double width = Math.Max(options.AutoNodeMinWidth,
                Math.Min(options.AutoNodeMaxWidth, naturalWidth));

            var wrappedLines = WrapText(displayText, width - HorizontalPadding, fontSize);
            double lineHeight = fontSize * 1.45;
            double height = Math.Max(minimumHeight, wrappedLines.Count * lineHeight + VerticalPadding);
            return new NodeSize { Width = Math.Ceiling(width), Height = Math.Ceiling(height) };
        }

        public static List<string> WrapText(string text, double availableWidth, double fontSize)
        {
            int maxDisplayWidth = Math.Max(1, (int)Math.Floor(availableWidth / (fontSize * 0.56)));
            var result = new List<string>();
            foreach (string sourceLine in NormalizeLines(text))
                WrapLine(sourceLine, maxDisplayWidth, result);
            if (result.Count == 0)
                result.Add("");
            return result;
        }

        private static List<string> NormalizeLines(string text)
        {
            return (text ?? "")
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n')
                .ToList();
        }

        private static void WrapLine(string line, int maxDisplayWidth, List<string> result)
        {
            if (line.Length == 0)
            {
                result.Add("");
                return;
            }

            int position = 0;
            while (position < line.Length)
            {
                int width = 0;
                int end = position;
                int lastWhitespace = -1;
                while (end < line.Length)
                {
                    int charWidth = TextDisplayWidthHelper.DisplayWidth(line[end].ToString());
                    if (width + charWidth > maxDisplayWidth && end > position)
                        break;
                    width += charWidth;
                    if (char.IsWhiteSpace(line[end]))
                        lastWhitespace = end;
                    end++;
                    if (width >= maxDisplayWidth)
                        break;
                }

                if (end < line.Length && lastWhitespace >= position)
                    end = lastWhitespace + 1;
                if (end <= position)
                    end = position + 1;

                string segment = line.Substring(position, end - position).Trim();
                result.Add(segment);
                position = end;
                while (position < line.Length && char.IsWhiteSpace(line[position]))
                    position++;
            }
        }
    }
}
