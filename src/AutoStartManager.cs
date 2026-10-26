using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AwakeWin.Core
{
    public static class AutoStartManager
    {
        private const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "AwakeWin";

        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_KEY, false))
                {
                    if (key == null) return false;
                    object val = key.GetValue(APP_NAME);
                    return val != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetAutoStart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                {
                    if (key == null) return;
                    if (enable)
                    {
                        // Inicia con el argumento --tray para no abrir la ventana al encender la PC
                        string exePath = "\"" + Application.ExecutablePath + "\" --tray";
                        key.SetValue(APP_NAME, exePath);
                    }
                    else
                    {
                        key.DeleteValue(APP_NAME, false);
                    }
                }
            }
            catch { }
        }
    }
}
