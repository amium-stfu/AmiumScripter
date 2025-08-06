using System;
using AmiumScripter.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AmiumScripter.Simulation
{
    public class DemoSignal : Module
    {

        public Module Tau = new Module("Tau");
        public Module UpdateRate = new Module("UpdateRate");
        public Module NoiseStrength = new Module("NoiseStrength");
        public Module NoiseFrequence = new Module("NoiseFrequence");
        public Module Noise = new Module("ValueNoise");
        public Module Frequence = new Module("ValueFrequence");

        //tau simulation parameter
        private double start = 0;
        private double time = 0;
        private double range = 0;
        private double newSet = 0;

        //Sample simulation parameter
        private int randomCounter = 0;
        public int RandomValueAt = -1;
        public int RandomRange = 100;

        //Noise parameter
        private int noiseCounter = 1;
        private double NoiseValue;
        private int noisePeak = 0;

        //rate
        DateTime startTime;
        TimeSpan duration;

        public string Name;

        private readonly object _lock = new();
        private AThread _thread;

        public DemoSignal(string name, string text, string unit) : base(name)
        {
            Text = text;
            Unit = unit;
            startTime = DateTime.Now;

            Value = 0;
            Out.Value = 0;
            Set.Value = 0;
            time = 0;

            Out.Text = text + ".Out";
            Set.Text = text + ".Set";
            Set.Unit = unit;

            NoiseFrequence.Value = 1;
            NoiseStrength.Set.Value = 0;
            NoiseFrequence.Set.Value = 1;
            NoiseStrength.Value = 0;

            Tau.Value = 0.1;
            Tau.Set.Value = 0.1;

            _thread = new AThread("DemoSignalThread", () => RunSimulation(), isBackground: true);
            _thread.Start();
        }

        private void RunSimulation()
        {
            while (_thread.IsRunning)
            {
                lock (_lock)
                {
                    addNoise();
                    generate();

                    Tau.Value = Tau.Set.Value;
                    NoiseStrength.Value = NoiseStrength.Set.Value;
                    NoiseFrequence.Value = NoiseFrequence.Set.Value;
                }
                System.Threading.Thread.Sleep(100); // Intervall wie Timer
            }
        }
        public void generate()
        {
            // lock (_lock) nicht nötig, da RunSimulation schon locked
            duration = DateTime.Now - startTime;
            double rate = duration.TotalSeconds;
            if (newSet != Set.Value)
            {
                newSet = Set.Value;
                range = Set.Value - Value;
                start = Value;
                time = 0;
            }
            if (time < 10 * Tau.Value)
            {
                time = time + rate;
                var t1 = time * -1;
                var t2 = t1 / Tau.Value;
                var et = 1 - Math.Exp(t2);
                var d = range * et;
                Value = start + d + Noise.Value;
            }
            else
            {
                Value = Set.Value + Noise.Value;
            }
            startTime = DateTime.Now;
        }

        private void addNoise()
        {
            double nf = NoiseFrequence.Value;
            noiseCounter++;
            if (noiseCounter > nf)
                noiseCounter = 0;
            if (noiseCounter == nf)
            {
                var r = new Random();
                double n = r.Next(-100, 100);

                double rstl = n / 100 * NoiseStrength.Value + noisePeak;
                Noise.Value = rstl;
                noiseCounter = 0;
                noisePeak = 0;
            }
            else
            {
                Noise.Value = 0 + noisePeak;
                noisePeak = 0;
            }
        }

        public void AddPeak()
        {
            lock (_lock)
            {
                var r = new Random();
                noisePeak = r.Next(-500, 500);
            }
        }

    }
}
