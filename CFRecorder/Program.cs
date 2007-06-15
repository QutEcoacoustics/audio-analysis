using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
			if (args.Length > 0 && args[0] == "AppRunAtTime")
			{
				try
				{
					MainForm.QueueNextReading();
					MainForm.TakeReading();
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