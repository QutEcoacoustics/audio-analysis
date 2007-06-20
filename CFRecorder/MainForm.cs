using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OpenNETCF.Net.Ftp;
using System.Net.Sockets;
using System.Net;
using OpenNETCF.Net;
using CFRecorder.QutSensors.Services;
using CFRecorder.QutSensors;
using System.Reflection;
using System.Threading;

namespace CFRecorder
{
	public partial class MainForm : Form
	{
		#region Statics
		static MainForm current;

		public static void LogError(Exception e)
		{
			// This is here to allow errors to be logged even if EnableLogging is set off if we wish to later.
			// Not all errors are sent here yet.
			Log(e.ToString());
		}

		public static void Log(string format, params object[] args)
		{
			try
			{
				if (current == null)
				{
					if (Settings.EnableLogging)
					{
						using (StreamWriter writer = new StreamWriter("Log.txt", true))
						{
							writer.Write(DateTime.Now.ToString("g"));
							writer.Write(": ");
							writer.WriteLine(format, args);
						}
					}
				}
				else
				{
					if (current.InvokeRequired)
						current.Invoke((EventHandler)delegate(object s, EventArgs a) { Log(format, args); });
					else
					{
						current.txtLog.Text = DateTime.Now.ToString("HH:mm:ss") + ": " + string.Format(format, args) + "\r\n" + current.txtLog.Text;
						current.txtLog.Update();
					}
				}
			}
			catch { }
		}

		public static void QueueNextReading()
		{
			DateTime nextRun = DateTime.Now.AddMilliseconds(Settings.ReadingFrequency);
			Log("Queueing next reading for {0}", nextRun);
			OpenNETCF.WindowsCE.Notification.Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, nextRun);
		}

		public static void ClearQueuedReading()
		{
			OpenNETCF.WindowsCE.Notification.Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, DateTime.MaxValue);
		}

		static ManualResetEvent staticRecordingComplete;
		public static bool TakeReading()
		{
			try
			{
				Log("Taking reading.");
				staticRecordingComplete = new ManualResetEvent(false);
				Recording recording = new Recording();
				recording.DoneRecording += new EventHandler(staticRecording_DoneRecording);
				recording.RecordFor(Settings.ReadingDuration);
				return true;
			}
			catch (Exception e)
			{
				LogError(e);
				return false;
			}
		}

		public static void WaitForReading()
		{
			staticRecordingComplete.WaitOne();
		}

		static void staticRecording_DoneRecording(object sender, EventArgs e)
		{
			Log("Reading complete");
			Recording recording = (Recording)sender;
			DataUploader.Upload(recording);
			//TODO: Housekeeping starts here
			//1. If connection to server fail, keep a list of file that needs to be uploaded
			//2. Check for available diskspace.
			PDA.Utility.StartHouseKeeping();

			staticRecordingComplete.Set();
		}
		#endregion
        
        public MainForm()
		{
			current = this;
            try
            {
                InitializeComponent();

                txtSensorName.Text = Settings.SensorName;
                txtFolder.Text = Settings.SensorDataPath;
                txtServer.Text = Settings.Server;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
		}

		#region Event Handlers
		private void timer_Tick(object sender, EventArgs e)
		{
			TakeReading();
		}

		private void cmdSelectFolder_Click(object sender, EventArgs e)
		{
			using (System.Windows.Forms.SaveFileDialog dia = new SaveFileDialog())
			{
				dia.FileName = Path.Combine(txtFolder.Text, "Filename ignored...");
				if (dia.ShowDialog() == DialogResult.OK)
					Settings.SensorDataPath = txtFolder.Text = Path.GetDirectoryName(dia.FileName);
			}
		}

		private void txtSensorName_TextChanged(object sender, EventArgs e)
		{
			Settings.SensorName = txtSensorName.Text;
		}

		private void mnuExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void mnuRecordNow_Click(object sender, EventArgs e)
		{
			TakeReading();
		}

		private void txtServer_TextChanged(object sender, EventArgs e)
		{
			Settings.Server = txtServer.Text;
		}
		#endregion

		List<Adapter> GetWirelessAdapters()
		{
			List<Adapter> retVal = new List<Adapter>();
			foreach (Adapter adapter in Networking.GetAdapters())
				if (adapter.IsWirelessZeroConfigCompatible)
					retVal.Add(adapter);
			return retVal;
		}

		private void wirelessTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				List<Adapter> adapters = GetWirelessAdapters();
				foreach (Adapter adapter in adapters)
				{
					// Check if already connected to an appropriate network
					if (adapter.AssociatedAccessPoint == Settings.WirelessSSID)
					{
						UpdateWirelessLabel(adapters);
						return;
					}
				}

				foreach (Adapter adapter in adapters)
				{
					// Trys to connect every wireless adapter to the network... Probably not the best option, but sufficient
					EAPParameters eapParams = new EAPParameters();
					eapParams.EapFlags = EAPFlags.Disabled;
					eapParams.EapType = EAPType.PEAP;
					eapParams.Enable8021x = false;
					adapter.SetWirelessSettingsAddEx(Settings.WirelessSSID, true, (byte[])null, 1, AuthenticationMode.Ndis802_11AuthModeOpen, WEPStatus.Ndis802_11EncryptionDisabled, eapParams);
					//adapter.SetWirelessSettingsEx(SSID, true, (byte[])null, AuthenticationMode.Ndis802_11AuthModeOpen);
					adapter.RebindAdapter();
				}

				UpdateWirelessLabel(adapters);
			}
			catch
			{
			}
		}

		private void UpdateWirelessLabel(List<Adapter> adapters)
		{
			foreach (Adapter adapter in adapters)
				if (adapter.AssociatedAccessPoint == Settings.WirelessSSID)
				{
					lblWireless.Text = string.Format("Wireless: {0} ({1}dB)", adapter.CurrentIpAddress, adapter.SignalStrengthInDecibels);
					return;
				}
			lblWireless.Text = "Wireless: Not connected";
		}

		private void mnuSensorDetails_Click(object sender, EventArgs e)
		{
			using (SensorDetails dia = new SensorDetails())
				if (dia.ShowDialog() == DialogResult.OK)
					txtSensorName.Text = Settings.SensorName;
		}

        private void menuItem2_Click(object sender, EventArgs e)
        {
            PDA.Video.PowerOffScreen();
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            PDA.Hardware.SoftReset();
        }

		private void mnuStartPeriodicRecording_Click(object sender, EventArgs e)
		{
			PDA.Video.PowerOffScreen();
			QueueNextReading();
			Application.Exit();
		}
        private void menuItem4_Click(object sender, EventArgs e)
        {
            OpenNETCF.IO.DriveInfo DI = new OpenNETCF.IO.DriveInfo("\\");
            MessageBox.Show(string.Format("{0:0.00} mb left", PDA.Utility.BytesToMegabytes(DI.AvailableFreeSpace)));            
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {
            MessageBox.Show(PDA.Hardware.GetBatteryLeftPercentage().ToString());
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            PDA.Hardware.TurnOffBackLight();
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Format("{0:0.00} mb left",(PDA.Hardware.GetAvailablePhysicalMemory()/1024)),this.Text);
        }

		private void mnuProcessFailures_Click(object sender, EventArgs e)
		{
			DataUploader.ProcessFailures();
		}
	}
}