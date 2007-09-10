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

		public void TakeRecording(DateTime start, DateTime end)
		{
			if (end < DateTime.Now)
				throw new InvalidOperationException();

			LongRecorder recorder = null;
			if (start < DateTime.Now)
			{
				int duration = (int)(end - DateTime.Now).TotalMilliseconds;
				recorder = new LongRecorder(GetPath(), duration);
			}
			else
			{
				int duration = (int)(end - start).TotalMilliseconds;
				recorder = new LongRecorder(GetPath(), duration);
				WaitTill(start);
			}

			if (!recorder.PerformRecording())
				throw new Exception("PerformRecording failed");

			recorder.WaitTillEnd();
		}

		void WaitTill(DateTime time)
		{
			int sleepTime = (int)(time - DateTime.Now).TotalMilliseconds;
			if (sleepTime > 0)
				Thread.Sleep(sleepTime);
		}

		public void RecordFor(int duration)
		{
			Start(duration);
			timer = new Timer(new TimerCallback(timer_Tick), null, duration, Timeout.Infinite);
		}

		private void Start(int expectedDuration)
		{
			PrepareRecording(expectedDuration);

			Wave.MMSYSERR result = waveIn.Start();
			if (result != Wave.MMSYSERR.NOERROR)
				throw new Exception(string.Format("Error saving recording - {0}", result));
			startTime = DateTime.Now;
		}

		bool prepared = false;
		private void PrepareRecording(int expectedDuration)
		{
			if (waveIn != null)
				throw new InvalidOperationException("Recording already started");

			if (!prepared)
			{
				waveIn = new WaveIn();
				Wave.MMSYSERR result = waveIn.Preload(expectedDuration, 2 * 1024 * 1024);
				if (result != Wave.MMSYSERR.NOERROR)
					throw new Exception(string.Format("Error saving recording - {0}", result));
				prepared = true;
			}
		}

		public void Stop()
		{
			string path = GetPath();
			try
			{
				waveIn.Stop();
				Wave.MMSYSERR result = waveIn.Save(path);
                if (result != Wave.MMSYSERR.NOERROR)
                    Utilities.Log("Error saving recording - {0}", result);
				succeeded = true;
			}
			catch (IOException)
			{
				//Log("Recording stop failed - {0}", e);
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