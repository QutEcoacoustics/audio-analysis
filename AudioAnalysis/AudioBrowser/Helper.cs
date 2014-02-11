namespace AudioBrowser
{
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;
    using AnalysisBase;
    using AnalysisRunner;
    using AudioAnalysisTools;
    using AudioBase;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using TowseyLib;

    public class Helper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Helper));

        // translatable values
        public static string ProgramTitle { get { return "AUDIO-BROWSER: To explore long bio-acoustic recordings"; } }
        public static string Copyright { get { return "\u00a9 Queensland University of Technology (QUT)"; } }
        public static string ImageTitle { get { return "Image produced by AUDIO-BROWSER, (QUT)."; } }

        public static string SelectAudioFilter { get { return "Audio Files|*.mp3;*.wav;*.ogg;*.wma;*.webm;*.wv;*.aac|All files|*.*"; } }
        public static string SelectConfigFilter { get { return "Text Files|*.txt;*.cfg;*.ini|All files|*.*"; } }
        public static string SelectCsvFilter { get { return "Csv Files|*.csv;*.txt|All files|*.*"; } }
        public static string SelectImageFilter { get { return "Image Files|*.png;*.jpg;*.jpeg|All files|*.*"; } }

        // fields
        private PluginHelper pluginHelper = null;
        private IEnumerable<KeyValuePair<string, string>> analysersAvailable = null;

        // global properties initialised from app.config file
        public FileInfo AudioUtilityFfmpegExe { get; private set; }
        public FileInfo AudioUtilityFfprobeExe { get; private set; }
        public FileInfo AudioUtilityWvunpackExe { get; private set; }
        public FileInfo AudioUtilityMp3SpltExe { get; private set; }
        public FileInfo AudioUtilitySoxExe { get; private set; }
        public FileInfo AudioUtilityShntoolExe { get; private set; }

        public FileInfo AudacityExe { get; private set; }
        public FileInfo TextEditorExe { get; private set; }
        public FileInfo ConsoleExe { get; private set; }
        public FileInfo AnalysisProgramsExe { get { return new FileInfo(Path.Combine(this.GetExeDir.FullName, "AnalysisPrograms.exe")); } }

        public DirectoryInfo AnalysisWorkingDir { get; private set; }

        public DirectoryInfo DefaultConfigDir { get; private set; }
        public DirectoryInfo DefaultSourceDir { get; private set; }
        public DirectoryInfo DefaultOutputDir { get; private set; }

        public string DefaultAudioFileExt { get; private set; }
        public string DefaultConfigFileExt { get; private set; }

        public string DefaultResultTextFileExt { get; private set; }
        public string DefaultResultImageFileExt { get; private set; }

        public string DefaultAnalysisIdentifier { get; private set; }

        public double DefaultSegmentDuration { get; private set; }
        public int DefaultResampleRate { get; private set; }

        public int TrackHeight { get; private set; }
        public int TrackCount { get; private set; }
        public bool TrackNormalisedDisplay { get; private set; }
        public double SonogramBackgroundThreshold { get; private set; }

        // other global properties
        public IAnalyser DefaultAnalyser { get { return this.pluginHelper.GetAcousticAnalyser(this.DefaultAnalysisIdentifier); } }

        public IEnumerable<KeyValuePair<string, string>> AnalysersAvailable
        {
            get
            {
                if (this.analysersAvailable == null)
                {
                    this.analysersAvailable = this.pluginHelper.GetAnalysisPluginsList();
                }
                return this.analysersAvailable;
            }
        }

        public DirectoryInfo DefaultTempFilesDir { get { return new DirectoryInfo(Path.GetTempPath()); } }

        public DirectoryInfo GetExeDir
        {
            get
            {
                var codebase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new System.Uri(codebase);
                var localPath = uri.LocalPath;
                var directory = Path.GetDirectoryName(localPath);

                return new DirectoryInfo(directory);
            }
        }

        private AnalysisCoordinator analysisCoordinator;

        public Helper()
        {
            this.LoadSettings();

            this.pluginHelper = new PluginHelper();
            this.pluginHelper.FindIAnalysisPlugins();
        }

        public IAnalyser GetAnalyser(string analyserIdentifier)
        {
            return this.pluginHelper.GetAcousticAnalyser(analyserIdentifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioFile"></param>
        /// <param name="analyser"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public IEnumerable<AnalysisResult> ProcessRecording(FileInfo audioFile, FileInfo configFile, IAnalyser analyser, AnalysisSettings settings)
        {
            //var analyserResults = analysisCoordinator.Run(fileSegments, analyser, settings).OrderBy(a => a.SegmentStartOffset);
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(analyser != null, "Analyser must not be null.");
            //Contract.Requires(file != null, "Source file must not be null.");

            bool saveIntermediateWavFiles = false;
            if (settings.ConfigDict.ContainsKey(AudioAnalysisTools.Keys.SAVE_INTERMEDIATE_WAV_FILES))
                saveIntermediateWavFiles = ConfigDictionary.GetBoolean(AudioAnalysisTools.Keys.SAVE_INTERMEDIATE_WAV_FILES, settings.ConfigDict);

            bool doParallelProcessing = false;
            if (settings.ConfigDict.ContainsKey(AudioAnalysisTools.Keys.PARALLEL_PROCESSING))
                doParallelProcessing = ConfigDictionary.GetBoolean(AudioAnalysisTools.Keys.PARALLEL_PROCESSING, settings.ConfigDict);

            //initilise classes that will do the analysis
            this.analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = (!saveIntermediateWavFiles), // create and delete directories 
                IsParallel = doParallelProcessing,         // ########### PARALLEL OR SEQUENTIAL ??????????????
                SubFoldersUnique = false
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //################# PROCESS THE RECORDING #####################################################################################
            var results = analysisCoordinator.Run(audioFile, analyser, settings);

            if (results == null)
            {
                Log.FatalFormat("No information from analysis {0} for audio file {1} and config file {2}.", analyser.Identifier, audioFile, configFile);
                return null;
            }

            DataTable datatable = ResultsTools.MergeResultsIntoSingleDataTable(results);

            //get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
            var audioUtility = new MasterAudioUtility();
            var mimeType = MediaTypes.GetMediaType(audioFile.Extension);
            var sourceInfo = audioUtility.Info(audioFile);

            double scoreThreshold = double.Parse(settings.ConfigDict[AudioAnalysisTools.Keys.EVENT_THRESHOLD]);  //min score for an acceptable event
            scoreThreshold *= 3; // triple the threshold - used to filter high scoring events
            if (scoreThreshold > 1.0) scoreThreshold = 1.0;

            var op1 = ResultsTools.GetEventsAndIndicesDataTables(datatable, analyser, sourceInfo.Duration.Value, scoreThreshold);
            var eventsDatatable = op1.Item1;
            var indicesDatatable = op1.Item2;
            int eventsCount = 0;
            if (eventsDatatable != null) eventsCount = eventsDatatable.Rows.Count;
            int indicesCount = 0;
            if (indicesDatatable != null) indicesCount = indicesDatatable.Rows.Count;
            var opdir = results.ElementAt(0).SettingsUsed.AnalysisInstanceOutputDirectory;
            string fName = Path.GetFileNameWithoutExtension(audioFile.Name) + "_" + analyser.Identifier;
            var op2 = ResultsTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);

            //#############################################################################################################################
            stopwatch.Stop();
            var fiEventsCSV = op2.Item1;
            var fiIndicesCSV = op2.Item2;

            //Remaining LINES ARE FOR DIAGNOSTIC PURPOSES ONLY
            TimeSpan ts = stopwatch.Elapsed;
            Log.InfoFormat("Processing time: {0:f3} seconds ({1}min {2}s)", (stopwatch.ElapsedMilliseconds / (double)1000), ts.Minutes, ts.Seconds);

            int outputCount = eventsCount;
            if (eventsCount == 0) outputCount = indicesCount;
            Log.InfoFormat("Number of units of output: {0}", outputCount);

            if (outputCount == 0) outputCount = 1;
            Log.InfoFormat("Average time per unit of output: {0:f3} seconds.", (stopwatch.ElapsedMilliseconds / (double)1000 / (double)outputCount));
            Log.InfoFormat("Finished processing analysis {0} for audio file {1} and config file {2}.", analyser.Identifier, audioFile, configFile);

            //LoggedConsole.WriteLine("Output  to  directory: " + this.tfOutputDirectory.Text);
            if (fiEventsCSV != null)
            {
                Log.Info("EVENTS CSV file(s) = " + fiEventsCSV.Name);
                Log.Info("\tNumber of events = " + eventsCount);
            }
            if (fiIndicesCSV != null)
            {
                Log.Info("INDICES CSV file(s) = " + fiIndicesCSV.Name);
                Log.Info("\tNumber of indices = " + indicesCount);
            }

            return results;

        } //ProcessRecording()

        private static int GetThreadsInUse()
        {
            int availableWorkerThreads, availableCompletionPortThreads, maxWorkerThreads, maxCompletionPortThreads;
            ThreadPool.GetAvailableThreads(out  availableWorkerThreads, out availableCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            int threadsInUse = maxWorkerThreads - availableWorkerThreads;

            return threadsInUse;
        }

        public static FileInfo PromptUserToSelectFile(string title, string filter, string initialDirectory)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = title;
            fdlg.Filter = filter;
            fdlg.RestoreDirectory = false;

            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            {
                fdlg.InitialDirectory = initialDirectory;
            }

            fdlg.Multiselect = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                return new FileInfo(fdlg.FileName);
            }
            else
            {
                return null;
            }
        }

        public static DirectoryInfo PromptUserToSelectDirectory(string descr, string initialDirectory)
        {
            FolderBrowserDialog fdlg = new FolderBrowserDialog();

            // Set the help text description for the FolderBrowserDialog. 
            fdlg.Description = descr;

            // Do not allow the user to create new files via the FolderBrowserDialog. 
            fdlg.ShowNewFolderButton = false;

            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            {
                fdlg.SelectedPath = initialDirectory;
            }

            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                return new DirectoryInfo(fdlg.SelectedPath);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="diSourceDir"></param>
        /// <returns></returns>
        public FileInfo InferSourceFileFromCSVFileName(FileInfo csvFile, DirectoryInfo diSourceDir)
        {
            string csvFname = Path.GetFileNameWithoutExtension(csvFile.FullName);
            string[] parts = csvFname.Split('_'); //assume that underscore plus analysis type has been added to source name
            string sourceName = parts[0];
            for (int i = 1; i < parts.Length - 1; i++) sourceName += ("_" + parts[i]);

            var fiSourceMP3 = new FileInfo(Path.Combine(diSourceDir.FullName, sourceName + ".mp3"));
            var fiSourceWAV = new FileInfo(Path.Combine(diSourceDir.FullName, sourceName + ".wav"));
            if (fiSourceMP3.Exists) return fiSourceMP3;
            else
                if (fiSourceWAV.Exists) return fiSourceWAV;
                else
                    return null;
        }

        public int CSV2ARFF(FileInfo fiCsvfile)
        {
            if (!fiCsvfile.Exists)
            {
                Log.ErrorFormat("The selected CSV file does not exist: <{0}>", fiCsvfile.FullName);
                return 1;
            }

            int error = 0;

            // finally process the CSV file
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvfile.FullName, true);
            string[] headers = DataTableTools.GetColumnNames(dt);

            Log.DebugFormat("Table has {0} rows and {1} columns.", dt.Rows.Count, headers.Length);
            Log.Debug("List of column headers:");
            for (int i = 0; i < headers.Length; i++)
            {
                Log.DebugFormat("   {0}   {1}", i, headers[i]);
            }
            ConvertTable2ARFF(fiCsvfile, dt);

            return error;
        } // CSV2ARFF()

        public int ConvertTable2ARFF(FileInfo fiCsvfile, DataTable dt)
        {

            int error = 0;
            string title = fiCsvfile.Name;
            StringBuilder sb = new StringBuilder("% 1. Title: " + title);
            sb.AppendLine("%");
            sb.AppendLine("% 2. Sources:");
            sb.AppendLine("%    CSV file from Acoustic Analysis project.");
            sb.AppendLine("@RELATION acousticIndices");

            string[] headers = DataTableTools.GetColumnNames(dt);
            for (int i = 0; i < headers.Length; i++)
            {
                sb.AppendLine("@ATTRIBUTE " + headers[i] + " \tNUMERIC");
            }

            //sb.AppendLine("@ATTRIBUTE class \t{0,1,2,3,4,5,6,7}");
            sb.AppendLine("@DATA");
            foreach (DataRow row in dt.Rows)
            {
                sb.Append(row[headers[0]]);
                for (int i = 1; i < headers.Length; i++)
                {
                    sb.Append("," + row[headers[i]]);
                }
                sb.AppendLine();
                //sb.AppendLine(",5"); //use this line if insert class ATTRIBUTE above
            }

            string fName = Path.GetFileNameWithoutExtension(fiCsvfile.FullName);
            string arffPath = Path.Combine(fiCsvfile.DirectoryName, fName + ".arff");
            FileTools.WriteTextFile(arffPath, sb.ToString());

            return error;
        } // ConvertTable2ARFF()

        public IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            return dir.EnumerateFiles().Where(f => allowedExtensions.Contains(f.Extension));
        }

        private void LoadSettings()
        {
            const char tick = '\u2714';
            const char cross = '\u2718';

            LoggedConsole.WriteLine("BROWSER SETTINGS:");
            LoggedConsole.WriteWarnLine("(If a warning appears below, the problem can be corrected by editing the AudioBrowser.exe.config file.)");

            var SystemTemp = new List<DirectoryInfo>() { this.DefaultTempFilesDir };
            var ExeDir = new List<DirectoryInfo>() { this.GetExeDir };
            var ExpectedConfigDir = new List<DirectoryInfo>() { new DirectoryInfo(Path.Combine(this.GetExeDir.FullName, "ConfigFiles")) };

            // check for audio utilities
            try
            {
                this.AudioUtilityFfmpegExe = AppConfigHelper.GetFiles("AudioUtilityFfmpegExe", true, ",").FirstOrDefault(f => File.Exists(f.FullName));
                this.AudioUtilityFfprobeExe = AppConfigHelper.GetFiles("AudioUtilityFfprobeExe", true, ",").FirstOrDefault(f => File.Exists(f.FullName));
                this.AudioUtilityWvunpackExe = AppConfigHelper.GetFiles("AudioUtilityWvunpackExe", true, ",").FirstOrDefault(f => File.Exists(f.FullName));
                this.AudioUtilityMp3SpltExe = AppConfigHelper.GetFiles("AudioUtilityMp3SpltExe", true, ",").FirstOrDefault(f => File.Exists(f.FullName));
                this.AudioUtilitySoxExe = AppConfigHelper.GetFiles("AudioUtilitySoxExe", true, ",").FirstOrDefault(f => File.Exists(f.FullName));
                this.AudioUtilityShntoolExe = AppConfigHelper.GetFiles("AudioUtilityShntoolExe", true, ",").FirstOrDefault(f => File.Exists(f.FullName));

                LoggedConsole.WriteLine("{0} All audio utilities located.", tick);
            }
            catch (Exception ex)
            {
                LoadSettingsProblem("{0} WARNING! Could not find one or more of the audio utilities. " +
                    "You will not be able to work with the original source file. Fix the error: {1}", cross, ex);
            }

            // external programs
            this.AudacityExe = AppConfigHelper.GetFiles("AudacityExeList", false, ",").FirstOrDefault(f => File.Exists(f.FullName));
            this.TextEditorExe = AppConfigHelper.GetFiles("TextEditorExeList", false, ",").FirstOrDefault(f => File.Exists(f.FullName));
            this.ConsoleExe = AppConfigHelper.GetFiles("ConsoleExeList", false, ",").FirstOrDefault(f => File.Exists(f.FullName));

            if (this.AudacityExe != null && File.Exists(this.AudacityExe.FullName))
            {
                LoggedConsole.WriteLine("{0} Audacity located at {1}.", tick, this.AudacityExe.FullName);
            }
            else
            {
                LoadSettingsProblem("{0} WARNING! Could not find Audacity.", cross);

            }

            if (this.TextEditorExe != null && File.Exists(this.TextEditorExe.FullName))
            {
                LoggedConsole.WriteLine("{0} Text editor located at {1}.", tick, this.TextEditorExe.FullName);
            }
            else
            {
                LoadSettingsProblem("{0} WARNING! Could not find a text editor. You will not be able to edit config files.", cross);
            }

            // directories
            this.AnalysisWorkingDir = AppConfigHelper.GetDirs("AnalysisWorkingDir", false, ",").Concat(SystemTemp).FirstOrDefault(f => Directory.Exists(f.FullName));
            this.DefaultConfigDir = AppConfigHelper.GetDirs("DefaultConfigDir", false, ",").Concat(ExpectedConfigDir).Concat(SystemTemp).FirstOrDefault(f => Directory.Exists(f.FullName));
            this.DefaultSourceDir = AppConfigHelper.GetDirs("DefaultSourceDir", false, ",").Concat(ExeDir).FirstOrDefault(f => Directory.Exists(f.FullName));
            this.DefaultOutputDir = AppConfigHelper.GetDirs("DefaultOutputDir", false, ",").Concat(ExeDir).FirstOrDefault(f => Directory.Exists(f.FullName));

            // DefaultConfigDir
            if (this.DefaultConfigDir != null && Directory.Exists(this.DefaultConfigDir.FullName))
            {
                LoggedConsole.WriteLine("{0} Found the config directory at {1}.", tick, this.DefaultConfigDir.FullName);
            }
            else
            {
                LoadSettingsProblem("{0} WARNING! The configuration file directory was not found: {1}.", cross, this.DefaultConfigDir);
            }

            // DefaultSourceDir
            if (this.DefaultSourceDir != null && Directory.Exists(this.DefaultSourceDir.FullName))
            {
                LoggedConsole.WriteLine("{0} Found the source audio directory at {1}.", tick, this.DefaultSourceDir.FullName);
            }
            else
            {
                LoadSettingsProblem("{0} WARNING! The source audio directory was not found: {1}.", cross, this.DefaultSourceDir);
            }

            // DefaultOutputDir
            if (this.DefaultOutputDir != null && Directory.Exists(this.DefaultOutputDir.FullName))
            {
                LoggedConsole.WriteLine("{0} Found the output directory at {1}.", tick, this.DefaultOutputDir.FullName);
            }
            else
            {
                LoadSettingsProblem("{0} WARNING! The output directory was not found: {1}. ", cross, this.DefaultOutputDir);
            }

            // check remaining values
            try
            {
                this.DefaultAudioFileExt = AppConfigHelper.GetString("DefaultAudioFileExt");
                this.DefaultConfigFileExt = AppConfigHelper.GetString("DefaultConfigFileExt");

                this.DefaultResultTextFileExt = AppConfigHelper.GetString("DefaultResultTextFileExt");
                this.DefaultResultImageFileExt = AppConfigHelper.GetString("DefaultResultImageFileExt");

                this.DefaultAnalysisIdentifier = AppConfigHelper.GetString("DefaultAnalysisName");

                this.DefaultSegmentDuration = AppConfigHelper.GetDouble("DefaultSegmentDuration");
                this.DefaultResampleRate = AppConfigHelper.GetInt("DefaultResampleRate");

                this.TrackHeight = AppConfigHelper.GetInt("TrackHeight");
                this.TrackCount = AppConfigHelper.GetInt("TrackCount");
                this.TrackNormalisedDisplay = AppConfigHelper.GetBool("TrackNormalisedDisplay");
                this.SonogramBackgroundThreshold = AppConfigHelper.GetDouble("SonogramBackgroundThreshold");

                LoggedConsole.WriteLine("{0} Other settings loaded successfully.", tick);

            }
            catch (Exception ex)
            {
                LoadSettingsProblem("{0} WARNING: There was a problem loading settings. Fix the error: {1}.", cross, ex);

            } //catch
        }

        private void LoadSettingsProblem(string formatString, params object[] args)
        {
            LoggedConsole.WriteWarnLine(formatString, args);

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }


    }
}
