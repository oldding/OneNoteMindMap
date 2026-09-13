using System;
using System.Diagnostics;
using System.Threading;
using Extensibility;
using OneNoteMindMap.AddIn;
using OneNoteMindMap.UI;

internal static class LifecycleTests
{
    private static void Check(bool ok, string name)
    {
        if (!ok) throw new Exception(name);
        Console.WriteLine("PASS " + name);
    }

    [STAThread]
    private static int Main()
    {
        try
        {
            for (int i = 0; i < 5; i++)
            {
                var connect = new Connect();
                Array custom = new object[0];
                var clock = Stopwatch.StartNew();
                connect.OnConnection(new object(), ext_ConnectMode.ext_cm_Startup, new object(), ref custom);
                Check(clock.ElapsedMilliseconds < 2000, "nonblocking connection " + i);
                var ready = UiThread.InvokeAsync(() => Thread.CurrentThread.GetApartmentState());
                Check(ready.Wait(5000) && ready.Result == ApartmentState.STA, "dispatcher STA");
                connect.OnBeginShutdown(ref custom);
                connect.OnDisconnection(ext_DisconnectMode.ext_dm_HostShutdown, ref custom);
                Check(UiThread.LastShutdown.Wait(5000), "dispatcher exit " + i);
                Check(Connect.Instance == null && connect.OneNoteApp == null, "detached COM owner");
                Check(UiThread.InvokeAsync(() => 1).IsCanceled, "no late restart");
            }
            UiThread.EnsureStarted();
            using (var entered = new ManualResetEventSlim())
            using (var unblock = new ManualResetEventSlim())
            {
                int released = 0, stale = 0;
                var running = UiThread.InvokeAsync(() => { entered.Set(); unblock.Wait(5000); return 1; });
                Check(entered.Wait(5000), "busy operation entered");
                var queued = UiThread.InvokeAsync(() => ++stale);
                var clock = Stopwatch.StartNew();
                var stopped = UiThread.Shutdown(() => released++);
                Check(clock.ElapsedMilliseconds < 1000 && released == 0, "shutdown nonblocking, release deferred");
                unblock.Set();
                Check(stopped.Wait(5000) && released == 1 && stale == 0, "busy return before release, queued work cancelled");
            }
            // Modal WPF editor is a nested dispatcher frame; cleanup must wait until it unwinds.
            UiThread.EnsureStarted();
            using (var shown = new ManualResetEventSlim())
            {
                bool unwound = false, releasedAfterWindow = false;
                var modal = UiThread.InvokeAsync(() =>
                {
                    var window = new System.Windows.Window { Width = 150, Height = 80, ShowInTaskbar = false };
                    var untrack = UiThread.TrackWindow(window.Close);
                    window.Loaded += (s, e) => shown.Set();
                    try { window.ShowDialog(); }
                    finally { untrack(); unwound = true; }
                    return true;
                });
                Check(shown.Wait(5000), "modal window shown");
                Check(UiThread.Shutdown(() => releasedAfterWindow = unwound).Wait(5000), "shutdown closes modal window");
                Check(releasedAfterWindow, "release after nested frame unwinds");
            }
            for (int i = 0; i < 10; i++)
            {
                UiThread.EnsureStarted();
                Check(UiThread.Shutdown().Wait(5000), "startup/shutdown race " + i);
            }
            Console.WriteLine("All MindMap lifecycle tests passed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
