using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using QUT.Service;

public static class Settings
{
	#region Statics
	[DllImport("coredll.dll")]
	private extern static int GetDeviceUniqueID([In, Out] byte[] appdata, int cbApplictionData, int dwDeviceIDVersion, [In, Out] byte[] DeviceIDOuput, out uint pcbDeviceIDOutput);

	private static string GetDeviceID(string appData)
	{
		byte[] appBuffer = System.Text.UTF8Encoding.UTF8.GetBytes(appData);
		int appBufferSize = appBuffer.Length;

		byte[] output = new byte[20]; uint sizeOut = 20;
		GetDeviceUniqueID(appBuffer, appBufferSize, 1, output, out sizeOut);
		return Convert.ToBase64String(output);
	}
	#endregion

	#region Properties
	public static string HardwareID
	{
		get { return GetDeviceID("QutSensors"); }
	}

	public static Guid? DeploymentID
	{
		get
		{
			object retVal = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DeploymentID", null);
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
						Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\QUT", true);
						key.DeleteValue("SensorID", false);
					}
					catch { }
				}
			}

			return null;
		}

		set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DeploymentID", value.ToString()); }
	}

	public static DateTime? DeploymentStartTime
	{
		get
		{
			string retVal = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DeploymentStartTime", null);
			if (retVal == null)
				return null;
			return DateTime.Parse(retVal);
		}

		set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DeploymentStartTime", value.ToString()); }
	}

	public static string Name
	{
		get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Name", null); }
		set
		{
			if (value == null)
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\QUT", true);
				key.DeleteValue("Name", false);
				key.Close();
			}
			else
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Name", value);
		}
	}

	public static string Description
	{
		get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Description", null); }
		set
		{
			if (value == null)
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\QUT", true);
				key.DeleteValue("Description", false);
				key.Close();
			}
			else
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Description", value);
		}
	}

	public static string SensorDataPath
	{
		get {return ((string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", null) ?? "\\");}
		set
		{
			if (value == null)
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\QUT", true);
				key.DeleteValue("SensorDataPath", false);
				key.Close();
			}
			else
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "SensorDataPath", value);
		}
	}

	const string DefaultServer = "www.mquter.qut.edu.au/sensor/demo/";
	public static string Server
	{
		get { return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "Server", DefaultServer); }
		set
		{
			if (value == null)
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\QUT", true);
				key.DeleteValue("Server", false);
				key.Close();
			}
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
		set
		{
			if (value == null)
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\QUT", true);
				key.DeleteValue("LogPath", false);
				key.Close();
			}
			else
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "LogPath", value);
		}
	}

	public static bool DebugMode
	{
		get { return Convert.ToBoolean(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DebugMode", false)); }
		set { Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\QUT", "DebugMode", value); }
	}
	#endregion

	public static void UpdateDeployment(Deployment deployment)
	{
		DeploymentID = deployment.DeploymentID;
		DeploymentStartTime = deployment.DateStarted;
		Name = deployment.Name;
		Description = deployment.Description;
	}
}
