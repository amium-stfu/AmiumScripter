
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{
    public class ATask
    {
        public string InstanceName { get; }
        private CancellationTokenSource _cts = new();
        private Task? _task;
        public bool IsRunning => _task != null && !_task.IsCompleted && !_cts.IsCancellationRequested;

        public event Action? OnCompleted;
        public event Action<Exception>? OnException;
        public event Action? OnCancelled;

        public ATask(string instanceName, Func<CancellationToken, Task> work)
        {
            InstanceName = instanceName;
            TasksManager.Register(this);

            _task = Task.Run(async () =>
            {
                try
                {
                    await work(_cts.Token);
                    OnCompleted?.Invoke();
                }
                catch (OperationCanceledException)
                {
                    Logger.DebugMsg($"[ATask] {InstanceName} cancelled.");
                    OnCancelled?.Invoke();
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.DebugMsg($"[ATask] {InstanceName} exception: {ex.Message}");
                    OnException?.Invoke(ex);
                    throw;
                }
            }, _cts.Token);
            Logger.DebugMsg($"[ATask] Registered: {InstanceName}");
        }

        public async Task AwaitAsync()
        {
            if (_task == null) return;
            try { await _task; }
            catch { /* swallow for now */ }
        }

        public void Stop()
        {
            if (_task == null || _task.IsCompleted)
                return;

            Logger.DebugMsg($"[ATask] Stop requested: {InstanceName}");

            _cts.Cancel();
            try { _task.Wait(2000); }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException)) { }
            catch (Exception ex) { Logger.DebugMsg($"[ATask] {InstanceName} error during stop: {ex.Message}"); }

            Logger.DebugMsg($"✅ [ATask] Cleanly stopped: {InstanceName}");
            TasksManager.Deregister(this);
        }
    }

    public class ATask<T>
    {
        public string InstanceName { get; }
        private CancellationTokenSource _cts = new();
        private Task<T>? _task;
        public bool IsRunning => _task != null && !_task.IsCompleted && !_cts.IsCancellationRequested;

        public event Action<T>? OnResult;
        public event Action<Exception>? OnException;
        public event Action? OnCancelled;

        public ATask(string instanceName, Func<CancellationToken, Task<T>> work)
        {
            InstanceName = instanceName;
            TasksManager.Register(this);

            _task = Task.Run(async () =>
            {
                try
                {
                    var result = await work(_cts.Token);
                    OnResult?.Invoke(result);
                    return result;
                }
                catch (OperationCanceledException)
                {
                    Logger.DebugMsg($"[ATask] {InstanceName} cancelled.");
                    OnCancelled?.Invoke();
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.DebugMsg($"[ATask] {InstanceName} exception: {ex.Message}");
                    OnException?.Invoke(ex);
                    throw;
                }
            }, _cts.Token);
            Logger.DebugMsg($"[ATask] Registered: {InstanceName}");
        }

        public async Task<T?> AwaitAsync()
        {
            if (_task == null) return default;
            try
            {
                return await _task;
            }
            catch
            {
                return default;
            }
        }

        public void Stop()
        {
            if (_task == null || _task.IsCompleted)
                return;

            Logger.DebugMsg($"[ATask] Stop requested: {InstanceName}");

            _cts.Cancel();
            try
            {
                _task.Wait(2000);
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
            {
                Logger.DebugMsg($"[ATask] {InstanceName} cancelled.");
            }
            catch (Exception ex)
            {
                Logger.DebugMsg($"[ATask] {InstanceName} error during stop: {ex.Message}");
            }

            Logger.DebugMsg($"✅ [ATask] Cleanly stopped: {InstanceName}");
            TasksManager.Deregister(this);
        }
    }

    public static class TasksManager
    {
        private static readonly List<object> _tasks = new();

        public static void Register<T>(ATask<T> task)
        {
            if (!_tasks.Contains(task))
                _tasks.Add(task);
        }

        public static void Register(object task)
        {
            if (!_tasks.Contains(task))
                _tasks.Add(task);
        }

        public static void Deregister<T>(ATask<T> task)
        {
            _tasks.Remove(task);
        }

        public static void Deregister(object task)
        {
            _tasks.Remove(task);
        }

        public static void StopAll()
        {
            foreach (var t in _tasks.OfType<dynamic>().ToList())
                t.Stop();
            _tasks.Clear();
        }
    }


    //    // Erstelle einen neuen verwalteten Task
    //    var atask = new ATask("MyAsyncWork", async token =>
    //    {
    //    while (!token.IsCancellationRequested)
    //    {
    //        // ... deine Arbeit ...
    //        await Task.Delay(1000, token);
    //    }
    //    });

    //// Stoppe ihn später
    //   atask.Stop();


}
