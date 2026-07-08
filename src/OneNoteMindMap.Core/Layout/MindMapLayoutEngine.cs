using System;
using System.Collections.Generic;
using System.Linq;
using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Core.Layout
{
    public class MindMapLayoutEngine
    {
        public LayoutOptions Options { get; set; } = new LayoutOptions();

        private const double OrgChartLevelGap = 70;
        private const double OrgChartSiblingGap = 28;

        public List<NodeLayout> CalculateLayout(MindMapNode root)
        {
            var layouts = new List<NodeLayout>();
            switch (Options.Layout)
            {
                case "BothSides":
                    LayoutBothSides(root, layouts);
                    break;
                case "OrgChart":
                    LayoutOrgChart(root, null, 0, 0, layouts);
                    break;
                default:
                    LayoutRightTree(root, null, 0, 0, layouts);
                    break;
            }
            return layouts;
        }

        private double CalculateSubtreeHeight(MindMapNode node)
        {
            if (node.Collapsed || node.Children.Count == 0)
                return Options.NodeHeight;

            double total = 0;
            for (int i = 0; i < node.Children.Count; i++)
                total += CalculateSubtreeHeight(node.Children[i]);
            total += (node.Children.Count - 1) * Options.VerticalGap;
            return Math.Max(total, Options.NodeHeight);
        }

        private int GetDepth(MindMapNode parent, List<NodeLayout> layouts)
        {
            if (parent == null) return 0;
            var parentLayout = layouts.Find(l => l.NodeId == parent.Id);
            return (parentLayout?.Depth ?? 0) + 1;
        }

        private void LayoutRightTree(MindMapNode node, MindMapNode parent, double x, double y, List<NodeLayout> layouts)
        {
            double subtreeHeight = CalculateSubtreeHeight(node);
            double nodeY = y + (subtreeHeight - Options.NodeHeight) / 2;

            layouts.Add(new NodeLayout
            {
                NodeId = node.Id,
                X = x,
                Y = nodeY,
                Width = Options.NodeWidth,
                Height = Options.NodeHeight,
                Depth = GetDepth(parent, layouts)
            });

            if (!node.Collapsed && node.Children.Count > 0)
            {
                double childX = x + Options.NodeWidth + Options.LevelGap;
                double childY = y;

                for (int i = 0; i < node.Children.Count; i++)
                {
                    var child = node.Children[i];
                    double childSubtreeHeight = CalculateSubtreeHeight(child);
                    LayoutRightTree(child, node, childX, childY, layouts);
                    childY += childSubtreeHeight + Options.VerticalGap;
                }
            }
        }

        private void LayoutLeftTree(MindMapNode node, MindMapNode parent, double x, double y, List<NodeLayout> layouts)
        {
            double subtreeHeight = CalculateSubtreeHeight(node);
            double nodeY = y + (subtreeHeight - Options.NodeHeight) / 2;

            layouts.Add(new NodeLayout
            {
                NodeId = node.Id,
                X = x,
                Y = nodeY,
                Width = Options.NodeWidth,
                Height = Options.NodeHeight,
                Depth = GetDepth(parent, layouts)
            });

            if (!node.Collapsed && node.Children.Count > 0)
            {
                double childX = x - Options.NodeWidth - Options.LevelGap;
                double childY = y;

                for (int i = 0; i < node.Children.Count; i++)
                {
                    var child = node.Children[i];
                    double childSubtreeHeight = CalculateSubtreeHeight(child);
                    LayoutLeftTree(child, node, childX, childY, layouts);
                    childY += childSubtreeHeight + Options.VerticalGap;
                }
            }
        }

        private void LayoutBothSides(MindMapNode root, List<NodeLayout> layouts)
        {
            var children = root.Collapsed
                ? new List<MindMapNode>()
                : root.Children;

            int mid = (children.Count + 1) / 2;
            var rightChildren = children.Take(mid).ToList();
            var leftChildren = children.Skip(mid).ToList();

            double rightHeight = ColumnHeight(rightChildren);
            double leftHeight = ColumnHeight(leftChildren);
            double total = Math.Max(Options.NodeHeight, Math.Max(rightHeight, leftHeight));

            layouts.Add(new NodeLayout
            {
                NodeId = root.Id,
                X = 0,
                Y = (total - Options.NodeHeight) / 2,
                Width = Options.NodeWidth,
                Height = Options.NodeHeight,
                Depth = 0
            });

            double rightX = Options.NodeWidth + Options.LevelGap;
            double y = (total - rightHeight) / 2;
            foreach (var child in rightChildren)
            {
                double h = CalculateSubtreeHeight(child);
                LayoutRightTree(child, root, rightX, y, layouts);
                y += h + Options.VerticalGap;
            }

            double leftX = -(Options.NodeWidth + Options.LevelGap);
            y = (total - leftHeight) / 2;
            foreach (var child in leftChildren)
            {
                double h = CalculateSubtreeHeight(child);
                LayoutLeftTree(child, root, leftX, y, layouts);
                y += h + Options.VerticalGap;
            }
        }

        private double ColumnHeight(List<MindMapNode> children)
        {
            if (children.Count == 0) return 0;
            double total = 0;
            foreach (var child in children)
                total += CalculateSubtreeHeight(child);
            total += (children.Count - 1) * Options.VerticalGap;
            return total;
        }

        private double CalculateSubtreeWidth(MindMapNode node)
        {
            if (node.Collapsed || node.Children.Count == 0)
                return Options.NodeWidth;

            double total = 0;
            for (int i = 0; i < node.Children.Count; i++)
                total += CalculateSubtreeWidth(node.Children[i]);
            total += (node.Children.Count - 1) * OrgChartSiblingGap;
            return Math.Max(total, Options.NodeWidth);
        }

        private void LayoutOrgChart(MindMapNode node, MindMapNode parent, double x, double y, List<NodeLayout> layouts)
        {
            double subtreeWidth = CalculateSubtreeWidth(node);
            double nodeX = x + (subtreeWidth - Options.NodeWidth) / 2;

            layouts.Add(new NodeLayout
            {
                NodeId = node.Id,
                X = nodeX,
                Y = y,
                Width = Options.NodeWidth,
                Height = Options.NodeHeight,
                Depth = GetDepth(parent, layouts)
            });

            if (!node.Collapsed && node.Children.Count > 0)
            {
                double childY = y + Options.NodeHeight + OrgChartLevelGap;
                double childX = x;

                for (int i = 0; i < node.Children.Count; i++)
                {
                    var child = node.Children[i];
                    double childWidth = CalculateSubtreeWidth(child);
                    LayoutOrgChart(child, node, childX, childY, layouts);
                    childX += childWidth + OrgChartSiblingGap;
                }
            }
        }
    }
}
