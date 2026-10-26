using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AwakeWin.UI.Controls
{
    public class ToggleSwitch : Control
    {
        private bool _checked = false;
        private bool _isHovered = false;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    this.Invalidate();
                    if (CheckedChanged != null)
                    {
                        CheckedChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public ToggleSwitch()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.Size = new Size(46, 24);
            this.Cursor = Cursors.Hand;
            this.TabStop = true;
            this.BackColor = Color.Transparent;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                this.Checked = !this.Checked;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                this.Checked = !this.Checked;
                e.Handled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int w = this.Width - 1;
            int h = this.Height - 1;
            int r = h;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, r, r, 90, 180);
                path.AddArc(w - r, 0, r, r, 270, 180);
                path.CloseFigure();

                if (_checked)
                {
                    Color fill = _isHovered ? Color.FromArgb(16, 185, 129) : Color.FromArgb(5, 150, 105);
                    using (SolidBrush brush = new SolidBrush(fill))
                    {
                        g.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(Color.FromArgb(52, 211, 153), 1.2f))
                    {
                        g.DrawPath(pen, path);
                    }

                    // Knob ON (derecha)
                    float knobX = w - h + 3f;
                    float knobSize = h - 6f;
                    using (SolidBrush knobBrush = new SolidBrush(Color.White))
                    {
                        g.FillEllipse(knobBrush, knobX, 3f, knobSize, knobSize);
                    }
                }
                else
                {
                    Color fill = _isHovered ? Color.FromArgb(55, 65, 81) : Color.FromArgb(39, 39, 42);
                    using (SolidBrush brush = new SolidBrush(fill))
                    {
                        g.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(Color.FromArgb(75, 85, 99), 1.2f))
                    {
                        g.DrawPath(pen, path);
                    }

                    // Knob OFF (izquierda)
                    float knobX = 3f;
                    float knobSize = h - 6f;
                    using (SolidBrush knobBrush = new SolidBrush(Color.FromArgb(209, 213, 219)))
                    {
                        g.FillEllipse(knobBrush, knobX, 3f, knobSize, knobSize);
                    }
                }
            }
        }
    }
}
