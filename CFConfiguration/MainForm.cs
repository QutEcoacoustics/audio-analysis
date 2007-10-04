using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using QUT;
using System.IO;
using CFRecorder;
using QUT.Service;
using System.Reflection;

namespace CFConfiguration
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			// Recordings
			txtFrequency.Text = Settings.ReadingFrequency.ToString();
			txtDuration.Text = Settings.ReadingDuration.ToString();
			txtRecordingPath.Text = Settings.SensorDataPath;

			DateTime endTime;
			txtNextRecording.Text = DeviceManager.CalculateNextRecordingTime(out endTime).ToLongTimeString();

			// Server
			txtServer.Text = Settings.Server;

			// Identity
			txtHardwareID.Text = Settings.HardwareID;
			RefreshIdentityTab();

			// Info
			chkDebugMode.Checked = Settings.DebugMode;
			RefreshInfoTab();
		}

		#region Properties
		TabPage SelectedTab
		{
			get { return tabs.TabPages[tabs.SelectedIndex]; }
		}
		#endregion

		private void RefreshIdentityTab()
		{
			txtDeploymentID.Text = Settings.DeploymentID == null ? "" : Settings.DeploymentID.ToString();
			txtDeploymentStarted.Text = Settings.DeploymentStartTime == null ? "" : Settings.DeploymentStartTime.Value.ToString("dd/mm/yy HH:mm:ss");
			txtDeploymentName.Text = Settings.Name;
			txtDeploymentDescription.Text = Settings.Description;
		}

		private void RefreshInfoTab()
		{
			txtLastRecording.Text = Settings.LastRecordingTime == null ? "Never" : Settings.LastRecordingTime.ToString();
			string log = txtLog.Text = Utilities.LoadLog();
			if (log != null && log.Length > 0)
			{
				txtLog.Select(log.Length - 1, 1);
				txtLog.ScrollToCaret();
			}
		}

		#region Event Handlers
		#region Recordings
		private void cmdUploadRecordings_Click(object sender, EventArgs e)
		{
			try
			{
				int uploaded = DataUploader.ProcessFailures();
				MessageBox.Show("Uploaded " + uploaded + " recordings.", "Upload Complete");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Upload failed");
			}
		}

		private void cmdRecordNow_Click(object sender, EventArgs e)
		{
			int duration = Convert.ToInt32(txtDuration.Text);

			DeviceManager.TakeRecording(DateTime.Now.AddSeconds(15), DateTime.Now.AddSeconds(15).AddMilliseconds(duration));
		}

		private void cmdChooseRecordingPath_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog dia = new SaveFileDialog())
			{
				dia.FileName = Path.Combine(Settings.SensorDataPath, "Filename Ignored");
				if (dia.ShowDialog() == DialogResult.OK)
					txtRecordingPath.Text = Path.GetDirectoryName(dia.FileName);
			}
		}

		private void cmdSaveRecordings_Click(object sender, EventArgs e)
		{
			int freq = Convert.ToInt32(txtFrequency.Text);
			int duration = Convert.ToInt32(txtDuration.Text);
			if (!Directory.Exists(txtRecordingPath.Text))
			{
				MessageBox.Show("Invalid folder specified for recording path.");
				return;
			}

			Settings.ReadingFrequency = freq;
			Settings.ReadingDuration = duration;
			Settings.SensorDataPath = txtRecordingPath.Text;

			if (nextRecordingChanged)
			{
				try
				{
					DateTime nextRecording = DateTime.Parse(txtNextRecording.Text);
					Settings.LastRecordingTime = nextRecording.AddMilliseconds(-1 * freq);
				}
				catch (FormatException)
				{
					MessageBox.Show("Invalid format for next recording time.", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
				}
			}

			DateTime endTime;
			txtNextRecording.Text = DeviceManager.CalculateNextRecordingTime(out endTime).ToLongTimeString();

			Utilities.Log("Recording settings updated.");
			Utilities.QueueNextAppRunForRecorder(DateTime.Now.AddSeconds(15));
			MessageBox.Show("Recording settings updated.");
		}

		bool nextRecordingChanged;
		private void txtNextRecording_TextChanged(object sender, EventArgs e)
		{
			nextRecordingChanged = true;
		}
		#endregion

		#region Info
		private void cmdRefreshInfo_Click(object sender, EventArgs e)
		{
			RefreshInfoTab();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (SelectedTab == tabInfo)
				txtCurrentTime.Text = DateTime.Now.ToString("dd/MM HH:mm:ss");
		}

		private void chkDebugMode_CheckStateChanged(object sender, EventArgs e)
		{
			Settings.DebugMode = chkDebugMode.Checked;
		}
		#endregion

		#region Server
		private void cmdTestServer_Click(object sender, EventArgs e)
		{
			try
			{
				Service service = DataUploader.GetServiceProxy(txtServer.Text);
				if (service.TestConnection())
					MessageBox.Show("Server contacted", "Server Connection Succeeded");
				else
					MessageBox.Show("Server return false for connection.", "Unexpected error");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Error contacting server");
			}
		}

		private void cmdSaveServer_Click(object sender, EventArgs e)
		{
			Settings.Server = txtServer.Text;
		}

		private void cmdResetServer_Click(object sender, EventArgs e)
		{
			Settings.Server = null;
			txtServer.Text = Settings.Server;
		}
		#endregion

		#region Identity
		private void cmdStartDeployment_Click(object sender, EventArgs e)
		{
			//if (MessageBox.Show("Are you sure?", "Start new deployment", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
			if (MessageBox.Show("Are you sure you wish to start a new deployment called: '" + txtNewDeploymentName.Text + "'?", "Start New Deployment", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
			{
				Service service = DataUploader.GetServiceProxy(Settings.Server);
				Deployment deployment = service.StartDeployment(Settings.HardwareID, txtNewDeploymentName.Text);
				if (deployment != null)
					Settings.UpdateDeployment(deployment);

				RefreshIdentityTab();
			}
		}

		private void cmdUpdateDeployment_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Not implemented yet");
		}

		private void cmdRefreshDeployment_Click(object sender, EventArgs e)
		{
			Service service = DataUploader.GetServiceProxy(Settings.Server);
			Deployment deployment = service.GetLatestDeployment(Settings.HardwareID);
			if (deployment != null)
				Settings.UpdateDeployment(deployment);
			RefreshIdentityTab();
			MessageBox.Show("Refreshed");
		}
		#endregion
		#endregion
	}
}