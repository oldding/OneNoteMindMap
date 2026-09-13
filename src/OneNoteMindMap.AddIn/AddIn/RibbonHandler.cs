using Microsoft.Office.Core;
using System.Runtime.InteropServices;

namespace OneNoteMindMap.AddIn
{
    internal static class RibbonState
    {
        public static IRibbonUI RibbonUI { get; set; }

        public static void InvalidateRibbon()
        {
            RibbonUI?.Invalidate();
        }

        public static void Release()
        {
            var ribbon = RibbonUI;
            RibbonUI = null;
            if (ribbon != null && Marshal.IsComObject(ribbon)) Marshal.ReleaseComObject(ribbon);
        }
    }
}
