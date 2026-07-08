using System;
using System.Runtime.InteropServices;

namespace OneNoteMindMap.UI
{
    public static class ForceForegroundWindow
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern int AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        public static void ForceForeground(IntPtr targetHwnd)
        {
            if (targetHwnd == IntPtr.Zero) return;

            IntPtr foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            IntPtr currentThread = GetCurrentThreadId();

            if (foregroundThread != currentThread)
            {
                AttachThreadInput(currentThread, foregroundThread, 1);
                SetForegroundWindow(targetHwnd);
                AttachThreadInput(currentThread, foregroundThread, 0);
            }
            else
            {
                SetForegroundWindow(targetHwnd);
            }
        }
    }
}
