using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AudioStuff;
using System.IO;
using System.Net;
using AudioTools;
using ProcessorUI.WebServices;

namespace ProcessorUI
{
	public partial class MainForm : Form
	{
		ProcessorManager processor = new ProcessorManager();

		public MainForm()
		{
			InitializeComponent();

			processor.Stopped += new EventHandler(processor_Stopped);
			processor.Log += Log;
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			txtWorker.Text = Settings.WorkerName;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			processor.StopAndWait();
		}

		void cmdStart_Click(object sender, EventArgs e)
		{
			if (processor.State == ProcessorManager.ProcessorState.Ready)
			{
				cmdStart.Text = "&Stop";
				processor.Start();
			}
			else if (processor.State == ProcessorManager.ProcessorState.Running)
			{
				processor.Stop();
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
				txtLog.Text = log + "\r\n" + txtLog.Text;
				if (txtLog.Text.Length > 10000)
					txtLog.Text = txtLog.Text.Substring(0, 8000);
			}
		}
	}
}