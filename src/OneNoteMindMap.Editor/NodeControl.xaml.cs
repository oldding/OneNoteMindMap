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

        private static readonly SolidColorBrush SelectedBorderBrush =
            new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
        private static readonly SolidColorBrush DefaultBorderBrush =
            new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
        private static readonly SolidColorBrush RootTextBrush =
            new SolidColorBrush(Colors.White);

        public bool IsSelected
        {
            get => NodeBorder.BorderBrush == SelectedBorderBrush;
            set => NodeBorder.BorderBrush = value ? SelectedBorderBrush : DefaultBorderBrush;
        }

        public NodeControl(MindMapNode node, NodeLayout layout, LayoutOptions options)
        {
            InitializeComponent();
            Node = node;
            Layout = layout;
            Options = options;

            TextBlock.Text = node.Text ?? "";
            ApplyStyle();

            if (layout.Depth == 0)
            {
                TextBlock.Foreground = RootTextBrush;
                NodeBorder.Height = 50;
                TextBlock.FontSize = 15;
            }

            this.Width = options.NodeWidth;
            this.Height = layout.Depth == 0 ? 50 : options.NodeHeight;
        }

        private void ApplyStyle()
        {
            var theme = ThemeManager.GetTheme(Options.Theme);
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

        public void EnterEditMode()
        {
            EditBox.Text = TextBlock.Text;
            TextBlock.Visibility = Visibility.Collapsed;
            EditBox.Visibility = Visibility.Visible;
            EditBox.Focus();
            EditBox.SelectAll();
        }

        private void ExitEditMode()
        {
            string newText = EditBox.Text?.Trim();
            if (!string.IsNullOrEmpty(newText))
            {
                Node.Text = newText;
                TextBlock.Text = newText;
            }
            TextBlock.Visibility = Visibility.Visible;
            EditBox.Visibility = Visibility.Collapsed;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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
    }
}
