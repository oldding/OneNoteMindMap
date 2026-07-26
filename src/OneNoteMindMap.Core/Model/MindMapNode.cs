using System;
using System.Collections.Generic;

namespace OneNoteMindMap.Core.Model
{
    public class MindMapNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Text { get; set; } = "";
        public string Note { get; set; } = "";
        public string ContentType { get; set; } = "Text";
        public List<MindMapNode> Children { get; set; } = new List<MindMapNode>();
        public bool Collapsed { get; set; }
        public string Color { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Link { get; set; } = "";
        public string SourceObjectId { get; set; } = "";
        public double ManualOffsetX { get; set; }
        public double ManualOffsetY { get; set; }

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
                ContentType = ContentType,
                Collapsed = Collapsed,
                Color = Color,
                Icon = Icon,
                Link = Link,
                SourceObjectId = SourceObjectId,
                ManualOffsetX = ManualOffsetX,
                ManualOffsetY = ManualOffsetY,
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

        /// <summary>Finds a node by id in the subtree rooted at this node.</summary>
        public MindMapNode FindById(string id)
        {
            if (Id == id) return this;
            foreach (var child in Children)
            {
                var found = child.FindById(id);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>Finds the parent of the node with the given id.</summary>
        public MindMapNode FindParentOf(string childId)
        {
            foreach (var child in Children)
            {
                if (child.Id == childId) return this;
                var found = child.FindParentOf(childId);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Moves a node and its entire subtree under a different parent.
        /// Returns false when the move would target the root, keep the same parent,
        /// or create a cycle by moving a node under one of its descendants.
        /// </summary>
        public bool ReparentNode(string nodeId, string newParentId)
        {
            if (string.IsNullOrEmpty(nodeId) ||
                string.IsNullOrEmpty(newParentId) ||
                nodeId == Id)
                return false;

            var node = FindById(nodeId);
            var newParent = FindById(newParentId);
            if (node == null || newParent == null || node == newParent)
                return false;

            if (node.FindById(newParentId) != null)
                return false;

            var oldParent = FindParentOf(nodeId);
            if (oldParent == null || oldParent == newParent)
                return false;

            if (!oldParent.Children.Remove(node))
                return false;

            node.ManualOffsetX = 0;
            node.ManualOffsetY = 0;
            newParent.Collapsed = false;
            newParent.Children.Add(node);
            return true;
        }
    }
}
