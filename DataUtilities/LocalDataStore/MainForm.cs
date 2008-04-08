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
	}
}