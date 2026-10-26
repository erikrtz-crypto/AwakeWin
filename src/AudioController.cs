using System;
using System.Runtime.InteropServices;

namespace AwakeWin.Core
{
    public static class AudioController
    {
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumeratorComObject { }

        private enum EDataFlow { eRender, eCapture, eAll }
        private enum ERole { eConsole, eMultimedia, eCommunications }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IntPtr ppDevices);
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
            int GetDevice(string pwstrId, out IMMDevice ppDevice);
            int RegisterEndpointNotificationCallback(IntPtr pClient);
            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            int GetState(out int pdwState);
        }

        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            int RegisterControlChangeNotify(IntPtr pNotify);
            int UnregisterControlChangeNotify(IntPtr pNotify);
            int GetChannelCount(out int pnChannelCount);
            int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
            int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
            int GetMasterVolumeLevel(out float pfLevelDB);
            int GetMasterVolumeLevelScalar(out float pfLevel);
            int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
            int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
            int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
            int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
            int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
            int VolumeStepUp(ref Guid pguidEventContext);
            int VolumeStepDown(ref Guid pguidEventContext);
            int QueryHardwareSupport(out uint pdwHardwareSupportMask);
            int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        private static IAudioEndpointVolume GetVolumeEndpoint(ERole role)
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice dev;
                int hr = enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, role, out dev);
                if (hr != 0 || dev == null) return null;

                Guid iid = typeof(IAudioEndpointVolume).GUID;
                object obj;
                dev.Activate(ref iid, 1, IntPtr.Zero, out obj);
                return obj as IAudioEndpointVolume;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsMuted()
        {
            try
            {
                var epMulti = GetVolumeEndpoint(ERole.eMultimedia);
                if (epMulti != null)
                {
                    bool muted;
                    epMulti.GetMute(out muted);
                    return muted;
                }
                var epConsole = GetVolumeEndpoint(ERole.eConsole);
                if (epConsole != null)
                {
                    bool muted;
                    epConsole.GetMute(out muted);
                    return muted;
                }
            }
            catch { }
            return false;
        }

        public static bool SetMute(bool mute)
        {
            bool anySuccess = false;
            Guid empty = Guid.Empty;

            // Mutear todos los roles de audio activos (Multimedia, Consola de juegos/navegador y Comunicaciones)
            ERole[] roles = new ERole[] { ERole.eMultimedia, ERole.eConsole, ERole.eCommunications };
            foreach (ERole role in roles)
            {
                try
                {
                    var ep = GetVolumeEndpoint(role);
                    if (ep != null)
                    {
                        int hr = ep.SetMute(mute, ref empty);
                        if (hr == 0) anySuccess = true;
                    }
                }
                catch { }
            }

            return anySuccess;
        }
    }
}
