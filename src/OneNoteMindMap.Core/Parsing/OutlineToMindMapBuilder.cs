using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Core.Parsing
{
    public static class OutlineToMindMapBuilder
    {
        private static readonly XNamespace OneNs = "http://schemas.microsoft.com/office/onenote/2013/onenote";

        public static MindMapDocument BuildFromPageXml(string pageXml, string pageId, string pageTitle)
        {
            var doc = XDocument.Parse(pageXml);
            var root = doc.Root;
            if (root == null) return CreateDefault(pageTitle);

            var titleText = ExtractTitle(root);
            var parsedNodes = ParseOutlines(root);
            MindMapNode rootNode;

            string centerTitle = !string.IsNullOrWhiteSpace(titleText)
                ? titleText
                : (pageTitle ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(centerTitle))
            {
                // Page has a title: title is the central topic, every top-level
                // paragraph becomes a level-1 branch.
                rootNode = new MindMapNode { Text = centerTitle, SourceObjectId = pageId };
                foreach (var node in parsedNodes)
                    rootNode.Children.Add(node);
            }
            else if (parsedNodes.Count > 0)
            {
                // Untitled page: first top-level paragraph is the central topic,
                // remaining top-level paragraphs become level-1 branches.
                rootNode = parsedNodes[0];
                rootNode.SourceObjectId = pageId;
                for (int i = 1; i < parsedNodes.Count; i++)
                    rootNode.Children.Add(parsedNodes[i]);
            }
            else
            {
                rootNode = new MindMapNode
                {
                    Text = string.IsNullOrWhiteSpace(pageTitle) ? titleText : pageTitle,
                    SourceObjectId = pageId
                };
            }

            if (rootNode.Children.Count == 0)
            {
                rootNode.Text = pageTitle ?? "未命名页面";
                return CreateDefault(rootNode.Text);
            }

            var mmDoc = new MindMapDocument
            {
                Title = rootNode.Text,
                Root = rootNode,
                Source = new MindMapSourceInfo
                {
                    OneNotePageId = pageId,
                    OneNotePageTitle = rootNode.Text,
                    SourceType = "CurrentPage"
                }
            };

            return mmDoc;
        }

        private static List<MindMapNode> ParseOutlines(XElement pageEl)
        {
            var nodes = new List<MindMapNode>();

            foreach (var outline in SelectContentOutlines(pageEl))
            {
                var children = outline.Element(OneNs + "OEChildren");
                var topOes = children != null ? children.Elements(OneNs + "OE") : outline.Elements(OneNs + "OE");
                foreach (var oe in topOes)
                    nodes.AddRange(ParseOe(oe));
            }

            return nodes;
        }

        /// <summary>
        /// Picks which outlines represent real page content.
        /// Preference order:
        /// 1. Outlines written by this add-in's "convert to outline" command
        ///    (marked with a Meta element) - they are the authoritative content
        ///    and ignoring the rest avoids duplicates accumulated by repeated
        ///    generate/convert cycles on the same page.
        /// 2. Otherwise every outline that is not an add-in artifact
        ///    (preview images, legacy data blocks).
        /// </summary>
        private static List<XElement> SelectContentOutlines(XElement pageEl)
        {
            var all = pageEl.Elements(OneNs + "Outline").ToList();

            var marked = all.Where(o => o.Elements(OneNs + "Meta")
                    .Any(m => string.Equals((string)m.Attribute("name"), "onenote-mindmap-outline-v1", StringComparison.Ordinal)))
                .ToList();
            if (marked.Count > 0)
                return marked;

            return all.Where(o => !IsAddInArtifact(o)).ToList();
        }

        private static bool IsAddInArtifact(XElement outline)
        {
            // Preview image outlines
            if (outline.Descendants(OneNs + "Meta")
                .Any(m => string.Equals((string)m.Attribute("name"), "onenote-mindmap-preview-v1", StringComparison.Ordinal)))
                return true;

            // Legacy visible data blocks
            if (outline.Descendants(OneNs + "T")
                .Any(t => t.Value != null && t.Value.Trim().StartsWith("ONENOTE_MINDMAP_DATA_V1:", StringComparison.Ordinal)))
                return true;

            // Image-only outlines (old previews without a marker)
            bool hasText = outline.Descendants(OneNs + "T")
                .Any(t => !string.IsNullOrWhiteSpace(StripHtml(t.Value)));
            bool hasImage = outline.Descendants(OneNs + "Image").Any();
            if (hasImage && !hasText)
                return true;

            return false;
        }

        private static List<MindMapNode> ParseOe(XElement oe)
        {
            var nodes = new List<MindMapNode>();
            var lines = ExtractTextLines(oe).ToList();

            MindMapNode current = null;
            foreach (var line in lines)
            {
                if (line.StartsWith("ONENOTE_MINDMAP_DATA_V1:", StringComparison.Ordinal)) continue;
                var node = new MindMapNode { Text = line };
                if (current == null)
                {
                    current = node;
                    nodes.Add(node);
                }
                else
                {
                    current.Children.Add(node);
                }
            }

            var children = oe.Element(OneNs + "OEChildren");
            if (children != null)
            {
                var childNodes = children.Elements(OneNs + "OE").SelectMany(ParseOe).ToList();
                if (current != null)
                    current.Children.AddRange(childNodes);
                else
                    nodes.AddRange(childNodes);
            }

            return nodes;
        }

        private static IEnumerable<string> ExtractTextLines(XElement oe)
        {
            var textEl = oe.Element(OneNs + "T");
            if (textEl == null) yield break;

            foreach (var line in StripHtml(textEl.Value)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                yield return line;
            }
        }

        private static string ExtractTitle(XElement pageEl)
        {
            var titleEl = pageEl.Element(OneNs + "Title");
            if (titleEl == null) return "";
            var oe = titleEl.Element(OneNs + "OE");
            if (oe == null) return "";
            var t = oe.Element(OneNs + "T");
            if (t == null) return "";
            return StripHtml(t.Value).Trim();
        }

        public static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            string withBreaks = System.Text.RegularExpressions.Regex.Replace(html, @"<(br|/p|/div)\s*/?>", "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            string plain = System.Text.RegularExpressions.Regex.Replace(withBreaks, "<[^>]+>", "");
            return WebUtility.HtmlDecode(plain);
        }

        private static MindMapDocument CreateDefault(string title)
        {
            var root = new MindMapNode { Text = string.IsNullOrWhiteSpace(title) ? "新脑图" : title };
            root.Children.Add(new MindMapNode { Text = "双击编辑节点" });
            return new MindMapDocument { Title = root.Text, Root = root };
        }
    }
}
