using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Rendering;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class ExportPngCommand
    {
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

                string fileName = DialogHost.ShowSaveFileDialog(
                    Strings.FileDialogFilter,
                    Strings.If("导出为 PNG", "Export as PNG"),
                    SanitizeFileName(doc.Title) + ".png");

                if (string.IsNullOrEmpty(fileName)) return;

                var engine = new MindMapLayoutEngine();
                engine.Options.Layout = doc.Settings?.Layout ?? "RightTree";
                var layouts = engine.CalculateLayout(doc.Root);
                string svgContent = SvgRenderer.Render(doc, layouts);

                // Convert SVG to PNG using our simple renderer
                var pngBytes = SvgToPngConverter.ConvertSvgToPng(svgContent, 2.0);
                if (pngBytes == null || pngBytes.Length == 0)
                {
                    Msg.Warn(Strings.If("生成图片失败", "Failed to generate image"));
                    return;
                }

                File.WriteAllBytes(fileName, pngBytes);
                Logger.Info("PNG exported: " + fileName);
                Msg.Info(Strings.ExportSuccess + "\n" + fileName);
            }
            catch (Exception ex)
            {
                Logger.Error("ExportPngCommand failed", ex);
                Msg.Error(Strings.If("导出 PNG 失败：" + ex.Message, "Export PNG failed: " + ex.Message));
            }
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "mindmap";
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
                name = name.Replace(c, '_');
            return name.Length > 100 ? name.Substring(0, 100) : name;
        }
    }
}
