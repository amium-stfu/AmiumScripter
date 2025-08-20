using AmiumScripter.Helpers;
using AmiumScripter;

public class AThread
{
    public string InstanceName { get; init; }
    private Thread _thread;
    private CancellationTokenSource _cts = new();
    public bool IsRunning => _thread.IsAlive && !_cts.IsCancellationRequested;

    bool done = false;

    public bool IsDone
    {
        get => done;
        set
        {
            if (value && !done)
            {
                done = true;
                Logger.DebugMsg($"[AThread] {InstanceName} marked as done.");
            }
        }
    }

    public bool IsStoppRequest => _cts.IsCancellationRequested;

    public AThread(string instanceName, Action work, bool isBackground = true)
    {
        InstanceName = instanceName;

        _thread = new Thread(() =>
        {
            
            try {
            done = false;
                work(); 
            done = true;
            }
            catch (OperationCanceledException)
            {
                Logger.DebugMsg($"[AThread] {InstanceName} cancelled.");
                done = true;
            }
            catch (Exception ex)
            {
                Logger.DebugMsg($"[AThread] {InstanceName} error: {ex.Message}");
                done = true;
            }

        });
        _thread.IsBackground = isBackground;

        ThreadsManager.Register(this);
        Logger.DebugMsg($"[AThread] {InstanceName}: Registered");
    }

    public void Start()
    {
        Logger.DebugMsg($"[AThread] {InstanceName}: Try to start");
        if (!IsRunning)
        {
            _thread.Start();
            Logger.DebugMsg($"[AThread] {InstanceName}: Started");
        }
        else { 
            Logger.DebugMsg($"[AThread] {InstanceName}: ThreadState {_thread.ThreadState.ToString()} ");
        }
    }

    public void Wait(int milliSeconds)
    {
        DateTime start = DateTime.Now;
        while (DateTime.Now < start.AddMilliseconds(milliSeconds))
        {
            if (IsStoppRequest || !IsRunning) break;
            System.Threading.Thread.Sleep(5);
        }
    }

    public void Stop()
    {
        if (_thread == null || !_thread.IsAlive)
            return;

        Logger.DebugMsg($"[AThread] Stop requested: {InstanceName}");

        _cts.Cancel();
        if (!_thread.Join(5000))
        {
            Logger.DebugMsg($"[AThread] Still running after Cancel: {InstanceName} — trying Interrupt...");
            _thread.Interrupt();

            if (!_thread.Join(1000))
            {
                Logger.DebugMsg($"❌ [AThread] Cannot stop thread {InstanceName} cleanly.");
                throw new InvalidOperationException($"Thread {InstanceName} refused to stop.");
            }
        }


        Logger.DebugMsg($"✅ [AThread] Cleanly stopped: {InstanceName}");
        done = true;
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

