using System;

namespace OneNoteMindMap.Core.Parsing
{
    internal static class TextDisplayWidthHelper
    {
        public static int DisplayWidth(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int width = 0;
            foreach (char c in text)
                width += IsWide(c) ? 2 : 1;
            return width;
        }

        public static string PadRight(string text, int displayWidth)
        {
            text = text ?? "";
            int missing = Math.Max(0, displayWidth - DisplayWidth(text));
            return text + new string(' ', missing);
        }

        private static bool IsWide(char c)
        {
            return c >= 0x2E80 ||
                   (c >= 0x1100 && c <= 0x11FF) ||
                   (c >= 0x3000 && c <= 0x30FF) ||
                   (c >= 0xAC00 && c <= 0xD7AF) ||
                   (c >= 0xFF00 && c <= 0xFFEF);
        }
    }
}
