using System;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Timers;
using System.IO;
using QutSensors;

namespace DataImporter
{
	public partial class Importer : ServiceBase
	{
		const int TimerInterval = 60 * 1000;

		Timer timer;

		public Importer()
		{
			InitializeComponent();
		}

		public void DebugStart()
		{
			OnStart(null);
		}

		#region Service Overrides
		protected override void OnStart(string[] args)
		{
			timer = new Timer(TimerInterval);
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
			timer.Start();
		}

		protected override void OnStop()
		{
			if (timer != null)
			{
				timer.Stop();
				timer = null;
			}
		}
		#endregion

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			SynchroniseData();
		}

		object dataSyncObject = new object();
		public void SynchroniseData()
		{
			if (System.Threading.Monitor.TryEnter(dataSyncObject, 0))
			{
				try
				{
					foreach (string sensorFolder in Directory.GetDirectories(@"D:\stargate\home\stuart"))
					{
						SynchoniseSensor(sensorFolder);
					}
				}
				finally
				{
					System.Threading.Monitor.Exit(dataSyncObject);
				}
			}
		}

		private void SynchoniseSensor(string sensorFolder)
		{
			string sensorName = Path.GetFileName(sensorFolder);
			Console.WriteLine("Found sensor: {0}", Path.GetFileName(sensorFolder));
			Sensor sensor = GetSensor(sensorName);
			string photoFolder = Path.Combine(sensorFolder, "picture");
			if (Directory.Exists(photoFolder))
				foreach (string file in Directory.GetFiles(photoFolder, "*.jpg"))
					SynchoniseJpeg(sensor, file);
			string audioFolder = Path.Combine(sensorFolder, "sound");
			if (Directory.Exists(audioFolder))
				foreach (string file in Directory.GetFiles(audioFolder, "*.wav"))
					SynchoniseWav(sensor, file);
		}

		private Sensor GetSensor(string sensorName)
		{
			Sensor retVal = Sensor.GetSensor(sensorName);
			if (retVal == null)
			{
				retVal = new Sensor(sensorName);
				retVal.Save();
			}
			return retVal;
		}

		private void SynchoniseJpeg(Sensor sensor, string file)
		{
			DateTime time;
			if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(file).Substring(4), "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeLocal, out time))
			{
				Console.WriteLine("Found JPEG: {0}", Path.GetFileNameWithoutExtension(file));
				PhotoReading reading = new PhotoReading(sensor.ID.Value);
				reading.Time = time;
				reading.Save();

				using (FileStream stream = File.Open(file, FileMode.Open))
				using (BinaryReader reader = new BinaryReader(stream))
				{
					byte[] buffer = reader.ReadBytes(1500 * 1024);
					reading.UpdateData(buffer);
				}

				File.Move(file, file + ".bak");
			}
		}

		private void SynchoniseWav(Sensor sensor, string file)
		{
			DateTime time;
			if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(file).Substring(4), "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeLocal, out time))
			{
				Console.WriteLine("Found WAV: {0}", Path.GetFileNameWithoutExtension(file));
				AudioReading reading = new AudioReading(sensor.ID.Value);
				reading.Time = time;
				reading.Save();

				using (FileStream stream = File.Open(file, FileMode.Open))
				using (BinaryReader reader = new BinaryReader(stream))
				{
					byte[] buffer = reader.ReadBytes(1500 * 1024);
					reading.UpdateData(buffer);
				}

				File.Move(file, file + ".bak");
			}
		}
	}
}