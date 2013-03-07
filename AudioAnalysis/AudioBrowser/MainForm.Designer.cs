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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabAnalyseFile = new System.Windows.Forms.TabPage();
            this.lblAnalysisPanelHeader = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxAnalysisGo = new System.Windows.Forms.TextBox();
            this.txtBoxAnalysisEditConfig = new System.Windows.Forms.TextBox();
            this.txtBoxAnalysisOutputDir = new System.Windows.Forms.TextBox();
            this.txtBoxAnalysisFile = new System.Windows.Forms.TextBox();
            this.comboAnalysisType = new System.Windows.Forms.ComboBox();
            this.btnAnalysisStart = new System.Windows.Forms.Button();
            this.btnAnalysisEditConfig = new System.Windows.Forms.Button();
            this.lblAnalysisStart = new System.Windows.Forms.Label();
            this.lblAnalysisEditConfig = new System.Windows.Forms.Label();
            this.lblAnalysisType = new System.Windows.Forms.Label();
            this.btnAnalysisOutputDir = new System.Windows.Forms.Button();
            this.lblAnalysisOutputDir = new System.Windows.Forms.Label();
            this.lblAnalysisFile = new System.Windows.Forms.Label();
            this.btnAnalysisFile = new System.Windows.Forms.Button();
            this.tabPageSourceFiles = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxSourceFileAnalysisType = new System.Windows.Forms.ComboBox();
            this.btnAnalyseSelectedAudioFiles = new System.Windows.Forms.Button();
            this.dataGridViewFileList = new System.Windows.Forms.DataGridView();
            this.selectedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.durationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileLengthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mediaTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mediaFileItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectSourceDirectory = new System.Windows.Forms.Button();
            this.btnUpdateSourceFileList = new System.Windows.Forms.Button();
            this.tfSourceDirectory = new System.Windows.Forms.TextBox();
            this.tabPageDisplay = new System.Windows.Forms.TabPage();
            this.btnViewFileOfIndices = new System.Windows.Forms.Button();
            this.labelCSVHeaders = new System.Windows.Forms.Label();
            this.listBoxDisplayedTracks = new System.Windows.Forms.ListBox();
            this.panelDisplayImages = new System.Windows.Forms.Panel();
            this.panelDisplayImageAndTrackBar = new System.Windows.Forms.Panel();
            this.pictureBoxVisualIndices = new System.Windows.Forms.PictureBox();
            this.pictureBoxBarTrack = new System.Windows.Forms.PictureBox();
            this.panelDisplaySpectrogram = new System.Windows.Forms.Panel();
            this.pictureBoxSonogram = new System.Windows.Forms.PictureBox();
            this.labelSourceFileName = new System.Windows.Forms.Label();
            this.labelDisplayInfo = new System.Windows.Forms.Label();
            this.checkBoxSonogramAnnotate = new System.Windows.Forms.CheckBox();
            this.checkBoxSonnogramNoiseReduce = new System.Windows.Forms.CheckBox();
            this.labelSonogramFileName = new System.Windows.Forms.Label();
            this.labelCursorValue = new System.Windows.Forms.Label();
            this.labelSonogramName = new System.Windows.Forms.Label();
            this.buttonRefreshSonogram = new System.Windows.Forms.Button();
            this.buttonAudacityRun = new System.Windows.Forms.Button();
            this.textBoxCursorValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxCursorLocation = new System.Windows.Forms.TextBox();
            this.tabPageConsole = new System.Windows.Forms.TabPage();
            this.textBoxConsole = new System.Windows.Forms.TextBox();
            this.tabPageSearchCsv = new System.Windows.Forms.TabPage();
            this.panelSearchEntries = new System.Windows.Forms.Panel();
            this.btnSearchRemoveFilterLine = new System.Windows.Forms.Button();
            this.tfSearchFieldMax = new System.Windows.Forms.TextBox();
            this.tfSearchFieldMin = new System.Windows.Forms.TextBox();
            this.lblSearchFieldMax = new System.Windows.Forms.Label();
            this.lblSearchFieldName = new System.Windows.Forms.Label();
            this.lblSearchFieldMin = new System.Windows.Forms.Label();
            this.tfSearchFieldName = new System.Windows.Forms.TextBox();
            this.btnFindInCSV = new System.Windows.Forms.Button();
            this.btnSelectCSVSourceFolder = new System.Windows.Forms.Button();
            this.textBoxCSVSourceFolderPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnAddFilterLine = new System.Windows.Forms.Button();
            this.folderBrowserDialogChooseDir = new System.Windows.Forms.FolderBrowserDialog();
            this.backgroundWorkerUpdateSourceFileList = new System.ComponentModel.BackgroundWorker();
            this.fullNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileNameDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastModifiedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.durationDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mediaTypeDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileLengthDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.csvFileItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabMisc = new System.Windows.Forms.TabPage();
            this.btnCSV2ARFF = new System.Windows.Forms.Button();
            this.tabControlMain.SuspendLayout();
            this.tabAnalyseFile.SuspendLayout();
            this.tabPageSourceFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mediaFileItemBindingSource)).BeginInit();
            this.tabPageDisplay.SuspendLayout();
            this.panelDisplayImages.SuspendLayout();
            this.panelDisplayImageAndTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualIndices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBarTrack)).BeginInit();
            this.panelDisplaySpectrogram.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSonogram)).BeginInit();
            this.tabPageConsole.SuspendLayout();
            this.tabPageSearchCsv.SuspendLayout();
            this.panelSearchEntries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.csvFileItemBindingSource)).BeginInit();
            this.tabMisc.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabAnalyseFile);
            this.tabControlMain.Controls.Add(this.tabPageSourceFiles);
            this.tabControlMain.Controls.Add(this.tabPageDisplay);
            this.tabControlMain.Controls.Add(this.tabPageConsole);
            this.tabControlMain.Controls.Add(this.tabPageSearchCsv);
            this.tabControlMain.Controls.Add(this.tabMisc);
            this.tabControlMain.ItemSize = new System.Drawing.Size(105, 18);
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1458, 681);
            this.tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlMain.TabIndex = 0;
            // 
            // tabAnalyseFile
            // 
            this.tabAnalyseFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabAnalyseFile.Controls.Add(this.lblAnalysisPanelHeader);
            this.tabAnalyseFile.Controls.Add(this.label2);
            this.tabAnalyseFile.Controls.Add(this.textBoxAnalysisGo);
            this.tabAnalyseFile.Controls.Add(this.txtBoxAnalysisEditConfig);
            this.tabAnalyseFile.Controls.Add(this.txtBoxAnalysisOutputDir);
            this.tabAnalyseFile.Controls.Add(this.txtBoxAnalysisFile);
            this.tabAnalyseFile.Controls.Add(this.comboAnalysisType);
            this.tabAnalyseFile.Controls.Add(this.btnAnalysisStart);
            this.tabAnalyseFile.Controls.Add(this.btnAnalysisEditConfig);
            this.tabAnalyseFile.Controls.Add(this.lblAnalysisStart);
            this.tabAnalyseFile.Controls.Add(this.lblAnalysisEditConfig);
            this.tabAnalyseFile.Controls.Add(this.lblAnalysisType);
            this.tabAnalyseFile.Controls.Add(this.btnAnalysisOutputDir);
            this.tabAnalyseFile.Controls.Add(this.lblAnalysisOutputDir);
            this.tabAnalyseFile.Controls.Add(this.lblAnalysisFile);
            this.tabAnalyseFile.Controls.Add(this.btnAnalysisFile);
            this.tabAnalyseFile.Location = new System.Drawing.Point(4, 22);
            this.tabAnalyseFile.Name = "tabAnalyseFile";
            this.tabAnalyseFile.Padding = new System.Windows.Forms.Padding(3);
            this.tabAnalyseFile.Size = new System.Drawing.Size(1450, 655);
            this.tabAnalyseFile.TabIndex = 5;
            this.tabAnalyseFile.Text = "Analyse Audio File";
            this.tabAnalyseFile.UseVisualStyleBackColor = true;
            // 
            // lblAnalysisPanelHeader
            // 
            this.lblAnalysisPanelHeader.AutoSize = true;
            this.lblAnalysisPanelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisPanelHeader.ForeColor = System.Drawing.Color.Navy;
            this.lblAnalysisPanelHeader.Location = new System.Drawing.Point(30, 21);
            this.lblAnalysisPanelHeader.Name = "lblAnalysisPanelHeader";
            this.lblAnalysisPanelHeader.Size = new System.Drawing.Size(341, 17);
            this.lblAnalysisPanelHeader.TabIndex = 28;
            this.lblAnalysisPanelHeader.Text = "To analyse an audio file complete steps 1 - 6.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(30, 336);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(589, 15);
            this.label2.TabIndex = 27;
            this.label2.Text = "6: Paste the command line below into a command-line shell such as \'powershell.exe" +
    "\' or \'cmd.exe\' and run:";
            // 
            // textBoxAnalysisGo
            // 
            this.textBoxAnalysisGo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxAnalysisGo.Location = new System.Drawing.Point(234, 356);
            this.textBoxAnalysisGo.Multiline = true;
            this.textBoxAnalysisGo.Name = "textBoxAnalysisGo";
            this.textBoxAnalysisGo.ReadOnly = true;
            this.textBoxAnalysisGo.Size = new System.Drawing.Size(988, 69);
            this.textBoxAnalysisGo.TabIndex = 26;
            this.textBoxAnalysisGo.Text = "COMMAND LINE";
            // 
            // txtBoxAnalysisEditConfig
            // 
            this.txtBoxAnalysisEditConfig.BackColor = System.Drawing.SystemColors.Control;
            this.txtBoxAnalysisEditConfig.Location = new System.Drawing.Point(389, 228);
            this.txtBoxAnalysisEditConfig.Name = "txtBoxAnalysisEditConfig";
            this.txtBoxAnalysisEditConfig.Size = new System.Drawing.Size(272, 20);
            this.txtBoxAnalysisEditConfig.TabIndex = 25;
            // 
            // txtBoxAnalysisOutputDir
            // 
            this.txtBoxAnalysisOutputDir.BackColor = System.Drawing.SystemColors.Control;
            this.txtBoxAnalysisOutputDir.Location = new System.Drawing.Point(389, 118);
            this.txtBoxAnalysisOutputDir.Name = "txtBoxAnalysisOutputDir";
            this.txtBoxAnalysisOutputDir.Size = new System.Drawing.Size(272, 20);
            this.txtBoxAnalysisOutputDir.TabIndex = 24;
            // 
            // txtBoxAnalysisFile
            // 
            this.txtBoxAnalysisFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtBoxAnalysisFile.Location = new System.Drawing.Point(389, 67);
            this.txtBoxAnalysisFile.Name = "txtBoxAnalysisFile";
            this.txtBoxAnalysisFile.Size = new System.Drawing.Size(272, 20);
            this.txtBoxAnalysisFile.TabIndex = 23;
            // 
            // comboAnalysisType
            // 
            this.comboAnalysisType.FormattingEnabled = true;
            this.comboAnalysisType.Location = new System.Drawing.Point(232, 173);
            this.comboAnalysisType.Name = "comboAnalysisType";
            this.comboAnalysisType.Size = new System.Drawing.Size(121, 21);
            this.comboAnalysisType.TabIndex = 22;
            this.comboAnalysisType.SelectedValueChanged += new System.EventHandler(this.comboAnalysisType_SelectedValueChanged);
            // 
            // btnAnalysisStart
            // 
            this.btnAnalysisStart.Location = new System.Drawing.Point(234, 282);
            this.btnAnalysisStart.Name = "btnAnalysisStart";
            this.btnAnalysisStart.Size = new System.Drawing.Size(120, 23);
            this.btnAnalysisStart.TabIndex = 21;
            this.btnAnalysisStart.Text = "Construct";
            this.btnAnalysisStart.UseVisualStyleBackColor = true;
            this.btnAnalysisStart.Click += new System.EventHandler(this.btnAnalysisStart_Click);
            // 
            // btnAnalysisEditConfig
            // 
            this.btnAnalysisEditConfig.Location = new System.Drawing.Point(234, 225);
            this.btnAnalysisEditConfig.Name = "btnAnalysisEditConfig";
            this.btnAnalysisEditConfig.Size = new System.Drawing.Size(120, 23);
            this.btnAnalysisEditConfig.TabIndex = 20;
            this.btnAnalysisEditConfig.Text = "Edit";
            this.btnAnalysisEditConfig.UseVisualStyleBackColor = true;
            this.btnAnalysisEditConfig.Click += new System.EventHandler(this.btnAnalysisEditConfig_Click);
            // 
            // lblAnalysisStart
            // 
            this.lblAnalysisStart.AutoSize = true;
            this.lblAnalysisStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisStart.Location = new System.Drawing.Point(30, 285);
            this.lblAnalysisStart.Name = "lblAnalysisStart";
            this.lblAnalysisStart.Size = new System.Drawing.Size(176, 15);
            this.lblAnalysisStart.TabIndex = 19;
            this.lblAnalysisStart.Text = "5: Construct the command line:";
            // 
            // lblAnalysisEditConfig
            // 
            this.lblAnalysisEditConfig.AutoSize = true;
            this.lblAnalysisEditConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisEditConfig.Location = new System.Drawing.Point(30, 228);
            this.lblAnalysisEditConfig.Name = "lblAnalysisEditConfig";
            this.lblAnalysisEditConfig.Size = new System.Drawing.Size(166, 15);
            this.lblAnalysisEditConfig.TabIndex = 18;
            this.lblAnalysisEditConfig.Text = "4: Edit the analysis config file:";
            // 
            // lblAnalysisType
            // 
            this.lblAnalysisType.AutoSize = true;
            this.lblAnalysisType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisType.Location = new System.Drawing.Point(30, 174);
            this.lblAnalysisType.Name = "lblAnalysisType";
            this.lblAnalysisType.Size = new System.Drawing.Size(181, 15);
            this.lblAnalysisType.TabIndex = 17;
            this.lblAnalysisType.Text = "3: Select \'analysis type\' from list:";
            // 
            // btnAnalysisOutputDir
            // 
            this.btnAnalysisOutputDir.Location = new System.Drawing.Point(234, 118);
            this.btnAnalysisOutputDir.Name = "btnAnalysisOutputDir";
            this.btnAnalysisOutputDir.Size = new System.Drawing.Size(120, 23);
            this.btnAnalysisOutputDir.TabIndex = 16;
            this.btnAnalysisOutputDir.Text = "Select";
            this.btnAnalysisOutputDir.UseVisualStyleBackColor = true;
            this.btnAnalysisOutputDir.Click += new System.EventHandler(this.btnAnalysisOutputDir_Click);
            // 
            // lblAnalysisOutputDir
            // 
            this.lblAnalysisOutputDir.AutoSize = true;
            this.lblAnalysisOutputDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisOutputDir.Location = new System.Drawing.Point(30, 120);
            this.lblAnalysisOutputDir.Name = "lblAnalysisOutputDir";
            this.lblAnalysisOutputDir.Size = new System.Drawing.Size(190, 15);
            this.lblAnalysisOutputDir.TabIndex = 15;
            this.lblAnalysisOutputDir.Text = "2: Select a directory for the output:";
            // 
            // lblAnalysisFile
            // 
            this.lblAnalysisFile.AutoSize = true;
            this.lblAnalysisFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisFile.Location = new System.Drawing.Point(30, 67);
            this.lblAnalysisFile.Name = "lblAnalysisFile";
            this.lblAnalysisFile.Size = new System.Drawing.Size(185, 15);
            this.lblAnalysisFile.TabIndex = 14;
            this.lblAnalysisFile.Text = "1: Select an audio file to analyse:";
            // 
            // btnAnalysisFile
            // 
            this.btnAnalysisFile.Location = new System.Drawing.Point(234, 64);
            this.btnAnalysisFile.Name = "btnAnalysisFile";
            this.btnAnalysisFile.Size = new System.Drawing.Size(120, 23);
            this.btnAnalysisFile.TabIndex = 2;
            this.btnAnalysisFile.Text = "Select";
            this.btnAnalysisFile.UseVisualStyleBackColor = true;
            this.btnAnalysisFile.Click += new System.EventHandler(this.btnAnalysisFile_Click);
            // 
            // tabPageSourceFiles
            // 
            this.tabPageSourceFiles.Controls.Add(this.label4);
            this.tabPageSourceFiles.Controls.Add(this.comboBoxSourceFileAnalysisType);
            this.tabPageSourceFiles.Controls.Add(this.btnAnalyseSelectedAudioFiles);
            this.tabPageSourceFiles.Controls.Add(this.dataGridViewFileList);
            this.tabPageSourceFiles.Controls.Add(this.label1);
            this.tabPageSourceFiles.Controls.Add(this.btnSelectSourceDirectory);
            this.tabPageSourceFiles.Controls.Add(this.btnUpdateSourceFileList);
            this.tabPageSourceFiles.Controls.Add(this.tfSourceDirectory);
            this.tabPageSourceFiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageSourceFiles.Name = "tabPageSourceFiles";
            this.tabPageSourceFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSourceFiles.Size = new System.Drawing.Size(1450, 655);
            this.tabPageSourceFiles.TabIndex = 2;
            this.tabPageSourceFiles.Text = "Source File";
            this.tabPageSourceFiles.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Analysis Type:";
            // 
            // comboBoxSourceFileAnalysisType
            // 
            this.comboBoxSourceFileAnalysisType.FormattingEnabled = true;
            this.comboBoxSourceFileAnalysisType.Location = new System.Drawing.Point(87, 6);
            this.comboBoxSourceFileAnalysisType.Name = "comboBoxSourceFileAnalysisType";
            this.comboBoxSourceFileAnalysisType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSourceFileAnalysisType.TabIndex = 14;
            // 
            // btnAnalyseSelectedAudioFiles
            // 
            this.btnAnalyseSelectedAudioFiles.Location = new System.Drawing.Point(840, 4);
            this.btnAnalyseSelectedAudioFiles.Name = "btnAnalyseSelectedAudioFiles";
            this.btnAnalyseSelectedAudioFiles.Size = new System.Drawing.Size(120, 23);
            this.btnAnalyseSelectedAudioFiles.TabIndex = 1;
            this.btnAnalyseSelectedAudioFiles.Text = "Analyse Audio File";
            this.btnAnalyseSelectedAudioFiles.UseVisualStyleBackColor = true;
            this.btnAnalyseSelectedAudioFiles.Click += new System.EventHandler(this.btnAnalyseSelectedAudioFiles_Click);
            // 
            // dataGridViewFileList
            // 
            this.dataGridViewFileList.AllowUserToAddRows = false;
            this.dataGridViewFileList.AllowUserToDeleteRows = false;
            this.dataGridViewFileList.AllowUserToResizeRows = false;
            this.dataGridViewFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewFileList.AutoGenerateColumns = false;
            this.dataGridViewFileList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewFileList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewFileList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFileList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.selectedDataGridViewCheckBoxColumn,
            this.fileNameDataGridViewTextBoxColumn,
            this.fileDateDataGridViewTextBoxColumn,
            this.durationDataGridViewTextBoxColumn,
            this.fileLengthDataGridViewTextBoxColumn,
            this.mediaTypeDataGridViewTextBoxColumn});
            this.dataGridViewFileList.DataSource = this.mediaFileItemBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewFileList.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewFileList.Location = new System.Drawing.Point(6, 37);
            this.dataGridViewFileList.Name = "dataGridViewFileList";
            this.dataGridViewFileList.ReadOnly = true;
            this.dataGridViewFileList.Size = new System.Drawing.Size(1438, 612);
            this.dataGridViewFileList.TabIndex = 0;
            this.dataGridViewFileList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellClick);
            this.dataGridViewFileList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellContentClick);
            this.dataGridViewFileList.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridViewFileList_CellFormatting);
            this.dataGridViewFileList.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridViewFileListSourceFileList_CellPainting);
            this.dataGridViewFileList.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellValueChanged);
            this.dataGridViewFileList.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewFileListSourceFileList_CurrentCellDirtyStateChanged);
            // 
            // selectedDataGridViewCheckBoxColumn
            // 
            this.selectedDataGridViewCheckBoxColumn.HeaderText = "";
            this.selectedDataGridViewCheckBoxColumn.Name = "selectedDataGridViewCheckBoxColumn";
            this.selectedDataGridViewCheckBoxColumn.ReadOnly = true;
            this.selectedDataGridViewCheckBoxColumn.Width = 5;
            // 
            // fileNameDataGridViewTextBoxColumn
            // 
            this.fileNameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn.HeaderText = "File Name";
            this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileNameDataGridViewTextBoxColumn.Width = 79;
            // 
            // fileDateDataGridViewTextBoxColumn
            // 
            this.fileDateDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.fileDateDataGridViewTextBoxColumn.DataPropertyName = "LastModified";
            this.fileDateDataGridViewTextBoxColumn.HeaderText = "Last Modified";
            this.fileDateDataGridViewTextBoxColumn.Name = "fileDateDataGridViewTextBoxColumn";
            this.fileDateDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileDateDataGridViewTextBoxColumn.Width = 95;
            // 
            // durationDataGridViewTextBoxColumn
            // 
            this.durationDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.durationDataGridViewTextBoxColumn.DataPropertyName = "Duration";
            this.durationDataGridViewTextBoxColumn.HeaderText = "Duration";
            this.durationDataGridViewTextBoxColumn.Name = "durationDataGridViewTextBoxColumn";
            this.durationDataGridViewTextBoxColumn.ReadOnly = true;
            this.durationDataGridViewTextBoxColumn.Width = 72;
            // 
            // fileLengthDataGridViewTextBoxColumn
            // 
            this.fileLengthDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.fileLengthDataGridViewTextBoxColumn.DataPropertyName = "FileLength";
            this.fileLengthDataGridViewTextBoxColumn.HeaderText = "File Length";
            this.fileLengthDataGridViewTextBoxColumn.Name = "fileLengthDataGridViewTextBoxColumn";
            this.fileLengthDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileLengthDataGridViewTextBoxColumn.Width = 84;
            // 
            // mediaTypeDataGridViewTextBoxColumn
            // 
            this.mediaTypeDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.mediaTypeDataGridViewTextBoxColumn.DataPropertyName = "MediaType";
            this.mediaTypeDataGridViewTextBoxColumn.HeaderText = "MediaType";
            this.mediaTypeDataGridViewTextBoxColumn.Name = "mediaTypeDataGridViewTextBoxColumn";
            this.mediaTypeDataGridViewTextBoxColumn.ReadOnly = true;
            this.mediaTypeDataGridViewTextBoxColumn.Width = 85;
            // 
            // mediaFileItemBindingSource
            // 
            this.mediaFileItemBindingSource.DataSource = typeof(AudioBrowser.MediaFileItem);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(214, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Source Folder:";
            // 
            // btnSelectSourceDirectory
            // 
            this.btnSelectSourceDirectory.CausesValidation = false;
            this.btnSelectSourceDirectory.Location = new System.Drawing.Point(574, 4);
            this.btnSelectSourceDirectory.Name = "btnSelectSourceDirectory";
            this.btnSelectSourceDirectory.Size = new System.Drawing.Size(120, 23);
            this.btnSelectSourceDirectory.TabIndex = 9;
            this.btnSelectSourceDirectory.Text = "Select Source Folder";
            this.btnSelectSourceDirectory.UseVisualStyleBackColor = true;
            this.btnSelectSourceDirectory.Click += new System.EventHandler(this.btnSelectSourceDirectory_Click);
            // 
            // btnUpdateSourceFileList
            // 
            this.btnUpdateSourceFileList.CausesValidation = false;
            this.btnUpdateSourceFileList.Location = new System.Drawing.Point(705, 4);
            this.btnUpdateSourceFileList.Name = "btnUpdateSourceFileList";
            this.btnUpdateSourceFileList.Size = new System.Drawing.Size(120, 23);
            this.btnUpdateSourceFileList.TabIndex = 10;
            this.btnUpdateSourceFileList.Text = "Update File List";
            this.btnUpdateSourceFileList.UseVisualStyleBackColor = true;
            this.btnUpdateSourceFileList.Click += new System.EventHandler(this.btnUpdateSourceFiles_Click);
            // 
            // tfSourceDirectory
            // 
            this.tfSourceDirectory.Location = new System.Drawing.Point(296, 6);
            this.tfSourceDirectory.Name = "tfSourceDirectory";
            this.tfSourceDirectory.Size = new System.Drawing.Size(272, 20);
            this.tfSourceDirectory.TabIndex = 8;
            // 
            // tabPageDisplay
            // 
            this.tabPageDisplay.Controls.Add(this.btnViewFileOfIndices);
            this.tabPageDisplay.Controls.Add(this.labelCSVHeaders);
            this.tabPageDisplay.Controls.Add(this.listBoxDisplayedTracks);
            this.tabPageDisplay.Controls.Add(this.panelDisplayImages);
            this.tabPageDisplay.Controls.Add(this.checkBoxSonogramAnnotate);
            this.tabPageDisplay.Controls.Add(this.checkBoxSonnogramNoiseReduce);
            this.tabPageDisplay.Controls.Add(this.labelSonogramFileName);
            this.tabPageDisplay.Controls.Add(this.labelCursorValue);
            this.tabPageDisplay.Controls.Add(this.labelSonogramName);
            this.tabPageDisplay.Controls.Add(this.buttonRefreshSonogram);
            this.tabPageDisplay.Controls.Add(this.buttonAudacityRun);
            this.tabPageDisplay.Controls.Add(this.textBoxCursorValue);
            this.tabPageDisplay.Controls.Add(this.label3);
            this.tabPageDisplay.Controls.Add(this.textBoxCursorLocation);
            this.tabPageDisplay.Location = new System.Drawing.Point(4, 22);
            this.tabPageDisplay.Name = "tabPageDisplay";
            this.tabPageDisplay.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDisplay.Size = new System.Drawing.Size(1450, 655);
            this.tabPageDisplay.TabIndex = 0;
            this.tabPageDisplay.Text = "Display Indices File";
            this.tabPageDisplay.UseVisualStyleBackColor = true;
            // 
            // btnViewFileOfIndices
            // 
            this.btnViewFileOfIndices.Location = new System.Drawing.Point(16, 17);
            this.btnViewFileOfIndices.Name = "btnViewFileOfIndices";
            this.btnViewFileOfIndices.Size = new System.Drawing.Size(146, 36);
            this.btnViewFileOfIndices.TabIndex = 12;
            this.btnViewFileOfIndices.Text = "View File of Indices ";
            this.btnViewFileOfIndices.UseVisualStyleBackColor = true;
            this.btnViewFileOfIndices.Click += new System.EventHandler(this.btnViewFileOfIndices_Click);
            // 
            // labelCSVHeaders
            // 
            this.labelCSVHeaders.AutoSize = true;
            this.labelCSVHeaders.Location = new System.Drawing.Point(3, 214);
            this.labelCSVHeaders.Name = "labelCSVHeaders";
            this.labelCSVHeaders.Size = new System.Drawing.Size(101, 13);
            this.labelCSVHeaders.TabIndex = 11;
            this.labelCSVHeaders.Text = "Headers in CSV File";
            // 
            // listBoxDisplayedTracks
            // 
            this.listBoxDisplayedTracks.FormattingEnabled = true;
            this.listBoxDisplayedTracks.Location = new System.Drawing.Point(6, 230);
            this.listBoxDisplayedTracks.Name = "listBoxDisplayedTracks";
            this.listBoxDisplayedTracks.Size = new System.Drawing.Size(163, 225);
            this.listBoxDisplayedTracks.TabIndex = 10;
            // 
            // panelDisplayImages
            // 
            this.panelDisplayImages.AccessibleName = "";
            this.panelDisplayImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDisplayImages.AutoScroll = true;
            this.panelDisplayImages.Controls.Add(this.panelDisplayImageAndTrackBar);
            this.panelDisplayImages.Controls.Add(this.panelDisplaySpectrogram);
            this.panelDisplayImages.Controls.Add(this.labelSourceFileName);
            this.panelDisplayImages.Controls.Add(this.labelDisplayInfo);
            this.panelDisplayImages.Location = new System.Drawing.Point(178, 6);
            this.panelDisplayImages.Name = "panelDisplayImages";
            this.panelDisplayImages.Size = new System.Drawing.Size(1800, 629);
            this.panelDisplayImages.TabIndex = 9;
            this.panelDisplayImages.Tag = "";
            // 
            // panelDisplayImageAndTrackBar
            // 
            this.panelDisplayImageAndTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDisplayImageAndTrackBar.AutoScroll = true;
            this.panelDisplayImageAndTrackBar.Controls.Add(this.pictureBoxVisualIndices);
            this.panelDisplayImageAndTrackBar.Controls.Add(this.pictureBoxBarTrack);
            this.panelDisplayImageAndTrackBar.Location = new System.Drawing.Point(3, 28);
            this.panelDisplayImageAndTrackBar.Name = "panelDisplayImageAndTrackBar";
            this.panelDisplayImageAndTrackBar.Size = new System.Drawing.Size(1369, 395);
            this.panelDisplayImageAndTrackBar.TabIndex = 7;
            // 
            // pictureBoxVisualIndices
            // 
            this.pictureBoxVisualIndices.BackColor = System.Drawing.Color.Black;
            this.pictureBoxVisualIndices.Location = new System.Drawing.Point(18, 6);
            this.pictureBoxVisualIndices.MinimumSize = new System.Drawing.Size(100, 100);
            this.pictureBoxVisualIndices.Name = "pictureBoxVisualIndices";
            this.pictureBoxVisualIndices.Size = new System.Drawing.Size(1800, 340);
            this.pictureBoxVisualIndices.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxVisualIndices.TabIndex = 0;
            this.pictureBoxVisualIndices.TabStop = false;
            this.pictureBoxVisualIndices.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxVisualIndex_MouseClick);
            this.pictureBoxVisualIndices.MouseHover += new System.EventHandler(this.pictureBoxVisualIndex_MouseHover);
            this.pictureBoxVisualIndices.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxVisualIndex_MouseMove);
            // 
            // pictureBoxBarTrack
            // 
            this.pictureBoxBarTrack.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.pictureBoxBarTrack.Location = new System.Drawing.Point(18, 349);
            this.pictureBoxBarTrack.Name = "pictureBoxBarTrack";
            this.pictureBoxBarTrack.Size = new System.Drawing.Size(1583, 24);
            this.pictureBoxBarTrack.TabIndex = 3;
            this.pictureBoxBarTrack.TabStop = false;
            // 
            // panelDisplaySpectrogram
            // 
            this.panelDisplaySpectrogram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDisplaySpectrogram.AutoScroll = true;
            this.panelDisplaySpectrogram.Controls.Add(this.pictureBoxSonogram);
            this.panelDisplaySpectrogram.Location = new System.Drawing.Point(3, 463);
            this.panelDisplaySpectrogram.Name = "panelDisplaySpectrogram";
            this.panelDisplaySpectrogram.Size = new System.Drawing.Size(1369, 288);
            this.panelDisplaySpectrogram.TabIndex = 8;
            // 
            // pictureBoxSonogram
            // 
            this.pictureBoxSonogram.BackColor = System.Drawing.Color.DarkGray;
            this.pictureBoxSonogram.Location = new System.Drawing.Point(0, -56);
            this.pictureBoxSonogram.Name = "pictureBoxSonogram";
            this.pictureBoxSonogram.Size = new System.Drawing.Size(1800, 300);
            this.pictureBoxSonogram.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxSonogram.TabIndex = 2;
            this.pictureBoxSonogram.TabStop = false;
            // 
            // labelSourceFileName
            // 
            this.labelSourceFileName.AutoSize = true;
            this.labelSourceFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSourceFileName.Location = new System.Drawing.Point(19, 12);
            this.labelSourceFileName.Name = "labelSourceFileName";
            this.labelSourceFileName.Size = new System.Drawing.Size(63, 13);
            this.labelSourceFileName.TabIndex = 3;
            this.labelSourceFileName.Text = "File Name";
            // 
            // labelDisplayInfo
            // 
            this.labelDisplayInfo.AutoSize = true;
            this.labelDisplayInfo.Location = new System.Drawing.Point(759, 11);
            this.labelDisplayInfo.Name = "labelDisplayInfo";
            this.labelDisplayInfo.Size = new System.Drawing.Size(37, 13);
            this.labelDisplayInfo.TabIndex = 4;
            this.labelDisplayInfo.Text = "..........";
            // 
            // checkBoxSonogramAnnotate
            // 
            this.checkBoxSonogramAnnotate.AutoSize = true;
            this.checkBoxSonogramAnnotate.Location = new System.Drawing.Point(24, 539);
            this.checkBoxSonogramAnnotate.Name = "checkBoxSonogramAnnotate";
            this.checkBoxSonogramAnnotate.Size = new System.Drawing.Size(118, 17);
            this.checkBoxSonogramAnnotate.TabIndex = 5;
            this.checkBoxSonogramAnnotate.Text = "Annotate sonogram";
            this.checkBoxSonogramAnnotate.UseVisualStyleBackColor = true;
            // 
            // checkBoxSonnogramNoiseReduce
            // 
            this.checkBoxSonnogramNoiseReduce.AutoSize = true;
            this.checkBoxSonnogramNoiseReduce.Location = new System.Drawing.Point(24, 516);
            this.checkBoxSonnogramNoiseReduce.Name = "checkBoxSonnogramNoiseReduce";
            this.checkBoxSonnogramNoiseReduce.Size = new System.Drawing.Size(138, 17);
            this.checkBoxSonnogramNoiseReduce.TabIndex = 4;
            this.checkBoxSonnogramNoiseReduce.Text = "Noise reduce sonogram";
            this.checkBoxSonnogramNoiseReduce.UseVisualStyleBackColor = true;
            // 
            // labelSonogramFileName
            // 
            this.labelSonogramFileName.AutoSize = true;
            this.labelSonogramFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSonogramFileName.Location = new System.Drawing.Point(8, 486);
            this.labelSonogramFileName.Name = "labelSonogramFileName";
            this.labelSonogramFileName.Size = new System.Drawing.Size(116, 13);
            this.labelSonogramFileName.TabIndex = 3;
            this.labelSonogramFileName.Text = "sonogram file name";
            // 
            // labelCursorValue
            // 
            this.labelCursorValue.AutoSize = true;
            this.labelCursorValue.Location = new System.Drawing.Point(3, 149);
            this.labelCursorValue.Name = "labelCursorValue";
            this.labelCursorValue.Size = new System.Drawing.Size(67, 13);
            this.labelCursorValue.TabIndex = 6;
            this.labelCursorValue.Text = "Cursor Value";
            // 
            // labelSonogramName
            // 
            this.labelSonogramName.AutoSize = true;
            this.labelSonogramName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSonogramName.Location = new System.Drawing.Point(8, 469);
            this.labelSonogramName.Name = "labelSonogramName";
            this.labelSonogramName.Size = new System.Drawing.Size(102, 15);
            this.labelSonogramName.TabIndex = 2;
            this.labelSonogramName.Text = "Sonogram Name";
            // 
            // buttonRefreshSonogram
            // 
            this.buttonRefreshSonogram.Location = new System.Drawing.Point(24, 562);
            this.buttonRefreshSonogram.Name = "buttonRefreshSonogram";
            this.buttonRefreshSonogram.Size = new System.Drawing.Size(120, 28);
            this.buttonRefreshSonogram.TabIndex = 1;
            this.buttonRefreshSonogram.Text = "Refresh Sonogram";
            this.buttonRefreshSonogram.UseVisualStyleBackColor = true;
            this.buttonRefreshSonogram.Click += new System.EventHandler(this.buttonRefreshSonogram_Click);
            // 
            // buttonAudacityRun
            // 
            this.buttonAudacityRun.Location = new System.Drawing.Point(24, 596);
            this.buttonAudacityRun.Name = "buttonAudacityRun";
            this.buttonAudacityRun.Size = new System.Drawing.Size(120, 28);
            this.buttonAudacityRun.TabIndex = 0;
            this.buttonAudacityRun.Text = "Run Audacity";
            this.buttonAudacityRun.UseVisualStyleBackColor = true;
            this.buttonAudacityRun.Click += new System.EventHandler(this.btnRunAudacity_Click);
            // 
            // textBoxCursorValue
            // 
            this.textBoxCursorValue.Location = new System.Drawing.Point(6, 165);
            this.textBoxCursorValue.Name = "textBoxCursorValue";
            this.textBoxCursorValue.Size = new System.Drawing.Size(160, 20);
            this.textBoxCursorValue.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Cursor Location";
            // 
            // textBoxCursorLocation
            // 
            this.textBoxCursorLocation.Location = new System.Drawing.Point(6, 126);
            this.textBoxCursorLocation.Name = "textBoxCursorLocation";
            this.textBoxCursorLocation.Size = new System.Drawing.Size(160, 20);
            this.textBoxCursorLocation.TabIndex = 2;
            // 
            // tabPageConsole
            // 
            this.tabPageConsole.Controls.Add(this.textBoxConsole);
            this.tabPageConsole.Location = new System.Drawing.Point(4, 22);
            this.tabPageConsole.Name = "tabPageConsole";
            this.tabPageConsole.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConsole.Size = new System.Drawing.Size(1450, 655);
            this.tabPageConsole.TabIndex = 1;
            this.tabPageConsole.Text = "Console";
            this.tabPageConsole.UseVisualStyleBackColor = true;
            // 
            // textBoxConsole
            // 
            this.textBoxConsole.BackColor = System.Drawing.Color.Black;
            this.textBoxConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxConsole.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold);
            this.textBoxConsole.ForeColor = System.Drawing.Color.Lime;
            this.textBoxConsole.Location = new System.Drawing.Point(3, 3);
            this.textBoxConsole.Multiline = true;
            this.textBoxConsole.Name = "textBoxConsole";
            this.textBoxConsole.ReadOnly = true;
            this.textBoxConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxConsole.Size = new System.Drawing.Size(1444, 649);
            this.textBoxConsole.TabIndex = 0;
            // 
            // tabPageSearchCsv
            // 
            this.tabPageSearchCsv.Controls.Add(this.panelSearchEntries);
            this.tabPageSearchCsv.Controls.Add(this.btnFindInCSV);
            this.tabPageSearchCsv.Controls.Add(this.btnSelectCSVSourceFolder);
            this.tabPageSearchCsv.Controls.Add(this.textBoxCSVSourceFolderPath);
            this.tabPageSearchCsv.Controls.Add(this.label6);
            this.tabPageSearchCsv.Controls.Add(this.btnAddFilterLine);
            this.tabPageSearchCsv.Location = new System.Drawing.Point(4, 22);
            this.tabPageSearchCsv.Name = "tabPageSearchCsv";
            this.tabPageSearchCsv.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSearchCsv.Size = new System.Drawing.Size(1450, 655);
            this.tabPageSearchCsv.TabIndex = 4;
            this.tabPageSearchCsv.Text = "Filter CSV Files";
            this.tabPageSearchCsv.UseVisualStyleBackColor = true;
            // 
            // panelSearchEntries
            // 
            this.panelSearchEntries.Controls.Add(this.btnSearchRemoveFilterLine);
            this.panelSearchEntries.Controls.Add(this.tfSearchFieldMax);
            this.panelSearchEntries.Controls.Add(this.tfSearchFieldMin);
            this.panelSearchEntries.Controls.Add(this.lblSearchFieldMax);
            this.panelSearchEntries.Controls.Add(this.lblSearchFieldName);
            this.panelSearchEntries.Controls.Add(this.lblSearchFieldMin);
            this.panelSearchEntries.Controls.Add(this.tfSearchFieldName);
            this.panelSearchEntries.Location = new System.Drawing.Point(8, 34);
            this.panelSearchEntries.Name = "panelSearchEntries";
            this.panelSearchEntries.Size = new System.Drawing.Size(1200, 565);
            this.panelSearchEntries.TabIndex = 19;
            // 
            // btnSearchRemoveFilterLine
            // 
            this.btnSearchRemoveFilterLine.Enabled = false;
            this.btnSearchRemoveFilterLine.Location = new System.Drawing.Point(598, 10);
            this.btnSearchRemoveFilterLine.Name = "btnSearchRemoveFilterLine";
            this.btnSearchRemoveFilterLine.Size = new System.Drawing.Size(118, 23);
            this.btnSearchRemoveFilterLine.TabIndex = 22;
            this.btnSearchRemoveFilterLine.Text = "Remove This Filter";
            this.btnSearchRemoveFilterLine.UseVisualStyleBackColor = true;
            this.btnSearchRemoveFilterLine.Click += new System.EventHandler(this.btnSearchRemoveFilterLine_Click);
            // 
            // tfSearchFieldMax
            // 
            this.tfSearchFieldMax.Location = new System.Drawing.Point(504, 11);
            this.tfSearchFieldMax.MaxLength = 10;
            this.tfSearchFieldMax.Name = "tfSearchFieldMax";
            this.tfSearchFieldMax.Size = new System.Drawing.Size(88, 20);
            this.tfSearchFieldMax.TabIndex = 21;
            // 
            // tfSearchFieldMin
            // 
            this.tfSearchFieldMin.Location = new System.Drawing.Point(350, 11);
            this.tfSearchFieldMin.MaxLength = 10;
            this.tfSearchFieldMin.Name = "tfSearchFieldMin";
            this.tfSearchFieldMin.Size = new System.Drawing.Size(88, 20);
            this.tfSearchFieldMin.TabIndex = 20;
            // 
            // lblSearchFieldMax
            // 
            this.lblSearchFieldMax.AutoSize = true;
            this.lblSearchFieldMax.Location = new System.Drawing.Point(444, 15);
            this.lblSearchFieldMax.Name = "lblSearchFieldMax";
            this.lblSearchFieldMax.Size = new System.Drawing.Size(54, 13);
            this.lblSearchFieldMax.TabIndex = 19;
            this.lblSearchFieldMax.Text = "Maximum:";
            // 
            // lblSearchFieldName
            // 
            this.lblSearchFieldName.AutoSize = true;
            this.lblSearchFieldName.Location = new System.Drawing.Point(6, 14);
            this.lblSearchFieldName.Name = "lblSearchFieldName";
            this.lblSearchFieldName.Size = new System.Drawing.Size(63, 13);
            this.lblSearchFieldName.TabIndex = 18;
            this.lblSearchFieldName.Text = "Field Name:";
            // 
            // lblSearchFieldMin
            // 
            this.lblSearchFieldMin.AutoSize = true;
            this.lblSearchFieldMin.Location = new System.Drawing.Point(293, 15);
            this.lblSearchFieldMin.Name = "lblSearchFieldMin";
            this.lblSearchFieldMin.Size = new System.Drawing.Size(51, 13);
            this.lblSearchFieldMin.TabIndex = 17;
            this.lblSearchFieldMin.Text = "Minimum:";
            // 
            // tfSearchFieldName
            // 
            this.tfSearchFieldName.Location = new System.Drawing.Point(75, 12);
            this.tfSearchFieldName.Name = "tfSearchFieldName";
            this.tfSearchFieldName.Size = new System.Drawing.Size(212, 20);
            this.tfSearchFieldName.TabIndex = 16;
            // 
            // btnFindInCSV
            // 
            this.btnFindInCSV.Location = new System.Drawing.Point(797, 6);
            this.btnFindInCSV.Name = "btnFindInCSV";
            this.btnFindInCSV.Size = new System.Drawing.Size(120, 23);
            this.btnFindInCSV.TabIndex = 18;
            this.btnFindInCSV.Text = "Find In CSV Files";
            this.btnFindInCSV.UseVisualStyleBackColor = true;
            this.btnFindInCSV.Click += new System.EventHandler(this.btnFindInCSV_Click);
            // 
            // btnSelectCSVSourceFolder
            // 
            this.btnSelectCSVSourceFolder.CausesValidation = false;
            this.btnSelectCSVSourceFolder.Location = new System.Drawing.Point(573, 6);
            this.btnSelectCSVSourceFolder.Name = "btnSelectCSVSourceFolder";
            this.btnSelectCSVSourceFolder.Size = new System.Drawing.Size(120, 23);
            this.btnSelectCSVSourceFolder.TabIndex = 16;
            this.btnSelectCSVSourceFolder.Text = "Select Source Folder";
            this.btnSelectCSVSourceFolder.UseVisualStyleBackColor = true;
            // 
            // textBoxCSVSourceFolderPath
            // 
            this.textBoxCSVSourceFolderPath.Location = new System.Drawing.Point(88, 8);
            this.textBoxCSVSourceFolderPath.Name = "textBoxCSVSourceFolderPath";
            this.textBoxCSVSourceFolderPath.Size = new System.Drawing.Size(479, 20);
            this.textBoxCSVSourceFolderPath.TabIndex = 15;
            this.textBoxCSVSourceFolderPath.Text = "I:\\Projects\\QUT\\QutSensors\\test-audio\\ID14";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Source Folder:";
            // 
            // btnAddFilterLine
            // 
            this.btnAddFilterLine.Location = new System.Drawing.Point(699, 6);
            this.btnAddFilterLine.Name = "btnAddFilterLine";
            this.btnAddFilterLine.Size = new System.Drawing.Size(92, 23);
            this.btnAddFilterLine.TabIndex = 0;
            this.btnAddFilterLine.Text = "Add New Filter";
            this.btnAddFilterLine.UseVisualStyleBackColor = true;
            this.btnAddFilterLine.Click += new System.EventHandler(this.btnAddSearchEntry_Click);
            // 
            // folderBrowserDialogChooseDir
            // 
            this.folderBrowserDialogChooseDir.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialogChooseDir.ShowNewFolderButton = false;
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
            // csvFileItemBindingSource
            // 
            this.csvFileItemBindingSource.DataSource = typeof(AudioBrowser.CsvFileItem);
            // 
            // tabMisc
            // 
            this.tabMisc.Controls.Add(this.btnCSV2ARFF);
            this.tabMisc.Location = new System.Drawing.Point(4, 22);
            this.tabMisc.Name = "tabMisc";
            this.tabMisc.Padding = new System.Windows.Forms.Padding(3);
            this.tabMisc.Size = new System.Drawing.Size(1450, 655);
            this.tabMisc.TabIndex = 6;
            this.tabMisc.Text = "Miscellaneous";
            this.tabMisc.UseVisualStyleBackColor = true;
            // 
            // btnCSV2ARFF
            // 
            this.btnCSV2ARFF.Location = new System.Drawing.Point(87, 130);
            this.btnCSV2ARFF.Name = "btnCSV2ARFF";
            this.btnCSV2ARFF.Size = new System.Drawing.Size(75, 23);
            this.btnCSV2ARFF.TabIndex = 0;
            this.btnCSV2ARFF.Text = "csv2Arff";
            this.btnCSV2ARFF.UseVisualStyleBackColor = true;
            this.btnCSV2ARFF.Click += new System.EventHandler(this.btnCSV2ARFF_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1460, 688);
            this.Controls.Add(this.tabControlMain);
            this.Location = new System.Drawing.Point(90, 90);
            this.MinimumSize = new System.Drawing.Size(830, 670);
            this.Name = "MainForm";
            this.Text = "Acoustic Environment Browser";
            this.tabControlMain.ResumeLayout(false);
            this.tabAnalyseFile.ResumeLayout(false);
            this.tabAnalyseFile.PerformLayout();
            this.tabPageSourceFiles.ResumeLayout(false);
            this.tabPageSourceFiles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mediaFileItemBindingSource)).EndInit();
            this.tabPageDisplay.ResumeLayout(false);
            this.tabPageDisplay.PerformLayout();
            this.panelDisplayImages.ResumeLayout(false);
            this.panelDisplayImages.PerformLayout();
            this.panelDisplayImageAndTrackBar.ResumeLayout(false);
            this.panelDisplayImageAndTrackBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualIndices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBarTrack)).EndInit();
            this.panelDisplaySpectrogram.ResumeLayout(false);
            this.panelDisplaySpectrogram.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSonogram)).EndInit();
            this.tabPageConsole.ResumeLayout(false);
            this.tabPageConsole.PerformLayout();
            this.tabPageSearchCsv.ResumeLayout(false);
            this.tabPageSearchCsv.PerformLayout();
            this.panelSearchEntries.ResumeLayout(false);
            this.panelSearchEntries.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.csvFileItemBindingSource)).EndInit();
            this.tabMisc.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TabPage tabPageSourceFiles;
        private DataGridView dataGridViewFileList;
        private TabPage tabPageDisplay;
        private TabPage tabPageConsole;
        private TextBox textBoxConsole;
        private Button btnUpdateSourceFileList;
        private Button btnSelectSourceDirectory;
        private TextBox tfSourceDirectory;
        private FolderBrowserDialog folderBrowserDialogChooseDir;
        private BindingSource mediaFileItemBindingSource;
        private Label label1;
        private System.ComponentModel.BackgroundWorker backgroundWorkerUpdateSourceFileList;
        private Button btnAnalyseSelectedAudioFiles;
        private PictureBox pictureBoxVisualIndices;
        private BindingSource csvFileItemBindingSource;
        private Label labelDisplayInfo;
        private Label labelSourceFileName;
        private TextBox textBoxCursorLocation;
        private Label label3;
        private Button buttonAudacityRun;
        private Label labelCursorValue;
        private TextBox textBoxCursorValue;
        private PictureBox pictureBoxBarTrack;
        private Button buttonRefreshSonogram;
        private CheckBox checkBoxSonogramAnnotate;
        private CheckBox checkBoxSonnogramNoiseReduce;
        private Label labelSonogramFileName;
        private Label labelSonogramName;
        public TabControl tabControlMain;
        private Panel panelDisplayImages;
        private Panel panelDisplayImageAndTrackBar;
        private Panel panelDisplaySpectrogram;
        private PictureBox pictureBoxSonogram;
        private DataGridViewCheckBoxColumn selectedDataGridViewCheckBoxColumn;
        private DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn fileDateDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn durationDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn fileLengthDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn mediaTypeDataGridViewTextBoxColumn;
        private Label label4;
        private ComboBox comboBoxSourceFileAnalysisType;
        private ListBox listBoxDisplayedTracks;
        private Label labelCSVHeaders;
        private TabPage tabPageSearchCsv;
        private Button btnSelectCSVSourceFolder;
        private TextBox textBoxCSVSourceFolderPath;
        private Label label6;
        private DataGridViewTextBoxColumn fullNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn lastModifiedDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn durationDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn mediaTypeDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn fileLengthDataGridViewTextBoxColumn1;
        private Button btnFindInCSV;
        private Panel panelSearchEntries;
        private Button btnAddFilterLine;
        private TextBox tfSearchFieldMax;
        private TextBox tfSearchFieldMin;
        private Label lblSearchFieldMax;
        private Label lblSearchFieldName;
        private Label lblSearchFieldMin;
        private TextBox tfSearchFieldName;
        private Button btnSearchRemoveFilterLine;
        private Button btnViewFileOfIndices;
        private TabPage tabAnalyseFile;
        private TextBox txtBoxAnalysisEditConfig;
        private TextBox txtBoxAnalysisOutputDir;
        private TextBox txtBoxAnalysisFile;
        private ComboBox comboAnalysisType;
        private Button btnAnalysisStart;
        private Button btnAnalysisEditConfig;
        private Label lblAnalysisStart;
        private Label lblAnalysisEditConfig;
        private Label lblAnalysisType;
        private Button btnAnalysisOutputDir;
        private Label lblAnalysisOutputDir;
        private Label lblAnalysisFile;
        private Button btnAnalysisFile;
        private TextBox textBoxAnalysisGo;
        private Label lblAnalysisPanelHeader;
        private Label label2;
        private TabPage tabMisc;
        private Button btnCSV2ARFF;


    }
}

