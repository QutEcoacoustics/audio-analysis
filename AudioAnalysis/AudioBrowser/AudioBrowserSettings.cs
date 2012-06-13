namespace AudioBrowser
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

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
            try
            {
                this.DefaultSourceDir = AppConfigHelper.GetDir("DefaultSourceDir", true);
                this.DefaultConfigDir = AppConfigHelper.GetDir("DefaultConfigDir", true);
                this.DefaultOutputDir = AppConfigHelper.GetDir("DefaultOutputDir", true);
                this.diSourceDir = this.DefaultSourceDir;
                this.diConfigDir = this.DefaultConfigDir;
                this.diOutputDir = this.DefaultOutputDir;   
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show("WARNING: " + ex.ToString());
                MessageBox.Show("  CHECK contents of app.config file.");

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            } //catch

            //CHECK THESE FILES EXIST
            //<add key="AudioUtilityFfmpegExe" value="audio-utils\ffmpeg\ffmpeg.exe" />
            //<add key="AudioUtilityFfprobeExe" value="audio-utils\ffmpeg\ffprobe.exe" />
            //<add key="AudioUtilityWvunpackExe" value="audio-utils\wavpack\wvunpack.exe" />
            //<add key="AudioUtilityMp3SpltExe" value="audio-utils\mp3splt\mp3splt.exe" />
            //<add key="AudioUtilitySoxExe" value="audio-utils\sox\sox.exe" />
            try
            {
                var fiEXE = AppConfigHelper.GetFile("AudioUtilityFfmpegExe", true);
                fiEXE = AppConfigHelper.GetFile("AudioUtilityFfprobeExe", true);
                fiEXE = AppConfigHelper.GetFile("AudioUtilityWvunpackExe", true);
                fiEXE = AppConfigHelper.GetFile("AudioUtilityMp3SpltExe", true);
                fiEXE = AppConfigHelper.GetFile("AudioUtilitySoxExe", true);
                this.AudacityExe = AppConfigHelper.GetFile("AudacityExe", false);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("WARNING: " + ex.ToString());
                MessageBox.Show("  CHECK contents of app.config file.");

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            } //catch


            try
            {
                //this.AnalysisList = AppConfigHelper.GetStrings("AnalysisList", ',');
                this.AnalysisIdentifier = AppConfigHelper.GetString("DefaultAnalysisName");
                this.fiAnalysisConfig = new FileInfo(Path.Combine(diConfigDir.FullName, AnalysisIdentifier + DefaultConfigExt));

                this.DefaultSegmentDuration = AppConfigHelper.GetDouble("DefaultSegmentDuration");
                this.DefaultResampleRate = AppConfigHelper.GetInt("DefaultResampleRate");
                this.SourceFileExt = AppConfigHelper.GetString("SourceFileExt");
                this.SonogramBackgroundThreshold = AppConfigHelper.GetDouble("SonogramBackgroundThreshold");
                this.TrackHeight = AppConfigHelper.GetInt("TrackHeight");
                this.TrackCount = AppConfigHelper.GetInt("TrackCount");
                this.TrackNormalisedDisplay = AppConfigHelper.GetBool("TrackNormalisedDisplay");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("WARNING: COULD NOT READ ALL ITEMS FROM THE APP.CONFIG. CHECK contents of app.config in working directory.");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            } //catch
        } // LoadBrowserSettings()


        //public void ConfirmAllOtherFilesExist()
        //{
        //    try
        //    {
                //if (this.diConfigDir == null)
                //{
                //    MessageBox.Show("WARNING: A valid config directory has not been set.\n\nCheck entry in the application file: app.config ");
                //    throw new Exception();
                //}
                //else
                //if (!this.diConfigDir.Exists)
                //{
                //    MessageBox.Show("WARNING: The config directory does not exist: {0}.\n\nCheck entry in the application file: app.config.", this.diConfigDir.FullName);
                //    throw new Exception();
                //}
                ////check the source directory
                //if (this.diSourceDir == null)
                //{
                //    MessageBox.Show("WARNING: A valid source directory has not been set.\n\nCheck entry in the application file: app.config ");
                //    throw new Exception();
                //}
                //else
                //if (!this.diSourceDir.Exists)
                //{
                //    MessageBox.Show("WARNING: The source directory does not exist: {0}.\n\nCheck entry in the application file: app.config.", this.diSourceDir.FullName);
                //    throw new Exception();
                //}
                ////check the output directory
                //if (this.diOutputDir == null)
                //{
                //    MessageBox.Show("WARNING: A valid output directory has not been set.\n\nCheck entry in the application file: app.config ");
                //    throw new Exception();
                //}
                //else
                //if (!this.diOutputDir.Exists)
                //{
                //    MessageBox.Show("WARNING: The output directory does not exist: {0}.\n\nCheck entry in the application file: app.config.", this.diOutputDir.FullName);
                //    throw new Exception();
                //}


                //var fiEXE = new FileInfo(@"audio-utils\ffmpeg\ffmpeg.exe");
                //if (! fiEXE.Exists)
                //{
                //    MessageBox.Show("WARNING: The file <audio-utils\\ffmpeg\\ffmpeg.exe> does not exist: {0}.\n\nCheck entry in the application file: app.config.");
                //    throw new Exception();
                //}
                //fiEXE = new FileInfo(@"audio-utils\ffmpeg\ffprobe.exe");
                //if (!fiEXE.Exists)
                //{
                //    MessageBox.Show("WARNING: The file <audio-utils\\ffprobe\\ffprobe.exe> does not exist: {0}.\n\nCheck entry in the application file: app.config.");
                //    throw new Exception();
                //}



                //fiEXE = new FileInfo(@"audio-utils\wavpack\wvunpack.exe");
                //if (!fiEXE.Exists)
                //{
                //    MessageBox.Show("WARNING: The file <audio-utils\\wavpack\\wvunpack.exe> does not exist: {0}.\n\nCheck entry in the application file: app.config.");
                //    throw new Exception();
                //}
                //fiEXE = new FileInfo(@"audio-utils\mp3splt\mp3splt.exe");
                //if (!fiEXE.Exists)
                //{
                //    MessageBox.Show("WARNING: The file <audio-utils\\mp3splt\\mp3splt.exe> does not exist: {0}.\n\nCheck entry in the application file: app.config.");
                //    throw new Exception();
                //}
                //fiEXE = new FileInfo(@"audio-utils\sox\sox.exe");
                //if (!fiEXE.Exists)
                //{
                //    MessageBox.Show("WARNING: The file <audio-utils\\sox\\sox.exe> does not exist: {0}.");
                //    throw new Exception("Check entry in the application file: app.config.");
                //}
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //        if (Debugger.IsAttached)
        //        {
        //            Debugger.Break();
        //        }

        //    } //catch
        //}


        public void WriteSettings2Console()
        {
            Console.WriteLine();
            Console.WriteLine("# Browser Settings:");
            Console.WriteLine("\tAnalysis Name: " + this.AnalysisIdentifier);
            if (this.fiAnalysisConfig == null)
                Console.WriteLine("\tAnalysis Config File: NULL");
            else Console.WriteLine("\tAnalysis Config File: " + this.fiAnalysisConfig.FullName);
            Console.WriteLine("\tSource Directory:     " + this.diSourceDir.FullName);
            Console.WriteLine("\tOutput Directory:     " + this.diOutputDir.FullName);
            Console.WriteLine("\tDisplay:  Track Height={0}pixels. Tracks normalised={1}.", this.TrackHeight, this.TrackNormalisedDisplay);
            Console.WriteLine("####################################################################################\n");
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
        public string AnalysisIdentifier { get; set; }
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
