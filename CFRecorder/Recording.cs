using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace CFRecorder
{
	public class Recording
	{
		Timer timer;
		WaveIn waveIn;

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

		private bool succeeded;
		public bool Succeeded
		{
			get { return succeeded; }
		}
		#endregion

		public string GetPath()
		{
			return Path.Combine(Settings.SensorDataPath, string.Format("{0}_{1:yyyyMMdd-HHmmss}.wav", Settings.SensorName, startTime));
		}

		public void RecordFor(int duration)
		{
			if (waveIn != null)
				throw new InvalidOperationException("Recording already started");

			waveIn = new WaveIn();
			Wave.MMSYSERR result = waveIn.Preload(duration, 2 * 1024 * 1024);
			if (result != Wave.MMSYSERR.NOERROR)
				throw new Exception(string.Format("Error saving recording - {0}", result));
			result = waveIn.Start();
			if (result != Wave.MMSYSERR.NOERROR)
				throw new Exception(string.Format("Error saving recording - {0}", result));
			startTime = DateTime.Now;
			timer = new Timer(new TimerCallback(timer_Tick), null, duration, Timeout.Infinite);
		}

		public void Stop()
		{
			string path = GetPath();
			try
			{
				waveIn.Stop();
				Wave.MMSYSERR result = waveIn.Save(path);
				if (result != Wave.MMSYSERR.NOERROR)
					MainForm.Log("Error saving recording - {0}", result);
				succeeded = true;
			}
			catch (IOException e)
			{
				MainForm.Log("Recording stop failed - {0}", e);
				try
				{
					File.Delete(path);
				}
				catch { }
			}
		}

		void timer_Tick(object state)
		{
			Stop();
			OnDoneRecording();
		}

		void recorder_DoneRecording()
		{
			OnDoneRecording();
		}

		#region Event Sources
		public event EventHandler DoneRecording;
		protected void OnDoneRecording()
		{
			if (DoneRecording != null)
				DoneRecording(this, EventArgs.Empty);
		}
		#endregion
	}
}