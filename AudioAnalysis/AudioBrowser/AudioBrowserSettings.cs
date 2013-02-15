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


            LoggedConsole.WriteLine("\n####################################################################################\nLOADING BROWSER SETTINGS:");


            try
            {
                this.DefaultConfigDir = AppConfigHelper.GetDir("DefaultConfigDir", true);
                this.diConfigDir = this.DefaultConfigDir;
            }
            catch (DirectoryNotFoundException ex)
            {
                LoggedConsole.WriteLine("WARNING!  The default directory containing analysis config files was not found. \n" + ex.ToString());
                LoggedConsole.WriteLine("          You will not be able to analyse audio files.");
                LoggedConsole.WriteLine("          Enter correct directory location of the config files in the app.config file.");

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            } //catch

            try
            {
                // specify false rather than true so GetDir doesn't throw exceptions
                this.DefaultSourceDir = AppConfigHelper.GetDir("DefaultSourceDir", true);
                this.diSourceDir = this.DefaultSourceDir;
            }
            catch (DirectoryNotFoundException ex)
            {
                string lastResort = @"C:\temp";
                this.diSourceDir = new DirectoryInfo(lastResort);
                if (!diSourceDir.Exists) diSourceDir.Create();

                LoggedConsole.WriteLine("WARNING!  The default source directory was not found. <" + this.DefaultSourceDir + ">");
                LoggedConsole.WriteLine("          Created new directory <" + this.diSourceDir.FullName + "> \n\n");
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
                this.diOutputDir = new DirectoryInfo(lastResort);
                if (!diOutputDir.Exists) diOutputDir.Create();

                LoggedConsole.WriteLine("WARNING!  The default output directory was not found. <" + this.DefaultOutputDir + ">");
                LoggedConsole.WriteLine("          Created new directory <" + this.diOutputDir.FullName + "> \n\n");

            } //catch

            // check for remainder of app.config arguments
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


            //CHECK THESE AUDIO ANALYSIS FILES EXIST
            //<add key="AudioUtilityFfmpegExe" value="audio-utils\ffmpeg\ffmpeg.exe" />
            //<add key="AudioUtilityFfprobeExe" value="audio-utils\ffmpeg\ffprobe.exe" />
            //<add key="AudioUtilityWvunpackExe" value="audio-utils\wavpack\wvunpack.exe" />
            //<add key="AudioUtilityMp3SpltExe" value="audio-utils\mp3splt\mp3splt.exe" />
            //<add key="AudioUtilitySoxExe" value="audio-utils\sox\sox.exe" />
            if (! AudioAnalysisFilesExist())
            {
                // MessageBox.Show("WARNING: " + ex.ToString());
                // MessageBox.Show("  CHECK paths in app.config file for following executable files: Ffmpeg.exe, Ffprobe.exe, Wvunpack.exe, Mp3Splt.exe, Sox.exe");
                LoggedConsole.WriteLine("WARNING!  Could not find one or more of the following audio analysis files:");
                LoggedConsole.WriteLine("          Ffmpeg.exe, Ffprobe.exe, Wvunpack.exe, Mp3Splt.exe, Sox.exe");
                LoggedConsole.WriteLine("          You will not be able to work with the original source file.");

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            if (! AudacityExists())
            {
                //MessageBox.Show("WARNING: Unable to find Audacity. Enter correct location in the app.config file.");
                LoggedConsole.WriteLine("WARNING!  Unable to find Audacity at default locations.");
                LoggedConsole.WriteLine("          Audacity.exe is required to view spectrograms of source recording.");
                LoggedConsole.WriteLine("          Enter correct location in the app.config file.");
            }

        } // LoadBrowserSettings()


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
            if (AudacityExists())
                LoggedConsole.WriteLine("\tAudacity Path   :     " + this.AudacityExe.FullName);
            else
                LoggedConsole.WriteLine("\tAudacity Path   :     NOT FOUND!");
            LoggedConsole.WriteLine("\tDisplay:  Track Height={0}pixels. Tracks normalised={1}.", this.TrackHeight, this.TrackNormalisedDisplay);
            LoggedConsole.WriteLine("####################################################################################\n");
        } // WriteSettings2Console()


        public bool AudacityExists()
        {
            try // locate AUDACITY
            {
                FileInfo audacity = AppConfigHelper.GetFile("AudacityExe", false);
                string possiblePath = @"audio-utils\Audacity\audacity.exe";
                string anotherPath  = @"C:\Program Files (x86)\Audacity 1.3 Beta (Unicode)\audacity.exe";
                if (!audacity.Exists) audacity = new FileInfo(possiblePath);
                if (!audacity.Exists) audacity = new FileInfo(anotherPath);
                if (!audacity.Exists)
                {
                    audacity = null;
                    throw new FileNotFoundException();
                }
                this.AudacityExe = audacity;
                return true;
            }
            catch (FileNotFoundException ex)
            {
                //MessageBox.Show("WARNING: Unable to find Audacity. Enter correct location in the app.config file.");
                //MessageBox.Show(ex.ToString());
                return false;
            } //catch
        }

        
        public bool AudioAnalysisFilesExist()
        {
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
                return false;
            } //catch
            return true;
        } // AudioAnalysisFilesExist()


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
