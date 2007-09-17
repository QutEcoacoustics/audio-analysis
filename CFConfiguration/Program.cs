using System;
using System.Collections.Generic;
using System.Windows.Forms;
using QUT;
using System.Reflection;

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
		}
	}
}