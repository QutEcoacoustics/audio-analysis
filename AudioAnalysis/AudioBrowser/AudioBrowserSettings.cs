namespace AudioBrowser
{
    using System;
    using System.IO;

    using QutSensors.Shared.Tools;

    public class AudioBrowserSettings
    {
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

        public static bool[] displayColumn = { false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false };
        public static bool[] weightedIndexColumn = { false, false, false, false, false, false, true, false, false, false, false, false, false, true, true, true, true, false };
        public static double[] comboWeights = { 0.0, 0.4, 0.1, 0.4, 0.1 };  //IMPORTANT THIS ARRAY SIZE MUST EQUAL TRUE COUNT IN weightedIndexColumn
        //                       SegmentCount = 0.0;   H[avSpectrum] = 0.4;   H[varSpectrum] = 0.1;  NumberOfClusters = 0.4; avClusterDuration = 0.1;


        public void LoadSettings()
        {
            this.DefaultSourceDir = AppConfigHelper.GetDir("DefaultSourceDir", true);
            this.DefaultOutputDir = AppConfigHelper.GetDir("DefaultOutputDir", true);
            this.SourceDir = this.DefaultSourceDir;
            this.OutputDir = this.DefaultOutputDir;
            this.AudacityExe = AppConfigHelper.GetFile("AudacityExe", false);
            this.FrameLength = AppConfigHelper.GetInt("FrameLength");
            this.ResampleRate = AppConfigHelper.GetInt("ResampleRate");
            this.LowFreqBound = AppConfigHelper.GetInt("LowFreqBound");
            this.SegmentDuration = AppConfigHelper.GetInt("SegmentDuration");
            this.FrameOverlap = AppConfigHelper.GetDouble("FrameOverlap");
            this.SonogramBackgroundThreshold = AppConfigHelper.GetDouble("SonogramBackgroundThreshold");
            this.TrackHeight = AppConfigHelper.GetInt("TrackHeight");
            this.TrackCount = AppConfigHelper.GetInt("TrackCount");
        }

        public FileInfo AudacityExe { get; private set; }
        public int FrameLength { get; private set; }
        public int ResampleRate{ get; private set; }
        public int LowFreqBound { get; private set; }
        public int SegmentDuration{ get; private set; }
        public double FrameOverlap{ get; private set; }
        public double SonogramBackgroundThreshold { get; private set; }
        public int TrackHeight { get; private set; }
        public int TrackCount { get; private set; }

        public DirectoryInfo DefaultSourceDir { get; private set; }
        public DirectoryInfo DefaultOutputDir { get; private set; }
        public DirectoryInfo OutputDir { get; set; }
        public DirectoryInfo SourceDir { get; set; }

        public FileInfo fiSourceRecording { get; set; }
        public FileInfo fiCSVFile { get; set; }
        public FileInfo fiSegmentrecording { get; set; }
        //public DirectoryInfo InputDir { get; set; }

    }
}
