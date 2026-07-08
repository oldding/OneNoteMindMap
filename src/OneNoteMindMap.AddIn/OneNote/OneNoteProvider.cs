using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.Office.Interop.OneNote;
using OneNoteMindMap.AddIn;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote.Interop;

namespace OneNoteMindMap.OneNote
{
    public static class OneNoteProvider
    {
        private static readonly XNamespace OneNs = "http://schemas.microsoft.com/office/onenote/2013/onenote";

        private static IOneNoteApplication GetApp()
        {
            object raw;
            try { raw = Marshal.GetActiveObject("OneNote.Application"); }
            catch { raw = Connect.Instance?.OneNoteApp; }

            if (raw == null)
                throw new InvalidOperationException("OneNote application not available.");

            return raw as IOneNoteApplication
                ?? throw new InvalidOperationException("Cast to IOneNoteApplication failed.");
        }

        public static string GetCurrentPageId()
        {
            try
            {
                var app = GetApp();
                var windows = app.GetWindows();
                if (windows?.CurrentWindow == null) return null;
                return windows.CurrentWindow.CurrentPageId;
            }
            catch (Exception ex)
            {
                Logger.Error("GetCurrentPageId failed", ex);
                return null;
            }
        }

        public static string GetCurrentSectionId()
        {
            try
            {
                var app = GetApp();
                var windows = app.GetWindows();
                if (windows?.CurrentWindow == null) return null;
                return windows.CurrentWindow.CurrentSectionId;
            }
            catch (Exception ex)
            {
                Logger.Error("GetCurrentSectionId failed", ex);
                return null;
            }
        }

        public static string GetCurrentNotebookId()
        {
            try
            {
                var app = GetApp();
                var windows = app.GetWindows();
                if (windows?.CurrentWindow == null) return null;
                return windows.CurrentWindow.CurrentNotebookId;
            }
            catch (Exception ex)
            {
                Logger.Error("GetCurrentNotebookId failed", ex);
                return null;
            }
        }

        public static string GetPageContent(string pageId, out string pageTitle)
        {
            pageTitle = null;
            try
            {
                var app = GetApp();
                app.GetPageContent(pageId, out string xml, PageInfo.piAll, XMLSchema.xs2013);

                if (!string.IsNullOrEmpty(xml))
                {
                    try
                    {
                        var xDoc = XDocument.Parse(xml);
                        var titleEl = xDoc.Root?.Element(OneNs + "Title")?.Element(OneNs + "OE")?.Element(OneNs + "T");
                        pageTitle = titleEl?.Value?.Trim();
                    }
                    catch { }
                }

                return xml;
            }
            catch (Exception ex)
            {
                Logger.Error("GetPageContent failed", ex);
                return null;
            }
        }

        public static void UpdatePageContent(string pageId, string pageXml)
        {
            try
            {
                var app = GetApp();

                // DateTime.MinValue skips the last-modified check (avoids 0x80042010
                // caused by timezone/precision mismatch when re-passing lastModifiedTime).
                app.UpdatePageContent(pageXml, DateTime.MinValue, XMLSchema.xs2013, false);
                Logger.Info("Page content updated: " + pageId);
            }
            catch (Exception ex)
            {
                Logger.Error("UpdatePageContent failed", ex);
                throw;
            }
        }

        public static string CreateNewPage(string sectionId = null)
        {
            try
            {
                var app = GetApp();

                if (string.IsNullOrEmpty(sectionId))
                {
                    var windows = app.GetWindows();
                    sectionId = windows?.CurrentWindow?.CurrentSectionId;
                }

                if (string.IsNullOrEmpty(sectionId)) return null;

                app.CreateNewPage(sectionId, out string pageId, NewPageStyle.npsBlankPageWithTitle);
                return pageId;
            }
            catch (Exception ex)
            {
                Logger.Error("CreateNewPage failed", ex);
                return null;
            }
        }

        public static void NavigateTo(string objectId)
        {
            try
            {
                var app = GetApp();
                app.NavigateTo(objectId, null, false);
            }
            catch (Exception ex)
            {
                Logger.Error("NavigateTo failed", ex);
            }
        }

        public static void SetPageTitle(string pageId, string title)
        {
            try
            {
                string xml = GetPageContent(pageId, out _);
                if (string.IsNullOrEmpty(xml)) return;

                var doc = XDocument.Parse(xml);
                var titleEl = doc.Root?.Element(OneNs + "Title");
                if (titleEl == null)
                {
                    titleEl = new XElement(OneNs + "Title",
                        new XElement(OneNs + "OE",
                            new XElement(OneNs + "T", new XCData(title))));
                    doc.Root?.AddFirst(titleEl);
                }
                else
                {
                    var t = titleEl.Element(OneNs + "OE")?.Element(OneNs + "T");
                    if (t == null) return;
                    t.ReplaceNodes(new XCData(title));
                }

                UpdatePageContent(pageId, doc.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error("SetPageTitle failed", ex);
            }
        }

        public static void Publish(string pageId, string filePath, PublishFormat format)
        {
            try
            {
                var app = GetApp();
                app.Publish(pageId, filePath, format, "");
                Logger.Info("Published page to: " + filePath);
            }
            catch (Exception ex)
            {
                Logger.Error("Publish failed", ex);
                throw;
            }
        }
    }
}
