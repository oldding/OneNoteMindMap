using System;
using System.Collections.Generic;
using System.Xml.Linq;
using OneNoteMindMap.Logging;

namespace OneNoteMindMap.OneNote
{
    public static class PageParser
    {
        private static readonly XNamespace OneNs = "http://schemas.microsoft.com/office/onenote/2013/onenote";

        public class OutlineItem
        {
            public string Text { get; set; }
            public int IndentLevel { get; set; }
            public bool IsHeading { get; set; }
            public bool HasTag { get; set; }
            public string ObjectId { get; set; }
        }

        public static List<OutlineItem> ExtractOutlineItems(string pageXml)
        {
            var items = new List<OutlineItem>();
            if (string.IsNullOrEmpty(pageXml)) return items;

            try
            {
                var doc = XDocument.Parse(pageXml);
                var root = doc.Root;
                if (root == null) return items;

                foreach (var outline in root.Elements(OneNs + "Outline"))
                {
                    ExtractFromOutlineElement(outline, items, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ExtractOutlineItems failed", ex);
            }

            return items;
        }

        private static void ExtractFromOutlineElement(XElement outline, List<OutlineItem> items, int baseIndent)
        {
            foreach (var oe in outline.Elements(OneNs + "OE"))
            {
                var textEl = oe.Element(OneNs + "T");
                if (textEl == null) continue;

                var text = StripHtml(textEl.Value);
                if (string.IsNullOrWhiteSpace(text)) continue;

                var indent = oe.Element(OneNs + "List");
                int depth = baseIndent;

                var styleIdx = (string)oe.Attribute("quickStyleIndex");
                bool isHeading = styleIdx == "1" || styleIdx == "2";

                // Check for tag
                bool hasTag = oe.Element(OneNs + "Tag") != null;

                items.Add(new OutlineItem
                {
                    Text = text.Trim(),
                    IndentLevel = depth,
                    IsHeading = isHeading,
                    HasTag = hasTag,
                    ObjectId = (string)oe.Attribute("objectID")
                });

                // Process children
                var children = oe.Element(OneNs + "OEChildren");
                if (children != null)
                {
                    ExtractFromOutlineElement(children, items, depth + 1);
                }
            }
        }

        public static string GetPageTitle(string pageXml)
        {
            try
            {
                var doc = XDocument.Parse(pageXml);
                var root = doc.Root;
                if (root == null) return null;

                var titleEl = root.Element(OneNs + "Title");
                if (titleEl == null) return null;
                var oe = titleEl.Element(OneNs + "OE");
                if (oe == null) return null;
                var t = oe.Element(OneNs + "T");
                return t?.Value?.Trim();
            }
            catch
            {
                return null;
            }
        }

        public static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
        }
    }
}
