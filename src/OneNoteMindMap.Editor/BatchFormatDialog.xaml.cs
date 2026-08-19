using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Editor
{
    public partial class BatchFormatDialog : Window
    {
        public bool ApplyColor => ApplyColorCheckBox.IsChecked == true;
        public bool ApplyIcon => ApplyIconCheckBox.IsChecked == true;
        public string NodeColor => ColorTextBox.Text?.Trim() ?? "";
        public string NodeIcon => IconTextBox.Text?.Trim() ?? "";

        public BatchFormatDialog(IReadOnlyList<MindMapNode> nodes)
        {
            InitializeComponent();
            Title = EditorStrings.BatchFormatTitle;
            SummaryText.Text = EditorStrings.BatchFormatSummary(nodes?.Count ?? 0);
            ApplyColorCheckBox.Content = EditorStrings.ApplyColor;
            ApplyIconCheckBox.Content = EditorStrings.ApplyIcon;
            BtnClearFormatting.Content = EditorStrings.ClearFormatting;
            BtnOk.Content = EditorStrings.Ok;
            BtnCancel.Content = EditorStrings.Cancel;
            HintText.Text = EditorStrings.BatchFormatHint;

            if (nodes != null && nodes.Count > 0)
            {
                string firstColor = nodes[0].Color ?? "";
                string firstIcon = nodes[0].Icon ?? "";
                ColorTextBox.Text = nodes.All(node => string.Equals(node.Color ?? "", firstColor, StringComparison.OrdinalIgnoreCase))
                    ? firstColor
                    : "";
                IconTextBox.Text = nodes.All(node => string.Equals(node.Icon ?? "", firstIcon, StringComparison.Ordinal))
                    ? firstIcon
                    : "";
            }

            UpdateInputState();
        }

        private void ApplyOption_Changed(object sender, RoutedEventArgs e)
        {
            UpdateInputState();
        }

        private void UpdateInputState()
        {
            if (ColorTextBox == null || IconTextBox == null || ColorPreview == null)
                return;
            ColorTextBox.IsEnabled = ApplyColor;
            IconTextBox.IsEnabled = ApplyIcon;
            ColorPreview.Opacity = ApplyColor ? 1.0 : 0.35;
        }

        private void ClearFormatting_Click(object sender, RoutedEventArgs e)
        {
            ApplyColorCheckBox.IsChecked = true;
            ApplyIconCheckBox.IsChecked = true;
            ColorTextBox.Clear();
            IconTextBox.Clear();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!ApplyColor && !ApplyIcon)
            {
                MessageBox.Show(EditorStrings.BatchFormatSelectOption, EditorStrings.BatchFormatTitle,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ApplyColor && !TryGetColor(NodeColor, out _))
            {
                MessageBox.Show(EditorStrings.InvalidNodeColor, EditorStrings.BatchFormatTitle,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ColorTextBox.Focus();
                return;
            }

            DialogResult = true;
        }

        private void ColorTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ColorPreview == null)
                return;
            ColorPreview.Background = TryGetColor(ColorTextBox.Text?.Trim(), out var color)
                ? new SolidColorBrush(color)
                : Brushes.Transparent;
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
