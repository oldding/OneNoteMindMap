using System;
using System.Linq;
using System.Xml.Linq;
using OneNoteMindMap.Core.Model;
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
}
