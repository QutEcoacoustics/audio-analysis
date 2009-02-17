using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace ProcessorUI
{
	static class Settings
	{
		public static string WorkerName
		{
			get { return Load("WorkerName", ""); }
			set { Save("WorkerName", value); }
		}

		public static string TempFolder
		{
			get { return Load("TempFolder", ""); }
			set { Save("TempFolder", value); }
		}

		public static string Server
		{
			get { return Load("Server", "http://sensor.mquter.qut.edu.au/Processor/Processor.svc"); }
			set { Save("Server", value); }
		}

		public static int NumberOfThreads
		{
			get { return Load<int>("Threads", 1); }
			set { Save("Threads", value); }
		}

		public static int FilesProcessed
		{
			get { return Load<int>("FilesProcessed", 0); }
			set { Save("FilesProcessed", value); }
		}

		public static TimeSpan TotalDuration
		{
			get { return Load<TimeSpan>("TotalDuration", TimeSpan.Zero); }
			set { Save("TotalDuration", value); }
		}

		static T Load<T>(string name, T defaultValue)
		{
			if (typeof(T) == typeof(TimeSpan))
			{
				var value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\ProcessorUI", name, defaultValue);
				if (value is TimeSpan)
					return (T)value;
				else
					return (T)(object)TimeSpan.Parse(value.ToString());
			}
			return (T)Convert.ChangeType(Registry.GetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\ProcessorUI", name, defaultValue), typeof(T));
		}

		static void Save<T>(string name, T value)
		{
			Registry.SetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\ProcessorUI", name, value.ToString());
		}
	}
}