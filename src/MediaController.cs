using System;
using System.Runtime.InteropServices;

namespace AwakeWin.Core
{
    public static class MediaController
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static void TogglePlayPause()
        {
            try
            {
                // Envía la pulsación estándar de la tecla multimedia Play/Pause de Windows
                keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, UIntPtr.Zero);
                keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            catch { }
        }

        public static void PauseMedia()
        {
            TogglePlayPause();
        }

        public static void ResumeMedia()
        {
            TogglePlayPause();
        }
    }
}
