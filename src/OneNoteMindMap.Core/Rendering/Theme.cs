namespace OneNoteMindMap.Core.Rendering
{
    public class Theme
    {
        public string Name { get; set; } = "Default";
        public string Background { get; set; } = "#FFFFFF";
        public string RootFill { get; set; } = "#7719AA";
        public string RootText { get; set; } = "#FFFFFF";
        public string Level1Fill { get; set; } = "#FFFFFF";
        public string Level2Fill { get; set; } = "#FFFFFF";
        public string NodeFill { get; set; } = "#FFFFFF";
        public string NodeText { get; set; } = "#333333";
        public string Border { get; set; } = "#CCCCCC";
        public string Line { get; set; } = "#AAAAAA";
        public string FontFamily { get; set; } = "Microsoft YaHei, Arial, sans-serif";
    }

    public static class ThemeManager
    {
        private static readonly Theme DefaultTheme = new Theme();
        private static readonly Theme PurpleTheme = new Theme
        {
            Name = "Purple",
            RootFill = "#7719AA",
            RootText = "#FFFFFF",
            Level1Fill = "#E8D5F5",
            Level2Fill = "#F3E8FB",
            NodeFill = "#FFFFFF",
            Line = "#7719AA"
        };
        private static readonly Theme MinimalTheme = new Theme
        {
            Name = "Minimal",
            Background = "#FFFFFF",
            RootFill = "#333333",
            RootText = "#FFFFFF",
            Level1Fill = "#F5F5F5",
            Level2Fill = "#FAFAFA",
            NodeFill = "#FFFFFF",
            Border = "#CCCCCC",
            Line = "#999999"
        };
        private static readonly Theme FreshTheme = new Theme
        {
            Name = "Fresh",
            RootFill = "#0F766E",
            RootText = "#FFFFFF",
            Level1Fill = "#CCFBF1",
            Level2Fill = "#ECFDF5",
            NodeFill = "#FFFFFF",
            Border = "#99F6E4",
            Line = "#14B8A6"
        };
        private static readonly Theme WarmTheme = new Theme
        {
            Name = "Warm",
            RootFill = "#C2410C",
            RootText = "#FFFFFF",
            Level1Fill = "#FFEDD5",
            Level2Fill = "#FFF7ED",
            NodeFill = "#FFFFFF",
            Border = "#FED7AA",
            Line = "#FB923C"
        };

        public static Theme GetTheme(string name)
        {
            return name switch
            {
                "Purple" => PurpleTheme,
                "Minimal" => MinimalTheme,
                "Fresh" => FreshTheme,
                "Warm" => WarmTheme,
                _ => DefaultTheme
            };
        }
    }
}
// Theme and ThemeManager are defined in SvgRenderer.cs
// This file is intentionally left empty to avoid duplicate definitions.
