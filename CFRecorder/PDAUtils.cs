using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CFRecorder
{
	class PDAUtils
	{
		const int POWER_STATE_RESET = 0x00800000;
		const int POWER_STATE_SUSPEND = 0x00200000;
		const int POWER_STATE_ON = 0x00010000;
		const int POWER_STATE_OFF = 0x00020000;

		[DllImport("coredll")]
		extern static int SetSystemPowerState(string psState, int stateFlags, int options);

		[DllImport("coredll")]
		public static extern void GlobalMemoryStatus(MemStat lpBuffer);

		public static void MemSummary()
		{
			MemStat memStatus = new MemStat();
			GlobalMemoryStatus(memStatus);

			MessageBox.Show(String.Format("{0} bytes total \r\n{1} bytes free", memStatus.dwTotalPhys, memStatus.dwAvailPhys));
		}

		public static uint GetFreeMemory()
		{
			MemStat memStatus = new MemStat();
			GlobalMemoryStatus(memStatus);
			return memStatus.dwAvailPhys;
		}

		public static uint GetMemoryUsage()
		{
			MemStat memStatus = new MemStat();
			GlobalMemoryStatus(memStatus);
			return memStatus.dwMemoryLoad;
		}

		public static void Reset(bool expected, bool logged)
		{
			//TODO: if unexpected check logs for previous unexpected resets and possibly 
			//perma-turnoff device if there is a continuing problem.
			//if (!expected)
			if (logged)
			{
				using (StreamWriter shutdownLog = new StreamWriter("\\QSShutdown.log", true))
					shutdownLog.WriteLine(DateTime.Now + "Shutdown Occuring.");
			}
			//Suspend for 10secs (docs says this ensure data is dumped to flash ram) 
			SetSystemPowerState(null, POWER_STATE_SUSPEND, 0);
			Thread.Sleep(1200);

			//Reset device
			if (logged)
			{
				using (StreamWriter shutdownLog = new StreamWriter("\\QSShutdown.log", true))
					shutdownLog.WriteLine(DateTime.Now + "Shutdown Complete.");
			}
			SetSystemPowerState(null, POWER_STATE_RESET, 0);
		}
	}

	class MemStat
	{
		public MemStat()
		{
			dwLength = (uint)Marshal.SizeOf(this);
		}

		// Size of structure
		public uint dwLength = 0;

		// Rough Memory utilization percentage
		public uint dwMemoryLoad = 0;

		// Total Physical Mem
		public uint dwTotalPhys = 0;

		// Bytes availiable
		public uint dwAvailPhys = 0;

		// Storage space of page file
		public uint dwTotalPageFile = 0;

		// Bytes avail in paging file
		public uint dwAvailPageFile = 0;

		// User-mode virt memory total 		
		public uint dwTotalVirtual = 0;

		// Uncommited virt memory
		public uint dwAvailVirtual = 0;
	}
}