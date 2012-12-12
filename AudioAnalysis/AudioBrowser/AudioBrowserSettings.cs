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

        /// <summary>
        /// loads settings from the executable's config file.
        /// If directories given in config file do not exist then create a temp directory in C drive.
        /// </summary>
        public void LoadBrowserSettings()
        {
            try
            {
                this.DefaultConfigDir = AppConfigHelper.GetDir("DefaultConfigDir", true);
                this.diConfigDir = this.DefaultConfigDir;
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show("WARNING: " + ex.ToString());
                MessageBox.Show("  Cannot find the app.config file.");
                MessageBox.Show("  Cannot proceed!");

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            } //catch

            try
            {
                this.DefaultSourceDir = AppConfigHelper.GetDir("DefaultSourceDir", true);
                this.diSourceDir = this.DefaultSourceDir;
            }
            catch (DirectoryNotFoundException ex)
            {
                string lastResort = @"C:\temp";
                MessageBox.Show("WARNING: " + ex.ToString());
                MessageBox.Show("WARNING:  The default source directory in the app.config file does not exist.\n  Creating new directory <" + lastResort + "> \n\n\n\n" + ex.ToString());
                this.diSourceDir = new DirectoryInfo(lastResort);
                if (!diSourceDir.Exists) diSourceDir.Create();
            } //catch

            try
            {
                this.DefaultOutputDir = AppConfigHelper.GetDir("DefaultOutputDir", false);
                if (!this.DefaultOutputDir.Exists) 
                {
                    this.DefaultOutputDir = this.DefaultOutputDir.Parent;
                    if (!this.DefaultOutputDir.Exists)
                    {
                        this.DefaultOutputDir = this.DefaultOutputDir.Parent;
                        if (!this.DefaultOutputDir.Exists) throw new DirectoryNotFoundException();
                    }
                }
                this.diOutputDir = this.DefaultOutputDir;
            }
            catch (DirectoryNotFoundException ex)
            {
                string lastResort = @"C:\temp";
                MessageBox.Show("WARNING:  The default output directory and parents do not exist.\n  Creating new directory <" + lastResort + "> \n\n\n\n" + ex.ToString());
                this.diOutputDir = new DirectoryInfo(lastResort);
                if (!diOutputDir.Exists) diOutputDir.Create();
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
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("WARNING: " + ex.ToString());
                MessageBox.Show("  CHECK paths in app.config file.");

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            } //catch

            try // locate AUDACITY
            {
                FileInfo audacity = AppConfigHelper.GetFile("AudacityExe", false);
                string anotherPath = @"C:\Program Files (x86)\Audacity 1.3 Beta (Unicode)\audacity.exe";
                if (!audacity.Exists) audacity = new FileInfo(anotherPath);
                if (!audacity.Exists) 
                {
                    audacity = null;
                    throw new FileNotFoundException();
                }
                this.AudacityExe = audacity;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("WARNING: Unable to find Audacity. Check location in app.config file.");
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
            LoggedConsole.WriteLine();
            LoggedConsole.WriteLine("# Browser Settings:");
            LoggedConsole.WriteLine("\tAnalysis Name: " + this.AnalysisIdentifier);
            if (this.fiAnalysisConfig == null)
            {
                LoggedConsole.WriteLine("\tAnalysis Config File: NULL");
            }
            else
            {
                LoggedConsole.WriteLine("\tAnalysis Config File: " + this.fiAnalysisConfig.FullName);
            }

            LoggedConsole.WriteLine("\tSource Directory:     " + this.diSourceDir.FullName);
            LoggedConsole.WriteLine("\tOutput Directory:     " + this.diOutputDir.FullName);
            LoggedConsole.WriteLine("\tDisplay:  Track Height={0}pixels. Tracks normalised={1}.", this.TrackHeight, this.TrackNormalisedDisplay);
            LoggedConsole.WriteLine("####################################################################################\n");
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
