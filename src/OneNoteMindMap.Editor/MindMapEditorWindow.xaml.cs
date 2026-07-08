using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Core.Rendering;

namespace OneNoteMindMap.Editor
{
    public partial class MindMapEditorWindow : Window
    {
        private MindMapDocument _document;
        private string _pageId;
        private Point _lastMousePos;
        private NodeControl _selectedNode;
        private double _zoomLevel = 1.0;
        private MindMapLayoutEngine _engine;

        public MindMapDocument Document => _document;
        public bool IsSaved { get; private set; }

        public MindMapEditorWindow(MindMapDocument doc, string pageId)
        {
            _document = doc;
            _pageId = pageId;
            _engine = new MindMapLayoutEngine();
            InitializeComponent();
            InputMethod.SetIsInputMethodEnabled(this, false);
            ApplySettings();
            RenderMindMap();
            this.Title = "脑图编辑器 - " + (doc.Title ?? "未命名");
            Loaded += MindMapEditorWindow_Loaded;
        }

        private void MindMapEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Activate();
            Focus();
            Dispatcher.BeginInvoke(new Action(() => Topmost = false), DispatcherPriority.ApplicationIdle);
        }

        private void ApplySettings()
        {
            if (_document.Settings != null)
            {
                _engine.Options.Layout = _document.Settings.Layout ?? "RightTree";
                _engine.Options.Theme = _document.Settings.Theme ?? "Default";
                _engine.Options.NodeShape = _document.Settings.NodeShape ?? "Rounded";
                foreach (var item in LayoutCombo.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _document.Settings.Layout)
                    {
                        LayoutCombo.SelectedItem = item;
                        break;
                    }
                }
                foreach (var item in ThemeCombo.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _document.Settings.Theme)
                    {
                        ThemeCombo.SelectedItem = item;
                        break;
                    }
                }
                foreach (var item in ShapeCombo.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _document.Settings.NodeShape)
                    {
                        ShapeCombo.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void RenderMindMap()
        {
            if (_document?.Root == null || _engine == null || MainCanvas == null)
                return;

            MainCanvas.Children.Clear();

            var layouts = _engine.CalculateLayout(_document.Root);

            if (layouts.Count == 0) return;

            double minX = layouts.Min(l => l.X);
            double minY = layouts.Min(l => l.Y);

            double offsetX = 60 - minX;
            double offsetY = 60 - minY;

            // Draw connection lines first so they never cover node hit testing.
            foreach (var layout in layouts.Where(l => l.Depth > 0))
            {
                var parentId = FindParentId(_document.Root, layout.NodeId);
                if (parentId == null) continue;

                var parentLayout = layouts.FirstOrDefault(l => l.NodeId == parentId);
                if (parentLayout == null) continue;

                var line = new System.Windows.Shapes.Path
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                    StrokeThickness = 2,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = LinkGeometryBuilder.Build(parentLayout, layout, offsetX, offsetY),
                    IsHitTestVisible = false
                };
                MainCanvas.Children.Add(line);
            }

            foreach (var layout in layouts)
            {
                var node = FindNode(_document.Root, layout.NodeId);
                if (node == null) continue;

                var ctrl = new NodeControl(node, layout, _engine.Options);
                Canvas.SetLeft(ctrl, layout.X + offsetX);
                Canvas.SetTop(ctrl, layout.Y + offsetY);
                ctrl.MouseDown += NodeCtrl_MouseDown;
                ctrl.RequestEdit += NodeCtrl_RequestEdit;
                MainCanvas.Children.Add(ctrl);
            }

            UpdateCanvasSize(layouts, offsetX, offsetY);
            UpdateStatusBar();
        }

        private void UpdateCanvasSize(System.Collections.Generic.List<NodeLayout> layouts, double offsetX, double offsetY)
        {
            if (layouts.Count == 0) return;

            double maxX = layouts.Max(l => l.Right) + offsetX + 60;
            double maxY = layouts.Max(l => l.Bottom) + offsetY + 60;

            MainCanvas.Width = Math.Max(maxX, 800);
            MainCanvas.Height = Math.Max(maxY, 600);
        }

        private MindMapNode FindNode(MindMapNode root, string id)
        {
            if (root.Id == id) return root;
            foreach (var child in root.Children)
            {
                var found = FindNode(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private string FindParentId(MindMapNode root, string childId)
        {
            foreach (var child in root.Children)
            {
                if (child.Id == childId) return root.Id;
                var found = FindParentId(child, childId);
                if (found != null) return found;
            }
            return null;
        }

        private void UpdateStatusBar()
        {
            if (_document?.Root == null || StatusText == null || ZoomText == null)
                return;

            int nodeCount = CountNodes(_document.Root);
            StatusText.Text = $"节点数: {nodeCount}  |  缩放: {(_zoomLevel * 100):F0}%";
            ZoomText.Text = $"{(_zoomLevel * 100):F0}%";
        }

        private int CountNodes(MindMapNode node)
        {
            int count = 1;
            foreach (var child in node.Children)
                count += CountNodes(child);
            return count;
        }

        private void SelectNode(NodeControl ctrl)
        {
            if (_selectedNode != null)
                _selectedNode.IsSelected = false;
            _selectedNode = ctrl;
            if (_selectedNode != null)
                _selectedNode.IsSelected = true;
        }

        private MindMapNode GetSelectedNodeData()
        {
            return _selectedNode?.Node;
        }

        private MindMapNode GetParentNode(MindMapNode root, string childId)
        {
            foreach (var child in root.Children)
            {
                if (child.Id == childId) return root;
                var found = GetParentNode(child, childId);
                if (found != null) return found;
            }
            return null;
        }

        private void NodeCtrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is NodeControl ctrl)
            {
                SelectNode(ctrl);
                _lastMousePos = e.GetPosition(MainCanvas);
            }
        }

        private void NodeCtrl_RequestEdit(object sender, EventArgs e)
        {
            if (sender is NodeControl ctrl)
            {
                SelectNode(ctrl);
                ctrl.EnterEditMode();
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == MainCanvas)
            {
                if (_selectedNode != null)
                {
                    _selectedNode.IsSelected = false;
                    _selectedNode = null;
                }
                _lastMousePos = e.GetPosition(MainCanvas);
                MainCanvas.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _selectedNode == null)
            {
                // Pan
                var pos = e.GetPosition(MainCanvas);
                double dx = pos.X - _lastMousePos.X;
                double dy = pos.Y - _lastMousePos.Y;

                var scrollViewer = FindVisualParent<ScrollViewer>(MainCanvas);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dx);
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dy);
                }
                _lastMousePos = pos;
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MainCanvas.IsMouseCaptured)
                MainCanvas.ReleaseMouseCapture();
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? 0.1 : -0.1;
            _zoomLevel = Math.Max(0.2, Math.Min(3.0, _zoomLevel + delta));
            ZoomTransform.ScaleX = _zoomLevel;
            ZoomTransform.ScaleY = _zoomLevel;
            UpdateStatusBar();
        }

        private void AddSibling_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null || _document.Root == node)
            {
                _document.Root.Children.Add(new MindMapNode { Text = "新节点" });
                IsSaved = true;
                RenderMindMap();
                return;
            }

            var parent = GetParentNode(_document.Root, node.Id);
            if (parent == null) return;

            var newNode = new MindMapNode { Text = "新节点" };
            int idx = parent.Children.IndexOf(node);
            parent.Children.Insert(idx + 1, newNode);
            IsSaved = true;
            RenderMindMap();
        }

        private void AddChild_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null) node = _document.Root;

            var newNode = new MindMapNode { Text = "新节点" };
            node.Children.Add(newNode);
            IsSaved = true;
            RenderMindMap();
        }

        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null || _document.Root == node) return;

            var parent = GetParentNode(_document.Root, node.Id);
            if (parent == null) return;

            parent.Children.Remove(node);
            _selectedNode = null;
            IsSaved = true;
            RenderMindMap();
        }

        private void CollapseToggle_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null) return;
            node.Collapsed = !node.Collapsed;
            IsSaved = true;
            RenderMindMap();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveMindMap();
        }

        private void SaveMindMap()
        {
            try
            {
                _document.Settings = new MindMapSettings
                {
                    Layout = ((ComboBoxItem)LayoutCombo.SelectedItem)?.Tag?.ToString() ?? "RightTree",
                    Theme = ((ComboBoxItem)ThemeCombo.SelectedItem)?.Tag?.ToString() ?? "Default",
                    NodeShape = ((ComboBoxItem)ShapeCombo.SelectedItem)?.Tag?.ToString() ?? "Rounded"
                };

                IsSaved = true;
                StatusText.Text = "已保存到 OneNote 页面";
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPng_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG 图片|*.png",
                FileName = (_document.Title ?? "mindmap") + ".png"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var pngBytes = PngExporter.Export(_document, 2.0);
                    if (pngBytes != null)
                    {
                        File.WriteAllBytes(dialog.FileName, pngBytes);
                        StatusText.Text = "PNG 已导出";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败: " + ex.Message);
                }
            }
        }

        private void ExportSvg_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "SVG 文件|*.svg",
                FileName = (_document.Title ?? "mindmap") + ".svg"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var layouts = _engine.CalculateLayout(_document.Root);
                    string svg = SvgRenderer.Render(_document, layouts);
                    File.WriteAllText(dialog.FileName, svg);
                    StatusText.Text = "SVG 已导出";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败: " + ex.Message);
                }
            }
        }

        private void Layout_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_engine != null && LayoutCombo?.SelectedItem is ComboBoxItem item)
            {
                _engine.Options.Layout = item.Tag?.ToString() ?? "RightTree";
                RenderMindMap();
            }
        }

        private void Theme_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_engine != null && ThemeCombo?.SelectedItem is ComboBoxItem item)
                _engine.Options.Theme = item.Tag?.ToString() ?? "Default";
            RenderMindMap();
        }

        private void Shape_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_engine != null && ShapeCombo?.SelectedItem is ComboBoxItem item)
                _engine.Options.NodeShape = item.Tag?.ToString() ?? "Rounded";
            RenderMindMap();
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T t) return t;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
