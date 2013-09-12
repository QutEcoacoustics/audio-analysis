using Acoustics.Shared;
using AnalysisBase;
using AnalysisPrograms;
using AudioAnalysisTools;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioBrowser
{
    public class TabBrowseAudio
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TabBrowseAudio));

        private Helper helper;

        // all properties are public get, private set
        // this is done to try and control when they can change
        // if they were public set, then any code could change them at any time. That is a headache. Don't do that.

        // given in SetNewFiles

        public FileInfo AudioFile { get; private set; }

        public FileInfo IndicesImageFile { get; private set; }

        public FileInfo CsvFile { get; private set; }

        // default is specified
        public DirectoryInfo OutputDirectory { get; private set; }

        // default is specified (so combo box selects default analysis type)
        public string AnalysisId { get; private set; }

        public FileInfo ConfigFile { get; private set; }

        // set with defaults

        public DirectoryInfo ConfigDir { get; private set; }

        public string ConfigFileExt { get; private set; }

        public string AudioFileExt { get; private set; }

        public string ResultImageFileExt { get; private set; }

        public string ResultTextFileExt { get; private set; }

        // calcuated when given new files in SetNewFiles

        public Image IndicesImage { get; private set; }

        public Image SonogramImage { get; private set; }

        public FileInfo SonogramImageFile { get; private set; }

        public IAnalyser Analyser { get; private set; }

        public Dictionary<string, string> AnalysisParams { get; private set; }

        public TimeSpan AudioDuration { get; private set; }

        public double[] TrackValues { get; private set; }

        public string CsvHeaderInfo { get; private set; }

        public bool DoNormalisation { get; private set; }

        public List<string> CsvHeaderList { get; private set; }

        public FileInfo AudioSegmentFile { get; private set; }

        public TimeSpan OffsetStart { get; private set; }

        public TimeSpan OffsetEnd { get; private set; }

        public Point ClickLocation { get; private set; }

        public TabBrowseAudio(Helper helper, string defaultAnalysisId, 
            DirectoryInfo defaultOutputDir, DirectoryInfo defaultconfigDir,
            string defaultConfigFileExt, string defaultAudioFileExt, string defaultResultImageFileExt, string defaultResultTextFileExt)
        {
            this.helper = helper;

            this.AnalysisId = defaultAnalysisId;
            this.OutputDirectory = defaultOutputDir;
            this.ConfigDir = defaultconfigDir;
            this.ConfigFileExt = defaultConfigFileExt;
            this.AudioFileExt = defaultAudioFileExt;
            this.ResultImageFileExt = defaultResultImageFileExt;
            this.ResultTextFileExt = defaultResultTextFileExt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvFile">Indices CSV file. Must exist if given. Specify either indices csv file or indices image file, not both.</param>
        /// <param name="indicesImageFile">Indices image file. Must exist if given. Specify either indices csv file or indices image file, not both.</param>
        /// <param name="audioFile">Source audio file. Can be null.</param>
        /// <param name="analysisId">Analysis identifier. Must not be null or empty.</param>
        /// <param name="configFile">Configuration file for analyser. Must exist.</param>
        /// <param name="outputDir">Output directory. Must exist.</param>
        public void SetNewFiles(FileInfo csvFile, FileInfo indicesImageFile, FileInfo audioFile, string analysisId, FileInfo configFile, DirectoryInfo outputDir, bool doNormalisation)
        {
            this.CsvFile = csvFile;
            this.IndicesImageFile = indicesImageFile;
            this.AudioFile = audioFile;
            this.AnalysisId = analysisId;
            this.ConfigFile = configFile;
            this.OutputDirectory = outputDir;

            UpdateCommon();
        }

        private void UpdateCommon()
        {
            // update analyser
            this.Analyser = this.helper.GetAnalyser(this.AnalysisId);
            if (this.Analyser == null)
            {
                Log.WarnFormat("Did not find an analyser matching id: {0}.", this.AnalysisId);
            }
            else
            {
                Log.InfoFormat("Loaded analyser matching id: {0}.", this.AnalysisId);
            }

            // compare config file to analyser expectations
            string fileName = Path.GetFileNameWithoutExtension(this.CsvFile.FullName);
            string[] fileNameSplit = fileName.Split('_');
            string[] array = fileNameSplit[fileNameSplit.Length - 1].Split('.');

            if (array.Length >= 3)
            {
                if (array[2].ToLowerInvariant() == "indices")
                {
                    Log.InfoFormat("Recognised as an indicies csv file: {0}", this.CsvFile);
                }
            }
            else if (array.Length >= 2)
            {
                var analysisName = array[0] + "." + array[1];
                Log.InfoFormat("Parsed analysis type {0} from file name {1}.", analysisName, this.CsvFile);
            }
            else if (fileNameSplit.Any(n => n.ToLowerInvariant().Contains(this.AnalysisId.ToLowerInvariant())))
            {
                Log.InfoFormat("Recognised analysis type {0} in csv file name: {1}", this.AnalysisId, this.CsvFile);
            }
            else
            {
                Log.WarnFormat("Could not parse analysis type from file name: {0}", this.CsvFile);
            }

            // update analysis parameters from config file
            if (this.ConfigFile != null && File.Exists(this.ConfigFile.FullName))
            {
                Log.InfoFormat("Loading config file from {0}.", this.ConfigFile);
                this.AnalysisParams = ConfigDictionary.ReadPropertiesFile(this.ConfigFile.FullName);
            }
            else
            {
                Log.WarnFormat("Could not load config file from {0}.", this.ConfigFile);
                this.AnalysisParams = null;
            }
        }

        private void ClearIndicesSettings()
        {
            this.CsvHeaderInfo = string.Empty;
            if (this.CsvHeaderList != null)
            {
                this.CsvHeaderList.Clear();
            }
            this.TrackValues = null;
            this.AudioDuration = TimeSpan.Zero;

            if (this.IndicesImage != null)
            {
                this.IndicesImage.Dispose();
            }
        }

        public void UpdateOffsets(TimeSpan offsetStart, TimeSpan offsetEnd, Point clickLocation)
        {
            this.OffsetStart = offsetStart;
            this.OffsetEnd = offsetEnd;
            this.ClickLocation = clickLocation;
        }

        public void UpdateSonogram(bool noiseReduced, double backgroundNoiseThreshold, bool annotated, TimeSpan segmentBuffer)
        {
            var audioFile = this.AudioFile;
            var outputDir = this.OutputDirectory;
            var analysisId = this.AnalysisId;
            var configFile = this.ConfigFile;
            var analyser = this.Analyser;
            var analysisParams = this.AnalysisParams;

            var offsetStart = this.OffsetStart;
            var offsetEnd = this.OffsetEnd;

            var audioDuration = this.AudioDuration;

            var imageFileExt = this.ResultImageFileExt;
            var configFileExt = this.ConfigFileExt;

            var audioSegmentFile = this.AudioSegmentFile = GetAudioSegmentFile(audioFile, offsetStart, outputDir);

            var FrameLength = 1024; // do not want long spectrogram
            var st = new Stopwatch();

            var adjustedStart = offsetStart;
            if (adjustedStart < TimeSpan.Zero) { adjustedStart = TimeSpan.Zero; }

            var adjustedEnd = offsetEnd;
            if (adjustedEnd > audioDuration) { adjustedEnd = audioDuration; }

            // delete sonogram if it already exists


            if (File.Exists(audioSegmentFile.FullName))
            {
                File.Delete(audioSegmentFile.FullName);
            }

            Log.InfoFormat("Extracting audio segment from source audio: {0} to {1}.", adjustedStart, adjustedEnd);
            Log.InfoFormat("Writing audio segment to dir: {0}", outputDir);
            Log.InfoFormat("File Name: {0}", audioSegmentFile);

            st.Start();

            AudioRecording.ExtractSegment(
                audioFile,
                adjustedStart,
                adjustedEnd,
                segmentBuffer,
                analysisParams,
                audioSegmentFile);

            st.Stop();

            Log.InfoFormat("Time taken to extract segment: {0}", st.Elapsed);


            //check recording segment exists
            if (audioSegmentFile == null)
            {
                Log.Warn("No audio segment path set.");
                return;
            }

            if (!File.Exists(audioSegmentFile.FullName))
            {
                Log.WarnFormat("Specified audio segment does not exist: {0}.", audioSegmentFile);
                return;
            }

            if (analysisId.ToLowerInvariant() == "none")
            {
                Log.Warn("Cannot annotate analysis type 'none'.");
                return;
            }

            if (!File.Exists(configFile.FullName))
            {
                Log.WarnFormat("Config file does not exist: {0}", configFile);
                return;
            }

            // get sonogram image
            FileInfo fiImage = new FileInfo(Path.Combine(outputDir.FullName, Path.GetFileNameWithoutExtension(audioSegmentFile.FullName) + imageFileExt));
            this.SonogramImageFile = fiImage;

            Log.InfoFormat("Generating sonogram from audio segment: {0} to {1}.", adjustedStart, adjustedEnd);
            Log.InfoFormat("Writing sonogram to dir: {0}", outputDir);
            Log.InfoFormat("File Name: {0}", this.SonogramImageFile);

            var config = ConfigDictionary.ReadPropertiesFile(configFile.FullName);
            SetConfigValue(config, AudioAnalysisTools.Keys.ANNOTATE_SONOGRAM, annotated.ToString().ToLowerInvariant());
            SetConfigValue(config, AudioAnalysisTools.Keys.NOISE_DO_REDUCTION, noiseReduced.ToString().ToLowerInvariant());
            SetConfigValue(config, AudioAnalysisTools.Keys.NOISE_BG_REDUCTION, backgroundNoiseThreshold.ToString().ToLowerInvariant());
            SetConfigValue(config, AudioAnalysisTools.Keys.FRAME_LENGTH, FrameLength.ToString().ToLowerInvariant());

            config[AudioAnalysisTools.Keys.ANALYSIS_NAME] = analysisId;
            var fiTempConfig = TempFileHelper.NewTempFileWithExt(outputDir, configFileExt);
            ConfigDictionary.WriteConfgurationFile(config, fiTempConfig);

            st.Reset();
            st.Start();
            Image image = SonogramTools.GetImageFromAudioSegment(audioSegmentFile, fiTempConfig, fiImage, analyser);
            st.Stop();
            Log.InfoFormat("Time taken to generate sonogram: {0}", st.Elapsed);


            this.SonogramImage = image;
        }

        public string GetLocationString(int currentCursorX)
        {
            int minInHour = 60;

            //assumes scale= 1 pixel / minute
            string text = (currentCursorX / minInHour) + "hr:" + (currentCursorX % minInHour) + "min (" + currentCursorX + ")";
            return text;
        }

        public string GetValueString(int currentCursorX, double[] currentTrackValues)
        {
            var display = string.Empty;

            // display the values at the cursor location
            if ((currentTrackValues == null) || (currentTrackValues.Length < 2) || currentCursorX >= currentTrackValues.Length)
            {
                return display;
            }

            var left = currentCursorX > 0 ? (currentTrackValues[currentCursorX - 1]).ToString("f2") : "START";
            var middle = currentTrackValues[currentCursorX];
            var right = currentCursorX < (currentTrackValues.Length - 1) ? (currentTrackValues[currentCursorX + 1]).ToString("f2") : "END";

            display = string.Format("{0:f2} <<{1:f2}>> {2:f2}", left, middle, right);
            return display;
        }

        private void SetConfigValue(Dictionary<string, string> config, string key, string value)
        {
            if (!config.ContainsKey(key)) config.Add(key, value);
            else config[key] = value;
        }

        // ********************************************************************************************
        // Refresh indices image from image file
        // ******************************************************************************************** 

        public void UpdateIndicesFromImageFile()
        {
            this.ClearIndicesSettings();

            var indicesImage = Image.FromFile(this.IndicesImageFile.FullName);
            this.IndicesImage = indicesImage;

            this.AudioDuration = TimeSpan.FromMinutes(this.IndicesImage.Width);

            this.CsvHeaderInfo = "Indices used to create image";
            this.CsvHeaderList = new List<string>()
            {
                "Temporal Entropy", "Acoustic Complexity Index", "Average Power"
            };
        }

        private static void LoadImageFile(FileInfo imageFile)
        {
            /*
            //this.CurrentImageFile

            //loads an image file. Assume it is a false colour spectrogram derived from three acoustic indices
            //returns a status integer. 0= no error
            int error = 0;
            string analysisName = "Towsey.Indices"; // assume the analysis type if Towsey.Acoustic

            //var op = LoadAnalysisConfigFile(analysisName);
            //this.Helper.fiAnalysisConfig = op.Item1;
            //this.analysisParams = op.Item2;

            // label to show selected analysis type for viewing CSV file.
            this.labelDisplayInfo.Text = "Analysis type = " + analysisName + ". ";
            //display column headers in the list box of displayed tracks
            string labelText = "Indices used to create image";
            this.labelCSVHeaders.Text = labelText;
            this.listBoxDisplayedTracks.Items.Clear(); //remove previous entries in list box.
            this.listBoxDisplayedTracks.Items.Add("Temporal Entropy");
            this.listBoxDisplayedTracks.Items.Add("Acoustic Complexity Index");
            this.listBoxDisplayedTracks.Items.Add("Average Power");

            // read the image
            var fiImage = new FileInfo(imagePath);
            this.pictureBoxVisualIndices.Image = new Bitmap(fiImage.FullName);
            this.sourceRecording_MinutesDuration = this.pictureBoxVisualIndices.Image.Width; //CAUTION: assume one value per minute - sets global variable !!!!
            return error;

            //OPEN A FILE DIALOGUE TO FIND IMAGE FILE
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open Image Dialogue";
            fdlg.InitialDirectory = this.Helper.diOutputDir.FullName;
            fdlg.Filter = "PNG files (*.png)|*.png";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                var fiImageFile = new FileInfo(fdlg.FileName);
                this.Helper.fiCSVFile = fiImageFile; // store in settings so can be accessed later.

                this.Helper.diOutputDir = new DirectoryInfo(Path.GetDirectoryName(fiImageFile.FullName)); // change to selected directory
                this.pictureBoxSonogram.Image = null;  //reset in case old sonogram image is showing.
                this.labelSonogramFileName.Text = "Spectrogram file name";

                // clear console window
                this.textBoxConsole.Clear();
                LoggedConsole.WriteLine(Helper.BROWSER_TITLE_TEXT);
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(date);

                // ## LOAD A PNG IMAGE ##############################################################################################
                int status = this.LoadColourSpectrogramFile(fiImageFile.FullName);
                // ##################################################################################################################
                if (status >= 3)
                {
                    //this.tabControlMain.SelectTab("tabPageConsole");
                    //LoggedConsole.WriteLine("FATAL ERROR: Error opening csv file");
                    //LoggedConsole.WriteLine("\t\tfile name:" + fiCSVFile.FullName);
                    //if (status == 1) LoggedConsole.WriteLine("\t\tfile exists but could not extract values.");
                    //if (status == 2) LoggedConsole.WriteLine("\t\tfile exists but contains no values.");
                }
                else
                {
                    LoggedConsole.WriteLine("# Display of false colour spectrogram: " + fiImageFile.FullName);
                    this.selectionTrackImage = new Bitmap(this.pictureBoxBarTrack.Width, this.pictureBoxBarTrack.Height);
                    this.pictureBoxBarTrack.Image = this.selectionTrackImage;

                    //###################### MAKE VISUAL ADJUSTMENTS FOR HEIGHT OF THE VISUAL INDEX IMAGE  - THIS DEPENDS ON NUMBER OF TRACKS 
                    this.pictureBoxBarTrack.Location = new Point(0, this.pictureBoxVisualIndices.Height + 1);
                    //this.pictureBoxVisualIndex.Location = new Point(0, tracksImage.Height + 1);
                    this.panelDisplayImageAndTrackBar.Height = this.pictureBoxVisualIndices.Height + this.pictureBoxBarTrack.Height + 20; //20 = ht of scroll bar
                    this.panelDisplaySpectrogram.Location = new Point(3, panelDisplayImageAndTrackBar.Height + 1);
                    this.pictureBoxSonogram.Location = new Point(3, 0);
                    this.pictureBoxPen = new Pen(Color.Black, 1.0F); // used to draw vertical hairs


                    this.labelSourceFileName.Text = Path.GetFileNameWithoutExtension(fiImageFile.FullName);
                    if (status == 0)
                    {
                        this.labelSourceFileName.Text = Path.GetFileNameWithoutExtension(fiImageFile.FullName);
                        this.labelDisplayInfo.Text += "   Image scale = 1 minute/pixel.     File duration = " + this.sourceRecording_MinutesDuration + " minutes";
                    }
                    else
                    {
                        this.labelSourceFileName.Text = String.Format("WARNING: ERROR loading image file <{0}>.    READ CONSOLE MESSAGE!", Path.GetFileNameWithoutExtension(fiImageFile.FullName));
                        this.labelDisplayInfo.Text += "         READ CONSOLE MESSAGE!";
                    }
                    this.tabControlMain.SelectTab("tabPageDisplay");
                } // (status)
            } // if (DialogResult.OK)
            */
        }

        // ********************************************************************************************
        // Refresh indices image from csv file
        // ******************************************************************************************** 

        public void UpdateIndicesFromCsvFile()
        {
            this.ClearIndicesSettings();

            // uses an analyser, config file and csv results file to create an image of the indices

            var analyser = this.Analyser;// = GetAnalyser(this.an;
            var csvFile = this.CsvFile;
            var configFile = this.ConfigFile;
            var imageExt = this.helper.DefaultResultImageFileExt;
            var imageTitle = Helper.ImageTitle;
            var doNormalisation = this.DoNormalisation;

            this.IndicesImageFile = new FileInfo(Path.Combine(this.OutputDirectory.FullName, (Path.GetFileNameWithoutExtension(csvFile.FullName) + imageExt)));

            // process the CSV file
            var output = analyser.ProcessCsvFile(csvFile, configFile);
            DataTable dtRaw = output.Item1;
            DataTable dt2Display = output.Item2;

            string[] originalHeaders = DataTableTools.GetColumnNames(dtRaw);
            string[] displayHeaders = DataTableTools.GetColumnNames(dt2Display);

            var formattedList = GetFormattedHeaders(originalHeaders, displayHeaders);

            if (!File.Exists(this.IndicesImageFile.FullName))
            {
                Bitmap tracksImage = DisplayIndices.ConstructVisualIndexImage(dt2Display, imageTitle, doNormalisation, this.IndicesImageFile.FullName);
                this.IndicesImage = tracksImage;
            }
            else
            {
                this.IndicesImage = Image.FromFile(this.IndicesImageFile.FullName);
            }

            // assumes one value per minute
            this.AudioDuration = TimeSpan.FromMinutes(dt2Display.Rows.Count);

            //make values of bottom track available
            string header = displayHeaders[displayHeaders.Length - 1];

            this.TrackValues = DataTableTools.Column2ArrayOfDouble(dt2Display, header);
            this.CsvHeaderList = formattedList;
            this.CsvHeaderInfo = originalHeaders.Length + " headers (" + displayHeaders.Length + " displayed)";
        }

        private static List<string> GetFormattedHeaders(string[] originalHeaders, string[] displayHeaders)
        {
            //display column headers in the list box of displayed tracks
            List<string> displayList = displayHeaders.ToList();
            List<string> abbrevList = new List<string>();
            List<string> formattedList = new List<string>();

            foreach (string str in displayList)
            {
                string text = str;  // the headers have been tampered with!! but assume not first 5 chars ([Mark] I have no idea what this comment means.)
                if (text.Length > 6) text = str.Substring(0, 5);
                abbrevList.Add(text);
            }
            for (int i = 0; i < originalHeaders.Length; i++)
            {
                string text = originalHeaders[i];
                if (text.Length > 6) text = originalHeaders[i].Substring(0, 5);
                if (abbrevList.Contains(text))
                    formattedList.Add(string.Format("{0:d2}: {1}  (displayed)", (i + 1), originalHeaders[i]));
                else
                    formattedList.Add(string.Format("{0:d2}: {1}", (i + 1), originalHeaders[i]));
            }

            return formattedList;
        }

        // ********************************************************************************************
        // Refresh Sonogram Image
        // ******************************************************************************************** 

        private static FileInfo GetAudioSegmentFile(FileInfo audioFile, TimeSpan offsetStart, DirectoryInfo outputDir)
        {
            var segmentFileName = Path.GetFileNameWithoutExtension(audioFile.FullName) + "_min" +
                (int)offsetStart.TotalMinutes + ".wav";

            var segmentFilePath = Path.Combine(outputDir.FullName, segmentFileName);

            return new FileInfo(segmentFilePath);
        }
    }
}
