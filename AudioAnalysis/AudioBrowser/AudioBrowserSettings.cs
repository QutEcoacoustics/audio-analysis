namespace AudioBrowser
{
    using System.IO;

    using Acoustics.Shared;

    public class AudioBrowserSettings
    {
        //OBLIGATORY KEYS REQUIRED FOR ALL ANALYSES - these are used by the browser to run an analysis
        public const string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public const string key_SEGMENT_OVERLAP  = "SEGMENT_OVERLAP";
        public const string key_FRAME_LENGTH     = "FRAME_LENGTH";
        public const string key_FRAME_OVERLAP    = "FRAME_OVERLAP";
        public const string key_RESAMPLE_RATE    = "RESAMPLE_RATE";


        /*
        private const int DefaultTrackHeight = 20; //number of tracks to appear in the visual index
        private const int DefaultTrackCount = 15; //pixel height of track in the visual index
        private const int DefaultSegmentDuration = 1;
        private const int DefaultResampleRate = 17640;
        private const int DefaultFrameLength = 512;
        private const int DefaultLowFreqBound = 500;
        private const double DefaultFrameOverlap = 0.0;
        private const double DefaultSonogramBackgroundThreshold = 4.0;  //dB
        AUDACITY_PATH=C:\Program Files (x86)\Audacity 1.3 Beta (Unicode)\audacity.exe
        */


        public void LoadBrowserSettings()
        {
            this.fiAnalysisConfig = AppConfigHelper.GetFile("DefaultConfigFile", true);
            this.DefaultSourceDir = AppConfigHelper.GetDir("DefaultSourceDir", true);
            this.DefaultOutputDir = AppConfigHelper.GetDir("DefaultOutputDir", true);
            this.diSourceDir = this.DefaultSourceDir;
            this.diOutputDir = this.DefaultOutputDir;
            this.AudacityExe = AppConfigHelper.GetFile("AudacityExe", false);
            this.SonogramBackgroundThreshold = AppConfigHelper.GetDouble("SonogramBackgroundThreshold");
            this.TrackHeight = AppConfigHelper.GetInt("TrackHeight");
            this.TrackCount = AppConfigHelper.GetInt("TrackCount");
            this.TrackNormalisedDisplay = AppConfigHelper.GetBool("TrackNormalisedDisplay");
            this.SourceFileExt = AppConfigHelper.GetString("SourceFileExt");
            this.AnalysisName = AppConfigHelper.GetString("DefaultAnalysisName");
            this.AnalysisList = AppConfigHelper.GetStrings("AnalysisList", ',');
        }

        //public void LoadAnalysisSettings()
        //{
        //    this.AnalysisName = AppConfigHelper.GetString("DefaultAnalysisName");
        //    this.ResampleRate = AppConfigHelper.GetInt("ResampleRate");
        //    this.SegmentDuration = AppConfigHelper.GetInt("SegmentDuration");
        //    this.SegmentOverlap = AppConfigHelper.GetInt("SegmentOverlap");
        //    this.FrameLength = AppConfigHelper.GetInt("FrameLength");
        //    this.FrameOverlap = AppConfigHelper.GetDouble("FrameOverlap");
        //    this.LowFreqBound = AppConfigHelper.GetInt("LowFreqBound");
        //    this.MidFreqBound = AppConfigHelper.GetInt("MidFreqBound");
        //}

        public FileInfo AudacityExe { get; private set; }
        //public string AnalysisName { get; private set; }
        //public int FrameLength { get; private set; }
        //public int ResampleRate{ get; private set; }
        //public int LowFreqBound { get; private set; }
        //public int MidFreqBound { get; private set; }
        //public double SegmentDuration { get; private set; }  //measured in minutes
        //public int SegmentOverlap { get; private set; }   //measured in seconds
        //public double FrameOverlap { get; private set; }
        public double SonogramBackgroundThreshold { get; private set; }
        public int TrackHeight { get; private set; }
        public int TrackCount { get; private set; }
        public bool TrackNormalisedDisplay { get; private set; }
        public string SourceFileExt { get; private set; }
        public string AnalysisName { get; set; }
        public string[] AnalysisList { get; private set; }

        public DirectoryInfo DefaultSourceDir { get; private set; }
        public DirectoryInfo DefaultOutputDir { get; private set; }
        public DirectoryInfo diOutputDir { get; set; }
        public DirectoryInfo diSourceDir { get; set; }

        public FileInfo fiAnalysisConfig { get; set; }
        public FileInfo fiSourceRecording { get; set; }
        public FileInfo fiCSVFile { get; set; }
        public FileInfo fiSegmentRecording { get; set; }
        //public DirectoryInfo InputDir { get; set; }

    }
}
