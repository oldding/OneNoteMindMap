using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OneNoteMindMap.Core.Serialization;

namespace OneNoteMindMap.Editor
{
    public partial class MindMapEditorWindow : Window
    {
        private MindMapDocument _document;
        private string _pageId;
        private Point _lastMousePos;
        private NodeControl _selectedNode;
        private readonly HashSet<string> _selectedNodeIds = new HashSet<string>();
        private string _primarySelectedNodeId;
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
        private bool _isRestoringHistory;
        private readonly List<EditorState> _undoHistory = new List<EditorState>();
        private readonly List<EditorState> _redoHistory = new List<EditorState>();
        private const int MaxHistoryEntries = 100;
        private const string NodeClipboardFormat = "OneNoteMindMap.Node.V1";

        private sealed class EditorState
        {
            public MindMapDocument Document { get; set; }
            public List<string> SelectedNodeIds { get; set; }
            public string PrimarySelectedNodeId { get; set; }
            public bool IsDirty { get; set; }
        }

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
            BtnUndo.Content = EditorStrings.Undo;
            BtnUndo.ToolTip = EditorStrings.TipUndo;
            BtnRedo.Content = EditorStrings.Redo;
            BtnRedo.ToolTip = EditorStrings.TipRedo;
            BtnAddSibling.Content = EditorStrings.AddSibling;
            BtnAddSibling.ToolTip = EditorStrings.TipAddSibling;
            BtnAddChild.Content = EditorStrings.AddChild;
            BtnAddChild.ToolTip = EditorStrings.TipAddChild;
            BtnDelete.Content = EditorStrings.Delete;
            BtnDelete.ToolTip = EditorStrings.TipDelete;
            BtnCollapse.Content = EditorStrings.CollapseExpand;
            BtnCollapse.ToolTip = EditorStrings.TipCollapse;
            BtnProperties.Content = EditorStrings.Properties;
            BtnProperties.ToolTip = EditorStrings.TipProperties;
            BtnBatchFormat.Content = EditorStrings.BatchFormat;
            BtnBatchFormat.ToolTip = EditorStrings.TipBatchFormat;
            BtnOpenLink.Content = EditorStrings.OpenLink;
            BtnOpenLink.ToolTip = EditorStrings.TipOpenLink;
            BtnCopy.Content = EditorStrings.Copy;
            BtnCopy.ToolTip = EditorStrings.TipCopy;
            BtnPaste.Content = EditorStrings.Paste;
            BtnPaste.ToolTip = EditorStrings.TipPaste;
            BtnMoveUp.Content = EditorStrings.MoveUp;
            BtnMoveUp.ToolTip = EditorStrings.TipMoveUp;
            BtnMoveDown.Content = EditorStrings.MoveDown;
            BtnMoveDown.ToolTip = EditorStrings.TipMoveDown;
            BtnSave.Content = EditorStrings.Save;
            BtnSave.ToolTip = EditorStrings.TipSave;
            BtnExportPng.Content = EditorStrings.ExportPng;
            BtnExportPng.ToolTip = EditorStrings.TipExportPng;
            BtnExportSvg.Content = EditorStrings.ExportSvg;
            BtnExportSvg.ToolTip = EditorStrings.TipExportSvg;
            BtnFitView.Content = EditorStrings.FitView;
            BtnFitView.ToolTip = EditorStrings.TipFitView;
            BtnResetZoom.Content = EditorStrings.ResetZoom;
            BtnResetZoom.ToolTip = EditorStrings.TipResetZoom;
            LblLayout.Content = EditorStrings.LayoutLabel;
            LblTheme.Content = EditorStrings.ThemeLabel;
            LblShape.Content = EditorStrings.ShapeLabel;
            LblNodeSize.Content = EditorStrings.NodeSizeLabel;
            NodeSizeCombo.ToolTip = EditorStrings.TipNodeSize;
            LblConnection.Content = EditorStrings.ConnectionLabel;
            LblEndpoint.Content = EditorStrings.EndpointLabel;
            LblSearch.Content = EditorStrings.SearchLabel;
            SearchBox.ToolTip = EditorStrings.TipSearch;
            BtnFindPrevious.Content = EditorStrings.FindPrevious;
            BtnFindNext.Content = EditorStrings.FindNext;

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
            ((ComboBoxItem)NodeSizeCombo.Items[0]).Content = EditorStrings.NodeSizeFixed;
            ((ComboBoxItem)NodeSizeCombo.Items[1]).Content = EditorStrings.NodeSizeAutoFit;
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

            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Undo();
                e.Handled = true;
            }
            else if ((e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control) ||
                     (e.Key == Key.Z && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)))
            {
                Redo();
                e.Handled = true;
            }
            else if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SearchBox.Focus();
                SearchBox.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                CopySelectedNode();
                e.Handled = true;
            }
            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                PasteNode();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenSelectedLink();
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                if (_selectedNodeIds.Count > 1)
                    BatchFormat_Click(sender, e);
                else
                    EditSelectedNodeProperties();
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt &&
                     (e.Key == Key.Up || e.Key == Key.Down))
            {
                MoveSelectedNodes(e.Key == Key.Up ? -1 : 1);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.None &&
                     (e.Key == Key.Left || e.Key == Key.Right ||
                      e.Key == Key.Up || e.Key == Key.Down))
            {
                NavigateSelection(e.Key);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
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
                _engine.Options.NodeSizeMode = _document.Settings.NodeSizeMode ?? "Fixed";
                _engine.Options.NodeWidth = _document.Settings.NodeWidth;
                _engine.Options.NodeHeight = _document.Settings.NodeHeight;
                _engine.Options.AutoNodeMaxWidth = _document.Settings.AutoNodeMaxWidth > 0
                    ? _document.Settings.AutoNodeMaxWidth
                    : 320;
                _engine.Options.HorizontalGap = _document.Settings.HorizontalGap;
                _engine.Options.VerticalGap = _document.Settings.VerticalGap;
                _engine.Options.LevelGap = _document.Settings.LevelGap;
                SetZoom(_document.Settings.CanvasZoom > 0 ? _document.Settings.CanvasZoom : 1.0);
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
                foreach (var item in NodeSizeCombo.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _document.Settings.NodeSizeMode)
                    {
                        NodeSizeCombo.SelectedItem = item;
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

            _selectedNode = null;
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

                var lineNode = _document.Root.FindById(layout.NodeId);
                string lineColor = _engine.Options.ConnectionStyle == "ClassicMindMap"
                    ? string.IsNullOrWhiteSpace(lineNode?.Color)
                        ? ClassicMindMapStyle.GetBranchColor(_document.Root, lineNode)
                        : lineNode.Color
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
                ctrl.ContentChanging += NodeCtrl_ContentChanging;
                ctrl.ContentChanged += NodeCtrl_ContentChanged;
                MainCanvas.Children.Add(ctrl);
            }

            UpdateCanvasSize(layouts, offsetX, offsetY);
            UpdateStatusBar();
            var visibleNodeIds = new HashSet<string>(
                MainCanvas.Children.OfType<NodeControl>().Select(ctrl => ctrl.Node.Id));
            _selectedNodeIds.RemoveWhere(id => !visibleNodeIds.Contains(id));
            if (!_selectedNodeIds.Contains(_primarySelectedNodeId))
                _primarySelectedNodeId = _selectedNodeIds.FirstOrDefault();
            ApplySelectionVisuals();
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
            if (_selectedNodeIds.Count > 1)
                StatusText.Text += "  |  " + EditorStrings.SelectedStatus(_selectedNodeIds.Count);
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
            _selectedNodeIds.Clear();
            if (ctrl != null)
                _selectedNodeIds.Add(ctrl.Node.Id);
            _primarySelectedNodeId = ctrl?.Node.Id;
            ApplySelectionVisuals();
        }

        private void ToggleNodeSelection(NodeControl ctrl)
        {
            if (ctrl == null)
                return;

            string nodeId = ctrl.Node.Id;
            if (_selectedNodeIds.Contains(nodeId))
            {
                _selectedNodeIds.Remove(nodeId);
                if (_primarySelectedNodeId == nodeId)
                    _primarySelectedNodeId = _selectedNodeIds.FirstOrDefault();
            }
            else
            {
                _selectedNodeIds.Add(nodeId);
                _primarySelectedNodeId = nodeId;
            }
            ApplySelectionVisuals();
        }

        private void ApplySelectionVisuals()
        {
            var controls = MainCanvas.Children.OfType<NodeControl>().ToList();
            foreach (var ctrl in controls)
                ctrl.IsSelected = _selectedNodeIds.Contains(ctrl.Node.Id);

            _selectedNode = controls.FirstOrDefault(ctrl => ctrl.Node.Id == _primarySelectedNodeId)
                ?? controls.FirstOrDefault(ctrl => _selectedNodeIds.Contains(ctrl.Node.Id));
            _primarySelectedNodeId = _selectedNode?.Node.Id;
            UpdateSelectionActions();
            UpdateStatusBar();
        }

        private void UpdateSelectionActions()
        {
            if (BtnProperties == null)
                return;

            int selectedCount = _selectedNodeIds.Count;
            bool hasSingleSelection = selectedCount == 1 && _selectedNode != null;
            BtnProperties.IsEnabled = hasSingleSelection;
            BtnBatchFormat.IsEnabled = selectedCount > 0;
            BtnCopy.IsEnabled = hasSingleSelection;
            BtnOpenLink.IsEnabled = hasSingleSelection &&
                !string.IsNullOrWhiteSpace(_selectedNode.Node.Link);
            BtnMoveUp.IsEnabled = CanMoveSelectedNodes(-1);
            BtnMoveDown.IsEnabled = CanMoveSelectedNodes(1);
        }

        private MindMapNode GetSelectedNodeData()
        {
            return _selectedNode?.Node;
        }

        private List<MindMapNode> GetSelectedNodes()
        {
            return EnumerateNodes(_document.Root)
                .Where(node => _selectedNodeIds.Contains(node.Id))
                .ToList();
        }

        private MindMapNode GetParentNode(MindMapNode root, string childId)
        {
            return root.FindParentOf(childId);
        }

        private void NodeCtrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is NodeControl ctrl && !ctrl.IsEditing && e.ChangedButton == MouseButton.Left)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    ToggleNodeSelection(ctrl);
                    e.Handled = true;
                    return;
                }

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

                RecordUndoState();
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

        private void NodeCtrl_ContentChanging(object sender, EventArgs e)
        {
            RecordUndoState();
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
                    SelectNode(null);
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
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                return;

            double delta = e.Delta > 0 ? 0.1 : -0.1;
            SetZoom(_zoomLevel + delta);
            e.Handled = true;
        }

        private void FitView_Click(object sender, RoutedEventArgs e)
        {
            if (MainCanvas.Width <= 0 || MainCanvas.Height <= 0)
                return;

            double viewportWidth = CanvasScrollViewer.ViewportWidth;
            double viewportHeight = CanvasScrollViewer.ViewportHeight;
            if (double.IsNaN(viewportWidth) || viewportWidth <= 0 ||
                double.IsNaN(viewportHeight) || viewportHeight <= 0)
                return;

            double scaleX = Math.Max(0.2, (viewportWidth - 24) / MainCanvas.Width);
            double scaleY = Math.Max(0.2, (viewportHeight - 24) / MainCanvas.Height);
            SetZoom(Math.Min(1.0, Math.Min(scaleX, scaleY)));
            CanvasScrollViewer.ScrollToHorizontalOffset(0);
            CanvasScrollViewer.ScrollToVerticalOffset(0);
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            SetZoom(1.0);
        }

        private void SetZoom(double zoom)
        {
            _zoomLevel = Math.Max(0.2, Math.Min(3.0, zoom));
            if (ZoomTransform != null)
            {
                ZoomTransform.ScaleX = _zoomLevel;
                ZoomTransform.ScaleY = _zoomLevel;
            }
            UpdateStatusBar();
        }

        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedNodeProperties();
        }

        private void EditSelectedNodeProperties()
        {
            if (_selectedNodeIds.Count != 1)
                return;
            var node = GetSelectedNodeData();
            if (node == null)
                return;

            var dialog = new NodePropertiesDialog(node) { Owner = this };
            if (dialog.ShowDialog() != true)
                return;

            bool changed = !string.Equals(node.Text, dialog.NodeText, StringComparison.Ordinal) ||
                           !string.Equals(node.Note, dialog.NodeNote, StringComparison.Ordinal) ||
                           !string.Equals(node.Link, dialog.NodeLink, StringComparison.Ordinal) ||
                           !string.Equals(node.Color, dialog.NodeColor, StringComparison.OrdinalIgnoreCase) ||
                           !string.Equals(node.Icon, dialog.NodeIcon, StringComparison.Ordinal);
            if (!changed)
                return;

            RecordUndoState();
            node.Text = dialog.NodeText;
            node.Note = dialog.NodeNote;
            node.Link = dialog.NodeLink;
            node.Color = dialog.NodeColor;
            node.Icon = dialog.NodeIcon;
            _isDirty = true;
            RenderMindMap();
        }

        private void BatchFormat_Click(object sender, RoutedEventArgs e)
        {
            var nodes = GetSelectedNodes();
            if (nodes.Count == 0)
                return;

            var dialog = new BatchFormatDialog(nodes) { Owner = this };
            if (dialog.ShowDialog() != true)
                return;

            bool changed = nodes.Any(node =>
                (dialog.ApplyColor && !string.Equals(node.Color ?? "", dialog.NodeColor, StringComparison.OrdinalIgnoreCase)) ||
                (dialog.ApplyIcon && !string.Equals(node.Icon ?? "", dialog.NodeIcon, StringComparison.Ordinal)));
            if (!changed)
                return;

            RecordUndoState();
            foreach (var node in nodes)
            {
                if (dialog.ApplyColor)
                    node.Color = dialog.NodeColor;
                if (dialog.ApplyIcon)
                    node.Icon = dialog.NodeIcon;
            }
            _isDirty = true;
            RenderMindMap();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedNodes(-1);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedNodes(1);
        }

        private bool CanMoveSelectedNodes(int direction)
        {
            if (_document?.Root == null || _selectedNodeIds.Count == 0)
                return false;

            foreach (var parent in GetSelectedNodes()
                .Select(node => _document.Root.FindParentOf(node.Id))
                .Where(parent => parent != null)
                .Distinct())
            {
                if (direction < 0)
                {
                    for (int i = 1; i < parent.Children.Count; i++)
                    {
                        if (_selectedNodeIds.Contains(parent.Children[i].Id) &&
                            !_selectedNodeIds.Contains(parent.Children[i - 1].Id))
                            return true;
                    }
                }
                else
                {
                    for (int i = parent.Children.Count - 2; i >= 0; i--)
                    {
                        if (_selectedNodeIds.Contains(parent.Children[i].Id) &&
                            !_selectedNodeIds.Contains(parent.Children[i + 1].Id))
                            return true;
                    }
                }
            }
            return false;
        }

        private void MoveSelectedNodes(int direction)
        {
            if (!CanMoveSelectedNodes(direction))
                return;

            var parents = GetSelectedNodes()
                .Select(node => _document.Root.FindParentOf(node.Id))
                .Where(parent => parent != null)
                .Distinct()
                .ToList();

            RecordUndoState();
            foreach (var parent in parents)
            {
                if (direction < 0)
                {
                    for (int i = 1; i < parent.Children.Count; i++)
                    {
                        if (!_selectedNodeIds.Contains(parent.Children[i].Id) ||
                            _selectedNodeIds.Contains(parent.Children[i - 1].Id))
                            continue;

                        var moved = parent.Children[i];
                        parent.Children[i] = parent.Children[i - 1];
                        parent.Children[i - 1] = moved;
                        moved.ManualOffsetX = 0;
                        moved.ManualOffsetY = 0;
                    }
                }
                else
                {
                    for (int i = parent.Children.Count - 2; i >= 0; i--)
                    {
                        if (!_selectedNodeIds.Contains(parent.Children[i].Id) ||
                            _selectedNodeIds.Contains(parent.Children[i + 1].Id))
                            continue;

                        var moved = parent.Children[i];
                        parent.Children[i] = parent.Children[i + 1];
                        parent.Children[i + 1] = moved;
                        moved.ManualOffsetX = 0;
                        moved.ManualOffsetY = 0;
                    }
                }
            }

            _isDirty = true;
            RenderMindMap();
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedLink();
        }

        private void OpenSelectedLink()
        {
            if (_selectedNodeIds.Count != 1)
                return;
            string link = GetSelectedNodeData()?.Link?.Trim();
            if (string.IsNullOrEmpty(link))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(EditorStrings.InvalidLink + ": " + ex.Message,
                    EditorStrings.UnsavedTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedNode();
        }

        private void CopySelectedNode()
        {
            if (_selectedNodeIds.Count != 1)
                return;
            var node = GetSelectedNodeData();
            if (node == null)
                return;

            try
            {
                var clipboardDocument = new MindMapDocument
                {
                    Title = node.Text,
                    Root = node.Clone(),
                    Settings = _document.Settings?.Clone() ?? new MindMapSettings()
                };
                var data = new DataObject();
                data.SetData(NodeClipboardFormat, MindMapSerializer.Serialize(clipboardDocument));
                data.SetText(BuildOutlineText(node));
                Clipboard.SetDataObject(data, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(EditorStrings.ClipboardFailed + ": " + ex.Message,
                    EditorStrings.UnsavedTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            PasteNode();
        }

        private void PasteNode()
        {
            try
            {
                var nodes = new List<MindMapNode>();
                if (Clipboard.ContainsData(NodeClipboardFormat))
                {
                    string json = Clipboard.GetData(NodeClipboardFormat) as string;
                    var clipboardDocument = MindMapSerializer.Deserialize(json);
                    if (clipboardDocument?.Root != null)
                    {
                        var pastedRoot = clipboardDocument.Root.Clone();
                        RegenerateNodeIds(pastedRoot);
                        nodes.Add(pastedRoot);
                    }
                }
                else if (Clipboard.ContainsText())
                {
                    nodes.AddRange(ParseOutlineText(Clipboard.GetText()));
                }

                if (nodes.Count == 0)
                    return;

                var parent = GetSelectedNodeData() ?? _document.Root;
                RecordUndoState();
                parent.Collapsed = false;
                foreach (var node in nodes)
                    parent.Children.Add(node);
                _isDirty = true;
                RenderMindMap();
                SelectNodeById(nodes[0].Id);
                ScrollNodeIntoView(nodes[0].Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(EditorStrings.ClipboardFailed + ": " + ex.Message,
                    EditorStrings.UnsavedTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void RegenerateNodeIds(MindMapNode node)
        {
            node.Id = Guid.NewGuid().ToString("N");
            node.SourceObjectId = "";
            node.ManualOffsetX = 0;
            node.ManualOffsetY = 0;
            foreach (var child in node.Children)
                RegenerateNodeIds(child);
        }

        private static string BuildOutlineText(MindMapNode node)
        {
            var lines = new List<string>();
            AppendOutlineText(node, 0, lines);
            return string.Join(Environment.NewLine, lines);
        }

        private static void AppendOutlineText(MindMapNode node, int depth, List<string> lines)
        {
            lines.Add(new string('\t', depth) + (node.Text ?? ""));
            foreach (var child in node.Children)
                AppendOutlineText(child, depth + 1, lines);
        }

        private static List<MindMapNode> ParseOutlineText(string text)
        {
            var roots = new List<MindMapNode>();
            var stack = new List<MindMapNode>();
            string[] lines = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            foreach (string rawLine in lines)
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                int indentation = 0;
                int position = 0;
                while (position < rawLine.Length && (rawLine[position] == ' ' || rawLine[position] == '\t'))
                {
                    indentation += rawLine[position] == '\t' ? 2 : 1;
                    position++;
                }

                int depth = indentation / 2;
                string nodeText = rawLine.Substring(position).Trim();
                if (nodeText.StartsWith("- ") || nodeText.StartsWith("* ") || nodeText.StartsWith("• "))
                    nodeText = nodeText.Substring(2).Trim();
                if (nodeText.Length == 0)
                    continue;

                depth = Math.Min(depth, stack.Count);
                while (stack.Count > depth)
                    stack.RemoveAt(stack.Count - 1);

                var node = new MindMapNode { Text = nodeText };
                if (stack.Count == 0)
                    roots.Add(node);
                else
                    stack[stack.Count - 1].Children.Add(node);
                stack.Add(node);
            }

            return roots;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateStatusBar();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            FindMatch((Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? -1 : 1);
            e.Handled = true;
        }

        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            FindMatch(-1);
        }

        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            FindMatch(1);
        }

        private void FindMatch(int direction)
        {
            string query = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(query))
            {
                SearchBox.Focus();
                return;
            }

            var matches = EnumerateNodes(_document.Root)
                .Where(node => NodeMatches(node, query))
                .ToList();
            if (matches.Count == 0)
            {
                StatusText.Text = EditorStrings.SearchNoResults;
                return;
            }

            string selectedId = GetSelectedNodeData()?.Id;
            int current = matches.FindIndex(node => node.Id == selectedId);
            int next = current < 0
                ? (direction > 0 ? 0 : matches.Count - 1)
                : (current + direction + matches.Count) % matches.Count;
            var match = matches[next];
            EnsureNodeVisible(match);
            SelectNodeById(match.Id);
            ScrollNodeIntoView(match.Id);
            StatusText.Text = EditorStrings.SearchResultFormat(next + 1, matches.Count);
        }

        private static IEnumerable<MindMapNode> EnumerateNodes(MindMapNode node)
        {
            if (node == null)
                yield break;

            yield return node;
            foreach (var child in node.Children)
            foreach (var descendant in EnumerateNodes(child))
                yield return descendant;
        }

        private static bool NodeMatches(MindMapNode node, string query)
        {
            return ContainsIgnoreCase(node.Text, query) ||
                   ContainsIgnoreCase(node.Note, query) ||
                   ContainsIgnoreCase(node.Link, query) ||
                   ContainsIgnoreCase(node.Icon, query);
        }

        private static bool ContainsIgnoreCase(string value, string query)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void EnsureNodeVisible(MindMapNode node)
        {
            var collapsedAncestors = new List<MindMapNode>();
            var current = _document.Root.FindParentOf(node.Id);
            while (current != null)
            {
                if (current.Collapsed)
                    collapsedAncestors.Add(current);
                current = _document.Root.FindParentOf(current.Id);
            }

            if (collapsedAncestors.Count == 0)
                return;

            RecordUndoState();
            foreach (var ancestor in collapsedAncestors)
                ancestor.Collapsed = false;
            _isDirty = true;
            RenderMindMap();
        }

        private void NavigateSelection(Key direction)
        {
            if (_selectedNode == null)
            {
                SelectNodeById(_document.Root.Id);
                ScrollNodeIntoView(_document.Root.Id);
                return;
            }

            var current = _currentLayouts.FirstOrDefault(layout => layout.NodeId == _selectedNode.Node.Id);
            if (current == null)
                return;

            double currentX = current.X + current.Width / 2;
            double currentY = current.Y + current.Height / 2;
            NodeLayout best = null;
            double bestScore = double.MaxValue;

            foreach (var candidate in _currentLayouts)
            {
                if (candidate.NodeId == current.NodeId)
                    continue;

                double dx = candidate.X + candidate.Width / 2 - currentX;
                double dy = candidate.Y + candidate.Height / 2 - currentY;
                bool isInDirection = direction == Key.Left ? dx < -1 :
                                     direction == Key.Right ? dx > 1 :
                                     direction == Key.Up ? dy < -1 : dy > 1;
                if (!isInDirection)
                    continue;

                double primary = direction == Key.Left || direction == Key.Right ? Math.Abs(dx) : Math.Abs(dy);
                double secondary = direction == Key.Left || direction == Key.Right ? Math.Abs(dy) : Math.Abs(dx);
                double score = primary + secondary * 2.0;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            if (best != null)
            {
                SelectNodeById(best.NodeId);
                ScrollNodeIntoView(best.NodeId);
            }
        }

        private void ScrollNodeIntoView(string nodeId)
        {
            var ctrl = MainCanvas.Children
                .OfType<NodeControl>()
                .FirstOrDefault(item => item.Node.Id == nodeId);
            if (ctrl == null)
                return;

            double centerX = (Canvas.GetLeft(ctrl) + ctrl.Width / 2) * _zoomLevel;
            double centerY = (Canvas.GetTop(ctrl) + ctrl.Height / 2) * _zoomLevel;
            CanvasScrollViewer.ScrollToHorizontalOffset(Math.Max(0, centerX - CanvasScrollViewer.ViewportWidth / 2));
            CanvasScrollViewer.ScrollToVerticalOffset(Math.Max(0, centerY - CanvasScrollViewer.ViewportHeight / 2));
        }

        private void AddSibling_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null || _document.Root == node)
            {
                RecordUndoState();
                var newRootChild = new MindMapNode { Text = EditorStrings.NewNodeText };
                _document.Root.Children.Add(newRootChild);
                _isDirty = true;
                RenderMindMap();
                SelectNodeById(newRootChild.Id, true);
                return;
            }

            var parent = GetParentNode(_document.Root, node.Id);
            if (parent == null) return;

            RecordUndoState();
            var newNode = new MindMapNode { Text = EditorStrings.NewNodeText };
            int idx = parent.Children.IndexOf(node);
            parent.Children.Insert(idx + 1, newNode);
            _isDirty = true;
            RenderMindMap();
            SelectNodeById(newNode.Id, true);
        }

        private void AddChild_Click(object sender, RoutedEventArgs e)
        {
            var node = GetSelectedNodeData();
            if (node == null) node = _document.Root;

            RecordUndoState();
            var newNode = new MindMapNode { Text = EditorStrings.NewNodeText };
            node.Children.Add(newNode);
            _isDirty = true;
            RenderMindMap();
            SelectNodeById(newNode.Id, true);
        }

        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            var selectedNodes = GetSelectedNodes()
                .Where(node => node != _document.Root)
                .ToList();
            if (selectedNodes.Count == 0)
                return;

            var topLevelNodes = selectedNodes.Where(node =>
            {
                var ancestor = _document.Root.FindParentOf(node.Id);
                while (ancestor != null)
                {
                    if (ancestor != _document.Root && _selectedNodeIds.Contains(ancestor.Id))
                        return false;
                    ancestor = _document.Root.FindParentOf(ancestor.Id);
                }
                return true;
            }).ToList();

            string parentId = _document.Root.FindParentOf(
                topLevelNodes.FirstOrDefault(node => node.Id == _primarySelectedNodeId)?.Id ??
                topLevelNodes[0].Id)?.Id ?? _document.Root.Id;

            RecordUndoState();
            foreach (var node in topLevelNodes)
            {
                var parent = _document.Root.FindParentOf(node.Id);
                parent?.Children.Remove(node);
            }
            _selectedNode = null;
            _selectedNodeIds.Clear();
            _primarySelectedNodeId = null;
            _isDirty = true;
            RenderMindMap();
            SelectNodeById(parentId);
        }

        private void CollapseToggle_Click(object sender, RoutedEventArgs e)
        {
            var nodes = GetSelectedNodes().Where(node => node.Children.Count > 0).ToList();
            if (nodes.Count == 0) return;
            bool collapse = nodes.Any(node => !node.Collapsed);
            RecordUndoState();
            foreach (var node in nodes)
                node.Collapsed = collapse;
            _isDirty = true;
            RenderMindMap();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void Undo()
        {
            if (_undoHistory.Count == 0)
                return;

            _redoHistory.Add(CaptureCurrentState());
            var state = _undoHistory[_undoHistory.Count - 1];
            _undoHistory.RemoveAt(_undoHistory.Count - 1);
            RestoreState(state);
        }

        private void Redo()
        {
            if (_redoHistory.Count == 0)
                return;

            _undoHistory.Add(CaptureCurrentState());
            var state = _redoHistory[_redoHistory.Count - 1];
            _redoHistory.RemoveAt(_redoHistory.Count - 1);
            RestoreState(state);
        }

        private void RecordUndoState()
        {
            if (_isRestoringHistory)
                return;

            _undoHistory.Add(CaptureCurrentState());
            if (_undoHistory.Count > MaxHistoryEntries)
                _undoHistory.RemoveAt(0);
            _redoHistory.Clear();
            UpdateHistoryButtons();
        }

        private EditorState CaptureCurrentState()
        {
            if (_document.Settings == null)
                _document.Settings = new MindMapSettings();

            var snapshot = _document.Clone();
            CaptureEditorSettings(snapshot.Settings);
            return new EditorState
            {
                Document = snapshot,
                SelectedNodeIds = _selectedNodeIds.ToList(),
                PrimarySelectedNodeId = _primarySelectedNodeId,
                IsDirty = _isDirty
            };
        }

        private void RestoreState(EditorState state)
        {
            _isRestoringHistory = true;
            try
            {
                _document = state.Document.Clone();
                _isDirty = state.IsDirty;
                _selectedNode = null;
                _selectedNodeIds.Clear();
                foreach (string nodeId in state.SelectedNodeIds ?? new List<string>())
                    _selectedNodeIds.Add(nodeId);
                _primarySelectedNodeId = state.PrimarySelectedNodeId;
                ApplySettings();
                RenderMindMap();
            }
            finally
            {
                _isRestoringHistory = false;
                UpdateHistoryButtons();
            }
        }

        private void UpdateHistoryButtons()
        {
            if (BtnUndo != null)
                BtnUndo.IsEnabled = _undoHistory.Count > 0;
            if (BtnRedo != null)
                BtnRedo.IsEnabled = _redoHistory.Count > 0;
        }

        private void CaptureEditorSettings(MindMapSettings settings)
        {
            settings.Layout = _engine.Options.Layout;
            settings.Theme = _engine.Options.Theme;
            settings.NodeShape = _engine.Options.NodeShape;
            settings.ConnectionStyle = _engine.Options.ConnectionStyle;
            settings.EndpointStyle = _engine.Options.EndpointStyle;
            settings.NodeSizeMode = _engine.Options.NodeSizeMode;
            settings.CanvasZoom = _zoomLevel;
            settings.NodeWidth = _engine.Options.NodeWidth;
            settings.NodeHeight = _engine.Options.NodeHeight;
            settings.AutoNodeMaxWidth = _engine.Options.AutoNodeMaxWidth;
            settings.HorizontalGap = _engine.Options.HorizontalGap;
            settings.VerticalGap = _engine.Options.VerticalGap;
            settings.LevelGap = _engine.Options.LevelGap;
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
            if (_isRestoringHistory) return;
            if (_engine != null && LayoutCombo?.SelectedItem is ComboBoxItem item)
            {
                if (IsLoaded) RecordUndoState();
                _engine.Options.Layout = item.Tag?.ToString() ?? "RightTree";
                if (IsLoaded) _isDirty = true;
                RenderMindMap();
            }
        }

        private void Theme_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isRestoringHistory) return;
            if (_engine != null && ThemeCombo?.SelectedItem is ComboBoxItem item)
            {
                if (IsLoaded) RecordUndoState();
                _engine.Options.Theme = item.Tag?.ToString() ?? "Default";
            }
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private void Shape_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isRestoringHistory) return;
            if (_engine != null && ShapeCombo?.SelectedItem is ComboBoxItem item)
            {
                if (IsLoaded) RecordUndoState();
                _engine.Options.NodeShape = item.Tag?.ToString() ?? "Rounded";
            }
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private void Connection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isRestoringHistory) return;
            if (_engine != null && ConnectionCombo?.SelectedItem is ComboBoxItem item)
            {
                if (IsLoaded) RecordUndoState();
                _engine.Options.ConnectionStyle = item.Tag?.ToString() ?? "Curved";
            }
            if (EndpointCombo != null)
                EndpointCombo.IsEnabled = _engine?.Options.ConnectionStyle != "ClassicMindMap";
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private void NodeSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isRestoringHistory) return;
            if (_engine != null && NodeSizeCombo?.SelectedItem is ComboBoxItem item)
            {
                if (IsLoaded) RecordUndoState();
                _engine.Options.NodeSizeMode = item.Tag?.ToString() ?? "Fixed";
            }
            if (IsLoaded) _isDirty = true;
            RenderMindMap();
        }

        private NodeControl SelectNodeById(string nodeId, bool beginEdit = false)
        {
            if (string.IsNullOrEmpty(nodeId))
                return null;

            var ctrl = MainCanvas.Children
                .OfType<NodeControl>()
                .FirstOrDefault(item => item.Node.Id == nodeId);
            SelectNode(ctrl);
            if (beginEdit && ctrl != null)
                ctrl.EnterEditMode();
            return ctrl;
        }

        private void Endpoint_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isRestoringHistory) return;
            if (_engine != null && EndpointCombo?.SelectedItem is ComboBoxItem item)
            {
                if (IsLoaded) RecordUndoState();
                _engine.Options.EndpointStyle = item.Tag?.ToString() ?? "None";
            }
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
            settings.NodeSizeMode = ((ComboBoxItem)NodeSizeCombo.SelectedItem)?.Tag?.ToString() ?? "Fixed";
            settings.ConnectionStyle = ((ComboBoxItem)ConnectionCombo.SelectedItem)?.Tag?.ToString() ?? "Curved";
            settings.EndpointStyle = ((ComboBoxItem)EndpointCombo.SelectedItem)?.Tag?.ToString() ?? "None";
            settings.CanvasZoom = _zoomLevel;
            settings.AutoNodeMaxWidth = _engine.Options.AutoNodeMaxWidth;
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
