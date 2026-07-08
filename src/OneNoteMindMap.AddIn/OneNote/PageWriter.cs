using System;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using OneNoteMindMap.Logging;

namespace OneNoteMindMap.OneNote
{
    public static class PageWriter
    {
        private static readonly XNamespace OneNs = "http://schemas.microsoft.com/office/onenote/2013/onenote";

        public static XElement CreateImageOutline(byte[] imageData, string altText = null, string markerMetaName = null)
        {
            double width = 800;
            double height = 600;
            using (var ms = new MemoryStream(imageData))
            using (var imageInfo = Image.FromStream(ms))
            {
                width = imageInfo.Width;
                height = imageInfo.Height;
            }

            var outline = new XElement(OneNs + "Outline");
            outline.Add(new XElement(OneNs + "Position",
                new XAttribute("x", 36.0),
                new XAttribute("y", 120.0),
                new XAttribute("z", 0)));

            var oe = new XElement(OneNs + "OE");
            oe.SetAttributeValue("alignment", "left");
            if (!string.IsNullOrEmpty(markerMetaName))
            {
                oe.Add(new XElement(OneNs + "Meta",
                    new XAttribute("name", markerMetaName),
                    new XAttribute("content", "true")));
            }

            var image = new XElement(OneNs + "Image",
                new XAttribute("format", "png"),
                new XElement(OneNs + "Size",
                    new XAttribute("width", width.ToString("0")),
                    new XAttribute("height", height.ToString("0"))),
                new XElement(OneNs + "Data", Convert.ToBase64String(imageData)));
            if (!string.IsNullOrEmpty(altText))
            {
                image.SetAttributeValue("alt", altText);
            }

            oe.Add(image);
            outline.Add(new XElement(OneNs + "OEChildren", oe));

            return outline;
        }

        public static XElement CreateDataBlockOutline(string dataString, string marker = null)
        {
            string fullData = marker != null ? marker + dataString : dataString;

            var outline = new XElement(OneNs + "Outline");
            var oe = new XElement(OneNs + "OE");
            var t = new XElement(OneNs + "T", new XCData(fullData));
            // Use a hidden/compact style
            oe.SetAttributeValue("quickStyleIndex", 0);

            oe.Add(t);
            outline.Add(new XElement(OneNs + "OEChildren", oe));

            return outline;
        }

        public static XElement CreateOutlineFromText(string text, int indentLevel = 0, bool isHeading = false)
        {
            var oe = new XElement(OneNs + "OE");
            var t = new XElement(OneNs + "T", new XCData(CreateTextWithLinq(text)));

            oe.Add(t);

            if (isHeading)
            {
                oe.SetAttributeValue("quickStyleIndex", 1);
            }

            var outlineEl = new XElement(OneNs + "Outline");
            outlineEl.Add(new XElement(OneNs + "OEChildren", oe));
            return outlineEl;
        }

        private static string CreateTextWithLinq(string text)
        {
            // Escape special characters for OneNote XML text content
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
