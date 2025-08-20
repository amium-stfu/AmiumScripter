﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{
    public abstract class ATimerBase : IDisposable
    {
        public string InstanceName { get; init; }
        public int IntervalMs { get; init; }
        public bool IsRunning { get; protected set; }
        public event Action Tick;

        protected void RaiseTick() => Tick?.Invoke();

        public abstract void Start();
        public abstract void Stop();
        public abstract void Dispose();
    }

    public class ATimerUI : ATimerBase
    {
        private System.Windows.Forms.Timer _timer;

        public ATimerUI(string name, int intervalMs)
        {
            InstanceName = name;
            IntervalMs = intervalMs;
            _timer = new System.Windows.Forms.Timer { Interval = intervalMs };
            _timer.Tick += (s, e) => RaiseTick();
            TimerManager.Register(this);
        }

        public override void Start()
        {
            if (!IsRunning)
            {
                _timer.Start();
                IsRunning = true;
            }
        }

        public override void Stop()
        {
            if (IsRunning)
            {
                _timer.Stop();
                IsRunning = false;
            }
        }

        public override void Dispose()
        {
            Stop();
            _timer?.Dispose();
            TimerManager.Deregister(this);
        }
    }
    public class ATimerPage : ATimerBase
    {
        private System.Threading.Timer _timer;

        public ATimerPage(string name, int intervalMs)
        {
            InstanceName = name;
            IntervalMs = intervalMs;
            TimerManager.Register(this);
        }

        public override void Start()
        {
            if (!IsRunning)
            {
                _timer = new System.Threading.Timer(_ => RaiseTick(), null, IntervalMs, IntervalMs);
                IsRunning = true;
            }
        }

        public override void Stop()
        {
            if (IsRunning)
            {
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();
                _timer = null;
                IsRunning = false;
            }
        }

        public override void Dispose()
        {
            Stop();
            TimerManager.Deregister(this);
        }
    }
    public class ATimerHighPrecision : ATimerBase
    {
        private HighPrecisionTimer _timer;

        public ATimerHighPrecision(string name, int intervalMs)
        {
            InstanceName = name;
            IntervalMs = intervalMs;
            _timer = new HighPrecisionTimer(intervalMs);
            _timer.Tick += () => RaiseTick();
            TimerManager.Register(this);
        }

        public override void Start()
        {
            if (!IsRunning)
            {
                _timer.Start();
                IsRunning = true;
            }
        }

        public override void Stop()
        {
            if (IsRunning)
            {
                _timer.Stop();
                IsRunning = false;
            }
        }

        public override void Dispose()
        {
            Stop();
            _timer?.Dispose();
            TimerManager.Deregister(this);
        }
    }

    public static class TimerManager
    {
        private static readonly List<ATimerBase> _timers = new();

        public static void Register(ATimerBase timer)
        {
            lock (_timers)
            {
                _timers.Add(timer);
            }
        }

        public static void Deregister(ATimerBase timer)
        {
            lock (_timers)
            {
                _timers.Remove(timer);
            }
        }

        public static void StopAll()
        {
            lock (_timers)
            {
                foreach (var timer in _timers.ToList())
                {
                    timer.Stop();
                }
            }
        }

        public static void DisposeAll()
        {
            lock (_timers)
            {
                foreach (var timer in _timers.ToList())
                {
                    timer.Dispose();
                }
                _timers.Clear();
            }
        }
    }




    internal class HighPrecisionTimer : IDisposable
    {
        private uint timerId;
        private readonly TimeProc callback;
        private readonly int interval;
        private bool isRunning;

        public event Action Tick;

        public HighPrecisionTimer(int intervalMs)
        {
            interval = intervalMs;
            callback = new TimeProc(TimerCallback);
        }

        public void Start()
        {
            if (isRunning) return;

            timeBeginPeriod(1); // Systemweite Auflösung auf 1ms setzen
            timerId = timeSetEvent((uint)interval, 0, callback, IntPtr.Zero, TIME_PERIODIC);
            isRunning = true;
        }

        public void Stop()
        {
            if (!isRunning) return;

            timeKillEvent(timerId);
            timeEndPeriod(1);
            isRunning = false;
        }

        private void TimerCallback(uint id, uint msg, IntPtr user, IntPtr param1, IntPtr param2)
        {
            Tick?.Invoke();
        }

        public void Dispose()
        {
            Stop();
        }

        // --- WINMM Imports ---
        private delegate void TimeProc(uint uID, uint uMsg, IntPtr dwUser, IntPtr dw1, IntPtr dw2);

        [DllImport("winmm.dll")]
        private static extern uint timeSetEvent(uint msDelay, uint msResolution, TimeProc handler, IntPtr userCtx, uint eventType);

        [DllImport("winmm.dll")]
        private static extern void timeKillEvent(uint uTimerId);

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uMilliseconds);

        private const int TIME_PERIODIC = 1;
    }
}
