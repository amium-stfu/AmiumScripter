using AmiumScripter.Helpers;
using AmiumScripter.Shared;

public class AThread
{
    public string InstanceName { get; init; }
    private Thread _thread;
    private CancellationTokenSource _cts = new();
    public bool IsRunning => _thread.IsAlive && !_cts.IsCancellationRequested;

    public AThread(string instanceName, Action work, bool isBackground = true)
    {
        InstanceName = instanceName;

        _thread = new Thread(() =>
        {
            try { work(); }
            catch (OperationCanceledException)
            {
                Logger.Log($"[AThread] {InstanceName} cancelled.");
            }
            catch (Exception ex)
            {
                Logger.Log($"[AThread] {InstanceName} error: {ex.Message}");
            }
        });
        _thread.IsBackground = isBackground;

        ThreadsManager.Register(this);
        Logger.Log($"[AThread] {InstanceName}: Registered");
    }

    public void Start()
    {
        Logger.Log($"[AThread] {InstanceName}: Try to start");
        if (!IsRunning)
        {
            _thread.Start();
            Logger.Log($"[AThread] {InstanceName}: Started");
        }
        else { 
            Logger.Log($"[AThread] {InstanceName}: ThreadState {_thread.ThreadState.ToString()} ");
        }
    }

    public void Stop()
    {
        if (_thread == null || !_thread.IsAlive)
            return;

        Logger.Log($"[AThread] Stop requested: {InstanceName}");

        _cts.Cancel();
        if (!_thread.Join(2000))
        {
            Logger.Log($"[AThread] Still running after Cancel: {InstanceName} — trying Interrupt...");
            _thread.Interrupt();

            if (!_thread.Join(1000))
            {
                Logger.Log($"❌ [AThread] Cannot stop thread {InstanceName} cleanly.");
                throw new InvalidOperationException($"Thread {InstanceName} refused to stop.");
            }
        }

        Logger.Log($"✅ [AThread] Cleanly stopped: {InstanceName}");
        ThreadsManager.Deregister(this);
    }
}


public static class ThreadsManager
{
    private static readonly List<AThread> _threads = new();

    public static void Register(AThread thread)
    {
        lock (_threads)
        {
            _threads.Add(thread);
        }
    }

    public static void Deregister(AThread thread)
    {
        _threads.Remove(thread);
    }


    public static void StopAll()
    {
        lock (_threads)
        {
            foreach (var thread in _threads.ToList()) // Kopie!
            {
                thread.Stop();
            }
            _threads.Clear();
        }
    }
}

