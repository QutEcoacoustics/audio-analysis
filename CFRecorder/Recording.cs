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
		#endregion

		public string GetPath()
		{
			return Path.Combine(Settings.SensorDataPath, string.Format("{0}_{1:yyyyMMdd-HHmmss}.wav", Settings.SensorName, startTime));
		}

		public void RecordFor(short duration)
		{
			if (waveIn != null)
				throw new InvalidOperationException("Recording already started");

			waveIn = new WaveIn();
			waveIn.Preload(duration, 2 * 1024 * 1024);
			waveIn.Start();
			startTime = DateTime.Now;
			timer = new Timer(new TimerCallback(timer_Tick), null, duration, Timeout.Infinite);
		}

		public void Stop()
		{
			try
			{
				waveIn.Stop();
				waveIn.Save(GetPath());
			}
			catch (IOException e)
			{
				MainForm.Log("Recording stop failed - {0}", e);
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