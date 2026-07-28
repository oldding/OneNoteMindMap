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
        private NodeControl _dragNode;
        private NodeControl _dropTarget;
        private Point _dragStartPosition;
        private double _dragStartLeft;
        private double _dragStartTop;
        private double _dragStartOffsetX;
        private double _dragStartOffsetY;
        private bool _isNodeDragging;
        private System.Collections.Generic.List<NodeLayout> _currentLayouts =
            new System.Collections.Generic.List<NodeLayout>();
        private readonly System.Collections.Generic.Dictionary<string, System.Windows.Shapes.Path> _connectionPaths =
            new System.Collections.Generic.Dictionary<string, System.Windows.Shapes.Path>();
        private readonly System.Collections.Generic.Dictionary<string, System.Windows.Shapes.Path> _endpointPaths =
            new System.Collections.Generic.Dictionary<string, System.Windows.Shapes.Path>();
        private double _canvasOffsetX;
        private double _canvasOffsetY;
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
            LblConnection.Content = EditorStrings.ConnectionLabel;
            LblEndpoint.Content = EditorStrings.EndpointLabel;

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
            ((ComboBoxItem)ConnectionCombo.Items[0]).Content = EditorStrings.ConnectionCurved;
            ((ComboBoxItem)ConnectionCombo.Items[1]).Content = EditorStrings.ConnectionStraight;
            ((ComboBoxItem)ConnectionCombo.Items[2]).Content = EditorStrings.ConnectionOrthogonal;
            ((ComboBoxItem)ConnectionCombo.Items[3]).Content = EditorStrings.ConnectionClassicMindMap;
            ((ComboBoxItem)EndpointCombo.Items[0]).Content = EditorStrings.EndpointNone;
            ((ComboBoxItem)EndpointCombo.Items[1]).Content = EditorStrings.EndpointArrow;
            ((ComboBoxItem)EndpointCombo.Items[2]).Content = EditorStrings.EndpointDoubleArrow;
            ((ComboBoxItem)EndpointCombo.Items[3]).Content = EditorStrings.EndpointCircle;
            ((ComboBoxItem)EndpointCombo.Items[4]).Content = EditorStrings.EndpointDiamond;

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
                _engine.Options.ConnectionStyle = _document.Settings.ConnectionStyle ?? "Curved";
                _engine.Options.EndpointStyle = _document.Settings.EndpointStyle ?? "None";
                _engine.Options.NodeWidth = _document.Settings.NodeWidth;
                _engine.Options.NodeHeight = _document.Settings.NodeHeight;
                _engine.Options.HorizontalGap = _document.Settings.HorizontalGap;
                _engine.Options.VerticalGap = _document.Settings.VerticalGap;
                _engine.Options.LevelGap = _document.Settings.LevelGap;
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
                foreach (var item in ConnectionCombo.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _document.Settings.ConnectionStyle)
                    {
                        ConnectionCombo.SelectedItem = item;
                        break;
                    }
                }
                foreach (var item in EndpointCombo.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _document.Settings.EndpointStyle)
                    {
                        EndpointCombo.SelectedItem = item;
                        break;
                    }
                }
                EndpointCombo.IsEnabled = _engine.Options.ConnectionStyle != "ClassicMindMap";
            }
        }

        private void RenderMindMap()
        {
            if (_document?.Root == null || _engine == null || MainCanvas == null)
                return;

            MainCanvas.Children.Clear();
            _connectionPaths.Clear();
            _endpointPaths.Clear();

            var layouts = _engine.CalculateLayout(_document.Root);
            _currentLayouts = layouts;

            if (layouts.Count == 0) return;

            double minX = layouts.Min(l => l.X);
            double minY = layouts.Min(l => l.Y);

            double offsetX = 60 - minX;
            double offsetY = 60 - minY;
            _canvasOffsetX = offsetX;
            _canvasOffsetY = offsetY;

            // Draw connection lines first so they never cover node hit testing.
            foreach (var layout in layouts.Where(l => l.Depth > 0))
            {
                var parentNode = _document.Root.FindParentOf(layout.NodeId);
                if (parentNode == null) continue;

                var parentLayout = layouts.FirstOrDefault(l => l.NodeId == parentNode.Id);
                if (parentLayout == null) continue;

                string lineColor = _engine.Options.ConnectionStyle == "ClassicMindMap"
                    ? ClassicMindMapStyle.GetBranchColor(_document.Root, _document.Root.FindById(layout.NodeId))
                    : "#AAAAAA";
                var line = new System.Windows.Shapes.Path
                {
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineColor)),
                    StrokeThickness = 2,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = LinkGeometryBuilder.Build(
                        parentLayout,
                        layout,
                        offsetX,
                        offsetY,
                        _engine.Options.ConnectionStyle),
                    IsHitTestVisible = false
                };
                MainCanvas.Children.Add(line);
                _connectionPaths[layout.NodeId] = line;

                var endpoint = new System.Windows.Shapes.Path
                {
                    Fill = line.Stroke,
                    Stroke = line.Stroke,
                    StrokeThickness = 1,
                    Data = LinkEndpointBuilder.Build(
                        parentLayout,
                        layout,
                        offsetX,
                        offsetY,
                        _engine.Options.ConnectionStyle,
                        _engine.Options.EndpointStyle),
                    IsHitTestVisible = false
                };
                MainCanvas.Children.Add(endpoint);
                _endpointPaths[layout.NodeId] = endpoint;
            }

            foreach (var layout in layouts)
            {
                var node = _document.Root.FindById(layout.NodeId);
                if (node == null) continue;

                string branchColor = ClassicMindMapStyle.GetBranchColor(_document.Root, node);
                var ctrl = new NodeControl(node, layout, _engine.Options, branchColor);
                Canvas.SetLeft(ctrl, layout.X + offsetX);
                Canvas.SetTop(ctrl, layout.Y + offsetY);
                ctrl.MouseDown += NodeCtrl_MouseDown;
                ctrl.MouseMove += NodeCtrl_MouseMove;
                ctrl.MouseUp += NodeCtrl_MouseUp;
                ctrl.RequestEdit += NodeCtrl_RequestEdit;
                ctrl.ContentChanged += NodeCtrl_ContentChanged;
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
            if (sender is NodeControl ctrl && !ctrl.IsEditing && e.ChangedButton == MouseButton.Left)
            {
                SelectNode(ctrl);
                _dragNode = ctrl;
                _dragStartPosition = e.GetPosition(MainCanvas);
                _dragStartLeft = Canvas.GetLeft(ctrl);
                _dragStartTop = Canvas.GetTop(ctrl);
                _dragStartOffsetX = ctrl.Node.ManualOffsetX;
                _dragStartOffsetY = ctrl.Node.ManualOffsetY;
                _isNodeDragging = false;
                ctrl.CaptureMouse();
                e.Handled = true;
            }
        }

        private void NodeCtrl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragNode == null || sender != _dragNode || e.LeftButton != MouseButtonState.Pressed)
                return;

            var position = e.GetPosition(MainCanvas);
            double deltaX = position.X - _dragStartPosition.X;
            double deltaY = position.Y - _dragStartPosition.Y;

            if (!_isNodeDragging)
            {
                if (Math.Abs(deltaX) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(deltaY) < SystemParameters.MinimumVerticalDragDistance)
                    return;

                _isNodeDragging = true;
            }

            Canvas.SetLeft(_dragNode, _dragStartLeft + deltaX);
            Canvas.SetTop(_dragNode, _dragStartTop + deltaY);
            _dragNode.Node.ManualOffsetX = _dragStartOffsetX + deltaX;
            _dragNode.Node.ManualOffsetY = _dragStartOffsetY + deltaY;

            var layout = _currentLayouts.FirstOrDefault(l => l.NodeId == _dragNode.Node.Id);
            if (layout != null)
            {
                layout.X = _dragStartLeft + deltaX - _canvasOffsetX;
                layout.Y = _dragStartTop + deltaY - _canvasOffsetY;
            }

            RefreshConnectionLines();
            UpdateDropTarget(position);
            e.Handled = true;
        }

        private void NodeCtrl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragNode == null || sender != _dragNode || e.ChangedButton != MouseButton.Left)
                return;

            string nodeId = _dragNode.Node.Id;
            bool moved = _isNodeDragging;
            string dropTargetId = _dropTarget?.Node.Id;
            ClearDropTarget();

            if (_dragNode.IsMouseCaptured)
                _dragNode.ReleaseMouseCapture();

            _dragNode = null;
            _isNodeDragging = false;

            if (moved)
            {
                _isDirty = true;
                if (!string.IsNullOrEmpty(dropTargetId))
                    _document.Root.ReparentNode(nodeId, dropTargetId);

                RenderMindMap();
                var movedControl = MainCanvas.Children
                    .OfType<NodeControl>()
                    .FirstOrDefault(ctrl => ctrl.Node.Id == nodeId);
                SelectNode(movedControl);
            }

            e.Handled = true;
        }

        private void UpdateDropTarget(Point position)
        {
            var candidate = FindDropTarget(position);
            if (_dropTarget == candidate)
                return;

            ClearDropTarget();
            _dropTarget = candidate;
            if (_dropTarget != null)
                _dropTarget.IsDropTarget = true;
        }

        private NodeControl FindDropTarget(Point position)
        {
            if (_dragNode == null || _document.Root == _dragNode.Node)
                return null;

            var currentParent = _document.Root.FindParentOf(_dragNode.Node.Id);

            foreach (var candidate in MainCanvas.Children.OfType<NodeControl>())
            {
                if (candidate == _dragNode ||
                    candidate.Node == currentParent ||
                    _dragNode.Node.FindById(candidate.Node.Id) != null)
                    continue;

                double left = Canvas.GetLeft(candidate);
                double top = Canvas.GetTop(candidate);
                double width = candidate.ActualWidth > 0 ? candidate.ActualWidth : candidate.Width;
                double height = candidate.ActualHeight > 0 ? candidate.ActualHeight : candidate.Height;

                if (position.X >= left && position.X <= left + width &&
                    position.Y >= top && position.Y <= top + height)
                    return candidate;
            }

            return null;
        }

        private void ClearDropTarget()
        {
            if (_dropTarget != null)
                _dropTarget.IsDropTarget = false;
            _dropTarget = null;
        }

        private void RefreshConnectionLines()
        {
            foreach (var layout in _currentLayouts.Where(l => l.Depth > 0))
            {
                if (!_connectionPaths.TryGetValue(layout.NodeId, out var line))
                    continue;

                var parentNode = _document.Root.FindParentOf(layout.NodeId);
                if (parentNode == null) continue;

                var parentLayout = _currentLayouts.FirstOrDefault(l => l.NodeId == parentNode.Id);
                if (parentLayout == null) continue;

                line.Data = LinkGeometryBuilder.Build(
                    parentLayout,
                    layout,
                    _canvasOffsetX,
                    _canvasOffsetY,
                    _engine.Options.ConnectionStyle);

                if (_endpointPaths.TryGetValue(layout.NodeId, out var endpoint))
                {
                    endpoint.Data = LinkEndpointBuilder.Build(
                        parentLayout,
                        layout,
                        _canvasOffsetX,
                        _canvasOffsetY,
                        _engine.Options.ConnectionStyle,
                        _engine.Options.EndpointStyle);
                }
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

        private void NodeCtrl_ContentChanged(object sender, EventArgs e)
        {
            if (!(sender is NodeControl ctrl))
                return;

            string nodeId = ctrl.Node.Id;
            _isDirty = true;
            RenderMindMap();
            var editedControl = MainCanvas.Children
                .OfType<NodeControl>()
                .FirstOrDefault(item => item.Node.Id == nodeId);
            SelectNode(editedControl);
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
                if (_document.Settings == null)
                    _document.Settings = new MindMapSettings();
                ApplyToolbarSettings(_document.Settings);

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
                    var pngBytes = PngExporter.Export(CreateExportDocument(), 2.0);
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
                    var exportDocument = CreateExportDocument();
                    var layouts = _engine.CalculateLayout(exportDocument.Root);
                    string svg = SvgRenderer.Render(exportDocument, layouts);
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
                if (IsLoaded) _isDirty = true;
                RenderMindMap();
            }
        }

        private void Theme_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_engine != null && ThemeCombo?.SelectedItem is ComboBoxItem item)
                _engine.Options.Theme = item.Tag?.ToString() ?? "Default";
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private void Shape_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_engine != null && ShapeCombo?.SelectedItem is ComboBoxItem item)
                _engine.Options.NodeShape = item.Tag?.ToString() ?? "Rounded";
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private void Connection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_engine != null && ConnectionCombo?.SelectedItem is ComboBoxItem item)
                _engine.Options.ConnectionStyle = item.Tag?.ToString() ?? "Curved";
            if (EndpointCombo != null)
                EndpointCombo.IsEnabled = _engine?.Options.ConnectionStyle != "ClassicMindMap";
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private void Endpoint_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_engine != null && EndpointCombo?.SelectedItem is ComboBoxItem item)
                _engine.Options.EndpointStyle = item.Tag?.ToString() ?? "None";
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private MindMapDocument CreateExportDocument()
        {
            var exportDocument = _document.Clone();
            ApplyToolbarSettings(exportDocument.Settings);
            return exportDocument;
        }

        private void ApplyToolbarSettings(MindMapSettings settings)
        {
            settings.Layout = ((ComboBoxItem)LayoutCombo.SelectedItem)?.Tag?.ToString() ?? "RightTree";
            settings.Theme = ((ComboBoxItem)ThemeCombo.SelectedItem)?.Tag?.ToString() ?? "Default";
            settings.NodeShape = ((ComboBoxItem)ShapeCombo.SelectedItem)?.Tag?.ToString() ?? "Rounded";
            settings.ConnectionStyle = ((ComboBoxItem)ConnectionCombo.SelectedItem)?.Tag?.ToString() ?? "Curved";
            settings.EndpointStyle = ((ComboBoxItem)EndpointCombo.SelectedItem)?.Tag?.ToString() ?? "None";
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
