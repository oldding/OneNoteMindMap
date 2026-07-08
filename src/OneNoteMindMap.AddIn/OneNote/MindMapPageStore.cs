using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Core.Serialization;
using OneNoteMindMap.Logging;

namespace OneNoteMindMap.OneNote
{
    public static class MindMapPageStore
    {
        public const string DataMarker = "ONENOTE_MINDMAP_DATA_V1:";
        private const string DataMetaName = "onenote-mindmap-data-v1";

        public static bool SaveMindMapToPage(string pageId, MindMapDocument doc)
        {
            try
            {
                var json = MindMapSerializer.Serialize(doc);
                var encoded = MindMapPayloadCodec.Encode(json);

                // Get current page XML
                string pageXml = OneNoteProvider.GetPageContent(pageId, out string pageTitle);
                if (string.IsNullOrEmpty(pageXml)) return false;

                var pageDoc = System.Xml.Linq.XDocument.Parse(pageXml);
                var ns = pageDoc.Root.Name.Namespace;

                RemoveLegacyDataBlocks(pageDoc, ns);

                var meta = FindDataMeta(pageDoc, ns);
                if (meta != null)
                {
                    meta.SetAttributeValue("content", encoded);
                }
                else
                {
                    var targetOe = FindMetaTargetOe(pageDoc, ns);
                    if (targetOe == null) return false;

                    targetOe.AddFirst(new System.Xml.Linq.XElement(ns + "Meta",
                        new System.Xml.Linq.XAttribute("name", DataMetaName),
                        new System.Xml.Linq.XAttribute("content", encoded)));
                }

                OneNoteProvider.UpdatePageContent(pageId, pageDoc.ToString());
                Logger.Info("Mind map data saved to page: " + pageId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("SaveMindMapToPage failed", ex);
                return false;
            }
        }

        public static MindMapDocument LoadMindMapFromPage(string pageId)
        {
            try
            {
                string pageXml = OneNoteProvider.GetPageContent(pageId, out string pageTitle);
                if (string.IsNullOrEmpty(pageXml)) return null;

                var pageDoc = System.Xml.Linq.XDocument.Parse(pageXml);
                var ns = pageDoc.Root.Name.Namespace;

                var meta = FindDataMeta(pageDoc, ns);
                string encoded = (string)meta?.Attribute("content");

                if (string.IsNullOrEmpty(encoded))
                    encoded = FindLegacyEncodedData(pageDoc, ns);
                if (string.IsNullOrEmpty(encoded)) return null;

                string json = MindMapPayloadCodec.Decode(encoded);
                if (json == null) return null;

                var doc = MindMapSerializer.Deserialize(json);
                if (doc != null && doc.Source == null)
                {
                    doc.Source = new MindMapSourceInfo
                    {
                        OneNotePageId = pageId,
                        OneNotePageTitle = pageTitle,
                        SourceType = "CurrentPage"
                    };
                }

                Logger.Info("Mind map data loaded from page: " + pageId);
                return doc;
            }
            catch (Exception ex)
            {
                Logger.Error("LoadMindMapFromPage failed", ex);
                return null;
            }
        }

        public static bool HasMindMapData(string pageId)
        {
            try
            {
                string pageXml = OneNoteProvider.GetPageContent(pageId, out _);
                if (string.IsNullOrEmpty(pageXml)) return false;

                var pageDoc = System.Xml.Linq.XDocument.Parse(pageXml);
                var ns = pageDoc.Root.Name.Namespace;

                return FindDataMeta(pageDoc, ns) != null || FindDataBlock(pageDoc, ns) != null;
            }
            catch
            {
                return false;
            }
        }

        private static System.Xml.Linq.XElement FindDataBlock(System.Xml.Linq.XDocument pageDoc, System.Xml.Linq.XNamespace ns)
        {
            foreach (var outline in pageDoc.Root.Elements(ns + "Outline"))
            {
                foreach (var oe in outline.Descendants(ns + "OE"))
                {
                    var t = oe.Element(ns + "T");
                    if (t != null && t.Value != null && t.Value.Trim().StartsWith(DataMarker))
                        return outline;
                }
            }
            return null;
        }

        private static System.Xml.Linq.XElement FindDataMeta(System.Xml.Linq.XDocument pageDoc, System.Xml.Linq.XNamespace ns)
        {
            return pageDoc.Descendants(ns + "Meta")
                .FirstOrDefault(m => string.Equals((string)m.Attribute("name"), DataMetaName, StringComparison.Ordinal));
        }

        private static System.Xml.Linq.XElement FindMetaTargetOe(System.Xml.Linq.XDocument pageDoc, System.Xml.Linq.XNamespace ns)
        {
            var titleOe = pageDoc.Root?.Element(ns + "Title")?.Element(ns + "OE");
            if (titleOe != null)
                return titleOe;

            return pageDoc.Root?.Descendants(ns + "OE").FirstOrDefault();
        }

        private static void RemoveLegacyDataBlocks(System.Xml.Linq.XDocument pageDoc, System.Xml.Linq.XNamespace ns)
        {
            foreach (var outline in pageDoc.Root.Elements(ns + "Outline").ToList())
            {
                var hasLegacyData = outline.Descendants(ns + "T")
                    .Any(t => t.Value != null && t.Value.Trim().StartsWith(DataMarker));
                if (hasLegacyData)
                    outline.Remove();
            }
        }

        private static string FindLegacyEncodedData(System.Xml.Linq.XDocument pageDoc, System.Xml.Linq.XNamespace ns)
        {
            var dataBlock = FindDataBlock(pageDoc, ns);
            var t = dataBlock?.Descendants(ns + "T").FirstOrDefault();
            string fullText = t?.Value?.Trim();
            if (string.IsNullOrEmpty(fullText) || !fullText.StartsWith(DataMarker)) return null;
            return fullText.Substring(DataMarker.Length);
        }

        public static string FindAndExtractData(string pageXml)
        {
            if (string.IsNullOrEmpty(pageXml)) return null;

            try
            {
                var pageDoc = System.Xml.Linq.XDocument.Parse(pageXml);
                var ns = pageDoc.Root.Name.Namespace;

                var encoded = (string)FindDataMeta(pageDoc, ns)?.Attribute("content")
                    ?? FindLegacyEncodedData(pageDoc, ns);
                if (!string.IsNullOrEmpty(encoded))
                    return MindMapPayloadCodec.Decode(encoded);
            }
            catch { }

            return null;
        }
    }
}
