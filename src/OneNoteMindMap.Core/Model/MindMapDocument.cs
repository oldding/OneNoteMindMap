using System;

namespace OneNoteMindMap.Core.Model
{
    public class MindMapDocument
    {
        public string Format { get; set; } = "OneNoteMindMap";
        public int Version { get; set; } = 1;
        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public string Title { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public MindMapSourceInfo Source { get; set; } = new MindMapSourceInfo();
        public MindMapSettings Settings { get; set; } = new MindMapSettings();
        public MindMapNode Root { get; set; } = new MindMapNode { Text = "中心主题" };

        public MindMapDocument Clone()
        {
            return new MindMapDocument
            {
                Format = Format,
                Version = Version,
                Id = Id,
                Title = Title,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Source = Source.Clone(),
                Settings = Settings.Clone(),
                Root = Root.Clone()
            };
        }
    }

    public class MindMapSourceInfo
    {
        public string OneNotePageId { get; set; } = "";
        public string OneNotePageTitle { get; set; } = "";
        public string SourceType { get; set; } = "Manual";

        public MindMapSourceInfo Clone()
        {
            return new MindMapSourceInfo
            {
                OneNotePageId = OneNotePageId,
                OneNotePageTitle = OneNotePageTitle,
                SourceType = SourceType
            };
        }
    }
}
