using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OneNoteMindMap.Logging;

namespace OneNoteMindMap.Features
{
    public static class SvgToPngConverter
    {
        public static byte[] ConvertSvgToPng(string svgContent, double scale = 2.0)
        {
            int width = 800;
            int height = 600;

            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(svgContent,
                    @"viewBox=""0 0 (\d+) (\d+)""");
                if (match.Success)
                {
                    width = int.Parse(match.Groups[1].Value);
                    height = int.Parse(match.Groups[2].Value);
                }

                int w = (int)(width * scale);
                int h = (int)(height * scale);

                using (var bmp = new Bitmap(w, h))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    g.Clear(Color.White);

                    DrawSvgContent(g, svgContent, scale);

                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ConvertSvgToPng failed", ex);
                return null;
            }
        }

        private static void DrawSvgContent(Graphics g, string svgContent, double scale)
        {
            var doc = XDocument.Parse(svgContent);
            var ns = doc.Root.Name.Namespace;

            foreach (var rect in doc.Descendants(ns + "rect"))
            {
                float x = (float)(double.Parse((string)rect.Attribute("x") ?? "0") * scale);
                float y = (float)(double.Parse((string)rect.Attribute("y") ?? "0") * scale);
                float w = (float)(double.Parse((string)rect.Attribute("width") ?? "0") * scale);
                float h = (float)(double.Parse((string)rect.Attribute("height") ?? "0") * scale);
                float rx = (float)(double.Parse((string)rect.Attribute("rx") ?? "0") * scale);

                string fill = (string)rect.Attribute("fill") ?? "#FFFFFF";
                string stroke = (string)rect.Attribute("stroke") ?? "#CCCCCC";

                using (var fillBrush = new SolidBrush(ColorTranslator.FromHtml(fill)))
                using (var strokePen = new Pen(ColorTranslator.FromHtml(stroke), 1))
                {
                    if (rx > 0)
                    {
                        g.FillRoundRectangle(fillBrush, x, y, w, h, rx);
                        g.DrawRoundRectangle(strokePen, x, y, w, h, rx);
                    }
                    else
                    {
                        g.FillRectangle(fillBrush, x, y, w, h);
                        g.DrawRectangle(strokePen, x, y, w, h);
                    }
                }
            }

            foreach (var text in doc.Descendants(ns + "text"))
            {
                float fontSize = (float)(double.Parse((string)text.Attribute("font-size") ?? "12") * scale);
                string fill = (string)text.Attribute("fill") ?? "#333333";
                string anchor = (string)text.Attribute("text-anchor") ?? "middle";

                using (var brush = new SolidBrush(ColorTranslator.FromHtml(fill)))
                using (var font = new Font("Microsoft YaHei", fontSize * 72f / 96f, GraphicsUnit.Pixel))
                {
                    var format = new StringFormat(StringFormat.GenericTypographic)
                    {
                        Alignment = anchor == "start" ? StringAlignment.Near :
                            anchor == "end" ? StringAlignment.Far : StringAlignment.Center,
                        LineAlignment = StringAlignment.Near
                    };

                    var spans = text.Elements(ns + "tspan").ToList();
                    if (spans.Count > 0)
                    {
                        foreach (var span in spans)
                        {
                            float x = (float)(double.Parse((string)span.Attribute("x") ?? "0") * scale);
                            float baseline = (float)(double.Parse((string)span.Attribute("y") ?? "0") * scale);
                            g.DrawString(span.Value, font, brush, x, baseline - fontSize * 0.86f, format);
                        }
                    }
                    else
                    {
                        float x = (float)(double.Parse((string)text.Attribute("x") ?? "0") * scale);
                        float y = (float)(double.Parse((string)text.Attribute("y") ?? "0") * scale);
                        g.DrawString(text.Value, font, brush, x, y - fontSize * 0.5f, format);
                    }
                }
            }

            foreach (var path in doc.Descendants(ns + "path"))
            {
                string d = (string)path.Attribute("d") ?? "";
                string stroke = (string)path.Attribute("stroke") ?? "#AAAAAA";

                using (var pen = new Pen(ColorTranslator.FromHtml(stroke), 2))
                {
                    var points = ParseSvgPath(d, scale);
                    if (points.Count > 1)
                    {
                        g.DrawCurve(pen, points.ToArray());
                    }
                }
            }
        }

        private static List<PointF> ParseSvgPath(string d, double scale)
        {
            var points = new List<PointF>();
            var matches = System.Text.RegularExpressions.Regex.Matches(d,
                @"[MC]\s*([\d.]+)\s*[, ]\s*([\d.]+)");

            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                float x = (float)(double.Parse(m.Groups[1].Value) * scale);
                float y = (float)(double.Parse(m.Groups[2].Value) * scale);
                points.Add(new PointF(x, y));
            }

            return points;
        }
    }

    internal static class GraphicsExtensions
    {
        public static void FillRoundRectangle(this Graphics g, Brush brush, float x, float y, float w, float h, float r)
        {
            using (var path = CreateRoundRectPath(x, y, w, h, r))
                g.FillPath(brush, path);
        }

        public static void DrawRoundRectangle(this Graphics g, Pen pen, float x, float y, float w, float h, float r)
        {
            using (var path = CreateRoundRectPath(x, y, w, h, r))
                g.DrawPath(pen, path);
        }

        private static System.Drawing.Drawing2D.GraphicsPath CreateRoundRectPath(float x, float y, float w, float h, float r)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
