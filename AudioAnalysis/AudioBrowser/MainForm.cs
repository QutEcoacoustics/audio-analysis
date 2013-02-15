namespace AudioBrowser
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
    // using AnalysisPrograms.Processing;
    using AnalysisRunner;

    using AudioAnalysisTools;

    using LINQtoCSV;

    using log4net;

    using TowseyLib;
    using AnalysisBase;


    // 3 hr test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //8 min test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //SCC file site 4  // sunshinecoast1 "Y:\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //SCC file site 4  // sunshinecoast1 "\\hpc-fs.qut.edu.au\staging\availae\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"



    public partial class MainForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainForm));
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

        //private AnalysisCoordinator analysisCoordinator;
        private PluginHelper pluginHelper;

        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            // must be here, must be first
            InitializeComponent();

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



            //finds valid analysis files that implement the IAnalysis interface
            this.pluginHelper = new PluginHelper();
            this.pluginHelper.FindIAnalysisPlugins();
            //create list of analysers for display to user
            var analyserDict = new Dictionary<string, string>();
            foreach (var plugin in this.pluginHelper.AnalysisPlugins)
            {
                analyserDict.Add(plugin.Identifier, plugin.DisplayName);
            }
            var analyserList = analyserDict.OrderBy(a => a.Value).ToList();
            analyserList.Insert(0, new KeyValuePair<string, string>("none", "No Analysis"));

            //create comboBox display
            this.comboBoxSourceFileAnalysisType.DataSource = analyserList.ToList();
            this.comboBoxSourceFileAnalysisType.DisplayMember = "Key";
            this.comboBoxSourceFileAnalysisType.DisplayMember = "Value";

            this.comboBoxCSVFileAnalysisType.DataSource = analyserList.ToList();
            this.comboBoxCSVFileAnalysisType.DisplayMember = "Key";
            this.comboBoxCSVFileAnalysisType.DisplayMember = "Value";


            LoggedConsole.WriteLine(AudioBrowserTools.BROWSER_TITLE_TEXT);
            LoggedConsole.WriteLine(DateTime.Now);
            this.tabControlMain.SelectTab(tabPageConsoleLabel);
            //return;


            //REMAINDER OF METHOD LOADS BROWSER SETTINGS
            //initialize instance of AudioBrowserSettings class
            this.browserSettings = new AudioBrowserSettings();
            this.browserSettings.LoadBrowserSettings();


            // if input and output dirs exist, populate datagrids
            if (browserSettings.diSourceDir != null)
            {
                this.tfSourceDirectory.Text = browserSettings.diSourceDir.FullName;

                if (Directory.Exists(browserSettings.diSourceDir.FullName))
                {
                    this.UpdateSourceFileList();
                }
            }

            if (browserSettings.diOutputDir != null)
            {
                //this.browserSettings.diOutputDir = browserSettings.DefaultOutputDir;
                this.tfOutputDirectory.Text = this.browserSettings.diOutputDir.FullName;

                if (Directory.Exists(browserSettings.diOutputDir.FullName))
                {
                    this.UpdateOutputFileList();
                }
            }


            //set default analyser
            var defaultAnalyserExists = analyserList.Any(a => a.Key == browserSettings.AnalysisIdentifier);
            if (defaultAnalyserExists)
            {
                var defaultAnalyser = analyserList.First(a => a.Key == browserSettings.AnalysisIdentifier);
                this.comboBoxSourceFileAnalysisType.SelectedItem = defaultAnalyser;
                this.comboBoxCSVFileAnalysisType.SelectedItem = defaultAnalyser;
            }

            string analysisName = ((KeyValuePair<string, string>)this.comboBoxCSVFileAnalysisType.SelectedItem).Key;
            //this.comboBoxSourceFileAnalysisType.SelectedItem = analysisName;
            this.comboBoxCSVFileAnalysisType.SelectedItem = analysisName;
            this.browserSettings.AnalysisIdentifier = analysisName;

            var op = LoadAnalysisConfigFile(analysisName);
            this.browserSettings.fiAnalysisConfig = op.Item1;
            this.analysisParams = op.Item2;

            this.browserSettings.WriteSettings2Console();


        } //MainForm()


        /// <summary>
        /// THIS METHOD ASSUMES THAT CONFIG FILE IS IN CONFIG DIR AND HAS DEFAULT NAME
        /// </summary>
        private Tuple<FileInfo, Dictionary<string, string>> LoadAnalysisConfigFile(string analysisName)
        {
            FileInfo fi = null;
            Dictionary<string, string> dict = null;
            if ((analysisName == "None") || (analysisName == "none"))
            {
                LoggedConsole.WriteLine("#######  WARNING: ANALAYSIS NAME = 'None' #######");
                LoggedConsole.WriteLine("\t There is no CONFIG file for the \"none\" ANALYSIS! ");
                return Tuple.Create(fi, dict);
            }

            string configDir = this.browserSettings.diConfigDir.FullName;
            string configPath = Path.Combine(configDir, analysisName + AudioBrowserSettings.DefaultConfigExt);
            var fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                LoggedConsole.WriteLine("#######  WARNING: The CONFIG file does not exist: <" + configPath + ">");
                return Tuple.Create(fi, dict);
            }

            return Tuple.Create(fiConfig, ConfigDictionary.ReadPropertiesFile(configPath));
        }

        private static void WriteAnalysisParameters2Console(Dictionary<string, string> dict, string analysisName)
        {
            LoggedConsole.WriteLine("# Parameters for Analysis: " + analysisName);
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                LoggedConsole.WriteLine("\t{0} = {1}", kvp.Key, kvp.Value);
            }
            LoggedConsole.WriteLine("####################################################################################");
        }

        private static bool CheckForConsistencyOfAnalysisTypes(string currentAnalysisName, Dictionary<string, string> dict)
        {
            string analysisName = dict[AudioAnalysisTools.Keys.ANALYSIS_NAME];
            if (!currentAnalysisName.Equals(analysisName))
            {
                LoggedConsole.WriteLine("WARNING: Analysis type selected in browser ({0}) not same as that in config file ({1})", currentAnalysisName, analysisName);
                return false;
            }
            LoggedConsole.WriteLine("Analysis type: " + currentAnalysisName);
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


            //this.textBoxConsole.Clear();
            this.tabControlMain.SelectTab(tabPageConsoleLabel);

            int count = 0;
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
                    LoggedConsole.WriteLine("# Source audio - filename: " + Path.GetFileName(fiSourceRecording.Name));
                    LoggedConsole.WriteLine("# Source audio - datetime: {0}    {1}", fiSourceRecording.CreationTime.ToLongDateString(), fiSourceRecording.CreationTime.ToLongTimeString());
                    LoggedConsole.WriteLine("# Start processing at: {0}", DateTime.Now.ToLongTimeString());

                    Stopwatch stopwatch = new Stopwatch(); //for checking the parallel loop.
                    stopwatch.Start();

                    var currentlySelectedIdentifier = ((KeyValuePair<string, string>)this.comboBoxSourceFileAnalysisType.SelectedItem).Key;
                    var analyser = this.pluginHelper.AnalysisPlugins.FirstOrDefault(a => a.Identifier == currentlySelectedIdentifier);

                    var settings = analyser.DefaultSettings;
                    var configuration = new ConfigDictionary(fiConfig.FullName);
                    settings.SetUserConfiguration(fiConfig, configuration.GetTable(), this.browserSettings.diOutputDir,
                                                  AudioAnalysisTools.Keys.SEGMENT_DURATION, AudioAnalysisTools.Keys.SEGMENT_OVERLAP);

                    //################# PROCESS THE RECORDING #####################################################################################
                    var analyserResults = AudioBrowserTools.ProcessRecording(fiSourceRecording, analyser, settings);
                    //NEXT LINE was my old code
                    // var op1 = AudioBrowserTools.ProcessRecording(fiSourceRecording, this.browserSettings.diOutputDir, fiConfig);

                    if (analyserResults == null)
                    {
                        LoggedConsole.WriteLine("###################################################");
                        LoggedConsole.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
                        LoggedConsole.WriteLine("FATAL ERROR! NULL RETURN FROM analysisCoordinator.Run()");
                        return;
                    }

                    DataTable datatable = ResultsTools.MergeResultsIntoSingleDataTable(analyserResults);

                    //get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
                    var audioUtility = new MasterAudioUtility();
                    var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
                    var sourceInfo = audioUtility.Info(fiSourceRecording);

                    var op1 = ResultsTools.GetEventsAndIndicesDataTables(datatable, analyser, sourceInfo.Duration.Value);
                    var eventsDatatable = op1.Item1;
                    var indicesDatatable = op1.Item2;
                    int eventsCount = 0;
                    if (eventsDatatable != null) eventsCount = eventsDatatable.Rows.Count;
                    int indicesCount = 0;
                    if (indicesDatatable != null) indicesCount = indicesDatatable.Rows.Count;
                    var opdir = analyserResults.ElementAt(0).SettingsUsed.AnalysisRunDirectory;
                    string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + analyser.Identifier;
                    var op2 = ResultsTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);

                    //#############################################################################################################################
                    stopwatch.Stop();
                    //DataTableTools.WriteTable2Console(indicesDataTable);


                    //string reportFileExt = ".csv";
                    //string opDir = this.tfOutputDirectory.Text;
                    //string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + this.CurrentSourceFileAnalysisType;
                    //string reportfilePath;
                    //int outputCount = 0;

                    ////different things happen depending on the content of the analysis data table
                    //if (indicesDataTable != null) //outputdata consists of rows of one minute indices 
                    //{
                    //    outputCount = indicesDataTable.Rows.Count;
                    //    string sortString = (AudioAnalysisTools.Keys.INDICES_COUNT + " ASC");
                    //    indicesDataTable = DataTableTools.SortTable(indicesDataTable, sortString);    //sort by start time
                    //    reportfilePath = Path.Combine(opDir, fName + "Indices" + reportFileExt);
                    //    CsvTools.DataTable2CSV(indicesDataTable, reportfilePath);

                    //    string target = Path.Combine(opDir, fName + "Indices_BACKUP" + reportFileExt);
                    //    File.Delete(target);               // Ensure that the target does not exist.
                    //    File.Copy(reportfilePath, target); // Copy the file 2 target
                    //}

                    //if (eventsDataTable != null) //outputdata consists of rows of acoustic events 
                    //{
                    //    outputCount = eventsDataTable.Rows.Count;
                    //    string sortString = (AudioAnalysisTools.Keys.EVENT_START_ABS + " ASC");
                    //    eventsDataTable = DataTableTools.SortTable(eventsDataTable, sortString);    //sort by start time
                    //    reportfilePath = Path.Combine(opDir, fName + "Events" + reportFileExt);
                    //    CsvTools.DataTable2CSV(eventsDataTable, reportfilePath);

                    //    string target = Path.Combine(opDir, fName + "Events_BACKUP" + reportFileExt);
                    //    File.Delete(target);               // Ensure that the target does not exist.
                    //    File.Copy(reportfilePath, target); // Copy the file 2 target
                    //}

                    var fiEventsCSV = op2.Item1;
                    var fiIndicesCSV = op2.Item2;

                    //Remaining LINES ARE FOR DIAGNOSTIC PURPOSES ONLY
                    TimeSpan ts = stopwatch.Elapsed;
                    LoggedConsole.WriteLine("Processing time: {0:f3} seconds ({1}min {2}s)", (stopwatch.ElapsedMilliseconds / (double)1000), ts.Minutes, ts.Seconds);
                    int outputCount = eventsCount;
                    if (eventsCount == 0) outputCount = indicesCount;
                    LoggedConsole.WriteLine("Number of units of output: {0}", outputCount);
                    if (outputCount == 0) outputCount = 1;
                    LoggedConsole.WriteLine("Average time per unit of output: {0:f3} seconds.", (stopwatch.ElapsedMilliseconds / (double)1000 / (double)outputCount));

                    LoggedConsole.WriteLine("###################################################");
                    LoggedConsole.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
                    LoggedConsole.WriteLine("Output  to  directory: " + this.tfOutputDirectory.Text);
                    if (fiEventsCSV != null)
                    {
                        LoggedConsole.WriteLine("EVENTS CSV file(s) = " + fiEventsCSV.Name);
                        LoggedConsole.WriteLine("\tNumber of events = " + eventsCount);
                    }
                    if (fiIndicesCSV != null)
                    {
                        LoggedConsole.WriteLine("INDICES CSV file(s) = " + fiIndicesCSV.Name);
                        LoggedConsole.WriteLine("\tNumber of indices = " + indicesCount);
                    }
                    LoggedConsole.WriteLine("###################################################\n");

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

            LoggedConsole.WriteLine(AudioBrowserTools.BROWSER_TITLE_TEXT);
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(date);

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

                    //LoggedConsole.WriteLine("# Display tracks in csv file: " + csvFileName);

                    this.pictureBoxSonogram.Image = null;  //reset in case old sonogram image is showing.
                    this.labelSonogramFileName.Text = "File Name";
                    this.browserSettings.fiCSVFile = csvFilePath; //store in settings so can be accessed later.

                    //##################################################################################################################
                    int status = this.LoadIndicesCSVFile(csvFilePath.FullName);
                    //##################################################################################################################

                    if (status != 0)
                    {
                        this.tabControlMain.SelectTab("tabPageConsole");
                        LoggedConsole.WriteLine("FATAL ERROR: Error opening csv file");
                        LoggedConsole.WriteLine("\t\tfile name:" + csvFilePath.FullName);
                        if (status == 1) LoggedConsole.WriteLine("\t\tfile exists but could not extract values.");
                        if (status == 2) LoggedConsole.WriteLine("\t\tfile exists but contains no values.");
                    }
                    else
                    {
                        this.selectionTrackImage = new Bitmap(this.pictureBoxBarTrack.Width, this.pictureBoxBarTrack.Height);
                        this.pictureBoxBarTrack.Image = this.selectionTrackImage;

                        //###################### MAKE VISUAL ADJUSTMENTS FOR HEIGHT OF THE VISUAL INDEX IMAGE  - THIS DEPENDS ON NUMBER OF TRACKS 
                        this.pictureBoxBarTrack.Location = new Point(0, this.pictureBoxVisualIndices.Height + 1);
                        //this.pictureBoxVisualIndex.Location = new Point(0, tracksImage.Height + 1);
                        this.panelDisplayImageAndTrackBar.Height = this.pictureBoxVisualIndices.Height + this.pictureBoxBarTrack.Height + 20; //20 = ht of scroll bar
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



        private void btnViewFileOfIndices_Click(object sender, EventArgs e)
        {
            //USE FOLLOWING LINE TO LOAD A PNG IMAGE
            //visualIndex.Image = new Bitmap(parameters.visualIndexPath);

            this.textBoxConsole.Clear();

            LoggedConsole.WriteLine(AudioBrowserTools.BROWSER_TITLE_TEXT);
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(date);

            //OPEN A FILE DIALOGUE TO FIND CSV FILE
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open File Dialogue";
            fdlg.InitialDirectory = this.browserSettings.diOutputDir.FullName;
            fdlg.Filter = "CSV files (*.csv)|*.csv";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                var fiCSVFile = new FileInfo(fdlg.FileName);
                this.browserSettings.fiCSVFile = fiCSVFile; // store in settings so can be accessed later.

                this.browserSettings.diOutputDir = new DirectoryInfo(Path.GetDirectoryName(fiCSVFile.FullName)); // change to selected directory
                this.pictureBoxSonogram.Image = null;  //reset in case old sonogram image is showing.
                this.labelSonogramFileName.Text = "Sonogram file name";

                // ##################################################################################################################
                int status = this.LoadIndicesCSVFile(fiCSVFile.FullName);
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
                        LoggedConsole.WriteLine("# Display of the acoustic indices in csv file: " + fiCSVFile.FullName);
                        this.selectionTrackImage = new Bitmap(this.pictureBoxBarTrack.Width, this.pictureBoxBarTrack.Height);
                        this.pictureBoxBarTrack.Image = this.selectionTrackImage;

                        //###################### MAKE VISUAL ADJUSTMENTS FOR HEIGHT OF THE VISUAL INDEX IMAGE  - THIS DEPENDS ON NUMBER OF TRACKS 
                        this.pictureBoxBarTrack.Location = new Point(0, this.pictureBoxVisualIndices.Height + 1);
                        //this.pictureBoxVisualIndex.Location = new Point(0, tracksImage.Height + 1);
                        this.panelDisplayImageAndTrackBar.Height = this.pictureBoxVisualIndices.Height + this.pictureBoxBarTrack.Height + 20; //20 = ht of scroll bar
                        this.panelDisplaySpectrogram.Location = new Point(3, panelDisplayImageAndTrackBar.Height + 1);
                        this.pictureBoxSonogram.Location = new Point(3, 0);
                        this.labelSourceFileDurationInMinutes.Text = "                Image scale = 1 minute/pixel.   File duration = " + this.sourceRecording_MinutesDuration + " minutes";

                        this.labelSourceFileName.Text = Path.GetFileNameWithoutExtension(fiCSVFile.FullName);
                        if (status == 0)
                        {
                            this.labelSourceFileName.Text = Path.GetFileNameWithoutExtension(fiCSVFile.FullName);
                        }
                        else
                        {
                            this.labelSourceFileName.Text = String.Format("WARNING: ERROR parsing file name <{0}>.  READ CONSOLE MESSAGE!  ", Path.GetFileNameWithoutExtension(fiCSVFile.FullName));                            
                        }
                        this.tabControlMain.SelectTab("tabPageDisplay");
                    } // (status == 0)
            } // if (DialogResult.OK)

        }



        /// <summary>
        /// loads a csv file of indices
        /// returns a status integer. 0= no error
        /// </summary>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        private int LoadIndicesCSVFile(string csvPath)
        {
            int error = 0;

            // get analysis config settings from the comboBox
            // string analysisName = ((KeyValuePair<string, string>)this.comboBoxCSVFileAnalysisType.SelectedItem).Key;
            // NO!! Instead determine the analysis name from the CSV file name.
            // IMPORTANT: ASSUME that file name can be parsed to get the analysis type.

            // PARSE the file name
            string fileName = Path.GetFileNameWithoutExtension(csvPath);
            string[] array = fileName.Split('_');
            array = array[array.Length -1].Split('.');
            string analysisName = "Towsey.Default";
            if (array.Length >= 2) analysisName = array[0] + "." + array[1];

            bool isIndicesFile = false;
            if (array.Length >= 3)
                isIndicesFile = (array[2] == "Indices");

            if (!isIndicesFile)
            {
                LoggedConsole.WriteLine("\nWARNING: The file name did not parse correctly. This may not be a file of indices: " + fileName);
                error = 1;
            }
            var op = LoadAnalysisConfigFile(analysisName);
            this.browserSettings.fiAnalysisConfig = op.Item1;
            this.analysisParams = op.Item2;

            IAnalyser analyser = AudioBrowserTools.GetAcousticAnalyser(analysisName, this.pluginHelper.AnalysisPlugins);
            if ((! isIndicesFile) || (analyser == null))
            {
                LoggedConsole.WriteLine("\nWARNING: Analysis name not recognized: " + analysisName);
                LoggedConsole.WriteLine("           File name format should be: <AudioID_PersonID.AnalysisID.Indices.csv>");
                LoggedConsole.WriteLine("                          For example: <DM36000_Towsey.MultiAnalyser.Indices.csv>");
                LoggedConsole.WriteLine("           The file name should end with <.Indices.csv>");
                LoggedConsole.WriteLine("           The full analysis name will be: <PersonID.AnalysisID>, e.g. Towsey.MultiAnalyser");
                LoggedConsole.WriteLine("           The CSV file will be displayed using the default analysis module <Towsey.Default>.");
                error = 2;
                analyser = new AnalysisTemplate();
            }

            var output = analyser.ProcessCsvFile(new FileInfo(csvPath), this.browserSettings.fiAnalysisConfig);
            DataTable dtRaw = output.Item1;
            DataTable dt2Display = output.Item2;
            this.sourceRecording_MinutesDuration = dt2Display.Rows.Count; //CAUTION: assume one value per minute - //set global variable !!!!
            analyser = null;

            string[] originalHeaders = DataTableTools.GetColumnNames(dtRaw);
            string[] displayHeaders = DataTableTools.GetColumnNames(dt2Display);

            //make values of bottom track available
            string header = displayHeaders[displayHeaders.Length - 1];
            this.trackValues = DataTableTools.Column2ArrayOfDouble(dt2Display, header);

            //display column headers in the list box of displayed tracks
            List<string> displayList = displayHeaders.ToList();
            List<string> abbrevList = new List<string>();
            this.listBoxDisplayedTracks.Items.Clear(); //remove previous entries in list box.

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
                    this.listBoxDisplayedTracks.Items.Add(String.Format("{0:d2}: {1}  (displayed)", (i + 1), originalHeaders[i]));
                else
                    this.listBoxDisplayedTracks.Items.Add(String.Format("{0:d2}: {1}", (i + 1), originalHeaders[i]));
            }
            string labelText = originalHeaders.Length + " headers in CSV file - " + displayHeaders.Length + " displayed.";
            this.labelCSVHeaders.Text = labelText;

            string imagePath = Path.Combine(browserSettings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(csvPath) + ".png"));
            Bitmap tracksImage = DisplayIndices.ConstructVisualIndexImage(dt2Display, AudioBrowserTools.IMAGE_TITLE_TEXT, browserSettings.TrackNormalisedDisplay, imagePath);
            this.pictureBoxVisualIndices.Image = tracksImage;

            return error;
        }

        private void pictureBoxVisualIndex_MouseHover(object sender, EventArgs e)
        {
            this.pictureBoxVisualIndices.Cursor = Cursors.HSplit;
        }

        private void pictureBoxVisualIndex_MouseMove(object sender, MouseEventArgs e)
        {
            int myX = e.X; //other mouse calls:       Form.MousePosition.X  and  Mouse.GetPosition(this.pictureBoxVisualIndex); and   Cursor.Position;
            if (myX > this.sourceRecording_MinutesDuration - 1) return; //minuteDuration was set during load

            string text = (myX / 60) + "hr:" + (myX % 60) + "min (" + myX + ")"; //assumes scale= 1 pixel / minute
            this.textBoxCursorLocation.Text = text; // pixel position = minutes

            //mark the time scale
            Graphics g = this.pictureBoxVisualIndices.CreateGraphics();
            g.DrawImage(this.pictureBoxVisualIndices.Image, 0, 0);
            float[] dashValues = { 2, 2, 2, 2 };
            Pen pen = new Pen(Color.Red, 1.0F);
            pen.DashPattern = dashValues;
            Point pt1 = new Point(myX - 1, 2);
            Point pt2 = new Point(myX - 1, this.pictureBoxVisualIndices.Height);
            g.DrawLine(pen, pt1, pt2);
            pt1 = new Point(myX + 1, 2);
            pt2 = new Point(myX + 1, this.pictureBoxVisualIndices.Height);
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
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# ACOUSTIC ENVIRONMENT BROWSER");

            //Infer source file name from CSV file name
            FileInfo inferredSourceFile = AudioBrowserTools.InferSourceFileFromCSVFileName(browserSettings.fiCSVFile, this.browserSettings.diSourceDir);
            if (inferredSourceFile == null)
            {
                browserSettings.fiSourceRecording = null;
                LoggedConsole.WriteLine("# \tWARNING: Cannot find mp3/wav source for csv: " + Path.GetFileNameWithoutExtension(browserSettings.fiCSVFile.FullName));
                LoggedConsole.WriteLine("    Cannot proceed with display of segment sonogram.");
                return;
            }
            else
            {
                browserSettings.fiSourceRecording = inferredSourceFile;
                LoggedConsole.WriteLine("# \tInferred source recording: " + inferredSourceFile.Name);
                LoggedConsole.WriteLine("# \t\tCHECK THAT THIS IS THE CORRECT SOURCE RECORDING FOR THE CSV FILE.");
            }


            // GET MOUSE LOCATION
            int myX = e.X;
            int myY = e.Y;

            //DRAW RED LINE ON BAR TRACK
            for (int y = 0; y < selectionTrackImage.Height; y++)
                selectionTrackImage.SetPixel(this.pictureBoxVisualIndices.Left + myX, y, Color.Red);
            this.pictureBoxBarTrack.Image = selectionTrackImage;

            double segmentDuration = this.browserSettings.DefaultSegmentDuration;
            int resampleRate = this.browserSettings.DefaultResampleRate;
            if (analysisParams != null) //###################################### THIS MAy NEED CHECKING BECAUSE DO NOT SET RESAMPLE RATE BY DEFAULT
            {
                //if (analysisParams.ContainsKey(AudioBrowserSettings.key_SEGMENT_DURATION)) 
                //    segmentDuration = ConfigDictionary.GetDouble(AudioBrowserSettings.key_SEGMENT_DURATION, analysisParams);
                //if (analysisParams.ContainsKey(AudioBrowserSettings.key_RESAMPLE_RATE)) 
                //    resampleRate    = ConfigDictionary.GetInt(AudioBrowserSettings.key_RESAMPLE_RATE, analysisParams);
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
            LoggedConsole.WriteLine("\n\tExtracting audio segment from source audio: minute " + myX + " to minute " + (myX + 1));
            LoggedConsole.WriteLine("\n\tWriting audio segment to dir: " + browserSettings.diOutputDir.FullName);
            LoggedConsole.WriteLine("\n\t\t\tFile Name: " + segmentFName);
            FileInfo fiOutputSegment = new FileInfo(outputSegmentPath);
            //if (!fiOutputSegment.Exists) //extract the segment
            //{
            AudioRecording.ExtractSegment(fiSource, startMinute, endMinute, buffer, analysisParams, fiOutputSegment);
            //}

            fiOutputSegment.Refresh();
            if (!fiOutputSegment.Exists) //still has not been extracted
            {
                LoggedConsole.WriteLine("WARNING: Unable to extract segment to: {0}", fiOutputSegment.FullName);
                this.tabControlMain.SelectTab(this.tabPageConsoleLabel);
                return;
            }

            DateTime time2 = DateTime.Now;
            TimeSpan timeSpan = time2 - time1;
            LoggedConsole.WriteLine("\n\t\t\tExtraction time: " + timeSpan.TotalSeconds + " seconds");

            //store info
            this.labelSonogramFileName.Text = Path.GetFileName(outputSegmentPath);
            this.browserSettings.fiSegmentRecording = fiOutputSegment;
            Image image = GetSonogram(fiOutputSegment);

            if (image == null)
            {
                LoggedConsole.WriteLine("FAILED TO EXTRACT IMAGE FROM AUDIO SEGMENT: " + fiOutputSegment.FullName);
                this.checkBoxSonogramAnnotate.Checked = false; //if it was checked then uncheck because annotation failed
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
            }
            else
            {
                if (this.pictureBoxSonogram.Image != null)
                {
                    this.pictureBoxSonogram.Image.Dispose();
                }

                this.pictureBoxSonogram.Image = image;
                //this.panelDisplaySpectrogram.Height = image.Height;
                LoggedConsole.WriteLine("\n\tSaved sonogram to image file: " + fiOutputSegment.FullName);
                this.tabControlMain.SelectTab(this.tabPageDisplayLabel);
                string title = fiOutputSegment.Name;
                if (title.Length > 23)
                {
                    int remainder = title.Length - 22;
                    title = title.Substring(0, 21) + "\n   " + title.Substring(22, remainder);
                }

                this.labelSonogramFileName.Text = title;

                //attempt to deal with variable height of spectrogram
                //TODO:  MUST BE BETTER WAY TO DO THIS!!!!!
                if (this.pictureBoxSonogram.Image.Height > 270) this.panelDisplaySpectrogram.Height = 500;
                //Point location = this.panelDisplaySpectrogram.Location;
                //this.panelDisplaySpectrogram.Height = this.Height - location.Y;
                //this.panelDisplaySpectrogram.Height = this.pictureBoxSonogram.Image.Height;
                //this.pictureBoxSonogram.Location = new Point(3, 0);
                //this.vScrollBarSonogram.Minimum = 0;
            }

        } //pictureBoxVisualIndex_MouseClick()


        private void buttonRunAudacity_Click(object sender, EventArgs e)
        {
            // check Audacity is available
            if (browserSettings.AudacityExe == null)
            {
                LoggedConsole.WriteLine("\nWARNING: Cannot find Audacity at default locations.");
                LoggedConsole.WriteLine("   Enter correct Audacity path in the Browser's app.config file.");
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return;
            }
            FileInfo audacity = new FileInfo(browserSettings.AudacityExe.FullName);
            if (! audacity.Exists)
            {
                LoggedConsole.WriteLine("\nWARNING: Cannot find Audacity at <{0}>", browserSettings.AudacityExe.FullName);
                LoggedConsole.WriteLine("   Enter correct Audacity path in the Browser's app.config file.");
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return;
            }

            int status = 0;
            if ((browserSettings.fiSegmentRecording == null) || (!browserSettings.fiSegmentRecording.Exists))
            {
                LoggedConsole.WriteLine("Audacity cannot open audio segment file: <" + browserSettings.fiSegmentRecording + ">");
                LoggedConsole.WriteLine("The audio file does not exist!");
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                status = AudioBrowserTools.RunAudacity(browserSettings.AudacityExe.FullName, " ", browserSettings.diOutputDir.FullName);
            }
            else
                status = AudioBrowserTools.RunAudacity(browserSettings.AudacityExe.FullName, browserSettings.fiSegmentRecording.FullName, browserSettings.diOutputDir.FullName);
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
                            item.Duration = this.audioUtilityForDurationColumn.Info(f).Duration.Value;
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
            if (browserSettings.fiSegmentRecording == null)
            {
                LoggedConsole.WriteLine("YOU MUST SELECT A SEGMENT OF AUDIO BY CLICKING ON THE 'TRACKS' IMAGE.");
                this.checkBoxSonogramAnnotate.Checked = false; //if it was checked then uncheck because annotation failed
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return;
            }
            Image image = GetSonogram(browserSettings.fiSegmentRecording);
            if (image == null)
            {
                LoggedConsole.WriteLine("FAILED TO EXTRACT IMAGE FROM AUDIO SEGMENT: " + browserSettings.fiSegmentRecording.FullName);
                this.checkBoxSonogramAnnotate.Checked = false; //if it was checked then uncheck because annotation failed
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return;
            }
            else
            {
                if (this.pictureBoxSonogram.Image != null)
                {
                    this.pictureBoxSonogram.Image.Dispose();
                }

                this.pictureBoxSonogram.Image = image;
                LoggedConsole.WriteLine("\n\tSaved sonogram to image file: " + browserSettings.fiSegmentRecording.FullName);
                //this.tabControlMain.SelectTab(this.tabPageDisplayLabel);
                //this.labelSonogramFileName.Text = browserSettings.fiSegmentRecording.Name;
                //attempt to deal with variable height of spectrogram
                //TODO:  MUST BE BETTER WAY TO DO THIS!!!!!
                if (this.pictureBoxSonogram.Image.Height > 270) this.panelDisplaySpectrogram.Height = 500;
            }
        } //buttonRefreshSonogram_Click()

        private Image GetSonogram(FileInfo fiAudio)
        {
            //check recording segment exists
            if ((fiAudio == null) || (!fiAudio.Exists))
            {
                if (fiAudio == null) LoggedConsole.WriteLine("#######  CANNOT FIND AUDIO SEGMENT: segment = null");
                else
                    LoggedConsole.WriteLine("#######  CANNOT FIND AUDIO SEGMENT: " + fiAudio.FullName);
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return null;
            }

            string analysisName = this.CurrentSourceFileAnalysisType;
            if ((this.checkBoxSonogramAnnotate.Checked) && (analysisName == "none"))
            {
                LoggedConsole.WriteLine("#######  CANNOT ANNOTATE SONOGRAM because SOURCE ANALYSIS TYPE = \"none\".");
                this.tabControlMain.SelectTab(tabPageConsoleLabel);
                return null;
            }

            //reload indices for source analysis type
            string opDir = browserSettings.diOutputDir.FullName;
            string configDir = this.browserSettings.diConfigDir.FullName;
            string configPath = Path.Combine(configDir, analysisName + AudioBrowserSettings.DefaultConfigExt);

            if (!(new FileInfo(configPath)).Exists)
            {
                LoggedConsole.WriteLine("Config file does not exists: {0}", configPath);
                return null;
            }

            this.browserSettings.fiAnalysisConfig = new FileInfo(configPath);
            var config = ConfigDictionary.ReadPropertiesFile(configPath);
            SetConfigValue(config, AudioAnalysisTools.Keys.ANNOTATE_SONOGRAM, this.checkBoxSonogramAnnotate.Checked.ToString());
            SetConfigValue(config, AudioAnalysisTools.Keys.NOISE_DO_REDUCTION, this.checkBoxSonnogramNoiseReduce.Checked.ToString());
            SetConfigValue(config, AudioAnalysisTools.Keys.NOISE_BG_REDUCTION, this.browserSettings.SonogramBackgroundThreshold.ToString());
            SetConfigValue(config, AudioAnalysisTools.Keys.FRAME_LENGTH, "1024"); // do not want long spectrogram

            //config.Add(AudioAnalysisTools.Keys.NOISE_BG_REDUCTION, this.browserSettings.SonogramBackgroundThreshold.ToString());
            config[AudioAnalysisTools.Keys.ANALYSIS_NAME] = analysisName;
            var fiTempConfig = new FileInfo(Path.Combine(opDir, "temp.cfg"));
            ConfigDictionary.WriteConfgurationFile(config, fiTempConfig);

            LoggedConsole.WriteLine("\n\tPreparing sonogram of audio segment");
            FileInfo fiImage = new FileInfo(Path.Combine(opDir, Path.GetFileNameWithoutExtension(fiAudio.FullName) + ".png"));
            IAnalyser analyser = AudioBrowserTools.GetAcousticAnalyser(analysisName, this.pluginHelper.AnalysisPlugins);
            Image image = SonogramTools.GetImageFromAudioSegment(fiAudio, fiTempConfig, fiImage, analyser);
            return image;
        } //GetSonogram()

        private void SetConfigValue(Dictionary<string, string> config, string key, string value)
        {
            if (!config.ContainsKey(key)) config.Add(key, value);
            else config[key] = value;
        }

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

        private const int FilterLineNumControls = 7;
        private const int FilterLineOffsetTop = 29;

        private void btnAddSearchEntry_Click(object sender, EventArgs e)
        {
            // there are 7 controls to copy, and one just gets moved down


            var controlCount = this.panelSearchEntries.Controls.Count;
            var currentCount = controlCount / FilterLineNumControls;
            var amountToAddToTop = currentCount * FilterLineOffsetTop;

            // copy the search field line and set the location and event handlers
            TextBox tfSearchFieldMaxNew = new TextBox();
            this.panelSearchEntries.Controls.Add(tfSearchFieldMaxNew);

            TextBox tfSearchFieldMinNew = new TextBox();
            this.panelSearchEntries.Controls.Add(tfSearchFieldMinNew);

            Label lblSearchFieldMaxNew = new Label();
            this.panelSearchEntries.Controls.Add(lblSearchFieldMaxNew);

            Label lblSearchFieldNameNew = new Label();
            this.panelSearchEntries.Controls.Add(lblSearchFieldNameNew);

            Label lblSearchFieldMinNew = new Label();
            this.panelSearchEntries.Controls.Add(lblSearchFieldMinNew);

            TextBox tfSearchFieldNameNew = new TextBox();
            this.panelSearchEntries.Controls.Add(tfSearchFieldNameNew);

            Button btnSearchRemoveFilterLineNew = new Button();
            this.panelSearchEntries.Controls.Add(btnSearchRemoveFilterLineNew);


            // 
            // tfSearchFieldName
            // 
            tfSearchFieldNameNew.Location = new System.Drawing.Point(75, 12 + amountToAddToTop);
            tfSearchFieldNameNew.Name = "tfSearchFieldName" + amountToAddToTop;
            tfSearchFieldNameNew.Size = new System.Drawing.Size(212, 20);
            tfSearchFieldNameNew.TabIndex = 16;
            // 
            // lblSearchFieldMin
            // 
            lblSearchFieldMinNew.AutoSize = true;
            lblSearchFieldMinNew.Location = new System.Drawing.Point(293, 15 + amountToAddToTop);
            lblSearchFieldMinNew.Name = "lblSearchFieldMin" + amountToAddToTop;
            lblSearchFieldMinNew.Size = new System.Drawing.Size(51, 13);
            lblSearchFieldMinNew.TabIndex = 17;
            lblSearchFieldMinNew.Text = "Minimum:";
            // 
            // lblSearchFieldName
            // 
            lblSearchFieldNameNew.AutoSize = true;
            lblSearchFieldNameNew.Location = new System.Drawing.Point(6, 14 + amountToAddToTop);
            lblSearchFieldNameNew.Name = "lblSearchFieldName" + amountToAddToTop;
            lblSearchFieldNameNew.Size = new System.Drawing.Size(63, 13);
            lblSearchFieldNameNew.TabIndex = 18;
            lblSearchFieldNameNew.Text = "Field Name:";
            // 
            // lblSearchFieldMax
            // 
            lblSearchFieldMaxNew.AutoSize = true;
            lblSearchFieldMaxNew.Location = new System.Drawing.Point(444, 15 + amountToAddToTop);
            lblSearchFieldMaxNew.Name = "lblSearchFieldMax" + amountToAddToTop;
            lblSearchFieldMaxNew.Size = new System.Drawing.Size(54, 13);
            lblSearchFieldMaxNew.TabIndex = 19;
            lblSearchFieldMaxNew.Text = "Maximum:";
            // 
            // tfSearchFieldMin
            // 
            tfSearchFieldMinNew.Location = new System.Drawing.Point(350, 11 + amountToAddToTop);
            tfSearchFieldMinNew.MaxLength = 10;
            tfSearchFieldMinNew.Name = "tfSearchFieldMin" + amountToAddToTop;
            tfSearchFieldMinNew.Size = new System.Drawing.Size(88, 20);
            tfSearchFieldMinNew.TabIndex = 20;
            // 
            // tfSearchFieldMax
            // 
            tfSearchFieldMaxNew.Location = new System.Drawing.Point(504, 11 + amountToAddToTop);
            tfSearchFieldMaxNew.MaxLength = 10;
            tfSearchFieldMaxNew.Name = "tfSearchFieldMax" + amountToAddToTop;
            tfSearchFieldMaxNew.Size = new System.Drawing.Size(88, 20);
            tfSearchFieldMaxNew.TabIndex = 21;
            // 
            // btnSearchRemoveFilterLine
            // 
            btnSearchRemoveFilterLineNew.Location = new System.Drawing.Point(598, 10 + amountToAddToTop);
            btnSearchRemoveFilterLineNew.Name = "btnSearchRemoveFilterLine" + amountToAddToTop;
            btnSearchRemoveFilterLineNew.Size = new System.Drawing.Size(118, 23);
            btnSearchRemoveFilterLineNew.TabIndex = 22;
            btnSearchRemoveFilterLineNew.Text = "Remove This Filter";
            btnSearchRemoveFilterLineNew.UseVisualStyleBackColor = true;
            btnSearchRemoveFilterLineNew.Click += this.btnSearchRemoveFilterLine_Click;

            // move the add button down
            // add 29 top top, left stays the same

        }

        private void btnSearchRemoveFilterLine_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;

            if (btn != null)
            {
                var controlNameList = new List<string>
                    {
                        "btnSearchRemoveFilterLine",
                        "tfSearchFieldName",
                        "lblSearchFieldMin",
                        "lblSearchFieldName",
                        "lblSearchFieldMax",
                        "tfSearchFieldMin",
                        "tfSearchFieldMax"
                    };


                var suffix = int.Parse(btn.Name.Replace("btnSearchRemoveFilterLine", string.Empty));

                FindAndRemoveControlFromTabPageSearchCsv(controlNameList, suffix);
            }
        }

        private void FindAndRemoveControlFromTabPageSearchCsv(IEnumerable<string> controlNameList, int suffix)
        {
            foreach (var item in controlNameList)
            {
                // remove the control
                var controls = this.panelSearchEntries.Controls.Find(item + suffix, false);
                if (controls.Count() == 1)
                {
                    this.panelSearchEntries.Controls.Remove(controls.First());
                }

                // move all other controls of the same name (and high postfix) up by one place
                var changingPostfix = suffix;
                while ((controls = this.panelSearchEntries.Controls.Find(item + (changingPostfix + FilterLineOffsetTop), false)).Any())
                {
                    changingPostfix += FilterLineOffsetTop;
                    controls[0].Location = new Point(controls[0].Location.X, controls[0].Location.Y - FilterLineOffsetTop);
                }
            }
        }

        private void btnFindInCSV_Click(object sender, EventArgs e)
        {
            var filter = new CsvFileFilter();

            var topDir = new DirectoryInfo(textBoxCSVSourceFolderPath.Text);

            var allControls = this.panelSearchEntries.Controls.Cast<Control>();
            var textBoxes = allControls.Select(i => i as TextBox).Where(i => i != null);
            var suffixes =
                textBoxes.Select(
                    i =>
                    i.Name.Replace("tfSearchFieldName", string.Empty)
                     .Replace("tfSearchFieldMin", string.Empty)
                     .Replace("tfSearchFieldMax", string.Empty)).Distinct();

            var filters =
                suffixes.Select(
                    i =>
                    new CsvFileFilter.CsvFilter
                        {
                            FieldName = textBoxes.First(tf => tf.Name == "tfSearchFieldName" + i).Text,
                            Minimum = double.Parse(textBoxes.First(tf => tf.Name == "tfSearchFieldMin" + i).Text),
                            Maximum = double.Parse(textBoxes.First(tf => tf.Name == "tfSearchFieldMax" + i).Text)
                        })
                        .Where(i => !string.IsNullOrWhiteSpace(i.FieldName));


            var results = filter.Run(topDir, filters);

            foreach (var result in results)
            {
                LoggedConsole.WriteLine("Processed " + result.ProcessedFile.FullName);
                LoggedConsole.WriteLine("Headers: " + string.Join(", ", result.Headers));
                LoggedConsole.WriteLine("Using filters: " + string.Join(", ", result.Filters.Select(f => f.FieldName + " " + f.Minimum + "-" + f.Maximum)));
                //LoggedConsole.WriteLine("File Stats: " + string.Join(", ", result.ColumnStats.Select(i => i.Key + ": " + i.Value.ToString())));
                LoggedConsole.WriteLine(result.Rows.Count + " Rows Matched Filter");

                //filter.WriteCSV(new FileInfo(result.ProcessedFile.FullName + ".matched.csv"), result.Headers, result.Rows);
            }


        }


    } //class MainForm : Form
}
