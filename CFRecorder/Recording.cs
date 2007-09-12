using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using QUT;

namespace CFRecorder
{
    public class Recording
    {
		public Recording()
		{
		}

		public Recording(DateTime startTime)
		{
			this.startTime = startTime;
		}

		#region Properties
		private DateTime? startTime;
		public DateTime? StartTime
		{
			get { return startTime; }
		}
		#endregion

		public string GetPath()
		{
			return Path.Combine(Settings.SensorDataPath, string.Format("{0}_{1:yyyyMMdd-HHmmss}.wav", Settings.DeploymentID == null ? "Unknown" : Settings.DeploymentID.ToString(), startTime));
		}

		public void TakeRecording(DateTime start, DateTime end)
		{
			if (end < DateTime.Now)
				throw new InvalidOperationException();

			LongRecorder recorder = null;
			if (start < DateTime.Now)
			{
				startTime = DateTime.Now;
				int duration = (int)(end - DateTime.Now).TotalMilliseconds;
				recorder = new LongRecorder(GetPath(), duration);
			}
			else
			{
				startTime = start;
				int duration = (int)(end - start).TotalMilliseconds;
				
				WaitTill(start);
				recorder = new LongRecorder(GetPath(), duration); // Would prefer this to be above the WaitTill but not working if put there.
			}

			if (!recorder.PerformRecording())
				throw new Exception("PerformRecording failed");

			recorder.WaitTillEnd();
			Utilities.Log("Recording complete: {0:dd/MM HH:mm:ss}", DateTime.Now);
		}

		void WaitTill(DateTime time)
		{
			int sleepTime = (int)(time - DateTime.Now).TotalMilliseconds;
			if (sleepTime > 0)
				Thread.Sleep(sleepTime);
		}
	}
}