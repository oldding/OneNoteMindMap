namespace OneNoteMindMap.Core.Layout
{
    public class LayoutOptions
    {
        public string Layout { get; set; } = "RightTree";
        public string Theme { get; set; } = "Default";
        public string NodeShape { get; set; } = "Rounded";
        public double NodeWidth { get; set; } = 160;
        public double NodeHeight { get; set; } = 44;
        public double HorizontalGap { get; set; } = 90;
        public double VerticalGap { get; set; } = 24;
        public double LevelGap { get; set; } = 110;
        public double Padding { get; set; } = 40;
    }
}
