using AmiumScripter.Helpers;

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

    public static void StopAll()
    {
        lock (_threads)
        {
            foreach (var thread in _threads)
            {
                thread.Stop(); // interne Prüfung + Join
            }
            _threads.Clear();
        }
    }
}

