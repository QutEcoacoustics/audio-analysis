﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using Acoustics.Shared;
using AnalysisBase;
using AnalysisPrograms;
using AudioAnalysisTools;
using log4net;

using AudioAnalysisTools.WavTools;
using TowseyLibrary;

namespace AudioBrowser
{
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;

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

        public IAnalyser2 Analyser { get; private set; }

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

            if (this.CsvFile != null)
            {
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
            } else if(this.IndicesImageFile != null){
                Log.InfoFormat("Loading an image: {0}", this.IndicesImageFile);
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

        public void UpdateSonogram(bool noiseReduced, double defaultBackgroundNoiseThreshold, bool annotated, TimeSpan segmentBuffer)
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

            var st = new Stopwatch();

            var adjustedStart = offsetStart;
            if (adjustedStart < TimeSpan.Zero)
            {
                adjustedStart = TimeSpan.Zero;
            }

            var adjustedEnd = offsetEnd;
            if (adjustedEnd > audioDuration)
            {
                adjustedEnd = audioDuration;
            }

            // delete sonogram if it already exists


            if (File.Exists(audioSegmentFile.FullName))
            {
                File.Delete(audioSegmentFile.FullName);
            }

            Log.DebugFormat("Extracting audio segment at offset {0} to {1} to new file {2}.", adjustedStart, adjustedEnd, audioSegmentFile);

            st.Start();

            AudioRecording.ExtractSegment(
                audioFile,
                adjustedStart,
                adjustedEnd,
                segmentBuffer,
                int.Parse(analysisParams[AnalysisKeys.ResampleRate]),
                audioSegmentFile);

            st.Stop();

            Log.DebugFormat("Time taken to extract audio segment: {0}", st.Elapsed);


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

            Log.DebugFormat("Generating sonogram from audio segment {0} to image {1}.", audioSegmentFile.Name, this.SonogramImageFile);

            //Read the config file for the current analysis type
            var config = ConfigDictionary.ReadPropertiesFile(configFile.FullName);

            //Set following key/value pairs from radio buttons.
            SetConfigValue(config, AudioAnalysisTools.AnalysisKeys.AnnotateSonogram, annotated.ToString().ToLowerInvariant());
            SetConfigValue(config, AudioAnalysisTools.AnalysisKeys.NoiseDoReduction, noiseReduced.ToString().ToLowerInvariant());

            //If any of following config key/value pairs are missing, then add in the defaults.
            if (!config.ContainsKey(AudioAnalysisTools.AnalysisKeys.FrameLength))
            {
                var defaultFrameLength = 1024; // do not want long spectrogram
                SetConfigValue(config, AudioAnalysisTools.AnalysisKeys.FrameLength, defaultFrameLength.ToString().ToLowerInvariant());
            }

            if (!config.ContainsKey(AudioAnalysisTools.AnalysisKeys.NoiseBgThreshold))
            {
                SetConfigValue(config, AudioAnalysisTools.AnalysisKeys.NoiseBgThreshold, defaultBackgroundNoiseThreshold.ToString().ToLowerInvariant());
            }

            config[AudioAnalysisTools.AnalysisKeys.AnalysisName] = analysisId;
            var fiTempConfig = TempFileHelper.NewTempFile(outputDir, configFileExt);
            ConfigDictionary.WriteConfgurationFile(config, fiTempConfig);

            st.Reset();
            st.Start();
            Image image = SpectrogramTools.GetImageFromAudioSegment(audioSegmentFile, fiTempConfig, fiImage, analyser);
            st.Stop();

            Log.DebugFormat("Time taken to generate sonogram: {0}", st.Elapsed);

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
            // HACK: using only one varient of process csv file, this is probbably broken
            var output = TabBrowseAudio.ProcessCsvFile(csvFile, configFile);
            DataTable dtRaw = output.Item1;
            DataTable dt2Display = output.Item2;

            string[] originalHeaders = DataTableTools.GetColumnNames(dtRaw);
            string[] displayHeaders = DataTableTools.GetColumnNames(dt2Display);

            var formattedList = GetFormattedHeaders(originalHeaders, displayHeaders);

            if (!File.Exists(this.IndicesImageFile.FullName))
            {
                Bitmap tracksImage = TabBrowseAudio.ConstructVisualIndexImage(dt2Display, imageTitle);
                tracksImage.Save(this.IndicesImageFile.FullName);
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
            int minInHour = 60;
            int currentCursorX = (int)offsetStart.TotalMinutes;
            string timeStr = String.Format("{0:d2}{1:d2}h", (currentCursorX / minInHour), (currentCursorX % minInHour));
                                           
            var segmentFileName = Path.GetFileNameWithoutExtension(audioFile.FullName) + "_" + timeStr + ".wav";

            var segmentFilePath = Path.Combine(outputDir.FullName, segmentFileName);

            return new FileInfo(segmentFilePath);
        }


        //########################################################################################################
        //#### The below three methods were transfered from the class IndexDisplay.cs on 02-12-2014.
        //#### They use the Datatable class for handling the index data. 
        //#### This was discontinued when Anthony refactored the code mid2014. however it still persists in the Audio Browser. 
        //############################################################################################
        //########################################################################################################

        public static Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
            if ((dt == null) || (dt.Rows.Count == 0)) return null;
            //get its column headers
            var dtHeaders = new List<string>();
            var dtTypes = new List<Type>();
            foreach (DataColumn col in dt.Columns)
            {
                dtHeaders.Add(col.ColumnName);
                dtTypes.Add(col.DataType);
            }

            List<string> displayHeaders = null;
            //check if config file contains list of display headers
            if ((fiConfigFile != null) && (fiConfigFile.Exists))
            {
                var configuration = new ConfigDictionary(fiConfigFile.FullName);
                Dictionary<string, string> configDict = configuration.GetTable();
                if (configDict.ContainsKey(AnalysisKeys.DisplayColumns))
                {
                    displayHeaders = configDict[AnalysisKeys.DisplayColumns].Split(',').ToList();
                    for (int i = 0; i < displayHeaders.Count; i++) // trim the headers just in case
                    {
                        displayHeaders[i] = displayHeaders[i].Trim();
                    }
                }
            }
            //if config file does not exist or does not contain display headers then use the original headers
            if (displayHeaders == null) displayHeaders = dtHeaders; //use existing headers if user supplies none.

            //now determine how to display tracks in display datatable
            Type[] displayTypes = new Type[displayHeaders.Count];
            bool[] canDisplay = new bool[displayHeaders.Count];
            for (int i = 0; i < displayTypes.Length; i++)
            {
                displayTypes[i] = typeof(double);
                string columnName = displayHeaders[i];
                if (dtHeaders.Contains(displayHeaders[i])) canDisplay[i] = true;
                if (dtTypes[i] == typeof(string)) canDisplay[i] = false;
            }

            DataTable table2Display = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes);
            foreach (DataRow oldRow in dt.Rows)
            {
                DataRow newRow = table2Display.NewRow();
                for (int i = 0; i < canDisplay.Length; i++)
                {
                    string header = displayHeaders[i];
                    if (canDisplay[i])
                    {
                        newRow[header] = oldRow[header];
                    }
                    else newRow[header] = 0.0;
                }
                table2Display.Rows.Add(newRow);
            }

            //order the table if possible
            if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventStartAbs))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventStartAbs + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventCount))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventCount + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyRankOrder))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyRankOrder + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyStartMinute))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyStartMinute + " ASC");
            }

            //table2Display = NormaliseColumnsOfDataTable(table2Display);
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalisation"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static Bitmap ConstructVisualIndexImage(DataTable dt, string title)
        {
            //construct an order array - this assumes that the table is already properly ordered.
            int length = dt.Rows.Count;
            double[] order = new double[length];
            for (int i = 0; i < length; i++) order[i] = i;
            List<string> headers = (from DataColumn col in dt.Columns select col.ColumnName).ToList();
            List<double[]> values = DataTableTools.ListOfColumnValues(dt);

            Bitmap tracksImage = TabBrowseAudio.ConstructImageOfIndexTracks(headers, values, title, order);
            return tracksImage;
        }



        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <param name="timeScale"></param>
        /// <param name="order"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalise"></param>
        /// <returns></returns>
        public static Bitmap ConstructImageOfIndexTracks(List<string> headers, List<double[]> values, string title, double[] order)
        {
            int trackHeight = DrawSummaryIndices.DefaultTrackHeight;


            // accumulate the individual tracks
            int duration = values[0].Length;    // time in minutes - 1 value = 1 pixel
            int imageWidth = duration + DrawSummaryIndices.TrackEndPanelWidth;

            var listOfBitmaps = new List<Bitmap>();
            double threshold = 0.0;
            double[] array;
            for (int i = 0; i < values.Count - 1; i++) // for each column of values in data table (except last) create a display track
            {
                if (values[i].Length == 0) continue;
                array = values[i];
                listOfBitmaps.Add(Image_Track.DrawBarScoreTrack(order, array, imageWidth, threshold, headers[i]));
            }

            // last track is weighted index
            //int x = values.Count - 1;
            //array = values[x];
            //bool doNormalise = false;
            //if (doNormalise) array = DataTools.normalise(values[x]);
            ////if (values[x].Length > 0)
            ////    bitmaps.Add(Image_Track.DrawColourScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[x])); //assumed to be weighted index
            //if (values[x].Length > 0)
            //    listOfBitmaps.Add(Image_Track.DrawBarScoreTrack(order, array, imageWidth, threshold, headers[x])); //assumed to be weighted index

            //set up the composite image parameters
            int imageHt = trackHeight * (listOfBitmaps.Count + 3);  //+3 for title and top and bottom time tracks
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);

            TimeSpan timeDuration = TimeSpan.FromMinutes(duration);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(timeDuration, TimeSpan.Zero, DrawSummaryIndices.TimeScale, imageWidth, trackHeight, "Time (hours)");

            //draw the composite bitmap
            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            using (Graphics gr = Graphics.FromImage(compositeBmp))
            {
                gr.Clear(Color.Black);

                int offset = 0;
                gr.DrawImage(titleBmp, 0, offset); //draw in the top title
                offset += trackHeight;
                gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
                offset += trackHeight;
                for (int i = 0; i < listOfBitmaps.Count; i++)
                {
                    gr.DrawImage(listOfBitmaps[i], 0, offset);
                    offset += trackHeight;
                }
                gr.DrawImage(timeBmp, 0, offset); //draw in bottom time scale
            }
            return compositeBmp;
        }


    }
}
