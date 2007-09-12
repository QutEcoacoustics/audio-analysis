using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CFRecorder;
using System.Text.RegularExpressions;
using QUT;
using QUT.Service;
using System.Windows.Forms;

namespace CFRecorder
{
	public static class DataUploader
	{
		static Guid GetDeploymentID()
		{
			Guid? retVal = Settings.DeploymentID;
			if (retVal == null)
			{
				Service service = GetServiceProxy(Settings.Server);
				Deployment deployment = service.GetLatestDeployment(Settings.HardwareID);
				if (deployment == null)
					throw new Exception("DeploymentID is unavailable");
				retVal = deployment.DeploymentID;
				Settings.UpdateDeployment(deployment);
			}
			return retVal.Value;
		}

		public static bool Upload(Recording recording)
		{
			return Upload(recording, false);
		}

		public static bool Upload(Recording recording, bool silent)
		{
			return Upload(GetDeploymentID(), recording, silent);
		}

		public static bool Upload(Guid deploymentID, Recording recording, bool silent)
		{
			if (recording.StartTime == null)
			{
				if (!silent)
					Utilities.Log("Unable to upload recording - start time not specified");
			}
			else
			{
				if (!silent)
					Utilities.Log("Commencing upload...");
			}

			FileInfo file = new FileInfo(recording.GetPath());
			try
			{
				byte[] buffer = new byte[file.Length];
				using (FileStream input = file.OpenRead())
					input.Read(buffer, 0, (int)file.Length);

				Service service = GetServiceProxy(Settings.Server);
				service.AddAudioReading(deploymentID, null, recording.StartTime.Value, buffer);

				File.Delete(file.FullName);
				return true;
			}
			catch (Exception e) { Utilities.Log(e, "Uploading data"); }
			return false;
		}

		public static Service GetServiceProxy(string server)
		{
			Service service = new Service();
			service.Url = string.Format("http://{0}/Service.asmx", server);
			return service;
		}

		public static int ProcessFailures()
		{
			return ProcessFailures(-1);
		}

		public static int ProcessFailures(int maxUploads)
		{
			int uploaded = 0;
			Regex fileRegex = new Regex(@"^(?<id>.*?)_(?<date>\d{8}-\d{6})");

			foreach (string file in Directory.GetFiles(Settings.SensorDataPath))
			{
				if (maxUploads == 0)
					break;

				Match m = fileRegex.Match(file.Substring(1));
				if (m.Success)
				{
					string deploymentIDString = m.Groups["id"].Value;
					if (deploymentIDString != "Unknown")
					{
						Guid deploymentID;
						try
						{
							deploymentID = new Guid(deploymentIDString);
						}
						catch (FormatException)
						{
							MessageBox.Show(deploymentIDString);
							continue;
						}
						DateTime time = DateTime.ParseExact(m.Groups["date"].Value, "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat);
						Recording recording = new Recording(time);
						if (Upload(deploymentID, recording, true))
						{
							maxUploads--;
							uploaded++;
						}
					}
				}
			}
			return uploaded;
		}
	}
}