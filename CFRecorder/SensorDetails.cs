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
			txtID.Text = Settings.Current.SensorName;
			dtpDuration.Value = DateTime.Today.AddMilliseconds(Settings.Current.ReadingDuration);
			dtpFrequency.Value = DateTime.Today.AddMilliseconds(Settings.Current.ReadingFrequency);
			chkEnableLogging.Checked = Settings.Current.EnableLogging;
			service.BeginFindSensor(Settings.Current.SensorID.ToString(), new AsyncCallback(service_FoundSensor), null);
		}

		void service_FoundSensor(IAsyncResult ar)
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
				catch (WebException) {}

				if (sensor != null)
				{
					txtID.Text = sensor.Name;
					txtName.Text = sensor.FriendlyName;
					txtDescription.Text = sensor.Description;
				}
				txtID.Enabled = txtName.Enabled = txtDescription.Enabled = true;
			}
		}

		private void mnuSave_Click(object sender, EventArgs e)
		{
			try
			{
				service.UpdateSensor(Settings.Current.SensorID.ToString(), txtID.Text, txtName.Text, txtDescription.Text);
			}
			catch (WebException) { }

			Settings.Current.SensorName = txtID.Text;
			Settings.Current.ReadingDuration = Convert.ToInt16(dtpDuration.Value.TimeOfDay.TotalMilliseconds);
			Settings.Current.ReadingFrequency = Convert.ToInt32(dtpFrequency.Value.TimeOfDay.TotalMilliseconds);
			Settings.Current.EnableLogging = chkEnableLogging.Checked;
			DialogResult = DialogResult.OK;
		}

		private void mnuCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}