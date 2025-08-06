using AmiumScripter.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AmiumScripter.Core.StorageItem;

namespace AmiumScripter.Core
{
    public static class SignalManager
    {
        private static readonly ConcurrentQueue<(string Name, object Value)> _setQueue = new();
        private static readonly System.Threading.Timer _processTimer;

        static SignalManager()
        {
            _processTimer = new System.Threading.Timer(_ => ProcessQueue(), null, 10, 10);
        }

        /// <summary>
        /// Enqueues a value update for a signal to be processed asynchronously.
        /// </summary>
        internal static void QueueSet(string name, object value)
        {
            _setQueue.Enqueue((name, value));
        }

        /// <summary>
        /// Immediately sets the value of an existing signal in the DataStorage.
        /// </summary>
        internal static void SetImmediate(string name, object value)
        {
            if (value is not BaseSignalCommon incoming)
                return;

            if (!SignalPool.TryGet(name, out var obj))
            {
                // Neues Signal registrieren
                SignalPool.Set(name, incoming);
                OnSignalUpdated?.Invoke(name, incoming);
                return;
            }

            if (obj is Signal targetSignal && value is Signal incomingSignal)
            {
                targetSignal.Value = incomingSignal.Value;
                OnSignalUpdated?.Invoke(name, targetSignal);
            }
            else if (obj is BoolSignal targetBool && value is BoolSignal incomingBool)
            {
                targetBool.Value = incomingBool.Value;
                OnSignalUpdated?.Invoke(name, targetBool);
            }
            else if (obj is StringSignal targetString && value is StringSignal incomingString)
            {
                targetString.Value = incomingString.Value;
                OnSignalUpdated?.Invoke(name, targetString);
            }
            else if (obj is Module targetModule && value is Module incomingModule)
            {
                targetModule.Value = incomingModule.Value;
                OnSignalUpdated?.Invoke(name, targetModule);
            }
            else
            {
                // Typ passt nicht – optional loggen oder ignorieren
            }
        }

        public static bool DebugFlag;
        public static void PushSignal(string sender, BaseSignalCommon signal, bool direct = false)
        {
            if (DebugFlag)
                Debug.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} -> {sender} push signal {signal.Name}");

            signal.SetLastSender(sender);

            if (direct)
                SetImmediate(signal.Name, signal);
            else
                QueueSet(signal.Name, signal);
        }

        internal static void ProcessQueue()
        {
            int maxPerTick = 100;
            while (maxPerTick-- > 0 && _setQueue.TryDequeue(out var entry))
            {
                SetImmediate(entry.Name, entry.Value);
            }
        }

        internal static void SetProperty(string name, string key, string? value)
        {
            if (SignalPool.TryGet(name, out var obj) && obj is BaseSignalCommon signal)
            {
                signal.SetProperty(key, value);
            }
        }

        internal static void RemoveSignal(string name)
        {
            SignalPool.Remove(name);
        }

        /// <summary>
        /// Optional callback when a signal was updated.
        /// </summary>
        public static Action<string, BaseSignalCommon>? OnSignalUpdated;
    }


}
