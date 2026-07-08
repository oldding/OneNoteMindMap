using System;
using System.Windows.Forms;

namespace OneNoteMindMap.UI
{
    public static class DialogHost
    {
        public static IWin32Window OneNoteWindow { get; set; }

        public static string ShowOpenFileDialog(string filter, string title = "")
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = filter;
                dlg.Title = title;
                if (OneNoteWindow != null)
                    dlg.ShowDialog(OneNoteWindow);
                else
                    dlg.ShowDialog();
                return dlg.FileName;
            }
        }

        public static string ShowSaveFileDialog(string filter, string title = "", string defaultName = "")
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.Title = title;
                dlg.FileName = defaultName;
                if (OneNoteWindow != null)
                    dlg.ShowDialog(OneNoteWindow);
                else
                    dlg.ShowDialog();
                return dlg.FileName;
            }
        }
    }
}
