using System;
using System.Linq;
using System.Xml.Linq;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class MindMapToOutlineCommand
    {
        private const string OutlineMetaName = "onenote-mindmap-outline-v1";

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

                string pageXml = OneNoteProvider.GetPageContent(pageId, out _);
                if (string.IsNullOrEmpty(pageXml))
                {
                    Msg.Warn(Strings.If("无法读取页面内容", "Cannot read page content"));
                    return;
                }

                var pageDoc = XDocument.Parse(pageXml);
                var ns = pageDoc.Root.Name.Namespace;

                // Replace all visible outlines on this mind-map page. The editable
                // mind-map payload is stored in the page title's Meta element.
                foreach (var outline in pageDoc.Root.Elements(ns + "Outline").ToList())
                    outline.Remove();

                var children = new XElement(ns + "OEChildren");
                string rootText = (doc.Root?.Text ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(rootText))
                {
                    children.Add(BuildOe(doc.Root, ns, 0));
                }
                else if (doc.Root != null)
                {
                    foreach (var child in doc.Root.Children)
                        children.Add(BuildOe(child, ns, 0));
                }

                if (!children.HasElements)
                {
                    Msg.Warn(Strings.If("脑图中没有可转换的大纲内容", "No outline content to convert"));
                    return;
                }

                var outlineEl = new XElement(ns + "Outline",
                    new XElement(ns + "Position",
                        new XAttribute("x", 36.0),
                        new XAttribute("y", 120.0),
                        new XAttribute("z", 0)),
                    new XElement(ns + "Meta",
                        new XAttribute("name", OutlineMetaName),
                        new XAttribute("content", "true")),
                    children);

                pageDoc.Root.Add(outlineEl);

                OneNoteProvider.UpdatePageContent(pageId, pageDoc.ToString());
                OneNoteProvider.NavigateTo(pageId);

                Logger.Info("Mind map converted to outline: " + pageId);
                Msg.Info(Strings.If("脑图已转换为大纲", "Mind map converted to outline"));
            }
            catch (Exception ex)
            {
                Logger.Error("MindMapToOutlineCommand failed", ex);
                Msg.Error(Strings.If("转换失败：" + ex.Message, "Conversion failed: " + ex.Message));
            }
        }

        private static XElement BuildOe(MindMapNode node, XNamespace ns, int depth)
        {
            string text = (node?.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text))
                text = Strings.If("未命名节点", "Untitled Node");

            var oe = new XElement(ns + "OE",
                new XElement(ns + "T", new XCData(ToOneNoteText(text))));

            if (depth == 0)
                oe.SetAttributeValue("quickStyleIndex", "1");

            if (node != null && !node.Collapsed && node.Children.Count > 0)
            {
                var children = new XElement(ns + "OEChildren");
                foreach (var child in node.Children)
                    children.Add(BuildOe(child, ns, depth + 1));
                oe.Add(children);
            }

            return oe;
        }

        private static string ToOneNoteText(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\r\n", "<br/>")
                .Replace("\n", "<br/>")
                .Replace("\r", "<br/>");
        }
    }
}
