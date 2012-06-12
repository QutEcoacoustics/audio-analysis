﻿namespace AudioBrowser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics; //for the StopWatch only
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

    using AnalysisPrograms;
    using AnalysisRunner;

    using AudioAnalysisTools;


    using log4net;

    using TowseyLib;
    using AnalysisBase;


    // 3 hr test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //8 min test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //SCC file site 4  // sunshinecoast1 "Y:\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //SCC file site 4  // sunshinecoast1 "\\hpc-fs.qut.edu.au\staging\availae\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"



    public partial class MainForm : Form
    {



        private static readonly ILog log = LogManager.GetLogger(typeof(MainForm));
        private readonly TextWriter consoleWriter;
        private readonly IAudioUtility audioUtilityForDurationColumn;

        int totalCheckBoxesCSVFileList = 0;
        int totalCheckedCheckBoxesCSVFileList = 0;
        CheckBox headerCheckBoxCSVFileList = null;
        bool isHeaderCheckBoxClickedCSVFileList = false;

        int totalCheckBoxesSourceFileList = 0;
        int totalCheckedCheckBoxesSourceFileList = 0;
        CheckBox headerCheckBoxSourceFileList = null;
        bool isHeaderCheckBoxClickedSourceFileList = false;

        private AudioBrowserSettings browserSettings;
        private Dictionary<string, string> analysisParams;

        // for calculating a visual index image
        private int sourceRecording_MinutesDuration = 0; //width of the index imageTracks = minutes duration of source recording.
        private double[] trackValues;
        private Bitmap selectionTrackImage;

        private string CurrentSourceFileAnalysisType { get { return ((KeyValuePair<string, string>)this.comboBoxSourceFileAnalysisType.SelectedItem).Key; } }
        private string CurrentCSVFileAnalysisType { get { return ((KeyValuePair<string, string>)this.comboBoxCSVFileAnalysisType.SelectedItem).Key; } }

        //identifers for the TAB panels/pages
        private string tabPageOutputFilesLabel = "tabPageOutputFiles";
        private string tabPageSourceFilesLabel = "tabPageSourceFiles";
        private string tabPageDisplayLabel = "tabPageDisplay";
        private string tabPageConsoleLabel = "tabPageConsole";

        private AnalysisCoordinator analysisCoordinator;
        private PluginHelper pluginHelper;

        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            // must be here, must be first
            InitializeComponent();



            //initialize instance of AudioBrowserSettings class
            browserSettings = new AudioBrowserSettings();
            try
            {
                browserSettings.LoadBrowserSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("WARNING: CANNOT LOCATE ONE OF THE FOLLOWING:\n\t The Browser Config File\n\t The Default Source Directory;\n" +
                                             "\t The Default Output Directory.\n\nCheck entries in the application file: app.config ");

                MessageBox.Show(ex.ToString());
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

            }

            //initialize analysis parameters
            try
            {
                this.analysisParams = ConfigDictionary.ReadPropertiesFile(browserSettings.fiAnalysisConfig.FullName);
            }
            catch (Exception ex) //DO NOT CATCH THIS EXCEPTION - DEAL WITH LATER
            {
                //MessageBox.Show("WARNING: CANNOT LOCATE ANALYSIS PROPERTIES FILE! ");
                //MessageBox.Show(ex.ToString());
            }

            this.analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = false,
                IsParallel = true,
                SubFoldersUnique = false
            };

            this.pluginHelper = new PluginHelper();
            this.pluginHelper.FindIAnalysisPlugins();

            //var results = this.analysisCoordinator.Run(files,analysis from dropdown,settings using config file);

            //Add the CheckBox into the source file list datagridview);
            this.headerCheckBoxSourceFileList = new CheckBox { Size = new Size(15, 15), ThreeState = true };
            this.dataGridViewFileList.Controls.Add(this.headerCheckBoxSourceFileList);
            this.headerCheckBoxSourceFileList.KeyUp += this.HeaderCheckBoxSourceFileList_KeyUp;
            this.headerCheckBoxSourceFileList.MouseClick += this.HeaderCheckBoxSourceFileList_MouseClick;

            //Add the CheckBox into the output file list datagridview
            this.headerCheckBoxCSVFileList = new CheckBox { Size = new Size(15, 15), ThreeState = true };
            this.dataGridCSVfiles.Controls.Add(this.headerCheckBoxCSVFileList);
            this.headerCheckBoxCSVFileList.KeyUp += this.HeaderCheckBoxCSVFileList_KeyUp;
            this.headerCheckBoxCSVFileList.MouseClick += this.HeaderCheckBoxCSVFileList_MouseClick;

            // Redirect the out Console stream
            this.consoleWriter = new TextBoxStreamWriter(this.textBoxConsole);
            Console.SetOut(this.consoleWriter);

            //use to display file size in file open window
            this.audioUtilityForDurationColumn = new MasterAudioUtility();

            // column formatting for output datagridview
            this.CsvFileDate.DefaultCellStyle.FormatProvider = new DateTimeFormatter();
            this.CsvFileDate.DefaultCellStyle.Format = DateTimeFormatter.FormatString;

            this.dataGridViewTextBoxColumnFileLength.DefaultCellStyle.FormatProvider = new ByteCountFormatter();
            this.dataGridViewTextBoxColumnFileLength.DefaultCellStyle.Format = ByteCountFormatter.FormatString;

            this.fileLengthDataGridViewTextBoxColumn.DefaultCellStyle.FormatProvider = new ByteCountFormatter();
            this.fileLengthDataGridViewTextBoxColumn.DefaultCellStyle.Format = ByteCountFormatter.FormatString;

            this.fileDateDataGridViewTextBoxColumn.DefaultCellStyle.FormatProvider = new DateTimeFormatter();
            this.fileDateDataGridViewTextBoxColumn.DefaultCellStyle.Format = DateTimeFormatter.FormatString;

            this.durationDataGridViewTextBoxColumn.DefaultCellStyle.FormatProvider = new TimeSpanFormatter();
            this.durationDataGridViewTextBoxColumn.DefaultCellStyle.Format = TimeSpanFormatter.FormatString;

            // background workers setup
            this.backgroundWorkerUpdateSourceFileList.DoWork += this.BackgroundWorkerUpdateSourceFileListDoWork;
            this.backgroundWorkerUpdateSourceFileList.RunWorkerCompleted +=
                (sender, e) =>
                this.BeginInvoke(
                    new Action<object, RunWorkerCompletedEventArgs>(
                    this.BackgroundWorkerUpdateSourceFileListRunWorkerCompleted),
                    sender,
                    e);

            this.backgroundWorkerUpdateCSVFileList.DoWork += this.BackgroundWorkerUpdateOutputFileListDoWork;
            this.backgroundWorkerUpdateCSVFileList.RunWorkerCompleted +=
                (sender, e) =>
                this.BeginInvoke(
                    new Action<object, RunWorkerCompletedEventArgs>(
                    this.BackgroundWorkerUpdateOutputFileListRunWorkerCompleted),
                    sender,
                    e);

            // if input and output dirs exist, populate datagrids
            if (browserSettings.DefaultSourceDir != null)
            {
                this.tfSourceDirectory.Text = browserSettings.DefaultSourceDir.FullName;

                if (Directory.Exists(browserSettings.DefaultSourceDir.FullName))
                {
                    this.UpdateSourceFileList();
                }
            }

            if (browserSettings.DefaultOutputDir != null)
            {
                //string opDir = Path.Combine(browserSettings.DefaultOutputDir.FullName, browserSettings.AnalysisName);
                //this.browserSettings.diOutputDir = new DirectoryInfo(opDir);
                this.browserSettings.diOutputDir = browserSettings.DefaultOutputDir;
                this.tfOutputDirectory.Text = this.browserSettings.diOutputDir.FullName;

                if (Directory.Exists(browserSettings.diOutputDir.FullName))
                {
                    this.UpdateOutputFileList();
                }
            }

            //this.comboBoxSourceFileAnalysisType.DataSource = browserSettings.AnalysisList.ToList();
            //this.comboBoxCSVFileAnalysisType.DataSource = browserSettings.AnalysisList.ToList();

            var analyserDict = new Dictionary<string, string>();


            foreach (var plugin in this.pluginHelper.AnalysisPlugins)
            {
                analyserDict.Add(plugin.Identifier, plugin.DisplayName);
            }

            var analyserList = analyserDict.OrderBy(a => a.Value).ToList();

            analyserList.Insert(0, new KeyValuePair<string, string>("none", "No Analysis"));

            this.comboBoxSourceFileAnalysisType.DataSource = analyserList.ToList();
            this.comboBoxSourceFileAnalysisType.DisplayMember = "Key";
            this.comboBoxSourceFileAnalysisType.DisplayMember = "Value";

            this.comboBoxCSVFileAnalysisType.DataSource = analyserList.ToList();
            this.comboBoxCSVFileAnalysisType.DisplayMember = "Key";
            this.comboBoxCSVFileAnalysisType.DisplayMember = "Value";

            //set default
            var defaultAnalyserExists = analyserList.Any(a => a.Key == browserSettings.AnalysisIdentifier);
            if (defaultAnalyserExists)
            {
                var defaultAnalyser = analyserList.First(a => a.Key == browserSettings.AnalysisIdentifier);
                this.comboBoxSourceFileAnalysisType.SelectedItem = defaultAnalyser;
                this.comboBoxCSVFileAnalysisType.SelectedItem = defaultAnalyser;
            }

            string analysisName = ((KeyValuePair<string, string>)this.comboBoxCSVFileAnalysisType.SelectedItem).Key;

            Console.WriteLine(AudioBrowserTools.BROWSER_TITLE_TEXT);
            Console.WriteLine(DateTime.Now);

            LoadAnalysisConfigFile(analysisName);
            WriteBrowserParameters2Console(this.browserSettings);
            ConfirmAllDefaultDirectoriesAndFilesExist(this.browserSettings);



            this.tabControlMain.SelectTab(tabPageConsoleLabel);

        } //MainForm()

        /// <summary>
        /// THIS METHOD ASSUMES THAT CONFIG FILE IS IN CONFIG DIR AND HAS DEFAULT NAME
        /// </summary>
        private void LoadAnalysisConfigFile(string analysisName)
        {
            this.comboBoxSourceFileAnalysisType.SelectedItem = analysisName;
            this.comboBoxCSVFileAnalysisType.SelectedItem = analysisName;

            this.browserSettings.AnalysisIdentifier = analysisName;
            if (analysisName == "None")
            {
                Console.WriteLine("#######  WARNING: ANALAYSIS NAME = 'None' #######");
                Console.WriteLine("\t There is no CONFIG file for the \"none\" ANALYSIS! ");
                this.browserSettings.fiAnalysisConfig = null;
                this.analysisParams = null;
                return;
            }

            string configDir = this.browserSettings.diConfigDir.FullName;
            string configPath = Path.Combine(configDir, analysisName + AudioBrowserSettings.DefaultConfigExt);
            this.browserSettings.fiAnalysisConfig = new FileInfo(configPath);
            this.analysisParams = ConfigDictionary.ReadPropertiesFile(configPath);
        }

        private static void WriteBrowserParameters2Console(AudioBrowserSettings parameters)
        {
            Console.WriteLine();
            Console.WriteLine("# Browser Settings:");
            Console.WriteLine("\tAnalysis Name: " + parameters.AnalysisIdentifier);
            if (parameters.fiAnalysisConfig == null)
                Console.WriteLine("\tAnalysis Config File: NULL");
            else Console.WriteLine("\tAnalysis Config File: " + parameters.fiAnalysisConfig.FullName);
            Console.WriteLine("\tSource Directory:     " + parameters.diSourceDir.FullName);
            Console.WriteLine("\tOutput Directory:     " + parameters.diOutputDir.FullName);
            Console.WriteLine("\tDisplay:  Track Height={0}pixels. Tracks normalised={1}.", parameters.TrackHeight, parameters.TrackNormalisedDisplay);
            Console.WriteLine("####################################################################################\n");
        }

        private static void ConfirmAllDefaultDirectoriesAndFilesExist(AudioBrowserSettings parameters)
        {

            if (parameters.fiAnalysisConfig == null) Console.WriteLine("\tWARNING: A valid config has not been set.");
            else
                if (!parameters.fiAnalysisConfig.Exists) Console.WriteLine("\tWARNING: The config does not exist: {0}", parameters.fiAnalysisConfig.FullName);
            //check the source directory
            if (parameters.diSourceDir == null) Console.WriteLine("\tWARNING: A valid source directory has not been set.");
            else
                if (!parameters.diSourceDir.Exists) Console.WriteLine("\tWARNING: The source directory does not exist: {0}", parameters.diSourceDir.FullName);

            //check the output directory
            if (parameters.diOutputDir == null) Console.WriteLine("\tWARNING: A valid output directory has not been set.");
            else
                if (!parameters.diOutputDir.Exists) Console.WriteLine("\tWARNING: The output directory does not exist: {0}", parameters.diOutputDir.FullName);
        }

        private static void WriteAnalysisParameters2Console(Dictionary<string, string> dict, string analysisName)
        {
            Console.WriteLine("# Parameters for Analysis: " + analysisName);
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                Console.WriteLine("\t{0} = {1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine("####################################################################################");
        }

        private static bool CheckForConsistencyOfAnalysisTypes(string currentAnalysisName, Dictionary<string, string> dict)
        {
            string analysisName = dict[AudioAnalysisTools.Keys.ANALYSIS_NAME];
            if (!currentAnalysisName.Equals(analysisName))
            {
                Console.WriteLine("WARNING: Analysis type selected in browser ({0}) not same as that in config file ({1})", currentAnalysisName, analysisName);
                return false;
            }
            Console.WriteLine("Analysis type: " + currentAnalysisName);
            return true;
        }

        private void btnAnalyseSelectedAudioFiles_Click(object sender, EventArgs e)
        {

            string analysisName = ((KeyValuePair<string, string>)this.comboBoxSourceFileAnalysisType.SelectedItem).Key;
            this.browserSettings.AnalysisIdentifier = analysisName;
            string configPath = Path.Combine(browserSettings.diConfigDir.FullName, analysisName + AudioBrowserSettings.DefaultConfigExt);
            var fiConfig = new FileInfo(configPath);
            this.analysisParams = ConfigDictionary.ReadPropertiesFile(configPath);

            this.browserSettings.fiAnalysisConfig = fiConfig;
            WriteAnalysisParameters2Console(this.analysisParams, this.CurrentSourceFileAnalysisType);
            CheckForConsistencyOfAnalysisTypes(this.CurrentSourceFileAnalysisType, this.analysisParams);

            int count = 0;

            //this.textBoxConsole.Clear();
            this.tabControlMain.SelectTab(tabPageConsoleLabel);
            //string date = "# DATE AND TIME: " + DateTime.Now;
            //Console.WriteLine(date);
            //Console.WriteLine("# ACOUSTIC ENVIRONMENT BROWSER");

            //var files = ((IEnumerable<DataGridViewRow>)this.dataGridViewFileList.Rows).Select(i => i.DataBoundItem as MediaFileItem).Select(m => new FileSegment { OriginalFile = m.FullName });

            //this.analysisCoordinator.Run(files, null, null, null);

            foreach (DataGridViewRow row in this.dataGridViewFileList.Rows)
            {
                var checkBoxCol = row.Cells["selectedDataGridViewCheckBoxColumn"] as DataGridViewCheckBoxCell;
                var item = row.DataBoundItem as MediaFileItem;

                if (checkBoxCol == null || item == null || checkBoxCol.Value == null) continue;

                var isChecked = (bool)checkBoxCol.Value;

                if (isChecked)
                {
                    count++;

                    var audioFileName = item.FileName;
                    var fiSourceRecording = item.FullName;
                    browserSettings.fiSourceRecording = fiSourceRecording;
                    Console.WriteLine("# Source audio - filename: " + Path.GetFileName(fiSourceRecording.Name));
                    Console.WriteLine("# Source audio - datetime: {0}    {1}", fiSourceRecording.CreationTime.ToLongDateString(), fiSourceRecording.CreationTime.ToLongTimeString());
                    Console.WriteLine("# Start processing at: {0}", DateTime.Now.ToLongTimeString());

                    Stopwatch stopwatch = new Stopwatch(); //for checking the parallel loop.
                    stopwatch.Start();
                    //################# PROCESS THE RECORDING #####################################################################################

                    var currentlySelectedIdentifier = ((KeyValuePair<string, string>)this.comboBoxSourceFileAnalysisType.SelectedItem).Key;
                    var analyser = this.pluginHelper.AnalysisPlugins.FirstOrDefault(a => a.Identifier == currentlySelectedIdentifier);

                    var settings = analyser.DefaultSettings;
                    settings.AnalysisBaseDirectory = this.browserSettings.diOutputDir;
                    settings.ConfigFile = fiConfig;

                    // this will only work for one file, since we need to sort the output afterwards.
                    var file = new FileSegment { 
                        OriginalFile = fiSourceRecording, 
                        //SegmentStartOffset = TimeSpan.Zero, 
                        //SegmentEndOffset = TimeSpan.FromMinutes(3) 
                    };

                    var analyserResults = this.analysisCoordinator.Run(new[] { file }, analyser, settings).OrderBy(a => a.SegmentStartOffset);

                    DataTable datatable = null;
                    for (var index = 0; index < analyserResults.Count(); index++)
                    {
                        var analyserResult = analyserResults.Skip(index).FirstOrDefault();
                        datatable = AudioBrowserTools.AppendToDataTable(
                            datatable,
                            analyserResult.Data,
                            analyserResult.AudioDuration,
                            analyserResult.SegmentStartOffset,
                            index);
                    }

                    var audioUtility = new MasterAudioUtility(settings.SegmentTargetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);
                    var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
                    var sourceDuration = audioUtility.Duration(fiSourceRecording, mimeType);

                    var op = AudioBrowserTools.GetEventsAndIndiciesDataTables(datatable, analyser, sourceDuration);

                    // var op = AudioBrowserTools.ProcessRecording(fiSourceRecording, this.browserSettings.diOutputDir, fiConfig);
                    var eventsDataTable = op.Item1;
                    var indicesDataTable = op.Item2;
                    //#############################################################################################################################
                    stopwatch.Stop();
                    //DataTableTools.WriteTable2Console(indicesDataTable);


                    string reportFileExt = ".csv";
                    string opDir = this.tfOutputDirectory.Text;
                    string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + this.CurrentSourceFileAnalysisType;
                    string reportfilePath;
                    int outputCount = 0;

                    //different things happen depending on the content of the analysis data table
                    if (indicesDataTable != null) //outputdata consists of rows of one minute indices 
                    {
                        outputCount = indicesDataTable.Rows.Count;
                        string sortString = (AudioAnalysisTools.Keys.INDICES_COUNT + " ASC");
                        indicesDataTable = DataTableTools.SortTable(indicesDataTable, sortString);    //sort by start time
                        reportfilePath = Path.Combine(opDir, fName + "Indices" + reportFileExt);
                        CsvTools.DataTable2CSV(indicesDataTable, reportfilePath);

                        string target = Path.Combine(opDir, fName + "Indices_BACKUP" + reportFileExt);
                        File.Delete(target);               // Ensure that the target does not exist.
                        File.Copy(reportfilePath, target); // Copy the file 2 target
                    }

                    if (eventsDataTable != null) //outputdata consists of rows of acoustic events 
                    {
                        outputCount = eventsDataTable.Rows.Count;
                        string sortString = (AudioAnalysisTools.Keys.EVENT_START_ABS + " ASC");
                        eventsDataTable = DataTableTools.SortTable(eventsDataTable, sortString);    //sort by start time
                        reportfilePath = Path.Combine(opDir, fName + "Events" + reportFileExt);
                        CsvTools.DataTable2CSV(eventsDataTable, reportfilePath);

                        string target = Path.Combine(opDir, fName + "Events_BACKUP" + reportFileExt);
                        File.Delete(target);               // Ensure that the target does not exist.
                        File.Copy(reportfilePath, target); // Copy the file 2 target
                    }

                    Console.WriteLine("###################################################");
                    Console.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
                    Console.WriteLine("Output  to  directory: " + opDir);
                    Console.WriteLine("CSV file(s): " + fName + "Events/Indices" + reportFileExt);

                    //Remaining LINES ARE FOR DIAGNOSTIC PURPOSES ONLY
                    TimeSpan ts = stopwatch.Elapsed;
                    Console.WriteLine("Processing time: {0:f3} seconds ({1}min {2}s)", (stopwatch.ElapsedMilliseconds / (double)1000), ts.Minutes, ts.Seconds);
                    Console.WriteLine("Number of units of output: {0}", outputCount);
                    if (outputCount == 0) outputCount = 1;
                    Console.WriteLine("Average time per unit of output: {0:f3} seconds.", (stopwatch.ElapsedMilliseconds / (double)1000 / (double)outputCount));

                    Console.WriteLine("###################################################\n");

                }// if checked
            } //foreach

            if (this.dataGridViewFileList.RowCount < 1 || count < 1)
            {
                MessageBox.Show("No file is selected.");
            }
        }

        private void btnLoadVisualIndexAllSelected_Click(object sender, EventArgs e)
        {
            int count = 0;

            //USE FOLLOWING LINES TO LOAD A PNG IMAGE
            //visualIndex.Image = new Bitmap(parameters.visualIndexPath);

            this.textBoxConsole.Clear();

            Console.WriteLine(AudioBrowserTools.BROWSER_TITLE_TEXT);
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(date);

            foreach (DataGridViewRow row in this.dataGridCSVfiles.Rows)
            {
                var checkBoxCol = row.Cells["dataGridViewCheckBoxColumnSelected"] as DataGridViewCheckBoxCell;
                var item = row.DataBoundItem as CsvFileItem;

                if (checkBoxCol == null || item == null || checkBoxCol.Value == null) continue;

                var isChecked = (bool)checkBoxCol.Value;

                if (isChecked)
                {
                    count++;

                    var csvFileName = item.FileName;
                    var csvFilePath =
                        new FileInfo(
                            Path.Combine(this.browserSettings.diOutputDir.FullName, csvFileName));

                    Console.WriteLine("# Display tracks in csv file: " + csvFileName);

                    //get analysis config settings
                    string analysisName = ((KeyValuePair<string, string>)this.comboBoxCSVFileAnalysisType.SelectedItem).Key;
                    LoadAnalysisConfigFile(analysisName);


                    this.pictureBoxSonogram.Image = null;  //reset in case old sonogram image is showing.
                    this.labelSonogramFileName.Text = "File Name";
                    this.browserSettings.fiCSVFile = csvFilePath; //store in settings so can be accessed later.

                    //##################################################################################################################
                    int status = this.LoadIndicesCSVFile(csvFilePath.FullName);
                    //##################################################################################################################

                    if (status != 0)
                    {
                        this.tabControlMain.SelectTab("tabPageConsole");
                        Console.WriteLine("FATAL ERROR: Error opening csv file");
                        Console.WriteLine("\t\tfile name:" + csvFilePath.FullName);
                        if (status == 1) Console.WriteLine("\t\tfile exists but could not extract values.");
                        if (status == 2) Console.WriteLine("\t\tfile exists but contains no values.");
                    }
                    else
                    {
                        this.selectionTrackImage = new Bitmap(this.pictureBoxBarTrack.Width, this.pictureBoxBarTrack.Height);
                        this.pictureBoxBarTrack.Image = this.selectionTrackImage;

                        //###################### MAKE VISUAL ADJUSTMENTS FOR HEIGHT OF THE VISUAL INDEX IMAGE  - THIS DEPENDS ON NUMBER OF TRACKS 
                        this.pictureBoxBarTrack.Location = new Point(0, this.pictureBoxVisualIndex.Height + 1);
                        //this.pictureBoxVisualIndex.Location = new Point(0, tracksImage.Height + 1);
                        this.panelDisplayImageAndTrackBar.Height = this.pictureBoxVisualIndex.Height + this.pictureBoxBarTrack.Height + 20; //20 = ht of scroll bar
                        this.panelDisplaySpectrogram.Location = new Point(3, panelDisplayImageAndTrackBar.Height + 1);
                        this.pictureBoxSonogram.Location = new Point(3, 0);

                        this.labelSourceFileName.Text = Path.GetFileNameWithoutExtension(csvFileName);
                        this.labelSourceFileDurationInMinutes.Text = "File duration = " + this.sourceRecording_MinutesDuration + " minutes";
                        this.tabControlMain.SelectTab("tabPageDisplay");
                    } // (status == 0)
                } // if (isChecked)
            } //for each row in dataGridCSVfiles
            //settings.fiCSVFile = new FileInfo();

            if (this.dataGridCSVfiles.RowCount < 1 || count < 1)
            {
                MessageBox.Show("No CSV file is selected.");
            }
        }

        /// <summary>
        /// loads a csv file of indices
        /// returns a status integer. 0= no error
        /// </summary>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        private int LoadIndicesCSVFile(string csvPath)
        {
            string analyisName = this.CurrentCSVFileAnalysisType;
            IAnalysis analyser = AudioBrowserTools.GetAcousticAnalyser(analyisName, this.pluginHelper.AnalysisPlugins);
            if (analyser == null)
            {
                Console.WriteLine("\nWARNING: Could not construct image from CSV file.");
                Console.WriteLine("\t Analysis name not recognized: " + analyisName);
                return 3;
            }

            var output = analyser.ProcessCsvFile(new FileInfo(csvPath), this.browserSettings.fiAnalysisConfig);
            DataTable dtRaw = output.Item1;
            DataTable dt2Display = output.Item2;
            this.sourceRecording_MinutesDuration = dt2Display.Rows.Count; //CAUTION: assume one value per minute - //set global variable !!!!
            analyser = null;

            string[] originalHeaders = DataTableTools.GetColumnNames(dtRaw);
            string[] displayHeaders = DataTableTools.GetColumnNames(dt2Display);

            //make values of bottom track available
            this.trackValues = DataTableTools.Column2ListOfDouble(dtRaw, displayHeaders[displayHeaders.Length - 1]).ToArray();

            //display column headers in the list box of displayed tracks
            List<string> displayList = displayHeaders.ToList();
            List<string> abbrevList = new List<string>();
            foreach (string str in displayList) abbrevList.Add(str.Substring(0, 5)); //the headers have been tampered with!! but assume not first 5 chars
            for (int i = 0; i < originalHeaders.Length; i++)
            {
                string text = originalHeaders[i].Substring(0, 5);
                if (abbrevList.Contains(text))
                    this.listBoxDisplayedTracks.Items.Add(String.Format("{0:d2}: {1}  (displayed)", (i + 1), originalHeaders[i]));
                else
                    this.listBoxDisplayedTracks.Items.Add(String.Format("{0:d2}: {1}", (i + 1), originalHeaders[i]));
            }
            string labelText = originalHeaders.Length + " headers in CSV file - " + displayHeaders.Length + " displayed.";
            this.labelCSVHeaders.Text = labelText;

            string imagePath = Path.Combine(browserSettings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(csvPath) + ".png"));
            Bitmap tracksImage = AudioBrowserTools.ConstructVisualIndexImage(dt2Display, browserSettings.TrackHeight, browserSettings.TrackNormalisedDisplay, imagePath);
            this.pictureBoxVisualIndex.Image = tracksImage;

            int error = 0;
            return error;
        }

        private void pictureBoxVisualIndex_MouseHover(object sender, EventArgs e)
        {
            this.pictureBoxVisualIndex.Cursor = Cursors.HSplit;
        }

        private void pictureBoxVisualIndex_MouseMove(object sender, MouseEventArgs e)
        {
            int myX = e.X; //other mouse calls:       Form.MousePosition.X  and  Mouse.GetPosition(this.pictureBoxVisualIndex); and   Cursor.Position;
            if (myX > this.sourceRecording_MinutesDuration - 1) return; //minuteDuration was set during load

            string text = (myX / 60) + "hr:" + (myX % 60) + "min (" + myX + ")"; //assumes scale= 1 pixel / minute
            this.textBoxCursorLocation.Text = text; // pixel position = minutes

            //mark the time scale
            Graphics g = this.pictureBoxVisualIndex.CreateGraphics();
            g.DrawImage(this.pictureBoxVisualIndex.Image, 0, 0);
            float[] dashValues = { 2, 2, 2, 2 };
            Pen pen = new Pen(Color.Red, 1.0F);
            pen.DashPattern = dashValues;
            Point pt1 = new Point(myX - 1, 2);
            Point pt2 = new Point(myX - 1, this.pictureBoxVisualIndex.Height);
            g.DrawLine(pen, pt1, pt2);
            pt1 = new Point(myX + 1, 2);
            pt2 = new Point(myX + 1, this.pictureBoxVisualIndex.Height);
            g.DrawLine(pen, pt1, pt2);

            if ((trackValues == null) || (trackValues.Length < 2)) return;
            if (myX >= this.trackValues.Length - 1)
                this.textBoxCursorValue.Text = String.Format("{0:f2} <<{1:f2}>> {2:f2}", this.trackValues[myX - 1], this.trackValues[myX], "END");
            else
                if (myX <= 0)
                    this.textBoxCursorValue.Text = String.Format("{0:f2} <<{1:f2}>> {2:f2}", "START", this.trackValues[myX], this.trackValues[myX + 1]);
                else
                    this.textBoxCursorValue.Text = String.Format("{0:f2} <<{1:f2}>> {2:f2}", this.trackValues[myX - 1], this.trackValues[myX], this.trackValues[myX + 1]);
        }

        private void pictureBoxVisualIndex_MouseClick(object sender, MouseEventArgs e)
        {
            this.textBoxConsole.Clear();
            this.tabControlMain.SelectTab("tabPageConsole");
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(date);
            Console.WriteLine("# ACOUSTIC ENVIRONMENT BROWSER");

            //Infer source file name from CSV file name
            FileInfo inferredSourceFile = AudioBrowserTools.InferSourceFileFromCSVFileName(browserSettings.fiCSVFile, this.browserSettings.diSourceDir);
            if (inferredSourceFile == null)
            {
                browserSettings.fiSourceRecording = null;
                Console.WriteLine("# \tWARNING: Cannot find mp3/wav source for csv: " + Path.GetFileNameWithoutExtension(browserSettings.fiCSVFile.FullName));
                Console.WriteLine("    Cannot proceed with display of segment sonogram.");
                return;
            }
            else
            {
                browserSettings.fiSourceRecording = inferredSourceFile;
                Console.WriteLine("# \tInferred source recording: " + inferredSourceFile.Name);
                Console.WriteLine("# \t\tCHECK THAT THIS IS THE CORRECT SOURCE RECORDING FOR THE CSV FILE.");
            }


            // GET MOUSE LOCATION
            int myX = e.X;
            int myY = e.Y;

            //DRAW RED LINE ON BAR TRACK
            for (int y = 0; y < selectionTrackImage.Height; y++)
                selectionTrackImage.SetPixel(this.pictureBoxVisualIndex.Left + myX, y, Color.Red);
            this.pictureBoxBarTrack.Image = selectionTrackImage;

            double segmentDuration = this.browserSettings.DefaultSegmentDuration;
            int resampleRate = this.browserSettings.DefaultResampleRate;
            if (analysisParams != null)
            {
                segmentDuration = ConfigDictionary.GetDouble(AudioBrowserSettings.key_SEGMENT_DURATION, analysisParams);
                resampleRate = ConfigDictionary.GetInt(AudioBrowserSettings.key_RESAMPLE_RATE, analysisParams);
            }


            //EXTRACT RECORDING SEGMENT
            TimeSpan startMinute = new TimeSpan(0, myX, 0);
            TimeSpan endMinute = new TimeSpan(0, myX + 1, 0);
            //if (segmentDuration == 3)
            //{
            //    startMinute = new TimeSpan(0, myX - 1, 0);
            //    endMinute   = new TimeSpan(0, myX + 2, 0);
            //}
            TimeSpan buffer = new TimeSpan(0, 0, 15);


            //get segment from source recording
            DateTime time1 = DateTime.Now;
            var fiSource = browserSettings.fiSourceRecording;
            string sourceFName = Path.GetFileNameWithoutExtension(fiSource.FullName);
            string segmentFName = sourceFName + "_min" + (int)startMinute.TotalMinutes + ".wav"; //want a wav file
            string outputSegmentPath = Path.Combine(browserSettings.diOutputDir.FullName, segmentFName); //path name of the segment file extracted from long recording
            Console.WriteLine("\n\tExtracting audio segment from source audio: minute " + myX + " to minute " + (myX + 1));
            Console.WriteLine("\n\tWriting audio segment to dir: " + browserSettings.diOutputDir.FullName);
            Console.WriteLine("\n\t\t\tFile Name: " + segmentFName);
            FileInfo fiOutputSegment = new FileInfo(outputSegmentPath);
            AudioBrowserTools.ExtractSegment(fiSource, startMinute, endMinute, buffer, resampleRate, fiOutputSegment);

            if (!fiOutputSegment.Exists)
            {
                Console.WriteLine("WARNING: Unable to extract segment to: {0}", fiOutputSegment.FullName);
                this.tabControlMain.SelectTab(this.tabPageConsoleLabel);
                return;
            }

            DateTime time2 = DateTime.Now;
            TimeSpan timeSpan = time2 - time1;
            Console.WriteLine("\n\t\t\tExtraction time: " + timeSpan.TotalSeconds + " seconds");

            //store info
            this.labelSonogramFileName.Text = Path.GetFileName(outputSegmentPath);
            this.browserSettings.fiSegmentRecording = fiOutputSegment;
            GetSonogram(fiOutputSegment);
        }


        private void buttonRunAudacity_Click(object sender, EventArgs e)
        {
            if ((browserSettings.fiSegmentRecording == null) || (!browserSettings.fiSegmentRecording.Exists))
            {
                Console.WriteLine("Audacity cannot open audio segment file: <" + browserSettings.fiSegmentRecording + ">");
                Console.WriteLine("It does not exist!");
                this.tabControlMain.SelectTab("tabPageConsole");
                AudioBrowserTools.RunAudacity(browserSettings.AudacityExe.FullName, " ", browserSettings.diOutputDir.FullName);
            }
            else
                AudioBrowserTools.RunAudacity(browserSettings.AudacityExe.FullName, browserSettings.fiSegmentRecording.FullName, browserSettings.diOutputDir.FullName);
        }

        // here be dragons!
        #region background workers for grid view lists

        private void BackgroundWorkerUpdateSourceFileListRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var items = e.Result as List<MediaFileItem>;
            if (!e.Cancelled && e.Error == null && items != null)
            {
                foreach (var item in items)
                {
                    mediaFileItemBindingSource.Add(item);
                }
            }

            this.dataGridViewFileList.Refresh();

            this.totalCheckBoxesSourceFileList = this.dataGridViewFileList.RowCount;
            this.totalCheckedCheckBoxesSourceFileList = 0;

            // replace existing settings
            browserSettings.diSourceDir = new DirectoryInfo(this.tfSourceDirectory.Text);
        }

        private void BackgroundWorkerUpdateSourceFileListDoWork(object sender, DoWorkEventArgs e)
        {
            var dir = new DirectoryInfo(this.tfSourceDirectory.Text);
            var files =
                dir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(
                    f =>
                    new[] { ".wav", ".mp3", ".wv", ".ogg", ".wma" }.Contains(f.Extension.ToLowerInvariant()))
                    .OrderBy(f => f.Name).Select(
                        f =>
                        {
                            var item = new MediaFileItem(f);
                            item.Duration = this.audioUtilityForDurationColumn.Duration(f, item.MediaType);
                            return item;
                        });
            e.Result = files.ToList();
        }

        private void BackgroundWorkerUpdateOutputFileListRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var items = e.Result as List<CsvFileItem>;
            if (!e.Cancelled && e.Error == null && items != null)
            {
                foreach (var item in items)
                {
                    this.csvFileItemBindingSource.Add(item);
                }
            }

            this.dataGridCSVfiles.Refresh();

            this.totalCheckBoxesCSVFileList = this.dataGridCSVfiles.RowCount;
            this.totalCheckedCheckBoxesCSVFileList = 0;

            // replace existing settings
            browserSettings.diOutputDir = new DirectoryInfo(tfOutputDirectory.Text);
        }

        private void BackgroundWorkerUpdateOutputFileListDoWork(object sender, DoWorkEventArgs e)
        {
            var dir = new DirectoryInfo(this.tfOutputDirectory.Text);
            var files =
                dir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(
                    f =>
                    new[] { ".csv" }.Contains(f.Extension.ToLowerInvariant()))
                    .OrderBy(f => f.Name).Select(
                        f =>
                        {
                            var item = new CsvFileItem(f);
                            return item;
                        });
            e.Result = files.ToList();
        }

        #endregion


        #region main form

        private void MainForm_Load(object sender, EventArgs e)
        {
            browserSettings = new AudioBrowserSettings();
            try
            {
                browserSettings.LoadBrowserSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnUpdateSourceFiles_Click(object sender, EventArgs e)
        {
            this.Validate();

            this.tabControlMain.SelectTab("tabPageSourceFiles");

            UpdateSourceFileList();
        }

        private void UpdateSourceFileList()
        {
            if (string.IsNullOrWhiteSpace(this.tfSourceDirectory.Text))
            {
                MessageBox.Show("Source directory path was not given.", "Error", MessageBoxButtons.OK);
                return;
            }

            if (!Directory.Exists(this.tfSourceDirectory.Text))
            {
                MessageBox.Show("The given source directory does not exist.", "Error", MessageBoxButtons.OK);
                return;
            }

            this.mediaFileItemBindingSource.Clear();

            if (!this.backgroundWorkerUpdateSourceFileList.IsBusy)
            {
                this.backgroundWorkerUpdateSourceFileList.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Already updating the file list. Please wait until the current update is complete.");
            }
        }

        private void btnUpdateCSVFileList_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.tabControlMain.SelectTab("tabPageOutputFiles");
            UpdateOutputFileList();

        }

        private void UpdateOutputFileList()
        {
            if (string.IsNullOrWhiteSpace(this.tfOutputDirectory.Text))
            {
                MessageBox.Show("Output directory path was not given.", "Error", MessageBoxButtons.OK);
                return;
            }

            if (!Directory.Exists(this.tfOutputDirectory.Text))
            {
                MessageBox.Show("The given output directory does not exist.", "Error", MessageBoxButtons.OK);
                return;
            }

            this.csvFileItemBindingSource.Clear();

            if (!this.backgroundWorkerUpdateCSVFileList.IsBusy)
            {
                this.backgroundWorkerUpdateCSVFileList.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Already updating the CSV file list. Please wait until the current update is complete.");
            }
        }

        private void btnSelectSourceDirectory_Click(object sender, EventArgs e)
        {
            this.Validate();

            if (Helpers.ValidDirectory(this.tfSourceDirectory.Text))
            {
                this.folderBrowserDialogChooseDir.SelectedPath = this.tfSourceDirectory.Text;
            }

            if (this.folderBrowserDialogChooseDir.ShowDialog() == DialogResult.OK)
            {
                this.tfSourceDirectory.Text = this.folderBrowserDialogChooseDir.SelectedPath;
                browserSettings.diSourceDir = new DirectoryInfo(this.tfSourceDirectory.Text);
            }

            this.tabControlMain.SelectTab("tabPageSourceFiles");
        }

        private void btnSelectOutputDirectory_Click(object sender, EventArgs e)
        {
            this.Validate();

            if (Helpers.ValidDirectory(this.tfOutputDirectory.Text))
            {
                this.folderBrowserDialogChooseDir.SelectedPath = this.tfOutputDirectory.Text;
            }

            if (this.folderBrowserDialogChooseDir.ShowDialog() == DialogResult.OK)
            {
                this.tfOutputDirectory.Text = this.folderBrowserDialogChooseDir.SelectedPath;
                browserSettings.diOutputDir = new DirectoryInfo(this.tfOutputDirectory.Text);
            }

            this.tabControlMain.SelectTab("tabPageOutputFiles");
        }

        private bool IsANonHeaderTextBoxCell(DataGridViewCellEventArgs cellEvent)
        {
            return this.dataGridViewFileList.Columns[cellEvent.ColumnIndex] is DataGridViewTextBoxColumn &&
                   cellEvent.RowIndex != -1;
        }

        private bool IsANonHeaderButtonCell(DataGridViewCellEventArgs cellEvent)
        {
            return this.dataGridViewFileList.Columns[cellEvent.ColumnIndex] is DataGridViewButtonColumn &&
                   cellEvent.RowIndex != -1;
        }

        private bool IsANonHeaderCheckBoxCell(DataGridViewCellEventArgs cellEvent)
        {
            return this.dataGridViewFileList.Columns[cellEvent.ColumnIndex] is DataGridViewCheckBoxColumn &&
                   cellEvent.RowIndex != -1;
        }

        #endregion

        #region dataGridViewSouceFileList source

        private void dataGridViewFileListSourceFileList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            var column = this.dataGridViewFileList.Columns[e.ColumnIndex];

            if (IsANonHeaderButtonCell(e))
            {

            }
            else if (this.IsANonHeaderCheckBoxCell(e))
            {
                var cell = this.dataGridViewFileList[e.ColumnIndex, e.RowIndex] as DataGridViewCheckBoxCell;
                if (cell != null)
                {
                    if (cell.Value == null)
                    {
                        cell.Value = true;
                    }
                    else
                    {
                        cell.Value = !((bool)cell.Value);
                    }

                    //MessageBox.Show(cell.Value.ToString());
                }
            }
            else if (this.IsANonHeaderTextBoxCell(e))
            {
                //MessageBox.Show("text clicked");
            }
        }

        private void dataGridViewFileListSourceFileList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex <= -1 || e.RowIndex <= -1)
            {
                return;
            }

            var cell = this.dataGridViewFileList[e.ColumnIndex, e.RowIndex] as DataGridViewCheckBoxCell;

            if (cell != null)
            {
                this.dataGridViewFileList.Rows[e.RowIndex].DefaultCellStyle.BackColor = (bool)cell.Value
                                                                                       ? Color.Yellow
                                                                                       : Color.White;

                this.dataGridViewFileList.CommitEdit(DataGridViewDataErrorContexts.Commit);
                this.dataGridViewFileList.EndEdit(DataGridViewDataErrorContexts.LeaveControl);
            }


            if (cell != null && !this.isHeaderCheckBoxClickedSourceFileList)
            {
                this.RowCheckBoxClickSourceFileList(cell);
            }
        }

        private void dataGridViewFileListSourceFileList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewFileList.CurrentCell is DataGridViewCheckBoxCell)
                dataGridViewFileList.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dataGridViewFileListSourceFileList_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex == 0)
                ResetHeaderCheckBoxLocationSourceFileList(e.ColumnIndex, e.RowIndex);
        }

        private void dataGridViewFileListSourceFileList_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        #endregion

        #region datagridviewoutputfilelist

        private void dataGridViewFileListCSVFileList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            var column = this.dataGridCSVfiles.Columns[e.ColumnIndex];

            if (IsANonHeaderButtonCell(e))
            {

            }
            else if (this.IsANonHeaderCheckBoxCell(e))
            {
                var cell = this.dataGridCSVfiles[e.ColumnIndex, e.RowIndex] as DataGridViewCheckBoxCell;
                if (cell != null)
                {
                    if (cell.Value == null)
                    {
                        cell.Value = true;
                    }
                    else
                    {
                        cell.Value = !((bool)cell.Value);
                    }

                    //MessageBox.Show(cell.Value.ToString());
                }
            }
            else if (this.IsANonHeaderTextBoxCell(e))
            {
                //MessageBox.Show("text clicked");
            }
        }

        private void dataGridViewFileListCSVFileList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex <= -1 || e.RowIndex <= -1)
            {
                return;
            }

            var cell = this.dataGridCSVfiles[e.ColumnIndex, e.RowIndex] as DataGridViewCheckBoxCell;

            if (cell != null)
            {
                this.dataGridCSVfiles.Rows[e.RowIndex].DefaultCellStyle.BackColor = (bool)cell.Value
                                                                                       ? Color.Yellow
                                                                                       : Color.White;

                this.dataGridCSVfiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
                this.dataGridCSVfiles.EndEdit(DataGridViewDataErrorContexts.LeaveControl);
            }


            if (cell != null && !this.isHeaderCheckBoxClickedCSVFileList)
            {
                this.RowCheckBoxClickCSVFileList(cell);
            }
        }

        private void dataGridViewFileListCSVFileList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridCSVfiles.CurrentCell is DataGridViewCheckBoxCell)
                dataGridCSVfiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dataGridViewFileListCSVFileList_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex == 0)
                ResetHeaderCheckBoxLocationCSVFileList(e.ColumnIndex, e.RowIndex);
        }

        private void dataGridViewFileListCSVFileList_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        #endregion

        #region checkboxheader source

        private void HeaderCheckBoxSourceFileList_MouseClick(object sender, MouseEventArgs e)
        {
            HeaderCheckBoxClickSourceFileList((CheckBox)sender);
        }

        private void HeaderCheckBoxSourceFileList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Space)
                HeaderCheckBoxClickSourceFileList((CheckBox)sender);
        }

        private void ResetHeaderCheckBoxLocationSourceFileList(int ColumnIndex, int RowIndex)
        {
            //Get the column header cell bounds
            Rectangle oRectangle = this.dataGridViewFileList.GetCellDisplayRectangle(ColumnIndex, RowIndex, true);

            Point oPoint = new Point
                {
                    X = oRectangle.Location.X + (oRectangle.Width - this.headerCheckBoxSourceFileList.Width) / 2 + 1,
                    Y = oRectangle.Location.Y + (oRectangle.Height - this.headerCheckBoxSourceFileList.Height) / 2 + 1
                };

            //Change the location of the CheckBox to make it stay on the header
            this.headerCheckBoxSourceFileList.Location = oPoint;
        }

        private void HeaderCheckBoxClickSourceFileList(CheckBox hCheckBox)
        {
            this.isHeaderCheckBoxClickedSourceFileList = true;

            if (hCheckBox.CheckState == CheckState.Indeterminate)
            {
                hCheckBox.CheckState = CheckState.Unchecked;
            }

            foreach (DataGridViewRow row in dataGridViewFileList.Rows)
            {
                row.Cells["selectedDataGridViewCheckBoxColumn"].Value = hCheckBox.CheckState == CheckState.Checked;
            }

            dataGridViewFileList.RefreshEdit();

            this.totalCheckedCheckBoxesSourceFileList = hCheckBox.Checked ? this.totalCheckBoxesSourceFileList : 0;

            this.isHeaderCheckBoxClickedSourceFileList = false;
        }

        private void RowCheckBoxClickSourceFileList(DataGridViewCheckBoxCell rCheckBox)
        {
            if (rCheckBox != null)
            {
                var state = (bool)rCheckBox.Value;

                //Modifiy Counter;            
                if (state && this.totalCheckedCheckBoxesSourceFileList < this.totalCheckBoxesSourceFileList)
                    this.totalCheckedCheckBoxesSourceFileList++;
                else if (this.totalCheckedCheckBoxesSourceFileList > 0)
                    this.totalCheckedCheckBoxesSourceFileList--;

                //Change state of the header CheckBox.
                if (this.totalCheckedCheckBoxesSourceFileList == 0)
                {
                    this.headerCheckBoxSourceFileList.CheckState = CheckState.Unchecked;
                }
                else if (this.totalCheckedCheckBoxesSourceFileList < this.totalCheckBoxesSourceFileList)
                {
                    this.headerCheckBoxSourceFileList.CheckState = CheckState.Indeterminate;
                }
                else if (this.totalCheckedCheckBoxesSourceFileList == this.totalCheckBoxesSourceFileList)
                {
                    this.headerCheckBoxSourceFileList.CheckState = CheckState.Checked;
                }
            }
        }

        #endregion

        #region check boxes output file list

        private void HeaderCheckBoxCSVFileList_MouseClick(object sender, MouseEventArgs e)
        {
            HeaderCheckBoxClickCSVFileList((CheckBox)sender);
        }

        private void HeaderCheckBoxCSVFileList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Space)
                HeaderCheckBoxClickCSVFileList((CheckBox)sender);
        }

        private void ResetHeaderCheckBoxLocationCSVFileList(int ColumnIndex, int RowIndex)
        {
            //Get the column header cell bounds
            Rectangle oRectangle = this.dataGridCSVfiles.GetCellDisplayRectangle(ColumnIndex, RowIndex, true);

            Point oPoint = new Point
            {
                X = oRectangle.Location.X + (oRectangle.Width - this.headerCheckBoxCSVFileList.Width) / 2 + 1,
                Y = oRectangle.Location.Y + (oRectangle.Height - this.headerCheckBoxCSVFileList.Height) / 2 + 1
            };

            //Change the location of the CheckBox to make it stay on the header
            this.headerCheckBoxCSVFileList.Location = oPoint;
        }

        private void HeaderCheckBoxClickCSVFileList(CheckBox hCheckBox)
        {
            this.isHeaderCheckBoxClickedCSVFileList = true;

            if (hCheckBox.CheckState == CheckState.Indeterminate)
            {
                hCheckBox.CheckState = CheckState.Unchecked;
            }

            foreach (DataGridViewRow row in dataGridCSVfiles.Rows)
            {
                row.Cells["dataGridViewCheckBoxColumnSelected"].Value = hCheckBox.CheckState == CheckState.Checked;
            }

            dataGridCSVfiles.RefreshEdit();

            this.totalCheckedCheckBoxesCSVFileList = hCheckBox.Checked ? this.totalCheckBoxesCSVFileList : 0;

            this.isHeaderCheckBoxClickedCSVFileList = false;
        }

        private void RowCheckBoxClickCSVFileList(DataGridViewCheckBoxCell rCheckBox)
        {
            if (rCheckBox != null)
            {
                var state = (bool)rCheckBox.Value;

                //Modifiy Counter;            
                if (state && this.totalCheckedCheckBoxesCSVFileList < this.totalCheckBoxesCSVFileList)
                    this.totalCheckedCheckBoxesCSVFileList++;
                else if (this.totalCheckedCheckBoxesCSVFileList > 0)
                    this.totalCheckedCheckBoxesCSVFileList--;

                //Change state of the header CheckBox.
                if (this.totalCheckedCheckBoxesCSVFileList == 0)
                {
                    this.headerCheckBoxCSVFileList.CheckState = CheckState.Unchecked;
                }
                else if (this.totalCheckedCheckBoxesCSVFileList < this.totalCheckBoxesCSVFileList)
                {
                    this.headerCheckBoxCSVFileList.CheckState = CheckState.Indeterminate;
                }
                else if (this.totalCheckedCheckBoxesCSVFileList == this.totalCheckBoxesCSVFileList)
                {
                    this.headerCheckBoxCSVFileList.CheckState = CheckState.Checked;
                }
            }
        }

        #endregion

        private void hScrollBarSonogram_ValueChanged(object sender, EventArgs e)
        {
            //this.pictureBoxSonogram.Left = -this.hScrollBarSonogram.Value;
        }

        /// <summary>
        /// handle event when refreshSonogram button is clicked
        /// redisplay the sonogram but with new Settings
        /// </summary>
        private void buttonRefreshSonogram_Click(object sender, EventArgs e)
        {
            GetSonogram(browserSettings.fiSegmentRecording);
        }

        private void GetSonogram(FileInfo fiAudio)
        {
            //check recording segment exists
            if ((fiAudio == null) || (!fiAudio.Exists))
            {
                if (fiAudio == null) Console.WriteLine("#######  CANNOT FIND AUDIO SEGMENT: segment = null");
                else
                    Console.WriteLine("#######  CANNOT FIND AUDIO SEGMENT: " + fiAudio.FullName);
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return;
            }

            string analysisName = this.CurrentSourceFileAnalysisType;
            if ((this.checkBoxSonogramAnnotate.Checked) && (analysisName == "none"))
            {
                Console.WriteLine("#######  CANNOT ANNOTATE SONOGRAM because SOURCE ANALYSIS TYPE = \"none\".");
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return;
            }

            //reload indices for source analysis type
            string opDir = browserSettings.diOutputDir.FullName;
            string configDir = this.browserSettings.diConfigDir.FullName;
            string configPath = Path.Combine(configDir, analysisName + AudioBrowserSettings.DefaultConfigExt);
            this.browserSettings.fiAnalysisConfig = new FileInfo(configPath);
            var config = ConfigDictionary.ReadPropertiesFile(configPath);
            config.Add(AudioAnalysisTools.Keys.ANNOTATE_SONOGRAM, this.checkBoxSonogramAnnotate.Checked.ToString());
            config.Add(AudioAnalysisTools.Keys.NOISE_DO_REDUCTION, this.checkBoxSonnogramNoiseReduce.Checked.ToString());
            config.Add(AudioAnalysisTools.Keys.NOISE_BG_REDUCTION, this.browserSettings.SonogramBackgroundThreshold.ToString());
            config[AudioAnalysisTools.Keys.ANALYSIS_NAME] = analysisName;
            var fiTempConfig = new FileInfo(Path.Combine(opDir, "temp.cfg"));
            ConfigDictionary.WriteConfgurationFile(config, fiTempConfig);

            Console.WriteLine("\n\tPreparing sonogram of audio segment");
            FileInfo fiImage = new FileInfo(Path.Combine(opDir, Path.GetFileNameWithoutExtension(fiAudio.FullName) + ".png"));
            Image image = AudioBrowserTools.GetImageFromAudioSegment(fiAudio, fiTempConfig, fiImage);

            if (image == null)
            {
                Console.WriteLine("FAILED TO EXTRACT IMAGE FROM AUDIO SEGMENT: " + fiAudio.FullName);
                this.checkBoxSonogramAnnotate.Checked = false; //if it was checked then uncheck because annotation failed
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
            }
            else
            {
                this.pictureBoxSonogram.Image = image;
                //this.panelDisplaySpectrogram.Height = image.Height;
                Console.WriteLine("\n\tSaved sonogram to image file: " + fiImage.FullName);
                this.tabControlMain.SelectTab(this.tabPageDisplayLabel);
                this.labelSonogramFileName.Text = fiAudio.Name;
                //attempt to deal with variable height of spectrogram
                //TODO:  MUST BE BETTER WAY TO DO THIS!!!!!
                if (this.pictureBoxSonogram.Image.Height > 270) this.panelDisplaySpectrogram.Height = 500;
                //Point location = this.panelDisplaySpectrogram.Location;
                //this.panelDisplaySpectrogram.Height = this.Height - location.Y;
                //this.panelDisplaySpectrogram.Height = this.pictureBoxSonogram.Image.Height;
                //this.pictureBoxSonogram.Location = new Point(3, 0);
                //this.vScrollBarSonogram.Minimum = 0;
            }

        } //buttonRefreshSonogram_Click()

        private void dataGridViewFileList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var formatter = e.CellStyle.FormatProvider as ICustomFormatter;
            if (formatter != null)
            {
                e.Value = formatter.Format(e.CellStyle.Format, e.Value, e.CellStyle.FormatProvider);
                e.FormattingApplied = true;
            }

        }

        private void dataGridCSVfiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var formatter = e.CellStyle.FormatProvider as ICustomFormatter;
            if (formatter != null)
            {
                e.Value = formatter.Format(e.CellStyle.Format, e.Value, e.CellStyle.FormatProvider);
                e.FormattingApplied = true;
            }

        }

    } //class MainForm : Form
}
