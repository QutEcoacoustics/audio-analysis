using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AudioAnalysis;
using System.IO;
using System.Net;
using AudioTools;

using QutSensors.Processor;
using Settings = QutSensors.Processor.Settings;

namespace ProcessorUI
{
	public partial class MainForm : Form
	{
		PollingSystem pollingSystem = new PollingSystem();

		public MainForm()
		{
			InitializeComponent();

			pollingSystem.Stopped += new EventHandler(processor_Stopped);
			pollingSystem.Log += Log;
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			txtWorker.Text = Settings.WorkerName;
			pollingSystem.FilesProcessed = Settings.FilesProcessed;
			pollingSystem.TotalDuration = Settings.TotalDuration;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			pollingSystem.StopAndWait();
		}

		void cmdStart_Click(object sender, EventArgs e)
		{
			if (pollingSystem.State == PollingSystem.ProcessorState.Ready)
			{
				cmdStart.Text = "&Stop";
				pollingSystem.Start();
			}
			else if (pollingSystem.State == PollingSystem.ProcessorState.Running)
			{
				pollingSystem.Stop();
				cmdStart.Enabled = false;
				cmdStart.Text = "&Start";
			}
		}

		void processor_Stopped(object sender, EventArgs e)
		{
			if (InvokeRequired)
				Invoke(new EventHandler(processor_Stopped), sender, e);
			else
			{
				cmdStart.Text = "&Start";
				cmdStart.Enabled = true;
				Utilities.MinimizeMemory();
			}
		}

		void txtWorker_TextChanged(object sender, EventArgs e)
		{
			Settings.WorkerName = txtWorker.Text;
		}

		void Log(object sender, string log)
		{
			if (InvokeRequired)
				BeginInvoke((EventHandler)delegate { Log(sender, log); });
			else
			{
				txtLog.Text = DateTime.Now.ToString("HH:mm:ss") + ":" + log + "\r\n" + txtLog.Text;
				if (txtLog.Text.Length > 10000)
					txtLog.Text = txtLog.Text.Substring(0, 8000);
				Settings.FilesProcessed = pollingSystem.FilesProcessed;
				Settings.TotalDuration = pollingSystem.TotalDuration;
				statusTotal.Text = "Total: " + pollingSystem.FilesProcessed;
				statusDuration.Text = "Duration: " + pollingSystem.TotalDuration;
				statusThreads.Text = "Threads: " + pollingSystem.ThreadsRunning;
			}
		}

		private void cmdOptions_Click(object sender, EventArgs e)
		{
			using (var dia = new OptionsForm())
				dia.ShowDialog(this);
		}
	}
}