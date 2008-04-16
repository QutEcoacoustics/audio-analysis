using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WiFiEnable
{
    class Program
    {
        private const uint HKEY_LOCAL_MACHINE = 0x80000002;
        
        private enum PowerState
        {
            PwrDeviceUnspecified = -1,
            D0 = 0, // Full On: full power, full functionality
            D1 = 1, // Low Power On: fully functional at low power/performance
            D2 = 2, // Standby: partially powered with automatic wake
            D3 = 3, // Sleep: partially powered with device initiated wake
            D4 = 4, // Off: unpowered
            PwrDeviceMaximum = 5
        }

        [DllImport("coredll.dll")]
        private static extern int DevicePowerNotify(string name, PowerState state, int flags);

        [DllImport("coredll.dll")]
        private static extern int SetDevicePower(string name, int flags, PowerState state);

        [DllImport("coredll.dll")]
        private static extern int GetDevicePower(string name, int flags, out PowerState state);

        [DllImport("coredll.dll", CharSet = CharSet.Unicode)]
        private static extern uint RegOpenKeyEx(
            uint HKEY,
            string lpSubKey,
            int ulOptions,
            uint samDesired,
            out uint phkResult);

        [DllImport("coredll.dll", CharSet = CharSet.Unicode)]
        private static extern uint RegQueryValueEx(
            uint hKey,
            string lpValueName,
            int lpReserved,
            ref int lpType,
            StringBuilder lpData,
            ref int lpcbData);

        [DllImport("coredll.dll")]
        private static extern uint RegEnumValue(
             uint hKey, int dwIndex, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpName, ref int lpcName, int lpReserved,
             int lpType, int lpData, out double lpcbData);

        [DllImport("coredll.dll")]
        static extern uint RegEnumKeyEx(
            uint hKey, int dwIndex, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpName, ref int lpcName, int lpReserved, 
            int lpClass, int lpcClass, out double lastWriteTime);
        
        [DllImport("coredll.dll")]
        private static extern int RegCloseKey(uint hkey);

        public static string GetWirelessRegistryKey()
        {
            uint key;
            uint ret;

            StringBuilder lpData = new StringBuilder();
            int lpcbData = lpData.Length;
            ret = RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"System\CurrentControlSet\Control\POWER\State\", 0, 0, out key);

            StringBuilder name = new StringBuilder(260);
            int length = 260;
            int classLength = 0;
            double time = 0;
            if (ret == 0)
            {
                int count = 0;
                ret = RegEnumValue(key, count, name, ref length, 0, 0, classLength, out time);
                
                while (ret == 0 && !name.ToString().Contains(@"{98C5250D-C29A-4985-AE5F-AFE5367E5006}"))
                {
                    length = 260;
                    ret = RegEnumValue(key, ++count, name, ref length, 0, 0, classLength, out time);
                }
            }
            RegCloseKey(key);

            if ((name.ToString()).Contains(@"{98C5250D-C29A-4985-AE5F-AFE5367E5006}"))
            {
                return name.ToString();
            }
            return null;
        } 

        static void Main(string[] args)
        {
            string wirelessDevice = GetWirelessRegistryKey();
            try
            {
                //   HKEY_LOCAL_MACHINE/System/CurrentControlSet/Control/POWER/State/{98C5250D-C29A-4985-AE5F-AFE5367E5006}/---------
                DevicePowerNotify(wirelessDevice, PowerState.D0, 1);
                SetDevicePower(wirelessDevice, 1, PowerState.D0);
            }
            catch {}
        }
    }
}
