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
		Stream stream;
		WaveIn waveIn;

		public Recording(string target)
		{
			this.target = target;
		}

		#region Properties
		private string target;
		public string Target
		{
			get { return target; }
			set { target = value; }
		}
		#endregion

		public void Stop()
		{
			waveIn.Stop();
			waveIn.Save(target);
		}

		public void RecordFor(short duration)
		{
			if (waveIn != null)
				throw new InvalidOperationException("Recording already started");

			waveIn = new WaveIn();
			waveIn.Preload(duration * 1000, 2 * 1024 * 1024);
			waveIn.Start();

			timer = new Timer(new TimerCallback(timer_Tick), null, duration * 1000, Timeout.Infinite);
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