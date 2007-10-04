using System;
using System.Windows.Forms;
using QUT;

namespace CFRecorder
{
    static class Program
    {
        [MTAThread]
        static void Main()
        {
			try
			{
				// Attempt to ensure any faults will be recovered from by re-running the application
				Utilities.QueueNextAppRun(DateTime.Now.AddMinutes(5));

				Utilities.QueueNextAppRun(DateTime.Now.AddDays(1), @"\Program Files\QUTSensors\Rebooter1.exe");
				Utilities.QueueNextAppRun(DateTime.Now.AddDays(3), @"\Program Files\QUTSensors\Rebooter3.exe");
				Utilities.QueueNextAppRun(DateTime.Now.AddDays(5), @"\Program Files\QUTSensors\Rebooter5.exe");

				if (!Settings.DebugMode)
					PDA.Video.PowerOffScreen();

				DeviceManager.Start();

				PDA.Video.Standby();
			}
			catch (Exception e)
			{
				Utilities.Log(e, "Caught at top-level handler");
			}
        }
    }
}