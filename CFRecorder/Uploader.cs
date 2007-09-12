using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CFRecorder;
using System.Text.RegularExpressions;
using QUT;

namespace CFRecorder
{
	public static class DataUploader
	{
		public static void Upload(Recording recording)
		{
			Upload(recording, false);
		}

		public static bool Upload(Recording recording, bool silent)
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

				QUT.Service.Service service = GetServiceProxy(Settings.Server);
				service.AddAudioReading(Settings.SensorID, null, recording.StartTime.Value, buffer);

				File.Delete(file.FullName);
				return true;
			}
			catch (Exception e) { Utilities.Log(e, "Uploading data"); }
			return false;
		}

		public static QUT.Service.Service GetServiceProxy(string server)
		{
			QUT.Service.Service service = new QUT.Service.Service();
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
			Regex fileRegex = new Regex(Settings.SensorName + @"_(?<date>\d{8}-\d{6})");

			foreach (string file in Directory.GetFiles(Settings.SensorDataPath))
			{
				if (maxUploads == 0)
					break;

				Match m = fileRegex.Match(file);
				if (m.Success)
				{
					DateTime time = DateTime.ParseExact(m.Groups["date"].Value, "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat);
					Recording recording = new Recording(time);
					if (Upload(recording, true))
					{
						maxUploads--;
						uploaded++;
					}
				}
			}
			return uploaded;
		}
	}
}