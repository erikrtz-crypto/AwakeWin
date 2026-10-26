using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AwakeWin.UI;

namespace AwakeWin.Core
{
    public static class DisplayWakeController
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MOUSEMOVE = 0x0200;

        private const int HWND_BROADCAST = 0xFFFF;
        private const uint WM_SYSCOMMAND = 0x0112;
        private static readonly IntPtr SC_MONITORPOWER = new IntPtr(0xF170);

        private const uint MOUSEEVENTF_MOVE = 0x0001;

        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        private static IntPtr _keyboardHook = IntPtr.Zero;
        private static IntPtr _mouseHook = IntPtr.Zero;
        private static LowLevelProc _keyboardProc;
        private static LowLevelProc _mouseProc;

        private static POINT _initialMousePos;
        private static long _turnOffTimestamp = 0;
        private static long _lastWakeTimestamp = 0;
        public static bool IsDisplayOff { get; private set; }
        private static bool _wasHardwareOff = false;

        public static event Action DisplayWokenUp;

        public static void TurnOffDisplay(bool useBlackoutMode = true)
        {
            // Cooldown de 800ms tras despertar: evita bucles si se pulsa Enter repetidamente
            if (Environment.TickCount - _lastWakeTimestamp < 800) return;
            if (IsDisplayOff || BlackoutOverlay.IsActive) return;

            IsDisplayOff = true;
            _wasHardwareOff = !useBlackoutMode;

            GetCursorPos(out _initialMousePos);
            _turnOffTimestamp = Environment.TickCount;

            InstallHooks();

            if (useBlackoutMode)
            {
                BlackoutOverlay.ShowBlackout();
            }
            else
            {
                PostMessage((IntPtr)HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, (IntPtr)2);
            }
        }

        public static void WakeDisplay()
        {
            if (!IsDisplayOff && !BlackoutOverlay.IsActive) return;

            _lastWakeTimestamp = Environment.TickCount;
            IsDisplayOff = false;

            UninstallHooks();

            // 1. Quitar overlay negro de inmediato si estaba activo
            if (BlackoutOverlay.IsActive)
            {
                BlackoutOverlay.Dismiss();
            }

            // 2. Si fue apagado físico por hardware, encender la pantalla
            if (_wasHardwareOff)
            {
                PowerManager.EnableDisplay();
                PostMessage((IntPtr)HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, (IntPtr)(-1));
                mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero);
                mouse_event(MOUSEEVENTF_MOVE, 0, -1, 0, UIntPtr.Zero);
                _wasHardwareOff = false;
            }

            if (DisplayWokenUp != null)
            {
                DisplayWokenUp();
            }
        }

        private static void InstallHooks()
        {
            if (_keyboardHook == IntPtr.Zero)
            {
                _keyboardProc = KeyboardHookCallback;
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    IntPtr hMod = GetModuleHandle(curModule.ModuleName);
                    _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, hMod, 0);
                }
            }

            if (_mouseHook == IntPtr.Zero)
            {
                _mouseProc = MouseHookCallback;
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    IntPtr hMod = GetModuleHandle(curModule.ModuleName);
                    _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, hMod, 0);
                }
            }
        }

        private static void UninstallHooks()
        {
            if (_keyboardHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHook);
                _keyboardHook = IntPtr.Zero;
            }
            if (_mouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHook);
                _mouseHook = IntPtr.Zero;
            }
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    if (IsDisplayOff || BlackoutOverlay.IsActive)
                    {
                        WakeDisplay();
                        // CONSUMIR LA TECLA: Retornar (IntPtr)1 para que la tecla (como Enter o Espacio)
                        // no sea entregada al botón que tenía el foco en la app, evitando el bucle de re-apagado.
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                long elapsed = Environment.TickCount - _turnOffTimestamp;

                if (elapsed > 400)
                {
                    if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN || msg == WM_MBUTTONDOWN)
                    {
                        if (IsDisplayOff || BlackoutOverlay.IsActive)
                        {
                            WakeDisplay();
                            return (IntPtr)1; // Consumir el clic
                        }
                    }
                    else if (msg == WM_MOUSEMOVE)
                    {
                        POINT current;
                        GetCursorPos(out current);
                        int dx = Math.Abs(current.X - _initialMousePos.X);
                        int dy = Math.Abs(current.Y - _initialMousePos.Y);
                        if (dx > 20 || dy > 20)
                        {
                            WakeDisplay();
                        }
                    }
                }
            }
            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }
    }
}
