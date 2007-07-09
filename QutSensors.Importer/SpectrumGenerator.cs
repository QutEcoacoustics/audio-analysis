using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;

namespace QutSensors.Importer
{
	class SpectrumGenerator : ServiceBase
	{
		const int TimerInterval = 60 * 1000;

		Timer timer;

		public SpectrumGenerator()
		{
			ServiceName = "SensorsVisualisationGenerator";
		}

		public void DebugStart()
		{
			OnStart(null);
		}

		public void DebugStop()
		{
			OnStop();
		}

		#region Service Overrides
		protected override void OnStart(string[] args)
		{
			timer = new Timer(timer_Tick, null, 5000, TimerInterval);
		}

		protected override void OnStop()
		{
			if (timer != null)
			{
				timer.Change(Timeout.Infinite, Timeout.Infinite);
				timer.Dispose();
				timer = null;
			}
		}
		#endregion

		bool Stopping
		{
			get { return timer == null; }
		}

		readonly object dataSyncObject = new object();
		void timer_Tick(object state)
		{
			// Don't allow two concurrent generation threads
			if (Monitor.TryEnter(dataSyncObject, 0))
			{
				try
				{
					Console.WriteLine("Generating spectrum data.");
					GenerateSpectrums();
					GenerateSpectrograms();
				}
				finally
				{
					Monitor.Exit(dataSyncObject);
				}
			}
		}

		private void GenerateSpectrums()
		{
			List<AudioReading> readings = AudioReading.GetReadingsWithoutSpectrums();
			GenerateAnalysisImages(readings);
		}

		private void GenerateSpectrograms()
		{
			List<AudioReading> readings = AudioReading.GetReadingsWithoutSpectrograms();
			GenerateAnalysisImages(readings);
		}

		private void GenerateAnalysisImages(List<AudioReading> readings)
		{
			int index = 1;
			foreach (AudioReading reading in readings)
			{
				if (Stopping) // Indicates we're supposed to stop
					return;
				Console.WriteLine("Processing reading {0}/{1} - {2}", index++, readings.Count, reading.ID);

				byte[] buffer = reading.GetData();
				string basePath = Path.GetTempFileName();
				File.Delete(basePath);
				basePath = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["SpectrumGeneratorPath"], Path.GetFileNameWithoutExtension(basePath));
				string wavPath = basePath + ".wav";
				string freqSpectrumPath = basePath + "-freqspectrum.jpg";
				string spectrogramPath = basePath + "-spectrogram.png";
				string dataPath = basePath + "-values.txt";

				try
				{
					using (FileStream stream = new FileStream(wavPath, FileMode.Create))
						stream.Write(buffer, 0, buffer.Length);
					ProcessStartInfo psi = new ProcessStartInfo("java", string.Format("-classpath .;jfreechart-1.0.5.jar;jcommon-1.0.9.jar;jl1.0.jar SpectrumGenerator {0}", wavPath));
					psi.WorkingDirectory = System.Configuration.ConfigurationManager.AppSettings["SpectrumGeneratorPath"];
					psi.CreateNoWindow = true;
					Process process = Process.Start(psi);
					process.WaitForExit();

					using (FileStream stream = File.Open(freqSpectrumPath, FileMode.Open))
					using (BinaryReader reader = new BinaryReader(stream))
						buffer = reader.ReadBytes(5000 * 1024);
					if (buffer.Length > 0)
					{
						Console.WriteLine("Spectrum image generated.");
						reading.UpdateSpectrumData(buffer);
					}

					Thread.Sleep(20000); // Spectrogram takes time to be created!?!
				RetrySpectrogram:
					using (FileStream stream = File.Open(spectrogramPath, FileMode.Open))
					using (BinaryReader reader = new BinaryReader(stream))
						buffer = reader.ReadBytes(5000 * 1024);
					if (buffer.Length > 0)
					{
						Console.WriteLine("Spectrogram image generated.");
						reading.UpdateSpectrogramData(buffer);
					}
					else
					{
						Thread.Sleep(5000);
						goto RetrySpectrogram;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Failed generation... will retry later.");
					Console.WriteLine(e);
				}
				finally
				{
					try
					{
						if (File.Exists(wavPath))
							File.Delete(wavPath);
						if (File.Exists(freqSpectrumPath))
							File.Delete(freqSpectrumPath);
						if (File.Exists(spectrogramPath))
							File.Delete(spectrogramPath);
						if (File.Exists(dataPath))
							File.Delete(dataPath);
					}
					catch { }
				}
			}
		}
	}
}