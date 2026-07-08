using System;

namespace OneNoteMindMap.Logging
{
    public static class Logger
    {
        private static string _logPath;

        public static void Initialize()
        {
            try
            {
                var dir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OneNoteMindMap");
                System.IO.Directory.CreateDirectory(dir);
                _logPath = System.IO.Path.Combine(dir, "log.txt");

                if (System.IO.File.Exists(_logPath) && new System.IO.FileInfo(_logPath).Length > 1024 * 1024)
                {
                    System.IO.File.WriteAllText(_logPath, "");
                }
            }
            catch { }
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Warn(string message) => Write("WARN", message);
        public static void Error(string message, Exception ex = null)
        {
            Write("ERROR", message + (ex != null ? " | " + ex.ToString() : ""));
        }

        private static void Write(string level, string message)
        {
            if (_logPath == null) return;
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                System.IO.File.AppendAllText(_logPath, line + Environment.NewLine);
            }
            catch { }
        }
    }
}
