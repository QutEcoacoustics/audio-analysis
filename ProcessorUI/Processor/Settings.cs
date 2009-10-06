using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace QutSensors.Processor
{
	public static class Settings
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
			get { return Load("Server", "http://localhost:2669/WebFrontend/Processor/Processor.svc"); }
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
				var value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\ProcessorUI", name, defaultValue) ?? default(T);
				if (value is TimeSpan)
					return (T)value;
				else
					return (T)(object)TimeSpan.Parse(value.ToString());
			}

            // the default value seems to be ignored for value types (returns null instead of 0 when T = int).  Therefore we coalesce
            // any resulting null value into the default for T - reference T = null (unchanged), value T = non-null and convertable...

			return (T)Convert.ChangeType(Registry.GetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\ProcessorUI", name, defaultValue) ?? default(T), typeof(T));
		}

		static void Save<T>(string name, T value)
		{
			Registry.SetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\ProcessorUI", name, value.ToString());
		}
	}
}