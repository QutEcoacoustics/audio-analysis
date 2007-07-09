using System;
using System.IO;
using System.Threading;
using System.Configuration;
using System.ServiceProcess;
using QutSensors;

namespace QutSensors.Importer
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

		void timer_Tick(object state)
		{
			SynchroniseData();
		}

		readonly object dataSyncObject = new object();
		public void SynchroniseData()
		{
			if (Monitor.TryEnter(dataSyncObject, 0))
			{
				try
				{
					Console.WriteLine("Synchronising data...");
					foreach (string sensorFolder in Directory.GetDirectories(ConfigurationManager.AppSettings["DataPath"]))
					{
						if (Stopping) // Indicates we're supposed to stop
							return;
						SynchoniseSensor(sensorFolder);
					}
				}
				finally
				{
					Monitor.Exit(dataSyncObject);
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
				{
					if (Stopping) // Indicates we're supposed to stop
						return;
					SynchoniseJpeg(sensor, file);
				}
			string audioFolder = Path.Combine(sensorFolder, "sound");
			if (Directory.Exists(audioFolder))
				foreach (string file in Directory.GetFiles(audioFolder, "*.wav"))
				{
					if (Stopping) // Indicates we're supposed to stop
						return;
					SynchoniseWav(sensor, file);
				}
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