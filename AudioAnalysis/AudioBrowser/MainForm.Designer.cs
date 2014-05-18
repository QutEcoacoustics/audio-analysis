namespace AudioBrowser
{
    using System.Drawing;
    using System.Windows.Forms;

    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageAnalyseAudioFile = new System.Windows.Forms.TabPage();
            this.textBoxAnalyseOutputDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAnalyseOutputDirBrowse = new System.Windows.Forms.Button();
            this.labelAnalyseSelectedAnalyserKey = new System.Windows.Forms.Label();
            this.btnAnalyseConfigFileEdit = new System.Windows.Forms.Button();
            this.textboxAnalyseConfigFilePath = new System.Windows.Forms.TextBox();
            this.textboxAnalyseAudioFilePath = new System.Windows.Forms.TextBox();
            this.comboboxAnalyseAnalyser = new System.Windows.Forms.ComboBox();
            this.btnAanlyseRun = new System.Windows.Forms.Button();
            this.btnAnalyseConfigFileBrowse = new System.Windows.Forms.Button();
            this.lblAnalysisStart = new System.Windows.Forms.Label();
            this.lblAnalysisEditConfig = new System.Windows.Forms.Label();
            this.lblAnalysisType = new System.Windows.Forms.Label();
            this.lblAnalysisFile = new System.Windows.Forms.Label();
            this.btnAnalyseAudioFileBrowse = new System.Windows.Forms.Button();
            this.tabPageBrowseAudioFile = new System.Windows.Forms.TabPage();
            this.splitContainerImages = new System.Windows.Forms.SplitContainer();
            this.pictureBoxAudioNavIndicies = new System.Windows.Forms.PictureBox();
            this.pictureBoxAudioNavClickTrack = new System.Windows.Forms.PictureBox();
            this.pictureBoxAudioNavSonogram = new System.Windows.Forms.PictureBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtAudioNavCursorValue = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtAudioNavCursorLocation = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tabControlBrowse = new System.Windows.Forms.TabControl();
            this.tabPageBrowseActions = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.lblCurrentSegment = new System.Windows.Forms.Label();
            this.btnDisplaySimilarSegments = new System.Windows.Forms.Button();
            this.listBoxSimilarSegments = new System.Windows.Forms.ListBox();
            this.btnAudioNavSelectFiles = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxSonogramBuffer = new System.Windows.Forms.TextBox();
            this.chkSonogramBuffer = new System.Windows.Forms.CheckBox();
            this.chkAudioNavNoiseReduce = new System.Windows.Forms.CheckBox();
            this.chkAudioNavAnnotateSonogram = new System.Windows.Forms.CheckBox();
            this.btnAudioNavRunAudacity = new System.Windows.Forms.Button();
            this.btnAudioNavRefreshSonogram = new System.Windows.Forms.Button();
            this.tabPageBrowseInformation = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.textBoxBrowseSonogramImageFile = new System.Windows.Forms.TextBox();
            this.textBoxBrowseAudioSegmentFile = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtAudioNavDuration = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtAudioNavImgScale = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtAudioNavAnalysisType = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtAudioNavClickValue = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtAudioNavClickLocation = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblAudioNavCSVHeaders = new System.Windows.Forms.Label();
            this.listBoxAudioNavCSVHeaders = new System.Windows.Forms.ListBox();
            this.tabPageConsole = new System.Windows.Forms.TabPage();
            this.btnClearConsole = new System.Windows.Forms.Button();
            this.richTextBoxConsole = new System.Windows.Forms.RichTextBox();
            this.tabUnderDevelopment = new System.Windows.Forms.TabPage();
            this.btnCSV2ARFF = new System.Windows.Forms.Button();
            this.fullNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileNameDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastModifiedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.durationDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mediaTypeDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileLengthDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControlMain.SuspendLayout();
            this.tabPageAnalyseAudioFile.SuspendLayout();
            this.tabPageBrowseAudioFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerImages)).BeginInit();
            this.splitContainerImages.Panel1.SuspendLayout();
            this.splitContainerImages.Panel2.SuspendLayout();
            this.splitContainerImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAudioNavIndicies)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAudioNavClickTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAudioNavSonogram)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tabControlBrowse.SuspendLayout();
            this.tabPageBrowseActions.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPageBrowseInformation.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPageConsole.SuspendLayout();
            this.tabUnderDevelopment.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageAnalyseAudioFile);
            this.tabControlMain.Controls.Add(this.tabPageBrowseAudioFile);
            this.tabControlMain.Controls.Add(this.tabPageConsole);
            this.tabControlMain.Controls.Add(this.tabUnderDevelopment);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.ItemSize = new System.Drawing.Size(105, 18);
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1505, 785);
            this.tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlMain.TabIndex = 0;
            // 
            // tabPageAnalyseAudioFile
            // 
            this.tabPageAnalyseAudioFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPageAnalyseAudioFile.Controls.Add(this.textBoxAnalyseOutputDir);
            this.tabPageAnalyseAudioFile.Controls.Add(this.label1);
            this.tabPageAnalyseAudioFile.Controls.Add(this.btnAnalyseOutputDirBrowse);
            this.tabPageAnalyseAudioFile.Controls.Add(this.labelAnalyseSelectedAnalyserKey);
            this.tabPageAnalyseAudioFile.Controls.Add(this.btnAnalyseConfigFileEdit);
            this.tabPageAnalyseAudioFile.Controls.Add(this.textboxAnalyseConfigFilePath);
            this.tabPageAnalyseAudioFile.Controls.Add(this.textboxAnalyseAudioFilePath);
            this.tabPageAnalyseAudioFile.Controls.Add(this.comboboxAnalyseAnalyser);
            this.tabPageAnalyseAudioFile.Controls.Add(this.btnAanlyseRun);
            this.tabPageAnalyseAudioFile.Controls.Add(this.btnAnalyseConfigFileBrowse);
            this.tabPageAnalyseAudioFile.Controls.Add(this.lblAnalysisStart);
            this.tabPageAnalyseAudioFile.Controls.Add(this.lblAnalysisEditConfig);
            this.tabPageAnalyseAudioFile.Controls.Add(this.lblAnalysisType);
            this.tabPageAnalyseAudioFile.Controls.Add(this.lblAnalysisFile);
            this.tabPageAnalyseAudioFile.Controls.Add(this.btnAnalyseAudioFileBrowse);
            this.tabPageAnalyseAudioFile.Location = new System.Drawing.Point(4, 22);
            this.tabPageAnalyseAudioFile.Name = "tabPageAnalyseAudioFile";
            this.tabPageAnalyseAudioFile.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAnalyseAudioFile.Size = new System.Drawing.Size(1497, 759);
            this.tabPageAnalyseAudioFile.TabIndex = 5;
            this.tabPageAnalyseAudioFile.Text = "Analyse Audio File";
            this.tabPageAnalyseAudioFile.UseVisualStyleBackColor = true;
            // 
            // textBoxAnalyseOutputDir
            // 
            this.textBoxAnalyseOutputDir.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxAnalyseOutputDir.Location = new System.Drawing.Point(298, 179);
            this.textBoxAnalyseOutputDir.Name = "textBoxAnalyseOutputDir";
            this.textBoxAnalyseOutputDir.Size = new System.Drawing.Size(565, 20);
            this.textBoxAnalyseOutputDir.TabIndex = 35;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(18, 180);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 15);
            this.label1.TabIndex = 34;
            this.label1.Text = "4: Select the output directory:";
            // 
            // btnAnalyseOutputDirBrowse
            // 
            this.btnAnalyseOutputDirBrowse.Location = new System.Drawing.Point(222, 177);
            this.btnAnalyseOutputDirBrowse.Name = "btnAnalyseOutputDirBrowse";
            this.btnAnalyseOutputDirBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnAnalyseOutputDirBrowse.TabIndex = 33;
            this.btnAnalyseOutputDirBrowse.Text = "Browse...";
            this.btnAnalyseOutputDirBrowse.UseVisualStyleBackColor = true;
            this.btnAnalyseOutputDirBrowse.Click += new System.EventHandler(this.btnAnalyseOutputDirBrowse_Click);
            // 
            // labelAnalyseSelectedAnalyserKey
            // 
            this.labelAnalyseSelectedAnalyserKey.AutoSize = true;
            this.labelAnalyseSelectedAnalyserKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalyseSelectedAnalyserKey.Location = new System.Drawing.Point(497, 64);
            this.labelAnalyseSelectedAnalyserKey.Name = "labelAnalyseSelectedAnalyserKey";
            this.labelAnalyseSelectedAnalyserKey.Size = new System.Drawing.Size(25, 15);
            this.labelAnalyseSelectedAnalyserKey.TabIndex = 32;
            this.labelAnalyseSelectedAnalyserKey.Text = "key";
            // 
            // btnAnalyseConfigFileEdit
            // 
            this.btnAnalyseConfigFileEdit.Location = new System.Drawing.Point(743, 116);
            this.btnAnalyseConfigFileEdit.Name = "btnAnalyseConfigFileEdit";
            this.btnAnalyseConfigFileEdit.Size = new System.Drawing.Size(120, 23);
            this.btnAnalyseConfigFileEdit.TabIndex = 29;
            this.btnAnalyseConfigFileEdit.Text = "Edit Config File";
            this.btnAnalyseConfigFileEdit.UseVisualStyleBackColor = true;
            this.btnAnalyseConfigFileEdit.Click += new System.EventHandler(this.btnAnalyseConfigFileEdit_Click);
            // 
            // textboxAnalyseConfigFilePath
            // 
            this.textboxAnalyseConfigFilePath.BackColor = System.Drawing.SystemColors.Window;
            this.textboxAnalyseConfigFilePath.Location = new System.Drawing.Point(298, 117);
            this.textboxAnalyseConfigFilePath.Name = "textboxAnalyseConfigFilePath";
            this.textboxAnalyseConfigFilePath.Size = new System.Drawing.Size(439, 20);
            this.textboxAnalyseConfigFilePath.TabIndex = 25;
            // 
            // textboxAnalyseAudioFilePath
            // 
            this.textboxAnalyseAudioFilePath.BackColor = System.Drawing.SystemColors.Window;
            this.textboxAnalyseAudioFilePath.Location = new System.Drawing.Point(298, 17);
            this.textboxAnalyseAudioFilePath.Name = "textboxAnalyseAudioFilePath";
            this.textboxAnalyseAudioFilePath.Size = new System.Drawing.Size(565, 20);
            this.textboxAnalyseAudioFilePath.TabIndex = 23;
            // 
            // comboboxAnalyseAnalyser
            // 
            this.comboboxAnalyseAnalyser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxAnalyseAnalyser.FormattingEnabled = true;
            this.comboboxAnalyseAnalyser.Location = new System.Drawing.Point(220, 64);
            this.comboboxAnalyseAnalyser.Name = "comboboxAnalyseAnalyser";
            this.comboboxAnalyseAnalyser.Size = new System.Drawing.Size(271, 21);
            this.comboboxAnalyseAnalyser.TabIndex = 22;
            this.comboboxAnalyseAnalyser.SelectedIndexChanged += new System.EventHandler(this.comboboxAnalyseAnalyser_SelectedIndexChanged);
            // 
            // btnAanlyseRun
            // 
            this.btnAanlyseRun.Location = new System.Drawing.Point(222, 232);
            this.btnAanlyseRun.Name = "btnAanlyseRun";
            this.btnAanlyseRun.Size = new System.Drawing.Size(70, 23);
            this.btnAanlyseRun.TabIndex = 21;
            this.btnAanlyseRun.Text = "Analyse";
            this.btnAanlyseRun.UseVisualStyleBackColor = true;
            this.btnAanlyseRun.Click += new System.EventHandler(this.btnAanlyseRun_Click);
            // 
            // btnAnalyseConfigFileBrowse
            // 
            this.btnAnalyseConfigFileBrowse.Location = new System.Drawing.Point(222, 116);
            this.btnAnalyseConfigFileBrowse.Name = "btnAnalyseConfigFileBrowse";
            this.btnAnalyseConfigFileBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnAnalyseConfigFileBrowse.TabIndex = 20;
            this.btnAnalyseConfigFileBrowse.Text = "Browse...";
            this.btnAnalyseConfigFileBrowse.UseVisualStyleBackColor = true;
            this.btnAnalyseConfigFileBrowse.Click += new System.EventHandler(this.btnAnalyseConfigFileBrowse_Click);
            // 
            // lblAnalysisStart
            // 
            this.lblAnalysisStart.AutoSize = true;
            this.lblAnalysisStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisStart.Location = new System.Drawing.Point(18, 235);
            this.lblAnalysisStart.Name = "lblAnalysisStart";
            this.lblAnalysisStart.Size = new System.Drawing.Size(115, 15);
            this.lblAnalysisStart.TabIndex = 19;
            this.lblAnalysisStart.Text = "5: Start the analysis:";
            // 
            // lblAnalysisEditConfig
            // 
            this.lblAnalysisEditConfig.AutoSize = true;
            this.lblAnalysisEditConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisEditConfig.Location = new System.Drawing.Point(18, 119);
            this.lblAnalysisEditConfig.Name = "lblAnalysisEditConfig";
            this.lblAnalysisEditConfig.Size = new System.Drawing.Size(179, 30);
            this.lblAnalysisEditConfig.TabIndex = 18;
            this.lblAnalysisEditConfig.Text = "3: Select and optionally edit the \r\nanalysis config file:";
            // 
            // lblAnalysisType
            // 
            this.lblAnalysisType.AutoSize = true;
            this.lblAnalysisType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisType.Location = new System.Drawing.Point(18, 65);
            this.lblAnalysisType.Name = "lblAnalysisType";
            this.lblAnalysisType.Size = new System.Drawing.Size(188, 15);
            this.lblAnalysisType.TabIndex = 17;
            this.lblAnalysisType.Text = "2: Select type of analysis from list:";
            // 
            // lblAnalysisFile
            // 
            this.lblAnalysisFile.AutoSize = true;
            this.lblAnalysisFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisFile.Location = new System.Drawing.Point(18, 18);
            this.lblAnalysisFile.Name = "lblAnalysisFile";
            this.lblAnalysisFile.Size = new System.Drawing.Size(185, 15);
            this.lblAnalysisFile.TabIndex = 14;
            this.lblAnalysisFile.Text = "1: Select an audio file to analyse:";
            // 
            // btnAnalyseAudioFileBrowse
            // 
            this.btnAnalyseAudioFileBrowse.Location = new System.Drawing.Point(222, 15);
            this.btnAnalyseAudioFileBrowse.Name = "btnAnalyseAudioFileBrowse";
            this.btnAnalyseAudioFileBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnAnalyseAudioFileBrowse.TabIndex = 2;
            this.btnAnalyseAudioFileBrowse.Text = "Browse...";
            this.btnAnalyseAudioFileBrowse.UseVisualStyleBackColor = true;
            this.btnAnalyseAudioFileBrowse.Click += new System.EventHandler(this.btnAnalyseAudioFileBrowse_Click);
            // 
            // tabPageBrowseAudioFile
            // 
            this.tabPageBrowseAudioFile.Controls.Add(this.splitContainerImages);
            this.tabPageBrowseAudioFile.Controls.Add(this.groupBox3);
            this.tabPageBrowseAudioFile.Controls.Add(this.tabControlBrowse);
            this.tabPageBrowseAudioFile.Location = new System.Drawing.Point(4, 22);
            this.tabPageBrowseAudioFile.Name = "tabPageBrowseAudioFile";
            this.tabPageBrowseAudioFile.Size = new System.Drawing.Size(1497, 759);
            this.tabPageBrowseAudioFile.TabIndex = 7;
            this.tabPageBrowseAudioFile.Text = "Browse Audio File";
            this.tabPageBrowseAudioFile.UseVisualStyleBackColor = true;
            // 
            // splitContainerImages
            // 
            this.splitContainerImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerImages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerImages.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerImages.Location = new System.Drawing.Point(194, 3);
            this.splitContainerImages.Name = "splitContainerImages";
            this.splitContainerImages.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerImages.Panel1
            // 
            this.splitContainerImages.Panel1.AutoScroll = true;
            this.splitContainerImages.Panel1.Controls.Add(this.pictureBoxAudioNavIndicies);
            this.splitContainerImages.Panel1.Controls.Add(this.pictureBoxAudioNavClickTrack);
            // 
            // splitContainerImages.Panel2
            // 
            this.splitContainerImages.Panel2.AutoScroll = true;
            this.splitContainerImages.Panel2.Controls.Add(this.pictureBoxAudioNavSonogram);
            this.splitContainerImages.Size = new System.Drawing.Size(1303, 753);
            this.splitContainerImages.SplitterDistance = 404;
            this.splitContainerImages.TabIndex = 2;
            // 
            // pictureBoxAudioNavIndicies
            // 
            this.pictureBoxAudioNavIndicies.BackColor = System.Drawing.Color.Black;
            this.pictureBoxAudioNavIndicies.Location = new System.Drawing.Point(4, 25);
            this.pictureBoxAudioNavIndicies.Name = "pictureBoxAudioNavIndicies";
            this.pictureBoxAudioNavIndicies.Size = new System.Drawing.Size(1200, 350);
            this.pictureBoxAudioNavIndicies.TabIndex = 0;
            this.pictureBoxAudioNavIndicies.TabStop = false;
            this.pictureBoxAudioNavIndicies.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxAudioNavIndicies_MouseClick);
            this.pictureBoxAudioNavIndicies.MouseHover += new System.EventHandler(this.pictureBoxAudioNavIndicies_MouseHover);
            this.pictureBoxAudioNavIndicies.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxAudioNavIndicies_MouseMove);
            // 
            // pictureBoxAudioNavClickTrack
            // 
            this.pictureBoxAudioNavClickTrack.BackColor = System.Drawing.Color.DarkGray;
            this.pictureBoxAudioNavClickTrack.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxAudioNavClickTrack.Name = "pictureBoxAudioNavClickTrack";
            this.pictureBoxAudioNavClickTrack.Size = new System.Drawing.Size(1200, 20);
            this.pictureBoxAudioNavClickTrack.TabIndex = 1;
            this.pictureBoxAudioNavClickTrack.TabStop = false;
            // 
            // pictureBoxAudioNavSonogram
            // 
            this.pictureBoxAudioNavSonogram.BackColor = System.Drawing.Color.DarkGray;
            this.pictureBoxAudioNavSonogram.Location = new System.Drawing.Point(4, 2);
            this.pictureBoxAudioNavSonogram.Name = "pictureBoxAudioNavSonogram";
            this.pictureBoxAudioNavSonogram.Size = new System.Drawing.Size(1200, 350);
            this.pictureBoxAudioNavSonogram.TabIndex = 0;
            this.pictureBoxAudioNavSonogram.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtAudioNavCursorValue);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.txtAudioNavCursorLocation);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(185, 99);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Cursor";
            // 
            // txtAudioNavCursorValue
            // 
            this.txtAudioNavCursorValue.Location = new System.Drawing.Point(6, 71);
            this.txtAudioNavCursorValue.Name = "txtAudioNavCursorValue";
            this.txtAudioNavCursorValue.ReadOnly = true;
            this.txtAudioNavCursorValue.Size = new System.Drawing.Size(173, 20);
            this.txtAudioNavCursorValue.TabIndex = 25;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 55);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Value";
            // 
            // txtAudioNavCursorLocation
            // 
            this.txtAudioNavCursorLocation.Location = new System.Drawing.Point(6, 32);
            this.txtAudioNavCursorLocation.Name = "txtAudioNavCursorLocation";
            this.txtAudioNavCursorLocation.ReadOnly = true;
            this.txtAudioNavCursorLocation.Size = new System.Drawing.Size(173, 20);
            this.txtAudioNavCursorLocation.TabIndex = 21;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Location";
            // 
            // tabControlBrowse
            // 
            this.tabControlBrowse.Controls.Add(this.tabPageBrowseActions);
            this.tabControlBrowse.Controls.Add(this.tabPageBrowseInformation);
            this.tabControlBrowse.Location = new System.Drawing.Point(3, 108);
            this.tabControlBrowse.Name = "tabControlBrowse";
            this.tabControlBrowse.SelectedIndex = 0;
            this.tabControlBrowse.Size = new System.Drawing.Size(189, 538);
            this.tabControlBrowse.TabIndex = 29;
            // 
            // tabPageBrowseActions
            // 
            this.tabPageBrowseActions.Controls.Add(this.groupBox6);
            this.tabPageBrowseActions.Controls.Add(this.btnAudioNavSelectFiles);
            this.tabPageBrowseActions.Controls.Add(this.groupBox1);
            this.tabPageBrowseActions.Location = new System.Drawing.Point(4, 22);
            this.tabPageBrowseActions.Name = "tabPageBrowseActions";
            this.tabPageBrowseActions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBrowseActions.Size = new System.Drawing.Size(181, 512);
            this.tabPageBrowseActions.TabIndex = 0;
            this.tabPageBrowseActions.Text = "Actions";
            this.tabPageBrowseActions.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.lblCurrentSegment);
            this.groupBox6.Controls.Add(this.btnDisplaySimilarSegments);
            this.groupBox6.Controls.Add(this.listBoxSimilarSegments);
            this.groupBox6.Location = new System.Drawing.Point(7, 47);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(168, 193);
            this.groupBox6.TabIndex = 17;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Indices Image";
            // 
            // lblCurrentSegment
            // 
            this.lblCurrentSegment.AutoSize = true;
            this.lblCurrentSegment.Location = new System.Drawing.Point(6, 50);
            this.lblCurrentSegment.Name = "lblCurrentSegment";
            this.lblCurrentSegment.Size = new System.Drawing.Size(99, 13);
            this.lblCurrentSegment.TabIndex = 19;
            this.lblCurrentSegment.Text = "Location and Value";
            // 
            // btnDisplaySimilarSegments
            // 
            this.btnDisplaySimilarSegments.Location = new System.Drawing.Point(6, 19);
            this.btnDisplaySimilarSegments.Name = "btnDisplaySimilarSegments";
            this.btnDisplaySimilarSegments.Size = new System.Drawing.Size(156, 28);
            this.btnDisplaySimilarSegments.TabIndex = 18;
            this.btnDisplaySimilarSegments.Text = "Find Similar Segments";
            this.btnDisplaySimilarSegments.UseVisualStyleBackColor = true;
            this.btnDisplaySimilarSegments.Click += new System.EventHandler(this.btnDisplaySimilarSegments_Click);
            // 
            // listBoxSimilarSegments
            // 
            this.listBoxSimilarSegments.FormattingEnabled = true;
            this.listBoxSimilarSegments.Location = new System.Drawing.Point(6, 66);
            this.listBoxSimilarSegments.Name = "listBoxSimilarSegments";
            this.listBoxSimilarSegments.Size = new System.Drawing.Size(156, 121);
            this.listBoxSimilarSegments.TabIndex = 13;
            // 
            // btnAudioNavSelectFiles
            // 
            this.btnAudioNavSelectFiles.Location = new System.Drawing.Point(6, 6);
            this.btnAudioNavSelectFiles.Name = "btnAudioNavSelectFiles";
            this.btnAudioNavSelectFiles.Size = new System.Drawing.Size(169, 34);
            this.btnAudioNavSelectFiles.TabIndex = 16;
            this.btnAudioNavSelectFiles.Text = "Select Files (csv, image, audio)";
            this.btnAudioNavSelectFiles.UseVisualStyleBackColor = true;
            this.btnAudioNavSelectFiles.Click += new System.EventHandler(this.btnAudioNavSelectFiles_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxSonogramBuffer);
            this.groupBox1.Controls.Add(this.chkSonogramBuffer);
            this.groupBox1.Controls.Add(this.chkAudioNavNoiseReduce);
            this.groupBox1.Controls.Add(this.chkAudioNavAnnotateSonogram);
            this.groupBox1.Controls.Add(this.btnAudioNavRunAudacity);
            this.groupBox1.Controls.Add(this.btnAudioNavRefreshSonogram);
            this.groupBox1.Location = new System.Drawing.Point(6, 246);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(169, 185);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sonogram";
            // 
            // textBoxSonogramBuffer
            // 
            this.textBoxSonogramBuffer.Location = new System.Drawing.Point(45, 63);
            this.textBoxSonogramBuffer.Name = "textBoxSonogramBuffer";
            this.textBoxSonogramBuffer.Size = new System.Drawing.Size(21, 20);
            this.textBoxSonogramBuffer.TabIndex = 11;
            this.textBoxSonogramBuffer.Text = "15";
            // 
            // chkSonogramBuffer
            // 
            this.chkSonogramBuffer.AutoSize = true;
            this.chkSonogramBuffer.Location = new System.Drawing.Point(6, 65);
            this.chkSonogramBuffer.Name = "chkSonogramBuffer";
            this.chkSonogramBuffer.Size = new System.Drawing.Size(167, 17);
            this.chkSonogramBuffer.TabIndex = 10;
            this.chkSonogramBuffer.Text = "Add        sec buffer both sides\r\n";
            this.chkSonogramBuffer.UseVisualStyleBackColor = true;
            // 
            // chkAudioNavNoiseReduce
            // 
            this.chkAudioNavNoiseReduce.AutoSize = true;
            this.chkAudioNavNoiseReduce.Location = new System.Drawing.Point(6, 19);
            this.chkAudioNavNoiseReduce.Name = "chkAudioNavNoiseReduce";
            this.chkAudioNavNoiseReduce.Size = new System.Drawing.Size(138, 17);
            this.chkAudioNavNoiseReduce.TabIndex = 8;
            this.chkAudioNavNoiseReduce.Text = "Noise reduce sonogram";
            this.chkAudioNavNoiseReduce.UseVisualStyleBackColor = true;
            // 
            // chkAudioNavAnnotateSonogram
            // 
            this.chkAudioNavAnnotateSonogram.AutoSize = true;
            this.chkAudioNavAnnotateSonogram.Location = new System.Drawing.Point(6, 42);
            this.chkAudioNavAnnotateSonogram.Name = "chkAudioNavAnnotateSonogram";
            this.chkAudioNavAnnotateSonogram.Size = new System.Drawing.Size(118, 17);
            this.chkAudioNavAnnotateSonogram.TabIndex = 9;
            this.chkAudioNavAnnotateSonogram.Text = "Annotate sonogram";
            this.chkAudioNavAnnotateSonogram.UseVisualStyleBackColor = true;
            // 
            // btnAudioNavRunAudacity
            // 
            this.btnAudioNavRunAudacity.Location = new System.Drawing.Point(6, 135);
            this.btnAudioNavRunAudacity.Name = "btnAudioNavRunAudacity";
            this.btnAudioNavRunAudacity.Size = new System.Drawing.Size(120, 28);
            this.btnAudioNavRunAudacity.TabIndex = 6;
            this.btnAudioNavRunAudacity.Text = "Run Audacity";
            this.btnAudioNavRunAudacity.UseVisualStyleBackColor = true;
            this.btnAudioNavRunAudacity.Click += new System.EventHandler(this.btnAudioNavRunAudacity_Click);
            // 
            // btnAudioNavRefreshSonogram
            // 
            this.btnAudioNavRefreshSonogram.Location = new System.Drawing.Point(6, 101);
            this.btnAudioNavRefreshSonogram.Name = "btnAudioNavRefreshSonogram";
            this.btnAudioNavRefreshSonogram.Size = new System.Drawing.Size(120, 28);
            this.btnAudioNavRefreshSonogram.TabIndex = 7;
            this.btnAudioNavRefreshSonogram.Text = "Refresh Sonogram";
            this.btnAudioNavRefreshSonogram.UseVisualStyleBackColor = true;
            this.btnAudioNavRefreshSonogram.Click += new System.EventHandler(this.btnAudioNavRefreshSonogram_Click);
            // 
            // tabPageBrowseInformation
            // 
            this.tabPageBrowseInformation.Controls.Add(this.groupBox5);
            this.tabPageBrowseInformation.Controls.Add(this.groupBox4);
            this.tabPageBrowseInformation.Controls.Add(this.groupBox2);
            this.tabPageBrowseInformation.Location = new System.Drawing.Point(4, 22);
            this.tabPageBrowseInformation.Name = "tabPageBrowseInformation";
            this.tabPageBrowseInformation.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBrowseInformation.Size = new System.Drawing.Size(181, 512);
            this.tabPageBrowseInformation.TabIndex = 1;
            this.tabPageBrowseInformation.Text = "Information";
            this.tabPageBrowseInformation.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.textBoxBrowseSonogramImageFile);
            this.groupBox5.Controls.Add(this.textBoxBrowseAudioSegmentFile);
            this.groupBox5.Controls.Add(this.label19);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.txtAudioNavDuration);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.txtAudioNavImgScale);
            this.groupBox5.Controls.Add(this.label17);
            this.groupBox5.Controls.Add(this.label18);
            this.groupBox5.Controls.Add(this.txtAudioNavAnalysisType);
            this.groupBox5.Location = new System.Drawing.Point(6, 266);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(169, 218);
            this.groupBox5.TabIndex = 33;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Files";
            // 
            // textBoxBrowseSonogramImageFile
            // 
            this.textBoxBrowseSonogramImageFile.Location = new System.Drawing.Point(6, 71);
            this.textBoxBrowseSonogramImageFile.Name = "textBoxBrowseSonogramImageFile";
            this.textBoxBrowseSonogramImageFile.ReadOnly = true;
            this.textBoxBrowseSonogramImageFile.Size = new System.Drawing.Size(157, 20);
            this.textBoxBrowseSonogramImageFile.TabIndex = 39;
            // 
            // textBoxBrowseAudioSegmentFile
            // 
            this.textBoxBrowseAudioSegmentFile.Location = new System.Drawing.Point(6, 32);
            this.textBoxBrowseAudioSegmentFile.Name = "textBoxBrowseAudioSegmentFile";
            this.textBoxBrowseAudioSegmentFile.ReadOnly = true;
            this.textBoxBrowseAudioSegmentFile.Size = new System.Drawing.Size(157, 20);
            this.textBoxBrowseAudioSegmentFile.TabIndex = 38;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 172);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(77, 13);
            this.label19.TabIndex = 31;
            this.label19.Text = "Audio Duration";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 55);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 37;
            this.label9.Text = "Sonogram File";
            // 
            // txtAudioNavDuration
            // 
            this.txtAudioNavDuration.Location = new System.Drawing.Point(6, 188);
            this.txtAudioNavDuration.Name = "txtAudioNavDuration";
            this.txtAudioNavDuration.ReadOnly = true;
            this.txtAudioNavDuration.Size = new System.Drawing.Size(157, 20);
            this.txtAudioNavDuration.TabIndex = 32;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 13);
            this.label5.TabIndex = 36;
            this.label5.Text = "Audio Segment File";
            // 
            // txtAudioNavImgScale
            // 
            this.txtAudioNavImgScale.Location = new System.Drawing.Point(6, 149);
            this.txtAudioNavImgScale.Name = "txtAudioNavImgScale";
            this.txtAudioNavImgScale.ReadOnly = true;
            this.txtAudioNavImgScale.Size = new System.Drawing.Size(157, 20);
            this.txtAudioNavImgScale.TabIndex = 30;
            this.txtAudioNavImgScale.Text = "1 minute/pixel";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(6, 94);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(72, 13);
            this.label17.TabIndex = 21;
            this.label17.Text = "Analysis Type";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 133);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(66, 13);
            this.label18.TabIndex = 29;
            this.label18.Text = "Image Scale";
            // 
            // txtAudioNavAnalysisType
            // 
            this.txtAudioNavAnalysisType.Location = new System.Drawing.Point(6, 110);
            this.txtAudioNavAnalysisType.Name = "txtAudioNavAnalysisType";
            this.txtAudioNavAnalysisType.ReadOnly = true;
            this.txtAudioNavAnalysisType.Size = new System.Drawing.Size(157, 20);
            this.txtAudioNavAnalysisType.TabIndex = 28;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.txtAudioNavClickValue);
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.txtAudioNavClickLocation);
            this.groupBox4.Location = new System.Drawing.Point(5, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(169, 100);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Most Recent Click";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 56);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(34, 13);
            this.label14.TabIndex = 19;
            this.label14.Text = "Value";
            // 
            // txtAudioNavClickValue
            // 
            this.txtAudioNavClickValue.Location = new System.Drawing.Point(6, 72);
            this.txtAudioNavClickValue.Name = "txtAudioNavClickValue";
            this.txtAudioNavClickValue.ReadOnly = true;
            this.txtAudioNavClickValue.Size = new System.Drawing.Size(157, 20);
            this.txtAudioNavClickValue.TabIndex = 34;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 16);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(48, 13);
            this.label15.TabIndex = 16;
            this.label15.Text = "Location";
            // 
            // txtAudioNavClickLocation
            // 
            this.txtAudioNavClickLocation.Location = new System.Drawing.Point(6, 33);
            this.txtAudioNavClickLocation.Name = "txtAudioNavClickLocation";
            this.txtAudioNavClickLocation.ReadOnly = true;
            this.txtAudioNavClickLocation.Size = new System.Drawing.Size(157, 20);
            this.txtAudioNavClickLocation.TabIndex = 33;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblAudioNavCSVHeaders);
            this.groupBox2.Controls.Add(this.listBoxAudioNavCSVHeaders);
            this.groupBox2.Location = new System.Drawing.Point(5, 112);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(169, 148);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Selected Csv File Headers";
            // 
            // lblAudioNavCSVHeaders
            // 
            this.lblAudioNavCSVHeaders.AutoSize = true;
            this.lblAudioNavCSVHeaders.Location = new System.Drawing.Point(6, 16);
            this.lblAudioNavCSVHeaders.Name = "lblAudioNavCSVHeaders";
            this.lblAudioNavCSVHeaders.Size = new System.Drawing.Size(92, 13);
            this.lblAudioNavCSVHeaders.TabIndex = 13;
            this.lblAudioNavCSVHeaders.Text = "Selected Headers";
            // 
            // listBoxAudioNavCSVHeaders
            // 
            this.listBoxAudioNavCSVHeaders.FormattingEnabled = true;
            this.listBoxAudioNavCSVHeaders.Location = new System.Drawing.Point(6, 32);
            this.listBoxAudioNavCSVHeaders.Name = "listBoxAudioNavCSVHeaders";
            this.listBoxAudioNavCSVHeaders.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxAudioNavCSVHeaders.Size = new System.Drawing.Size(157, 108);
            this.listBoxAudioNavCSVHeaders.TabIndex = 12;
            // 
            // tabPageConsole
            // 
            this.tabPageConsole.Controls.Add(this.btnClearConsole);
            this.tabPageConsole.Controls.Add(this.richTextBoxConsole);
            this.tabPageConsole.Location = new System.Drawing.Point(4, 22);
            this.tabPageConsole.Name = "tabPageConsole";
            this.tabPageConsole.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConsole.Size = new System.Drawing.Size(1497, 759);
            this.tabPageConsole.TabIndex = 1;
            this.tabPageConsole.Text = "Console";
            this.tabPageConsole.UseVisualStyleBackColor = true;
            // 
            // btnClearConsole
            // 
            this.btnClearConsole.Location = new System.Drawing.Point(9, 7);
            this.btnClearConsole.Name = "btnClearConsole";
            this.btnClearConsole.Size = new System.Drawing.Size(95, 23);
            this.btnClearConsole.TabIndex = 2;
            this.btnClearConsole.Text = "Clear Console";
            this.btnClearConsole.UseVisualStyleBackColor = true;
            this.btnClearConsole.Click += new System.EventHandler(this.btnClearConsole_Click);
            // 
            // richTextBoxConsole
            // 
            this.richTextBoxConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxConsole.BackColor = System.Drawing.Color.Black;
            this.richTextBoxConsole.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold);
            this.richTextBoxConsole.ForeColor = System.Drawing.Color.White;
            this.richTextBoxConsole.Location = new System.Drawing.Point(3, 36);
            this.richTextBoxConsole.Name = "richTextBoxConsole";
            this.richTextBoxConsole.ReadOnly = true;
            this.richTextBoxConsole.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxConsole.Size = new System.Drawing.Size(1491, 720);
            this.richTextBoxConsole.TabIndex = 1;
            this.richTextBoxConsole.Text = "";
            // 
            // tabUnderDevelopment
            // 
            this.tabUnderDevelopment.Controls.Add(this.btnCSV2ARFF);
            this.tabUnderDevelopment.Location = new System.Drawing.Point(4, 22);
            this.tabUnderDevelopment.Name = "tabUnderDevelopment";
            this.tabUnderDevelopment.Padding = new System.Windows.Forms.Padding(3);
            this.tabUnderDevelopment.Size = new System.Drawing.Size(1497, 759);
            this.tabUnderDevelopment.TabIndex = 6;
            this.tabUnderDevelopment.Text = "Under Development";
            this.tabUnderDevelopment.UseVisualStyleBackColor = true;
            // 
            // btnCSV2ARFF
            // 
            this.btnCSV2ARFF.Location = new System.Drawing.Point(22, 29);
            this.btnCSV2ARFF.Name = "btnCSV2ARFF";
            this.btnCSV2ARFF.Size = new System.Drawing.Size(75, 23);
            this.btnCSV2ARFF.TabIndex = 0;
            this.btnCSV2ARFF.Text = "csv2Arff";
            this.btnCSV2ARFF.UseVisualStyleBackColor = true;
            this.btnCSV2ARFF.Click += new System.EventHandler(this.btnCSV2ARFF_Click);
            // 
            // fullNameDataGridViewTextBoxColumn
            // 
            this.fullNameDataGridViewTextBoxColumn.DataPropertyName = "FullName";
            this.fullNameDataGridViewTextBoxColumn.HeaderText = "FullName";
            this.fullNameDataGridViewTextBoxColumn.Name = "fullNameDataGridViewTextBoxColumn";
            this.fullNameDataGridViewTextBoxColumn.Width = 76;
            // 
            // fileNameDataGridViewTextBoxColumn1
            // 
            this.fileNameDataGridViewTextBoxColumn1.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn1.HeaderText = "FileName";
            this.fileNameDataGridViewTextBoxColumn1.Name = "fileNameDataGridViewTextBoxColumn1";
            this.fileNameDataGridViewTextBoxColumn1.Width = 76;
            // 
            // lastModifiedDataGridViewTextBoxColumn
            // 
            this.lastModifiedDataGridViewTextBoxColumn.DataPropertyName = "LastModified";
            this.lastModifiedDataGridViewTextBoxColumn.HeaderText = "LastModified";
            this.lastModifiedDataGridViewTextBoxColumn.Name = "lastModifiedDataGridViewTextBoxColumn";
            this.lastModifiedDataGridViewTextBoxColumn.Width = 92;
            // 
            // durationDataGridViewTextBoxColumn1
            // 
            this.durationDataGridViewTextBoxColumn1.DataPropertyName = "Duration";
            this.durationDataGridViewTextBoxColumn1.HeaderText = "Duration";
            this.durationDataGridViewTextBoxColumn1.Name = "durationDataGridViewTextBoxColumn1";
            this.durationDataGridViewTextBoxColumn1.Width = 72;
            // 
            // mediaTypeDataGridViewTextBoxColumn1
            // 
            this.mediaTypeDataGridViewTextBoxColumn1.DataPropertyName = "MediaType";
            this.mediaTypeDataGridViewTextBoxColumn1.HeaderText = "MediaType";
            this.mediaTypeDataGridViewTextBoxColumn1.Name = "mediaTypeDataGridViewTextBoxColumn1";
            this.mediaTypeDataGridViewTextBoxColumn1.Width = 85;
            // 
            // fileLengthDataGridViewTextBoxColumn1
            // 
            this.fileLengthDataGridViewTextBoxColumn1.DataPropertyName = "FileLength";
            this.fileLengthDataGridViewTextBoxColumn1.HeaderText = "FileLength";
            this.fileLengthDataGridViewTextBoxColumn1.Name = "fileLengthDataGridViewTextBoxColumn1";
            this.fileLengthDataGridViewTextBoxColumn1.Width = 81;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1505, 785);
            this.Controls.Add(this.tabControlMain);
            this.Location = new System.Drawing.Point(90, 90);
            this.MinimumSize = new System.Drawing.Size(830, 670);
            this.Name = "MainForm";
            this.Text = "Acoustic Environment Browser";
            this.tabControlMain.ResumeLayout(false);
            this.tabPageAnalyseAudioFile.ResumeLayout(false);
            this.tabPageAnalyseAudioFile.PerformLayout();
            this.tabPageBrowseAudioFile.ResumeLayout(false);
            this.splitContainerImages.Panel1.ResumeLayout(false);
            this.splitContainerImages.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerImages)).EndInit();
            this.splitContainerImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAudioNavIndicies)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAudioNavClickTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAudioNavSonogram)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabControlBrowse.ResumeLayout(false);
            this.tabPageBrowseActions.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPageBrowseInformation.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPageConsole.ResumeLayout(false);
            this.tabUnderDevelopment.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TabPage tabPageConsole;
        public TabControl tabControlMain;
        private DataGridViewTextBoxColumn fullNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn lastModifiedDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn durationDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn mediaTypeDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn fileLengthDataGridViewTextBoxColumn1;
        private TabPage tabPageAnalyseAudioFile;
        private TextBox textboxAnalyseConfigFilePath;
        private TextBox textboxAnalyseAudioFilePath;
        private ComboBox comboboxAnalyseAnalyser;
        private Button btnAanlyseRun;
        private Button btnAnalyseConfigFileBrowse;
        private Label lblAnalysisStart;
        private Label lblAnalysisEditConfig;
        private Label lblAnalysisType;
        private Label lblAnalysisFile;
        private Button btnAnalyseAudioFileBrowse;
        private TabPage tabUnderDevelopment;
        private Button btnCSV2ARFF;
        private TabPage tabPageBrowseAudioFile;
        private GroupBox groupBox3;
        private Label label7;
        private Label label8;
        private GroupBox groupBox2;
        private Label lblAudioNavCSVHeaders;
        private ListBox listBoxAudioNavCSVHeaders;
        private GroupBox groupBox1;
        private CheckBox chkAudioNavNoiseReduce;
        private CheckBox chkAudioNavAnnotateSonogram;
        private Button btnAudioNavRunAudacity;
        private Button btnAudioNavRefreshSonogram;
        private TextBox txtAudioNavCursorLocation;
        private TextBox txtAudioNavDuration;
        private Label label19;
        private TextBox txtAudioNavImgScale;
        private Label label18;
        private Label label17;
        private GroupBox groupBox4;
        private Label label14;
        private Label label15;
        private TextBox txtAudioNavCursorValue;
        private TextBox txtAudioNavClickValue;
        private TextBox txtAudioNavClickLocation;
        private PictureBox pictureBoxAudioNavIndicies;
        private PictureBox pictureBoxAudioNavSonogram;
        private CheckBox chkSonogramBuffer;
        private Label label9;
        private Label label5;
        private Button btnAudioNavSelectFiles;
        private TextBox txtAudioNavAnalysisType;
        private Button btnAnalyseConfigFileEdit;
        private Label labelAnalyseSelectedAnalyserKey;
        private RichTextBox richTextBoxConsole;
        private Button btnClearConsole;
        private TextBox textBoxAnalyseOutputDir;
        private Label label1;
        private Button btnAnalyseOutputDirBrowse;
        private TabControl tabControlBrowse;
        private TabPage tabPageBrowseActions;
        private TabPage tabPageBrowseInformation;
        private GroupBox groupBox5;
        private TextBox textBoxBrowseSonogramImageFile;
        private TextBox textBoxBrowseAudioSegmentFile;
        private PictureBox pictureBoxAudioNavClickTrack;
        private GroupBox groupBox6;
        private Button btnDisplaySimilarSegments;
        private ListBox listBoxSimilarSegments;
        private Label lblCurrentSegment;
        private SplitContainer splitContainerImages;
        private TextBox textBoxSonogramBuffer;


    }
}

