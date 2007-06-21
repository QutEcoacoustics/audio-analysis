using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace CFRecorder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] args)
        {
#if NoUI
			try
			{
				if (File.Exists("\\NoUI.txt"))
				{
					using (StreamReader reader = File.OpenText("\\NoUI.txt"))
						Settings.Server = reader.ReadToEnd();
				}
				else
				{
					using (StreamWriter writer = File.CreateText("\\NoUI.txt"))
						writer.Write(Settings.Server);
				}
			}
			catch { }
			if (true)
#else
			if (args.Length > 0 && args[0] == "AppRunAtTime")
#endif
			{
				try
				{
					MainForm.QueueNextReading();
					DataUploader.ProcessFailures();
					MainForm.TakeReading();
					MainForm.WaitForReading();
				}
				catch (Exception e)
				{
					MainForm.Log("Error taking periodic reading: {0}", e);
				}
			}
			else
			{
				MainForm.ClearQueuedReading();
				Application.Run(new MainForm());
			}
		}
    }
}