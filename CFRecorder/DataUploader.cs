using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CFRecorder
{
	public static class DataUploader
	{
		public static void Upload(Recording recording)
		{
			Upload(recording, false);
		}

		public static void Upload(Recording recording, bool silent)
		{
			if (recording.StartTime == null)
			{
				if (!silent)
					MainForm.Log("Unable to upload recording - start time not specified");
			}
			else
			{
				if (!silent)
					MainForm.Log("Commencing upload...");

				FileInfo file = new FileInfo(recording.GetPath());
				try
				{
					byte[] buffer = new byte[file.Length];
					using (FileStream input = file.OpenRead())
						input.Read(buffer, 0, (int)file.Length);

					QutSensors.Services.Service service = new QutSensors.Services.Service();
					service.Url = string.Format("http://{0}/Service.asmx", Settings.Server);
					service.AddAudioReading(Settings.SensorID.ToString(), null, recording.StartTime.Value, buffer);
					if (!silent)
						MainForm.Log("Upload complete.");

					File.Delete(file.FullName); // To delete the audio recording once the file is uploaded.
				}
				catch (Exception e)
				{
					if (!silent)
						MainForm.Log("Upload failed - storing for later upload.\r\n{0}", e);
				}
			}
		}
		
        public static void ProcessFailures()
        {
			Regex fileRegex = new Regex(Settings.SensorName + @"_(?<date>\d{8}-\d{6})");

			foreach (string file in Directory.GetFiles(Settings.SensorDataPath))
			{
				Match m = fileRegex.Match(file);
				if (m.Success)
				{
					DateTime time = DateTime.ParseExact(m.Groups["date"].Value, "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat);
					Recording recording = new Recording(time);
					Upload(recording, true);
				}
			}
        }
	}
}