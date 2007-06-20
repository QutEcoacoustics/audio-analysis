using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace CFRecorder
{
	public partial class SensorDetails : Form
	{
		QutSensors.Services.Service service = new CFRecorder.QutSensors.Services.Service();

		public SensorDetails()
		{
			InitializeComponent();
			txtID.Text = Settings.SensorName;
			txtName.Text = Settings.FriendlyName;
			txtDescription.Text = Settings.Description;
			dtpDuration.Value = DateTime.Today.AddMilliseconds(Settings.ReadingDuration);
			dtpFrequency.Value = DateTime.Today.AddMilliseconds(Settings.ReadingFrequency);
			chkEnableLogging.Checked = Settings.EnableLogging;
			service.BeginFindSensor(Settings.SensorID.ToString(), new AsyncCallback(service_FoundSensor), null);
		}

		void service_FoundSensor(IAsyncResult ar)
		{
			if (!closed) // Handle case where form is closed before call completes
			{
				if (InvokeRequired)
					BeginInvoke(new AsyncCallback(service_FoundSensor), ar);
				else
				{
					QutSensors.Services.Sensor sensor = null;
					try
					{
						sensor = service.EndFindSensor(ar);
					}
					catch (WebException) { }

					if (sensor != null)
					{
						txtID.Text = sensor.Name;
						txtName.Text = sensor.FriendlyName;
						txtDescription.Text = sensor.Description;
					}
					txtID.Enabled = txtName.Enabled = txtDescription.Enabled = true;
				}
			}
		}

		bool closed;
		private void mnuSave_Click(object sender, EventArgs e)
		{
			closed = true;
			try
			{
				service.UpdateSensor(Settings.SensorID.ToString(), txtID.Text, txtName.Text, txtDescription.Text);
			}
			catch (WebException) { }

			Settings.SensorName = txtID.Text;
			Settings.FriendlyName = txtName.Text;
			Settings.Description = txtDescription.Text;
			Settings.ReadingDuration = Convert.ToInt16(dtpDuration.Value.TimeOfDay.TotalMilliseconds);
			Settings.ReadingFrequency = Convert.ToInt32(dtpFrequency.Value.TimeOfDay.TotalMilliseconds);
			Settings.EnableLogging = chkEnableLogging.Checked;
			DialogResult = DialogResult.OK;
		}

		private void mnuCancel_Click(object sender, EventArgs e)
		{
			closed = true;
			DialogResult = DialogResult.Cancel;
		}
	}
}