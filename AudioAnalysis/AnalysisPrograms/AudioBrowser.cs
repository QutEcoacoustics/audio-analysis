using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using AudioTools.AudioUtlity;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    class AudioBrowser : Form 
    {

        private Panel leftPanel = new Panel();
        private Panel rightPanel = new Panel();
        private Panel indexPanel = new Panel();
        private Panel sonogramPanel = new Panel();

        internal Button Button1;
        internal TextBox timePosition;
        internal TextBox segmentFileName;



        PictureBox visualIndex = new PictureBox();


        private Panel langPanel = new Panel();
        //private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
        private RadioButton radBritish = new RadioButton();
        private RadioButton radFrench = new RadioButton();
        private RadioButton radSpanish = new RadioButton();


        int resampleRate = 0;
        string workingDir = null;
        int frameLength = 256;
        double frameOverlap = 0.0;



        public static void Main() 
        {
            Application.Run(new AudioBrowser());
        }

        public AudioBrowser() //constructor
        {
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

            int windowWidth = 1760;
            int windowHeight = 750;
            int leftPanelWidth = 200;
            int leftPanelHeight = windowHeight;
            int rightPanelWidth = windowWidth - leftPanelWidth;
            int rightPanelHeight = windowHeight;
            int indexPanelWidth = rightPanelWidth;
            int indexPanelHeight = windowHeight/2;
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

            // Set color, location and size of the sonogramPanel panel
            sonogramPanel.BackColor = Color.DarkGray;
            sonogramPanel.Location = new Point(0, indexPanelHeight);
            sonogramPanel.Size = new Size(rightPanelWidth, sonogramPanelHeight);

            // That's the point, container controls exposes the Controls
            // collection that you could use to add controls programatically
            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);
            rightPanel.Controls.Add(indexPanel);
            rightPanel.Controls.Add(sonogramPanel);

            this.leftPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right);
            this.leftPanel.AutoScroll = true;
            this.leftPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.leftPanel.Controls.AddRange(new System.Windows.Forms.Control[] { this.Button1, this.timePosition, this.segmentFileName});


            //this.visualIndex.MouseHover += new System.EventHandler(this.image_MouseHover);
            this.visualIndex.MouseMove += new MouseEventHandler(this.image_MouseMove);
            this.visualIndex.MouseClick += new MouseEventHandler(this.image_MouseClick);
        }



        public void initaliseButtons()
        {
            this.Button1 = new System.Windows.Forms.Button();
            this.Button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Button1.Location = new System.Drawing.Point(16, 20);
            this.Button1.Name = "Load Index File";
            this.Button1.Size = new System.Drawing.Size(120, 24);
            this.Button1.TabIndex = 0;
            this.Button1.Text = "Load Index File";
            this.Button1.Click += new System.EventHandler(this.button1_Click);

            this.timePosition = new TextBox();
            timePosition.Top = 100;
            timePosition.Left = 10;

            this.segmentFileName = new TextBox();
            segmentFileName.Top = 200;
            segmentFileName.Left = 10;
            
        }

        private void button1_Click(object sender, EventArgs e)
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

            string fileName = @"C:\SensorNetworks\WavFiles\SunshineCoast\AcousticIndices_DM420062_WeightedIndices.png";
            visualIndex.Image = new Bitmap(fileName);
            visualIndex.Dock = DockStyle.Fill;
            indexPanel.Controls.Add(visualIndex);
        }//button1_Click


        //private void image_MouseHover(object sender, EventArgs e)
       // {
        //    int myX = Form.MousePosition.X;
        //    //int myX = e.X;
        //    //int myY = e.Y;
        //    //this.visualIndex.
        //    this.timePosition.Text = myX.ToString(); // pixel position = minutes
        //}

        private void image_MouseMove(object sender, EventArgs e)
        {
            int myX = Form.MousePosition.X - rightPanel.Left - this.Left - 8; //8 = border width - must be better way to do this!!!!!
            this.timePosition.Text = myX.ToString(); // pixel position = minutes
        }

        private void image_MouseClick(object sender, MouseEventArgs e)
        {
            int myX = e.X;
            int myY = e.Y;


            Point pt1 = new Point(this.visualIndex.Left + myX, this.visualIndex.Bottom-34);
            Point pt2 = new Point(this.visualIndex.Left + myX, this.visualIndex.Bottom -2);

            Graphics g = visualIndex.CreateGraphics();
            g.DrawLine(new Pen(Color.Red, 2.0F), pt1, pt2);


            //double startHr = 2.0;
            //double endHr   = 2.2;
            int startMilliseconds = (myX-1) * 60000;
            if (startMilliseconds < 0) startMilliseconds = 0;
            int endMilliseconds   = (myX + 1) * 60000;
            //Console.WriteLine("\nWAIT - extracting segment!");


            //string recordingPath = @"C:\SensorNetworks\WavFiles\SunshineCoast\AcousticIndices_browserTemp.wav";
            string recordingPath = @"Z:\Site 4\DM420062.mp3";
            this.resampleRate = 17640;
            this.frameLength = 256;
            this.frameOverlap = 0.0;
            string outputDir = @"C:\SensorNetworks\WavFiles\SunshineCoast\";
            //this.segmentFileName.Text = "WAIT!";
            AudioRecording recording = AudioRecording.GetSegmentFromAudioRecording(recordingPath, startMilliseconds, endMilliseconds, this.resampleRate, outputDir);
            this.segmentFileName.Text = recording.FileName;

            //make the sonogram
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameLength;
            sonoConfig.WindowOverlap = frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            System.Drawing.Image image = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            PictureBox picture = new PictureBox();
            picture.Dock = DockStyle.Fill;

            picture.Image = sonogram.GetImage(doHighlightSubband, add1kHzLines); 
            this.sonogramPanel.Controls.Add(picture);

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
