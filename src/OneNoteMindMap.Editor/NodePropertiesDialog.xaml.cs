using System;
using System.Windows;
using System.Windows.Media;
using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Editor
{
    public partial class NodePropertiesDialog : Window
    {
        public string NodeText => NodeTextBox.Text?.Trim() ?? "";
        public string NodeNote => NoteTextBox.Text?.Trim() ?? "";
        public string NodeLink => LinkTextBox.Text?.Trim() ?? "";
        public string NodeColor => ColorTextBox.Text?.Trim() ?? "";
        public string NodeIcon => IconTextBox.Text?.Trim() ?? "";

        public NodePropertiesDialog(MindMapNode node)
        {
            InitializeComponent();
            Title = EditorStrings.NodePropertiesTitle;
            LblText.Content = EditorStrings.NodeTextLabel;
            LblIcon.Content = EditorStrings.NodeIconLabel;
            LblColor.Content = EditorStrings.NodeColorLabel;
            LblNote.Content = EditorStrings.NodeNoteLabel;
            LblLink.Content = EditorStrings.NodeLinkLabel;
            BtnClearColor.Content = EditorStrings.Clear;
            BtnOk.Content = EditorStrings.Ok;
            BtnCancel.Content = EditorStrings.Cancel;
            HintText.Text = EditorStrings.NodePropertiesHint;

            NodeTextBox.Text = node?.Text ?? "";
            NoteTextBox.Text = node?.Note ?? "";
            LinkTextBox.Text = node?.Link ?? "";
            ColorTextBox.Text = node?.Color ?? "";
            IconTextBox.Text = node?.Icon ?? "";
            NodeTextBox.SelectAll();
            NodeTextBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NodeText))
            {
                MessageBox.Show(EditorStrings.NodeTextRequired, EditorStrings.NodePropertiesTitle,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NodeTextBox.Focus();
                return;
            }

            if (!TryGetColor(NodeColor, out _))
            {
                MessageBox.Show(EditorStrings.InvalidNodeColor, EditorStrings.NodePropertiesTitle,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ColorTextBox.Focus();
                return;
            }

            DialogResult = true;
        }

        private void ClearColor_Click(object sender, RoutedEventArgs e)
        {
            ColorTextBox.Clear();
        }

        private void ColorTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ColorPreview == null)
                return;

            if (TryGetColor(ColorTextBox.Text?.Trim(), out var color))
                ColorPreview.Background = new SolidColorBrush(color);
            else
                ColorPreview.Background = Brushes.Transparent;
        }

        private static bool TryGetColor(string value, out Color color)
        {
            color = Colors.Transparent;
            if (string.IsNullOrWhiteSpace(value))
                return true;

            try
            {
                var converted = ColorConverter.ConvertFromString(value);
                if (converted is Color parsed)
                {
                    color = parsed;
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
