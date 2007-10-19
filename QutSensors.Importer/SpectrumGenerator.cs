using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;
using QutSensors.Data.ActiveRecords;
using AudioTools;

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
			timer_Tick(null);
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
			try
			{
				QutSensors.Data.ActiveRecords.AudioReading[] readings = QutSensors.Data.ActiveRecords.AudioReading.GetWavReadingsWithoutSpectrums();
				GenerateAnalysisImages(readings);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void GenerateSpectrograms()
		{
			try
			{
				QutSensors.Data.ActiveRecords.AudioReading[] readings = QutSensors.Data.ActiveRecords.AudioReading.GetWavReadingsWithoutSpectrograms();
				GenerateAnalysisImages(readings);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void GenerateAnalysisImages(QutSensors.Data.ActiveRecords.AudioReading[] readings)
		{
			int index = 1;
			foreach (QutSensors.Data.ActiveRecords.AudioReading reading in readings)
			{
				GC.Collect();
				if (Stopping) // Indicates we're supposed to stop
					return;
				byte[] buffer = reading.Data.Data;
				if (!MimeTypes.IsWav(reading.MimeType))
				{
					if (MimeTypes.IsAsf(reading.MimeType))
					{
						try
						{
							buffer = AudioTools.WavConverter.FromAsf(buffer);
						}
						catch (Exception e)
						{
							Console.WriteLine("Unable to convert to WAV format - {0}", e);
							buffer = null;
						}
					}
					else
						buffer = null;
				}
				if (buffer != null)
				{
					Console.WriteLine("Processing reading {0}/{1} - {2} - {3}", index++, readings.Length, reading.ID, reading.MimeType);
					try
					{
						if (buffer != null)
						{
							string basePath = Path.GetTempFileName();
							File.Delete(basePath);
							basePath = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["SpectrumGeneratorDataPath"], Path.GetFileNameWithoutExtension(basePath));
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
								psi.UseShellExecute = false;
								Process process = Process.Start(psi);
								try
								{
									process.PriorityClass = ProcessPriorityClass.BelowNormal;
								}
								catch { }
								process.WaitForExit();

								using (FileStream stream = File.Open(freqSpectrumPath, FileMode.Open))
								using (BinaryReader reader = new BinaryReader(stream))
									buffer = reader.ReadBytes(5000 * 1024);
								if (buffer.Length > 0)
								{
									Console.WriteLine("Spectrum image generated.");
									reading.Data.SpectrumData = buffer;
									reading.Data.Save();
								}

								Thread.Sleep(20000); // Spectrogram takes time to be created!?!
							RetrySpectrogram:
								using (FileStream stream = File.Open(spectrogramPath, FileMode.Open))
								using (BinaryReader reader = new BinaryReader(stream))
									buffer = reader.ReadBytes(5000 * 1024);
								if (buffer.Length > 0)
								{
									Console.WriteLine("Spectrogram image generated.");
									reading.Data.SpectrogramData = buffer;
									reading.Data.Save();
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
								TryDeleteFile(wavPath);
								TryDeleteFile(freqSpectrumPath);
								TryDeleteFile(spectrogramPath);
								TryDeleteFile(dataPath);
							}
						}
					}
					catch (Exception e)
					{
						Console.WriteLine("Failed generation... will retry later.");
						Console.WriteLine(e);
					}
				}
				else
					Console.WriteLine("Skipping reading - invalid mime type: {0}", reading.MimeType);
			}
		}

		private static void TryDeleteFile(string wavPath)
		{
			while (File.Exists(wavPath))
			{
				try
				{
					File.Delete(wavPath);
				}
				catch { Thread.Sleep(500); }
			}
		}
	}
}