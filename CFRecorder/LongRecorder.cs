using System;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CFRecorder
{
    //Usage Example:
    //
    //LongRecorder recordingInfo = new LongRecorder("\\Storage Card\\filename.asf", 10000.0);
    //recordingInfo.PerformRecording();
    //while (recordingInfo.Active)
    //{
    //    Thread.Sleep(20);
    //    //DANCEBABY!!
    //}

    // Altered Usage Example:
    //
    //LongRecorder recordingInfo = new LongRecorder("\\Storage Card\\filename.asf", 10000.0);
    //recordingInfo.PerformRecording();
    //recordingInfo.WaitTillEnd();


    /// <summary>
    /// Wrapper/Implementer Class for longer audio recordings
    /// </summary>
    class LongRecorder
    {
        //Milliseconds to record for
        private int recordingTime;

        //Location to save too '\\Storage Card\\.....'
        private string fileLocation;

        private DateTime startTime;

        private DateTime endTime;

        //Is this object currently recording aution
        public bool recording = false;

        //Imports, can't unload.
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean InitializeAudioRecording();

        // ## Removed unsafe, see how it works without it ##
        [DllImport("AudioPhotoLibrary.dll")]
        unsafe public static extern Boolean PrepareAudioRecording([MarshalAs(UnmanagedType.LPWStr)]String str);
        //public static extern Boolean PrepareAudioRecording();

        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean StartAudioRecording();

        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean EndAudioRecording();

        /// <summary>
        /// Empty Constructor, won't record anything
        /// </summary>
        public LongRecorder()
        {
            recordingTime = 0;
            fileLocation = "";
        }

        /// <summary>
        /// Constructor to record audio.  Sets parameters and initializes DirectAudio
        /// so it can begin recording quicker.
        /// </summary>
        /// <param name="name">File name to save to</param>
        /// <param name="time">Milliseconds to record for</param>
        public LongRecorder(string name, int time)
        {
            recordingTime = time;
            fileLocation = name;
            InitializeAudioRecording();
            PrepareAudioRecording(name);
        }

        #region Properties
        /// <summary>
        /// Property to return milliseconds the object will record for
        /// </summary>
        public int RecordingTime
        {
            get
            {
                return recordingTime;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return endTime;
            }
        }

        public int TicksRemaining
        {
            get
            {
                return (int)(endTime - startTime).Ticks;
            }
        }

        /// <summary>
        /// Property to show if the object is currently recording or not.
        /// </summary>
        public bool Active
        {
            get
            {
                return recording;
            }
        }

        /// <summary>
        /// Property to show file location that audio is being saved to
        /// </summary>
        public string FileLocation
        {
            get
            {
                return fileLocation;
            }
        }
        #endregion

        /// <summary>
        /// Creates a seperate thread that will perform the recording for the specified time.
        /// </summary>
        /// <returns>If the thread has been created (won't create if already recording)</returns>
        public bool PerformRecording()
        {
            if (!recording)
            {
                recording = true;
                // NOTE: To speed up starting should we start the thread earlier and have it waiting on an event?
                // In fact... perhaps we even build in the waiting till start time to minimise the path to recording.
                ThreadPool.QueueUserWorkItem(new WaitCallback(DoRecording), this);
                // NOTE: Should we set these in the recording thread since that's more accurate as to when it actually started
                startTime = DateTime.Now;
                endTime = (startTime + new TimeSpan(recordingTime));
                return true;
            }
            return false;
        }

        AutoResetEvent recordingFinished = new AutoResetEvent(false);

        /// <summary>
        /// Starts the audio recording in DirectShow thread, sleeps for recording time then
        /// stops recording in DirectShow thread.
        /// </summary>
        /// <param name="recordingInfo">Recording info (time and name)</param>
        private void DoRecording(object recordingInfo)
        {
            PDA.Video.PowerOffScreen();
            StartAudioRecording();
            Thread.Sleep(((LongRecorder)recordingInfo).RecordingTime);
            EndAudioRecording();
            if (Settings.DebugMode)
                PDA.Video.PowerOnScreen();
            recording = false;
            recordingFinished.Set();
        }

        public void WaitTillEnd()
        {
            recordingFinished.WaitOne();
        }
    }
}