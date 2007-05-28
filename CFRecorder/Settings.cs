using System;
using System.Collections.Generic;
using System.Text;

namespace CFRecorder
{
	public class Settings
	{
		#region Statics
		static Settings current = new Settings();

		static Settings() { }

		public static Settings Current
		{
			get { return current; }
		}
		#endregion

		Settings()
		{
		}

		#region Properties
		public string SensorID
		{
			get
			{
				return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorID", null);
			}

			set
			{
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorID", value);
			}
		}

		public string SensorDataPath
		{
			get
			{
				return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", null);
			}

			set
			{
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", value);
			}
		}
		#endregion
	}
}