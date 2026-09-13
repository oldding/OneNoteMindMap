using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Core.Layout
{
    public class LayoutOptions
    {
        public string Layout { get; set; } = "RightTree";
        public string Theme { get; set; } = "Default";
        public string NodeShape { get; set; } = "Rounded";
        public string ConnectionStyle { get; set; } = "Curved";
        public string EndpointStyle { get; set; } = "None";
        public string NodeSizeMode { get; set; } = "Fixed";
        public double NodeWidth { get; set; } = 160;
        public double NodeHeight { get; set; } = 44;
        public double AutoNodeMinWidth { get; set; } = 120;
        public double AutoNodeMaxWidth { get; set; } = 320;
        public double HorizontalGap { get; set; } = 90;
        public double VerticalGap { get; set; } = 24;
        public double LevelGap { get; set; } = 110;
        public double Padding { get; set; } = 40;

        public void ApplySettings(MindMapSettings settings)
        {
            if (settings == null) return;
            Layout = settings.Layout ?? "RightTree";
            Theme = settings.Theme ?? "Default";
            NodeShape = settings.NodeShape ?? "Rounded";
            ConnectionStyle = settings.ConnectionStyle ?? "Curved";
            EndpointStyle = settings.EndpointStyle ?? "None";
            NodeSizeMode = settings.NodeSizeMode ?? "Fixed";
            NodeWidth = settings.NodeWidth > 0 ? settings.NodeWidth : 160;
            NodeHeight = settings.NodeHeight > 0 ? settings.NodeHeight : 44;
            AutoNodeMaxWidth = settings.AutoNodeMaxWidth > 0 ? settings.AutoNodeMaxWidth : 320;
            HorizontalGap = settings.HorizontalGap > 0 ? settings.HorizontalGap : 90;
            VerticalGap = settings.VerticalGap > 0 ? settings.VerticalGap : 24;
            LevelGap = settings.LevelGap > 0 ? settings.LevelGap : 110;
        }
    }
}
