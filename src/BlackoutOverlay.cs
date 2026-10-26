using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AwakeWin.UI
{
    public class BlackoutOverlay : Form
    {
        // WDA_EXCLUDEFROMCAPTURE (0x00000011): La ventana es visible en el monitor físico (negro puro),
        // pero es excluida de la captura de AnyDesk / Moonlight / Parsec, permitiendo streaming a 60 FPS sin bloqueos.
        private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        private Point _initialMousePos;
        private long _openTimestamp;
        private static List<BlackoutOverlay> _activeOverlays = new List<BlackoutOverlay>();

        public static bool IsActive
        {
            get { return _activeOverlays.Count > 0; }
        }

        public static event Action BlackoutDismissed;

        public static void ShowBlackout()
        {
            if (IsActive) return;

            try
            {
                Cursor.Hide();
            }
            catch { }

            foreach (Screen screen in Screen.AllScreens)
            {
                var overlay = new BlackoutOverlay(screen);
                _activeOverlays.Add(overlay);
                overlay.Show();
            }
        }

        public static void Dismiss()
        {
            if (!IsActive) return;

            try
            {
                Cursor.Show();
            }
            catch { }

            var list = new List<BlackoutOverlay>(_activeOverlays);
            _activeOverlays.Clear();

            foreach (var overlay in list)
            {
                try
                {
                    overlay.Close();
                    overlay.Dispose();
                }
                catch { }
            }

            if (BlackoutDismissed != null)
            {
                BlackoutDismissed();
            }
        }

        public BlackoutOverlay(Screen screen)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = screen.Bounds;
            this.BackColor = Color.Black;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;

            this.KeyDown += (s, e) =>
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // Consumir para evitar re-disparo de controles
                Dismiss();
            };
            this.MouseDown += (s, e) => Dismiss();
            this.MouseMove += OnMouseMoveCheck;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _initialMousePos = Cursor.Position;
            _openTimestamp = Environment.TickCount;

            try
            {
                SetWindowDisplayAffinity(this.Handle, WDA_EXCLUDEFROMCAPTURE);
            }
            catch { }
        }

        private void OnMouseMoveCheck(object sender, MouseEventArgs e)
        {
            long elapsed = Environment.TickCount - _openTimestamp;
            if (elapsed > 400)
            {
                Point current = Cursor.Position;
                int dx = Math.Abs(current.X - _initialMousePos.X);
                int dy = Math.Abs(current.Y - _initialMousePos.Y);
                if (dx > 20 || dy > 20)
                {
                    Dismiss();
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_TOOLWINDOW (evita Alt+Tab) | WS_EX_TOPMOST
                cp.ExStyle |= 0x00000080 | 0x00000008;
                return cp;
            }
        }
    }
}
