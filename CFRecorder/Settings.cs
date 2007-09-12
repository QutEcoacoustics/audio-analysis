using System;
using System.Collections.Generic;
using System.Text;

	public static class Settings
	{
		#region Properties
		public static Guid SensorID
		{
			get
			{
				object retVal = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorID", null);
				if (retVal != null)
				{
					try
					{
						return new Guid((string)retVal);
					}
					catch (Exception)
					{
						try
						{
							Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\QUT");
							key.DeleteValue("SensorID", false);
						}
						catch { }
					}
				}

				return GenerateID();
			}

			set {Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorID", value.ToString());}
		}

		public static string SensorName
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorName", null); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorName", value); }
		}

		public static string FriendlyName
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "FriendlyName", null); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "FriendlyName", value); }
		}

		public static string Description
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Description", null); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Description", value); }
		}

		public static string SensorDataPath
		{
			get {return ((string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", null) ?? "\\");}
			set {Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", value);}
		}

		const string DefaultServer = "www.mquter.qut.edu.au/sensor/demo/";
		public static string Server
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Server", DefaultServer); }
			set
			{
				if (value == null)
					Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Server", DefaultServer);
				else
					Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Server", value);
			}
		}

		public static int ReadingFrequency
		{
			get { return Convert.ToInt32(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingFrequency", 30 * 60 * 1000)); } // 30 Minutes
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingFrequency", value, Microsoft.Win32.RegistryValueKind.DWord); }
		}

		public static int ReadingDuration
		{
			get { return Convert.ToInt32(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingDuration", 30 * 1000)); } // 30 Seconds
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingDuration", value, Microsoft.Win32.RegistryValueKind.DWord); }
		}

		public static DateTime? LastRecordingTime
		{
			get
			{
				string retVal = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "LastRecording", null);
				if (retVal == null)
					return null;
				return DateTime.Parse(retVal);
			}

			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "LastRecording", value.ToString()); }
		}

		public static string LogPath
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "LogPath", "\\"); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "LogPath", value); }
		}

		public static bool DebugMode
		{
			get { return Convert.ToBoolean(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DebugMode", false)); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DebugMode", value); }
		}
		#endregion

		private static Guid GenerateID()
		{
			Guid retVal = Guid.NewGuid();
			SensorID = retVal;
			return retVal;
		}
	}
