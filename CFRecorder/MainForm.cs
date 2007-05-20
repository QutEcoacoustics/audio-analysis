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

namespace CFRecorder
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			TakeReading(Path.Combine(txtFolder.Text, string.Format("{0}_{1:yyyyMMdd-HHmmss}.wav", txtSensorName.Text, DateTime.Now)));
		}

		private void cmdRecordNow_Click(object sender, EventArgs e)
		{
			TakeReading(Path.Combine(txtFolder.Text, string.Format("{0}_{1:yyyyMMdd-HHmmss}.wav", txtSensorName.Text, DateTime.Now)));
		}

		string currentRecording;
		private void TakeReading(string path)
		{
			currentRecording = path;
			Record(path, 30);
		}

		FileStream stream;
		private void Record(string fileName, short duration)
		{
			txtLog.Text = string.Format("Starting recording - {0}\r\n{1}", fileName, txtLog.Text);
			txtLog.Update();
			OpenNETCF.Media.WaveAudio.Recorder recorder = new OpenNETCF.Media.WaveAudio.Recorder();
			stream = new FileStream(fileName, FileMode.CreateNew);
			recorder.DoneRecording += new OpenNETCF.Media.WaveAudio.WaveFinishedHandler(recorder_DoneRecording);
			recorder.RecordFor(stream, duration, OpenNETCF.Media.WaveAudio.SoundFormats.Mono16bit22kHz);
		}

		delegate void NullDelegate();

		void recorder_DoneRecording()
		{
			if (InvokeRequired)
				Invoke(new NullDelegate(recorder_DoneRecording));
			else
			{
				stream.Close();
				stream = null;
				txtLog.Text = "Recording complete\r\n" + txtLog.Text;
				txtLog.Update();

				Upload(currentRecording, Path.GetFileName(currentRecording));
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

		private void cmdSelectFolder_Click(object sender, EventArgs e)
		{
			using (System.Windows.Forms.SaveFileDialog dia = new SaveFileDialog())
			{
				dia.FileName = Path.Combine(txtFolder.Text, "Filename ignored...");
				if (dia.ShowDialog() == DialogResult.OK)
					txtFolder.Text = Path.GetDirectoryName(dia.FileName);
			}
		}
	}
}