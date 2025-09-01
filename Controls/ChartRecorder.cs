using AmiumScripter.Core;
using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.DataSources;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AmiumScripter.Controls
{

    public class PlotSeries
    {
        public UInt16 Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Unit { get; set; }
        public int AxisX { get; set; } 
        public int AxisY { get; set; } 
        public List<double> Values { get; set; }
        public List<DateTime> Timestamps { get; set; }
        public Scatter? Plot { get; set; }
       
        
        int Xrange { get; set; }
        int Interval { get; set; }
        Func<double> Source { get; set; }

        double LastValue;

        bool StepMode;

        Stopwatch stopwatch = new Stopwatch();
        public PlotSeries(UInt16 id,string name, Func<double> source, int xrange, int interval, int axisX = 1, int axisY = 1, string text = null, string unit = "", bool stepMode = false) 
        {
            Id = id;
            Name = name;
            Text = text == null ? name : text;
            Unit = unit;
            AxisX = axisX;
            AxisY = axisY;
            Values = new List<double>();
            Timestamps = new List<DateTime>();
            Source = source;
            Xrange = xrange;
            Interval = interval;
            StepMode = stepMode;
            stopwatch.Start();
            LastValue = Source() == null ? double.NaN: Source();

        }
        // 
        public PlotSeries(UInt16 id,string name, int xrange, int axisX = 1, int axisY = 1, string text = null, string unit = "", bool stepMode = false)
        {
            Id = id;
            Name = name;
            Text = text == null ? name : text;
            Unit = unit;
            AxisX = axisX;
            AxisY = axisY;
            Values = new List<double>();
            Timestamps = new List<DateTime>();
            Xrange = xrange;
            StepMode = stepMode;
        }

        public void RemoveLast()
        {
            Values.RemoveAt(0);
            Timestamps.RemoveAt(0);
        }

        public void Clear()
        {
            Values.Clear();
            Timestamps.Clear();
        }

        public void AddPointOnInterval(DateTime datetime)
        {
           
            if (stopwatch.ElapsedMilliseconds < Interval) return;

            double value = Source();
            

            if (StepMode) AddPointValue(datetime,LastValue);
           
            
            Timestamps.Add(datetime);
            Values.Add(value);

            LastValue = value;
            stopwatch.Restart();

            if ((Timestamps.Count > 0) && (datetime - Timestamps[0]).TotalSeconds > Xrange)
            {
                Timestamps.RemoveAt(0);
                Values.RemoveAt(0);
            }
        }

        public void AddPoint()
        {
            Timestamps.Add(DateTime.Now);
            Values.Add(Source());
        }

        public void AddPointValue(DateTime datetime, double value)
        {

            if (StepMode) 
            {
                Timestamps.Add(datetime);
                Values.Add(LastValue);
            }
            
            Timestamps.Add(datetime);
            Values.Add(value);

            LastValue = value;
            if ((Timestamps.Count > 0) && (datetime - Timestamps[0]).TotalSeconds > Xrange)
            {
                Timestamps.RemoveAt(0);
                Values.RemoveAt(0);
            }
        }

    }



    public class ChartRecorder : ClassBase
    {

        AThread RecordThread;



        bool IsRunning = false;

        DateTime Starttime;
        int RefreshInterval;

        FormsPlot Chart = new FormsPlot();
        public int History = 30;
        public int ViewRange = 60;

        bool recordMode = true;

        public string Name;


        Dictionary<UInt16, PlotSeries> Series = new Dictionary<UInt16, PlotSeries>();
        public LeftAxis y1 = new LeftAxis();
        public LeftAxis y2 = new LeftAxis();
        public LeftAxis y3 = new LeftAxis();
        public LeftAxis y4 = new LeftAxis();

        System.Windows.Forms.Timer updateTimer;

        public bool Y1AutoScale = true;
        public bool Y2AutoScale = true;
        public bool Y3AutoScale = true;
        public bool Y4AutoScale = true;

        public ChartRecorder(string name,FormsPlot chart, int refreshInterval, int history, int viewRange) : base(name)
        {
            Chart = chart;
            History = history;
            ViewRange = viewRange;
            Name = name;
            RefreshInterval = refreshInterval;
            RecordThread = new("ChartRecorder",work: () => RecordIdle());
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = RefreshInterval;
            updateTimer.Tick += new EventHandler((sender, e) => updateIdle());

            Chart.Plot.Axes.DateTimeTicksBottom();
            Chart.Plot.Axes.AddLeftAxis(y2 = new LeftAxis());
            Chart.Plot.Axes.AddLeftAxis(y3 = new LeftAxis());
            Chart.Plot.Axes.AddLeftAxis(y4 = new LeftAxis());
 
            Chart.Plot.Axes.Left.Label.Text = "Y1";
            y1.LabelText = "Y1";
            y2.LabelText = "Y2";
            y3.LabelText = "Y3";
            y4.LabelText = "Y4";

        }

        public void AddSeries(UInt16 id, string name, Func<double> source, int interval, string text = null, string unit = "", int axisX = 1, int axisY = 1, bool stepMode = false)
        {
            Series.Add(id, new PlotSeries(id:id, name:name, source:source, text: text, unit: unit, axisX: axisX, axisY: axisY,xrange: History, interval: interval,stepMode: stepMode));
        }
        public void AddSeries(UInt16 id,string name, string text = null, string unit = "", int axisX = 1, int axisY = 1, bool stepMode = false)
        {
            if(!Series.ContainsKey(id))
                Series.Add(id,new PlotSeries(id: id, name: name, text: text, unit: unit, axisX: axisX, axisY: axisY, xrange: History, stepMode: stepMode));
        }
        public void AddPointToSeries(UInt16 id, DateTime timestamp, double value) 
        {
            Series[id].AddPointValue(timestamp, value);
           // Debug.WriteLine(id + ": " + timestamp + " : " + value);

        }
        public bool HasSeries => Series.Count > 0;
        public void Start(bool record = true)
        {
            recordMode = record;
            IsRunning = true;
            updateTimer.Start();
            Debug.WriteLine(Name + "RecordMode: " + recordMode);
            if(recordMode) 
                RecordThread.Start();

            Play();
           
         
        }
        public void Stop() 
        {
            IsRunning = false;
        }

        private bool realtime = true;
        public bool Realtime 
        { get 
            { 
                return realtime; 
            } 
        }

        public void Pause()
        {
            realtime = false;
            Chart.UserInputProcessor.IsEnabled = true;
            EnableMouseTracker();
        }
       
        public void Play()
        {
            realtime = true;
            Chart.UserInputProcessor.IsEnabled = false;
            DisableMouseTracker();


        }

        void RecordIdle()
        {
            while (!RecordThread.IsStoppRequest)
            {
                DateTime now = DateTime.Now;

                foreach (PlotSeries series in Series.Values)
                {
                    if(RecordThread.IsStoppRequest || !IsRunning) break;
                    series.AddPointOnInterval(now);
                }
                Thread.Sleep(1);
               // RecordThread.Wait(1);
            }

        }

        int autoScaleCounter = 0;

        void updateIdle()
        {
            if (IsRunning)
            {
             if(Realtime) UpdateChart();
            }
        }

        void UpdateChart()
        {
                try
                {
                    Chart.Plot.Clear();
                y2.IsVisible = false;
                y3.IsVisible = false;
                y4.IsVisible = false;
                foreach (PlotSeries series in Series.Values)
                {
                    lock (series)
                    {
                        if (series.AxisY == 0) continue;

                        var xs = series.Timestamps.ToArray();
                        var ys = series.Values.ToArray();
                        series.Plot = Chart.Plot.Add.ScatterLine(xs, ys);
                        series.Plot.LegendText = "Y" + series.AxisY + " " + series.Text;

                        if (series.AxisY == 2)
                        {
                            series.Plot.Axes.YAxis = y2;
                            y2.IsVisible = true;
                           
                        }
                        if (series.AxisY == 3)
                        {
                            series.Plot.Axes.YAxis = y3;
                            y3.IsVisible = true;
                        }
                        if (series.AxisY == 4)
                        {
                            series.Plot.Axes.YAxis = y4;
                            y4.IsVisible = true;
                        }
                    }
                }

                DateTime now = DateTime.Now;
                    Chart.Plot.ShowLegend(Alignment.UpperLeft);
                    double xMin = now.AddSeconds(-ViewRange).ToOADate();
                    double xMax = now.ToOADate();
                    Chart.Plot.Axes.SetLimits(xMin, xMax);

                    Chart.Plot.Axes.AutoScaleY();
                if (Y1AutoScale) Chart.Plot.Axes.AutoScaleY();
                if (Y2AutoScale) Chart.Plot.Axes.AutoScaleY(y2);
                if (Y3AutoScale) Chart.Plot.Axes.AutoScaleY(y3);
                if (Y4AutoScale) Chart.Plot.Axes.AutoScaleY(y4);

                Chart.Refresh(); // Verwendet Render statt Refresh, um Flackern zu vermeiden


                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ChartUpdate Error: " + ex.Message);
                }
        }
        string getPattern(double min, double max)
        {
            double range = max - min;

            range = Math.Abs(range);

            string pattern = "{V:0.0000}";
            if (range > 10)
                pattern = "{V:0.000}";

            if (range > 100)
                pattern = "{V:0.00}";

            if (range > 1000)
                pattern = "{V:0.0}";

            if (range > 10000)
                pattern = "{V:0}";
            return pattern;
        }


        //Crosshair

        LegendItem timeLegend = new LegendItem();
        Annotation crosshairLegend;
        ScottPlot.Plottables.Crosshair CrosshairY1;

        private bool ShowCursorPosition = true;
        private bool ShowHorizontalCrosshair = true;
        private bool ShowVerticalCrosshair = true;
        private bool isMouseInside = false;

        public void EnableMouseTracker()
        {
            CrosshairY1 = Chart.Plot.Add.Crosshair(0, 0);
            CrosshairY1.TextColor = Colors.White;
            CrosshairY1.TextBackgroundColor = CrosshairY1.HorizontalLine.Color;

            Chart.MouseMove += Chart_MouseMove;

            Chart.MouseLeave += Chart_MouseLeave;

            Chart.Refresh();
        }

        public void DisableMouseTracker()
        {
            Chart.MouseMove -= Chart_MouseMove;
            Chart.MouseLeave -= Chart_MouseLeave;
            Chart.Plot.Remove(CrosshairY1);
            Chart.Refresh();
        }


        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (realtime) return;

            bool currentlyInside = Chart.ClientRectangle.Contains(Chart.PointToClient(Cursor.Position));

            if (currentlyInside && !isMouseInside)
            {
                isMouseInside = true;
                Chart.Cursor = Cursors.Cross;
            }
            else if (!currentlyInside && isMouseInside)
            {
                isMouseInside = false;
                Chart.Cursor = Cursors.Default;
            }

            Pixel mousePixel = new(e.X, e.Y);
            Coordinates mouseCoordinates = Chart.Plot.GetCoordinates(mousePixel);
            CrosshairY1.Position = mouseCoordinates;
            double xVal = mouseCoordinates.X;
            DateTime x = DateTime.FromOADate(xVal);

            double y1Value = double.NaN;
            double y2Value = double.NaN;
            double y3Value = double.NaN;
            double y4Value = double.NaN;

            string legendText = "Cursor Position:\r\n";

            if (crosshairLegend != null) Chart.Plot.Remove(crosshairLegend);

            if (Chart.Plot.Axes.Left.IsVisible)
            {
                y1Value = mouseCoordinates.Y;
                legendText += $"Y1: {y1Value:N3}\r\n";
            }

            if (y2.IsVisible)
            {
                y2Value = y2.GetCoordinate(mousePixel.Y, Chart.Plot.LastRender.DataRect);
                legendText += $"Y2: {y2Value:N3}\r\n";
            }
            if (y3.IsVisible)
            {
                y3Value = y3.GetCoordinate(mousePixel.Y, Chart.Plot.LastRender.DataRect);
                legendText += $"Y3: {y3Value:N3}\r\n";
            }
            if (y4.IsVisible)
            {
                y4Value = y4.GetCoordinate(mousePixel.Y, Chart.Plot.LastRender.DataRect);
                legendText += $"Y4: {y2Value:N3}\r\n";
            }
            legendText += "X1: " + x.ToString("yyyy-MM-dd HH:mm:ss.fff");


            Chart.Plot.Legend.ManualItems.Remove(timeLegend);

            bool valueFound = false;
            foreach (PlotSeries series in Series.Values)
            {
                double valueOnX = series.Plot.Data.GetNearestX(mouseCoordinates, Chart.Plot.LastRender).Y;

                if (!double.IsNaN(valueOnX))
                {
                    series.Plot.LegendText = "Y" + series.AxisY + " " + series.Text + ": " + Math.Round(valueOnX, 4);
                    valueFound = true;
                }
                else
                {
                    series.Plot.LegendText = "Y" + series.AxisY + " " + series.Text;


                }

            }
            if (valueFound)
            {
                timeLegend.LineColor = Colors.Transparent;
                timeLegend.MarkerFillColor = Colors.Green;
                timeLegend.MarkerLineColor = Colors.Green;
                timeLegend.LineWidth = 4;
                timeLegend.LabelText = "Time: " + x.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Chart.Plot.Legend.ManualItems.Add(timeLegend);
            }




            if (ShowCursorPosition)
            {
                crosshairLegend = Chart.Plot.Add.Annotation("");
                crosshairLegend.Text = legendText;
                crosshairLegend.LabelBackgroundColor = Colors.White;
                crosshairLegend.LabelBorderColor = Colors.Black;
                crosshairLegend.LabelFontSize = 12;
                crosshairLegend.LabelShadowColor = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
                crosshairLegend.Alignment = Alignment.UpperRight;
            }



            CrosshairY1.VerticalLine.IsVisible = ShowVerticalCrosshair;
            CrosshairY1.HorizontalLine.IsVisible = ShowHorizontalCrosshair;


            CrosshairY1.VerticalLine.Color = Colors.Red;
            CrosshairY1.HorizontalLine.Color = Colors.Red;

            Chart.Refresh();
        }

        private void Chart_MouseLeave(object sender, EventArgs e)
        {

            //Debug.WriteLine("Leaving Chart");
            Cursor.Show();
        }


        public override void Destroy()
        {
            updateTimer.Stop();
            updateTimer.Dispose();
            // AThead Stop by Manager
            //RecordThread.Stop();
        }
    }
}
