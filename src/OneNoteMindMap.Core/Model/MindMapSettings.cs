namespace OneNoteMindMap.Core.Model
{
    public class MindMapSettings
    {
        public string Layout { get; set; } = "RightTree";
        public string Theme { get; set; } = "Default";
        public string NodeShape { get; set; } = "Rounded";
        public string Direction { get; set; } = "Right";
        public double CanvasZoom { get; set; } = 1.0;
        public double NodeWidth { get; set; } = 160;
        public double NodeHeight { get; set; } = 44;
        public double HorizontalGap { get; set; } = 90;
        public double VerticalGap { get; set; } = 24;
        public double LevelGap { get; set; } = 110;

        public MindMapSettings Clone()
        {
            return new MindMapSettings
            {
                Layout = Layout,
                Theme = Theme,
                NodeShape = NodeShape,
                Direction = Direction,
                CanvasZoom = CanvasZoom,
                NodeWidth = NodeWidth,
                NodeHeight = NodeHeight,
                HorizontalGap = HorizontalGap,
                VerticalGap = VerticalGap,
                LevelGap = LevelGap
            };
        }
    }
}
