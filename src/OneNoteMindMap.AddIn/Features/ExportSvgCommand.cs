using System;
using System.IO;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Rendering;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class ExportSvgCommand
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
                    Strings.SvgFileDialogFilter,
                    Strings.If("导出为 SVG", "Export as SVG"),
                    SanitizeFileName(doc.Title) + ".svg");

                if (string.IsNullOrEmpty(fileName)) return;

                var engine = new MindMapLayoutEngine();
                engine.Options.Layout = doc.Settings?.Layout ?? "RightTree";
                var layouts = engine.CalculateLayout(doc.Root);
                string svgContent = SvgRenderer.Render(doc, layouts);

                File.WriteAllText(fileName, svgContent);
                Logger.Info("SVG exported: " + fileName);
                Msg.Info(Strings.ExportSuccess + "\n" + fileName);
            }
            catch (Exception ex)
            {
                Logger.Error("ExportSvgCommand failed", ex);
                Msg.Error(Strings.If("导出 SVG 失败：" + ex.Message, "Export SVG failed: " + ex.Message));
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
