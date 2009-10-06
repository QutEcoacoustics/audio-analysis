using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Settings = QutSensors.Processor.Settings;

namespace ProcessorUI
{
	public partial class OptionsForm : Form
	{
		public OptionsForm()
		{
			InitializeComponent();
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			txtServer.Text = Settings.Server;
			txtDataFolder.Text = Settings.TempFolder;
			updThreads.Value = Settings.NumberOfThreads;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			Settings.Server = txtServer.Text;
			Settings.TempFolder = txtDataFolder.Text;
			Settings.NumberOfThreads = (int)updThreads.Value;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void cmdChooseDataFolder_Click(object sender, EventArgs e)
		{
			using (var dia = new OpenFileDialog())
			{
				dia.InitialDirectory = txtDataFolder.Text;

				dia.Title = "Select the Data Folder";
				dia.CheckFileExists = false;

				dia.FileName = "Filename will be ignored...";
				dia.Filter = "Folders|no.files";

				if (dia.ShowDialog(this) == DialogResult.OK)
				{
					txtDataFolder.Text = Path.GetDirectoryName(dia.FileName);
				}
			}
		}
	}
}