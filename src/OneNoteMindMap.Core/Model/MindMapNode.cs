using System;
using System.Collections.Generic;

namespace OneNoteMindMap.Core.Model
{
    public class MindMapNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Text { get; set; } = "";
        public string Note { get; set; } = "";
        public List<MindMapNode> Children { get; set; } = new List<MindMapNode>();
        public bool Collapsed { get; set; }
        public string Color { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Link { get; set; } = "";
        public string SourceObjectId { get; set; } = "";

        public bool IsLeaf => Children.Count == 0;
        public bool HasChildren => Children.Count > 0;
        public int Depth { get; set; }

        public MindMapNode Clone()
        {
            var node = new MindMapNode
            {
                Id = Id,
                Text = Text,
                Note = Note,
                Collapsed = Collapsed,
                Color = Color,
                Icon = Icon,
                Link = Link,
                SourceObjectId = SourceObjectId,
                Depth = Depth
            };
            foreach (var child in Children)
                node.Children.Add(child.Clone());
            return node;
        }

        public int CountVisibleNodes()
        {
            int count = 1;
            if (!Collapsed)
                foreach (var child in Children)
                    count += child.CountVisibleNodes();
            return count;
        }

        public int CountAllNodes()
        {
            int count = 1;
            foreach (var child in Children)
                count += child.CountAllNodes();
            return count;
        }
    }
}
