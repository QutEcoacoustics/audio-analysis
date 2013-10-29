using AnalysisBase;
using AnalysisPrograms;
using AudioAnalysisTools;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioBrowser
{
    using Acoustics.Shared.Extensions;

    public class AudioNavigator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AudioNavigator));

        public string DefaultAnalysisName
        {
            get
            {
                var a = new Acoustic();
                return a.Identifier;
            }
        }

        public FileInfo AudioFile { get; set; }

        public FileInfo CsvFile { get; set; }

        public DirectoryInfo OutputDirectory { get; set; }

        public string AnalysisName { get; set; }

        public IAnalyser Analyser { get; set; }

        public Dictionary<string, string> AnalysisParams { get; set; }

        public FileInfo ConfigFile { get; set; }

        public TimeSpan AudioDuration { get; set; }

        public double[] TrackValues { get; set; }

        public Image IndicesImage { get; set; }

        public Image SonogramImage { get; set; }

        public string CsvHeaderInfo { get; set; }

        public List<string> CsvHeaderList { get; set; }

        public FileInfo CurrentAudioSegmentFile { get; set; }

        public FileInfo CurrentImageFile { get; set; }

        public TimeSpan CurrentOffsetStart { get; set; }

        public TimeSpan CurrentOffsetEnd { get; set; }

        public DirectoryInfo configFileBaseDir { get; private set; }

        public PluginHelper pluginHelper { get; private set; }

        public AudioNavigator(DirectoryInfo configFileBaseDir, PluginHelper pluginHelper)
        {
            this.configFileBaseDir = configFileBaseDir;
            this.pluginHelper = pluginHelper;
        }

        public void RefreshIndices(bool doNormalisation)
        {
            this.Analyser = null;
            this.ConfigFile = null;
            this.AnalysisParams = null;
            this.AudioDuration = TimeSpan.Zero;
            this.TrackValues = null;
            this.IndicesImage = null;
            this.CsvHeaderInfo = null;

            // is the file an indicies csv file?
            var isIndicesFile = IsIndiciesCsvFile(this.CsvFile);

            SetAnalysis();

            ProcessCsvFile(doNormalisation);
        }

        public void RefreshSonogram(bool noiseReduced, double backgroundNoiseThreshold, bool annotated, TimeSpan segmentBuffer)
        {
            var FrameLength = 1024; // do not want long spectrogram
            var st = new Stopwatch();

            var segmentFileName = Path.GetFileNameWithoutExtension(this.AudioFile.FullName) + "_min" +
                (int)this.CurrentOffsetStart.TotalMinutes + ".wav";

            var segmentFilePath = Path.Combine(this.OutputDirectory.FullName, segmentFileName);

            this.CurrentAudioSegmentFile = new FileInfo(segmentFilePath);

            var adjustedStart = this.CurrentOffsetStart - segmentBuffer;
            if (adjustedStart < TimeSpan.Zero) { adjustedStart = TimeSpan.Zero; }

            var adjustedEnd = this.CurrentOffsetEnd + segmentBuffer;
            if (adjustedEnd > this.AudioDuration) { adjustedEnd = this.AudioDuration; }

            // Extract segment if necessary
            if (!File.Exists(this.CurrentAudioSegmentFile.FullName))
            {
                Log.InfoFormat("Extracting audio segment from source audio: {0} to {1}.", adjustedStart, adjustedEnd);
                Log.InfoFormat("Writing audio segment to dir: {0}", this.OutputDirectory);
                Log.InfoFormat("File Name: {0}", segmentFileName);

                
                st.Start();

                AudioRecording.ExtractSegment(
                    this.AudioFile,
                    adjustedStart,
                    adjustedEnd,
                    segmentBuffer,
                    this.AnalysisParams,
                    this.CurrentAudioSegmentFile);

                st.Stop();

                Log.InfoFormat("Time taken to extract segment: {0}", st.Elapsed);
            }

            //check recording segment exists
            if (this.CurrentAudioSegmentFile == null)
            {
                Log.Warn("No audio segment path set.");
                return;
            }

            if (!File.Exists(this.CurrentAudioSegmentFile.FullName))
            {
                Log.WarnFormat("Specified audio segment does not exist: {0}.", this.CurrentAudioSegmentFile);
                return;
            }

            if (this.AnalysisName.ToLowerInvariant() == "none")
            {
                Log.Warn("Cannot annotate analysis type 'none'.");
                return;
            }

            if (!File.Exists(this.ConfigFile.FullName))
            {
                Log.WarnFormat("Config file does not exists: {0}", this.ConfigFile);
                return;
            }

            // get sonogram image
            FileInfo fiImage = new FileInfo(Path.Combine(this.OutputDirectory.FullName, Path.GetFileNameWithoutExtension(this.CurrentAudioSegmentFile.FullName) + ".png"));
            this.CurrentImageFile = fiImage;

            Log.InfoFormat("Generating sonogram from audio segment: {0} to {1}.", adjustedStart, adjustedEnd);
            Log.InfoFormat("Writing sonogram to dir: {0}", this.OutputDirectory);
            Log.InfoFormat("File Name: {0}", this.CurrentImageFile);

            var config = ConfigDictionary.ReadPropertiesFile(this.ConfigFile.FullName);
            SetConfigValue(config, AudioAnalysisTools.Keys.ANNOTATE_SONOGRAM, annotated.ToString().ToLowerInvariant());
            SetConfigValue(config, AudioAnalysisTools.Keys.NOISE_DO_REDUCTION, noiseReduced.ToString().ToLowerInvariant());
            SetConfigValue(config, AudioAnalysisTools.Keys.NOISE_BG_REDUCTION, backgroundNoiseThreshold.ToString().ToLowerInvariant());
            SetConfigValue(config, AudioAnalysisTools.Keys.FRAME_LENGTH, FrameLength.ToString().ToLowerInvariant());

            config[AudioAnalysisTools.Keys.ANALYSIS_NAME] = this.AnalysisName;
            var fiTempConfig = new FileInfo(Path.Combine(this.OutputDirectory.FullName, "temp.cfg"));
            ConfigDictionary.WriteConfgurationFile(config, fiTempConfig);

            st.Reset();
            st.Start();
            IAnalyser analyser = AudioBrowserTools.GetAcousticAnalyser(this.AnalysisName, this.pluginHelper.AnalysisPlugins);
            Image image = SonogramTools.GetImageFromAudioSegment(this.CurrentAudioSegmentFile, fiTempConfig, fiImage, analyser);
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
            if ((currentTrackValues == null) || (currentTrackValues.Length < 2))
            {
                return display;
            }

            if (currentCursorX >= currentTrackValues.Length - 1)
            {
                display =
                    string.Format("{0:f2} <<{1:f2}>> {2:f2}",
                    currentTrackValues[currentCursorX - 1],
                    currentTrackValues[currentCursorX],
                    "END");
            }
            else if (currentCursorX <= 0)
            {
                display =
                    string.Format("{0:f2} <<{1:f2}>> {2:f2}",
                    "START",
                    currentTrackValues[currentCursorX],
                    currentTrackValues[currentCursorX + 1]);
            }
            else
            {
                display =
                    string.Format("{0:f2} <<{1:f2}>> {2:f2}",
                    currentTrackValues[currentCursorX - 1],
                    currentTrackValues[currentCursorX],
                    currentTrackValues[currentCursorX + 1]);
            }

            return display;
        }

        private bool IsIndiciesCsvFile(FileInfo csvFile)
        {
            // PARSE the file name
            string fileName = Path.GetFileNameWithoutExtension(csvFile.FullName);

            string[] array = fileName.Split('_');
            array = array[array.Length - 1].Split('.');

            bool isIndicesFile = false;
            if (array.Length >= 3)
            {
                isIndicesFile = (array[2].ToLowerInvariant() == "indices");
            }

            if (!isIndicesFile)
            {
                Log.WarnFormat("Could not parse file name. May not be an indicies csv file: {0}", csvFile);
            }
            else
            {
                Log.InfoFormat("Recognised as an indicies csv file: {0}", csvFile);
            }

            return isIndicesFile;
        }

        private string GetAnalysisTypeFromFileName(FileInfo csvFile)
        {
            // PARSE the file name
            string fileName = Path.GetFileNameWithoutExtension(csvFile.FullName);

            string[] array = fileName.Split('_');
            array = array[array.Length - 1].Split('.');
            if (array.Length >= 2)
            {
                var analysisName = array[0] + "." + array[1];
                Log.InfoFormat("Parsed analysis type {0} from file name {1}.", analysisName, csvFile);
                return analysisName;
            }
            else
            {
                Log.WarnFormat("Could not parse analysis type from file name: {0}", csvFile);
                return string.Empty;
            }
        }

        private FileInfo AnalysisConfigPath(DirectoryInfo configDir, string analysisName)
        {
            if (analysisName.ToLowerInvariant() == "none")
            {
                Log.Warn("There is no config file for the 'none' analysis type.");
                return null;
            }
            else if (configDir == null)
            {
                Log.Warn("Config directory must be specified.");
                return null;
            }
            else if (string.IsNullOrWhiteSpace(analysisName))
            {
                Log.Warn("An analysis type must be specified.");
                return null;
            }
            else
            {
                var configPath = Path.Combine(configDir.FullName, analysisName + AudioBrowserSettings.DefaultConfigExt);
                if (File.Exists(configPath))
                {
                    Log.InfoFormat("Found config file for {0} analysis: {1}.", analysisName, configPath);
                    return new FileInfo(configPath);
                }
                else
                {
                    Log.WarnFormat("Could not find config file for {0} analysis: {1}.", analysisName, configPath);
                    return null;
                }
            }
        }

        private Dictionary<string, string> LoadAnalysisConfigFile(FileInfo configFile)
        {
            if (configFile != null && File.Exists(configFile.FullName))
            {
                Log.InfoFormat("Loading config file from {0}.", configFile);
                return ConfigDictionary.ReadPropertiesFile(configFile.FullName);
            }
            else
            {
                Log.WarnFormat("Could not load config file from {0}.", configFile);
                return null;
            }
        }

        private IAnalyser GetAnalyser(string analysisName)
        {
            IAnalyser analyser = AudioBrowserTools.GetAcousticAnalyser(analysisName);
            if (analyser == null)
            {
                Log.WarnFormat("Did not find an analyser matching name: {0}.", analysisName);
            }
            else
            {
                Log.InfoFormat("Loaded analyser matching name: {0}.", analysisName);
            }

            return analyser;
        }

        private bool SetAnalysisByName(DirectoryInfo configFilesBaseDir, string analysisName)
        {
            if (string.IsNullOrWhiteSpace(analysisName) ||
                analysisName.ToLowerInvariant() == "none")
            {
                return false;
            }

            IAnalyser analyser = GetAnalyser(analysisName);
            if (analyser == null)
            {
                return false;
            }

            FileInfo configFile = AnalysisConfigPath(configFilesBaseDir, analysisName);
            if (configFile == null)
            {
                return false;
            }

            Dictionary<string, string> analysisParams = LoadAnalysisConfigFile(configFile);
            if (analysisParams == null)
            {
                return false;
            }

            this.Analyser = analyser;
            this.ConfigFile = configFile;
            this.AnalysisParams = analysisParams;

            return true;
        }

        private void SetAnalysis()
        {
            // find an analysis name that works.
            var analysisNameFromModel = this.AnalysisName;
            if (SetAnalysisByName(this.configFileBaseDir, analysisNameFromModel))
            {
                return;
            }

            var analysisNameFromFileName = GetAnalysisTypeFromFileName(this.CsvFile);
            if (SetAnalysisByName(this.configFileBaseDir, analysisNameFromFileName))
            {
                return;
            }

            var analysisNameDefault = this.DefaultAnalysisName;
            if (SetAnalysisByName(this.configFileBaseDir, analysisNameDefault))
            {
                return;
            }

            Log.Fatal("Could not find a valid analysis name.");
            this.Analyser = null;
            this.ConfigFile = null;
            this.AnalysisParams = null;
            return;
        }

        private void ProcessCsvFile(bool doNormalisation)
        {
            // finally process the CSV file
            var output = this.Analyser.ProcessCsvFile(this.CsvFile, this.ConfigFile);
            DataTable dtRaw = output.Item1;
            DataTable dt2Display = output.Item2;

            // assumes one value per minute
            this.AudioDuration = TimeSpan.FromMinutes(dt2Display.Rows.Count);

            string[] originalHeaders = DataTableTools.GetColumnNames(dtRaw);
            string[] displayHeaders = DataTableTools.GetColumnNames(dt2Display);

            //make values of bottom track available
            string header = displayHeaders[displayHeaders.Length - 1];

            this.TrackValues = DataTableTools.Column2ArrayOfDouble(dt2Display, header);

            //display column headers in the list box of displayed tracks
            List<string> displayList = displayHeaders.ToList();
            List<string> abbrevList = new List<string>();
            List<string> formattedList = new List<string>();

            foreach (string str in displayList)
            {
                string text = str;  // the headers have been tampered with!! but assume not first 5 chars
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

            this.CsvHeaderList = formattedList;

            string labelText = originalHeaders.Length + " headers (" + displayHeaders.Length + " displayed)";
            //this.labelCSVHeaders.Text = labelText;
            this.CsvHeaderInfo = labelText;

            string imagePath = Path.Combine(this.OutputDirectory.FullName, (Path.GetFileNameWithoutExtension(this.CsvFile.FullName) + ".png"));

            if (!File.Exists(imagePath))
            {
                Bitmap tracksImage = DisplayIndices.ConstructVisualIndexImage(dt2Display, AudioBrowserTools.IMAGE_TITLE_TEXT, doNormalisation, imagePath.ToFileInfo());
                //this.pictureBoxVisualIndices.Image = tracksImage;
                this.IndicesImage = tracksImage;
            }
            else
            {
                this.IndicesImage = Image.FromFile(imagePath);
            }
        }

        private void SetConfigValue(Dictionary<string, string> config, string key, string value)
        {
            if (!config.ContainsKey(key)) config.Add(key, value);
            else config[key] = value;
        }


    }
}
