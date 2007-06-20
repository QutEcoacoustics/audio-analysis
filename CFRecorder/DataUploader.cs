using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

namespace CFRecorder
{
	public static class DataUploader
	{   
		public static void Upload(Recording recording)
		{
			if (recording.StartTime == null)
				MainForm.Log("Unable to upload recording - start time not specified");
			else
			{
				MainForm.Log("Commencing upload...");

				FileInfo file = new FileInfo(recording.GetPath());
				try
				{
					byte[] buffer = new byte[file.Length];
					using (FileStream input = file.OpenRead())
						input.Read(buffer, 0, (int)file.Length);

					QutSensors.Services.Service service = new CFRecorder.QutSensors.Services.Service();
					service.Url = string.Format("http://{0}/Service.asmx", Settings.Server);
					service.AddAudioReading(Settings.SensorID.ToString(), null, recording.StartTime.Value, buffer);
					MainForm.Log("Upload complete.");

					File.Delete(file.FullName); // To delete the audio recording once the file is uploaded.
				}
				catch (Exception e)
				{
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
					Upload(recording);
				}
			}
        }
	}
}