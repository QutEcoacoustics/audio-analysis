using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CFRecorder
{

    class PDA
    {
        #region Declaration
        const int SETPOWERMANAGEMENT = 6147;

        [DllImport("coredll.dll", EntryPoint = "ExtEscape")]
        static extern Int32 ExtEscapeSet(IntPtr hdc, Int32 nEscape, Int32 cbInput, byte[] plszInData, Int32 cbOutput, IntPtr lpszOutData);

        [DllImport("coredll.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("Coredll.dll")]
        private static extern int KernelIoControl(int dwIoControlCode, IntPtr lpInBuf, int nInBufSize, IntPtr lpOutBuf, int nOutBufSize, ref int lpBytesReturned);

                

        private enum VideoPowerState : int
        {

            VideoPowerOn = 1,

            VideoPowerStandBy,

            VideoPowerSuspend,

            VideoPowerOff,
        }

        private const int FILE_DEVICE_HAL = 257;

        private const int METHOD_BUFFERED = 0;

        private const int FILE_ANY_ACCESS = 0;
               
        static int CTL_CODE(int DeviceType, int Func, int Method, int Access)
        {
            return (DeviceType << 16) | (Access << 14) | (Func << 2) | Method;
        }    
#endregion

        public static void PowerOffScreen()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            byte[] vpm = { 12, 0, 0, 0, 1, 0, 0, 0, (byte)VideoPowerState.VideoPowerOff, 0, 0, 0, 0 };
            ExtEscapeSet(hdc, SETPOWERMANAGEMENT, 12, vpm, 0, IntPtr.Zero);
        }

        public static void PowerOnScreen()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            byte[] vpm = {12, 0, 0, 0, 1, 0, 0, 0, (byte)VideoPowerState.VideoPowerOn, 0, 0, 0, 0};
            ExtEscapeSet(hdc, SETPOWERMANAGEMENT, 12, vpm, 0, IntPtr.Zero);
        }

        public static int SoftReset()
        {
            int bytesReturned = 0;
            int IOCTL_HAL_REBOOT = CTL_CODE(FILE_DEVICE_HAL, 15, METHOD_BUFFERED, FILE_ANY_ACCESS);
            return KernelIoControl(IOCTL_HAL_REBOOT, IntPtr.Zero, 0, IntPtr.Zero, 0, ref bytesReturned);
        }

        public static void logError()
        {
            // TODO: Create a webservice in server to keep warning and error from the sensor.
        }
    }
}
