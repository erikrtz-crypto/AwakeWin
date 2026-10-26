using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AwakeWin.Core;
using AwakeWin.UI.Controls;

namespace AwakeWin.UI
{
    public class MainForm : Form
    {
        // Hotkey global (Ctrl + Alt + B)
        private const int HOTKEY_ID = 1001;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;
        private LidWatcher _lidWatcher;

        // Custom UI Controls (Fluent Windows 11)
        private HeroCard _heroCard;
        private ToggleSwitch _swAutoMute;
        private ToggleSwitch _swBatteryProtection;
        private ToggleSwitch _swThermalGuard;
        private ToggleSwitch _swAutoStart;
        private FluentComboBox _cboTimer;
        private Label _lblTimerCountdown;
        private Button _btnBlackout;
        private Label _lblStatusPill;
        private Timer _statusRefreshTimer;

        // State
        private bool _isAwakeEnabled = true;
        private bool _autoMutedByApp = false;
        private bool _startMinimized = false;

        public MainForm(bool startMinimized = false)
        {
            _startMinimized = startMinimized;

            InitializeComponent();
            LoadAppIcon();
            InitializeTray();
            InitializeLidWatcher();
            InitializeSleepTimer();

            // Despertar de pantalla automático
            DisplayWakeController.DisplayWokenUp += () =>
            {
                try
                {
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            if (_isAwakeEnabled) PowerManager.EnableAwake();
                            RefreshDashboard();
                        }));
                    }
                    else
                    {
                        if (_isAwakeEnabled) PowerManager.EnableAwake();
                        RefreshDashboard();
                    }
                }
                catch { }
            };

            // Activar Awake por defecto al iniciar
            ApplyAwakeState(true);
        }

        private void LoadAppIcon()
        {
            try
            {
                Icon appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (appIcon != null)
                {
                    this.Icon = appIcon;
                }
            }
            catch { }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.B);
            }
            catch { }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID);
            }
            catch { }
            base.OnHandleDestroyed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            PositionInBottomRightCorner();
            if (_startMinimized)
            {
                this.BeginInvoke(new Action(() => this.Hide()));
            }
        }

        private void PositionInBottomRightCorner()
        {
            try
            {
                Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
                int x = workArea.Right - this.Width - 16;
                int y = workArea.Bottom - this.Height - 16;
                this.Location = new Point(x, y);
            }
            catch { }
        }

        private void InitializeComponent()
        {
            this.Text = "AwakeWin";
            this.Size = new Size(500, 715);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.Manual;
            PositionInBottomRightCorner();
            this.BackColor = Color.FromArgb(15, 15, 18); // Deep Charcoal Slate
            this.ForeColor = Color.FromArgb(244, 244, 246);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // 1. Header Minimalista
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = Color.FromArgb(20, 20, 24)
            };
            Label lblTitle = new Label
            {
                Text = "AwakeWin",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153), // Emerald Mint
                Location = new Point(24, 14),
                AutoSize = true
            };
            Label lblSub = new Label
            {
                Text = "Tu equipo sigue trabajando aunque cierres tu dispositivo.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(161, 161, 170),
                Location = new Point(26, 46),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            this.Controls.Add(pnlHeader);

            int contentY = 94;
            int cardWidth = 436;

            // 2. Hero Card Principal
            _heroCard = new HeroCard
            {
                Location = new Point(24, contentY),
                Size = new Size(cardWidth, 78),
                IsActive = true
            };
            _heroCard.StateChanged += (s, e) => ApplyAwakeState(_heroCard.IsActive);
            this.Controls.Add(_heroCard);
            contentY += 88;

            // 3. Tarjetas de Configuración (Fluent Style)
            _swAutoMute = new ToggleSwitch { Checked = true };
            _swAutoMute.CheckedChanged += (s, e) => RefreshDashboard();
            SettingCard cardAutoMute = new SettingCard(
                "Pausar y silenciar al cerrar",
                "Pausa música/videos y silencia altavoces al bajar la pantalla",
                _swAutoMute
            ) { Location = new Point(24, contentY), Size = new Size(cardWidth, 58) };
            this.Controls.Add(cardAutoMute);
            contentY += 64;

            _swBatteryProtection = new ToggleSwitch { Checked = true };
            SettingCard cardBattery = new SettingCard(
                "Cuidado de Batería",
                "Pausa AwakeWin si la batería baja del 20% en modo portátil",
                _swBatteryProtection
            ) { Location = new Point(24, contentY), Size = new Size(cardWidth, 58) };
            this.Controls.Add(cardBattery);
            contentY += 64;

            _swThermalGuard = new ToggleSwitch { Checked = true };
            _swThermalGuard.CheckedChanged += (s, e) => ThermalGuard.IsProtectionEnabled = _swThermalGuard.Checked;
            SettingCard cardThermal = new SettingCard(
                "Mochila Segura",
                "Protege si sube el calor con la tapa cerrada en batería",
                _swThermalGuard
            ) { Location = new Point(24, contentY), Size = new Size(cardWidth, 58) };
            this.Controls.Add(cardThermal);
            contentY += 64;

            _swAutoStart = new ToggleSwitch { Checked = AutoStartManager.IsAutoStartEnabled() };
            _swAutoStart.CheckedChanged += (s, e) => AutoStartManager.SetAutoStart(_swAutoStart.Checked);
            SettingCard cardAutoStart = new SettingCard(
                "Iniciar con Windows",
                "Estar disponible junto al reloj al encender la PC",
                _swAutoStart
            ) { Location = new Point(24, contentY), Size = new Size(cardWidth, 58) };
            this.Controls.Add(cardAutoStart);
            contentY += 64;

            // 4. Tarjeta de Temporizador (con FluentComboBox compacto y sin encimarse)
            _cboTimer = new FluentComboBox
            {
                Size = new Size(150, 26)
            };
            _cboTimer.Items.AddRange(new object[] {
                "♾️ Siempre activo",
                "⏱️ 30 minutos",
                "⏱️ 1 hora",
                "⏱️ 2 horas",
                "⚙️ Personalizado..."
            });
            _cboTimer.SelectedIndex = 0;
            _cboTimer.SelectedIndexChanged += OnTimerSelectionChanged;

            SettingCard cardTimer = new SettingCard(
                "Temporizador de reposo",
                "Suspender tras un tiempo fijo",
                _cboTimer
            ) { Location = new Point(24, contentY), Size = new Size(cardWidth, 58) };
            this.Controls.Add(cardTimer);
            contentY += 64;

            _lblTimerCountdown = new Label
            {
                Text = "",
                Location = new Point(32, contentY),
                Size = new Size(cardWidth, 18),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(52, 211, 153),
                Visible = false
            };
            this.Controls.Add(_lblTimerCountdown);
            contentY += 22;

            // 5. Botón de Ocultar Pantalla (Blackout con atajo)
            _btnBlackout = new Button
            {
                Text = "🌙  Ocultar pantalla ahora  (Ctrl + Alt + B)",
                Size = new Size(cardWidth, 44),
                Location = new Point(24, contentY),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(32, 32, 36),
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            _btnBlackout.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            _btnBlackout.Click += (s, e) =>
            {
                this.ActiveControl = null;
                PowerManager.TurnOffDisplayNow(true);
            };
            this.Controls.Add(_btnBlackout);
            contentY += 50;

            // 6. Botón Configuración de Tapa en Windows
            Button btnConfigLid = new Button
            {
                Text = "⚙️  Configurar acción de la tapa en Windows",
                Size = new Size(cardWidth, 28),
                Location = new Point(24, contentY),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(161, 161, 170),
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnConfigLid.FlatAppearance.BorderSize = 0;
            btnConfigLid.Click += (s, e) =>
            {
                try
                {
                    Process.Start("control.exe", "/name Microsoft.PowerOptions /page pageGlobalSettings");
                }
                catch
                {
                    try { Process.Start("powercfg.cpl"); } catch { }
                }
            };
            this.Controls.Add(btnConfigLid);
            contentY += 36;

            // 7. Píldora de Estado Minimalista (Footer)
            Panel pnlPill = new Panel
            {
                Location = new Point(24, contentY),
                Size = new Size(cardWidth, 42),
                BackColor = Color.FromArgb(20, 20, 24)
            };
            _lblStatusPill = new Label
            {
                Text = "Iniciando...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(212, 212, 216)
            };
            pnlPill.Controls.Add(_lblStatusPill);
            this.Controls.Add(pnlPill);

            // Timer de refresco para el dashboard
            _statusRefreshTimer = new Timer { Interval = 2000 };
            _statusRefreshTimer.Tick += (s, e) => RefreshDashboard();
            _statusRefreshTimer.Start();

            // Interceptar cierre de ventana para minimizar al tray
            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                    _trayIcon.ShowBalloonTip(1500, "AwakeWin", "Sigue ejecutándose en segundo plano junto al reloj.", ToolTipIcon.Info);
                }
            };
        }

        private void InitializeSleepTimer()
        {
            SleepTimer.TimerTick += (remainingText) =>
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => UpdateTimerText(remainingText)));
                }
                else
                {
                    UpdateTimerText(remainingText);
                }
            };

            SleepTimer.TimerExpired += () =>
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        ApplyAwakeState(false);
                        _cboTimer.SelectedIndex = 0;
                        _trayIcon.ShowBalloonTip(3000, "AwakeWin", "El temporizador ha finalizado. La suspensión normal está activa.", ToolTipIcon.Info);
                    }));
                }
                else
                {
                    ApplyAwakeState(false);
                    _cboTimer.SelectedIndex = 0;
                    _trayIcon.ShowBalloonTip(3000, "AwakeWin", "El temporizador ha finalizado. La suspensión normal está activa.", ToolTipIcon.Info);
                }
            };
        }

        private void UpdateTimerText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _lblTimerCountdown.Visible = false;
            }
            else
            {
                _lblTimerCountdown.Text = "⏳ Tiempo restante: " + text;
                _lblTimerCountdown.Visible = true;
            }
        }

        private void OnTimerSelectionChanged(object sender, EventArgs e)
        {
            int idx = _cboTimer.SelectedIndex;
            if (idx == 0)
            {
                SleepTimer.Stop();
            }
            else if (idx == 1)
            {
                SleepTimer.Start(30);
            }
            else if (idx == 2)
            {
                SleepTimer.Start(60);
            }
            else if (idx == 3)
            {
                SleepTimer.Start(120);
            }
            else if (idx == 4)
            {
                using (CustomTimeDialog dlg = new CustomTimeDialog())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.TotalMinutes > 0)
                    {
                        SleepTimer.Start(dlg.TotalMinutes);
                    }
                    else
                    {
                        _cboTimer.SelectedIndex = 0;
                    }
                }
            }
        }

        private void InitializeTray()
        {
            _trayMenu = new ContextMenuStrip();
            var itemToggle = _trayMenu.Items.Add("⚡ Activar / Pausar AwakeWin", null, (s, e) => ToggleAwake());
            itemToggle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            _trayMenu.Items.Add("🌙 Ocultar pantalla ahora (Ctrl+Alt+B)", null, (s, e) => PowerManager.TurnOffDisplayNow(true));
            _trayMenu.Items.Add(new ToolStripSeparator());

            var menuTimer = new ToolStripMenuItem("⏱️ Temporizador");
            menuTimer.DropDownItems.Add("♾️ Siempre activo", null, (s, e) => { _cboTimer.SelectedIndex = 0; });
            menuTimer.DropDownItems.Add("⏱️ 30 minutos", null, (s, e) => { _cboTimer.SelectedIndex = 1; });
            menuTimer.DropDownItems.Add("⏱️ 1 hora", null, (s, e) => { _cboTimer.SelectedIndex = 2; });
            menuTimer.DropDownItems.Add("⏱️ 2 horas", null, (s, e) => { _cboTimer.SelectedIndex = 3; });
            menuTimer.DropDownItems.Add("⚙️ Personalizado...", null, (s, e) => { _cboTimer.SelectedIndex = 4; });
            _trayMenu.Items.Add(menuTimer);

            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("⚙️ Abrir Panel", null, (s, e) => ShowAndRestore());
            _trayMenu.Items.Add("❌ Salir", null, (s, e) => ExitApp());

            _trayIcon = new NotifyIcon
            {
                Text = "AwakeWin",
                Icon = CreateStatusIcon(true),
                ContextMenuStrip = _trayMenu,
                Visible = true
            };

            _trayIcon.DoubleClick += (s, e) => ShowAndRestore();
            _trayIcon.Click += (s, e) =>
            {
                var me = e as MouseEventArgs;
                if (me != null && me.Button == MouseButtons.Left)
                {
                    ShowAndRestore();
                }
            };
        }

        private void InitializeLidWatcher()
        {
            _lidWatcher = new LidWatcher();
            _lidWatcher.LidStateChanged += OnLidStateChanged;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if (BlackoutOverlay.IsActive)
                {
                    PowerManager.WakeDisplayNow();
                }
                else
                {
                    PowerManager.TurnOffDisplayNow(true);
                }
                return;
            }
            base.WndProc(ref m);
        }

        private void OnLidStateChanged(bool isOpen)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnLidStateChanged(isOpen)));
                return;
            }

            if (!_isAwakeEnabled) return;

            if (!isOpen)
            {
                if (_swAutoMute != null && _swAutoMute.Checked)
                {
                    MediaController.PauseMedia();
                    AudioController.SetMute(true);
                    _autoMutedByApp = true;
                }
            }
            else
            {
                DisplayWakeController.WakeDisplay();

                if (_autoMutedByApp)
                {
                    AudioController.SetMute(false);
                    MediaController.ResumeMedia();
                    _autoMutedByApp = false;
                }
            }

            RefreshDashboard();
        }

        private void ToggleAwake()
        {
            ApplyAwakeState(!_isAwakeEnabled);
        }

        private void ApplyAwakeState(bool enable)
        {
            _isAwakeEnabled = enable;
            if (_heroCard != null && _heroCard.IsActive != enable)
            {
                _heroCard.IsActive = enable;
            }

            if (enable)
            {
                PowerManager.EnableAwake();
                _trayIcon.Icon = CreateStatusIcon(true);
                _trayIcon.Text = "AwakeWin: ACTIVADO\nTareas y streaming a 60 FPS activos";
            }
            else
            {
                PowerManager.DisableAwake();
                _trayIcon.Icon = CreateStatusIcon(false);
                _trayIcon.Text = "AwakeWin: EN PAUSA\nSuspensión permitida por Windows";

                if (_autoMutedByApp)
                {
                    AudioController.SetMute(false);
                    _autoMutedByApp = false;
                }
            }

            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            PowerStatus ps = SystemInformation.PowerStatus;
            int pct = (int)(ps.BatteryLifePercent * 100);
            bool isAC = (ps.PowerLineStatus == PowerLineStatus.Online);

            bool isLidClosed = (_lidWatcher != null && _lidWatcher.CurrentLidOpenState.HasValue && !_lidWatcher.CurrentLidOpenState.Value);
            bool isMuted = AudioController.IsMuted();

            string statusText = _isAwakeEnabled ? "🟢 Activo" : "⚪ En pausa";
            string batStr = string.Format("Batería: {0}% {1}", pct, isAC ? "⚡" : "").Trim();

            string audioIconStr;
            if (_swAutoMute != null && _swAutoMute.Checked)
            {
                audioIconStr = isMuted ? "🔇 Pausado y silenciado" : "🔊 Audio silenciado al cerrar";
            }
            else
            {
                audioIconStr = "🔊 Sonido siempre activo";
            }

            if (_lblStatusPill != null)
            {
                _lblStatusPill.Text = string.Format("{0}   •   {1}   •   {2}", statusText, batStr, audioIconStr);
                _lblStatusPill.ForeColor = _isAwakeEnabled ? Color.FromArgb(52, 211, 153) : Color.FromArgb(161, 161, 170);
            }

            // Salvaguarda 1: Batería baja (< 20%)
            if (!isAC && _swBatteryProtection != null && _swBatteryProtection.Checked && pct < 20 && _isAwakeEnabled)
            {
                ApplyAwakeState(false);
                _trayIcon.ShowBalloonTip(3000, "AwakeWin - Batería Baja", "Se pausó AwakeWin para proteger tu batería (< 20%).", ToolTipIcon.Warning);
            }

            // Salvaguarda 2: Mochila Segura (Calor / Carga alta en batería con tapa cerrada)
            string thermalReason;
            if (_isAwakeEnabled && ThermalGuard.CheckRisk(isLidClosed, !isAC, out thermalReason))
            {
                ApplyAwakeState(false);
                _trayIcon.ShowBalloonTip(4000, "AwakeWin - Mochila Segura", thermalReason, ToolTipIcon.Warning);
            }
        }

        private void ShowAndRestore()
        {
            PositionInBottomRightCorner();
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        private void ExitApp()
        {
            DisplayWakeController.WakeDisplay();
            PowerManager.DisableAwake();
            if (_autoMutedByApp)
            {
                AudioController.SetMute(false);
            }
            if (_lidWatcher != null)
            {
                _lidWatcher.Dispose();
            }
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }

        private Icon CreateStatusIcon(bool active)
        {
            using (Bitmap bmp = new Bitmap(32, 32))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (Brush bgBrush = new SolidBrush(Color.FromArgb(24, 24, 27)))
                {
                    g.FillEllipse(bgBrush, 2, 2, 28, 28);
                }

                using (Pen borderPen = new Pen(active ? Color.FromArgb(52, 211, 153) : Color.FromArgb(107, 114, 128), 2))
                {
                    g.DrawEllipse(borderPen, 2, 2, 28, 28);
                }

                Color ledColor = active ? Color.FromArgb(16, 185, 129) : Color.FromArgb(156, 163, 175);
                using (Brush ledBrush = new SolidBrush(ledColor))
                {
                    g.FillEllipse(ledBrush, 9, 9, 14, 14);
                }

                if (active)
                {
                    using (Brush shineBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                    {
                        g.FillEllipse(shineBrush, 12, 11, 5, 4);
                    }
                }

                return Icon.FromHandle(bmp.GetHicon());
            }
        }
    }
}
