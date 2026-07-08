using System;

namespace OneNoteMindMap.UI
{
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle { get; }

        public WindowWrapper(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
