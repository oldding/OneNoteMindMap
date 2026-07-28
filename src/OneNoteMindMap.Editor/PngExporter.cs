using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Core.Rendering;

namespace OneNoteMindMap.Editor
{
    public static class PngExporter
    {
        public static byte[] Export(MindMapDocument doc, double scale = 2.0)
        {
            var engine = new MindMapLayoutEngine();
            engine.Options.Layout = doc.Settings?.Layout ?? "RightTree";
            engine.Options.Theme = doc.Settings?.Theme ?? "Default";
            engine.Options.NodeShape = doc.Settings?.NodeShape ?? "Rounded";
            engine.Options.ConnectionStyle = doc.Settings?.ConnectionStyle ?? "Curved";
            engine.Options.EndpointStyle = doc.Settings?.EndpointStyle ?? "None";
            engine.Options.NodeWidth = doc.Settings?.NodeWidth ?? 160;
            engine.Options.NodeHeight = doc.Settings?.NodeHeight ?? 44;
            engine.Options.HorizontalGap = doc.Settings?.HorizontalGap ?? 90;
            engine.Options.VerticalGap = doc.Settings?.VerticalGap ?? 24;
            engine.Options.LevelGap = doc.Settings?.LevelGap ?? 110;
            var layouts = engine.CalculateLayout(doc.Root);
            if (layouts.Count == 0) return null;

            double minX = layouts.Min(l => l.X);
            double minY = layouts.Min(l => l.Y);
            double offsetX = 60 - minX;
            double offsetY = 60 - minY;

            double width = Math.Max(layouts.Max(l => l.Right) + offsetX + 60, 800);
            double height = Math.Max(layouts.Max(l => l.Bottom) + offsetY + 60, 600);

            var canvas = new Canvas
            {
                Width = width,
                Height = height,
                Background = Brushes.White
            };

            foreach (var layout in layouts.Where(l => l.Depth > 0))
            {
                var parentNode = doc.Root.FindParentOf(layout.NodeId);
                if (parentNode == null) continue;

                var parentLayout = layouts.FirstOrDefault(l => l.NodeId == parentNode.Id);
                if (parentLayout == null) continue;

                string lineColor = engine.Options.ConnectionStyle == "ClassicMindMap"
                    ? ClassicMindMapStyle.GetBranchColor(doc.Root, doc.Root.FindById(layout.NodeId))
                    : "#AAAAAA";
                var lineBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineColor));
                canvas.Children.Add(new System.Windows.Shapes.Path
                {
                    Stroke = lineBrush,
                    StrokeThickness = 2,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = LinkGeometryBuilder.Build(
                        parentLayout,
                        layout,
                        offsetX,
                        offsetY,
                        engine.Options.ConnectionStyle),
                    IsHitTestVisible = false
                });
                canvas.Children.Add(new System.Windows.Shapes.Path
                {
                    Fill = lineBrush,
                    Stroke = lineBrush,
                    StrokeThickness = 1,
                    Data = LinkEndpointBuilder.Build(
                        parentLayout,
                        layout,
                        offsetX,
                        offsetY,
                        engine.Options.ConnectionStyle,
                        engine.Options.EndpointStyle),
                    IsHitTestVisible = false
                });
            }

            foreach (var layout in layouts)
            {
                var node = doc.Root.FindById(layout.NodeId);
                if (node == null) continue;

                string branchColor = ClassicMindMapStyle.GetBranchColor(doc.Root, node);
                var ctrl = new NodeControl(node, layout, engine.Options, branchColor);
                Canvas.SetLeft(ctrl, layout.X + offsetX);
                Canvas.SetTop(ctrl, layout.Y + offsetY);
                canvas.Children.Add(ctrl);
            }

            canvas.Measure(new Size(width, height));
            canvas.Arrange(new Rect(0, 0, width, height));
            canvas.UpdateLayout();

            int pixelWidth = Math.Max(1, (int)Math.Ceiling(width * scale));
            int pixelHeight = Math.Max(1, (int)Math.Ceiling(height * scale));
            var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
            bitmap.Render(canvas);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

    }
}
