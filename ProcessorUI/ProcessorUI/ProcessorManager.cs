using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioStuff;
using ProcessorUI.WebServices;
using AudioTools;
using System.Threading;

namespace ProcessorUI
{
	public delegate void GenericHandler<T>(object sender, T args);

	public class ProcessorManager
	{
		public enum ProcessorState
		{
			Ready,
			Running,
			Stopping
		}

		AutoResetEvent stopped;
		WebServices.ProcessorClient ws;

		public void Start()
		{
			State = ProcessorState.Running;
			GetNextJob();
		}

		public void Stop()
		{
			State = ProcessorState.Stopping;
		}

		public void StopAndWait()
		{
			if (State == ProcessorState.Ready)
				return;

			if (stopped == null)
				stopped = new AutoResetEvent(false);
			Stop();
			stopped.WaitOne();
			stopped = null;
		}

		#region Properties
		public ProcessorState State { get; private set; }
		#endregion

		void GetNextJob()
		{
			ws = new WebServices.ProcessorClient("WSHttpBinding_Processor", Settings.Server);
			ws.BeginGetJobItem(Settings.WorkerName, null, OnGotJob, null);
		}

		void OnGotJob(IAsyncResult ar)
		{
			if (State == ProcessorState.Stopping)
			{
				OnLog("Stopping");
				OnStopped();
			}
			else
			{
				try
				{
					ProcessorJobItemDescription item;
					try
					{
						item = ws.EndGetJobItem(ar);
					}
					catch (Exception e)
					{
						OnLog("Error in web service call - " + e.ToString());
						Thread.Sleep(5000);
						GetNextJob();
						return;
					}
					bool processed = false;
					try
					{
						if (item == null)
						{
							OnLog("No jobs available");
							System.Threading.Thread.Sleep(30000);
							GetNextJob();
						}
						else
						{
							processed = ProcessJobItem(item);

							if (State == ProcessorState.Running)
								GetNextJob();
							else
								OnStopped();
						}
					}
					finally
					{
						if (!processed && item != null)
							ws.ReturnJob(Settings.WorkerName, item.JobItemID);
					}
					
				}
				catch (Exception e)
				{
					OnLog("ERROR! " + e.ToString());
					OnStopped();
				}
			}
		}

		bool ProcessJobItem(ProcessorJobItemDescription item)
		{
			var parameters = BaseClassifierParameters.Deserialize(item.Job.Parameters);
			var classifier = BaseClassifier.Create(parameters);
			OnLog("Job Retrieved - JobItemID {0} for {1}({2})", item.JobItemID, item.Job.Name, parameters.Name);

			using (var tempFile = new TempFile(".wav"))
			{
				Utilities.DownloadFile(item.AudioReadingUrl, tempFile.FileName);
				if (State != ProcessorState.Stopping)
				{
					OnLog("Analysing {0}", item.AudioReadingUrl);
					var results = AnalyseFile(tempFile, item.MimeType, classifier);

					

					if (results != null)
					{
						ws.SubmitResults(Settings.WorkerName, item.JobItemID, results.ToArray());
						return true;
					}
				}
			}
			return false;
		}

		IEnumerable<ProcessorJobItemResult> AnalyseFile(TempFile file, string mimeType, BaseClassifier classifier)
		{
			var retVal = new List<ProcessorJobItemResult>();

			var duration = DShowConverter.GetDuration(file.FileName, mimeType);
			if (duration == null)
			{
				OnLog("Unable to calculate length");
				return null;
			}
			OnLog("Total length: {0}", duration);
			for (int i = 0; i < duration.Value.TotalMilliseconds; i += 60000)
			{
				OnLog("\t{0}-{1}", TimeSpan.FromMilliseconds(i), TimeSpan.FromMilliseconds(i + 60000));
				using (var converted = DShowConverter.ToWav(file.FileName, mimeType, i, i + 60000))
				{
					var result = classifier.Analyse(new AudioRecording() { FileName = converted.BufferFile.FileName }) as MMResult;

					OnLog("RESULT: {0}, {1}, {2}", result.NumberOfPeriodicHits, result.VocalBest, result.VocalBestLocation);
					retVal.Add(new ProcessorJobItemResult()
					{
						Start = i,
						Stop = i + 60000,
						NumberOfHits = result.NumberOfPeriodicHits ?? 0,
						BestScore = result.VocalBest,
						BestScoreLocation = result.VocalBestLocation
					});
				}
				if (State == ProcessorState.Stopping)
					return null;
			}
			return retVal;
		}

		#region Events
		public event GenericHandler<string> Log;
		protected void OnLog(string format, params object[] args)
		{
			if (Log != null)
				Log(this, string.Format(format, args));
		}

		public event EventHandler Stopped;
		protected void OnStopped()
		{
			OnLog("Stopped");
			State = ProcessorState.Ready;
			if (stopped != null)
				stopped.Set();
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);
		}
		#endregion
	}
}