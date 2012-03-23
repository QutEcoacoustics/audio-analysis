namespace AudioBrowser
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    //using System.Collections.Concurrent;
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

        private AudioBrowserSettings settings;

        // for calculating a visual index image
        private int sourceRecording_MinutesDuration = 0; //width of the index imageTracks = mintes duration of source recording.
        private double[] weightedIndices;
        private Bitmap visualIndexTimeScale;
        private Bitmap barTrackImage;
        //private FileInfo fiCurrentWaveSegment;

        public MainForm()
        {
            // must be here, must be first
            InitializeComponent();
            //initialize instance of AudioBrowserSettings clsas
            settings = new AudioBrowserSettings();
            try
            {
                settings.LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


            //Add the CheckBox into the source file list datagridview
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


            // only for debugging
            this.tfSourceDirectory.Text = settings.DefaultSourceDir.FullName;
            this.tfOutputDirectory.Text = settings.DefaultOutputDir.FullName;
        }

        public static void WriteExtractionParameters2Console(AudioBrowserSettings parameters)
        {
            Console.WriteLine("# Parameter Settings for Extraction of Indices from long Audio File:");
            Console.WriteLine("\tSegment size: Duration = {0} minutes.", parameters.SegmentDuration);
            Console.WriteLine("\tResample rate: {0} samples/sec.  Nyquist: {1} Hz.", parameters.ResampleRate, (parameters.ResampleRate / 2));
            Console.WriteLine("\tFrame Length: {0} samples.", parameters.FrameLength);
            Console.WriteLine("\tLow frequency Band: 0 Hz - {0} Hz.", parameters.LowFreqBound);
            Console.WriteLine("####################################################################################");
        }

        public void WriteDisplayParameters2Console(AudioBrowserSettings parameters)
        {
            Console.WriteLine("# Parameter Settings for Display of Indices and Sonograms:");
            Console.WriteLine("\tSonogram size: Duration = {0} minutes.", parameters.SegmentDuration);
            Console.WriteLine("\tResample rate: {0} samples/sec.  Nyquist: {1} Hz.", parameters.ResampleRate, (parameters.ResampleRate / 2));
            Console.WriteLine("\tFrame Length: {0} samples.  Fractional overlap: {1}.", parameters.FrameLength, parameters.FrameOverlap);
            Console.WriteLine("####################################################################################");
        }


        private void btnExtractIndiciesAllSelected_Click(object sender, EventArgs e)
        {
            int count = 0;

            this.textBoxConsole.Clear();
            this.tabControlMain.SelectTab("tabPageConsole");
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(date);
            Console.WriteLine("# ACOUSTIC ENVIRONMENT BROWSER");

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
                    settings.fiSourceRecording = fiSourceRecording;
                    Console.WriteLine("# Source audio - filename: " + Path.GetFileName(fiSourceRecording.Name));
                    Console.WriteLine("# Source audio - datetime: {0}    {1}", fiSourceRecording.CreationTime.ToLongDateString(), fiSourceRecording.CreationTime.ToLongTimeString());
                    //WriteExtractionParameters2Console(settings);
                    Console.WriteLine("# Start processing at: {0}",  DateTime.Now.ToLongTimeString());


                    Stopwatch stopwatch = new Stopwatch(); //for checking the parallel loop.
                    stopwatch.Start();
                    //################# PROCESS THE RECORDING #####################################################################################
                    var outputData = this.ProcessRecording(fiSourceRecording, settings);
                    //#############################################################################################################################

                    //List<string> list = op.Item1;
                    string reportFileExt = ".csv";
                    string reportSeparator = "CSV";
                    string header = AcousticIndices.FormatHeader(reportSeparator);
                    outputData.Insert(0, header); //put header at top of list

                    //var fiOutputCSVFile =
                    //    new FileInfo(
                    //        Path.Combine(
                    //            this.settings.OutputDir.FullName,
                    //            Path.GetFileNameWithoutExtension(audioFileName) + reportFileExt));
                    //string opDir = this.settings.OutputDir.FullName;
                    string opDir = this.tfOutputDirectory.Text;
                    string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + settings.AnalysisName;
                    string reportfilePath = Path.Combine(opDir, fName + reportFileExt);
                    FileTools.WriteTextFile(reportfilePath, outputData);

                    string target = Path.Combine(opDir, fName + "_BACKUP" + reportFileExt);
                    File.Delete(target);                        // Ensure that the target does not exist.
                    File.Copy(reportfilePath, target); //copy the file 2 target

                    Console.WriteLine("###################################################");
                    Console.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
                    Console.WriteLine("Output  to  directory: " + opDir);
                    Console.WriteLine("CSV file is @ " + reportfilePath);
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    Console.WriteLine("Parallel loop time: {0:f3} seconds ({1}min {2}s)", (stopwatch.ElapsedMilliseconds / (double)1000), ts.Minutes, ts.Seconds);

                    //NEXT TWO LINES ARE FOR DIAGNOSTIC PURPOSES ONLY
                    int iterationCount = 570;
                    Console.WriteLine(" Average iteration: {0:f3} seconds.", (stopwatch.ElapsedMilliseconds / (double)1000 / (double)iterationCount));

                    Console.WriteLine("###################################################\n");

                }// if checked
            } //foreach

            if (this.dataGridViewFileList.RowCount < 1 || count < 1)
            {
                MessageBox.Show("No file is selected.");
            }
        }

        private List<string> ProcessRecording(FileInfo fiSourceRecording, AudioBrowserSettings config)
        {
            string sourceRecordingPath = fiSourceRecording.FullName;
            string outputDir = config.diOutputDir.FullName;
            double segmentDuration_mins = config.SegmentDuration; 
            int segmentOverlap = config.SegmentOverlap;
            int resampleRate = config.ResampleRate;
            int frameLength = config.FrameLength; 
            int lowFreqBound = config.LowFreqBound;

            // CREATE RUN ANALYSIS CLASS HERE

            // Set up the file and get info



            IAudioUtility audioUtility = MasterAudioUtility.Create(resampleRate); //creates AudioUtility and
            var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
            var sourceAudioDuration = audioUtility.Duration(fiSourceRecording, mimeType);
            int segmentCount = (int)Math.Round(sourceAudioDuration.TotalMinutes / segmentDuration_mins); //convert length to minute chunks
            int segmentDuration_ms = (int)(segmentDuration_mins * 60000) + (segmentOverlap * 1000);

            //Console.WriteLine("# Source audio - duration: {0}hr:{1}min:{2}s:{3}ms", sourceAudioDuration.Hours, sourceAudioDuration.Minutes, sourceAudioDuration.Seconds, sourceAudioDuration.Milliseconds);
            //Console.WriteLine("# Source audio - duration: {0:f4} minutes", sourceAudioDuration.TotalMinutes);
            Console.WriteLine("# Source audio - segments: {0}", segmentCount);

            var outputData = new List<string>(); //List to store indices

            // LOOP THROUGH THE FILE
            //initialse timers for diagnostics
            //DateTime tStart = DateTime.Now;
            //DateTime tPrevious = tStart;
            
            //segmentCount = 30;   //for testing and debugging

            //for (int s = 0; s < segmentCount; s++)
            // Parallelize the loop to partition the source file by segments.
            //Parallel.For(0, 570, s =>              //USE FOR FIRST HALF OF RECORDING
            //Parallel.For(569, segmentCount, s =>   //USE FOR SECOND HALF OF RECORDING
            //Parallel.For(420, 421, s =>
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

                //double check that recording is over minimum length
                double wavSegmentDuration = recordingSegment.GetWavReader().Time.TotalSeconds;
                int sampleCount = recordingSegment.GetWavReader().Samples.Length; //get recording length to determine if long enough
                int minimumLength = 100 * frameLength; //ignore recordings shorter than 100 frames
                if (sampleCount <= minimumLength)
                {
                    Console.WriteLine("# WARNING: Recording is only {0} samples long (i.e. less than three frames). Will ignore.", sampleCount);
                    //break;
                }
                else
                {
                    //#############################################################################################################################################
                    //##### DO THE ANALYSIS ############ 
                    if (settings.AnalysisName.Equals(AcousticIndices.ANALYSIS_NAME)) //EXTRACT ACOUSTIC INDICES
                    {
                        var results = AcousticIndices.ExtractIndices(recordingSegment, frameLength, lowFreqBound);
                        AcousticIndices.Indices2 indices = results.Item1;
                        string line = AcousticIndices.FormatOneLineOfIndices("CSV", s, startMinutes, wavSegmentDuration, indices); //Store indices in CSV FORMAAT
                        outputData.Add(line);
                    }
                    else
                    if (settings.AnalysisName.Equals(KiwiRecogniser.ANALYSIS_NAME)) //KiwiRecogniser
                    {
                        //KiwiParams config
                        //var results = KiwiRecogniser.Analysis(config, recordingSegment);
                        //KiwiRecogniser.Indices2 indices = results.Item1;
                        //string line = KiwiRecogniser.FormatOneLineOfIndices("CSV", s, startMinutes, wavSegmentDuration, indices); //Store indices in CSV FORMAAT
                        //outputData.Add(line);
                    }

                    //#############################################################################################################################################
                }

                recordingSegment.Dispose();
                File.Delete(tempSegmentPath); //deleted the temp file
                startMinutes += segmentDuration_mins;
            } //end of for loop
            ); // Parallel.For

            return outputData;
        }




        private void btnLoadVisualIndexAllSelected_Click(object sender, EventArgs e)
        {
            int count = 0;

            //USE FOLLOWING LINES TO LOAD A PNG IMAGE
            //visualIndex.Image = new Bitmap(parameters.visualIndexPath);

            this.textBoxConsole.Clear();

            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(date);
            Console.WriteLine("# ACOUSTIC ENVIRONMENT BROWSER");

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
                            Path.Combine(this.settings.diOutputDir.FullName, csvFileName));

                    //get source file name = first part of CSV file name
                    string csvFname = Path.GetFileNameWithoutExtension(csvFileName);
                    string[] sourceParts = csvFname.Split('_');
                    var sourceFilePath =
                        new FileInfo(
                            Path.Combine(this.settings.diSourceDir.FullName, sourceParts[0] + settings.SourceFileExt));
                    settings.fiSourceRecording = sourceFilePath;

                    Console.WriteLine("# Display acoustic indices from csv file: " + csvFileName);
                    Console.WriteLine("# \t\tExpected source recording: " + sourceFilePath.Name);

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

        /// <summary>
        /// loads a csv file of indices
        /// returns a status integer. 0= no error
        /// </summary>
        /// <param name="csvPath"></param>
        /// <returns></returns>
        private int LoadIndicesCSVFile(string csvPath)
        {
            int error = 0;
            //USE FOLLOWING LINES TO LOAD A CSV FILE
            var tuple = FileTools.ReadCSVFile(csvPath);
            var headers = tuple.Item1;  //List<string>
            var values = tuple.Item2;  //List<double[]>> 

            if (values == null) return 1;
            if (values[0] == null) return 1;
            if (values.Count == 0) return 1;
            if (values[0].Length == 0) return 2;

            //set global variable !!!!
            this.sourceRecording_MinutesDuration = values[0].Length;

            //reconstruct new list of values to display
            var displayValues = new List<double[]>(); //reconstruct new list of values to display
            var displayHeaders = new List<string>();   //reconstruct new list of headers to display
            for (int i = 0; i < AcousticIndices.displayColumn.Length; i++)
            {
                if (AcousticIndices.displayColumn[i])
                {
                    displayValues.Add(values[i]);
                    displayHeaders.Add(headers[i]);
                }
            }

            //RECONSTRUCT NEW LIST OF VALUES to CALCULATE WEIGHTED COMBINATION INDEX
            var comboHeaders = new List<string>();          //reconstruct new list of headers used to calculate weighted index
            var weightedComboValues = new List<double[]>(); //reconstruct new list of values to calculate weighted combination index
            for (int i = 0; i < AcousticIndices.weightedIndexColumn.Length; i++)
            {
                if (AcousticIndices.weightedIndexColumn[i])
                {
                    double[] norm = DataTools.NormaliseArea(values[i]);
                    weightedComboValues.Add(norm);
                    comboHeaders.Add(headers[i]);
                }
            }
            this.weightedIndices = DataTools.GetWeightedCombinationOfColumns(weightedComboValues, AcousticIndices.comboWeights);
            this.weightedIndices = DataTools.normalise(this.weightedIndices);

            //add in weighted bias for chorus and backgorund noise
            //for (int i = 0; i < wtIndices.Length; i++)
            //{
            //if((i>=290) && (i<=470)) wtIndices[i] *= 1.1;  //morning chorus bias
            //background noise bias
            //if (bg_dB[i - 1] > -35.0) wtIndices[i] *= 0.8;
            //else
            //if (bg_dB[i - 1] > -30.0) wtIndices[i] *= 0.6;
            //}

            displayHeaders.Add("Weighted Index");
            displayValues.Add(weightedIndices);

            var output = AcousticIndices.ConstructVisualIndexImage(displayHeaders, displayValues, values[0], settings.TrackHeight, settings.TrackNormalisedDisplay); //values[0] is the order of rows in CSV file
            this.pictureBoxVisualIndex.Image = output.Item1;
            this.visualIndexTimeScale = output.Item2;//store the time scale because want the image later for refreshing purposes
            this.weightedIndices = DataTools.Order(this.weightedIndices, values[0]); //reorder the weighted indices: 0->N

            this.barTrackImage = new Bitmap(this.pictureBoxBarTrack.Width, this.pictureBoxBarTrack.Height);
            
            //SAVE THE IMAGE
            string imagePath = Path.Combine(settings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(csvPath) + ".png"));
            this.pictureBoxVisualIndex.Image.Save(imagePath);
            Console.WriteLine("\n\tSaved csv data tracks to image file: " + imagePath);

            Console.WriteLine("Index weights:   {0} = {1}\n\t\t {2} = {3}\n\t\t {4} = {5}\n\t\t {6} = {7}\n\t\t {8} = {9}",
                             comboHeaders[0], AcousticIndices.comboWeights[0], comboHeaders[1], AcousticIndices.comboWeights[1], comboHeaders[2], AcousticIndices.comboWeights[2],
                             comboHeaders[3], AcousticIndices.comboWeights[3], comboHeaders[4], AcousticIndices.comboWeights[4]);
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
            g.DrawImage(this.visualIndexTimeScale, 0, 0);
            Point pt1 = new Point(myX, 2);
            Point pt2 = new Point(myX, settings.TrackHeight - 1);
            g.DrawLine(new Pen(Color.Yellow, 1.0F), pt1, pt2);
            g.DrawImage(this.visualIndexTimeScale, 0, this.pictureBoxVisualIndex.Height - settings.TrackHeight);
            pt1 = new Point(myX, this.pictureBoxVisualIndex.Height - 2);
            pt2 = new Point(myX, this.pictureBoxVisualIndex.Height - settings.TrackHeight);
            g.DrawLine(new Pen(Color.Yellow, 1.0F), pt1, pt2);

            if (myX >= this.sourceRecording_MinutesDuration - 1)
                this.textBoxCursorValue.Text     = String.Format("{0:f2} <<{1:f2}>> {2:f2}", this.weightedIndices[myX - 1], this.weightedIndices[myX], "END");
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

            // GET MOUSE LOCATION
            int myX = e.X;
            int myY = e.Y;

            //DRAW RED LINE ON BAR TRACK
            for (int y = 0; y < barTrackImage.Height; y++)
                barTrackImage.SetPixel(this.pictureBoxVisualIndex.Left + myX, y, Color.Red);
            this.pictureBoxBarTrack.Image = barTrackImage;

            //EXTRACT RECORDING SEGMENT
            int startMilliseconds = (myX) * 60000;
            int endMilliseconds = (myX + 1) * 60000;
            if (settings.SegmentDuration == 3)
            {
                startMilliseconds = (myX - 1) * 60000;
                endMilliseconds = (myX + 2) * 60000;
            }
            if (startMilliseconds < 0) startMilliseconds = 0;

            string sourceFName = Path.GetFileNameWithoutExtension(settings.fiSourceRecording.FullName);
            string segmentFName = sourceFName + "_min" + myX.ToString() + ".wav"; //want a wav file

            string outputSegmentPath = Path.Combine(settings.diOutputDir.FullName, segmentFName); //path name of the segment file extracted from long recording

            FileInfo fiOutputSegment = new FileInfo(outputSegmentPath);


            Console.WriteLine("\n\tExtracting audio segment from source audio: minute " + myX + " to minute " + (myX + 1));
            Console.WriteLine("\n\tWriting audio segment to dir: " + settings.diOutputDir.FullName);
            Console.WriteLine("\n\t\t\tFile Name: " + segmentFName);

            //get segment from source recording
            DateTime time1 = DateTime.Now;
            //AudioRecording recording = AudioRecording.GetSegmentFromAudioRecording(settings.fiSourceRecording.FullName, startMilliseconds, endMilliseconds, settings.ResampleRate, outputSegmentPath);
            MasterAudioUtility.Segment(settings.ResampleRate, settings.fiSourceRecording, fiOutputSegment, startMilliseconds, endMilliseconds);
            AudioRecording recordingSegment = new AudioRecording(fiOutputSegment.FullName);

            DateTime time2 = DateTime.Now;
            TimeSpan timeSpan = time2 - time1;
            Console.WriteLine("\n\t\t\tExtraction time: " + timeSpan.TotalSeconds + " seconds");

            //store info
            this.labelSonogramFileName.Text = Path.GetFileName(recordingSegment.FilePath);
            settings.fiSegmentRecording = new FileInfo(recordingSegment.FilePath);


            //make the sonogram
            Image_MultiTrack image = MakeSonogram(recordingSegment);
            this.pictureBoxSonogram.Image = image.GetImage();


            this.hScrollBarSonogram.Location = new System.Drawing.Point(0, this.pictureBoxSonogram.Image.Height);
            //this.hScrollBarSonogram.Minimum = 0;
            this.hScrollBarSonogram.Maximum = pictureBoxSonogram.Width - this.panelSonogram.Width + 280; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            string sonogramPath = Path.Combine(settings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(segmentFName) + ".png"));
            Console.WriteLine("\n\tSaved sonogram to image file: " + sonogramPath);
            pictureBoxSonogram.Image.Save(sonogramPath);
            this.tabControlMain.SelectTab("tabPageDisplay");   
            this.labelSonogramFileName.Text = Path.GetFileName(segmentFName);
            
        }

        private Image_MultiTrack MakeSonogram(AudioRecording recordingSegment)
        {

            Console.WriteLine("\n\tPreparing sonogram of audio segment");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recordingSegment.FileName;
            sonoConfig.WindowSize = settings.FrameLength;
            sonoConfig.WindowOverlap = settings.FrameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recordingSegment.GetWavReader());

            // (iii) NOISE REDUCTION
            if (this.checkBoxSonnogramNoiseReduce.Checked)
            {
                Console.WriteLine("NOISE REDUCTION");
                var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, settings.SonogramBackgroundThreshold);
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
            if ((settings.fiSegmentRecording == null) || (!settings.fiSegmentRecording.Exists))
            {
                Console.WriteLine("Audacity cannot open audio segment file: <" + settings.fiSegmentRecording+">");
                Console.WriteLine("It does not exist!");
                this.tabControlMain.SelectTab("tabPageConsole");
                RunAudacity(settings.AudacityExe.FullName, " ", settings.diOutputDir.FullName);
            }
            else
                RunAudacity(settings.AudacityExe.FullName, settings.fiSegmentRecording.FullName, settings.diOutputDir.FullName);
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
            settings.diSourceDir = new DirectoryInfo(this.tfSourceDirectory.Text);
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
            settings.diOutputDir = new DirectoryInfo(tfOutputDirectory.Text);
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
            settings = new AudioBrowserSettings();
            try
            {
                settings.LoadSettings();
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
                settings.diSourceDir = new DirectoryInfo(this.tfSourceDirectory.Text);
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
                settings.diOutputDir = new DirectoryInfo(this.tfOutputDirectory.Text);
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
            this.pictureBoxSonogram.Left = -this.hScrollBarSonogram.Value;
        }

        /// <summary>
        /// handle event when refreshSonogram button is clicked
        /// redisplay the sonogram but with new Settings
        /// </summary>
        private void buttonRefreshSonogram_Click(object sender, EventArgs e)
        {
            if ((settings.fiSegmentRecording == null) || (!settings.fiSegmentRecording.Exists)) return;
            IAudioUtility audioUtility = MasterAudioUtility.Create(settings.ResampleRate); //creates AudioUtility and
            AudioRecording recordingSegment = new AudioRecording(settings.fiSegmentRecording.FullName);
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
