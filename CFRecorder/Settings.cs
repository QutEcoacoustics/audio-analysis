using System;
using System.Collections.Generic;
using System.Text;

namespace CFRecorder
{
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
			get {return ((string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", null) ?? "");}
			set {Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", value);}
		}

		public static string Server
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Server", "www.mquter.qut.edu.au/sensor/SensorInterface/"); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Server", value); }
		}

		public static int ReadingFrequency
		{
			get { return Convert.ToInt32(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingFrequency", 30 * 60 * 1000)); } // 30 Minutes
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingFrequency", value, Microsoft.Win32.RegistryValueKind.DWord); }
		}

		public static short ReadingDuration
		{
			get { return Convert.ToInt16(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingDuration", 30 * 1000)); } // 30 Seconds
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "ReadingDuration", value, Microsoft.Win32.RegistryValueKind.DWord); }
		}

		public static bool EnableLogging
		{
			get { return Convert.ToBoolean(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "EnableLogging", false)); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "EnableLogging", value.ToString(), Microsoft.Win32.RegistryValueKind.String); }
		}

		public static string WirelessSSID
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "WirelessSSID", "stargate"); }
			set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "WirelessSSID", value); }
		}

		public static double reservedDiskSpace
		{
			get { return 10.00; }
		}

		public static string errorFile
		{
			get { return "error.xml"; }
		}
		#endregion

		private static Guid GenerateID()
		{
			Guid retVal = Guid.NewGuid();
			SensorID = retVal;
			return retVal;
		}
	}
}