using System;
using System.Windows.Forms;

namespace AwakeWin.Core
{
    public static class SleepTimer
    {
        private static Timer _tickTimer;
        private static DateTime? _targetTime = null;

        public static event Action<string> TimerTick;
        public static event Action TimerExpired;

        public static bool IsRunning
        {
            get { return _targetTime.HasValue; }
        }

        public static void Start(int minutes)
        {
            if (minutes <= 0)
            {
                Stop();
                return;
            }

            _targetTime = DateTime.Now.AddMinutes(minutes);

            if (_tickTimer == null)
            {
                _tickTimer = new Timer { Interval = 1000 };
                _tickTimer.Tick += OnTick;
            }
            _tickTimer.Start();

            NotifyTick();
        }

        public static void Stop()
        {
            _targetTime = null;
            if (_tickTimer != null)
            {
                _tickTimer.Stop();
            }
            if (TimerTick != null)
            {
                TimerTick(null);
            }
        }

        private static void OnTick(object sender, EventArgs e)
        {
            if (!_targetTime.HasValue) return;

            TimeSpan remaining = _targetTime.Value - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                Stop();
                if (TimerExpired != null)
                {
                    TimerExpired();
                }
            }
            else
            {
                NotifyTick();
            }
        }

        private static void NotifyTick()
        {
            if (!_targetTime.HasValue) return;

            TimeSpan remaining = _targetTime.Value - DateTime.Now;
            string text;
            if (remaining.TotalHours >= 1)
            {
                text = string.Format("{0}h {1:D2}m", (int)remaining.TotalHours, remaining.Minutes);
            }
            else
            {
                text = string.Format("{0}m {1:D2}s", remaining.Minutes, remaining.Seconds);
            }

            if (TimerTick != null)
            {
                TimerTick(text);
            }
        }
    }
}
