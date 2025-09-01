using AmiumScripter.Core;
using AmiumScripter.Helpers;
using AmiumScripter.Modules;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FontAwesome.Sharp;
using Microsoft.VisualBasic.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace AmiumScripter.Controls
{
    public class LimitedSizeDictionary<TKey, TValue>
    {
        private readonly int maxSize;
        private readonly Queue<TKey> keyQueue;
        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public LimitedSizeDictionary(int maxSize)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("maxSize must be greater than zero.");
            }
            this.maxSize = maxSize;
            keyQueue = new Queue<TKey>(maxSize);
        }

        public void Add(TKey key, TValue value)
        {
            lock (dictionary)
            {
                if (dictionary.Count >= maxSize)
                {
                    TKey oldestKey = keyQueue.Dequeue();
                    dictionary.Remove(oldestKey);
                }

                keyQueue.Enqueue(key);
                dictionary[key] = value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (dictionary)
            {
                return dictionary.TryGetValue(key, out value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (dictionary.TryGetValue(key, out TValue value))
                {
                    return value;
                }
                else
                    return default(TValue);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (dictionary)
            {
                return dictionary.ContainsKey(key);
            }
        }

        public int GridScale = 5;

    }

    public static class Icons
    {
        static LimitedSizeDictionary<string, Image> _faIconDict = new LimitedSizeDictionary<string, Image>(100);
        public static Image GetFaIcon(string name, int size = 256, object colorName = null)
        {

            if (colorName == null)
                colorName = "black";
   

            try
            {
                string key = name + ":" + size + ":" + colorName;
                if (_faIconDict.ContainsKey(key))
                    return _faIconDict[key];
                else
                {
                    try
                    {
                        FontAwesome.Sharp.IconChar fa_icon = FontAwesome.Sharp.IconChar.None;
                        bool ok = Enum.TryParse<FontAwesome.Sharp.IconChar>(name.Replace("-", ""), true, out fa_icon);
                        Bitmap bmp = fa_icon.ToBitmap(size, size, colorName.ToColor()); // System.Drawing.Color.Black);
                        //HACK: the returned bmp is not centered. so move it slightly:
                        Bitmap bmp2 = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                        using (Graphics graph = Graphics.FromImage(bmp2))
                        {
                            graph.DrawImageUnscaled(bmp, (int)(bmp.Width * 0.01), (int)(bmp.Height * 0.08));
                        }
                        _faIconDict.Add(key, bmp2);
                        return bmp2;
                    }
                    catch
                    {
                        _faIconDict.Add(key, null);
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

    }

    public abstract class BaseControl : Control
    {
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor { get; set; } = Color.Black;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color HoverColor { get; set; } = Color.Transparent;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int GridScale { get; set; } = 5;

        public event EventHandler? LeftClicked;
        public event EventHandler? RightClicked;

        internal object? _source;

        private bool _isDragging = false;
        private Point _dragOffset;

        

        // Resize support
        private bool _isResizing = false;
        private bool _hoverResizeGrip = false;
        private Point _resizeStart;
        private Size _initialSize;
        protected const int ResizeGripSize = 15;    

        // Move support
        private bool _isMoving = false;
        private bool _hoverMoveGrip = false;
        private Point _moveStart;
        private const int MoveGripSize = 15;

        // Neu: ursprüngliche Hintergrundfarbe merken (für Restore)
        private Color _backColorBeforeHover = Color.Empty;

        [DllImport("user32.dll")]
        private static extern short GetKeyState(Keys key);

        protected BaseControl()
        {
            // Make sure the control can receive focus and arrow keys
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            var keyCode = keyData & Keys.KeyCode;
            if (keyCode == Keys.Left || keyCode == Keys.Right || keyCode == Keys.Up || keyCode == Keys.Down)
                return true;
            return base.IsInputKey(keyData);
        }

        // helper: snap to grid
        private int Snap(int value)
        {
            int s = Math.Max(1, GridScale);
            return (int)Math.Round(value / (double)s) * s;
        }
        private Point Snap(Point p) => new Point(Snap(p.X), Snap(p.Y));

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (HoverColor != Color.Transparent)
            {
                if (_backColorBeforeHover == Color.Empty)
                    _backColorBeforeHover = BackColor; // sichern (einmalig)
                BackColor = HoverColor;
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (HoverColor != Color.Transparent && _backColorBeforeHover != Color.Empty)
            {
                BackColor = _backColorBeforeHover;
            }
            if (_hoverResizeGrip)
            {
                _hoverResizeGrip = false;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }


        private bool _hasFocus = false;
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _hasFocus = true;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
           
            base.OnLostFocus(e);
            _hasFocus = false;
            Invalidate();

        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // ensure focus to receive keyboard
            if (!Focused) this.Focus();

            // Resize: only in EditMode and when pressing inside the grip
            if (UIEditor.EditMode && e.Button == MouseButtons.Left && IsInResizeGrip(e.Location))
            {
                _isResizing = true;
                _resizeStart = e.Location;
                _initialSize = this.Size;
                this.Capture = true;
                this.Cursor = Cursors.SizeNWSE;
                return;
            }

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
                {
                    if (UIEditor.EditMode) return;
                    LeftClicked?.Invoke(this, EventArgs.Empty);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (UIEditor.EditMode) return;
                    RightClicked?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Update hover state for grip
            if (UIEditor.EditMode && !_isDragging && !_isResizing)
            {
                bool wasHover = _hoverResizeGrip;
                _hoverResizeGrip = IsInResizeGrip(e.Location);
                if (wasHover != _hoverResizeGrip)
                {
                    Cursor = _hoverResizeGrip ? Cursors.SizeNWSE : Cursors.Default;
                    Invalidate();
                }
            }

            if (_isResizing)
            {
                int dx = e.X - _resizeStart.X;
                int dy = e.Y - _resizeStart.Y;
                int newW = Math.Max(10, _initialSize.Width + dx);
                int newH = Math.Max(10, _initialSize.Height + dy);
                this.Size = new Size(newW, newH);
                this.Invalidate();
                this.Parent?.Invalidate();
                return;
            }

            if (_isDragging)
            {
                var parent = this.Parent;
                if (parent == null) return;
                var newLeft = this.Left + e.X - _dragOffset.X;
                var newTop = this.Top + e.Y - _dragOffset.Y;
                var snapped = Snap(new Point(newLeft, newTop));
                this.Location = snapped;
                parent.Invalidate();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isResizing)
            {
                _isResizing = false;
                this.Capture = false;
                this.Cursor = _hoverResizeGrip ? Cursors.SizeNWSE : Cursors.Default;
                string pageName = UIEditor.CurrentPageName ?? "TestPage";
                string controlType = this.GetType().Name.ToString();
                UIEditor.UpdateControlSize(pageName, this.Name, controlType, this.Width, this.Height);
                return;
            }

            if (_isDragging)
            {
                _isDragging = false;
                this.Capture = false;
                this.Cursor = Cursors.Default;
                // snap once more before persisting
                var snapped = Snap(this.Location);
                this.Location = snapped;
                string pageName = UIEditor.CurrentPageName ?? "TestPage";
                string controlType = this.GetType().Name.ToString();
                UIEditor.UpdateControlPosition(pageName, this.Name, controlType, this.Left, this.Top);
            }
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Arrow keys are not delivered via KeyPress; handled in OnKeyDown.
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!UIEditor.EditMode)
                return;

            bool handled = false;
            string pageName = UIEditor.CurrentPageName ?? "TestPage";
            string controlType = this.GetType().Name.ToString();

            // Move with Shift + Arrows
            if (e.Shift)
            {
                int dx = 0, dy = 0;
                switch (e.KeyCode)
                {
                    case Keys.Left: dx = -GridScale; break;
                    case Keys.Right: dx = GridScale; break;
                    case Keys.Up: dy = -GridScale; break;
                    case Keys.Down: dy = GridScale; break;
                }
                if (dx != 0 || dy != 0)
                {
                    var target = new Point(this.Left + dx, this.Top + dy);
                    var snapped = Snap(target);
                    this.Location = snapped;
                    UIEditor.UpdateControlPosition(pageName, this.Name, controlType, this.Left, this.Top);
                    this.Parent?.Invalidate();
                    handled = true;
                }
            }
            // Resize with Ctrl + Arrows
            else if (e.Control)
            {
                int dw = 0, dh = 0;
                switch (e.KeyCode)
                {
                    case Keys.Left: dw = -GridScale; break;
                    case Keys.Right: dw = GridScale; break;
                    case Keys.Up: dh = -GridScale; break;
                    case Keys.Down: dh = GridScale; break;
                }
                if (dw != 0 || dh != 0)
                {
                    int newW = Math.Max(10, this.Width + dw);
                    int newH = Math.Max(10, this.Height + dh);
                    this.Size = new Size(newW, newH);
                    UIEditor.UpdateControlSize(pageName, this.Name, controlType, this.Width, this.Height);
                    this.Invalidate();
                    this.Parent?.Invalidate();
                    handled = true;
                }
            }

            if (handled)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        public void SetSource(object source)
        {
            ControlManager.Register(Name, ((BaseSignalCommon)source).Name, this);
            _source = source ?? throw new ArgumentNullException(nameof(source));
            Update();
        }
        public abstract void Update();

        // Draw small resize grip (bottom-right) only in edit mode and when hovered/resizing
        protected void DrawResizeGrip(Graphics g)
        {
            if (!UIEditor.EditMode) return;
            if (!_hoverResizeGrip && !_isResizing) return;

            using var b = new SolidBrush(Color.Yellow);
            var rect = GetResizeGripRect();
            g.FillRectangle(b, rect);
            using var p = new Pen(Color.Goldenrod);
            g.DrawRectangle(p, rect);
        }
        private Rectangle GetResizeGripRect()
        {
            return new Rectangle(this.Width - ResizeGripSize - 1, this.Height - ResizeGripSize - 1, ResizeGripSize, ResizeGripSize);
        }
        private bool IsInResizeGrip(Point p)
        {
            return GetResizeGripRect().Contains(p);
        }

        internal StringFormat RightTop = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Near
        };
        internal StringFormat LeftTop = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near
        };
        internal StringFormat CenterTop = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Near
        };
        internal StringFormat CenterCenter = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        internal StringFormat LeftCenter = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center
        };
        internal StringFormat RightCenter = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Center
        };
        internal StringFormat CenterBottom = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Far
        };
        internal StringFormat LeftBottom = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Far
        };
        internal StringFormat RightBottom = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Far
        };


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Paint(e); // Aufruf der virtuellen Methode
            if (_hasFocus && UIEditor.EditMode)
            {
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            }
          
        }



        internal virtual void Paint(PaintEventArgs e)
        {
        
        
        }

    }
    public class SignalView : BaseControl
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
        public string SourceName { get; set; } = "Unknown";

        public SignalView()
        {

            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                          | ControlStyles.OptimizedDoubleBuffer
                          | ControlStyles.UserPaint, true);
            this.BackColor = Color.White;

        }

        public override void Update()
        {
            //    Debug.WriteLine(Name + "Update");
            var signal = _source as Signal;
            if (signal == null) return;
            SignalText = signal.Text;
            SignalUnit = signal.Unit;
            if (signal.Value is IFormattable formattable)
            {
                SignalValue = formattable.ToString(signal.Format, CultureInfo.InvariantCulture);
            }
            else
            {
                SignalValue = signal.Value.ToString() ?? string.Empty;
            }
            Invalidate(); // Löst Neuzeichnen aus
        }

        Color backColor;
        public override Color BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                this.Invalidate();
            }
        }

        public void SaveInvoke(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        internal override void Paint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            var g = e.Graphics;
           
            using var borderPen = new Pen(BorderColor);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, Width - 2, this.ClientSize.Height - 2);
            g.DrawRectangle(borderPen, 0, 0, Width - 2, this.ClientSize.Height - 2);
            using var textBrush = new SolidBrush(ForeColor);

            float fontSize = Height / 4f;


            using var valueFont = new Font(Font.FontFamily, fontSize, FontStyle.Bold);
            using var unitFont = new Font(Font.FontFamily, fontSize, FontStyle.Italic);
            using var headerFont = new Font(Font.FontFamily, fontSize * 0.7f, FontStyle.Regular);



            RectangleF layoutValue = new RectangleF(2, (int)(Height * 0.25), Width - Height - 13, valueFont.Height);
            RectangleF layoutUnit = new RectangleF(Width-Height - 10, (int)(Height * 0.25), Height, valueFont.Height);
            g.DrawString(SignalValue, valueFont, textBrush, layoutValue, RightTop);
            g.DrawString(SignalUnit, unitFont, textBrush, layoutUnit, LeftTop);
            g.DrawString(SignalText, headerFont, textBrush, new PointF(1, 1));

            // draw resize grip on top
            DrawResizeGrip(g);
        }
    }
    public class StringSignalView : BaseControl
    {
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalText { get; set; } = "Text";

      //  [Browsable(true)]
      //  [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      //  public string SignalUnit { get; set; } = "°C";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalValue { get; set; } = "Unknown";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SourceName { get; set; } = "Unknown";

        public StringSignalView()
        {

            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                          | ControlStyles.OptimizedDoubleBuffer
                          | ControlStyles.UserPaint, true);
            this.BackColor = Color.White;

            LeftClicked += (sender, e) => _edit();

        }

        public override void Update()
        {
            var signal = _source as StringSignal;
            if (signal == null) return;
            SignalText = signal.Text;
            SignalValue = signal.Value.ToString() ?? string.Empty;
         
            Invalidate(); // Löst Neuzeichnen aus
        }

        Color backColor;
        public override Color BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                this.Invalidate();
            }
        }

        public void SaveInvoke(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        internal override void Paint(PaintEventArgs e)
        {
          //  base.OnPaint(e);
            var g = e.Graphics;
      
            using var borderPen = new Pen(BorderColor);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, Width - 2, this.ClientSize.Height - 2);
            g.DrawRectangle(borderPen, 0, 0, Width - 2, this.ClientSize.Height - 2);
            using var textBrush = new SolidBrush(ForeColor);

            float fontSize = Height / 4f;

            using var valueFont = new Font(Font.FontFamily, fontSize, FontStyle.Bold);
            using var unitFont = new Font(Font.FontFamily, fontSize, FontStyle.Italic);
            using var headerFont = new Font(Font.FontFamily, fontSize * 0.7f, FontStyle.Regular);


            RectangleF layoutValue = new RectangleF(10, (int)(Height * 0.25), Width - (Height * 0.3f), valueFont.Height);
            RectangleF layoutUnit = new RectangleF(Width - Height - 10, (int)(Height * 0.25), Height, valueFont.Height);
            g.DrawString(SignalValue, valueFont, textBrush, layoutValue, RightTop);
            g.DrawString(SignalText, headerFont, textBrush, new PointF(1, 1));

            // draw resize grip on top
            DrawResizeGrip(g);
        }


        void _edit()
        {
            string _value = "";

            if (EditValue.WithKeyboardDialog(ref _value, "Enter new value"))
            {
                if (_source != null)
                {
                    var signal = _source as StringSignal;
                    signal.Value = _value;
                    Update();
                }
                else
                {
                    Logger.InfoMsg($"[StringSignalView] {Name} : Source is null");
                    SignalValue = _value;
                    Invalidate();
                }

                    
            }
        }

        
    }
    public class ModuleView : BaseControl
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
        public string OutValue { get; set; } = "45%";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SetValue { get; set; } = "34.25";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SourceName { get; set; } = "Unknown";

        public ModuleView()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                          | ControlStyles.OptimizedDoubleBuffer
                          | ControlStyles.UserPaint, true);
            this.BackColor = Color.White;
        }

        public override void Update()
        {
            var signal = _source as Module;
            if (signal == null) return;
            SignalText = signal.Text;
            SignalUnit = signal.Unit;
            if (signal.Value is IFormattable formattable)
            {
                SignalValue = formattable.ToString(signal.Format, CultureInfo.InvariantCulture);
            }
            else
            {
                SignalValue = signal.Value.ToString() ?? string.Empty;
            }

            if (signal.Out.Value is IFormattable outFormattable)
            {
                OutValue = outFormattable.ToString("0.00", CultureInfo.InvariantCulture) + signal.Out.Unit;
            }
            else
            {
                OutValue = signal.Out.Value.ToString() ?? string.Empty;
            }

            if (signal.Set.Value is IFormattable setFormattable)
            {
                SetValue = setFormattable.ToString(signal.Format, CultureInfo.InvariantCulture);
            }
            else
            {
                SetValue = signal.Set.Value.ToString() ?? string.Empty;
            }


            Invalidate(); // Löst Neuzeichnen aus
        }

        internal override void Paint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            var g = e.Graphics;

            using var borderPen = new Pen(BorderColor);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, Width - 2, this.ClientSize.Height - 2);
            g.DrawRectangle(borderPen, 0, 0, Width - 2, this.ClientSize.Height - 2);
            using var textBrush = new SolidBrush(ForeColor);

            float fontSize = Height / 4f;


            using var valueFont = new Font(Font.FontFamily, fontSize, FontStyle.Bold);
            using var unitFont = new Font(Font.FontFamily, fontSize, FontStyle.Italic);
            using var headerFont = new Font(Font.FontFamily, fontSize * 0.7f, FontStyle.Regular);
            using var outSetFont = new Font(Font.FontFamily, fontSize * 0.6f, FontStyle.Regular);



            RectangleF layoutValue = new RectangleF(2, (int)(Height * 0.25), Width - Height - 13, valueFont.Height);
            RectangleF layoutUnit = new RectangleF(Width - Height - 10, (int)(Height * 0.25), Height, valueFont.Height);

            RectangleF layoutOut = new RectangleF(2, Height - outSetFont.Height - 2, Width / 2 -2, outSetFont.Height);
            RectangleF layoutSet = new RectangleF(Width / 2 + 4, Height - outSetFont.Height - 2, Width / 2 - 4, outSetFont.Height);



            g.DrawString(SignalValue, valueFont, textBrush, layoutValue, RightTop);
            g.DrawString(SignalUnit, unitFont, textBrush, layoutUnit, LeftTop);
            g.DrawString("Out: " + OutValue, outSetFont, textBrush, layoutOut, LeftCenter);
            g.DrawString("Set: " + SetValue, outSetFont, textBrush, layoutSet, LeftCenter);
            g.DrawString(SignalText, headerFont, textBrush, new PointF(1, 1));

            // draw resize grip on top
            DrawResizeGrip(g);
        }


    }
    public class SimpleButton : BaseControl
    {
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalText { get; set; } = "Text";

        //  [Browsable(true)]
        //  [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        //  public string SignalUnit { get; set; } = "°C";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalValue { get; set; } = "Unknown";

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SourceName { get; set; } = "Unknown";

        public SimpleButton()
        {

            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                          | ControlStyles.OptimizedDoubleBuffer
                          | ControlStyles.UserPaint, true);
            this.BackColor = Color.White;

        }

        public override void Update()
        {
            var signal = _source as StringSignal;
            if (signal == null) return;
            SignalText = signal.Text;
            SignalValue = signal.Value.ToString() ?? string.Empty;

            Invalidate(); // Löst Neuzeichnen aus
        }

        Color backColor;
        public override Color BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                this.Invalidate();
            }
        }

        public void SaveInvoke(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        internal override void Paint(PaintEventArgs e)
        {
          //  base.OnPaint(e);
            var g = e.Graphics;


            float fontSize = Height / 4f;

            using var borderPen = new Pen(BorderColor);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, Width - 2, this.ClientSize.Height - 2);
            g.DrawRectangle(borderPen, 0, 0, Width-1, this.ClientSize.Height-1);
            using var textBrush = new SolidBrush(ForeColor);
            using var valueFont = new Font(Font.FontFamily, fontSize, FontStyle.Bold);
           

            RectangleF layoutValue = new RectangleF(2, 2, Width-4, Height-4);
         
            g.DrawString(SignalValue, valueFont, textBrush, layoutValue, CenterCenter);
  

            // draw resize grip on top
            DrawResizeGrip(g);
        }
    }
    public class Chart : BaseControl
    {
        private ScottPlot.WinForms.FormsPlot _chart = new ScottPlot.WinForms.FormsPlot();

        ChartRecorder _recorder;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public List<Signal> Signals { get; set; } = new();

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int RefreshInterval { get; set; } = 20;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int HistorySeconds { get; set; } = 120;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ViewSeconds { get; set; } = 30;

        public Chart()
        {
           
            _chart.Padding = new Padding(0);
            _chart.Margin = new Padding(0);
            _chart.BackColor = Color.White;
            Controls.Add(_chart);

            _recorder = new ChartRecorder(Name, _chart, RefreshInterval, HistorySeconds, ViewSeconds);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                          | ControlStyles.OptimizedDoubleBuffer
                          | ControlStyles.UserPaint, true);
            this.BackColor = Color.White;

            this.MouseClick += Control_MouseClick;

            LayoutChart();
            _recorder.Start();
            Play();
        }

        ushort id = 0;
        public void Add(Signal signal,int axis)
        {
            id++;
            _recorder.AddSeries(id:id, name:signal.Name, text:signal.Text, unit:signal.Unit, source: () => signal.Value, axisY:axis, axisX:1, interval:RefreshInterval);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutChart();
        }

        private void LayoutChart()
        {
            // 1px Border + Platz für Resize-Grip unten/rechts
            int border = 1;
            int reserveRight = ResizeGripSize + 2;
            int reserveBottom = ResizeGripSize + 2;

            int x = border;
            int y = border;
            int w = Math.Max(0, this.ClientSize.Width - (border + reserveRight));
            int h = Math.Max(0, this.ClientSize.Height - (border + reserveBottom + 25));

            _chart.Bounds = new Rectangle(x, y, w, h);
        }

        public override void Update()
        {
        }

        Color backColor;
        public override System.Drawing.Color BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                this.Invalidate();
            }
        }

        public void SaveInvoke(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }


        private Rectangle iconPlayRect;
        private Rectangle iconPauseRect;

        internal override void Paint(PaintEventArgs e)
        {
            var g = e.Graphics;

            using var borderPen = new Pen(BorderColor);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, Width - 2, this.ClientSize.Height - 2);
            g.DrawRectangle(borderPen, 0, 0, Width - 1, this.ClientSize.Height - 1);

            int iconSize = 25;

            iconPlayRect = new Rectangle(Width - 50, Height - 25, iconSize, iconSize);
            iconPauseRect = new Rectangle(Width - 75, Height - 25, iconSize, iconSize);

            //   var iconPlay = new Rectangle(0,0, iconSize, iconSize);
            if (_recorder.Realtime)
            {
                g.DrawImage(Icons.GetFaIcon("play", 23, Color.Orange), iconPlayRect);
                g.DrawImage(Icons.GetFaIcon("pause", 23, Color.Black), iconPauseRect);
            }
            else
            {
                g.DrawImage(Icons.GetFaIcon("play", 23, Color.Black), iconPlayRect);
                g.DrawImage(Icons.GetFaIcon("pause", 23, Color.Orange), iconPauseRect);
            }


                // Grip bleibt sichtbar, da _chart kleiner als Client ist
                DrawResizeGrip(g);
        }
        

        private void Control_MouseClick(object sender, MouseEventArgs e)
        {
            if (iconPlayRect.Contains(e.Location))
            {
                Play();
                Invalidate();
            }
            else if (iconPauseRect.Contains(e.Location))
            {
                Pause();
                Invalidate();
            }
        }


        void Play()
        {
            _recorder.Play();
            _chart.MouseClick += setFocus;

        }

        void Pause()
        {
            _chart.MouseClick -= setFocus;
            _recorder.Pause();
            
        }

        void setFocus(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("..");
            this.Focus();
        }
    }




    public class IconButton : BaseControl
    {
        private string buttonText = "Aktion";
        private string shortcutText = "Ctrl+X";
        private string signalValue = "Unknown";
        private string buttonIcon = "";
        private int iconSize = 24;
        private Image iconImage;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ButtonText
        {
            get => buttonText;
            set
            {
                buttonText = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ShortcutText
        {
            get => shortcutText;
            set
            {
                shortcutText = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SignalValue
        {
            get => signalValue;
            set
            {
                signalValue = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ButtonIcon
        {
            get => buttonIcon;
            set
            {
                Debug.WriteLine(Name + " Icon is '" + value + "'");

                if (value.ToLower().StartsWith("fa:"))
                {
                    iconSize = (int)(Height * 0.6);
                    List<string> data = value.Split(':').ToList();
                    string iconName = data.Count > 1 ? data[1] : "";
                    string colorname = data.Count > 2 ? data[2] : null;
                    iconImage = Icons.GetFaIcon(iconName, iconSize, colorname);
                }
                buttonIcon = value;
                Invalidate();
            }
        }

        private Color backColor;
        public override Color BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                Invalidate();
            }
        }

        public override void Update()
        {
            Invalidate();
        }

        public IconButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint, true);

            BackColor = Color.White;
            Size = new Size(200, 45);
        }

        internal override void Paint(PaintEventArgs e)
        {
           // base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var borderPen = new Pen(BorderColor);
            using var textBrush = new SolidBrush(ForeColor);

            float fontSize = Height / 3;
            iconSize = (int)(Height * 0.6);

            using var textFont = new Font(Font.FontFamily, fontSize, FontStyle.Regular);
            using var shortcutFont = new Font(Font.FontFamily, fontSize / 2f, FontStyle.Italic);

            g.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);
            g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

            var iconRect = new Rectangle(8, (Height - iconSize) / 2, iconSize, iconSize);
            if (iconImage != null)
            {
                g.DrawImage(iconImage, iconRect);
            }

            // Text
            var textRect = new Rectangle((int)(Height*0.8) , 0 , Width - iconRect.Right - 60, Height);
            g.DrawString(ButtonText, textFont, textBrush, textRect, LeftCenter);

            // Shortcut
            var shortcutRect = new Rectangle(iconRect.Right + 8, 0, Width - iconRect.Right - 16, Height);
            g.DrawString(ShortcutText, shortcutFont, textBrush, shortcutRect, RightCenter);

            // draw resize grip on top
            DrawResizeGrip(g);
        }

        public void SaveInvoke(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }
    }

}
