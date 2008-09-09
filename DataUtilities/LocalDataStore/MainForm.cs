using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LocalDataStore.QutSensors;
using System.IO;
using System.Net;
using System.Threading;

namespace LocalDataStore
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			txtDataFolder.Text = Settings.DataFolder;
			FindDeployments();
		}

		private void FindDeployments()
		{
			SetStatus("Downloading deployments", 0.0);
			var pars = new LocalDataStore.QutSensors.DeploymentSearchParameters();
			pars.IncludeTests = false;
			var client = new QutSensors.AudioReadingsSearchSoapClient();
			client.BeginFindDeployments(pars, EndFindDeployments, client);
		}

		List<DeploymentSearchResult> deployments;
		void EndFindDeployments(IAsyncResult ar)
		{
			if (InvokeRequired)
				Invoke(new AsyncCallback(EndFindDeployments), ar);
			else
			{
				var client = ar.AsyncState as QutSensors.AudioReadingsSearchSoapClient;
				try
				{
					List<string> deselectedDeployments = ParseCommaSeparatedList(Settings.DeselectedDeployments);

					deployments = new List<DeploymentSearchResult>(client.EndFindDeployments(ar));
					foreach (var d in deployments)
						lstDeployments.Items.Add(d.Name, !deselectedDeployments.Contains(d.Name));
					client.Close();
					SetStatus("Deployments found.", 0.0);
				}
				catch (Exception e)
				{
					MessageBox.Show(string.Format("Error getting deployments: {0}", e));
				}
			}
		}

		private void mnuDownload_Click(object sender, EventArgs e)
		{
			
		}

		private void SaveDeselectedDeployments()
		{
			List<string> dd = new List<string>();
			for (int i = 0; i < lstDeployments.Items.Count; i++)
				if (!lstDeployments.GetItemChecked(i))
					dd.Add(deployments[i].Name);
			Settings.DeselectedDeployments = string.Join(",", dd.ToArray());
		}

		Queue<DeploymentSearchResult> deploymentsToGrab = new Queue<DeploymentSearchResult>();
		private void BeginDownload(string folder)
		{
			SetStatus("Downloading data", 0.0);
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			foreach (int i in lstDeployments.CheckedIndices)
				deploymentsToGrab.Enqueue(deployments[i]);

			backgroundWorker.RunWorkerAsync(folder);
		}

		void SetStatus(string text, double progress)
		{
			try
			{
				lblStatus.Text = text;
				progressBar.Value = (int)(100 * progress);
			}
			catch { }
		}

		AutoResetEvent stopEvent = new AutoResetEvent(false);
		double deploymentProgress = 0.0;
		private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			bool cancelled = false;
			downloadCount = 0;
			try
			{
				double baseDeploymentProgress = 1.0 / deploymentsToGrab.Count;
				string folder = e.Argument as string;
				while (deploymentsToGrab.Count > 0)
				{
					if (cancelled || stopEvent.WaitOne(0, false))
					{
						cancelled = true;
						break;
					}
					deploymentProgress += baseDeploymentProgress;

					var d = deploymentsToGrab.Dequeue();
					backgroundWorker.ReportProgress((int)(100 * deploymentProgress), "Downloading deployment " + d.Name);
					Invoke((EventHandler)delegate { lstDeployments.SelectedIndex = deployments.IndexOf(d); });

					var client = new AudioReadingsSearchSoapClient();
					var result = client.GetDeploymentReadings(d.ID, Convert.ToInt32(txtDownloadCount.Text));
					int i = 0;
					foreach (var r in result.Readings)
					{
						if (cancelled || stopEvent.WaitOne(0, false))
						{
							cancelled = true;
							break;
						}
						string ext = Path.GetExtension(r.AudioUrl);
						string url = r.AudioUrl.Substring(0, r.AudioUrl.Length - ext.Length) + ".wav";
						backgroundWorker.ReportProgress((int)(100 * (deploymentProgress + (baseDeploymentProgress * (i++ / (double)result.Readings.Count())))), string.Format("Downloading {0} ({1} total)", Path.GetFileName(url), downloadCount));

						string target = Path.Combine(folder, Path.Combine(folder,  d.Name + "/" + d.Name + "_" + Path.GetFileName(url)));
						if (!File.Exists(target))
							DownloadFile(url, target);

						// Download spectrogram - DISABLED
						/*target = Path.Combine(folder, Path.Combine(folder, d.Name + "/" + d.Name + "_" + Path.GetFileNameWithoutExtension(url) + ".jpg"));
						url = r.ImageUrl;
						if (!File.Exists(target))
							DownloadFile(url, target);*/
					}
				}
			}
			catch (Exception ex)
			{
				Invoke((EventHandler)delegate { MessageBox.Show(this, ex.ToString()); });
			}
		}

		int downloadCount;
		void DownloadFile(string url, string target)
		{
			try
			{
				if (!Directory.Exists(Path.GetDirectoryName(target)))
					Directory.CreateDirectory(Path.GetDirectoryName(target));
				WebClient client = new WebClient();
				client.DownloadFile(new Uri("http://www.mquter.qut.edu.au" + url), target);
				downloadCount++;
			}
			catch
			{
				if (File.Exists(target))
					File.Delete(target);
			}
		}

		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			SetStatus(e.UserState as string, e.ProgressPercentage / 100.0);
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			cmdStart.Text = "Go!";
			cmdStart.Enabled = true;
			MessageBox.Show(string.Format("Downloaded {0} readings", downloadCount));
			downloadCount = 0;
		}

		public static List<string> ParseCommaSeparatedList(string list)
		{
			if (list == null)
				return new List<string>();
			List<string> retVal = new List<string>();
			foreach (string s in list.Split(','))
				if (s != null && s.Length > 0)
					retVal.Add(s.Trim());
			return retVal;
		}

		private void cmdStart_Click(object sender, EventArgs e)
		{
			if (backgroundWorker.IsBusy)
			{
				cmdStart.Text = "Cancelling...";
				cmdStart.Enabled = false;
				stopEvent.Set();
			}
			else
			{
				bool goAhead = true;
				if (string.IsNullOrEmpty(txtDataFolder.Text))
					goAhead = ChooseDataFolder();
				else
					Settings.DataFolder = txtDataFolder.Text;

				if (goAhead)
				{
					cmdStart.Text = "Cancel";
					SaveDeselectedDeployments();
					BeginDownload(Settings.DataFolder);
				}
			}
		}

		private void cmdChooseDataFolder_Click(object sender, EventArgs e)
		{
			ChooseDataFolder();
		}

		private bool ChooseDataFolder()
		{
			using (FolderBrowserDialog dia = new FolderBrowserDialog())
			{
				dia.Description = "Choose which folder to save the data in";
				dia.SelectedPath = Settings.DataFolder;
				if (dia.ShowDialog(this) == DialogResult.OK)
				{
					txtDataFolder.Text = Settings.DataFolder = dia.SelectedPath;
					return true;
				}
			}
			return false;
		}

		private void txtDataFolder_TextChanged(object sender, EventArgs e)
		{
			txtDataFolder.ForeColor = Directory.Exists(txtDataFolder.Text) ? Color.Black : Color.Red;
		}

		private void cmdUploadBrowse_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dia = new OpenFileDialog())
			{
				if (dia.ShowDialog(this) == DialogResult.OK)
					txtUploadPath.Text = dia.FileName;
			}
		}

		public int MaxChunkSize
		{
			get { return (int)updChunkSize.Value; }
		}

		private void cmdSoapUpload_Click(object sender, EventArgs e)
		{

		}

		static readonly string DeviceID = "Virtual Sensor";

		private void cmdRestUpload_Click(object sender, EventArgs e)
		{
			var f = new FileInfo(txtUploadPath.Text);

			using (var fs = f.OpenRead())
			{
				int numChunks = (int)((f.Length + MaxChunkSize - 1) / MaxChunkSize);

				bool more = MaxChunkSize < f.Length;
				Guid serverID = AddAudioReadingFirstREST(DeviceID, f.CreationTime, GetMimeTypeFromExtension(f.Extension), fs, more);
				long position = fs.Position;
				if (!more)
					SetStatus(string.Format("Uploaded '{0}' - {1}", txtUploadPath.Text, serverID), 1.0);
				else
					SetStatus(string.Format("Uploaded '{0}' (1 of {1}) - {2}", txtUploadPath.Text, numChunks, serverID), 1.0 / numChunks);

				while (position < f.Length)
				{
					int chunk = (int)(position / MaxChunkSize) + 1;
					AddAudioReadingNextREST(serverID, fs, position, position + MaxChunkSize < f.Length);
					position = fs.Position;
					SetStatus(string.Format("Uploaded '{0}' ({1} of {2}) - {3}", txtUploadPath.Text, chunk, numChunks, serverID), chunk / (double)numChunks);
				}
			}
		}

		public const string AsfMimeType = "video/x-ms-asf";
		public const string WavMimeType = "audio/x-wav";
		public const string Mp3MimeType = "audio/mpeg";
		public const string BinMimeType = "application/octet-stream";
		public const string WavpackMimeType = "audio/x-wv";
		public static string GetMimeTypeFromExtension(string ext)
		{
			switch (ext.ToLower())
			{
				case ".asf":
					return AsfMimeType;
				case ".wav":
					return WavMimeType;
				case ".mp3":
					return Mp3MimeType;
				case ".wv":
					return WavpackMimeType;
				default:
					return BinMimeType;
			}
		}

		private Guid AddAudioReadingFirstREST(string deviceID, DateTime time, string mimeType, FileStream stream, bool more)
		{
			Guid audioReadingID = Guid.Empty;
			AddAudioReadingREST(
				deviceID,
				mimeType,
				time,
				more,
				null,
				ref audioReadingID,
				stream);
			return audioReadingID;
		}

		private void AddAudioReadingNextREST(Guid audioReadingID, FileStream stream, long offset, bool more)
		{
			AddAudioReadingREST(
				null,
				null,
				null,
				more,
				offset,
				ref audioReadingID,
				stream);
		}

		private void AddAudioReadingREST(string deviceID, string mimeType, DateTime? time, bool more, long? offset, ref Guid audioReadingID, FileStream fileStream)
		{
			var remaining = Math.Min(MaxChunkSize, fileStream.Length - fileStream.Position);

			var url = new StringBuilder();
			//url.AppendFormat("{0}/rest/data/{1}?More={2}", Settings.ServerUrlBase, audioReadingID, more);
			//url.AppendFormat("{0}/RestInterface/DataUpload.ashx?AudioReadingID={1}&More={2}", "http://www.mquter.qut.edu.au/sensor/demo", audioReadingID, more);
			url.AppendFormat("{0}/RestInterface/DataUpload.ashx?AudioReadingID={1}&More={2}", "http://localhost:4040/WebFrontend", audioReadingID, more);
			if (deviceID != null) url.AppendFormat("&DeviceID={0}", deviceID);
			if (mimeType != null) url.AppendFormat("&MimeType={0}", mimeType);
			if (time.HasValue) url.AppendFormat("&Time={0:yyyy-MM-ddTHHmmss}", time.Value);
			if (offset.HasValue) url.AppendFormat("&Offset={0}", offset.Value);

			System.Net.ServicePointManager.Expect100Continue = false;
			var req = (HttpWebRequest)WebRequest.Create(url.ToString());
			req.Method = "PUT";
			req.ContentType = "application/octet-stream"; ;
			req.ContentLength = remaining;
			req.AllowWriteStreamBuffering = false;
			req.Pipelined = false;
			//req.ReadWriteTimeout = (int)Settings.WebServiceTimeout.TotalMilliseconds;
			//req.Timeout = (int)Settings.WebServiceTimeout.TotalMilliseconds;

			var buffer = new byte[4096];
			using (Stream reqStream = req.GetRequestStream())
			{
				while (remaining > 0)
				{
					int br = fileStream.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
					reqStream.Write(buffer, 0, br);
					remaining -= br;
				}
				reqStream.Close();
			}

			try
			{
				var response = (HttpWebResponse)req.GetResponse();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					string audioReadingIdString;
					using (StreamReader reader = new StreamReader(response.GetResponseStream()))
						audioReadingIdString = reader.ReadToEnd();
					if (!string.IsNullOrEmpty(audioReadingIdString))
						audioReadingID = new Guid(audioReadingIdString);
				}
				else throw new WebException(null, null, WebExceptionStatus.ProtocolError, response);
			}
			catch (WebException e)
			{
				string details;
				using (StreamReader reader = new StreamReader(e.Response.GetResponseStream()))
					details = reader.ReadToEnd();
				MessageBox.Show(details);
				throw;
			}
		}
	}
}