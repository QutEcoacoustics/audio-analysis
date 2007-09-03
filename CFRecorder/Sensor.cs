using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using OpenNETCF.WindowsCE.Notification;
using System.Reflection;
using System.IO;
using System.Data;
using QUT.wsQUTSensor;
using System.Threading;

namespace QUT
{
    public class Sensor
    {
        static ManualResetEvent staticRecordingComplete;

        private HealthInfo hi = new HealthInfo();
        OpenNETCF.Media.WaveAudio.Recorder soundRecorder = new OpenNETCF.Media.WaveAudio.Recorder();

        #region Private Field
        string myFriendlyName;
        private string mySensorDataPath;
        wsQUTSensor.WebService ws = new WebService();
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
                return "QUTAndy";
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
            QueueNext(5);

            //Time table part.
            /* DataSet timeTable = new DataSet();
            timeTable = ws.GetTimeTable("207ec68d-dc5a-422f-8d62-98b074366ab2");
            GUI.dataGrid1.DataSource = timeTable.Tables[0];            
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string timeTableFile = Path.Combine(folderPath, "timetable.xml");
            timeTable.WriteXml(timeTableFile);
            MessageBox.Show(timeTableFile);
            timeTable.Clear();
            MessageBox.Show("Ready to load");
            timeTable.ReadXml(timeTableFile); */
            

            //Process Health Information here.
            //hi.Collect();   //Collect Health Information

            //TakeAudioRecording();

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
            //Check next recording time
            
            //TODO:If time is too short (less than 5 minutes), don't put it sleep.
            QueueNext(5);

            //TODO: Upload data to server.

            //Put the sensor into power idle mode
            // TODO: Before Mark's code arrived and we have found a better solution, we will keep restarting to ensure reliablity.

            PDA.SoftReset();
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
                QueueNext(30); 
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
