using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Model;

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
                var parentId = FindParentId(doc.Root, layout.NodeId);
                if (parentId == null) continue;

                var parentLayout = layouts.FirstOrDefault(l => l.NodeId == parentId);
                if (parentLayout == null) continue;

                canvas.Children.Add(new System.Windows.Shapes.Path
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                    StrokeThickness = 2,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = LinkGeometryBuilder.Build(parentLayout, layout, offsetX, offsetY),
                    IsHitTestVisible = false
                });
            }

            foreach (var layout in layouts)
            {
                var node = FindNode(doc.Root, layout.NodeId);
                if (node == null) continue;

                var ctrl = new NodeControl(node, layout, engine.Options);
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
    }
}
