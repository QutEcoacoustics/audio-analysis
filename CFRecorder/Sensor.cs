using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using OpenNETCF.WindowsCE.Notification;
using System.Reflection;
using System.IO;
using System.Data;
using System.Threading;
using QUT.Service;

namespace CFRecorder
{
    public class Sensor
    {
        static ManualResetEvent staticRecordingComplete;

        private HealthInfo hi = new HealthInfo();
        OpenNETCF.Media.WaveAudio.Recorder soundRecorder = new OpenNETCF.Media.WaveAudio.Recorder();

        #region Private Field
        string myFriendlyName;
        private string mySensorDataPath;
        QUT.Service.Service ws = new Service();        
        #endregion        

        #region Constructor
        public Sensor() //This is the constructor of the sensor, everything starts here.
        {
        }

        #endregion

        #region Properties
        public string FriendlyName
        {
            get
            {
                return Settings.FriendlyName;
            }
            set
            {
                myFriendlyName = value;
            }
        }
        
        public string SensorDataPath
        {
            get { return "\\Storage Card\\"; }//return mySensorDataPath; }
            set { mySensorDataPath = value; }
        }

        public Guid ID
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
        #endregion

        #region Public Methods
        public void Start()
        {            
            Log("Auto queue @ {0}", DateTime.Now.AddMinutes(5));
            QueueNext(5); //Automatically queue for the next 5 mintues, anything happen it will still start again.

            RecordAudio();
            WaitForReading();
        }       

        public void QueueNext(int minutes)
        {
            DateTime nextRun = DateTime.Now.AddMinutes(minutes);
            Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, nextRun);
            Log("Queue @ {0}", nextRun);            
        }

        public void QueueNext()
        {
            QueueNext(30); //At this stage we just set to queue 30 minutes after automatically
        }
        
        public void QueueNext(DateTime time)
        {
            DateTime nextRun = time;
            Notify.RunAppAtTime(Assembly.GetExecutingAssembly().GetName().CodeBase, nextRun);
            Log("Queue @ {0}", nextRun);
        }
        

        public void TakeImageRecording()
        {
            throw new System.NotImplementedException();
        }

        public void Log(string format, params object[] args)
        {
            using (StreamWriter writer = new StreamWriter("\\Storage Card\\log.txt", true))
            {
                writer.Write(DateTime.Now.ToString("g"));
                writer.Write(": ");
                writer.WriteLine(format, args);
            }
        }


#endregion

        #region Internal Methods

        private void Finalise()
        {
            DataUploader.ProcessFailures();
            //Check next recording time
            
            //TODO:If time is too short (less than 5 minutes), don't put it sleep.
            QueueNext();

            //TODO: Upload unUploaded data to server.

            //Put the sensor into power idle mode
            // TODO: Before Mark's code arrived and we have found a better solution, we will keep restarting to ensure reliablity.

            PDA.Hardware.SoftReset();
        }
        #endregion
        
        #region Audio Recording 

        public void WaitForReading()
        {
            staticRecordingComplete.WaitOne();
        }

        private bool RecordAudio()
        {
            try
            {
                QueueNext();
                //LongRecorder r = new LongRecorder("andytest", 10000);
                //r.FileLocation = "\\Storage Card";
                //r.PerformRecording();
                //while (r.Active)
                //{
                //    Thread.Sleep(20);
                //}
                //MessageBox.Show("Done");

                Log("Taking reading {0} ", Settings.ReadingDuration);
                staticRecordingComplete = new ManualResetEvent(false);
                Recording recording = new Recording();
                recording.DoneRecording += staticRecording_DoneRecording;
                recording.RecordFor(Settings.ReadingDuration);
                return true;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                return false;
            }
        }

        private void staticRecording_DoneRecording(object sender, EventArgs e)
        {
            Recording recording = (Recording)sender;

            if (recording.Succeeded)
            {
                Log("Reading done.");             
                DataUploader.Upload(recording);
                Log("Data upload done.");             
                Finalise();
            }
            else
            {
                Log("Reading fail.");
            }
            staticRecordingComplete.Set();
        }
        #endregion
    }
}
