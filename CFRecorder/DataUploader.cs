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

        public static void UploadHealthLog()
        {
            string sensorID;
            DateTime time;
            short batteryLevel;
            decimal freeMemory;
            decimal memoryUsage;
            File.Move("HealthLog.txt", "HealthLog.bak"); //create a backup copy
            StreamReader reader = new StreamReader("HealthLog.bak");
            while (!reader.EndOfStream)
            {                
                string[] healthLog = reader.ReadLine().Split(new char[]{','});                                
                sensorID = healthLog[0];
                time = Convert.ToDateTime(healthLog[1]);
                batteryLevel = Convert.ToInt16(healthLog[2]);
                freeMemory = Convert.ToDecimal(healthLog[3]);
                memoryUsage = Convert.ToDecimal(healthLog[4]);
                
                QutSensors.Services.Service service = new QutSensors.Services.Service();
				service.Url = string.Format("http://{0}/Service.asmx", Settings.Server);

                if (!service.AddSensorStatus(sensorID, time, batteryLevel, freeMemory, memoryUsage))
                {
                    using (StreamWriter writer = new StreamWriter("HealthLog.txt", true))
                    {
                        writer.WriteLine("{0},{1},{2},{3},{4}", sensorID, time, batteryLevel, freeMemory, memoryUsage);
                    }
                }
            }
        }
	}
}