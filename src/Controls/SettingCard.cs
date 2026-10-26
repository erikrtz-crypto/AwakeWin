using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AwakeWin.UI.Controls
{
    public class SettingCard : Panel
    {
        private bool _isHovered = false;
        private string _title = "";
        private string _subtitle = "";
        private Control _rightControl = null;

        public string Title
        {
            get { return _title; }
            set { _title = value; this.Invalidate(); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { _subtitle = value; this.Invalidate(); }
        }

        public SettingCard(string title, string subtitle, Control rightControl)
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            _title = title;
            _subtitle = subtitle;
            _rightControl = rightControl;

            this.Height = 58;
            this.BackColor = Color.FromArgb(24, 24, 27);
            this.Cursor = Cursors.Default;

            if (_rightControl != null)
            {
                this.Controls.Add(_rightControl);
                this.PositionRightControl();
            }

            this.MouseEnter += (s, e) => { _isHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { _isHovered = false; this.Invalidate(); };
            this.Resize += (s, e) => this.PositionRightControl();
        }

        private void PositionRightControl()
        {
            if (_rightControl != null)
            {
                int rightMargin = 16;
                int top = (this.Height - _rightControl.Height) / 2;
                _rightControl.Location = new Point(this.Width - _rightControl.Width - rightMargin, top);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = this.Width - 1;
            int h = this.Height - 1;
            int r = 8;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(w - r, 0, r, r, 270, 90);
                path.AddArc(w - r, h - r, r, r, 0, 90);
                path.AddArc(0, h - r, r, r, 90, 90);
                path.CloseFigure();

                Color bg = _isHovered ? Color.FromArgb(32, 32, 36) : Color.FromArgb(24, 24, 27);
                using (SolidBrush brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }

                using (Pen borderPen = new Pen(Color.FromArgb(39, 39, 42), 1f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Delimitar el área máxima de texto para que NUNCA colisione ni se encime con el control derecho
            float rightBound = (_rightControl != null) ? (_rightControl.Left - 16) : (this.Width - 16);
            float maxTextWidth = Math.Max(100, rightBound - 16);

            RectangleF titleRect = new RectangleF(16, 9, maxTextWidth, 20);
            RectangleF subRect = new RectangleF(16, 29, maxTextWidth, 20);

            using (StringFormat sf = new StringFormat(StringFormatFlags.NoWrap))
            {
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.LineAlignment = StringAlignment.Center;

                using (Font titleFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(244, 244, 246)))
                {
                    g.DrawString(_title, titleFont, titleBrush, titleRect, sf);
                }

                if (!string.IsNullOrEmpty(_subtitle))
                {
                    using (Font subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
                    using (SolidBrush subBrush = new SolidBrush(Color.FromArgb(161, 161, 170)))
                    {
                        g.DrawString(_subtitle, subFont, subBrush, subRect, sf);
                    }
                }
            }
        }
    }
}
