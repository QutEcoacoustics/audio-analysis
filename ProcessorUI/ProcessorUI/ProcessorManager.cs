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
		long runningThreads;

		public ProcessorManager()
		{
			TotalDuration = TimeSpan.Zero;
		}

		#region Properties
		public int FilesProcessed { get; set; }
		public TimeSpan TotalDuration { get; set; }
		#endregion

		public void Start()
		{
			State = ProcessorState.Running;
			lock (this)
			{
				if (Settings.NumberOfThreads == 1)
				{
					runningThreads = 1;
					GetNextJob(Settings.WorkerName);
				}
				else
				{
					for (int i = 0; i < Settings.NumberOfThreads; i++)
					{
						Interlocked.Increment(ref runningThreads);
						GetNextJob(Settings.WorkerName + "_" + i.ToString());
					}
				}
			}
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
			while (Interlocked.Read(ref runningThreads) > 0)
				stopped.WaitOne();
			stopped = null;
		}

		#region Properties
		public ProcessorState State { get; private set; }
		#endregion

		void GetNextJob(string workerName)
		{
			OnLog("Requesting jobs...");
			var ws = new ServiceWrapper();
			ws.Proxy.BeginGetJobItem(workerName, OnGotJob, new object[] { ws, workerName });
		}

		void OnGotJob(IAsyncResult ar)
		{
			var incomingWs = (ServiceWrapper)((object[])ar.AsyncState)[0];
			var workerName = (string)((object[])ar.AsyncState)[1];

			if (State == ProcessorState.Stopping)
			{
				incomingWs.Close();
				OnLog("Stopping");
				OnStopped();
			}
			else
			{
				try
				{
					ProcessorJobItemDescription item;
					{
						using (incomingWs)
						{
							try
							{
								item = incomingWs.Proxy.EndGetJobItem(ar);
								incomingWs.Close();
							}
							catch (Exception e)
							{
								OnLog("Error in web service call - " + e.ToString());
								OnLog("Sleeping...");
								Thread.Sleep(5000);
								GetNextJob(workerName);
								return;
							}
						}
					}

					bool processed = false;
					try
					{
						if (item == null)
						{
							OnLog("No jobs available");
							System.Threading.Thread.Sleep(30000);
							GetNextJob(workerName);
						}
						else
						{
							processed = ProcessJobItem(item, workerName);

							if (State == ProcessorState.Running)
								GetNextJob(workerName);
							else
								OnStopped();
						}
					}
					finally
					{
						if (!processed && item != null)
						{
							using (var ws = new ServiceWrapper())
							{
								ws.Proxy.ReturnJob(workerName, item.JobItemID);
								ws.Close();
							}
						}
					}
				}
				catch (Exception e)
				{
					OnLog("ERROR! " + e.ToString());
					OnLog("Sleeping...");
					Thread.Sleep(5000);
					GetNextJob(workerName);
					//OnStopped();
				}
			}
		}

		bool ProcessJobItem(ProcessorJobItemDescription item, string workerName)
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
					TimeSpan? duration;
					var results = AnalyseFile(tempFile, item.MimeType, classifier, out duration);

					if (results != null)
					{
						using (var ws = new ServiceWrapper())
						{
							ws.Proxy.SubmitResults(workerName, item.JobItemID, results.ToArray());
							ws.Close();
						}
						try
						{
							lock (this)
							{
								FilesProcessed++;
								if (duration != null)
									TotalDuration += duration.Value;
							}
						}
						catch { } // Don't allow this to bring down the processor!
						return true;
					}
				}
			}
			return false;
		}

		IEnumerable<ProcessorJobItemResult> AnalyseFile(TempFile file, string mimeType, BaseClassifier classifier, out TimeSpan? duration)
		{
			var retVal = new List<ProcessorJobItemResult>();

			duration = DShowConverter.GetDuration(file.FileName, mimeType);
			if (duration == null)
			{
				OnLog("Unable to calculate length");
				throw new Exception("Unable to calculate length");
			}
			OnLog("Total length: {0}", duration);
			for (int i = 0; i < duration.Value.TotalMilliseconds; i += 60000)
			{
				OnLog("\t{0}-{1}", TimeSpan.FromMilliseconds(i), TimeSpan.FromMilliseconds(i + 60000));
				using (var converted = DShowConverter.ConvertTo(file.FileName, mimeType, MimeTypes.WavMimeType, i, i + 60000) as BufferedDirectShowStream)
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
			Interlocked.Decrement(ref runningThreads);
			if (stopped != null)
				stopped.Set();
			if (Interlocked.Read(ref runningThreads) == 0 && Stopped != null)
				Stopped(this, EventArgs.Empty);
		}
		#endregion
	}

	/// <summary>
	/// Wraps the WCF Service to ensure Abort or Close is called as appropriate
	/// Close should be called in normal circumstances, Abort if there's an error.
	/// Easiest way to use is:
	/// using (var ws = new ServiceWrapper()) {
	///		ws.Proxy.Call();
	///		ws.Close();
	///	}
	///	That way, if Call() fails then an exception is thrown and Dispose is called without Close
	///	being called beforehand. In that case the wrapper will call Abort.
	/// </summary>
	class ServiceWrapper : IDisposable
	{
		WebServices.ProcessorClient proxy;
		public ServiceWrapper()
		{
			proxy = new WebServices.ProcessorClient("WSHttpBinding_Processor", Settings.Server);
		}

		public WebServices.ProcessorClient Proxy
		{
			get { return proxy; }
		}

		public void Close()
		{
			proxy.Close();
			proxy = null;
		}

		#region IDisposable
		public void Dispose()
		{
			if (proxy != null)
				proxy.Abort();
		}

		~ServiceWrapper()
		{
			Dispose();
		}
		#endregion
	}
}