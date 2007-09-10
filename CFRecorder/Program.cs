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
			// Attempt to ensure any faults will be recovered from by re-running the application
			Utilities.QueueNextAppRun(DateTime.Now.AddMinutes(5));
			//TEMP removal
			//PDA.Video.PowerOffScreen();

			DeviceManager.Start();
        }
    }
}