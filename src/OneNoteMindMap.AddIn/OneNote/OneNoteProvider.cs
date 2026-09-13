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
            // Borrow the connection's RCW, never reacquire from ROT after host shutdown.
            object raw = Connect.Instance?.OneNoteApp;

            if (raw == null)
                throw new InvalidOperationException("OneNote application not available.");

            return raw as IOneNoteApplication
                ?? throw new InvalidOperationException("Cast to IOneNoteApplication failed.");
        }

        public static string GetCurrentPageId()
        {
            try
            {
                return ReadCurrentWindow(window => window.CurrentPageId);
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
                return ReadCurrentWindow(window => window.CurrentSectionId);
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
                return ReadCurrentWindow(window => window.CurrentNotebookId);
            }
            catch (Exception ex)
            {
                Logger.Error("GetCurrentNotebookId failed", ex);
                return null;
            }
        }

        private static string ReadCurrentWindow(Func<Window, string> read)
        {
            Windows windows = null;
            Window window = null;
            try
            {
                windows = GetApp().GetWindows();
                window = windows?.CurrentWindow;
                return window == null ? null : read(window);
            }
            finally
            {
                if (window != null && Marshal.IsComObject(window)) Marshal.ReleaseComObject(window);
                if (windows != null && Marshal.IsComObject(windows)) Marshal.ReleaseComObject(windows);
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
                    sectionId = GetCurrentSectionId();
                }

                if (string.IsNullOrEmpty(sectionId)) return null;

                app = GetApp();
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
