using System.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OpenNETCF;
using OpenNETCF.IO;
using Microsoft.WindowsMobile;

namespace PDA
{

    public partial class Video
    {
        #region Declaration
        const int SETPOWERMANAGEMENT = 6147;

        [DllImport("coredll.dll", EntryPoint = "ExtEscape")]
        static extern Int32 ExtEscapeSet(IntPtr hdc, Int32 nEscape, Int32 cbInput, byte[] plszInData, Int32 cbOutput, IntPtr lpszOutData);

        [DllImport("coredll.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
              
        private enum VideoPowerState : int
        {

            VideoPowerOn = 1,

            VideoPowerStandBy,

            VideoPowerSuspend,

            VideoPowerOff,
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
    }

    public partial class Hardware
    {
        private const int FILE_DEVICE_HAL = 257;

        private const int METHOD_BUFFERED = 0;

        private const int FILE_ANY_ACCESS = 0;

        static int CTL_CODE(int DeviceType, int Func, int Method, int Access)
        {
            return (DeviceType << 16) | (Access << 14) | (Func << 2) | Method;
        }

        [DllImport("Coredll.dll")]
        private static extern int KernelIoControl(int dwIoControlCode, IntPtr lpInBuf, int nInBufSize, IntPtr lpOutBuf, int nOutBufSize, ref int lpBytesReturned);

        public static int SoftReset()
        {
            int bytesReturned = 0;
            int IOCTL_HAL_REBOOT = CTL_CODE(FILE_DEVICE_HAL, 15, METHOD_BUFFERED, FILE_ANY_ACCESS);
            return KernelIoControl(IOCTL_HAL_REBOOT, IntPtr.Zero, 0, IntPtr.Zero, 0, ref bytesReturned);
        }

        public class SYSTEM_POWER_STATUS_EX
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte Reserved1;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
            public byte Reserved2;
            public byte BackupBatteryFlag;
            public byte BackupBatteryLifePercent;
            public byte Reserved3;
            public uint BackupBatteryLifeTime;
            public uint BackupBatteryFullLifeTime;
        }

        [DllImport("Coredll")]
        private static extern uint GetSystemPowerStatusEx(SYSTEM_POWER_STATUS_EX lpSystemPowerStatus,
            bool fUpdate);

        public static byte GetBatteryLeftPercentage()
        {
            //SYSTEM_POWER_STATUS_EX powerStatus = new SYSTEM_POWER_STATUS_EX();
            //GetSystemPowerStatusEx(powerStatus, true);            
            //return powerStatus.BatteryLifePercent;

            return Convert.ToByte(Microsoft.WindowsMobile.Status.SystemState.GetValue(Microsoft.WindowsMobile.Status.SystemProperty.PowerBatteryStrength));
        }


        public enum DevicePowerState : int
        {

            Unspecified = -1,

            D0 = 0, // Full On: full power, full functionality
            D1, // Low Power On: fully functional at low power/performance
            D2, // Standby: partially powered with automatic wake
            D3, // Sleep: partially powered with device initiated wake
            D4, // Off: unpowered
        }

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern int SetDevicePower(
        string pvDevice,
        int dwDeviceFlags,
        DevicePowerState DeviceState);

        private const int POWER_NAME = 0x00000001;

        public static void TurnOffBackLight()
        {
            SetDevicePower("BKL1:", POWER_NAME, DevicePowerState.D4);
        }

        private struct MEMORY_STATUS
        {
            public UInt32 dwLength;
            public UInt32 dwMemoryLoad;
            public UInt32 dwTotalPhys;
            public int dwAvailPhys;
            public UInt32 dwTotalPageFile;
            public UInt32 dwAvailPageFile;
            public UInt32 dwTotalVirtual;
            public UInt32 dwAvailVirtual;
        }

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern void GlobalMemoryStatus(ref MEMORY_STATUS ms);

        public static double GetAvailablePhysicalMemory()
        {
            MEMORY_STATUS ms = new MEMORY_STATUS();
            try
            {
                GlobalMemoryStatus(ref ms);
                double avail = (double)ms.dwAvailPhys / 1048.576;
                return avail;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    
    public partial class Utility
    {
        public static void logError()
        {
            // TODO: Create a webservice in server to keep warning and error from the sensor.
        }      

        public static double BytesToMegabytes(double Bytes)
        {
            double dblAns;
            dblAns = (Bytes / 1024) / 1024;
            return dblAns;
        }

        public static void StartHouseKeeping()
        {
            //TODO: Check for remaining diskspace
<<<<<<< .mine
            OpenNETCF.IO.DriveInfo DI = new OpenNETCF.IO.DriveInfo("\\");
            //if (BytesToMegabytes(DI.AvailableFreeSpace) < CFRecorder.Settings.reservedDiskSpace)
            //{
                   //TODO: Add a warning row here
            //}
=======
            OpenNETCF.IO.DriveInfo DI = new OpenNETCF.IO.DriveInfo("\\");            
>>>>>>> .r56
            //throw new Exception("The method or operation is not implemented.");
        }
	}
}