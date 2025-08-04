using AmiumScripter.Core;
using AmiumScripter.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Controls
{
    public class SignalView : Control
    {
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalText { get; set; } = "Text";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalUnit { get; set; } = "°C";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalValue { get; set; } = "23.5";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor { get; set; } = Color.Black;

        public SignalView()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                          | ControlStyles.OptimizedDoubleBuffer
                          | ControlStyles.UserPaint, true);
            this.BackColor = Color.White;
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SourceName { get; set; } = "Unknown";
        public void UpdateSignal(string sourceName)
        {
            Signal source = SignalPool.Get<Signal>(sourceName);
            SignalText = source.Text;
            SignalUnit = source.Unit;

            if (source.Value is IFormattable formattable)
            {
                SignalValue = formattable.ToString(source.Format, CultureInfo.InvariantCulture);
            }
            else
            {
                // Für nicht-formattierbare Typen (z. B. bool, string)
                SignalValue = source.Value?.ToString() ?? string.Empty;
            }

          //  Invalidate(); // Löst Neuzeichnen aus
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Debug.WriteLine("Onpaint " + SourceName);
            base.OnPaint(e);
            var g = e.Graphics;

            Size = new Size(150, 45);

            using var borderPen = new Pen(BorderColor);
          //  g.FillRectangle(Brushes.LightGray, this.ClientRectangle);
            g.DrawRectangle(borderPen, 0, 0, Width-2, this.ClientSize.Height - 2);


            using var textBrush = new SolidBrush(ForeColor);
            using var valueFont = new Font(Font, FontStyle.Bold);
            using var unitFont = new Font(Font.FontFamily, Font.Size, FontStyle.Italic);

            StringFormat tr = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Near
            };

            StringFormat tl = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };

            RectangleF layoutValue = new RectangleF(2, 20, 100, valueFont.Height);
            RectangleF layoutUnit = new RectangleF(102, 20, 50, valueFont.Height);

            g.DrawString(SignalValue, valueFont, textBrush, layoutValue, tr);
            g.DrawString(SignalUnit, unitFont, textBrush, layoutUnit, tl);
            g.DrawString(SignalText, Font, textBrush, new PointF(1, 1));
        }

        private bool _isDragging = false;
        private Point _dragOffset;

        [DllImport("user32.dll")]
        private static extern short GetKeyState(Keys key);
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Starte Dragging, wenn Linksklick + Space
            if (e.Button == MouseButtons.Left && (GetKeyState(Keys.Space) & 0x8000) != 0)
            {
                _isDragging = true;
                _dragOffset = e.Location;
                this.Capture = true;
                this.Cursor = Cursors.SizeAll;
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                    LeftClicked?.Invoke(this, EventArgs.Empty);
                else if (e.Button == MouseButtons.Right)
                    RightClicked?.Invoke(this, EventArgs.Empty);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                var parent = this.Parent;
                if (parent == null) return;


                var newLeft = this.Left + e.X - _dragOffset.X;
                var newTop = this.Top + e.Y - _dragOffset.Y;
        
                this.Location = new Point(newLeft, newTop);
                parent.Invalidate();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isDragging)
            {
                _isDragging = false;
                this.Capture = false;
                this.Cursor = Cursors.Default;

                string pageName = UIEditor.CurrentPageName ?? "TestPage"; // Passe das nach deinem Setup an

                // **Hier aktualisierst du das "Live"-Model und speicherst die Position**
                UIEditor.UpdateControlPosition(pageName, this.Name, this.Left, this.Top);

                // Optional: Ein Event für weitere Verarbeitung
                //ControlMoved?.Invoke(this, EventArgs.Empty);
            }
        }


        public event EventHandler? LeftClicked;
        public event EventHandler? RightClicked;
    }



}
