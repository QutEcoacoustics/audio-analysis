using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using QutSensors;
using System;
using System.IO;
using System.Diagnostics;

namespace DataImporter
{
	static class Program
	{
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				switch (args[0].ToLower())
				{
					case "debug":
						Importer importer = new Importer();
						importer.SynchroniseData();
						break;
					case "spectrums":
						while (true)
						{
							GenerateSpectrums();
							GenerateSpectrograms();
							System.Threading.Thread.Sleep(30000);
						}
						break;
				}
			}
			else
			{
				// More than one user Service may run within the same process. To add
				// another service to this process, change the following line to
				// create a second service object. For example,
				//
				//   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
				//
				ServiceBase[] servicesToRun = new ServiceBase[] { new Importer() };

				ServiceBase.Run(servicesToRun);
			}
		}

		private static void GenerateSpectrums()
		{
			List<AudioReading> readings = AudioReading.GetReadingsWithoutSpectrums();
			GenerateAnalysisImages(readings);
		}

		private static void GenerateSpectrograms()
		{
			List<AudioReading> readings = AudioReading.GetReadingsWithoutSpectrograms();
			GenerateAnalysisImages(readings);
		}

		private static void GenerateAnalysisImages(List<AudioReading> readings)
		{
			int index = 1;
			foreach (AudioReading reading in readings)
			{
				Console.WriteLine("Processing reading {0}/{1}", index++, readings.Count);

				byte[] buffer = reading.GetData();
				string basePath = Path.GetTempFileName();
				File.Delete(basePath);
				basePath = Path.Combine(@"C:\SpectrumGenerator", Path.GetFileNameWithoutExtension(basePath));
				string wavPath = basePath + ".wav";
				string freqSpectrumPath = basePath + "-freqspectrum.jpg";
				string spectrogramPath = basePath + "-spectrogram.png";
				string dataPath = basePath + "-values.txt";

				try
				{
					using (FileStream stream = new FileStream(wavPath, FileMode.Create))
						stream.Write(buffer, 0, buffer.Length);
					ProcessStartInfo psi = new ProcessStartInfo("java", string.Format("-classpath .;jfreechart-1.0.5.jar;jcommon-1.0.9.jar;jl1.0.jar SpectrumGenerator {0}", wavPath));
					psi.WorkingDirectory = @"C:\SpectrumGenerator";
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

					System.Threading.Thread.Sleep(20000); // Spectrogram takes time to be created!?!
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
						System.Threading.Thread.Sleep(5000);
						goto RetrySpectrogram;
					}
				}
				finally
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
			}
		}
	}
}