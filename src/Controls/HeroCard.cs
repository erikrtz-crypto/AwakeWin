using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AwakeWin.UI.Controls
{
    public class HeroCard : Panel
    {
        private bool _isActive = true;
        private ToggleSwitch _toggle;

        public event EventHandler StateChanged;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_toggle != null && _toggle.Checked != value)
                    {
                        _toggle.Checked = value;
                    }
                    this.Invalidate();
                    if (StateChanged != null)
                    {
                        StateChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public HeroCard()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            this.Height = 78;
            this.BackColor = Color.FromArgb(24, 24, 27);
            this.Cursor = Cursors.Hand;

            _toggle = new ToggleSwitch
            {
                Size = new Size(50, 26),
                Checked = true
            };
            _toggle.CheckedChanged += (s, e) =>
            {
                this.IsActive = _toggle.Checked;
            };

            this.Controls.Add(_toggle);
            this.PositionToggle();
            this.Resize += (s, e) => this.PositionToggle();

            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.IsActive = !this.IsActive;
                }
            };
        }

        private void PositionToggle()
        {
            if (_toggle != null)
            {
                _toggle.Location = new Point(this.Width - _toggle.Width - 20, (this.Height - _toggle.Height) / 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = this.Width - 1;
            int h = this.Height - 1;
            int r = 10;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(w - r, 0, r, r, 270, 90);
                path.AddArc(w - r, h - r, r, r, 0, 90);
                path.AddArc(0, h - r, r, r, 90, 90);
                path.CloseFigure();

                Color bg = _isActive ? Color.FromArgb(6, 78, 59) : Color.FromArgb(24, 24, 27); // Deep Emerald o Dark Slate
                using (SolidBrush brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }

                Color borderColor = _isActive ? Color.FromArgb(16, 185, 129) : Color.FromArgb(39, 39, 42);
                using (Pen borderPen = new Pen(borderColor, _isActive ? 1.5f : 1f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Indicador LED circular
            float ledX = 20;
            float ledY = (this.Height - 14) / 2f;
            Color ledColor = _isActive ? Color.FromArgb(52, 211, 153) : Color.FromArgb(113, 113, 122);
            using (SolidBrush ledBrush = new SolidBrush(ledColor))
            {
                g.FillEllipse(ledBrush, ledX, ledY, 14, 14);
            }

            if (_isActive)
            {
                // Halo sutil alrededor del LED
                using (Pen haloPen = new Pen(Color.FromArgb(80, 16, 185, 129), 2f))
                {
                    g.DrawEllipse(haloPen, ledX - 3, ledY - 3, 20, 20);
                }
            }

            // Textos
            float textX = 46;
            string title = _isActive ? "MODO AWAKEWIN ACTIVADO" : "EN PAUSA (SUSPENSIÓN NORMAL)";
            string sub = _isActive ? "Equipo siempre activo • No suspender al cerrar" : "La laptop dormirá según tu configuración habitual";

            using (Font titleFont = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(_isActive ? Color.FromArgb(52, 211, 153) : Color.FromArgb(228, 228, 231)))
            {
                g.DrawString(title, titleFont, titleBrush, new PointF(textX, 18));
            }

            using (Font subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (SolidBrush subBrush = new SolidBrush(Color.FromArgb(161, 161, 170)))
            {
                g.DrawString(sub, subFont, subBrush, new PointF(textX, 42));
            }
        }
    }
}
