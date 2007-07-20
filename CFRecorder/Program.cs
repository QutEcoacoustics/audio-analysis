using System;
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
                    DateTime nextRun;
					nextRun = MainForm.QueueNextReading();
					PDA.Video.PowerOffScreen();
					DataUploader.ProcessFailures();
					MainForm.TakeReading();
					MainForm.SendStatus();
					MainForm.WaitForReading();
                    MainForm.Log("Next run will be {0}", nextRun);
                    if (DateTime.Now.AddMinutes(1) >= nextRun)
                    {
                        nextRun = DateTime.Now.AddMinutes(1);
                        MainForm.QueueNextReading(nextRun);
                    }
					//PDAUtils.Reset(true, false); Somehow not functioning on my iMate :(
                    PDA.Hardware.SoftReset(); //Will switch back to Richard's version once I found out why it's not working
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