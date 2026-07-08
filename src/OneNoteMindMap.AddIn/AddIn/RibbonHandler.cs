using Microsoft.Office.Core;

namespace OneNoteMindMap.AddIn
{
    internal static class RibbonState
    {
        public static IRibbonUI RibbonUI { get; set; }

        public static void InvalidateRibbon()
        {
            RibbonUI?.Invalidate();
        }
    }
}
