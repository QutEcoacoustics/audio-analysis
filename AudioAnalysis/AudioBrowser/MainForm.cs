namespace AudioBrowser
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics; //for the StopWatch only
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

    using AnalysisPrograms;

    using AudioAnalysisTools;


    using log4net;

    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    //using System.IO;

    using TowseyLib;

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
        private double[] weightedIndices;
        private Bitmap selectionTrackImage;


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
                MessageBox.Show(ex.ToString());
            }

            //initialize analysis parameters
            try
            {
                analysisParams = FileTools.ReadPropertiesFile(browserSettings.fiAnalysisConfig.FullName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            /* // Just testing
            var master = new MasterAudioUtility();
            var file = @"I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\A French Fiddle Speaks.mp3";
            //var result = master.Info(new FileInfo(file));


            var spectrogramUtil = new CustomSpectrogramUtility(master);
            var fileImg = @"I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\A French Fiddle Speaks.jpg";

            spectrogramUtil.Create(
                new FileInfo(file), MediaTypes.MediaTypeMp3, new FileInfo(fileImg), MediaTypes.MediaTypeJpeg); 
            */

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
            this.audioUtilityForDurationColumn  = new MasterAudioUtility();

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

                 if(Directory.Exists(browserSettings.DefaultSourceDir.FullName))
                 {
                     this.UpdateSourceFileList();
                 }
            }

            if (browserSettings.DefaultOutputDir != null)
            {
                this.tfOutputDirectory.Text = browserSettings.DefaultOutputDir.FullName;

                if (Directory.Exists(browserSettings.DefaultOutputDir.FullName))
                {
                    this.UpdateOutputFileList();
                }
            }

            WriteBrowserParameters2Console(this.browserSettings);
            WriteAnalysisParameters2Console(this.analysisParams, this.browserSettings.AnalysisName);
            CheckForConsistencyOfAnalysisTypes(this.browserSettings, this.analysisParams);

        } //MainForm()

        private void WriteBrowserParameters2Console(AudioBrowserSettings parameters)
        {
            string title = "# AUDIO BROWSER FOR THE ANALYSIS AND INTERROGATION OF LARGE AUDIO FILES";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("#");
            Console.WriteLine("# Browser Settings:");
            Console.WriteLine("\tAnalysis Name: " + parameters.AnalysisName);
            Console.WriteLine("\tAnalysis Config File: " + parameters.fiAnalysisConfig.FullName);
            Console.WriteLine("\tSource Directory:     " + parameters.diSourceDir.FullName);
            Console.WriteLine("\tOutput Directory:     " + parameters.diOutputDir.FullName);
            Console.WriteLine("\tDisplay:  Track Count={0}. Track Height={1}pixels. Tracks normalised={2}.", parameters.TrackCount, parameters.TrackHeight, parameters.TrackNormalisedDisplay);
            Console.WriteLine("####################################################################################\n");
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

        private static bool CheckForConsistencyOfAnalysisTypes(AudioBrowserSettings settings, Dictionary<string, string> dict)
        {
            string analysisName = dict[AcousticIndices.key_ANALYSIS_NAME];
            if (!settings.AnalysisName.Equals(analysisName))
            {
                Console.WriteLine("WARNING: Analysis type selected in browser ({0}) not same as that in config file ({1})", settings.AnalysisName, analysisName);
                return false;
            }
            Console.WriteLine("Analysis type: " + settings.AnalysisName);
            return true;
        }



        private void btnExtractIndiciesAllSelected_Click(object sender, EventArgs e)
        {
            int count = 0;

            //this.textBoxConsole.Clear();
            this.tabControlMain.SelectTab("tabPageConsole");
            //string date = "# DATE AND TIME: " + DateTime.Now;
            //Console.WriteLine(date);
            //Console.WriteLine("# ACOUSTIC ENVIRONMENT BROWSER");

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
                    Console.WriteLine("# Start processing at: {0}",  DateTime.Now.ToLongTimeString());
                    
                    Stopwatch stopwatch = new Stopwatch(); //for checking the parallel loop.
                    stopwatch.Start();
                    //################# PROCESS THE RECORDING #####################################################################################
                    var outputData = this.ProcessRecording(fiSourceRecording, browserSettings.diOutputDir, this.analysisParams);
                    //#############################################################################################################################
                    stopwatch.Stop();

                    
                    string reportFileExt = ".csv";
                    string opDir = this.tfOutputDirectory.Text;
                    string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + browserSettings.AnalysisName;
                    string reportfilePath = null;

                    //unfortunately different things happen depending on the analysis!
                    if (browserSettings.AnalysisName.Equals(AcousticIndices.ANALYSIS_NAME)) //KiwiRecogniser
                    {
                        //the output from ACOUSTIC analysis is rows of one minute indices                        
                        reportfilePath = Path.Combine(opDir, fName + reportFileExt);
                        CsvTools.DataTable2CSV(outputData, reportfilePath);
                    }
                    else
                    if (browserSettings.AnalysisName.Equals(KiwiRecogniser.ANALYSIS_NAME)) //KiwiRecogniser - save two files
                    {
                        //the output from KIWI analysis is rows of kiwi events - must save two files
                        reportfilePath = Path.Combine(opDir, fName + "Events" + reportFileExt);
                        CsvTools.DataTable2CSV(outputData, reportfilePath);

                        reportfilePath = Path.Combine(opDir, fName + reportFileExt);
                        DataTable temporalDataTable = KiwiRecogniser.ConvertListOfKiwiEvents2TemporalList(outputData); //this compatible with temporal acoustic data
                        CsvTools.DataTable2CSV(temporalDataTable, reportfilePath);
                    }
                        else return;


                    string target = Path.Combine(opDir, fName + "_BACKUP" + reportFileExt);
                    File.Delete(target);               // Ensure that the target does not exist.
                    File.Copy(reportfilePath, target); // Copy the file 2 target

                    Console.WriteLine("###################################################");
                    Console.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
                    Console.WriteLine("Output  to  directory: " + opDir);
                    Console.WriteLine("CSV file is @ " + reportfilePath);

                    //Remaining LINES ARE FOR DIAGNOSTIC PURPOSES ONLY
                    TimeSpan ts = stopwatch.Elapsed;
                    Console.WriteLine("Processing time: {0:f3} seconds ({1}min {2}s)",    (stopwatch.ElapsedMilliseconds / (double)1000), ts.Minutes, ts.Seconds);
                    Console.WriteLine("Average time per unit of output: {0:f3} seconds.", (stopwatch.ElapsedMilliseconds / (double)1000 / (double)outputData.Rows.Count));

                    Console.WriteLine("###################################################\n");

                }// if checked
            } //foreach

            if (this.dataGridViewFileList.RowCount < 1 || count < 1)
            {
                MessageBox.Show("No file is selected.");
            }
        }

        private DataTable ProcessRecording(FileInfo fiSourceRecording, DirectoryInfo diOutputDir, Dictionary<string, string> dict)
        {
            string sourceRecordingPath = fiSourceRecording.FullName;
            string outputDir = diOutputDir.FullName;

            double segmentDuration_mins = Configuration.GetDouble(AudioBrowserSettings.key_SEGMENT_DURATION, dict); 
            int segmentOverlap          = Configuration.GetInt(AudioBrowserSettings.key_SEGMENT_OVERLAP, dict); 
            int resampleRate            = Configuration.GetInt(AudioBrowserSettings.key_RESAMPLE_RATE, dict); 

            // CREATE RUN ANALYSIS CLASS HERE

            // Set up the file and get info



            IAudioUtility audioUtility = new MasterAudioUtility(resampleRate); //creates AudioUtility and
            var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
            var sourceAudioDuration = audioUtility.Duration(fiSourceRecording, mimeType);
            int segmentCount = (int)Math.Round(sourceAudioDuration.TotalMinutes / segmentDuration_mins); //convert length to minute chunks
            int segmentDuration_ms = (int)(segmentDuration_mins * 60000) + (segmentOverlap * 1000);

            //Console.WriteLine("# Source audio - duration: {0}hr:{1}min:{2}s:{3}ms", sourceAudioDuration.Hours, sourceAudioDuration.Minutes, sourceAudioDuration.Seconds, sourceAudioDuration.Milliseconds);
            //Console.WriteLine("# Source audio - duration: {0:f4} minutes", sourceAudioDuration.TotalMinutes);
            Console.WriteLine("# Source audio - segments: {0}", segmentCount);

            //var outputData = new List<string>(); //List to store indices
            //SET UP THE OUTPUT REPORT DATATABLE
            DataTable outputDataTable = null; 
            if (browserSettings.AnalysisName.Equals(AcousticIndices.ANALYSIS_NAME)) //EXTRACT ACOUSTIC INDICES
            {
                var parameters = AcousticIndices.InitOutputTableColumns();
                outputDataTable = DataTableTools.CreateTable(parameters.Item1, parameters.Item2);
            }
            else
                if (browserSettings.AnalysisName.Equals(KiwiRecogniser.ANALYSIS_NAME)) //KiwiRecogniser
                {
                    var parameters = KiwiRecogniser.InitOutputTableColumns();
                    outputDataTable = DataTableTools.CreateTable(parameters.Item1, parameters.Item2);
                }
                else
                {
                    return null;
                }


            // LOOP THROUGH THE FILE
            //initialise timers for diagnostics - ONLY IF IN SEQUENTIAL MODE
            //DateTime tStart = DateTime.Now;
            //DateTime tPrevious = tStart;
            
            //segmentCount = 30;   //for testing and debugging

            //for (int s = 0; s < segmentCount; s++)
            // Parallelize the loop to partition the source file by segments.
            //Parallel.For(0, 570, s =>              //USE FOR FIRST HALF OF RECORDING
            //Parallel.For(569, segmentCount, s =>   //USE FOR SECOND HALF OF RECORDING
            //Parallel.For(847, 848, s =>
            Parallel.For(0, segmentCount, s =>
            {
                //Console.WriteLine(string.Format("Worker threads in use: {0}", GetThreadsInUse()));
                double startMinutes = s * segmentDuration_mins;
                int startMilliseconds = (int)(startMinutes * 60000);
                int endMilliseconds = startMilliseconds + segmentDuration_ms;

                #region time diagnostics - used only in sequential loop - no use for parallel loop
                //DateTime tNow = DateTime.Now;
                //TimeSpan elapsedTime = tNow - tStart;
                //string timeDuration = DataTools.Time_ConvertSecs2Mins(elapsedTime.TotalSeconds);
                //double avIterTime = elapsedTime.TotalSeconds / s;
                //if (s == 0) avIterTime = 0.0; //correct for division by zero
                //double t2End = avIterTime * (segmentCount - s) / (double)60;
                //TimeSpan iterTimeSpan = tNow - tPrevious;
                //double iterTime = iterTimeSpan.TotalSeconds;
                //if (s == 0) iterTime = 0.0;
                //tPrevious = tNow;
                //Console.WriteLine("\n");
                //Console.WriteLine("## SEQUENTIAL SAMPLE {0}:  Starts@{1} min.  Elpased time:{2:f1}    E[t2End]:{3:f1} min.   Sec/iteration:{4:f2} (av={5:f2})",
                //                           s, startMinutes, timeDuration, t2End, iterTime, avIterTime);
                #endregion


                //AudioRecording.GetSegmentFromAudioRecording(sourceRecordingPath, startMilliseconds, endMilliseconds, parameters.resampleRate, outputSegmentPath);

                //set up the temporary audio segment output file
                string tempFname = "temp"+s+".wav";
                string tempSegmentPath = Path.Combine(outputDir, tempFname); //path name of the temporary segment files extracted from long recording
                FileInfo fiOutputSegment = new FileInfo(tempSegmentPath);
                MasterAudioUtility.Segment(resampleRate, fiSourceRecording, fiOutputSegment, startMilliseconds, endMilliseconds);
                AudioRecording recordingSegment = new AudioRecording(fiOutputSegment.FullName);
                FileInfo fiSegmentAudioFile = new FileInfo(recordingSegment.FilePath);

                //double check that recording is over minimum length
                double wavSegmentDuration = recordingSegment.GetWavReader().Time.TotalSeconds;
                int sampleCount = recordingSegment.GetWavReader().Samples.Length; //get recording length to determine if long enough
                int minimumDuration = 30; //seconds
                int minimumSamples = minimumDuration * resampleRate; //ignore recordings shorter than 100 frame
                if (sampleCount <= minimumSamples)
                {
                    Console.WriteLine("# WARNING: Segment @{0}minutes is only {1} samples long (i.e. less than {2} seconds). Will ignore.", startMinutes, sampleCount, minimumDuration);
                    //break;
                }
                else //do analysis
                {
                    //#############################################################################################################################################
                    //##### DO THE ANALYSIS ############ 
                    DataTable dt = null;
                    if (browserSettings.AnalysisName.Equals(AcousticIndices.ANALYSIS_NAME)) //ACOUSTIC INDICES
                    {
                        dt = AcousticIndices.Analysis(s, fiSegmentAudioFile, dict);
                    }
                    else
                    if (browserSettings.AnalysisName.Equals(KiwiRecogniser.ANALYSIS_NAME)) //Little Spotted Kiwi
                    {
                        dt = KiwiRecogniser.Analysis(s, fiSegmentAudioFile, dict, diOutputDir);
                        Log.WriteLine("# Event count for minute {0} = {1}", startMinutes, dt.Rows.Count);
                    }

                    if (dt != null)
                    {
                        foreach (DataRow row in dt.Rows) outputDataTable.ImportRow(row);
                    }
                    //#############################################################################################################################################
                }

                recordingSegment.Dispose();
                File.Delete(tempSegmentPath); //deleted the temp file
                startMinutes += segmentDuration_mins;
            } //end of for loop
            ); // Parallel.For

            return outputDataTable;
        }




        private void btnLoadVisualIndexAllSelected_Click(object sender, EventArgs e)
        {
            int count = 0;

            //USE FOLLOWING LINES TO LOAD A PNG IMAGE
            //visualIndex.Image = new Bitmap(parameters.visualIndexPath);

            this.textBoxConsole.Clear();

            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(date);
            Console.WriteLine("# ACOUSTIC FILE BROWSER");

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

                    Console.WriteLine("# Display acoustic indices from csv file: " + csvFileName);
                    //Infer source file name from CSV file name
                    FileInfo inferredSourceFile = InferSourceFileFromCSVFileName(csvFileName);
                    if (inferredSourceFile == null)
                    {
                        browserSettings.fiSourceRecording = null;
                        Console.WriteLine("# \tWARNING: Cannot find mp3 or wav source recording with name: " + Path.GetFileNameWithoutExtension(csvFileName));
                    }
                    else
                    {
                        browserSettings.fiSourceRecording = inferredSourceFile;
                        Console.WriteLine("# \t\tExpected source recording: " + inferredSourceFile.Name);
                    }

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
                        this.labelSourceFileName.Text = Path.GetFileNameWithoutExtension(csvFileName);
                        this.labelSourceFileDurationInMinutes.Text = "File duration = " + this.sourceRecording_MinutesDuration + " minutes";
                        this.tabControlMain.SelectTab("tabPageDisplay");
                    }
                } // if (isChecked)
            } //for each row in dataGridCSVfiles
            //settings.fiCSVFile = new FileInfo();

            if (this.dataGridCSVfiles.RowCount < 1 || count < 1)
            {
                MessageBox.Show("No CSV file is selected.");
            }
        }

        private FileInfo InferSourceFileFromCSVFileName(string csvFileName)
        {
            string csvFname = Path.GetFileNameWithoutExtension(csvFileName);
            string[] parts = csvFname.Split('_'); //assume that underscore plus analysis type has been added to source name
            string sourceName = parts[0];
            for (int i = 1; i < parts.Length - 1; i++) sourceName += ("_"+parts[i]);

            var fiSourceMP3 = new FileInfo(Path.Combine(this.browserSettings.diSourceDir.FullName, sourceName + ".mp3"));
            var fiSourceWAV = new FileInfo(Path.Combine(this.browserSettings.diSourceDir.FullName, sourceName + ".wav"));
            if (fiSourceMP3.Exists) return fiSourceMP3;
            else
                if (fiSourceWAV.Exists) return fiSourceWAV;
                else
            return null;
        }

        /// <summary>
        /// loads a csv file of indices
        /// returns a status integer. 0= no error
        /// </summary>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        private int LoadIndicesCSVFile(string csvPath)
        {
            DataTable dt          = null;
            double[] colouredArray = null; //array to be displayed in colour
            //########################

            if (browserSettings.AnalysisName.Equals(AcousticIndices.ANALYSIS_NAME))
            {
                var output = AcousticIndices.ProcessCsvFile(new FileInfo(csvPath));
                dt = output.Item1;
                colouredArray = output.Item2;
            }
            else
            if (browserSettings.AnalysisName.Equals(KiwiRecogniser.ANALYSIS_NAME))
            {
                var output = KiwiRecogniser.ProcessCsvFile(new FileInfo(csvPath));
                dt = output.Item1;
                colouredArray = output.Item2;
            }
            else
            {
                Console.WriteLine("\nWARNING: Could not construct image from CSV file.");
                Console.WriteLine("\t Browser analysis name not recognized: " + browserSettings.AnalysisName);
                return 3;
            }

            this.weightedIndices = colouredArray;
            Bitmap tracksImage = ConstructVisualIndexImage(dt, browserSettings.TrackHeight, browserSettings.TrackNormalisedDisplay);
            this.sourceRecording_MinutesDuration = dt.Rows.Count; //CAUTION: assume one value per minute - //set global variable !!!!

            //###################### MAKE VISUAL ADJUSTMENTS FOR HEIGHT OF THE VISUAL INDEX IMAGE  - THIS DEPENDS ON NUMBER OF TRACKS 
            this.pictureBoxVisualIndex.Height = tracksImage.Height;
            this.pictureBoxVisualIndex.Image = tracksImage;
            this.pictureBoxBarTrack.Location = new Point(0, tracksImage.Height + 1);
            this.selectionTrackImage = new Bitmap(this.pictureBoxBarTrack.Width, this.pictureBoxBarTrack.Height);
            
            //SAVE THE IMAGE
            string imagePath = Path.Combine(browserSettings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(csvPath) + ".png"));
            this.pictureBoxVisualIndex.Image.Save(imagePath);
            Console.WriteLine("\n\tSaved csv data tracks to image file: " + imagePath);

            int error = 0;
            return error;
        }




        /// <summary>
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="values"></param>
        /// <param name="imageWidth"></param>
        /// <param name="trackHeight"></param>
        /// <returns></returns>
        public static Bitmap ConstructVisualIndexImage(DataTable dt, double[] order, int trackHeight, bool normalisedTrackDisplay)
        {
            List<string> headers = (from DataColumn col in dt.Columns select col.ColumnName).ToList();
            List<double[]> values = DataTableTools.ListOfColumnValues(dt);

            // accumulate the indivudal tracks
            var bitmaps = new List<Bitmap>();
            double threshold = 0.0;
            for (int i = 0; i < values.Count - 1; i++) //for pixels in the line
            {
                bitmaps.Add(Image_Track.DrawBarScoreTrack(order, values[i], trackHeight, threshold, headers[i]));
            }
            int x = values.Count - 1;
            bitmaps.Add(Image_Track.DrawColourScoreTrack(order, values[x], trackHeight, threshold, headers[x])); //assumed to be weighted index

            //set up the composite image parameters
            int trackCount = values.Count + 2; //+2 for top and bottom time tracks
            int imageHt = trackHeight * trackCount;
            int duration = values[0].Length; //time in minutes
            int endPanelwidth = 100;
            int imageWidth = duration + endPanelwidth;
            int offset = 0;
            int scale = 60; //put a tik every 60 pixels = 1 hour
            Bitmap timeBmp = Image_Track.DrawTimeTrack(duration, scale, imageWidth, trackHeight, "Time (hours)");

            //draw the composite bitmap
            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            var font = new Font("Arial", 10.0f, FontStyle.Regular);
            offset += trackHeight;
            for (int i = 0; i < values.Count; i++) //for pixels in the line
            {
                gr.DrawImage(bitmaps[i], 0, offset);
                gr.DrawString(headers[i], font, Brushes.White, new PointF(duration + 5, offset));
                offset += trackHeight;
            }
            gr.DrawImage(timeBmp, 0, offset); //draw in bottom time scale
            return compositeBmp;
        }

        /// <summary>
        /// assumes the passed data arrays are in correct order for visualization
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="trackHeight"></param>
        /// <param name="normalisedTrackDisplay"></param>
        /// <returns></returns>
        public static Bitmap ConstructVisualIndexImage(DataTable dt, int trackHeight, bool normalisedTrackDisplay)
        {
            int length = dt.Rows.Count;
            double[] order = new double[length];
            for (int i = 0; i < length; i++) order[i] = i;
            return ConstructVisualIndexImage(dt, order, trackHeight, normalisedTrackDisplay);
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
            Point pt1 = new Point(myX, 2);
            Point pt2 = new Point(myX, this.pictureBoxVisualIndex.Height - browserSettings.TrackHeight);
            float[] dashValues = { 2, 2, 2, 2 };
            Pen pen = new Pen(Color.Green, 1.0F);
            pen.DashPattern = dashValues;
            g.DrawLine(pen, pt1, pt2);

            if (myX >= this.sourceRecording_MinutesDuration - 1)
                this.textBoxCursorValue.Text = String.Format("{0:f2} <<{1:f2}>> {2:f2}", this.weightedIndices[myX - 1], this.weightedIndices[myX], "END");
            else
                if (myX <= 0)
                    this.textBoxCursorValue.Text = String.Format("{0:f2} <<{1:f2}>> {2:f2}", "START", this.weightedIndices[myX], this.weightedIndices[myX + 1]);
                else
                    this.textBoxCursorValue.Text = String.Format("{0:f2} <<{1:f2}>> {2:f2}", this.weightedIndices[myX - 1], this.weightedIndices[myX], this.weightedIndices[myX + 1]);
        }

        private void pictureBoxVisualIndex_MouseClick(object sender, MouseEventArgs e)
        {
            this.textBoxConsole.Clear();
            this.tabControlMain.SelectTab("tabPageConsole");
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(date);
            Console.WriteLine("# ACOUSTIC ENVIRONMENT BROWSER");

            if (browserSettings.fiSourceRecording == null)
            {
                Console.WriteLine("    The source file has not been set.");
                Console.WriteLine("    Cannot proceed with display of segment sonogram.");
                return;
            }
            if (! browserSettings.fiSourceRecording.Exists)
            {
                Console.WriteLine("    The source file does not exist: <{0}>", browserSettings.fiSourceRecording.FullName);
                Console.WriteLine("    Cannot proceed with display of segment sonogram.");
                return;
            }



            // GET MOUSE LOCATION
            int myX = e.X;
            int myY = e.Y;

            //DRAW RED LINE ON BAR TRACK
            for (int y = 0; y < selectionTrackImage.Height; y++)
                selectionTrackImage.SetPixel(this.pictureBoxVisualIndex.Left + myX, y, Color.Red);
            this.pictureBoxBarTrack.Image = selectionTrackImage;

            //EXTRACT RECORDING SEGMENT
            double segmentDuration = Configuration.GetDouble(AudioBrowserSettings.key_SEGMENT_DURATION, analysisParams);
            int startMilliseconds = (myX) * 60000;
            int endMilliseconds = (myX + 1) * 60000;
            if (segmentDuration == 3)
            {
                startMilliseconds = (myX - 1) * 60000;
                endMilliseconds = (myX + 2) * 60000;
            }
            if (startMilliseconds < 0) startMilliseconds = 0;

            string sourceFName = Path.GetFileNameWithoutExtension(browserSettings.fiSourceRecording.FullName);
            string segmentFName = sourceFName + "_min" + myX.ToString() + ".wav"; //want a wav file

            string outputSegmentPath = Path.Combine(browserSettings.diOutputDir.FullName, segmentFName); //path name of the segment file extracted from long recording
            FileInfo fiOutputSegment = new FileInfo(outputSegmentPath);


            Console.WriteLine("\n\tExtracting audio segment from source audio: minute " + myX + " to minute " + (myX + 1));
            Console.WriteLine("\n\tWriting audio segment to dir: " + browserSettings.diOutputDir.FullName);
            Console.WriteLine("\n\t\t\tFile Name: " + segmentFName);

            //get segment from source recording
            DateTime time1 = DateTime.Now;
            int resampleRate = Configuration.GetInt(AudioBrowserSettings.key_RESAMPLE_RATE, analysisParams);
            //AudioRecording recording = AudioRecording.GetSegmentFromAudioRecording(settings.fiSourceRecording.FullName, startMilliseconds, endMilliseconds, settings.ResampleRate, outputSegmentPath);
            MasterAudioUtility.Segment(resampleRate, browserSettings.fiSourceRecording, fiOutputSegment, startMilliseconds, endMilliseconds);
            AudioRecording recordingSegment = new AudioRecording(fiOutputSegment.FullName);

            DateTime time2 = DateTime.Now;
            TimeSpan timeSpan = time2 - time1;
            Console.WriteLine("\n\t\t\tExtraction time: " + timeSpan.TotalSeconds + " seconds");

            //store info
            this.labelSonogramFileName.Text = Path.GetFileName(recordingSegment.FilePath);
            browserSettings.fiSegmentRecording = new FileInfo(recordingSegment.FilePath);


            //make the sonogram
            Image_MultiTrack image = MakeSonogram(recordingSegment);
            this.pictureBoxSonogram.Image = image.GetImage();


            //this.hScrollBarSonogram.Location = new System.Drawing.Point(0, this.pictureBoxSonogram.Image.Height);
            //this.hScrollBarSonogram.Minimum = 0;
            //this.hScrollBarSonogram.Maximum = pictureBoxSonogram.Width - this.panelSonogram.Width + 280; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            string sonogramPath = Path.Combine(browserSettings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(segmentFName) + ".png"));
            Console.WriteLine("\n\tSaved sonogram to image file: " + sonogramPath);
            pictureBoxSonogram.Image.Save(sonogramPath);
            this.tabControlMain.SelectTab("tabPageDisplay");   
            this.labelSonogramFileName.Text = Path.GetFileName(segmentFName);
            
        }

        private Image_MultiTrack MakeSonogram(AudioRecording recordingSegment)
        {

            Console.WriteLine("\n\tPreparing sonogram of audio segment");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName   = recordingSegment.FileName;
            sonoConfig.WindowSize    = Configuration.GetInt(AudioBrowserSettings.key_FRAME_LENGTH, analysisParams);
            sonoConfig.WindowOverlap = Configuration.GetDouble(AudioBrowserSettings.key_FRAME_OVERLAP, analysisParams);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recordingSegment.GetWavReader());

            // (iii) NOISE REDUCTION
            if (this.checkBoxSonnogramNoiseReduce.Checked)
            {
                Console.WriteLine("NOISE REDUCTION");
                var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, browserSettings.SonogramBackgroundThreshold);
                sonogram.Data = tuple.Item1;   // store data matrix
            }

            //prepare the image
            //;
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            //using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            //using (image = new Image_MultiTrack(img))
            //{
            //    if (pictureBoxSonogram.Image != null) pictureBoxSonogram.Image.Dispose(); //get rid of previous sonogram
            //    //add time scale
            //    image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));

            //    if (this.checkBoxSonogramAnnotate.Checked) 
            //    {
            //          Console.WriteLine("ANNOTATE SONOGRAM");
            //    }
            //}
            System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            Image_MultiTrack image = new Image_MultiTrack(img);
            //{
               // if (pictureBoxSonogram.Image != null) pictureBoxSonogram.Image.Dispose(); //get rid of previous sonogram
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));

                if (this.checkBoxSonogramAnnotate.Checked)
                {
                    Console.WriteLine("ANNOTATION OF SONOGRAMS IS NOT YET IMPLEMENTED.");  //NOT YET IMPLEMENTED!
                }
            //}

            return image;
        }//MakeSonogram

        private void buttonRunAudacity_Click(object sender, EventArgs e)
        {
            if ((browserSettings.fiSegmentRecording == null) || (!browserSettings.fiSegmentRecording.Exists))
            {
                Console.WriteLine("Audacity cannot open audio segment file: <" + browserSettings.fiSegmentRecording+">");
                Console.WriteLine("It does not exist!");
                this.tabControlMain.SelectTab("tabPageConsole");
                RunAudacity(browserSettings.AudacityExe.FullName, " ", browserSettings.diOutputDir.FullName);
            }
            else
                RunAudacity(browserSettings.AudacityExe.FullName, browserSettings.fiSegmentRecording.FullName, browserSettings.diOutputDir.FullName);
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

        private static int GetThreadsInUse()
        {
            int availableWorkerThreads, availableCompletionPortThreads, maxWorkerThreads, maxCompletionPortThreads;
            ThreadPool.GetAvailableThreads(out  availableWorkerThreads, out availableCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            int threadsInUse = maxWorkerThreads - availableWorkerThreads;

            return threadsInUse;
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
            if (e.KeyCode == Keys.Space)
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
            if (e.KeyCode == Keys.Space)
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

        private void panelDisplayVisual_Paint(object sender, PaintEventArgs e)
        {

        }

        public static void RunAudacity(string audacityPath, string recordingPath, string dir)
        {
            ProcessRunner process = new ProcessRunner(audacityPath);
            process.Run(recordingPath, dir);
        }// RunAudacity()

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
            if ((browserSettings.fiSegmentRecording == null) || (!browserSettings.fiSegmentRecording.Exists)) return;
            //IAudioUtility audioUtility = new MasterAudioUtility(settings.ResampleRate); //creates AudioUtility and
            AudioRecording recordingSegment = new AudioRecording(browserSettings.fiSegmentRecording.FullName);
            Image_MultiTrack image = MakeSonogram(recordingSegment);
            this.pictureBoxSonogram.Image = image.GetImage();
        }

        private void backgroundWorkerUpdateSourceFileList_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void backgroundWorkerUpdateCSVFileList_DoWork(object sender, DoWorkEventArgs e)
        {

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

    } //class MainForm : Form
}
