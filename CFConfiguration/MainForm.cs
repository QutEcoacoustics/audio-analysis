using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using QUT;
using System.IO;

namespace CFConfiguration
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			// Recordings
			txtFrequency.Text = Settings.ReadingFrequency.ToString();
			txtDuration.Text = Settings.ReadingDuration.ToString();
			txtRecordingPath.Text = Settings.SensorDataPath;

			// Info
			RefreshInfoTab();
		}

		#region Properties
		TabPage SelectedTab
		{
			get { return tabs.TabPages[tabs.SelectedIndex]; }
		}
		#endregion

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
			Utilities.Log("Recording settings updated.");
			Utilities.QueueNextAppRunForRecorder(DateTime.Now.AddSeconds(30));
			MessageBox.Show("Recording settings updated.");
		}

		private void cmdRefreshInfo_Click(object sender, EventArgs e)
		{
			RefreshInfoTab();
		}

		private void cmdChooseRecordingPath_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dia = new OpenFileDialog())
			{
				dia.FileName = "Filname Ignored";
				dia.InitialDirectory = Settings.SensorDataPath;
				if (dia.ShowDialog() == DialogResult.OK)
					txtRecordingPath.Text = Path.GetDirectoryName(dia.FileName);
			}
		}

		private void cmdRecordNow_Click(object sender, EventArgs e)
		{
			int duration = Convert.ToInt32(txtDuration.Text);

			DeviceManager.TakeRecording(DateTime.Now, DateTime.Now.AddMilliseconds(duration));
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (SelectedTab == tabInfo)
				txtCurrentTime.Text = DateTime.Now.ToString("dd/MM HH:mm:ss");
		}
		#endregion
	}
}