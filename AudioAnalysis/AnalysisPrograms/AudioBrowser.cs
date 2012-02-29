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

    class AudioBrowser : Form 
    {


        /// <summary>
        /// a set of parameters derived from ini file
        /// </summary>
        public struct Parameters
        {
            public int frameLength, resampleRate, segmentDuration;
            public double frameOverlap;
            public int trackHeight , DRAW_SONOGRAMS;
            public string csvPath, visualIndexPath, recordingPath, outputDir, AudacityPath;

            public Parameters(string dummy)
            {
                segmentDuration = 1;
                resampleRate = 17640;
                frameLength = 512;
                frameOverlap = 0.0;
                //string workingDir = null;

                csvPath         = @"C:\SensorNetworks\WavFiles\SunshineCoast\AcousticIndices_DM420062_WeightedIndices.csv";
                visualIndexPath = @"C:\SensorNetworks\WavFiles\SunshineCoast\AcousticIndices_DM420062_WeightedIndices.png";
                recordingPath   = @"Z:\Site 4\DM420062.mp3";
                outputDir       = @"C:\SensorNetworks\WavFiles\SunshineCoast\";
                AudacityPath    = @"C:\Program Files (x86)\Audacity 1.3 Beta (Unicode)\audacity.exe";

                DRAW_SONOGRAMS = 0;
                trackHeight = 20;
            } //Parameters
        } //struct Parameters
        Parameters parameters;




        private Panel leftPanel = new Panel();
        private Panel rightPanel = new Panel();
        private Panel indexPanel = new Panel();
        private Panel barTrackPanel = new Panel();
        private Panel sonogramPanel = new Panel();
        private HScrollBar sonogramPanel_hScrollBar = new HScrollBar();
        PictureBox indexPicture;
        PictureBox sonogramPicture;
        string recordingSegmentPath;


        internal Button loadIndicesButton;
        internal Button audacityButton;
        internal TextBox time_TextBox;
        internal TextBox segmentName_TextBox;



        PictureBox visualIndex = new PictureBox();


        private Panel langPanel = new Panel();
        //private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
        private RadioButton radBritish = new RadioButton();
        private RadioButton radFrench = new RadioButton();
        private RadioButton radSpanish = new RadioButton();



        public static void Main() 
        {
            string exeDir = System.Environment.CurrentDirectory;
            string[] args = System.Environment.GetCommandLineArgs();
            string iniPath = exeDir+"\\AudioBrowser.ini";
            if(args.Length == 1) iniPath = args[0];

            Application.Run(new AudioBrowser(iniPath));
        }

        public AudioBrowser(string iniPath) //constructor
        {
            int verbosity = 0;
            //AcousticIndices.Parameters parameters = AcousticIndices.ReadIniFile(iniPath, verbosity);

            string dummy = "";
            parameters = new Parameters(dummy);

            initaliseButtons();
            setUpPanels();
            //setUpRadioButtons();
 

        } // MainForm


        public void setUpPanels()
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

            int visualIndex_TrackCount  = 17;
            int visualIndex_TrackHeight = 20;
            int visualIndex_ImageHeight = visualIndex_TrackCount * visualIndex_TrackHeight;

            int windowWidth = 1760;
            int windowHeight = 750;
            int leftPanelWidth = 200;
            int leftPanelHeight = windowHeight;
            int rightPanelWidth = windowWidth - leftPanelWidth;
            int rightPanelHeight = windowHeight;

            int indexPanelWidth = rightPanelWidth;
            int indexPanelHeight = visualIndex_ImageHeight;
            int barTrackPanelWidth  = rightPanelWidth;
            int barTrackPanelHeight = visualIndex_TrackHeight;
            int sonogramPanelWidth = rightPanelWidth;
            int sonogramPanelHeight = windowHeight/2;


            visualIndex.Width = indexPanelWidth;
            visualIndex.Height = indexPanelHeight;



            this.Size = new System.Drawing.Size(windowWidth, windowHeight);
            this.SuspendLayout();

            // Set color, location and size of the leftPanel panel
            this.leftPanel.SuspendLayout();
            leftPanel.BackColor = Color.Black;
            leftPanel.Location = new Point(0, 0);
            this.leftPanel.Size = new System.Drawing.Size(leftPanelWidth, leftPanelHeight);

            // Set color, location and size of the leftPanel panel
            rightPanel.BackColor = Color.White;
            rightPanel.Location = new Point(leftPanelWidth, 0);
            rightPanel.Size = new System.Drawing.Size(rightPanelWidth, rightPanelHeight);
            rightPanel.Paint += PaintLineOnPanel;

            // Set color, location and size of the indexPanel panel
            indexPanel.BackColor = Color.LightGray;
            indexPanel.Location = new Point(0, 0);
            indexPanel.Size = new Size(rightPanelWidth, indexPanelHeight);

            // Set color, location and size of the barTrackPanel panel
            barTrackPanel.BackColor = Color.BlanchedAlmond;
            barTrackPanel.Location = new Point(0, indexPanelHeight);
            barTrackPanel.Size = new Size(barTrackPanelWidth, barTrackPanelHeight);


            // Set color, location and size of the sonogramPanel panel
            sonogramPanel.BackColor = Color.DarkGray;
            sonogramPanel.Location = new Point(0, indexPanelHeight + barTrackPanelHeight);
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
            //rightPanel.Controls.Add(barTrackPanel);
            rightPanel.Controls.Add(sonogramPanel);

            this.leftPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right);
            this.leftPanel.AutoScroll = true;
            this.leftPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.leftPanel.Controls.AddRange(new System.Windows.Forms.Control[] { this.loadIndicesButton, this.audacityButton, this.time_TextBox, this.segmentName_TextBox});


            //this.visualIndex.MouseHover += new System.EventHandler(this.image_MouseHover);
            this.visualIndex.MouseMove += new MouseEventHandler(this.image_MouseMove);
            this.visualIndex.MouseClick += new MouseEventHandler(this.image_MouseClick);
            this.visualIndex.MouseHover += new System.EventHandler(this.image_MouseHover);
            
        }



        public void initaliseButtons()
        {
            this.loadIndicesButton = new System.Windows.Forms.Button();
            this.loadIndicesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.loadIndicesButton.Location = new System.Drawing.Point(16, 20);
            this.loadIndicesButton.Name = "Load Index File";
            this.loadIndicesButton.Size = new System.Drawing.Size(120, 24);
            this.loadIndicesButton.TabIndex = 0;
            this.loadIndicesButton.Text = "Load Index File";
            this.loadIndicesButton.Click += new System.EventHandler(this.loadIndicesButton_Click);


            this.audacityButton = new System.Windows.Forms.Button();
            this.audacityButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.audacityButton.Location = new System.Drawing.Point(16, 300);
            this.audacityButton.Name = "Audacity";
            this.audacityButton.Size = new System.Drawing.Size(120, 24);
            this.audacityButton.TabIndex = 0;
            this.audacityButton.Text = "Load in Audacity";
            this.audacityButton.Click += new System.EventHandler(this.audacityButton_Click);


            this.time_TextBox = new TextBox();
            time_TextBox.Top = 100;
            time_TextBox.Left = 10;

            this.segmentName_TextBox = new TextBox();
            segmentName_TextBox.Top = 200;
            segmentName_TextBox.Left = 10;
            
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
            
        }//loadIndicesButton_Click


        public static Bitmap ConstructIndexImage(List<string> headers, List<double[]> values, int imageWidth, int trackHeight)
        {
            int headerCount = headers.Count;
            bool[] displayColumn = { false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
            double threshold = 0.5;

            int trackCount = DataTools.CountTrues(displayColumn) +3; //+2 for top and bottom time tracks
            int imageHt = trackHeight * trackCount;
            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt);
            Graphics gr = Graphics.FromImage(compositeBmp);
            int duration = 1436; //time in minutes
            int scale = 60;
            int offset = 0;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(duration, scale, imageWidth, trackHeight, "Time (hours)");
            gr.DrawImage(timeBmp, 0, offset);


            offset += trackHeight;
            for (int i = 0; i < displayColumn.Length; i++) //for pixels in the line
            {
                if (!displayColumn[i]) continue;
                if(i >= headerCount) break;
                Bitmap bmp = Image_Track.DrawBarScoreTrack(values[i], trackHeight, threshold, headers[i]);
                gr.DrawImage(bmp, 0, offset);
                gr.DrawString(headers[i], new Font("Arial", 9), Brushes.Black, new PointF(duration + 5, offset));
                offset += trackHeight;
            }
            gr.DrawImage(timeBmp, 0, offset);
            return compositeBmp;
        }




        private void audacityButton_Click(object sender, EventArgs e)
        {
            OpenAudacity(this.recordingSegmentPath);
        }//audacityButton_Click





        private void image_MouseHover(object sender, EventArgs e)
        {
            visualIndex.Cursor = Cursors.Cross;
        }

        private void image_MouseMove(object sender, EventArgs e)
        {
            int myX = Form.MousePosition.X - rightPanel.Left - this.Left - 8; //8 = border width - must be better way to do this!!!!!
            string text = (myX / 60) + "hr:" + (myX % 60) + "min (" + myX + ")";
            this.time_TextBox.Text = text; // pixel position = minutes

            //show mouse position in barGraph
            //Graphics g = barTrackPanel.CreateGraphics();
            //Point pt1 = new Point(this.barTrackPanel.Left + myX, this.barTrackPanel.Top);
            //Point pt2 = new Point(this.barTrackPanel.Left + myX + 100, this.barTrackPanel.Bottom);
            //g.DrawLine(new Pen(Color.Red, 2.0F), pt1, pt2);

        }

        private void image_MouseClick(object sender, MouseEventArgs e)
        {
            int myX = e.X;
            int myY = e.Y;


            Point pt1 = new Point(this.visualIndex.Left + myX, this.rightPanel.Top);
            Point pt2 = new Point(this.visualIndex.Left + myX, this.rightPanel.Bottom - 2);
            Graphics g = rightPanel.CreateGraphics();

            //Point pt1 = new Point(this.visualIndex.Left + myX, this.rightPanel.Top);
            //Point pt2 = new Point(this.visualIndex.Left + myX, this.rightPanel.Bottom);
            //Graphics g = barTrackPanel.CreateGraphics();

            g.DrawLine(new Pen(Color.Red, 2.0F), pt1, pt2);


            //double startHr = 2.0;
            //double endHr   = 2.2;
            int startMilliseconds = (myX) * 60000;
            int endMilliseconds   = (myX + 1) * 60000;
            if (parameters.segmentDuration == 3)
            {
                startMilliseconds = (myX - 1) * 60000;
                endMilliseconds   = (myX + 2) * 60000;
            }
            if (startMilliseconds < 0) startMilliseconds = 0;
            //Console.WriteLine("\nWAIT - extracting segment!");


            //string recordingPath = @"C:\SensorNetworks\WavFiles\SunshineCoast\AcousticIndices_browserTemp.wav";
            //this.segmentFileName.Text = "WAIT!";
            AudioRecording recording = AudioRecording.GetSegmentFromAudioRecording(parameters.recordingPath, startMilliseconds, endMilliseconds, parameters.resampleRate, parameters.outputDir);
            this.segmentName_TextBox.Text = recording.FileName;

            this.recordingSegmentPath = recording.FilePath;

            //make the sonogram
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName   = recording.FileName;
            sonoConfig.WindowSize    = parameters.frameLength;
            sonoConfig.WindowOverlap = parameters.frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());

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
                this.sonogramPanel_hScrollBar.Maximum = img.Width + this.ClientSize.Width;
                this.sonogramPanel_hScrollBar.Value = 0;
                this.sonogramPanel_hScrollBar.Visible = true;
            }
        }


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


        private void PaintLineOnPanel(object sender, PaintEventArgs e)
        {
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

            Point pt1 = new Point(this.visualIndex.Left + 100, 20);
            Point pt2 = new Point(this.visualIndex.Left + 100, 100);
            e.Graphics.DrawLine(new Pen(Color.Red, 4.0F), pt1, pt2);
        }


        private void sonogramPanel_hScrollBar_ValueChanged(object sender, System.EventArgs e)
        {
            //this.sonogramPicture.Left = -this.sonogramPanel_hScrollBar.Value;
            this.sonogramPicture.Left = -this.sonogramPanel_hScrollBar.Value;
            //this.sonogramPicture.Left = -5000;

            // Display the current values in the title bar.
            //this.segmentFileName.Text = "x = " + this.panel1.Location.X + ", y = " + this.panel1.Location.Y;
            this.segmentName_TextBox.Text = "x = " + this.sonogramPanel_hScrollBar.Value;
        }


        public void setUpRadioButtons ()
        {
            langPanel.Location = new Point(0,0);
            langPanel.AutoSize = true;

            radBritish.Location = new Point(0,0);
            radBritish.AutoSize = true;
            radBritish.Text = "English";
            radBritish.Checked = true;

            radFrench.Location = new Point(radBritish.Width,0);
            radFrench.AutoSize = true;
            radFrench.Text = "French";
            radSpanish.Location = new Point(radBritish.Width + radFrench.Width,0);
            radSpanish.AutoSize = true;
            radSpanish.Text = "Spanish";
            //lblDay.Location = new Point(0,radBritish.Height);
            //lblDay.AutoSize = true;

            //And finally the programmer must add the components to the form:
            langPanel.Controls.Add(radBritish);
            langPanel.Controls.Add(radFrench);
            langPanel.Controls.Add(radSpanish);
            this.Controls.Add(langPanel);
        } //setUpRadioButtons





   } //AudioBrowser


}
