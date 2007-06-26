using System;
using System.Net;
using System.Windows.Forms;

namespace CFRecorder
{
	public partial class SensorDetails : Form
	{
		public SensorDetails()
		{
			InitializeComponent();
			txtID.Text = Settings.SensorName;
			txtName.Text = Settings.FriendlyName;
			txtDescription.Text = Settings.Description;
			dtpDuration.Value = DateTime.Today.AddMilliseconds(Settings.ReadingDuration);
			dtpFrequency.Value = DateTime.Today.AddMilliseconds(Settings.ReadingFrequency);
			chkEnableLogging.Checked = Settings.EnableLogging;

			QutSensors.Services.Service service = new QutSensors.Services.Service();
			service.Url = string.Format("http://{0}/Service.asmx", Settings.Server);
			service.BeginFindSensor(Settings.SensorID.ToString(), service_FoundSensor, service);
		}

		void service_FoundSensor(IAsyncResult ar)
		{
			if (!closed) // Handle case where form is closed before call completes
			{
				QutSensors.Services.Service service = (QutSensors.Services.Service)ar.AsyncState;
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
						if (sensor.Name != null && sensor.Name != "")
							txtID.Text = sensor.Name;
						if (sensor.FriendlyName != null && sensor.FriendlyName != "" && sensor.FriendlyName != sensor.Name && sensor.FriendlyName != "Unnamed sensor")
							txtName.Text = sensor.FriendlyName;
						if (sensor.Description != null && sensor.Description != "")
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
				QutSensors.Services.Service service = new QutSensors.Services.Service();
				service.Url = string.Format("http://{0}/Service.asmx", Settings.Server);
				service.UpdateSensor(Settings.SensorID.ToString(), txtID.Text, txtName.Text, txtDescription.Text);
			}
			catch (WebException) { }

			Settings.SensorName = txtID.Text;
			Settings.FriendlyName = txtName.Text;
			Settings.Description = txtDescription.Text;
			Settings.ReadingDuration = Convert.ToInt32(dtpDuration.Value.TimeOfDay.TotalMilliseconds);
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