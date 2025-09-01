using AmiumScripter.Helpers;
using AmiumScripter.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AmiumScripter.Core
{
    public interface IView
    {
        void Initialize();
        void Run();
        void Destroy();
        void ViewIdleLoop();
    }
    public abstract class BaseView : UserControl, IView
    {
        private System.Windows.Forms.Timer idleTimer;
        int Interval = 100;

        public string PageText;

        // Auswahlrechteck-Zustand
        private bool _isSelecting = false;
        private System.Drawing.Point _selStart;
        private System.Drawing.Point _selEnd;

        (int X, int Y, int H, int W) NewControl;

        // Event: liefert das gewählte Rechteck in View-Client-Koordinaten
        public event Action<Rectangle>? SelectionCompleted;

        public void StartIdleLoop(int interval)
        {
            Interval = interval;
            if (idleTimer != null)
                return;

            idleTimer = new System.Windows.Forms.Timer();
            idleTimer.Interval = Interval;
            idleTimer.Tick += (s, e) => ViewIdleLoop();
            idleTimer.Start();
            Logger.DebugMsg("[PageView] Start IdleLoop (Timer)");
        }

        public void StopIdleLoop()
        {
            if (idleTimer != null)
            {
                idleTimer.Stop();
                idleTimer.Dispose();
                idleTimer = null;
                Logger.DebugMsg("[PageView] IdleLoop stopped (Timer)");
            }
        }

        public virtual void Initialize()
        {
            // optional glattes Zeichnen
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        public virtual void Run() { }

        public virtual void Destroy()
        {
            StopIdleLoop();
            Controls.Clear();
            Logger.DebugMsg("[PageView] TestPage Destroyed");
        }
        public virtual void ViewIdleLoop() { }

        // --- Rechteck-Selektion nur bei UIEditor.EditMode == true ---

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!UIEditor.EditMode) return;

            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _selStart = _selEnd = e.Location;
                this.Capture = true;
                this.Cursor = Cursors.Cross;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!UIEditor.EditMode) return;

            if (_isSelecting)
            {
                _selEnd = e.Location;
                Invalidate();
            }
        }
       
        protected override void OnMouseUp(MouseEventArgs e)
        {
           
            base.OnMouseUp(e);
            if (!UIEditor.EditMode) return;

            using ContextMenuStrip popupMenu = new ContextMenuStrip();

            if (_isSelecting && e.Button == MouseButtons.Left)
            {
                _isSelecting = false;
                this.Capture = false;
                this.Cursor = Cursors.Default;
                var rect = GetSelectionRectangle();

                // nur sinnvolle Auswahl zurückmelden
                if (rect.Width > 2 && rect.Height > 2)
                {
                    try { SelectionCompleted?.Invoke(rect); } catch { }
                }
                Invalidate();

                if(rect.Height > 10 && rect.Width > 10)
                ShowPopUp(rect.Right, rect.Bottom);
                
              
            }
        }

        void ShowPopUp(int x, int y)
        {
            FormAddControl form = new FormAddControl();
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new System.Drawing.Point(x, y);
            form.Show();
         
        }

        void dummy() { }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_isSelecting && UIEditor.EditMode)
            {
                var rect = GetSelectionRectangle();
                using var fill = new SolidBrush(Color.FromArgb(60, Color.DodgerBlue));
                using var pen = new Pen(Color.DodgerBlue) { DashStyle = DashStyle.Dash };
                e.Graphics.FillRectangle(fill, rect);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private Rectangle GetSelectionRectangle()
        {
            int x = Math.Min(_selStart.X, _selEnd.X);
            int y = Math.Min(_selStart.Y, _selEnd.Y);
            int w = Math.Abs(_selStart.X - _selEnd.X);
            int h = Math.Abs(_selStart.Y - _selEnd.Y);
            UIEditor.NewControl.X = x;
            UIEditor.NewControl.Y = y;
            UIEditor.NewControl.H = h;
            UIEditor.NewControl.W = w;

            return new Rectangle(x, y, w, h);
        }
    }
}
