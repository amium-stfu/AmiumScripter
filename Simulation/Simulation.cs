using System;
using System.Diagnostics;
using AmiumScripter.Modules;

namespace AmiumScripter.Simulation
{
    // Refaktorisierte Version: Parameter als primitive double statt Module.
    // Dynamik jetzt stabiler mit inkrementeller 1. Ordnung Annäherung:
    // Value += (1 - exp(-dt / Tau)) * (Set - Value)
    public class DemoSignal : Module
    {
        // Set-/Ist-Werte (Tau wirkt als Zeitkonstante in Sekunden)
        private double _tau = 0.1;
        public double Tau
        {
            get { lock (_lock) return _tau; }
            set
            {
                lock (_lock)
                {
                    if (double.IsNaN(value) || value <= 0) value = 1e-3;
                    _tau = value;
                }
            }
        }

        // Update-Periode (ms) für den Simulations-Thread
        public int UpdateRateMs { get; set; } = 100;

        // Rauschparameter
        private double _noiseStrength = 0.0;
        public double NoiseStrength
        {
            get { lock (_lock) return _noiseStrength; }
            set { lock (_lock) _noiseStrength = value < 0 ? 0 : value; }
        }

        // Noise-Frequenz: alle N Updates neuer Noise-Wert
        private int _noiseFrequency = 1;
        public int NoiseFrequency
        {
            get { lock (_lock) return _noiseFrequency; }
            set { lock (_lock) _noiseFrequency = value < 1 ? 1 : value; }
        }

        // Aktueller Noise-Wert
        public double Noise { get; private set; } = 0.0;

        // Peak-Injektion (einmalig addiert beim nächsten Sample)
        private int _noisePeak = 0;

        // Zähler für diskrete Noise-Erneuerung
        private int _noiseCounter = 0;

        // Thread / Timing
        private readonly object _lock = new();
        private readonly AThread _thread;
        private readonly Stopwatch _sw = new();
        private double _lastTime;

        // Letzte gültige Value zur Robustheit
        private double _lastGoodValue = 0;

        public DemoSignal(string name, string text, string unit)
            : base(name, register: true)
        {
            Text = text;
            Unit = unit;

            Value = 0;
            Set.Value = 0;
            Out.Value = 0;

            _tau = 0.1;
            _noiseStrength = 0.0;
            _noiseFrequency = 1;

            _sw.Start();
            _lastTime = _sw.Elapsed.TotalSeconds;

            _thread = new AThread("DemoSignalThread", RunLoop, isBackground: true);
            _thread.Start();
        }

        private void RunLoop()
        {
            while (_thread.IsRunning)
            {
                double now = _sw.Elapsed.TotalSeconds;
                double dt = now - _lastTime;
                _lastTime = now;

                if (dt < 0 || dt > 5) dt = 0.0; // Schutz gegen Sprünge

                lock (_lock)
                {
                    StepNoise();
                    StepDynamics(dt);
                }

                // Optional Out-Signal spiegeln
                Out.Value = Value;

                System.Threading.Thread.Sleep(UpdateRateMs);
            }
        }

        private void StepDynamics(double dt)
        {
            // Eingaben prüfen
            double set = Set.Value;
            double tau;
            lock (_lock) tau = _tau;
            if (tau <= 1e-9) tau = 1e-3;

            if (double.IsNaN(set) || double.IsInfinity(set))
                set = _lastGoodValue;

            // 1. Ordnung Low-Pass Annäherung
            // alpha in (0..1), für kleine dt/tau ≈ dt / tau
            double alpha = 1 - Math.Exp(-dt / tau);
            if (alpha < 0) alpha = 0;
            else if (alpha > 1) alpha = 1;

            double newVal = Value + alpha * (set - Value) + Noise;

            if (double.IsNaN(newVal) || double.IsInfinity(newVal))
            {
                Value = _lastGoodValue;
                Noise = 0;
            }
            else
            {
                Value = newVal;
                _lastGoodValue = Value;
            }
        }

        private static readonly ThreadLocal<Random> _rnd = new(() => new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

        private void StepNoise()
        {
            int nf;
            double ns;
            lock (_lock)
            {
                nf = _noiseFrequency;
                ns = _noiseStrength;
            }
            if (nf < 1) nf = 1;

            _noiseCounter++;
            if (_noiseCounter >= nf)
            {
                _noiseCounter = 0;
                var r = _rnd.Value;
                double baseNoise = (r.NextDouble() * 2 - 1) * ns + _noisePeak;
                Noise = baseNoise;
                _noisePeak = 0;
            }
            else
            {
                Noise = _noisePeak;
                _noisePeak = 0;
            }
        }

        public void AddPeak(int min = -500, int max = 500)
        {
            if (min > max) (min, max) = (max, min);
            var r = _rnd.Value;
            lock (_lock)
            {
                _noisePeak = r.Next(min, max + 1);
            }
        }

        public void Stop()
        {
            try { _thread.Stop(); } catch { }
        }
    }
}
