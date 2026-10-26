using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace AwakeWin.Core
{
    public static class ThermalGuard
    {
        private static PerformanceCounter _cpuCounter;
        private static int _highLoadConsecutiveChecks = 0;

        public static bool IsProtectionEnabled { get; set; }

        static ThermalGuard()
        {
            IsProtectionEnabled = true;
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue();
            }
            catch { }
        }

        // Evalúa las condiciones de riesgo: Tapa cerrada + Modo Batería + Carga pesada continua
        public static bool CheckRisk(bool isLidClosed, bool isOnBattery, out string reason)
        {
            reason = null;
            if (!IsProtectionEnabled) return false;

            // Si está conectado al cargador de pared o la tapa está abierta, no hay riesgo de mochila
            if (!isLidClosed || !isOnBattery)
            {
                _highLoadConsecutiveChecks = 0;
                return false;
            }

            // Si está cerrada y en batería, comprobamos el uso de la CPU
            if (_cpuCounter != null)
            {
                try
                {
                    float usage = _cpuCounter.NextValue();
                    if (usage > 80.0f)
                    {
                        _highLoadConsecutiveChecks++;
                        // Si supera el 80% de CPU de manera sostenida (por 3 revisiones = ~10 segundos) con la tapa cerrada en batería
                        if (_highLoadConsecutiveChecks >= 3)
                        {
                            reason = "Se detectó alto uso de procesador con la tapa cerrada en modo batería (riesgo de mochila).";
                            return true;
                        }
                    }
                    else
                    {
                        if (_highLoadConsecutiveChecks > 0) _highLoadConsecutiveChecks--;
                    }
                }
                catch { }
            }

            return false;
        }
    }
}
