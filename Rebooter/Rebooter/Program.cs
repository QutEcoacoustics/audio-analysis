using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Rebooter
{
    class Program
    {

        const int POWER_STATE_RESET = 0x00800000;
        const int POWER_STATE_SUSPEND = 0x00200000;
        const int POWER_STATE_ON = 0x00010000;
        const int POWER_STATE_OFF = 0x00020000;

        [DllImport("coredll")]
        extern static int SetSystemPowerState(string psState, int stateFlags, int options);

        static void Main(string[] args)
        {
			using (StreamWriter shutdownLog = new StreamWriter("\\QSShutdown.log", true))
				shutdownLog.WriteLine(DateTime.Now + "Process Shutdown Occuring.");

			//Suspend for 10secs (docs says this ensure data is dumped to flash ram) 
			SetSystemPowerState(null, POWER_STATE_SUSPEND, 0);
			Thread.Sleep(12000);

			//Reset device
			using (StreamWriter shutdownLog = new StreamWriter("\\QSShutdown.log", true))
				shutdownLog.WriteLine(DateTime.Now + "Shutdown Complete.");

			SetSystemPowerState(null, POWER_STATE_RESET, 0);
		}
    }
}
