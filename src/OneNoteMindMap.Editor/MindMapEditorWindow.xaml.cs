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
        private bool _isDirty;

        public MindMapDocument Document => _document;
        public bool IsSaved { get; private set; }

        public MindMapEditorWindow(MindMapDocument doc, string pageId)
        {
            _document = doc;
            _pageId = pageId;
            _engine = new MindMapLayoutEngine();
            InitializeComponent();
            InputMethod.SetIsInputMethodEnabled(this, false);
            ApplyLocalization();
            ApplySettings();
            RenderMindMap();
            this.Title = EditorStrings.EditorTitle + " - " + (doc.Title ?? EditorStrings.If("未命名", "Untitled"));
            Loaded += MindMapEditorWindow_Loaded;
            Closing += MindMapEditorWindow_Closing;
            KeyDown += MindMapEditorWindow_KeyDown;
        }

        private void ApplyLocalization()
        {
            BtnAddSibling.Content = EditorStrings.AddSibling;
            BtnAddSibling.ToolTip = EditorStrings.TipAddSibling;
            BtnAddChild.Content = EditorStrings.AddChild;
            BtnAddChild.ToolTip = EditorStrings.TipAddChild;
            BtnDelete.Content = EditorStrings.Delete;
            BtnDelete.ToolTip = EditorStrings.TipDelete;
            BtnCollapse.Content = EditorStrings.CollapseExpand;
            BtnCollapse.ToolTip = EditorStrings.TipCollapse;
            BtnSave.Content = EditorStrings.Save;
            BtnSave.ToolTip = EditorStrings.TipSave;
            BtnExportPng.Content = EditorStrings.ExportPng;
            BtnExportPng.ToolTip = EditorStrings.TipExportPng;
            BtnExportSvg.Content = EditorStrings.ExportSvg;
            BtnExportSvg.ToolTip = EditorStrings.TipExportSvg;
            LblLayout.Content = EditorStrings.LayoutLabel;
            LblTheme.Content = EditorStrings.ThemeLabel;
            LblShape.Content = EditorStrings.ShapeLabel;

            ((ComboBoxItem)LayoutCombo.Items[0]).Content = EditorStrings.LayoutRightTree;
            ((ComboBoxItem)LayoutCombo.Items[1]).Content = EditorStrings.LayoutBothSides;
            ((ComboBoxItem)LayoutCombo.Items[2]).Content = EditorStrings.LayoutOrgChart;
            ((ComboBoxItem)ThemeCombo.Items[0]).Content = EditorStrings.ThemeDefault;
            ((ComboBoxItem)ThemeCombo.Items[1]).Content = EditorStrings.ThemePurple;
            ((ComboBoxItem)ThemeCombo.Items[2]).Content = EditorStrings.ThemeMinimal;
            ((ComboBoxItem)ThemeCombo.Items[3]).Content = EditorStrings.ThemeFresh;
            ((ComboBoxItem)ThemeCombo.Items[4]).Content = EditorStrings.ThemeWarm;
            ((ComboBoxItem)ShapeCombo.Items[0]).Content = EditorStrings.ShapeRounded;
            ((ComboBoxItem)ShapeCombo.Items[1]).Content = EditorStrings.ShapeRectangle;
            ((ComboBoxItem)ShapeCombo.Items[2]).Content = EditorStrings.ShapePill;

            StatusText.Text = EditorStrings.Ready;
        }

        private void MindMapEditorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Skip shortcuts when editing a node text box
            if (e.OriginalSource is TextBox) return;

            if (e.Key == Key.Enter)
            {
                AddSibling_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                AddChild_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                DeleteNode_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                if (_selectedNode != null)
                {
                    _selectedNode.EnterEditMode();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Space)
            {
                CollapseToggle_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Save_Click(sender, e);
                e.Handled = true;
            }
        }

        private void MindMapEditorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isDirty && !IsSaved)
            {
                var result = MessageBox.Show(
                    EditorStrings.UnsavedChanges,
                    EditorStrings.UnsavedTitle,
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveMindMap();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
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
                var parentNode = _document.Root.FindParentOf(layout.NodeId);
                if (parentNode == null) continue;

                var parentLayout = layouts.FirstOrDefault(l => l.NodeId == parentNode.Id);
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
                var node = _document.Root.FindById(layout.NodeId);
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

        private void UpdateStatusBar()
        {
            if (_document?.Root == null || StatusText == null || ZoomText == null)
                return;

            int nodeCount = CountNodes(_document.Root);
            int zoomPercent = (int)(_zoomLevel * 100);
            StatusText.Text = EditorStrings.StatusFormat(nodeCount, zoomPercent);
            ZoomText.Text = $"{zoomPercent}%";
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
            return root.FindParentOf(childId);
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
                _document.Root.Children.Add(new MindMapNode { Text = EditorStrings.NewNodeText });
                _isDirty = true;
                RenderMindMap();
                return;
            }

            var parent = GetParentNode(_document.Root, node.Id);
            if (parent == null) return;

            var newNode = new MindMapNode { Text = EditorStrings.NewNodeText };
            int idx = parent.Children.IndexOf(node);
            parent.Children.Insert(idx + 1, newNode);
            _isDirty = true;
            RenderMindMap();
        }

        private void AddChild_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null) node = _document.Root;

            var newNode = new MindMapNode { Text = EditorStrings.NewNodeText };
            node.Children.Add(newNode);
            _isDirty = true;
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
            _isDirty = true;
            RenderMindMap();
        }

        private void CollapseToggle_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null) return;
            node.Collapsed = !node.Collapsed;
            _isDirty = true;
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
                StatusText.Text = EditorStrings.SavedToOneNote;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(EditorStrings.SaveFailed + ": " + ex.Message, EditorStrings.UnsavedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPng_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = EditorStrings.PngFilter,
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
                        StatusText.Text = EditorStrings.PngExported;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(EditorStrings.ExportFailed + ": " + ex.Message);
                }
            }
        }

        private void ExportSvg_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = EditorStrings.SvgFilter,
                FileName = (_document.Title ?? "mindmap") + ".svg"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var layouts = _engine.CalculateLayout(_document.Root);
                    string svg = SvgRenderer.Render(_document, layouts);
                    File.WriteAllText(dialog.FileName, svg);
                    StatusText.Text = EditorStrings.SvgExported;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(EditorStrings.ExportFailed + ": " + ex.Message);
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
