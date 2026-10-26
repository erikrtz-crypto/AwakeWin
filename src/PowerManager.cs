using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AwakeWin.Core
{
    public static class PowerManager
    {
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED   = 0x00000001,
            ES_DISPLAY_REQUIRED  = 0x00000002,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS        = 0x80000000
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        public static bool IsAwakeActive { get; private set; }

        public static void EnableAwake(bool keepDisplayOn = false)
        {
            EXECUTION_STATE flags = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;

            if (keepDisplayOn)
            {
                flags |= EXECUTION_STATE.ES_DISPLAY_REQUIRED;
            }

            SetThreadExecutionState(flags);
            IsAwakeActive = true;
            DisablePowerThrottling();
        }

        public static void EnableDisplay()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | 
                                    EXECUTION_STATE.ES_SYSTEM_REQUIRED | 
                                    EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        }

        public static void DisableAwake()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            IsAwakeActive = false;
        }

        public static void TurnOffDisplayNow(bool useBlackoutMode = true)
        {
            DisplayWakeController.TurnOffDisplay(useBlackoutMode);
        }

        public static void WakeDisplayNow()
        {
            DisplayWakeController.WakeDisplay();
        }

        private static void DisablePowerThrottling()
        {
            try
            {
                Process current = Process.GetCurrentProcess();
                current.PriorityClass = ProcessPriorityClass.High;
            }
            catch { }
        }
    }
}
