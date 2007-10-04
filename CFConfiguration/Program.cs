using System;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using QUT;
using System.IO;

namespace CFConfiguration
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main()
		{
			SetupProgramEntries();

			Application.Run(new MainForm());
		}

		/// <summary>
		/// Ensures that CFRecorder is set to run on startup and adds config to start menu.
		/// </summary>
		private static void SetupProgramEntries()
		{
			Utilities.AddToStartMenu("QutSensors Configuration", Assembly.GetExecutingAssembly().GetName().CodeBase);
			Utilities.AddToStartup(Utilities.GetRecorderExePath());

			// Create rebooter copies
			if (!File.Exists(@"\Program Files\QUTSensors\Rebooter1.exe"))
				File.Copy(@"\Program Files\QUTSensors\Rebooter.exe", @"\Program Files\QUTSensors\Rebooter1.exe");
			if (!File.Exists(@"\Program Files\QUTSensors\Rebooter3.exe"))
				File.Copy(@"\Program Files\QUTSensors\Rebooter.exe", @"\Program Files\QUTSensors\Rebooter3.exe");
			if (!File.Exists(@"\Program Files\QUTSensors\Rebooter5.exe"))
				File.Copy(@"\Program Files\QUTSensors\Rebooter.exe", @"\Program Files\QUTSensors\Rebooter5.exe");
		}
	}
}