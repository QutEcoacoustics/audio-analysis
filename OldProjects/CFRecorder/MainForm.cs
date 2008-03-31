using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CFRecorder.QutSensors.Services;
using OpenNETCF.Net;

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
            string currentLogFile = String.Format("Log{0}.txt",Settings.LogPosition);
			try
			{                
				if (current == null)
				{
					if (Settings.EnableLogging)
					{
						using (StreamWriter writer = new StreamWriter(currentLogFile, true))
						{
							writer.Write(DateTime.Now.ToString("g"));
							writer.Write(": ");
							writer.WriteLine(format, args);
						}

                        //HACK: Check file size here 
                        FileInfo fi = new FileInfo(currentLogFile);
                        if (fi.Length > 10000)  //If log file is bigger than 10k
                        {                            
                            int logPosition = Settings.LogPosition;
                            logPosition++;
                            if (logPosition > 5) // keep 5 rotating log file, so total of 50k
                                logPosition = 1;
                            Settings.LogPosition = logPosition;                            
                            fi = new FileInfo(String.Format("Log{0}.txt", Settings.LogPosition));
                            if (fi.Exists && fi.Length > 10000)
                                fi.Delete();
                        }
                        //TODO: We will have to decide whether we want to upload the old log file or not.
					}
				}
				else
				{
					if (current.InvokeRequired)
						current.Invoke((EventHandler)delegate { Log(format, args); });
					else
					{
						current.txtLog.Text = DateTime.Now.ToString("HH:mm:ss") + ": " + string.Format(format, args) + "\r\n" + current.txtLog.Text;
						current.txtLog.Update();
					}
				}
			}
			catch { }
		}

		public static DateTime QueueNextReading()
		{
			DateTime nextRun = DateTime.Now.AddMilliseconds(Settings.ReadingFrequency);
			Log("Queueing next reading for {0}", nextRun);
			OpenNETCF.WindowsCE.Notification.Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, nextRun);
			return nextRun;
		}

		public static void QueueNextReading(DateTime nextRun)
		{
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
				recording.DoneRecording += staticRecording_DoneRecording;
				recording.RecordFor(Settings.ReadingDuration);
				return true;
			}
			catch (Exception e)
			{
				LogError(e);
				return false;
			}
		}

		public static void SendStatus()
		{
			try
			{
				Service service = new Service();
				service.Url = string.Format("http://{0}/Service.asmx", Settings.Server);
                service.AddSensorStatus(Settings.SensorID.ToString(), DateTime.Now, PDA.Hardware.GetBatteryLeftPercentage(),
                   Convert.ToDecimal(PDAUtils.GetFreeMemory()), Convert.ToDecimal(PDAUtils.GetMemoryUsage()));                
                Log("Sensor status uploaded");
			}
			catch (Exception e)
			{                
                AddHealthLog("{0},{1},{2},{3},{4}",Settings.SensorID.ToString(), DateTime.Now, PDA.Hardware.GetBatteryLeftPercentage(),
                   Convert.ToDecimal(PDAUtils.GetFreeMemory()), Convert.ToDecimal(PDAUtils.GetMemoryUsage()));
				LogError(e);
			}
		}

        public static void AddHealthLog(string format, params object[] args)
        {
            using (StreamWriter writer = new StreamWriter("HealthLog.txt", true))
            {                
                writer.WriteLine(format, args);                
            }
        }



		public static void WaitForReading()
		{
			staticRecordingComplete.WaitOne();
		}

		static void staticRecording_DoneRecording(object sender, EventArgs e)
		{
			Recording recording = (Recording)sender;
			if (recording.Succeeded)
			{
				Log("Reading complete");
				DataUploader.Upload(recording);
				//TODO: Housekeeping starts here
				//1. If connection to server fail, keep a list of file that needs to be uploaded
				//2. Check for available diskspace.
				PDA.Utility.StartHouseKeeping();
			}
			else
				Log("Reading failed.");

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
			using (SaveFileDialog dia = new SaveFileDialog())
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
            SendStatus();
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

            // PDA.Video.PowerOffScreen();  //Don't turn off the screen temporarily to monitor the activity
            DateTime nextRun = DateTime.Now.AddMilliseconds(60000); //Take an immediate reading 1.5 minute after
            Log("Queueing next reading for {0}", nextRun);
            OpenNETCF.WindowsCE.Notification.Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, nextRun);
            //QueueNextReading();
            MessageBox.Show(string.Format("The System will now restarting itself and go into auto-recorder mode at frequency of {0} ",
                Settings.ReadingFrequency));
			PDA.Video.PowerOffScreen();
			// TODO: Move to Mark's reset code which sleeps for a bit first because that's recommended.
            PDA.Hardware.SoftReset(); //Application.Exit(); Perform reset instead of exit the program

            // ---- Original code goes here ---
            //PDA.Video.PowerOffScreen();  //Don't turn off the screen temporarily to monitor the activity
            //DateTime nextRun = DateTime.Now.AddMilliseconds(60000); //Take an immediate reading one minute after
            //Log("Queueing next reading for {0}", nextRun);
            //OpenNETCF.WindowsCE.Notification.Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, nextRun);            
			//QueueNextReading();
			//Application.Exit();
		}
        private void menuItem4_Click(object sender, EventArgs e)
        {
            OpenNETCF.IO.DriveInfo DI = new OpenNETCF.IO.DriveInfo("\\");
            MessageBox.Show(string.Format("{0:0.00} mb left", PDA.Utility.BytesToMegabytes(DI.AvailableFreeSpace)));            
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Microsoft.WindowsMobile.Status.SystemState.GetValue(Microsoft.WindowsMobile.Status.SystemProperty.PowerBatteryStrength).ToString());
            MessageBox.Show(Microsoft.WindowsMobile.Status.SystemState.GetValue(Microsoft.WindowsMobile.Status.SystemProperty.PowerBatteryState).ToString());
            MessageBox.Show(Microsoft.WindowsMobile.Status.SystemState.GetValue(Microsoft.WindowsMobile.Status.SystemProperty.PowerBatteryBackupStrength).ToString());
            MessageBox.Show(PDA.Hardware.GetBatteryLeftPercentage().ToString());
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            PDA.Hardware.TurnOffBackLight();
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Format("{0:0.00} mb left", (PDA.Hardware.GetAvailablePhysicalMemory()/1024)), Text);
        }

		private void mnuProcessFailures_Click(object sender, EventArgs e)
		{
			DataUploader.ProcessFailures();
		}

        private void menuItem8_Click(object sender, EventArgs e)
        {
            AdapterCollection na;
            Adapter adapter;
            na = OpenNETCF.Net.Networking.GetAdapters();
            adapter = na[0];
            byte[] b = adapter.MacAddress;
            int i;
            string s = "";
            for (i = 0; i <= b.Length - 1; i++)
            {
                s = s + string.Format("{0:x2}", b[i]) + ":";
            }
            MessageBox.Show(s);
        }

        private void menuItem9_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Format("Free memory: {0} \r\n Memory usage: {1}",
                PDA.Utility.BytesToMegabytes(PDAUtils.GetFreeMemory()).ToString("####.##"), PDAUtils.GetMemoryUsage().ToString()));                       
        }

        private void menuItem10_Click(object sender, EventArgs e)
        {
            AddHealthLog("{0},{1},{2},{3},{4}", Settings.SensorID.ToString(), DateTime.Now, PDA.Hardware.GetBatteryLeftPercentage(),
                   Convert.ToDecimal(PDAUtils.GetFreeMemory()), Convert.ToDecimal(PDAUtils.GetMemoryUsage()));
        }

        private void menuItem11_Click(object sender, EventArgs e)
        {
            DataUploader.UploadHealthLog();
        }
	}
}
