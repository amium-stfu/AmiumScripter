using AmiumScripter;
using AmiumScripter.Shared;
using AmiumScripter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Helpers
{
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
                try
                {
                    work();  // work ruft dann intern IsRunning ab
                }
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

            Logger.Log($"[AThread] Registered: {InstanceName}");
            ThreadsManager.Register(this);
        }

        public void Start()
        {
            if (!_thread.IsAlive)
            {
                _thread.Start();
                Logger.Log($"[AThread] Started: {InstanceName}");
            }
        }

        public void Stop()
        {
            if (_thread == null || !_thread.IsAlive)
                return;

            Logger.Log($"[AThread] Stop requested: {InstanceName}");

            _cts.Cancel(); // 1. Kooperativ abbrechen
            if (!_thread.Join(2000))
            {
                Logger.Log($"[AThread] Still running after Cancel: {InstanceName} — trying Interrupt...");
                _thread.Interrupt(); // 2. Versuche blockierende APIs zu unterbrechen

                if (!_thread.Join(1000))
                {
                    Logger.Log($"❌ [AThread] Cannot stop thread {InstanceName} cleanly.");
                    throw new InvalidOperationException($"Thread {InstanceName} refused to stop.");
                }
            }

            Logger.Log($"✅ [AThread] Cleanly stopped: {InstanceName}");
        }


    }




    //public class AWhile : AThread
    //{
    //    private readonly Func<bool> _condition;
    //    private readonly Action _work;

    //    public AWhile(string instanceName, Func<bool> condition, Action work, bool isBackground = true)
    //        : base(instanceName, () => { }, isBackground) // Platzhalter – wird gleich ersetzt
    //    {
    //        _condition = condition;
    //        _work = work;

    //        // Überschreibe den Thread mit eigener Loop-Logik
    //        SetWorker(idle);
    //    }

    //    private void idle()
    //    {
    //        while (_condition())
    //        {
    //            try
    //            {
    //                _work();
    //            }
    //            catch (Exception ex)
    //            {
    //                Logger.Log($"[AWhile] {InstanceName} Fehler: {ex.Message}");
    //                break;
    //            }
    //        }

    //        Logger.Log($"[AWhile] {InstanceName} finished (condition false)");
    //    }
    //}


}