namespace OneNoteMindMap.Core.Layout
{
    public class NodeLayout
    {
        public string NodeId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int Depth { get; set; }

        public double CenterX => X + Width / 2;
        public double CenterY => Y + Height / 2;
        public double Right => X + Width;
        public double Bottom => Y + Height;
        public double AnchorRightX => X + Width;
        public double AnchorRightY => Y + Height / 2;
        public double AnchorLeftX => X;
        public double AnchorLeftY => Y + Height / 2;
    }
}
