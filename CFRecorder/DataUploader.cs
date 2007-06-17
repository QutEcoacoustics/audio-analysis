using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;

namespace CFRecorder
{
	public static class DataUploader
	{
		static PDA.uploadFailure uf;

		static void LoadErrorLog()
		{
			uf = new PDA.uploadFailure();
			if (File.Exists("error.xml"))
				uf.ReadXml("error.xml");
		}

		public static void Upload(Recording recording)
		{
			MainForm.Log("Commencing upload...");

			FileInfo file = new FileInfo(recording.Target);
			try
			{
				byte[] buffer = new byte[file.Length];
				using (FileStream input = File.OpenRead(recording.Target))
					input.Read(buffer, 0, (int)file.Length);

				QutSensors.Services.Service service = new CFRecorder.QutSensors.Services.Service();
				service.Url = string.Format("http://{0}/QutSensors.WebService/Service.asmx", Settings.Server);
				service.AddAudioReading(Settings.SensorID.ToString(), null, recording.StartTime, buffer);
				MainForm.Log("Upload complete.");

				File.Delete(file.FullName); // To delete the audio recording once the file is uploaded.

				//TODO: Housekeeping starts here
				//1. If connection to server fail, keep a list of file that needs to be uploaded
				//2. Check for available diskspace.

				PDA.Utility.StartHouseKeeping();

			}
			catch (Exception e)
			{
				MainForm.Log("Upload failed - storing for later upload.\r\n{0}", e);
				// Upload failed...
				// TODO: we should retry this again sometime when the network comes back.

				// TODO: Write to uploadfailed.xml
				if (uf == null)
					LoadErrorLog();
				DataRow dr;
				dr = uf.NewRow();
				dr[1] = DateTime.Now.ToLongDateString();
				dr[2] = file.FullName;
				uf.Rows.Add(dr);
				uf.WriteXml("error.xml");
			}
		}
	}
}