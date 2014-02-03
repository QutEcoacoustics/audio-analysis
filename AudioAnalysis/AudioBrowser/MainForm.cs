namespace AudioBrowser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

    using AnalysisBase;
    using AnalysisPrograms;
    using AnalysisRunner;
    using AudioAnalysisTools;
    using TowseyLib;

    using LINQtoCSV;
    using log4net;
    using log4net.Appender;
    using System.Threading;
    using System.Threading.Tasks;

    // 3 hr test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //8 min test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //SCC file site 4  // sunshinecoast1 "Y:\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
    //SCC file site 4  // sunshinecoast1 "\\hpc-fs.qut.edu.au\staging\availae\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"

    public partial class MainForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainForm));


        // START hard code area (comment this before commiting!)

        /*
        private FileInfo csvFile = new FileInfo(@"F:\Projects\test-audio\Towsey.Crow\2012-01-20-megaherzzz-no-music_Towsey.Crow.Indices.csv");//this.tabBrowseAudio.CsvFile,
        //private FileInfo  ImgFile = this.tabBrowseAudio.IndicesImageFile,
        //private FileInfo  AnalysisId = this.tabBrowseAudio.AnalysisId,
        private FileInfo analysisConfigFile = new FileInfo(@"F:\Projects\QUT\qut-svn-trunk\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg");// this.tabBrowseAudio.ConfigFile,
        private DirectoryInfo outputDir = new DirectoryInfo(@"F:\Projects\test-audio\");// this.tabBrowseAudio.OutputDirectory,
        private FileInfo audioFile = new FileInfo(@"F:\Projects\test-audio\2012-01-20-megaherzzz-no-music.mp3");
        //this.tabBrowseAudio.AudioFile,
        */

        // END hard code area (comment this before commiting!)


        private Helper helper { get; set; }

        private TabBrowseAudio tabBrowseAudio;

        public MainForm()
        {
            // must be here, must be first
            InitializeComponent();

            // add richtextbox to logger
            RichTextBoxAppender.SetRichTextBox(this.richTextBoxConsole, "RichTextBoxAppender");

            // available logging levels:
            //Log.Fatal("Fatal", exception); //Log.FatalFormat("{0}", "Fatal"); 
            //Log.Error("Error", exception); //Log.FatalFormat("{0}", "Error");
            //Log.Warn("Warn", exception); //Log.FatalFormat("{0}", "Warn");
            //Log.Info("Info", exception); //Log.FatalFormat("{0}", "Info");
            //Log.Debug("Debug", exception); //Log.FatalFormat("{0}", "Debug");

            LoggedConsole.WriteLine("Starting up Audio Browser at {0}. {1}.", DateTime.Now, Helper.Copyright);

            this.helper = new Helper();

            this.tabBrowseAudio = new TabBrowseAudio(this.helper, this.helper.DefaultAnalysisIdentifier,
                this.helper.DefaultOutputDir, this.helper.DefaultConfigDir,
                this.helper.DefaultConfigFileExt, this.helper.DefaultAudioFileExt,
                this.helper.DefaultResultImageFileExt, this.helper.DefaultResultTextFileExt);


            // init tabs
            InitAnalyseTab();

            this.tabControlMain.SelectTab(this.tabPageConsole);


        } //MainForm()

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            this.richTextBoxConsole.Clear();
        }

        private void btnAnalyseSelectedAudioFiles_Click(object sender, EventArgs e)
        {
            /*
            //string analysisName = ((KeyValuePair<string, string>)this.comboBoxSourceFileAnalysisType.SelectedItem).Key;
            //this.Helper.AnalysisIdentifier = analysisName;
            //string configPath = Path.Combine(Helper.diConfigDir.FullName, analysisName + AudioBrowserSettings.DefaultConfigExt);
            //var fiConfig = new FileInfo(configPath);
            // this.analysisParams = ConfigDictionary.ReadPropertiesFile(configPath);

            //this.Helper.fiAnalysisConfig = fiConfig;
            // WriteAnalysisParameters2Console(this.analysisParams, this.CurrentSourceFileAnalysisType);
            //CheckForConsistencyOfAnalysisTypes(this.CurrentSourceFileAnalysisType, this.analysisParams);


            //this.textBoxConsole.Clear();
            this.tabControlMain.SelectTab(tabPageConsole);





            var audioFileName = item.FileName;
            var fiSourceRecording = item.FullName;
            Helper.fiSourceRecording = fiSourceRecording;
            LoggedConsole.WriteLine("# Source audio - filename: " + Path.GetFileName(fiSourceRecording.Name));
            LoggedConsole.WriteLine("# Source audio - datetime: {0}    {1}", fiSourceRecording.CreationTime.ToLongDateString(), fiSourceRecording.CreationTime.ToLongTimeString());
            LoggedConsole.WriteLine("# Start processing at: {0}", DateTime.Now.ToLongTimeString());

            Stopwatch stopwatch = new Stopwatch(); //for checking the parallel loop.
            stopwatch.Start();

            var currentlySelectedIdentifier = ((KeyValuePair<string, string>)this.comboBoxSourceFileAnalysisType.SelectedItem).Key;
            var analyser = this.Helper.GetAnalyser(currentlySelectedIdentifier);

            var settings = analyser.DefaultSettings;
            var configuration = new ConfigDictionary(fiConfig.FullName);
            settings.SetUserConfiguration(this.Helper.DefaultTempFilesDir, fiConfig, configuration.GetTable(), this.Helper.diOutputDir,
                                          AudioAnalysisTools.Keys.SEGMENT_DURATION, AudioAnalysisTools.Keys.SEGMENT_OVERLAP);

            //################# PROCESS THE RECORDING #####################################################################################
            var analyserResults = this.Helper.ProcessRecording(fiSourceRecording, analyser, settings);
            //NEXT LINE was my old code
            // var op1 = AudioBrowserTools.ProcessRecording(fiSourceRecording, this.Helper.diOutputDir, fiConfig);

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
            //LoggedConsole.WriteLine("Output  to  directory: " + this.tfOutputDirectory.Text);
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

            */
        }

        // ********************************************************************************************
        // Browse Tab
        // ********************************************************************************************

        private int SonogramBuffer
        {
            get
            {
                var bufferAmountString = this.textBoxSonogramBuffer.Text;
                int bufferAmount = 15;
                if (int.TryParse(bufferAmountString, out bufferAmount))
                {

                }

                if (bufferAmount < 0)
                {
                    bufferAmount = 0;
                }
                else if (bufferAmount > 60)
                {
                    bufferAmount = 60;
                }

                // update textbox to match determined value
                this.textBoxSonogramBuffer.Text = bufferAmount.ToString();
                return bufferAmount;
            }
            set
            {
                int bufferAmount = 15;
                if (value < 0)
                {
                    bufferAmount = 0;
                }
                else if (value > 60)
                {
                    bufferAmount = 60;
                }

                this.textBoxSonogramBuffer.Text = bufferAmount.ToString();
            }
        }

        private bool IsCreatingSonogramImage = false;

        private void ChangeSonogramImage()
        {
            if (this.IsCreatingSonogramImage)
            {
                return;
            }

            if (this.tabBrowseAudio.AudioFile == null || !File.Exists(this.tabBrowseAudio.AudioFile.FullName))
            {
                LoggedConsole.WriteLine("#############################################");
                LoggedConsole.WriteLine("Could not find audio file to create sonogram.");
                LoggedConsole.WriteLine("#############################################");
                this.tabControlMain.SelectTab(tabPageConsole);
                return;
            }

            this.IsCreatingSonogramImage = true;

            // collect data for sonogram generation
            var segmentbuffer = chkSonogramBuffer.Checked ? TimeSpan.FromSeconds(this.SonogramBuffer) : TimeSpan.Zero;
            var annotated = this.chkAudioNavAnnotateSonogram.Checked;
            var noiseReduced = this.chkAudioNavNoiseReduce.Checked;
            var backgroundNoiseThreshold = this.helper.SonogramBackgroundThreshold;

            this.ClearSonogramImage();

            var bgWorker = new BackgroundWorker();

            bgWorker.DoWork += (bgWorkerSender, bgWorkerDoWorkEvent) =>
            {
                // segment audio and generate sonogram
                this.tabBrowseAudio.UpdateSonogram(
                    noiseReduced,
                    backgroundNoiseThreshold,
                    annotated,
                    segmentbuffer);
            };

            bgWorker.RunWorkerCompleted += (bgWorkerSender, bgWorkerCompletedEvent) =>
            {
                Action done = () =>
                {
                    this.IsCreatingSonogramImage = false;
                    this.UpdateSonogramImage();
                };

                if (this.tabControlMain.InvokeRequired)
                {
                    this.tabControlMain.BeginInvoke(done);
                }
                else
                {
                    done();
                }
            };

            bgWorker.RunWorkerAsync();
        }

        private void ClearSonogramImage()
        {
            if (this.pictureBoxAudioNavSonogram.Image != null)
            {
                this.pictureBoxAudioNavSonogram.Image.Dispose();
                this.pictureBoxAudioNavSonogram.Image = new Bitmap(1200, 350);
                this.pictureBoxAudioNavSonogram.Size = new Size(1200, 350);
            }

            this.textBoxBrowseAudioSegmentFile.Text = string.Empty;
            this.textBoxBrowseSonogramImageFile.Text = string.Empty;

            this.txtAudioNavClickLocation.Text = string.Empty;
            this.txtAudioNavClickValue.Text = string.Empty;
        }

        private void UpdateSonogramImage()
        {
            this.textBoxBrowseAudioSegmentFile.Text = this.tabBrowseAudio.AudioSegmentFile.Name;
            this.textBoxBrowseSonogramImageFile.Text = this.tabBrowseAudio.SonogramImageFile.Name;

            // resize picture box to contain sonogram image
            this.pictureBoxAudioNavSonogram.Size = new Size(
                this.tabBrowseAudio.SonogramImage.Width,
                this.tabBrowseAudio.SonogramImage.Height);

            // set new sonogram image
            this.pictureBoxAudioNavSonogram.Image = this.tabBrowseAudio.SonogramImage;
            //this.tabControlMain.SelectTab(tabPageBrowseAudioFile);

            // set location and value at click
            this.txtAudioNavClickLocation.Text = this.tabBrowseAudio.GetLocationString(this.tabBrowseAudio.ClickLocation.X);
            this.txtAudioNavClickValue.Text = this.tabBrowseAudio.GetValueString(this.tabBrowseAudio.ClickLocation.X, this.tabBrowseAudio.TrackValues);

            var value = 0.0;
            if (this.tabBrowseAudio.TrackValues != null) // not applicable if clicking on spectrogram image
            {
                value = this.tabBrowseAudio.TrackValues[this.tabBrowseAudio.ClickLocation.X];
            }
            this.lblCurrentSegment.Text = this.txtAudioNavClickLocation.Text + " (" + value.ToString("f2") + ")";
        }

        private void ClearIndicesImage()
        {
            if (this.pictureBoxAudioNavIndicies.Image != null)
            {
                this.pictureBoxAudioNavIndicies.Image.Dispose();
                this.pictureBoxAudioNavIndicies.Image = new Bitmap(1200, 350);
                this.pictureBoxAudioNavIndicies.Size = new Size(1200, 350);
            }

            if (this.pictureBoxAudioNavSonogram.Image != null)
            {
                this.pictureBoxAudioNavSonogram.Image.Dispose();
                this.pictureBoxAudioNavSonogram.Image = new Bitmap(1200, 350);
                this.pictureBoxAudioNavSonogram.Size = new Size(1200, 350);
            }

            if (this.pictureBoxAudioNavClickTrack.Image != null)
            {
                this.pictureBoxAudioNavClickTrack.Image.Dispose();
                this.pictureBoxAudioNavClickTrack.Image = new Bitmap(1200, 20);
                this.pictureBoxAudioNavClickTrack.Size = new Size(1200, 20);
            }

            this.listBoxAudioNavCSVHeaders.Items.Clear();

            this.txtAudioNavClickLocation.Text = string.Empty;
            this.txtAudioNavClickValue.Text = string.Empty;
            this.txtAudioNavCursorLocation.Text = string.Empty;
            this.txtAudioNavCursorValue.Text = string.Empty;

            this.textBoxBrowseAudioSegmentFile.Text = string.Empty;
            this.textBoxBrowseSonogramImageFile.Text = string.Empty;
        }

        private void UpdateIndicesImage()
        {
            // show indices information
            this.txtAudioNavAnalysisType.Text = this.tabBrowseAudio.AnalysisId;
            this.txtAudioNavDuration.Text = this.tabBrowseAudio.AudioDuration.TotalMinutes.ToString();
            this.listBoxAudioNavCSVHeaders.Items.AddRange(this.tabBrowseAudio.CsvHeaderList.ToArray());
            this.lblAudioNavCSVHeaders.Text = this.tabBrowseAudio.CsvHeaderInfo;

            // resize picture box to contain indices image
            this.pictureBoxAudioNavIndicies.Size = new Size(
                this.tabBrowseAudio.IndicesImage.Width,
                this.tabBrowseAudio.IndicesImage.Height);

            // resize click track width
            this.pictureBoxAudioNavClickTrack.Width = this.tabBrowseAudio.IndicesImage.Width;

            // set new image to click track
            this.pictureBoxAudioNavClickTrack.Image = new Bitmap(this.pictureBoxAudioNavClickTrack.Width, this.pictureBoxAudioNavClickTrack.Height);

            // set new indices image
            this.pictureBoxAudioNavIndicies.Image = this.tabBrowseAudio.IndicesImage;
            //this.tabControlMain.SelectTab(tabPageBrowseAudioFile);

            // resize split box
            var offset = 20;
            var height = this.tabBrowseAudio.IndicesImage.Height + this.pictureBoxAudioNavClickTrack.Height + offset;
            this.splitContainerImages.SplitterDistance = height;
        }

        private void UpdateClickTrackImage()
        {
            var clickX = this.tabBrowseAudio.ClickLocation.X;
            var height = this.pictureBoxAudioNavClickTrack.Image.Height;

            var image = this.pictureBoxAudioNavClickTrack.Image;
            using (Graphics g = Graphics.FromImage(image))
            {
                // draw red line on click track
                g.DrawLine(Pens.Red, clickX, 0, clickX, height);
                this.pictureBoxAudioNavClickTrack.Image = image;
            }
        }

        private void btnAudioNavSelectFiles_Click(object sender, EventArgs e)
        {
            // open a window to collect information to create a new AudioNavigator object.
            /*
            var selectFilesForm = new AudioNavigatorFileSelectForm(this.helper)
            {
                CsvFile = csvFile,//this.tabBrowseAudio.CsvFile,
                ImgFile = this.tabBrowseAudio.IndicesImageFile,
                AnalysisId = this.tabBrowseAudio.AnalysisId,
                AnalysisConfigFile = analysisConfigFile,// this.tabBrowseAudio.ConfigFile,
                OutputDir = outputDir,// this.tabBrowseAudio.OutputDirectory,
                AudioFile = audioFile,
                //this.tabBrowseAudio.AudioFile,
            };
            */


            var selectFilesForm = new AudioNavigatorFileSelectForm(this.helper)
            {
                CsvFile = this.tabBrowseAudio.CsvFile,
                ImgFile = this.tabBrowseAudio.IndicesImageFile,
                AnalysisId = this.tabBrowseAudio.AnalysisId,
                AnalysisConfigFile = this.tabBrowseAudio.ConfigFile,
                OutputDir = this.tabBrowseAudio.OutputDirectory,
                AudioFile = this.tabBrowseAudio.AudioFile,
            };


            using (selectFilesForm)
            {
                var dialogResult = selectFilesForm.ShowDialog();
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    // form checks that values are valid, don't need to check again

                    this.btnAudioNavSelectFiles.Enabled = false;

                    // refresh indices info and image
                    this.tabBrowseAudio.SetNewFiles(
                        selectFilesForm.CsvFile,
                        selectFilesForm.ImgFile,
                        selectFilesForm.AudioFile,
                        selectFilesForm.AnalysisId,
                        selectFilesForm.AnalysisConfigFile,
                        selectFilesForm.OutputDir,
                        this.helper.TrackNormalisedDisplay);

                    ClearIndicesImage();

                    var bgWorker = new BackgroundWorker();

                    bgWorker.DoWork += (bgWorkerSender, bgWorkerDoWorkEvent) =>
                    {
                        if (this.tabBrowseAudio.CsvFile != null && File.Exists(this.tabBrowseAudio.CsvFile.FullName))
                        {
                            this.tabBrowseAudio.UpdateIndicesFromCsvFile();
                        }
                        else if (this.tabBrowseAudio.IndicesImageFile != null && File.Exists(this.tabBrowseAudio.IndicesImageFile.FullName))
                        {
                            this.tabBrowseAudio.UpdateIndicesFromImageFile();
                        }
                        else
                        {
                            // error
                            throw new ArgumentException();
                        }
                    };

                    bgWorker.RunWorkerCompleted += (bgWorkerSender, bgWorkerCompletedEvent) =>
                    {
                        Action done = () =>
                        {
                            //this.tabControlMain.SelectTab(this.tabPageAnalyseAudioFile);
                            this.btnAudioNavSelectFiles.Enabled = true;
                            this.UpdateIndicesImage();
                        };

                        if (this.tabControlMain.InvokeRequired)
                        {
                            this.tabControlMain.BeginInvoke(done);
                        }
                        else
                        {
                            done();
                        }
                    };

                    bgWorker.RunWorkerAsync();
                }
            }
        }

        private void btnAudioNavRunAudacity_Click(object sender, EventArgs e)
        {
            var audacityExe = this.helper.AudacityExe;
            var audioSegmentFile = this.tabBrowseAudio.AudioSegmentFile;

            if (audacityExe == null || !File.Exists(audacityExe.FullName))
            {
                LoggedConsole.WriteWarnLine("Audacity.exe not found." + 
                                           " Edit the AudioBrowser.exe.config file and enter correct path in the 'AudacityExeList' key.");
                // switch to the console.
                this.tabControlMain.SelectTab(this.tabPageConsole);
                //MessageBox.Show("Could not find Audacity. Is it installed?");
            }
            else 
            {
                string audioSegmentPath = string.Empty;
                if (audioSegmentFile == null || !File.Exists(audioSegmentFile.FullName))
                {
                    MessageBox.Show("There is no audio file specified for Audacity to open!");
                }
                else
                {
                    audioSegmentPath = audioSegmentFile.FullName;
                }
                TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(this.helper.AudacityExe.FullName);
                process.Run(audioSegmentPath, this.helper.DefaultOutputDir.FullName, false);
            }                            
        }

        private void pictureBoxAudioNavIndicies_MouseHover(object sender, EventArgs e)
        {
            this.pictureBoxAudioNavIndicies.Cursor = Cursors.HSplit;
        }

        private void pictureBoxAudioNavIndicies_MouseMove(object sender, MouseEventArgs e)
        {
            var currentCursorX = e.X; // pixel position = minutes
            var durationMin = Math.Ceiling(this.tabBrowseAudio.AudioDuration.TotalMinutes);

            if (currentCursorX > durationMin)
            {
                return;
            }

            var currentTrackValues = this.tabBrowseAudio.TrackValues;

            // set text for current cursor location

            this.txtAudioNavCursorLocation.Text = this.tabBrowseAudio.GetLocationString(currentCursorX);

            // draw dashed lines either side of the cursor
            if (this.pictureBoxAudioNavIndicies.Image != null)
            {
                float[] dashValues = { 2, 2, 2, 2 };
                var pictureBoxPen = new Pen(Color.Red, 1.0F); // double hair line used to show time position
                pictureBoxPen.DashPattern = dashValues;

                using (Graphics g = this.pictureBoxAudioNavIndicies.CreateGraphics())
                {
                    g.DrawImage(this.pictureBoxAudioNavIndicies.Image, 0, 0);

                    Point pt1 = new Point(currentCursorX - 1, 2);
                    Point pt2 = new Point(currentCursorX - 1, this.pictureBoxAudioNavIndicies.Height);
                    g.DrawLine(pictureBoxPen, pt1, pt2);

                    pt1 = new Point(currentCursorX + 1, 2);
                    pt2 = new Point(currentCursorX + 1, this.pictureBoxAudioNavIndicies.Height);
                    g.DrawLine(pictureBoxPen, pt1, pt2);
                }

                this.txtAudioNavCursorValue.Text = this.tabBrowseAudio.GetValueString(currentCursorX, currentTrackValues);
            }
        }

        private void pictureBoxAudioNavIndicies_MouseClick(object sender, MouseEventArgs e)
        {
            // gather information from click
            var currentCursorX = e.X;
            //var currentCursorY = e.Y;

            //var currentTrackValues = this.tabBrowseAudio.TrackValues;

            // segment start and end
            var segmentDuration = TimeSpan.FromMinutes(this.helper.DefaultSegmentDuration);
            var segmentOffsetStart = TimeSpan.FromMinutes(currentCursorX);
            var segmentOffsetEnd = segmentOffsetStart + segmentDuration;

            // update stored values
            this.tabBrowseAudio.UpdateOffsets(segmentOffsetStart, segmentOffsetEnd, new Point(e.X, e.Y));

            UpdateClickTrackImage();

            ChangeSonogramImage();
        }

        private void btnAudioNavRefreshSonogram_Click(object sender, EventArgs e)
        {
            ChangeSonogramImage();
        }

        private void btnDisplaySimilarSegments_Click(object sender, EventArgs e)
        {
            if (this.tabBrowseAudio.ClickLocation != null)
            {
                var selectedValue = this.tabBrowseAudio.TrackValues[this.tabBrowseAudio.ClickLocation.X];

                // value, index
                var allItems = new List<Tuple<double, int>>();

                for (var index = 0; index < this.tabBrowseAudio.TrackValues.Length; index++)
                {
                    var value = this.tabBrowseAudio.TrackValues[index];
                    allItems.Add(new Tuple<double, int>(value, index));
                }

                var sorted = allItems.OrderBy(i => Math.Abs(i.Item1 - selectedValue)).ToList();

                this.listBoxSimilarSegments.Items.Clear();

                // skip the first one - that's the one that was selected.
                this.listBoxSimilarSegments.Items.AddRange(sorted.Skip(1).Take(8).Select(i => string.Format("{0}: {1}", i.Item2, i.Item1.ToString("f2"))).ToArray());

                //Accord.Math.Distance.Euclidean(
            }
            // 
        }

        // ********************************************************************************************
        // Analyse Tab
        // ********************************************************************************************

        private CancellationTokenSource cancellationTokenSource;

        private string AnalyserAnalysisSelected
        {
            get
            {
                var selectedAnalysisType = ((KeyValuePair<string, string>)this.comboboxAnalyseAnalyser.SelectedItem).Key;
                return selectedAnalysisType;
            }
            set
            {
                if (value != null)
                {
                    this.comboboxAnalyseAnalyser.SelectedValue = value;
                }
            }
        }

        private FileInfo AnalyserAudioFile
        {
            get { try { return new FileInfo(this.textboxAnalyseAudioFilePath.Text); } catch { return null; } }
            set { if (value != null) { this.textboxAnalyseAudioFilePath.Text = value.FullName; } }
        }

        private FileInfo AnalyserConfigFile
        {
            get { try { return new FileInfo(this.textboxAnalyseConfigFilePath.Text); } catch { return null; } }
            set { if (value != null) { this.textboxAnalyseConfigFilePath.Text = value.FullName; } }
        }

        private DirectoryInfo AnalyserOutputDir
        {
            get { try { return new DirectoryInfo(this.textBoxAnalyseOutputDir.Text); } catch { return null; } }
            set { if (value != null) { this.textBoxAnalyseOutputDir.Text = value.FullName; } }
        }

        private void InitAnalyseTab()
        {
            //create comboBox display for anaylser
            this.comboboxAnalyseAnalyser.DataSource = this.helper.AnalysersAvailable.ToList();
            this.comboboxAnalyseAnalyser.ValueMember = "Key";
            this.comboboxAnalyseAnalyser.DisplayMember = "Value";

            // set defaults 
            this.AnalyserOutputDir = this.helper.DefaultOutputDir;
            this.AnalyserAnalysisSelected = this.helper.DefaultAnalysisIdentifier;

            //this.AnalyserAudioFile = audioFile;
            //this.AnalyserConfigFile = analysisConfigFile;
            //this.AnalyserOutputDir = outputDir;
        }

        private void btnAnalyseAudioFileBrowse_Click(object sender, EventArgs e)
        {
            var currentDir = this.helper.DefaultSourceDir != null ? this.helper.DefaultSourceDir.FullName : string.Empty;
            if (this.AnalyserAudioFile != null && Directory.Exists(this.AnalyserAudioFile.DirectoryName))
            {
                currentDir = this.AnalyserAudioFile.DirectoryName;
            }

            var file = Helper.PromptUserToSelectFile("Select Audio File", Helper.SelectAudioFilter, currentDir);
            if (file != null)
            {
                this.AnalyserAudioFile = file;
            }
        }

        private void btnAnalyseConfigFileBrowse_Click(object sender, EventArgs e)
        {
            var currentDir = string.Empty;
            if (this.AnalyserConfigFile != null && Directory.Exists(this.AnalyserConfigFile.DirectoryName))
            {
                currentDir = this.AnalyserConfigFile.DirectoryName;
            }
            else if (this.helper.DefaultConfigDir != null && Directory.Exists(this.helper.DefaultConfigDir.FullName))
            {
                currentDir = this.helper.DefaultConfigDir.FullName;
            }
            else
            {
                currentDir = this.helper.GetExeDir.FullName;
            }

            var file = Helper.PromptUserToSelectFile("Select configuration file for analyser", Helper.SelectConfigFilter, currentDir);
            if (file != null)
            {
                this.AnalyserConfigFile = file;
            }
        }

        private void btnAnalyseConfigFileEdit_Click(object sender, EventArgs e)
        {
            if (this.AnalyserConfigFile == null || !File.Exists(this.AnalyserConfigFile.FullName))
            {
                MessageBox.Show("Please specify a config file.");
            }
            else if (this.helper.TextEditorExe == null || !File.Exists(this.helper.TextEditorExe.FullName))
            {
                MessageBox.Show("Could not find a program to edit text files.");
            }
            else
            {
                TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(this.helper.TextEditorExe.FullName);
                process.Run(this.AnalyserConfigFile.FullName, this.helper.DefaultOutputDir.FullName, false);
            }
        }

        private void comboboxAnalyseAnalyser_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.labelAnalyseSelectedAnalyserKey.Text = "Selected analyser id: " + this.AnalyserAnalysisSelected;
        }

        private void btnAnalyseOutputDirBrowse_Click(object sender, EventArgs e)
        {
            var currentDir = string.Empty;
            if (this.AnalyserOutputDir != null && Directory.Exists(this.AnalyserOutputDir.FullName))
            {
                currentDir = this.AnalyserOutputDir.FullName;
            }
            else
            {
                currentDir = this.helper.GetExeDir.FullName;
            }

            var selectedDir = Helper.PromptUserToSelectDirectory("Select output directory for analysis", currentDir);

            if (selectedDir != null && Directory.Exists(selectedDir.FullName))
            {
                this.AnalyserOutputDir = selectedDir;
            }
        }

        private void btnAanlyseRun_Click(object sender, EventArgs e)
        {
            if (this.AnalyserAudioFile == null || !File.Exists(this.AnalyserAudioFile.FullName))
            {
                MessageBox.Show("Could not find audio file. Please check the path.");
                return;
            }

            if (this.AnalyserConfigFile == null || !File.Exists(this.AnalyserConfigFile.FullName))
            {
                MessageBox.Show("Could not find configuration file. Please check the path.");
                return;
            }


            if (this.AnalyserOutputDir == null || !Directory.Exists(this.AnalyserOutputDir.FullName))
            {
                MessageBox.Show("Could not find the output directory. Please check the path.");
                return;
            }

            // disable analyse button so it cannot be clicked again while analysis is already running.
            this.btnAanlyseRun.Enabled = false;

            // analysis information
            var analyserId = AnalyserAnalysisSelected;
            var analyser = this.helper.GetAnalyser(analyserId);
            var settings = analyser.DefaultSettings;

            var config = new ConfigDictionary(this.AnalyserConfigFile.FullName);
            var analysisParams = config.GetDictionary();

            settings.SetUserConfiguration(this.helper.DefaultTempFilesDir, this.AnalyserConfigFile, config.GetTable(), this.AnalyserOutputDir,
                                            AudioAnalysisTools.Keys.SEGMENT_DURATION, AudioAnalysisTools.Keys.SEGMENT_OVERLAP);

            // record run information
            Log.Debug("Parameters for selected analysis: " + analyserId);
            foreach (KeyValuePair<string, string> kvp in analysisParams)
            {
                Log.DebugFormat("\t{0} = {1}", kvp.Key, kvp.Value);
            }

            string analysisName = analysisParams[AudioAnalysisTools.Keys.ANALYSIS_NAME];
            if (analyserId != analysisName)
            {
                Log.WarnFormat("Analysis type selected in browser ({0}) not same as that in config file ({1})", analyserId, analysisName);
            }

            Log.Info("Analysis type: " + analyserId);

            // switch to the console.
            //this.tabControlMain.SelectTab(this.tabPageConsole);

            var backgroundWorkerAnalyser = new BackgroundWorker();
            backgroundWorkerAnalyser.WorkerReportsProgress = false;
            backgroundWorkerAnalyser.WorkerSupportsCancellation = false;

            backgroundWorkerAnalyser.DoWork += (bgWorkerSender, bgWorkerDoWorkEvent) =>
            {
                this.helper.ProcessRecording(this.AnalyserAudioFile, this.AnalyserConfigFile, analyser, settings);
            };

            backgroundWorkerAnalyser.RunWorkerCompleted += (bgWorkerSender, bgWorkerCompletedEvent) =>
            {
                Action done = () =>
                {
                    this.btnAanlyseRun.Enabled = true;
                };

                if (this.tabControlMain.InvokeRequired)
                {
                    this.tabControlMain.BeginInvoke(done);
                }
                else
                {
                    done();
                }
            };

            backgroundWorkerAnalyser.RunWorkerAsync();
        }

        // ********************************************************************************************
        // Under Development Tab
        // ********************************************************************************************

        private void btnCSV2ARFF_Click(object sender, EventArgs e)
        {
            //OPEN A FILE DIALOGUE TO FIND CSV FILE
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open File Dialogue";
            fdlg.InitialDirectory = this.helper.DefaultOutputDir.FullName;
            fdlg.Filter = "CSV files (*.csv)|*.csv";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                var fiCSVFile = new FileInfo(fdlg.FileName);
                //this.Helper.fiCSVFile = fiCSVFile; // store in settings so can be accessed later.
                //this.Helper.diOutputDir = new DirectoryInfo(Path.GetDirectoryName(fiCSVFile.FullName)); // change to selected directory

                // ##################################################################################################################
                int status = this.helper.CSV2ARFF(fiCSVFile);
                // ##################################################################################################################
                //this.tabControlMain.SelectTab("tabPageConsole");

                if (status > 0)
                {
                    Log.Warn("ERROR: Error converting csv file to ARFF and SEE5 formats");
                }
                else
                {
                    Log.Info("Successfully converted CSV file to ARFF and SEE5 formats.");
                } // (status)

            } // if (DialogResult.OK)


        }



    } //class MainForm : Form
}
