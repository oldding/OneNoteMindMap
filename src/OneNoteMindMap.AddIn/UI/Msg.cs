using System;
using System.Windows;

namespace OneNoteMindMap.UI
{
    public static class Msg
    {
        public static void Info(string message, string title = "OneNote 脑图")
        {
            UiThread.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public static void Warn(string message, string title = "OneNote 脑图")
        {
            UiThread.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        public static void Error(string message, string title = "OneNote 脑图")
        {
            UiThread.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public static bool Confirm(string message, string title = "OneNote 脑图")
        {
            return UiThread.Invoke(() =>
            {
                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }
    }
}
