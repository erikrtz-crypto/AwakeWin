using System;
using System.Threading;
using System.Windows.Forms;
using AwakeWin.UI;

namespace AwakeWin
{
    static class Program
    {
        private static Mutex _singleInstanceMutex;

        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            _singleInstanceMutex = new Mutex(true, "AwakeWin_SingleInstance_Mutex_App", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("AwakeWin ya está ejecutándose en la bandeja del sistema (junto al reloj).",
                                "AwakeWin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool startMinimized = false;
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--tray" || args[i] == "--minimized")
                    {
                        startMinimized = true;
                        break;
                    }
                }
            }

            try
            {
                Application.Run(new MainForm(startMinimized));
            }
            finally
            {
                if (_singleInstanceMutex != null)
                {
                    _singleInstanceMutex.ReleaseMutex();
                    _singleInstanceMutex.Close();
                }
            }
        }
    }
}
