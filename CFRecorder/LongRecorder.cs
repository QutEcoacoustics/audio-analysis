using System;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using QUT;

namespace CFRecorder
{
	// Altered Usage Example:
	//
    //--Begining
	//LongRecorder recordingInfo = new LongRecorder("\\Storage Card\\filename.asf", 10000.0);
	//recordingInfo.SetupAudioRecording();
    //
    //--When ready to record
    //recordingInfo.BeginAudioRecorind
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

        private AutoResetEvent startWait;

        //Is this object currently recording aution
        public bool recording = false;

        //Imports, can't unload.
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean InitializeAudioRecording();
        
        // ## Removed unsafe, see how it works without it ##
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean PrepareAudioRecording([MarshalAs(UnmanagedType.LPWStr)]String str);
        
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean EndAudioRecording();
        
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean PowerOnDisplay();
        
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern Boolean PowerOffDisplay();

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
            startWait = new AutoResetEvent(false);
            InitializeAudioRecording();
            SetupAudioRecording();
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
        public bool SetupAudioRecording()
        {
            if (!recording)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DoRecording), this);
                return true;
            }
            return false;
        }

		AutoResetEvent recordingFinished = new AutoResetEvent(false);

        public bool BeginAudioRecording()
        {
            startWait.Set();
            return true;
        }

        /// <summary>
        /// Starts the audio recording in DirectShow thread, sleeps for recording time then
        /// stops recording in DirectShow thread.
        /// </summary>
        /// <param name="recordingInfo">Recording info (time and name)</param>
        private void DoRecording(object recordingInfo)
        {
            PDA.Video.PowerOffScreen();
            if (Settings.DebugMode)
                Utilities.Log("DEBUG - Recording Start");
            PrepareAudioRecording(((LongRecorder)recordingInfo).FileLocation);
            startWait.WaitOne();       
            ((LongRecorder)recordingInfo).startTime = DateTime.Now;
            ((LongRecorder)recordingInfo).endTime = (startTime + new TimeSpan(recordingTime));
            Thread.Sleep(((LongRecorder)recordingInfo).RecordingTime);
            EndAudioRecording();
            if (Settings.DebugMode)
            {
                PDA.Video.PowerOnScreen();
                Utilities.Log("DEBUG - Recording Finished"); 
            }
            recording = false;
			recordingFinished.Set();
        }

		public void WaitTillEnd()
		{
			recordingFinished.WaitOne();
		}
    }
}