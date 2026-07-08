using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Core.Rendering;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class InsertPreviewCommand
    {
        private const string PreviewMetaName = "onenote-mindmap-preview-v1";

        public static void Execute()
        {
            try
            {
                string pageId = OneNoteProvider.GetCurrentPageId();
                if (string.IsNullOrEmpty(pageId))
                {
                    Msg.Warn(Strings.If("无法获取当前页面", "Cannot get current page"));
                    return;
                }

                var doc = MindMapPageStore.LoadMindMapFromPage(pageId);
                if (doc == null)
                {
                    Msg.Warn(Strings.NoMapDataFound);
                    return;
                }

                if (!InsertOrUpdatePreview(pageId, doc))
                    return;

                Msg.Info(Strings.InsertPreviewSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error("InsertPreviewCommand failed", ex);
                Msg.Error(Strings.If("插入预览图失败：" + ex.Message, "Failed to insert preview: " + ex.Message));
            }
        }

        private static bool IsPreviewOutline(XElement outline, XNamespace ns)
        {
            if (outline.Descendants(ns + "Meta")
                .Any(m => string.Equals((string)m.Attribute("name"), PreviewMetaName, StringComparison.Ordinal)))
                return true;

            // Legacy previews inserted before the meta marker existed.
            return outline.Descendants(ns + "Image").Any(img =>
            {
                string alt = (string)img.Attribute("alt") ?? (string)img.Attribute("altText");
                return alt != null &&
                       (alt.StartsWith("脑图预览 - ", StringComparison.Ordinal) ||
                        alt.StartsWith("Mind Map Preview - ", StringComparison.Ordinal));
            });
        }

        public static bool InsertOrUpdatePreview(string pageId, MindMapDocument doc)
        {
            try
            {
                var pngBytes = OneNoteMindMap.Editor.PngExporter.Export(doc, 2.0);
                if (pngBytes == null || pngBytes.Length == 0)
                {
                    Msg.Warn(Strings.If("生成预览图失败", "Failed to generate preview image"));
                    return false;
                }

                string pageXml = OneNoteProvider.GetPageContent(pageId, out _);
                if (string.IsNullOrEmpty(pageXml))
                {
                    Msg.Warn(Strings.If("无法读取页面内容", "Cannot read page content"));
                    return false;
                }

                var pageDoc = XDocument.Parse(pageXml);
                var ns = pageDoc.Root.Name.Namespace;

                foreach (var oldPreview in pageDoc.Root.Elements(ns + "Outline")
                    .Where(o => IsPreviewOutline(o, ns))
                    .ToList())
                {
                    oldPreview.Remove();
                }

                var imageOutline = PageWriter.CreateImageOutline(pngBytes,
                    Strings.If("脑图预览 - ", "Mind Map Preview - ") + doc.Title,
                    PreviewMetaName);
                pageDoc.Root.Add(imageOutline);

                OneNoteProvider.UpdatePageContent(pageId, pageDoc.ToString());
                OneNoteProvider.NavigateTo(pageId);

                Logger.Info("Preview image inserted into page: " + pageId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("InsertOrUpdatePreview failed", ex);
                Msg.Error(Strings.If("插入预览图失败：" + ex.Message, "Failed to insert preview: " + ex.Message));
                return false;
            }
        }

    }

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
                float x = (float)(double.Parse((string)text.Attribute("x") ?? "0") * scale);
                float y = (float)(double.Parse((string)text.Attribute("y") ?? "0") * scale);
                string content = text.Value;
                float fontSize = (float)(double.Parse((string)text.Attribute("font-size") ?? "12") * scale);
                string fill = (string)text.Attribute("fill") ?? "#333333";

                using (var brush = new SolidBrush(ColorTranslator.FromHtml(fill)))
                using (var font = new Font("Microsoft YaHei", fontSize * 72f / 96f, GraphicsUnit.Pixel))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(content, font, brush, x, y, format);
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

        private static System.Collections.Generic.List<PointF> ParseSvgPath(string d, double scale)
        {
            var points = new System.Collections.Generic.List<PointF>();
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
