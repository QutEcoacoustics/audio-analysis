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

        public const string DefaultConfigExt = ".cfg";


        public void LoadBrowserSettings()
        {
            this.DefaultSourceDir = AppConfigHelper.GetDir("DefaultSourceDir", true);
            this.DefaultConfigDir = AppConfigHelper.GetDir("DefaultConfigDir", true);
            this.DefaultOutputDir = AppConfigHelper.GetDir("DefaultOutputDir", true);
            this.diSourceDir = this.DefaultSourceDir;
            this.diConfigDir = this.DefaultConfigDir;
            this.diOutputDir = this.DefaultOutputDir;

            this.AnalysisList = AppConfigHelper.GetStrings("AnalysisList", ',');
            this.AnalysisName = AppConfigHelper.GetString("DefaultAnalysisName");
            this.fiAnalysisConfig = new FileInfo(Path.Combine(diConfigDir.FullName, AnalysisName + DefaultConfigExt));

            this.DefaultSegmentDuration = AppConfigHelper.GetDouble("DefaultSegmentDuration");
            this.DefaultResampleRate    = AppConfigHelper.GetInt("DefaultResampleRate");
            this.SourceFileExt          = AppConfigHelper.GetString("SourceFileExt");
            this.AudacityExe = AppConfigHelper.GetFile("AudacityExe", false);
            this.SonogramBackgroundThreshold = AppConfigHelper.GetDouble("SonogramBackgroundThreshold");
            this.TrackHeight = AppConfigHelper.GetInt("TrackHeight");
            this.TrackCount = AppConfigHelper.GetInt("TrackCount");
            this.TrackNormalisedDisplay = AppConfigHelper.GetBool("TrackNormalisedDisplay");
        }


        public FileInfo AudacityExe { get; private set; }
        //public string AnalysisName { get; private set; }
        //public int FrameLength { get; private set; }
        public int DefaultResampleRate { get; private set; }
        //public double FrameOverlap { get; private set; }
        public double DefaultSegmentDuration { get; private set; }  //measured in minutes
        //public int SegmentOverlap { get; private set; }   //measured in seconds
        public double SonogramBackgroundThreshold { get; private set; }
        public int TrackHeight { get; private set; }
        public int TrackCount { get; private set; }
        public bool TrackNormalisedDisplay { get; private set; }
        public string SourceFileExt { get; private set; }
        public string AnalysisName { get; set; }
        public string[] AnalysisList { get; private set; }

        public DirectoryInfo DefaultSourceDir { get; private set; }
        public DirectoryInfo DefaultConfigDir { get; private set; }
        public DirectoryInfo DefaultOutputDir { get; private set; }
        public DirectoryInfo diSourceDir { get; set; }
        public DirectoryInfo diConfigDir { get; set; }
        public DirectoryInfo diOutputDir { get; set; }

        public FileInfo fiSourceRecording { get; set; }
        public FileInfo fiAnalysisConfig  { get; set; }
        public FileInfo fiCSVFile         { get; set; }
        public FileInfo fiSegmentRecording { get; set; }
    }
}
