using System;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace OneNoteMindMap.UI
{
    public static class UiThread
    {
        private static Dispatcher _dispatcher;
        private static Thread _thread;

        public static void EnsureStarted()
        {
            if (_dispatcher != null) return;

            using (var readyEvent = new ManualResetEvent(false))
            {
                _thread = new Thread(() =>
                {
                    _dispatcher = Dispatcher.CurrentDispatcher;
                    try { InputMethod.Current.ImeState = InputMethodState.Off; } catch { }
                    readyEvent.Set();
                    Dispatcher.Run();
                })
                {
                    Name = "OneNote MindMap UI Thread",
                    IsBackground = true
                };
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
                readyEvent.WaitOne(5000);
            }
        }

        public static void Post(Action action)
        {
            if (_dispatcher == null || _dispatcher.HasShutdownFinished)
            {
                EnsureStarted();
            }
            _dispatcher?.BeginInvoke(action);
        }

        public static T Invoke<T>(Func<T> func)
        {
            if (_dispatcher == null || _dispatcher.HasShutdownFinished)
                EnsureStarted();

            if (_dispatcher == null) return default;
            return (T)_dispatcher.Invoke(func);
        }

        public static void Invoke(Action action)
        {
            if (_dispatcher == null || _dispatcher.HasShutdownFinished)
                EnsureStarted();
            _dispatcher?.Invoke(action);
        }

        public static void Shutdown()
        {
            if (_dispatcher != null && !_dispatcher.HasShutdownFinished)
            {
                _dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            }
        }
    }
}
