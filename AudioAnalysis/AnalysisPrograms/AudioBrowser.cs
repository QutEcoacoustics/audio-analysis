using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

//using AudioTools.AudioUtlity;
using AudioAnalysisTools;
using TowseyLib;

namespace AnalysisPrograms
{

    //default browser ini path - USE AS COMMAND LINE ARGUMENT
    //@"C:\SensorNetworks\WavFiles\SunshineCoast\AudioBrowser.ini"

    class AudioBrowser : Form 
    {
        // DEFAULT PARAMETER VALUES
//        const string DEFAULT_csvPath = @"C:\SensorNetworks\WavFiles\SunshineCoast\AcousticIndices_DM420062_WeightedIndices.csv";
//        const string DEFAULT_recordingPath = @"Z:\Site 4\DM420062.mp3";
        const string DEFAULT_csvPath = @"NOT_SUPPLIED";
        const string DEFAULT_recordingPath = @"NOT_SUPPLIED";
        const string DEFAULT_outputDir = @"NOT_SUPPLIED";
        const string DEFAULT_AudacityPath = @"C:\Program Files (x86)\Audacity 1.3 Beta (Unicode)\audacity.exe";

        const int DEFAULT_trackHeight = 20; //number of tracks to appear in the visual index
        const int DEFAULT_trackCount  = 15; //pixel height of track in the visual index
        const int DEFAULT_segmentDuration = 1;
        const int DEFAULT_resampleRate = 17640;
        const int DEFAULT_frameLength = 512;
        const double DEFAULT_frameOverlap = 0.0;
        const double DEFAULT_sonogram_BackgroundThreshold = 4.0;  //dB


        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_CSV_PATH        = "CSV_PATH";
        public static string key_RECORDING_PATH  = "RECORDING_PATH";
        public static string key_OUTPUT_DIR      = "OUTPUT_DIR";
        public static string key_AUDACITY_PATH   = "AUDACITY_PATH";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_RESAMPLE_RATE   = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH    = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_SONOGRAM_BG_THRESHOLD = "SONOGRAM_BG_THRESHOLD";


        /// <summary>
        /// a set of parameters derived from ini file
        /// </summary>
        public struct Parameters
        {
            public bool iniFileFound;
            public int frameLength, resampleRate, segmentDuration;
            public double frameOverlap, sonogram_BackgroundThreshold;
            public int trackHeight, trackCount;
            public string csvPath, recordingPath, outputDir, AudacityPath;

            public Parameters(string dummy)
            {
                iniFileFound  = false;
                csvPath       = DEFAULT_csvPath;
                recordingPath = DEFAULT_recordingPath;
                outputDir     = DEFAULT_outputDir;
                AudacityPath  = DEFAULT_AudacityPath;
                segmentDuration = DEFAULT_segmentDuration; // in whole minutes
                resampleRate  = DEFAULT_resampleRate; //samples per second
                frameLength   = DEFAULT_frameLength;
                frameOverlap  = DEFAULT_frameOverlap;
                trackHeight   = DEFAULT_trackHeight; //number of tracks to appear in the visual index
                trackCount    = DEFAULT_trackCount; //pixel height of track in the visual index
                sonogram_BackgroundThreshold = DEFAULT_sonogram_BackgroundThreshold;  //dB

            } //Parameters
        } //struct Parameters
        Parameters parameters;




        private Panel leftPanel = new Panel();
        private Panel rightPanel = new Panel();
        private Panel indexPanel = new Panel();
        private Panel barTrackPanel = new Panel();
        private Panel sonogramPanel = new Panel();
        private HScrollBar sonogramPanel_hScrollBar = new HScrollBar();
        private PictureBox visualIndex = new PictureBox();
        private PictureBox sonogramPicture;

        private Image visualIndexTimeScale; //used on index image and reused - hence store.


        internal Button loadIndicesButton;
        internal Button audacityButton;
        internal TextBox time_TextBox;
        internal TextBox segmentName_TextBox;
        internal TextBox recordingDir_TextBox;
        internal TextBox recordingFileName_TextBox;
        internal TextBox CSVDir_TextBox;
        internal TextBox outputDir_TextBox;
        internal TextBox message_TextBox;
        internal Label CSVDir_Label;
        internal Label time_Label;
        internal Label segmentName_Label;
        internal Label recordingDir_Label;
        internal Label recordingFileName_Label;
        internal Label outputDir_Label;
        internal Label message_Label;


        private string recordingSegmentPath;
        private int minutesDuration = 0;
        private string iniFileName = "AudioBrowser.ini";




        //private Panel langPanel = new Panel();
        //private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
        //private RadioButton radBritish = new RadioButton();
        //private RadioButton radFrench = new RadioButton();
        //private RadioButton radSpanish = new RadioButton();



        public static void Main() 
        {
            string exeDir = System.Environment.CurrentDirectory;
            string[] args = System.Environment.GetCommandLineArgs();

            Application.Run(new AudioBrowser(args, exeDir));
        }

        public AudioBrowser(string[] commandLineArguments, string exeDir) //constructor
        {

            string iniPath = Path.Combine(exeDir, iniFileName);
            if (commandLineArguments.Length == 2) iniPath = commandLineArguments[1]; //arg[0] is the exe file.
            if (!File.Exists(iniPath))
            {
                string dummy = "";
                this.parameters = new Parameters(dummy); //default values
            }
            else
            {
                int verbosity = 0;
                this.parameters = ReadIniFile(iniPath, verbosity);
            }
            InitaliseLeftPanelControls();
            SetUpPanels();

            if (! this.parameters.iniFileFound)
            {
                this.message_TextBox.ForeColor = Color.Red;
                this.message_TextBox.Text = "FATAL ERROR!   COULD NOT FIND INI FILE <" + iniPath + ">" + Environment.NewLine + Environment.NewLine +
                    "YOU CANNOT PROCEED." + Environment.NewLine + "";
                this.loadIndicesButton.Hide();
            }


        } // MainForm



        public static AudioBrowser.Parameters ReadIniFile(string iniPath, int verbosity)
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            AudioBrowser.Parameters p; // st
            p.iniFileFound = true;
            p.csvPath = dict[AudioBrowser.key_CSV_PATH];
            p.recordingPath = dict[AudioBrowser.key_RECORDING_PATH];
            p.outputDir = dict[AudioBrowser.key_OUTPUT_DIR];
            p.AudacityPath = dict[AudioBrowser.key_AUDACITY_PATH];
            p.frameLength = Int32.Parse(dict[AudioBrowser.key_FRAME_LENGTH]);
            p.resampleRate = Int32.Parse(dict[AudioBrowser.key_RESAMPLE_RATE]);
            p.segmentDuration = Int32.Parse(dict[AudioBrowser.key_SEGMENT_DURATION]);
            p.frameOverlap = Double.Parse(dict[AudioBrowser.key_FRAME_OVERLAP]);
            
            //add in internal parameters
            p.trackHeight = DEFAULT_trackHeight; //number of tracks to appear in the visual index
            p.trackCount = DEFAULT_trackCount; //pixel height of track in the visual index
            p.sonogram_BackgroundThreshold = DEFAULT_sonogram_BackgroundThreshold;

            //if (verbosity > 0)
            //{
            //    Log.WriteLine("# PARAMETER SETTINGS:");
            //    Log.WriteLine("Segment size: Duration = {0} minutes;  Overlap = {1} seconds.", paramaters.segmentDuration, paramaters.segmentOverlap);
            //    Log.WriteLine("Resample rate: {0} samples/sec.  Nyquist: {1} Hz.", paramaters.resampleRate, (paramaters.resampleRate / 2));
            //    Log.WriteLine("Frame Length: {0} samples.  Fractional overlap: {1}.", paramaters.frameLength, paramaters.frameOverlap);
            //    Log.WriteLine("####################################################################################");
            //}
            return p;
        }


        public void SetUpPanels()
        {

            //this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            //this.Button1 = new System.Windows.Forms.Button();
            //this.Button2 = new System.Windows.Forms.Button();
            //this.Button3 = new System.Windows.Forms.Button();
            //this.Button4 = new System.Windows.Forms.Button();
            //this.wrapContentsCheckBox = new System.Windows.Forms.CheckBox();
           // this.flowTopDownBtn = new System.Windows.Forms.RadioButton();
           // this.flowBottomUpBtn = new System.Windows.Forms.RadioButton();
           // this.flowLeftToRight = new System.Windows.Forms.RadioButton();
           // this.flowRightToLeftBtn = new System.Windows.Forms.RadioButton();
           // this.FlowLayoutPanel1.SuspendLayout();
           // this.SuspendLayout();

            int visualIndex_TrackCount  = parameters.trackCount + 2; //+ 2 time scale tracks
            int visualIndex_TrackHeight = parameters.trackHeight;
            int visualIndex_ImageHeight = visualIndex_TrackCount * visualIndex_TrackHeight;

            int windowWidth     = 1810;
            int windowHeight    = 750;
            int leftPanelWidth  = 210;
            int leftPanelHeight = windowHeight;
            int rightPanelWidth = windowWidth - leftPanelWidth;
            int rightPanelHeight = windowHeight;

            int indexPanelWidth = rightPanelWidth;
            int indexPanelHeight = visualIndex_ImageHeight;
            int barTrackPanelWidth  = rightPanelWidth;
            int barTrackPanelHeight = visualIndex_TrackHeight;
            int sonogramPanelWidth = rightPanelWidth;
            int sonogramPanelHeight = windowHeight - indexPanelHeight;


            visualIndex.Width = indexPanelWidth;
            visualIndex.Height = indexPanelHeight;



            this.Size = new System.Drawing.Size(windowWidth, windowHeight);
            this.SuspendLayout();

            // Set color, location and size of the leftPanel panel
            this.leftPanel.SuspendLayout();
            //leftPanel.BackColor = Color.Black;
            leftPanel.Location = new Point(0, 0);
            this.leftPanel.Size = new System.Drawing.Size(leftPanelWidth, leftPanelHeight);
            //Point pt1 = new Point(this.leftPanel.Width-10, 0);
            //Point pt2 = new Point(this.leftPanel.Size.Width-10, this.leftPanel.Size.Height);
            //Graphics gr = this.leftPanel.CreateGraphics();
            //gr.DrawLine(new Pen(Color.Red, 2.0F), pt1, pt2);

            // Set color, location and size of the leftPanel panel
            //rightPanel.BackColor = Color.White;
            rightPanel.Location = new Point(leftPanelWidth+1, 0);
            rightPanel.Size = new System.Drawing.Size(rightPanelWidth, rightPanelHeight);
            //rightPanel.Paint += PaintLineOnPanel;

            // Set color, location and size of the indexPanel panel
            indexPanel.BackColor = Color.Black;
            indexPanel.Location = new Point(0, 0);
            indexPanel.Size = new Size(rightPanelWidth, indexPanelHeight);

            // Set color, location and size of the barTrackPanel panel
            barTrackPanel.BackColor = Color.BlanchedAlmond;
            barTrackPanel.Location = new Point(0, indexPanelHeight+1);
            barTrackPanel.Size = new Size(barTrackPanelWidth, barTrackPanelHeight);


            // Set color, location and size of the sonogramPanel panel
            sonogramPanel.BackColor = Color.DarkGray;
            sonogramPanel.Location = new Point(0, indexPanelHeight + barTrackPanelHeight+1);
            sonogramPanel.Size = new Size(rightPanelWidth, sonogramPanelHeight);
            sonogramPanel.Controls.Add(this.sonogramPanel_hScrollBar);
            this.sonogramPanel_hScrollBar.LargeChange = 240;
            this.sonogramPanel_hScrollBar.Size = new System.Drawing.Size(sonogramPanel.Width, 13);
            this.sonogramPanel_hScrollBar.ValueChanged += new System.EventHandler(this.sonogramPanel_hScrollBar_ValueChanged);
            this.sonogramPanel_hScrollBar.Visible = false;




            // That's the point, container controls exposes the Controls collection that you could use to add controls programatically
            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);
            rightPanel.Controls.Add(indexPanel);
            rightPanel.Controls.Add(barTrackPanel);
            rightPanel.Controls.Add(sonogramPanel);

            this.leftPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right);
            //this.leftPanel.AutoScroll = true;
            this.leftPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.visualIndex.MouseMove += new MouseEventHandler(this.image_MouseMove);
            this.visualIndex.MouseClick += new MouseEventHandler(this.image_MouseClick);
            this.visualIndex.MouseHover += new System.EventHandler(this.image_MouseHover);
            
        }



        public void InitaliseLeftPanelControls()
        {
            int labelheight = 14;
            int pixelBuffer = 10;
            this.CSVDir_Label = new System.Windows.Forms.Label();
            this.CSVDir_Label.Text = "Directory containing CSV file";
            this.CSVDir_Label.Size = new System.Drawing.Size(this.leftPanel.Width - pixelBuffer, labelheight);

            this.time_Label = new System.Windows.Forms.Label();
            this.time_Label.Text = "Cursor location";
            this.time_Label.Size = new System.Drawing.Size(this.leftPanel.Width - pixelBuffer, labelheight);

            this.recordingFileName_Label = new System.Windows.Forms.Label();
            this.recordingFileName_Label.Text = "Name of source audio file";
            this.recordingFileName_Label.Size = new System.Drawing.Size(this.leftPanel.Width - pixelBuffer, labelheight);

            this.segmentName_Label = new System.Windows.Forms.Label();
            this.segmentName_Label.Text = "Name of extracted audio file";
            this.segmentName_Label.Size = new System.Drawing.Size(this.leftPanel.Width - pixelBuffer, labelheight);

            this.recordingDir_Label = new System.Windows.Forms.Label();
            this.recordingDir_Label.Text = "Directory of source audio files";
            this.recordingDir_Label.Size = new System.Drawing.Size(this.leftPanel.Width - pixelBuffer, labelheight);

            this.outputDir_Label = new System.Windows.Forms.Label();
            this.outputDir_Label.Text = "Directory for output";
            this.outputDir_Label.Size = new System.Drawing.Size(this.leftPanel.Width - pixelBuffer, labelheight);

            this.message_Label = new System.Windows.Forms.Label();
            this.message_Label.Text = "Messages and warnings!";
            this.message_Label.Size = new System.Drawing.Size(this.leftPanel.Width - pixelBuffer, labelheight);


            this.loadIndicesButton = new System.Windows.Forms.Button();
            this.loadIndicesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.loadIndicesButton.Name = "Load Index File";
            this.loadIndicesButton.Size = new System.Drawing.Size(120, 24);
            this.loadIndicesButton.TabIndex = 0;
            this.loadIndicesButton.Text = "Load Index File";
            this.loadIndicesButton.Click += new System.EventHandler(this.loadIndicesButton_Click);

            this.audacityButton = new System.Windows.Forms.Button();
            this.audacityButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.audacityButton.Name = "Audacity";
            this.audacityButton.Size = new System.Drawing.Size(150, 24);
            this.audacityButton.TabIndex = 0;
            this.audacityButton.Text = "Load Extract into Audacity";
            this.audacityButton.Click += new System.EventHandler(this.audacityButton_Click);


            this.CSVDir_TextBox = new TextBox();
            this.CSVDir_TextBox.Width = this.leftPanel.Width - pixelBuffer;
                        
            this.recordingDir_TextBox = new TextBox();
            this.recordingDir_TextBox.Width = this.leftPanel.Width - pixelBuffer;

            this.recordingFileName_TextBox = new TextBox();
            this.recordingFileName_TextBox.Width = this.leftPanel.Width - pixelBuffer;


            this.outputDir_TextBox = new TextBox();
            this.outputDir_TextBox.Width = this.leftPanel.Width - pixelBuffer;

            this.time_TextBox = new TextBox();
            this.time_TextBox.Width = this.leftPanel.Width - pixelBuffer;

            this.segmentName_TextBox = new TextBox();
            this.segmentName_TextBox.Width = this.leftPanel.Width - pixelBuffer;

            this.message_TextBox = new TextBox();
            this.message_TextBox.Width = this.leftPanel.Width - pixelBuffer;
            this.message_TextBox.Height = 100;
            this.message_TextBox.Multiline = true;
            this.message_TextBox.WordWrap = true;
            //this.message_TextBox.Text = @"ddddddddd ddddddddd ddddddddddd ddddd dddddddd dddddddddd ddddd dddddd ddddddd";


            //LOCATE ALL CONTROLS IN LEFT PANEL
            int yOffset = 10;
            int yGap    = 10;
            this.CSVDir_Label.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = CSVDir_Label.Bottom + 1;
            this.CSVDir_TextBox.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = CSVDir_TextBox.Bottom + yGap;

            this.recordingDir_Label.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = recordingDir_Label.Bottom + 1;
            this.recordingDir_TextBox.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = recordingDir_TextBox.Bottom + yGap;

            this.recordingFileName_Label.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = recordingFileName_Label.Bottom + 1;
            this.recordingFileName_TextBox.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = recordingFileName_TextBox.Bottom + yGap;

            this.outputDir_Label.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = outputDir_Label.Bottom + 1;
            this.outputDir_TextBox.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = outputDir_TextBox.Bottom + yGap + yGap;

            this.loadIndicesButton.Location = new System.Drawing.Point(pixelBuffer+ 6, yOffset);
            yOffset = loadIndicesButton.Bottom + yGap;

            this.time_Label.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = time_Label.Bottom + 1;
            this.time_TextBox.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = time_TextBox.Bottom + yGap;

            this.segmentName_Label.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = segmentName_Label.Bottom + 1;
            this.segmentName_TextBox.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = segmentName_TextBox.Bottom + yGap + yGap;

            this.audacityButton.Location = new System.Drawing.Point(pixelBuffer + 6, yOffset);
            yOffset = audacityButton.Bottom + yGap + yGap + yGap + yGap;

            this.message_Label.Location = new System.Drawing.Point(pixelBuffer, yOffset);
            yOffset = message_Label.Bottom + 1;
            this.message_TextBox.Location = new System.Drawing.Point(pixelBuffer, yOffset);



            //segmentName_TextBox.Top = 200;
            //segmentName_TextBox.Left = 10;
            this.leftPanel.Controls.AddRange(new System.Windows.Forms.Control[] { 
                this.CSVDir_Label,
                this.CSVDir_TextBox,
                this.recordingDir_Label,
                this.recordingDir_TextBox,
                this.recordingFileName_Label,
                this.recordingFileName_TextBox,
                this.outputDir_Label,
                this.outputDir_TextBox,
                this.time_Label, 
                this.time_TextBox, 
                this.segmentName_Label, 
                this.segmentName_TextBox, 
                this.loadIndicesButton, 
                this.audacityButton, 
                this.message_Label,
                this.message_TextBox,
            });


            //insert path values into appropriate text boxes. This should be done by dialog boxes if these worked!
            this.CSVDir_TextBox.Text = parameters.csvPath;
            this.recordingDir_TextBox.Text = Path.GetDirectoryName(parameters.recordingPath);
            this.recordingFileName_TextBox.Text = Path.GetFileName(parameters.recordingPath);
            this.outputDir_TextBox.Text = parameters.outputDir;
        }

        private void loadIndicesButton_Click(object sender, EventArgs e)
        {
            // Wrap the creation of the OpenFileDialog instance in a using statement to ensure proper disposal
            //using (OpenFileDialog dlg = new OpenFileDialog())
            //{
            //    dlg.Title = "Open Visual Index";
            //    dlg.Filter = "png files (*.png)|*.png";

            //    if (dlg.ShowDialog() == DialogResult.OK)
            //    {
            //        PictureBox PictureBox1 = new PictureBox();

            //        // Create a new Bitmap object from the picture file on disk,
            //        // and assign that to the PictureBox.Image property
            //        PictureBox1.Image = new Bitmap(dlg.FileName);
            //        // Add the new control to its parent's controls collection
            //        indexPanel.Controls.Add(PictureBox1);
            //    }
            //}

            //USE FOLLOWING THREE LINES TO LOAD A PNG IMAGE
            //visualIndex.Image = new Bitmap(parameters.visualIndexPath);
            //visualIndex.Dock = DockStyle.Fill;
            //indexPanel.Controls.Add(visualIndex);

            //USE FOLLOWING LINES TO LOAD A CSV FILE
            var tuple = FileTools.ReadCSVFile(parameters.csvPath);
            var headers = tuple.Item1;  //List<string>
            var values  = tuple.Item2;  //List<double[]>> 

            visualIndex.Image = ConstructIndexImage(headers, values, indexPanel.Width, parameters.trackHeight);
            visualIndex.Dock = DockStyle.Fill;
            indexPanel.Controls.Add(visualIndex);

            this.minutesDuration = values[0].Length; //time in minutes
            this.visualIndexTimeScale = (Bitmap)DrawVisualIndexTimeScale(this.minutesDuration, indexPanel.Width, parameters.trackHeight);
        }//loadIndicesButton_Click


        public static Bitmap ConstructIndexImage(List<string> headers, List<double[]> values, int imageWidth, int trackHeight)
        {
            int headerCount = headers.Count;
            bool[] displayColumn = { false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
            double threshold = 0.5;

            int trackCount = DataTools.CountTrues(displayColumn) +3; //+2 for top and bottom time tracks
            int imageHt = trackHeight * trackCount;
            int duration = values[0].Length; //time in minutes
            int offset = 0;
            Bitmap timeBmp = (Bitmap)DrawVisualIndexTimeScale(duration, imageWidth, trackHeight);

            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt);
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.DrawImage(timeBmp, 0, offset);


            offset += trackHeight;
            for (int i = 0; i < displayColumn.Length; i++) //for pixels in the line
            {
                if (!displayColumn[i]) continue;
                if(i >= headerCount) break;
                Bitmap bmp = Image_Track.DrawBarScoreTrack(values[i], trackHeight, threshold, headers[i]);
                gr.DrawImage(bmp, 0, offset);
                //var font = new Font("Tahoma", 9);
                //Font = New Font(Me.Font, FontStyle.Bold);
                var font = SystemFonts.IconTitleFont;
                gr.DrawString(headers[i], font, Brushes.White, new PointF(duration + 5, offset));
                offset += trackHeight;
            }
            gr.DrawImage(timeBmp, 0, offset);
            return compositeBmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration">length of the time track in pixels - 1 pixel=1minute</param>
        /// <param name="imageWidth"></param>
        /// <param name="trackHeight"></param>
        /// <returns></returns>
        private static Image DrawVisualIndexTimeScale(int duration, int imageWidth, int trackHeight)
        {
            int scale = 60; //put a tik every 60 pixels = 1 hour
            return Image_Track.DrawTimeTrack(duration, scale, imageWidth, trackHeight, "Time (hours)");
        } //DrawVisualIndexTimeScale()




        private void audacityButton_Click(object sender, EventArgs e)
        {
            OpenAudacity(this.recordingSegmentPath);
        }//audacityButton_Click()




        /// <summary>
        /// Change the cursor image to cross hairs when over the visual index image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseHover(object sender, EventArgs e)
        {
            visualIndex.Cursor = Cursors.HSplit;
        }

        private void image_MouseMove(object sender, EventArgs e)
        {
            int myX = Form.MousePosition.X - rightPanel.Left - this.Left - 8; //8 = border width - must be better way to do this!!!!!
            if (myX > this.minutesDuration) return;

            string text = (myX / 60) + "hr:" + (myX % 60) + "min (" + myX + ")"; //assumes scale= 1 pixel / minute
            this.time_TextBox.Text = text; // pixel position = minutes

            //mark the time scale
            Graphics g = visualIndex.CreateGraphics();
            g.DrawImage(this.visualIndexTimeScale, 0, 0);
            Point pt1 = new Point(this.visualIndex.Left + myX, 2);
            Point pt2 = new Point(this.visualIndex.Left + myX, parameters.trackHeight-1);
            g.DrawLine(new Pen(Color.Yellow, 1.0F), pt1, pt2);
            g.DrawImage(this.visualIndexTimeScale, 0, this.visualIndex.Height - parameters.trackHeight);
            pt1 = new Point(this.visualIndex.Left + myX, this.visualIndex.Height - 2);
            pt2 = new Point(this.visualIndex.Left + myX, this.visualIndex.Height - parameters.trackHeight);
            g.DrawLine(new Pen(Color.Yellow, 1.0F), pt1, pt2);
        } //image_MouseMove()



        /// <summary>
        /// ACTION when mouse is clicked over the VISUAL INDEX IMAGE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseClick(object sender, MouseEventArgs e)
        {
            // SHOW MOUSE LOCATION IN INFO BOX
            //this.segmentName_TextBox.Text = "WAIT!";
            int myX = e.X;
            int myY = e.Y;
            Point pt1 = new Point(this.visualIndex.Left + myX, 0);
            Point pt2 = new Point(this.visualIndex.Left + myX, this.barTrackPanel.Height);
            Graphics g = barTrackPanel.CreateGraphics();
            g.DrawLine(new Pen(Color.Red, 1.0F), pt1, pt2);


            //EXTRACT RECORDING SEGMENT
            int startMilliseconds = (myX) * 60000;
            int endMilliseconds   = (myX + 1) * 60000;
            if (parameters.segmentDuration == 3)
            {
                startMilliseconds = (myX - 1) * 60000;
                endMilliseconds   = (myX + 2) * 60000;
            }
            if (startMilliseconds < 0) startMilliseconds = 0;


            string fName = Path.GetFileNameWithoutExtension(parameters.recordingPath);
            string segmentName = fName + "_min"+myX.ToString() + ".wav"; //want a wav file
            string outputSegmentPath = Path.Combine(parameters.outputDir, segmentName); //path name of the segment file extracted from long recording
            AudioRecording recording = AudioRecording.GetSegmentFromAudioRecording(parameters.recordingPath, startMilliseconds, endMilliseconds, parameters.resampleRate, outputSegmentPath);

            //store info
            this.segmentName_TextBox.Text = Path.GetFileName(recording.FilePath);
            this.recordingSegmentPath = recording.FilePath;

            //make the sonogram
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName   = recording.FileName;
            sonoConfig.WindowSize    = parameters.frameLength;
            sonoConfig.WindowOverlap = parameters.frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());

            // (iii) NOISE REDUCTION
            var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, parameters.sonogram_BackgroundThreshold);
            sonogram.Data = tuple.Item1;   // store data matrix

            //prepare the image
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                if (sonogramPicture != null) sonogramPicture.Dispose(); //get rid of previous sonogram
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                sonogramPicture = new PictureBox();
                sonogramPicture.Image = image.GetImage();
                sonogramPicture.SetBounds(0, 0, sonogramPicture.Image.Width, sonogramPicture.Image.Height);
                this.sonogramPanel.Controls.Add(sonogramPicture);
                this.sonogramPanel_hScrollBar.Location = new System.Drawing.Point(0, img.Height+15);
                this.sonogramPanel_hScrollBar.Maximum = img.Width - this.sonogramPanel.Width + 260;  // PROBLEM WITH THIS CODE - 260 = FIDDLE FACTOR!!!  ORIGINAL WAS -this.ClientSize.Width;
                this.sonogramPanel_hScrollBar.Value = 0;
                this.sonogramPanel_hScrollBar.Visible = true;
            }
        } //image_MouseClick()


        /// <summary>
        /// Opens Audacity but NOT in separate thread.
        /// </summary>
        /// <param name="recordingPath"></param>
        private void OpenAudacity(string recordingPath)
        {
            string audacityDir = Path.GetDirectoryName(parameters.AudacityPath);
            DirectoryInfo dirInfo = new DirectoryInfo(audacityDir); 
            string appName = Path.GetFileName(parameters.AudacityPath);
            ProcessRunner process = new ProcessRunner(dirInfo, appName, recordingPath);
            process.Start();
            var consoleOutput = process.OutputData;
            var errorData     = process.ErrorData;
        }



        private void sonogramPanel_hScrollBar_ValueChanged(object sender, System.EventArgs e)
        {
            this.sonogramPicture.Left = -this.sonogramPanel_hScrollBar.Value;
            // Display the current values in the title bar.
            //this.segmentName_TextBox.Text = "x = " + this.sonogramPanel_hScrollBar.Value;
        }


        //public void setUpRadioButtons ()
        //{
        //    langPanel.Location = new Point(0,0);
        //    langPanel.AutoSize = true;

        //    radBritish.Location = new Point(0,0);
        //    radBritish.AutoSize = true;
        //    radBritish.Text = "English";
        //    radBritish.Checked = true;

        //    radFrench.Location = new Point(radBritish.Width,0);
        //    radFrench.AutoSize = true;
        //    radFrench.Text = "French";
        //    radSpanish.Location = new Point(radBritish.Width + radFrench.Width,0);
        //    radSpanish.AutoSize = true;
        //    radSpanish.Text = "Spanish";
        //    //lblDay.Location = new Point(0,radBritish.Height);
        //    //lblDay.AutoSize = true;

        //    //And finally the programmer must add the components to the form:
        //    langPanel.Controls.Add(radBritish);
        //    langPanel.Controls.Add(radFrench);
        //    langPanel.Controls.Add(radSpanish);
        //    this.Controls.Add(langPanel);
        //} //setUpRadioButtons


        //private void PaintLineOnPanel(object sender, PaintEventArgs e)
        //{
            // center the line endpoints on each button
            //Point pt1 = new Point(button1.Left + (button1.Width / 2), button1.Top + (button1.Height / 2));
            //Point pt2 = new Point(button2.Left + (button2.Width / 2), button2.Top + (button2.Height / 2));
            //if (sender is Button)
            //{
            //    // offset line so it's drawn over the button where
            //    // the line on the panel is drawn
            //    Button btn = (Button)sender;
            //    pt1.X -= btn.Left;
            //    pt1.Y -= btn.Top;
            //    pt2.X -= btn.Left;
            //    pt2.Y -= btn.Top;
            //}

        //    Point pt1 = new Point(this.visualIndex.Left + 100, 20);
        //    Point pt2 = new Point(this.visualIndex.Left + 100, 100);
        //    e.Graphics.DrawLine(new Pen(Color.Red, 4.0F), pt1, pt2);
        //}




   } //AudioBrowser


}
