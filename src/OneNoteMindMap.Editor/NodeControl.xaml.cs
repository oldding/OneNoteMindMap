using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OneNoteMindMap.Core.Layout;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Core.Rendering;

namespace OneNoteMindMap.Editor
{
    public partial class NodeControl : UserControl
    {
        public MindMapNode Node { get; }
        public NodeLayout Layout { get; }
        public LayoutOptions Options { get; }

        public event EventHandler RequestEdit;
        public event EventHandler ContentChanged;
        public bool IsEditing => EditBox.Visibility == Visibility.Visible;

        private static readonly SolidColorBrush SelectedBorderBrush =
            new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
        private static readonly SolidColorBrush DropTargetBorderBrush =
            new SolidColorBrush(Color.FromRgb(0x10, 0x8A, 0x55));
        private static readonly SolidColorBrush DefaultBorderBrush =
            new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
        private static readonly SolidColorBrush RootTextBrush =
            new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _classicBranchBrush;
        private readonly bool _isClassicMindMap;
        private bool _isSelected;
        private bool _isDropTarget;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateBorderState();
            }
        }

        public bool IsDropTarget
        {
            get => _isDropTarget;
            set
            {
                _isDropTarget = value;
                UpdateBorderState();
            }
        }

        public NodeControl(
            MindMapNode node,
            NodeLayout layout,
            LayoutOptions options,
            string branchColor = "#555555")
        {
            InitializeComponent();
            Node = node;
            Layout = layout;
            Options = options;
            _isClassicMindMap = options.ConnectionStyle == "ClassicMindMap";
            _classicBranchBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(branchColor));

            TextBlock.Text = ToSingleLine(node.Text);
            ApplyStyle();

            if (_isClassicMindMap)
            {
                TextBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
                TextBlock.TextAlignment = layout.Depth == 0
                    ? TextAlignment.Center
                    : TextAlignment.Left;
                TextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                TextBlock.Margin = new Thickness(4, 0, 4, 5);
                TextBlock.FontWeight = layout.Depth <= 1
                    ? FontWeights.SemiBold
                    : FontWeights.Normal;
                TextBlock.FontSize = layout.Depth == 0 ? 18 : 13;
            }
            else if (layout.Depth == 0)
            {
                TextBlock.Foreground = RootTextBrush;
                NodeBorder.Height = 50;
                TextBlock.FontSize = 15;
            }

            this.Width = options.NodeWidth;
            this.Height = _isClassicMindMap
                ? options.NodeHeight
                : layout.Depth == 0 ? 50 : options.NodeHeight;
        }

        private void ApplyStyle()
        {
            var theme = ThemeManager.GetTheme(Options.Theme);

            if (_isClassicMindMap)
            {
                NodeBorder.Background = Brushes.Transparent;
                NodeBorder.BorderBrush = Layout.Depth == 0
                    ? Brushes.Transparent
                    : _classicBranchBrush;
                NodeBorder.BorderThickness = Layout.Depth == 0
                    ? new Thickness(0)
                    : new Thickness(0, 0, 0, 2);
                NodeBorder.CornerRadius = new CornerRadius(0);
                NodeBorder.Padding = new Thickness(4, 2, 4, 0);
                TextBlock.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(theme.NodeText));
                return;
            }

            string fill = Layout.Depth == 0
                ? theme.RootFill
                : Layout.Depth == 1 ? theme.Level1Fill
                : Layout.Depth == 2 ? theme.Level2Fill
                : theme.NodeFill;

            NodeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fill));
            NodeBorder.BorderBrush = DefaultBorderBrush;
            TextBlock.Foreground = Layout.Depth == 0
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.RootText))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString(theme.NodeText));

            switch (Options.NodeShape)
            {
                case "Rectangle":
                    NodeBorder.CornerRadius = new CornerRadius(0);
                    break;
                case "Pill":
                    NodeBorder.CornerRadius = new CornerRadius(22);
                    break;
                default:
                    NodeBorder.CornerRadius = new CornerRadius(6);
                    break;
            }
        }

        private void UpdateBorderState()
        {
            if (_isDropTarget)
            {
                NodeBorder.BorderBrush = DropTargetBorderBrush;
                NodeBorder.BorderThickness = new Thickness(3);
            }
            else if (_isSelected)
            {
                NodeBorder.BorderBrush = SelectedBorderBrush;
                NodeBorder.BorderThickness = new Thickness(2);
            }
            else
            {
                NodeBorder.BorderBrush = _isClassicMindMap && Layout.Depth > 0
                    ? _classicBranchBrush
                    : DefaultBorderBrush;
                NodeBorder.BorderThickness = _isClassicMindMap && Layout.Depth > 0
                    ? new Thickness(0, 0, 0, 2)
                    : new Thickness(1);
            }
        }

        public void EnterEditMode()
        {
            EditBox.Text = Node.Text ?? "";
            TextBlock.Visibility = Visibility.Collapsed;
            EditBox.Visibility = Visibility.Visible;
            EditBox.Focus();
            EditBox.SelectAll();
        }

        private void ExitEditMode()
        {
            string newText = EditBox.Text?.Trim();
            bool changed = !string.IsNullOrEmpty(newText) && newText != Node.Text;
            if (!string.IsNullOrEmpty(newText))
            {
                Node.Text = newText;
                TextBlock.Text = ToSingleLine(newText);
            }
            TextBlock.Visibility = Visibility.Visible;
            EditBox.Visibility = Visibility.Collapsed;
            if (changed)
                ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEditing)
                return;

            Focus();
            if (e.ClickCount == 2)
            {
                RequestEdit?.Invoke(this, EventArgs.Empty);
                EnterEditMode();
                e.Handled = true;
            }
        }

        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ExitEditMode();
        }

        private void EditBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExitEditMode();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                TextBlock.Visibility = Visibility.Visible;
                EditBox.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private static string ToSingleLine(string text)
        {
            return (text ?? "")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }
}
