using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OpenNETCF.Net.Ftp;
using System.Net.Sockets;
using System.Net;
using OpenNETCF.Net;
using CFRecorder.QutSensors.Services;
using QutSensors; 

namespace CFRecorder
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			txtSensorName.Text = Settings.Current.SensorName;
			txtFolder.Text = Settings.Current.SensorDataPath;
			txtServer.Text = Settings.Current.Server;
		}

		#region Event Handlers
		private void timer_Tick(object sender, EventArgs e)
		{
			TakeReading(Path.Combine(txtFolder.Text, string.Format("{0}_{1:yyyyMMdd-HHmmss}.wav", txtSensorName.Text, DateTime.Now)));
		}

		private void cmdSelectFolder_Click(object sender, EventArgs e)
		{
			using (System.Windows.Forms.SaveFileDialog dia = new SaveFileDialog())
			{
				dia.FileName = Path.Combine(txtFolder.Text, "Filename ignored...");
				if (dia.ShowDialog() == DialogResult.OK)
					Settings.Current.SensorDataPath = txtFolder.Text = Path.GetDirectoryName(dia.FileName);
			}
		}

		private void txtSensorName_TextChanged(object sender, EventArgs e)
		{
			Settings.Current.SensorName = txtSensorName.Text;
		}

		private void mnuExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void mnuRecordNow_Click(object sender, EventArgs e)
		{
			TakeReading(Path.Combine(txtFolder.Text, string.Format("{0}_{1:yyyyMMdd-HHmmss}.wav", txtSensorName.Text, DateTime.Now)));
		}

		private void txtServer_TextChanged(object sender, EventArgs e)
		{
			Settings.Current.Server = txtServer.Text;
		}
		#endregion

		string currentRecording;
		private void TakeReading(string path)
		{
			currentRecording = path;
			Record(path, 10);
		}

		private void Record(string fileName, short duration)
		{
			txtLog.Text = string.Format("{0:HH:mm:ss} - Starting recording - {1}\r\n{2}", DateTime.Now, fileName, txtLog.Text);
			txtLog.Update();

			Recording recording = new Recording(fileName);
			recording.DoneRecording += new EventHandler(recording_DoneRecording);
			recording.RecordFor(duration);
		}

		void recording_DoneRecording(object sender, EventArgs e)
		{
			if (InvokeRequired)
				Invoke(new EventHandler(recording_DoneRecording), sender, e);
			else
			{
				txtLog.Text = string.Format("{0:HH:mm:ss} - Recording complete\r\n{1}", DateTime.Now, txtLog.Text);
				txtLog.Update();
				Recording recording = (Recording)sender;
				Upload2(recording);
			}
		}

		void Upload2(Recording recording)
		{
			try
			{
				txtLog.Text = string.Format("Commencing upload...\r\n") + txtLog.Text;
				txtLog.Update();

				QutSensors.Services.Service service = new CFRecorder.QutSensors.Services.Service();
				service.Url = string.Format("http://{0}/QutSensors.WebService/Service.asmx", Settings.Current.Server);

				FileInfo file = new FileInfo(recording.Target);
				byte[] buffer = new byte[file.Length];
				using (FileStream input = File.OpenRead(recording.Target))
					input.Read(buffer, 0, (int)file.Length);

				service.AddAudioReading(Settings.Current.SensorID.ToString(), null, recording.StartTime, buffer);

				txtLog.Text = string.Format("Upload complete.\r\n") + txtLog.Text;
				txtLog.Update();
			}
			catch (Exception e)
			{
				txtLog.Text = string.Format("Upload failed - storing for later upload.\r\n{0}\r\n", e) + txtLog.Text;
				txtLog.Update();
				// Upload failed...
				// TODO: we should retry this again sometime when the network comes back.
			}
		}

		private void Upload(string currentRecording, string targetFileName)
		{
			txtLog.Text = string.Format("Commencing upload...\r\n") + txtLog.Text;
			txtLog.Update();

			byte[] sourceBuffer;
			using (FileStream sourceStream = new FileStream(currentRecording, FileMode.Open, FileAccess.Read))
			{
				sourceBuffer = new byte[sourceStream.Length];
				sourceStream.Read(sourceBuffer, 0, (int)sourceStream.Length);
			}

			try
			{
				WebRequest request = WebRequest.Create(string.Format("http://www.mquter.qut.edu.au/sensor/demo/Upload.aspx?Path=" + UrlEncode(targetFileName)));
				//WebRequest request = WebRequest.Create(string.Format("http://131.181.111.186:2477/WebFrontend/Upload.aspx?Path=" + UrlEncode(targetFileName)));
				request.Method = "POST";
				request.PreAuthenticate = true;
				request.Credentials = System.Net.CredentialCache.DefaultCredentials;
				request.ContentLength = sourceBuffer.Length;

				using (BinaryWriter writer = new BinaryWriter(request.GetRequestStream()))
					writer.Write(sourceBuffer, 0, sourceBuffer.Length);

				WebResponse response = request.GetResponse();
				response.Close();

				txtLog.Text = string.Format("Upload complete.\r\n") + txtLog.Text;
				txtLog.Update();

				File.Delete(currentRecording);
			}
			catch (WebException)
			{
				txtLog.Text = string.Format("Upload failed - storing for later upload.\r\n") + txtLog.Text;
				txtLog.Update();
				// Upload failed...
				// TODO: we should retry this again sometime when the network comes back.
			}
		}

		public static string UrlEncode(string instring)
		{
			StringReader strRdr = new StringReader(instring);
			StringWriter strWtr = new StringWriter();
			int charValue = strRdr.Read();
			while (charValue != -1)
			{
				if (((charValue >= 48) && (charValue <= 57)) // 0-9
				|| ((charValue >= 65) && (charValue <= 90)) // A-Z
				|| ((charValue >= 97) && (charValue <= 122))) // a-z
				{
					strWtr.Write((char)charValue);
				}
				else if (charValue == 32) // Space
				{
					strWtr.Write("+");
				}
				else
				{
					strWtr.Write("%{0:x2}", charValue);
				}
				charValue = strRdr.Read();
			}
			return strWtr.ToString();
		}

		string SSID
		{
			get { return "stargate"; } // TODO: Throw to app.config
		}

		List<Adapter> GetWirelessAdapters()
		{
			List<Adapter> retVal = new List<Adapter>();
			foreach (Adapter adapter in Networking.GetAdapters())
				if (adapter.IsWirelessZeroConfigCompatible)
					retVal.Add(adapter);
			return retVal;
		}

		private void wirelessTimer_Tick(object sender, EventArgs e)
		{
			List<Adapter> adapters = GetWirelessAdapters();
			foreach (Adapter adapter in adapters)
			{
				// Check if already connected to an appropriate network
				if (adapter.AssociatedAccessPoint == SSID)
				{
					UpdateWirelessLabel(adapters);
					return;
				}
			}

			foreach (Adapter adapter in adapters)
			{
				/*txtLog.Text = string.Format("Attempting wireless on {0} ({1})\r\n", adapter.AssociatedAccessPoint, adapter.SignalStrengthInDecibels) + txtLog.Text;
				txtLog.Update();*/

				// Trys to connect every wireless adapter to the network... Probably not the best option, but sufficient
				EAPParameters eapParams = new EAPParameters();
				eapParams.EapFlags = EAPFlags.Disabled;
				eapParams.EapType = EAPType.PEAP;
				eapParams.Enable8021x = false;
				adapter.SetWirelessSettingsAddEx(SSID, true, (byte[])null, 1, AuthenticationMode.Ndis802_11AuthModeOpen, WEPStatus.Ndis802_11EncryptionDisabled, eapParams);
				//adapter.SetWirelessSettingsEx(SSID, true, (byte[])null, AuthenticationMode.Ndis802_11AuthModeOpen);
				adapter.RebindAdapter();
			}

			UpdateWirelessLabel(adapters);
		}

		private void UpdateWirelessLabel(List<Adapter> adapters)
		{
			foreach (Adapter adapter in adapters)
				if (adapter.AssociatedAccessPoint == SSID)
				{
					lblWireless.Text = string.Format("Wireless: {0} ({1}dB)", adapter.CurrentIpAddress, adapter.SignalStrengthInDecibels);
					return;
				}
			lblWireless.Text = "Wireless: Not connected";
		}

		private void mnuWSTest_Click(object sender, EventArgs e)
		{
			try
			{
				QutSensors.Services.Service service = new CFRecorder.QutSensors.Services.Service();
				service.Url = string.Format("http://{0}/QutSensors.WebService/Service.asmx", Settings.Current.Server);

				Sensor sensor = service.FindSensor("QUT01");
			}
			catch (WebException ex)
			{
				HttpWebResponse response = (HttpWebResponse)ex.Response;
				txtLog.Text = string.Format("Error: {0}\r\n{1}", response.StatusDescription, txtLog.Text);
			}
		}

        private void menuItem2_Click(object sender, EventArgs e)
        {
            PDA.PowerOffScreen();           
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            PDA.SoftReset();
        }
	}
}