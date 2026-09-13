using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using OneNoteMindMap.Logging;

namespace OneNoteMindMap.UI
{
    public static class UiThread
    {
        private sealed class Session
        {
            internal readonly TaskCompletionSource<Dispatcher> Ready = new TaskCompletionSource<Dispatcher>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource<bool> Exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly CancellationTokenSource Stop = new CancellationTokenSource();
            internal readonly List<Action> CloseWindows = new List<Action>();
            internal Action Cleanup;
            internal bool Finalizing;
        }

        private static readonly object Gate = new object();
        private static Session _session;
        private static Task _lastShutdown = Task.FromResult(true);
        public static Task LastShutdown { get { lock (Gate) return _lastShutdown; } }
        public static bool IsStopping { get { lock (Gate) return _session == null || _session.Stop.IsCancellationRequested; } }

        // Called only by OnConnection. Commands must never resurrect a stopped dispatcher.
        public static void EnsureStarted()
        {
            lock (Gate)
            {
                if (_session != null) return;
                var session = new Session();
                _session = session;
                var thread = new Thread(() => Run(session)) { Name = "OneNote MindMap UI Thread", IsBackground = true };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private static void Run(Session session)
        {
            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
                session.Ready.TrySetResult(dispatcher);
                Logger.Info("UI dispatcher ready");
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                session.Ready.TrySetException(ex);
                Logger.Error("UI dispatcher failed", ex);
            }
            finally
            {
                session.Stop.Cancel();
                Action cleanup;
                lock (Gate)
                {
                    session.Finalizing = true;
                    cleanup = session.Cleanup;
                    session.Cleanup = null;
                }
                try { cleanup?.Invoke(); }
                catch (Exception ex) { Logger.Error("COM cleanup failed", ex); }
                Logger.Info("UI dispatcher exited");
                session.Exited.TrySetResult(true);
            }
        }

        public static void Post(Action action)
        {
            InvokeAsync(() => { action(); return true; }).ContinueWith(
                t => Logger.Error("UI command failed", t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        public static Task<T> InvokeAsync<T>(Func<T> action)
        {
            Session session;
            lock (Gate) session = _session;
            return session == null ? Task.FromCanceled<T>(new CancellationToken(true)) : Dispatch(session, action);
        }

        private static async Task<T> Dispatch<T>(Session session, Func<T> action)
        {
            var result = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (session.Stop.Token.Register(() => result.TrySetCanceled()))
            {
                await Task.WhenAny(session.Ready.Task, result.Task).ConfigureAwait(false);
                session.Stop.Token.ThrowIfCancellationRequested();
                var dispatcher = await session.Ready.Task.ConfigureAwait(false);
                Action execute = () =>
                {
                    if (session.Stop.IsCancellationRequested) { result.TrySetCanceled(); return; }
                    try { result.TrySetResult(action()); }
                    catch (OperationCanceledException) { result.TrySetCanceled(); }
                    catch (Exception ex) { result.TrySetException(ex); }
                };
                if (dispatcher.CheckAccess()) execute();
                else _ = dispatcher.BeginInvoke(execute);
                return await result.Task.ConfigureAwait(false);
            }
        }

        public static T Invoke<T>(Func<T> func) => InvokeAsync(func).GetAwaiter().GetResult();
        public static void Invoke(Action action) => Invoke(() => { action(); return true; });

        // Register on the UI thread; host shutdown closes only this session's windows.
        public static Action TrackWindow(Action close)
        {
            Session session;
            lock (Gate) session = _session;
            if (session == null || session.Stop.IsCancellationRequested) throw new OperationCanceledException();
            session.CloseWindows.Add(close);
            return () => session.CloseWindows.Remove(close);
        }

        public static Task Shutdown(Action cleanup = null)
        {
            Session session;
            Task previous;
            lock (Gate)
            {
                session = _session;
                _session = null;
                previous = _lastShutdown;
                if (session != null && !session.Finalizing)
                {
                    session.Cleanup = cleanup;
                    _lastShutdown = session.Exited.Task;
                }
                else session = null;
            }
            if (session == null) { cleanup?.Invoke(); return previous; }
            session.Stop.Cancel();
            session.Ready.Task.ContinueWith(t =>
            {
                if (t.Status != TaskStatus.RanToCompletion) return;
                t.Result.BeginInvoke(new Action(() =>
                {
                    foreach (var close in session.CloseWindows.ToArray())
                    {
                        try { close(); }
                        catch (Exception ex) { Logger.Error("Closing add-in window failed", ex); }
                    }
                    t.Result.BeginInvokeShutdown(DispatcherPriority.Background);
                }), DispatcherPriority.Send);
            }, TaskScheduler.Default);
            return session.Exited.Task;
        }
    }
}
