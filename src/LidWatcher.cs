using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AwakeWin.Core
{
    public class LidWatcher : NativeWindow, IDisposable
    {
        // GUIDs oficiales de Microsoft Windows SDK (winnt.h)
        // 1. Sensor de bisagra física de la laptop (Lid Switch)
        public static readonly Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid("BA3E0F4D-B817-4094-A2D1-D56379E6A0F3");
        
        // 2. Estado de la pantalla física de consola (Se apaga siempre que bajas la tapa)
        public static readonly Guid GUID_CONSOLE_DISPLAY_STATE  = new Guid("6FE69556-704A-47A0-8F24-C28D936FDA47");
        
        // 3. Estado de pantalla de la sesión de usuario
        public static readonly Guid GUID_SESSION_DISPLAY_STATUS = new Guid("2B84C20E-AD23-4DDF-93DB-05FFBD7EFCA5");
        
        // 4. Alimentación del monitor
        public static readonly Guid GUID_MONITOR_POWER_ON       = new Guid("02731015-4510-4526-99E6-E5A17EBD1AEA");

        public const int WM_POWERBROADCAST = 0x0218;
        public const int PBT_POWERSETTINGCHANGE = 0x8013;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public int DataLength;
            public byte Data;
        }

        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr RegisterPowerSettingNotification(
            IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

        private List<IntPtr> _registeredNotifications = new List<IntPtr>();

        public event Action<bool> LidStateChanged; // true = Pantalla abierta/encendida, false = Pantalla cerrada/apagada

        public bool? CurrentLidOpenState { get; private set; }

        public LidWatcher()
        {
            // Crear ventana nativa permanente para capturar eventos de energía del kernel
            CreateParams cp = new CreateParams { Caption = "AwakeWinPowerListener" };
            this.CreateHandle(cp);

            RegisterNotifications();
        }

        private void RegisterNotifications()
        {
            RegisterGuid(GUID_LIDSWITCH_STATE_CHANGE);
            RegisterGuid(GUID_CONSOLE_DISPLAY_STATE);
            RegisterGuid(GUID_SESSION_DISPLAY_STATUS);
            RegisterGuid(GUID_MONITOR_POWER_ON);
        }

        private void RegisterGuid(Guid guid)
        {
            try
            {
                IntPtr hNotify = RegisterPowerSettingNotification(this.Handle, ref guid, DEVICE_NOTIFY_WINDOW_HANDLE);
                if (hNotify != IntPtr.Zero)
                {
                    _registeredNotifications.Add(hNotify);
                }
            }
            catch { }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_POWERSETTINGCHANGE)
            {
                try
                {
                    POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));
                    int state = Marshal.ReadInt32(m.LParam, 20);

                    if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                    {
                        // 0 = Tapa cerrada, 1 = Tapa abierta
                        bool isOpen = (state != 0);
                        ProcessStateChange(isOpen);
                    }
                    else if (ps.PowerSetting == GUID_CONSOLE_DISPLAY_STATE ||
                             ps.PowerSetting == GUID_SESSION_DISPLAY_STATUS ||
                             ps.PowerSetting == GUID_MONITOR_POWER_ON)
                    {
                        // 0 = Pantalla apagada (tapa bajada) -> Silenciar
                        // 1 = Pantalla encendida (tapa levantada) -> Restaurar
                        if (state == 0)
                        {
                            ProcessStateChange(false);
                        }
                        else if (state == 1)
                        {
                            ProcessStateChange(true);
                        }
                    }
                }
                catch { }
            }
            base.WndProc(ref m);
        }

        private void ProcessStateChange(bool isOpenOrOn)
        {
            CurrentLidOpenState = isOpenOrOn;
            if (LidStateChanged != null)
            {
                LidStateChanged(isOpenOrOn);
            }
        }

        public void Dispose()
        {
            foreach (var h in _registeredNotifications)
            {
                try { UnregisterPowerSettingNotification(h); } catch { }
            }
            _registeredNotifications.Clear();
            this.DestroyHandle();
        }
    }
}
