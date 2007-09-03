using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace QUT
{
    public static class PDA
    {

        public enum PowerState
        {
            POWER_STATE_ON = 0x00010000,
            POWER_STATE_OFF = 0x00020000,
            POWER_STATE_CRITICAL = 0x00040000,
            POWER_STATE_BOOT = 0x00080000,
            POWER_STATE_IDLE = 0x00100000,
            POWER_STATE_SUSPEND = 0x00200000,
            POWER_STATE_UNATTENDED = 0x00400000,
            POWER_STATE_RESET = 0x00800000,
            POWER_STATE_USERIDLE = 0x01000000,
            POWER_STATE_PASSWORD = 0x10000000
        }
        public enum PowerRequirement
        {
            POWER_NAME = 0x00000001,
            POWER_FORCE = 0x00001000,
            POWER_DUMPDW = 0x00002000
        }

        public enum ACLineStatus : byte
        {
            Offline = 0,
            Online = 1,
            BackUp = 2,
            Unknown = 255,
        }
 
        private struct SYSTEM_POWER_STATUS_EX2
        {
            public ACLineStatus ACLineStatus;
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
            public uint BatteryVoltage;
            public uint BatteryCurrent;
            public uint BatteryAverageCurrent;
            public uint BatteryAverageInterval;
            public uint BatterymAHourConsumed;
            public uint BatteryTemperature;
            public uint BackupBatteryVoltage;
            public byte BatteryChemistry;
        }

        [DllImport("coredll.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int GetSystemPowerStatusEx2(ref SYSTEM_POWER_STATUS_EX2 pSystemPowerStatusEx2, [MarshalAs(UnmanagedType.U4), In] int dwLen, [MarshalAs(UnmanagedType.Bool), In] bool fUpdate);

        private const int FILE_DEVICE_HAL = 257;
        private const int METHOD_BUFFERED = 0;
        private const int FILE_ANY_ACCESS = 0;
        static int CTL_CODE(int DeviceType, int Func, int Method, int Access)
        {return (DeviceType << 16) | (Access << 14) | (Func << 2) | Method;}

        [DllImport("Coredll.dll")]
        private static extern int KernelIoControl(int dwIoControlCode, IntPtr lpInBuf, int nInBufSize, IntPtr lpOutBuf, int nOutBufSize, ref int lpBytesReturned);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern int SetSystemPowerState(string psState, int StateFlags, int Options);

        public static double GetBatteryLevel()
        {
            SYSTEM_POWER_STATUS_EX2 status2 = new SYSTEM_POWER_STATUS_EX2();
            GetSystemPowerStatusEx2(ref status2, (int)Marshal.SizeOf(status2), false);
            return Convert.ToDouble(status2.BatteryLifePercent);
        }

        public static int SoftReset()
        {
            int bytesReturned = 0;
            int IOCTL_HAL_REBOOT = CTL_CODE(FILE_DEVICE_HAL, 15, METHOD_BUFFERED, FILE_ANY_ACCESS);
            return KernelIoControl(IOCTL_HAL_REBOOT, IntPtr.Zero, 0, IntPtr.Zero, 0, ref bytesReturned);
        }


    }
}
