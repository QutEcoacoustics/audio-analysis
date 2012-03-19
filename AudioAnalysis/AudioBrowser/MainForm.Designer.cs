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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageSourceFiles = new System.Windows.Forms.TabPage();
            this.btnExtractIndiciesAllSelected = new System.Windows.Forms.Button();
            this.dataGridViewFileList = new System.Windows.Forms.DataGridView();
            this.tabPageOutputFiles = new System.Windows.Forms.TabPage();
            this.btnLoadVisualIndexAllSelected = new System.Windows.Forms.Button();
            this.dataGridCSVfiles = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumnSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tabPageDisplay = new System.Windows.Forms.TabPage();
            this.splitContainerDisplay = new System.Windows.Forms.SplitContainer();
            this.panelDisplayControls = new System.Windows.Forms.Panel();
            this.panelSonogramControls = new System.Windows.Forms.Panel();
            this.checkBoxSonogramAnnotate = new System.Windows.Forms.CheckBox();
            this.checkBoxSonnogramNoiseReduce = new System.Windows.Forms.CheckBox();
            this.labelSonogramFileName = new System.Windows.Forms.Label();
            this.labelSonogramName = new System.Windows.Forms.Label();
            this.buttonRefreshSonogram = new System.Windows.Forms.Button();
            this.buttonAudacityRun = new System.Windows.Forms.Button();
            this.labelCursorValue = new System.Windows.Forms.Label();
            this.textBoxCursorValue = new System.Windows.Forms.TextBox();
            this.labelSourceFileDurationInMinutes = new System.Windows.Forms.Label();
            this.labelSourceFileName = new System.Windows.Forms.Label();
            this.textBoxCursorLocation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panelDisplayVisual = new System.Windows.Forms.Panel();
            this.panelSonogram = new System.Windows.Forms.Panel();
            this.hScrollBarSonogram = new System.Windows.Forms.HScrollBar();
            this.pictureBoxSonogram = new System.Windows.Forms.PictureBox();
            this.pictureBoxBarTrack = new System.Windows.Forms.PictureBox();
            this.pictureBoxVisualIndex = new System.Windows.Forms.PictureBox();
            this.tabPageConsole = new System.Windows.Forms.TabPage();
            this.textBoxConsole = new System.Windows.Forms.TextBox();
            this.btnUpdateSourceFileList = new System.Windows.Forms.Button();
            this.btnSelectSourceDirectory = new System.Windows.Forms.Button();
            this.tfSourceDirectory = new System.Windows.Forms.TextBox();
            this.folderBrowserDialogChooseDir = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnUpdateOutputFileList = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tfOutputDirectory = new System.Windows.Forms.TextBox();
            this.btnSelectOutputDirectory = new System.Windows.Forms.Button();
            this.backgroundWorkerUpdateSourceFileList = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerUpdateCSVFileList = new System.ComponentModel.BackgroundWorker();
            this.mediaFileItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataGridViewTextBoxColumnFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumnFileLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.csvFileItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.selectedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.durationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mediaTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileLengthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControlMain.SuspendLayout();
            this.tabPageSourceFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileList)).BeginInit();
            this.tabPageOutputFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCSVfiles)).BeginInit();
            this.tabPageDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDisplay)).BeginInit();
            this.splitContainerDisplay.Panel1.SuspendLayout();
            this.splitContainerDisplay.Panel2.SuspendLayout();
            this.splitContainerDisplay.SuspendLayout();
            this.panelDisplayControls.SuspendLayout();
            this.panelSonogramControls.SuspendLayout();
            this.panelDisplayVisual.SuspendLayout();
            this.panelSonogram.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSonogram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBarTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualIndex)).BeginInit();
            this.tabPageConsole.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mediaFileItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.csvFileItemBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageSourceFiles);
            this.tabControlMain.Controls.Add(this.tabPageOutputFiles);
            this.tabControlMain.Controls.Add(this.tabPageDisplay);
            this.tabControlMain.Controls.Add(this.tabPageConsole);
            this.tabControlMain.Location = new System.Drawing.Point(0, 95);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1794, 720);
            this.tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlMain.TabIndex = 0;
            // 
            // tabPageSourceFiles
            // 
            this.tabPageSourceFiles.Controls.Add(this.btnExtractIndiciesAllSelected);
            this.tabPageSourceFiles.Controls.Add(this.dataGridViewFileList);
            this.tabPageSourceFiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageSourceFiles.Name = "tabPageSourceFiles";
            this.tabPageSourceFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSourceFiles.Size = new System.Drawing.Size(1786, 694);
            this.tabPageSourceFiles.TabIndex = 2;
            this.tabPageSourceFiles.Text = "Source Audio Files";
            this.tabPageSourceFiles.UseVisualStyleBackColor = true;
            // 
            // btnExtractIndiciesAllSelected
            // 
            this.btnExtractIndiciesAllSelected.Location = new System.Drawing.Point(6, 6);
            this.btnExtractIndiciesAllSelected.Name = "btnExtractIndiciesAllSelected";
            this.btnExtractIndiciesAllSelected.Size = new System.Drawing.Size(185, 23);
            this.btnExtractIndiciesAllSelected.TabIndex = 1;
            this.btnExtractIndiciesAllSelected.Text = "Extract Indicies For Selected Items";
            this.btnExtractIndiciesAllSelected.UseVisualStyleBackColor = true;
            this.btnExtractIndiciesAllSelected.Click += new System.EventHandler(this.btnExtractIndiciesAllSelected_Click);
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
            this.dataGridViewFileList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
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
            this.durationDataGridViewTextBoxColumn,
            this.mediaTypeDataGridViewTextBoxColumn,
            this.fileLengthDataGridViewTextBoxColumn});
            this.dataGridViewFileList.DataSource = this.mediaFileItemBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewFileList.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewFileList.Location = new System.Drawing.Point(3, 35);
            this.dataGridViewFileList.Name = "dataGridViewFileList";
            this.dataGridViewFileList.Size = new System.Drawing.Size(1453, 500);
            this.dataGridViewFileList.TabIndex = 0;
            this.dataGridViewFileList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellClick);
            this.dataGridViewFileList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellContentClick);
            this.dataGridViewFileList.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridViewFileListSourceFileList_CellPainting);
            this.dataGridViewFileList.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellValueChanged);
            this.dataGridViewFileList.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewFileListSourceFileList_CurrentCellDirtyStateChanged);
            // 
            // tabPageOutputFiles
            // 
            this.tabPageOutputFiles.Controls.Add(this.btnLoadVisualIndexAllSelected);
            this.tabPageOutputFiles.Controls.Add(this.dataGridCSVfiles);
            this.tabPageOutputFiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageOutputFiles.Name = "tabPageOutputFiles";
            this.tabPageOutputFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOutputFiles.Size = new System.Drawing.Size(1786, 694);
            this.tabPageOutputFiles.TabIndex = 3;
            this.tabPageOutputFiles.Text = "Output Files";
            this.tabPageOutputFiles.UseVisualStyleBackColor = true;
            // 
            // btnLoadVisualIndexAllSelected
            // 
            this.btnLoadVisualIndexAllSelected.Location = new System.Drawing.Point(6, 3);
            this.btnLoadVisualIndexAllSelected.Name = "btnLoadVisualIndexAllSelected";
            this.btnLoadVisualIndexAllSelected.Size = new System.Drawing.Size(209, 29);
            this.btnLoadVisualIndexAllSelected.TabIndex = 2;
            this.btnLoadVisualIndexAllSelected.Text = "Load Visual Index for Selected CSV file";
            this.btnLoadVisualIndexAllSelected.UseVisualStyleBackColor = true;
            this.btnLoadVisualIndexAllSelected.Click += new System.EventHandler(this.btnLoadVisualIndexAllSelected_Click);
            // 
            // dataGridCSVfiles
            // 
            this.dataGridCSVfiles.AllowUserToAddRows = false;
            this.dataGridCSVfiles.AllowUserToDeleteRows = false;
            this.dataGridCSVfiles.AllowUserToResizeRows = false;
            this.dataGridCSVfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridCSVfiles.AutoGenerateColumns = false;
            this.dataGridCSVfiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridCSVfiles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridCSVfiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCSVfiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumnSelected,
            this.dataGridViewTextBoxColumnFileName,
            this.dataGridViewTextBoxColumnFileLength});
            this.dataGridCSVfiles.DataSource = this.csvFileItemBindingSource;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridCSVfiles.DefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridCSVfiles.Location = new System.Drawing.Point(4, 35);
            this.dataGridCSVfiles.MultiSelect = false;
            this.dataGridCSVfiles.Name = "dataGridCSVfiles";
            this.dataGridCSVfiles.Size = new System.Drawing.Size(1474, 500);
            this.dataGridCSVfiles.TabIndex = 1;
            this.dataGridCSVfiles.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListCSVFileList_CellClick);
            this.dataGridCSVfiles.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListCSVFileList_CellContentClick);
            this.dataGridCSVfiles.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridViewFileListCSVFileList_CellPainting);
            this.dataGridCSVfiles.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListCSVFileList_CellValueChanged);
            this.dataGridCSVfiles.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewFileListCSVFileList_CurrentCellDirtyStateChanged);
            // 
            // dataGridViewCheckBoxColumnSelected
            // 
            this.dataGridViewCheckBoxColumnSelected.HeaderText = "";
            this.dataGridViewCheckBoxColumnSelected.Name = "dataGridViewCheckBoxColumnSelected";
            this.dataGridViewCheckBoxColumnSelected.ReadOnly = true;
            this.dataGridViewCheckBoxColumnSelected.Width = 5;
            // 
            // tabPageDisplay
            // 
            this.tabPageDisplay.Controls.Add(this.splitContainerDisplay);
            this.tabPageDisplay.Location = new System.Drawing.Point(4, 22);
            this.tabPageDisplay.Name = "tabPageDisplay";
            this.tabPageDisplay.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDisplay.Size = new System.Drawing.Size(1786, 694);
            this.tabPageDisplay.TabIndex = 0;
            this.tabPageDisplay.Text = "Display";
            this.tabPageDisplay.UseVisualStyleBackColor = true;
            // 
            // splitContainerDisplay
            // 
            this.splitContainerDisplay.BackColor = System.Drawing.Color.Transparent;
            this.splitContainerDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerDisplay.Location = new System.Drawing.Point(3, 3);
            this.splitContainerDisplay.Name = "splitContainerDisplay";
            // 
            // splitContainerDisplay.Panel1
            // 
            this.splitContainerDisplay.Panel1.Controls.Add(this.panelDisplayControls);
            this.splitContainerDisplay.Panel1MinSize = 175;
            // 
            // splitContainerDisplay.Panel2
            // 
            this.splitContainerDisplay.Panel2.AutoScroll = true;
            this.splitContainerDisplay.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.splitContainerDisplay.Panel2.Controls.Add(this.panelDisplayVisual);
            this.splitContainerDisplay.Panel2MinSize = 639;
            this.splitContainerDisplay.Size = new System.Drawing.Size(1780, 688);
            this.splitContainerDisplay.SplitterDistance = 190;
            this.splitContainerDisplay.TabIndex = 0;
            // 
            // panelDisplayControls
            // 
            this.panelDisplayControls.BackColor = System.Drawing.Color.Transparent;
            this.panelDisplayControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDisplayControls.Controls.Add(this.panelSonogramControls);
            this.panelDisplayControls.Controls.Add(this.labelCursorValue);
            this.panelDisplayControls.Controls.Add(this.textBoxCursorValue);
            this.panelDisplayControls.Controls.Add(this.labelSourceFileDurationInMinutes);
            this.panelDisplayControls.Controls.Add(this.labelSourceFileName);
            this.panelDisplayControls.Controls.Add(this.textBoxCursorLocation);
            this.panelDisplayControls.Controls.Add(this.label3);
            this.panelDisplayControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDisplayControls.Location = new System.Drawing.Point(0, 0);
            this.panelDisplayControls.Name = "panelDisplayControls";
            this.panelDisplayControls.Size = new System.Drawing.Size(190, 688);
            this.panelDisplayControls.TabIndex = 0;
            // 
            // panelSonogramControls
            // 
            this.panelSonogramControls.Controls.Add(this.checkBoxSonogramAnnotate);
            this.panelSonogramControls.Controls.Add(this.checkBoxSonnogramNoiseReduce);
            this.panelSonogramControls.Controls.Add(this.labelSonogramFileName);
            this.panelSonogramControls.Controls.Add(this.labelSonogramName);
            this.panelSonogramControls.Controls.Add(this.buttonRefreshSonogram);
            this.panelSonogramControls.Controls.Add(this.buttonAudacityRun);
            this.panelSonogramControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSonogramControls.Location = new System.Drawing.Point(0, 364);
            this.panelSonogramControls.Name = "panelSonogramControls";
            this.panelSonogramControls.Size = new System.Drawing.Size(188, 322);
            this.panelSonogramControls.TabIndex = 7;
            // 
            // checkBoxSonogramAnnotate
            // 
            this.checkBoxSonogramAnnotate.AutoSize = true;
            this.checkBoxSonogramAnnotate.Location = new System.Drawing.Point(28, 89);
            this.checkBoxSonogramAnnotate.Name = "checkBoxSonogramAnnotate";
            this.checkBoxSonogramAnnotate.Size = new System.Drawing.Size(118, 17);
            this.checkBoxSonogramAnnotate.TabIndex = 5;
            this.checkBoxSonogramAnnotate.Text = "Annotate sonogram";
            this.checkBoxSonogramAnnotate.UseVisualStyleBackColor = true;
            // 
            // checkBoxSonnogramNoiseReduce
            // 
            this.checkBoxSonnogramNoiseReduce.AutoSize = true;
            this.checkBoxSonnogramNoiseReduce.Location = new System.Drawing.Point(28, 65);
            this.checkBoxSonnogramNoiseReduce.Name = "checkBoxSonnogramNoiseReduce";
            this.checkBoxSonnogramNoiseReduce.Size = new System.Drawing.Size(138, 17);
            this.checkBoxSonnogramNoiseReduce.TabIndex = 4;
            this.checkBoxSonnogramNoiseReduce.Text = "Noise reduce sonogram";
            this.checkBoxSonnogramNoiseReduce.UseVisualStyleBackColor = true;
            // 
            // labelSonogramFileName
            // 
            this.labelSonogramFileName.AutoSize = true;
            this.labelSonogramFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSonogramFileName.Location = new System.Drawing.Point(10, 26);
            this.labelSonogramFileName.Name = "labelSonogramFileName";
            this.labelSonogramFileName.Size = new System.Drawing.Size(144, 16);
            this.labelSonogramFileName.TabIndex = 3;
            this.labelSonogramFileName.Text = "sonogram file name";
            // 
            // labelSonogramName
            // 
            this.labelSonogramName.AutoSize = true;
            this.labelSonogramName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSonogramName.Location = new System.Drawing.Point(10, 10);
            this.labelSonogramName.Name = "labelSonogramName";
            this.labelSonogramName.Size = new System.Drawing.Size(102, 15);
            this.labelSonogramName.TabIndex = 2;
            this.labelSonogramName.Text = "Sonogram Name";
            // 
            // buttonRefreshSonogram
            // 
            this.buttonRefreshSonogram.Location = new System.Drawing.Point(28, 123);
            this.buttonRefreshSonogram.Name = "buttonRefreshSonogram";
            this.buttonRefreshSonogram.Size = new System.Drawing.Size(120, 28);
            this.buttonRefreshSonogram.TabIndex = 1;
            this.buttonRefreshSonogram.Text = "Refresh Sonogram";
            this.buttonRefreshSonogram.UseVisualStyleBackColor = true;
            this.buttonRefreshSonogram.Click += new System.EventHandler(this.buttonRefreshSonogram_Click);
            // 
            // buttonAudacityRun
            // 
            this.buttonAudacityRun.Location = new System.Drawing.Point(28, 174);
            this.buttonAudacityRun.Name = "buttonAudacityRun";
            this.buttonAudacityRun.Size = new System.Drawing.Size(120, 28);
            this.buttonAudacityRun.TabIndex = 0;
            this.buttonAudacityRun.Text = "Run Audacity";
            this.buttonAudacityRun.UseVisualStyleBackColor = true;
            this.buttonAudacityRun.Click += new System.EventHandler(this.buttonRunAudacity_Click);
            // 
            // labelCursorValue
            // 
            this.labelCursorValue.AutoSize = true;
            this.labelCursorValue.Location = new System.Drawing.Point(7, 117);
            this.labelCursorValue.Name = "labelCursorValue";
            this.labelCursorValue.Size = new System.Drawing.Size(67, 13);
            this.labelCursorValue.TabIndex = 6;
            this.labelCursorValue.Text = "Cursor Value";
            // 
            // textBoxCursorValue
            // 
            this.textBoxCursorValue.Location = new System.Drawing.Point(13, 133);
            this.textBoxCursorValue.Name = "textBoxCursorValue";
            this.textBoxCursorValue.Size = new System.Drawing.Size(160, 20);
            this.textBoxCursorValue.TabIndex = 5;
            // 
            // labelSourceFileDurationInMinutes
            // 
            this.labelSourceFileDurationInMinutes.AutoSize = true;
            this.labelSourceFileDurationInMinutes.Location = new System.Drawing.Point(10, 41);
            this.labelSourceFileDurationInMinutes.Name = "labelSourceFileDurationInMinutes";
            this.labelSourceFileDurationInMinutes.Size = new System.Drawing.Size(111, 13);
            this.labelSourceFileDurationInMinutes.TabIndex = 4;
            this.labelSourceFileDurationInMinutes.Text = "File Duration (minutes)";
            // 
            // labelSourceFileName
            // 
            this.labelSourceFileName.AutoSize = true;
            this.labelSourceFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSourceFileName.Location = new System.Drawing.Point(10, 13);
            this.labelSourceFileName.Name = "labelSourceFileName";
            this.labelSourceFileName.Size = new System.Drawing.Size(136, 17);
            this.labelSourceFileName.TabIndex = 3;
            this.labelSourceFileName.Text = "Source File Name";
            // 
            // textBoxCursorLocation
            // 
            this.textBoxCursorLocation.Location = new System.Drawing.Point(13, 90);
            this.textBoxCursorLocation.Name = "textBoxCursorLocation";
            this.textBoxCursorLocation.Size = new System.Drawing.Size(160, 20);
            this.textBoxCursorLocation.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Cursor Location";
            // 
            // panelDisplayVisual
            // 
            this.panelDisplayVisual.Controls.Add(this.panelSonogram);
            this.panelDisplayVisual.Controls.Add(this.pictureBoxBarTrack);
            this.panelDisplayVisual.Controls.Add(this.pictureBoxVisualIndex);
            this.panelDisplayVisual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDisplayVisual.Location = new System.Drawing.Point(0, 0);
            this.panelDisplayVisual.Name = "panelDisplayVisual";
            this.panelDisplayVisual.Size = new System.Drawing.Size(1586, 688);
            this.panelDisplayVisual.TabIndex = 3;
            this.panelDisplayVisual.Paint += new System.Windows.Forms.PaintEventHandler(this.panelDisplayVisual_Paint);
            // 
            // panelSonogram
            // 
            this.panelSonogram.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSonogram.Controls.Add(this.hScrollBarSonogram);
            this.panelSonogram.Controls.Add(this.pictureBoxSonogram);
            this.panelSonogram.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSonogram.Location = new System.Drawing.Point(0, 365);
            this.panelSonogram.Name = "panelSonogram";
            this.panelSonogram.Size = new System.Drawing.Size(1586, 323);
            this.panelSonogram.TabIndex = 4;
            // 
            // hScrollBarSonogram
            // 
            this.hScrollBarSonogram.LargeChange = 100;
            this.hScrollBarSonogram.Location = new System.Drawing.Point(0, 270);
            this.hScrollBarSonogram.Name = "hScrollBarSonogram";
            this.hScrollBarSonogram.Size = new System.Drawing.Size(1586, 20);
            this.hScrollBarSonogram.TabIndex = 3;
            this.hScrollBarSonogram.ValueChanged += new System.EventHandler(this.hScrollBarSonogram_ValueChanged);
            // 
            // pictureBoxSonogram
            // 
            this.pictureBoxSonogram.BackColor = System.Drawing.Color.DarkGray;
            this.pictureBoxSonogram.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxSonogram.Name = "pictureBoxSonogram";
            this.pictureBoxSonogram.Size = new System.Drawing.Size(1584, 270);
            this.pictureBoxSonogram.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxSonogram.TabIndex = 2;
            this.pictureBoxSonogram.TabStop = false;
            // 
            // pictureBoxBarTrack
            // 
            this.pictureBoxBarTrack.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.pictureBoxBarTrack.Location = new System.Drawing.Point(0, 341);
            this.pictureBoxBarTrack.Name = "pictureBoxBarTrack";
            this.pictureBoxBarTrack.Size = new System.Drawing.Size(1583, 24);
            this.pictureBoxBarTrack.TabIndex = 3;
            this.pictureBoxBarTrack.TabStop = false;
            // 
            // pictureBoxVisualIndex
            // 
            this.pictureBoxVisualIndex.BackColor = System.Drawing.Color.Black;
            this.pictureBoxVisualIndex.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBoxVisualIndex.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxVisualIndex.Name = "pictureBoxVisualIndex";
            this.pictureBoxVisualIndex.Size = new System.Drawing.Size(1586, 340);
            this.pictureBoxVisualIndex.TabIndex = 0;
            this.pictureBoxVisualIndex.TabStop = false;
            this.pictureBoxVisualIndex.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxVisualIndex_MouseClick);
            this.pictureBoxVisualIndex.MouseHover += new System.EventHandler(this.pictureBoxVisualIndex_MouseHover);
            this.pictureBoxVisualIndex.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxVisualIndex_MouseMove);
            // 
            // tabPageConsole
            // 
            this.tabPageConsole.Controls.Add(this.textBoxConsole);
            this.tabPageConsole.Location = new System.Drawing.Point(4, 22);
            this.tabPageConsole.Name = "tabPageConsole";
            this.tabPageConsole.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConsole.Size = new System.Drawing.Size(1786, 694);
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
            this.textBoxConsole.Size = new System.Drawing.Size(1780, 688);
            this.textBoxConsole.TabIndex = 0;
            // 
            // btnUpdateSourceFileList
            // 
            this.btnUpdateSourceFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdateSourceFileList.CausesValidation = false;
            this.btnUpdateSourceFileList.Location = new System.Drawing.Point(1625, 17);
            this.btnUpdateSourceFileList.Name = "btnUpdateSourceFileList";
            this.btnUpdateSourceFileList.Size = new System.Drawing.Size(129, 23);
            this.btnUpdateSourceFileList.TabIndex = 10;
            this.btnUpdateSourceFileList.Text = "Update Source File List";
            this.btnUpdateSourceFileList.UseVisualStyleBackColor = true;
            this.btnUpdateSourceFileList.Click += new System.EventHandler(this.btnUpdateSourceFiles_Click);
            // 
            // btnSelectSourceDirectory
            // 
            this.btnSelectSourceDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectSourceDirectory.CausesValidation = false;
            this.btnSelectSourceDirectory.Location = new System.Drawing.Point(1500, 17);
            this.btnSelectSourceDirectory.Name = "btnSelectSourceDirectory";
            this.btnSelectSourceDirectory.Size = new System.Drawing.Size(125, 23);
            this.btnSelectSourceDirectory.TabIndex = 9;
            this.btnSelectSourceDirectory.Text = "Select Source Folder...";
            this.btnSelectSourceDirectory.UseVisualStyleBackColor = true;
            this.btnSelectSourceDirectory.Click += new System.EventHandler(this.btnSelectSourceDirectory_Click);
            // 
            // tfSourceDirectory
            // 
            this.tfSourceDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tfSourceDirectory.Location = new System.Drawing.Point(86, 19);
            this.tfSourceDirectory.Name = "tfSourceDirectory";
            this.tfSourceDirectory.Size = new System.Drawing.Size(1408, 20);
            this.tfSourceDirectory.TabIndex = 8;
            // 
            // folderBrowserDialogChooseDir
            // 
            this.folderBrowserDialogChooseDir.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialogChooseDir.ShowNewFolderButton = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnUpdateOutputFileList);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tfOutputDirectory);
            this.groupBox1.Controls.Add(this.btnSelectOutputDirectory);
            this.groupBox1.Controls.Add(this.tfSourceDirectory);
            this.groupBox1.Controls.Add(this.btnUpdateSourceFileList);
            this.groupBox1.Controls.Add(this.btnSelectSourceDirectory);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1760, 77);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Step 1: Select Source and/or CSV Folders";
            // 
            // btnUpdateOutputFileList
            // 
            this.btnUpdateOutputFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdateOutputFileList.CausesValidation = false;
            this.btnUpdateOutputFileList.Location = new System.Drawing.Point(1625, 43);
            this.btnUpdateOutputFileList.Name = "btnUpdateOutputFileList";
            this.btnUpdateOutputFileList.Size = new System.Drawing.Size(129, 23);
            this.btnUpdateOutputFileList.TabIndex = 15;
            this.btnUpdateOutputFileList.Text = "Update CSV File List";
            this.btnUpdateOutputFileList.UseVisualStyleBackColor = true;
            this.btnUpdateOutputFileList.Click += new System.EventHandler(this.btnUpdateCSVFileList_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Output Folder:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Source Folder:";
            // 
            // tfOutputDirectory
            // 
            this.tfOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tfOutputDirectory.Location = new System.Drawing.Point(86, 45);
            this.tfOutputDirectory.Name = "tfOutputDirectory";
            this.tfOutputDirectory.Size = new System.Drawing.Size(1408, 20);
            this.tfOutputDirectory.TabIndex = 11;
            // 
            // btnSelectOutputDirectory
            // 
            this.btnSelectOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectOutputDirectory.CausesValidation = false;
            this.btnSelectOutputDirectory.Location = new System.Drawing.Point(1500, 43);
            this.btnSelectOutputDirectory.Name = "btnSelectOutputDirectory";
            this.btnSelectOutputDirectory.Size = new System.Drawing.Size(126, 23);
            this.btnSelectOutputDirectory.TabIndex = 12;
            this.btnSelectOutputDirectory.Text = "Select CSV Folder...";
            this.btnSelectOutputDirectory.UseVisualStyleBackColor = true;
            this.btnSelectOutputDirectory.Click += new System.EventHandler(this.btnSelectOutputDirectory_Click);
            // 
            // mediaFileItemBindingSource
            // 
            this.mediaFileItemBindingSource.DataSource = typeof(AudioBrowser.MediaFileItem);
            // 
            // dataGridViewTextBoxColumnFileName
            // 
            this.dataGridViewTextBoxColumnFileName.DataPropertyName = "FileName";
            this.dataGridViewTextBoxColumnFileName.HeaderText = "FileName";
            this.dataGridViewTextBoxColumnFileName.Name = "dataGridViewTextBoxColumnFileName";
            this.dataGridViewTextBoxColumnFileName.ReadOnly = true;
            this.dataGridViewTextBoxColumnFileName.Width = 76;
            // 
            // dataGridViewTextBoxColumnFileLength
            // 
            this.dataGridViewTextBoxColumnFileLength.DataPropertyName = "FileLength";
            this.dataGridViewTextBoxColumnFileLength.HeaderText = "FileLength";
            this.dataGridViewTextBoxColumnFileLength.Name = "dataGridViewTextBoxColumnFileLength";
            this.dataGridViewTextBoxColumnFileLength.ReadOnly = true;
            this.dataGridViewTextBoxColumnFileLength.Width = 81;
            // 
            // csvFileItemBindingSource
            // 
            this.csvFileItemBindingSource.DataSource = typeof(AudioBrowser.CsvFileItem);
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
            this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn.HeaderText = "FileName";
            this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileNameDataGridViewTextBoxColumn.Width = 76;
            // 
            // durationDataGridViewTextBoxColumn
            // 
            this.durationDataGridViewTextBoxColumn.DataPropertyName = "Duration";
            this.durationDataGridViewTextBoxColumn.HeaderText = "Duration (h:m:s)";
            this.durationDataGridViewTextBoxColumn.Name = "durationDataGridViewTextBoxColumn";
            this.durationDataGridViewTextBoxColumn.ReadOnly = true;
            this.durationDataGridViewTextBoxColumn.Width = 97;
            // 
            // mediaTypeDataGridViewTextBoxColumn
            // 
            this.mediaTypeDataGridViewTextBoxColumn.DataPropertyName = "MediaType";
            this.mediaTypeDataGridViewTextBoxColumn.HeaderText = "MediaType";
            this.mediaTypeDataGridViewTextBoxColumn.Name = "mediaTypeDataGridViewTextBoxColumn";
            this.mediaTypeDataGridViewTextBoxColumn.ReadOnly = true;
            this.mediaTypeDataGridViewTextBoxColumn.Width = 85;
            // 
            // fileLengthDataGridViewTextBoxColumn
            // 
            this.fileLengthDataGridViewTextBoxColumn.DataPropertyName = "FileLength";
            this.fileLengthDataGridViewTextBoxColumn.HeaderText = "FileLength";
            this.fileLengthDataGridViewTextBoxColumn.Name = "fileLengthDataGridViewTextBoxColumn";
            this.fileLengthDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileLengthDataGridViewTextBoxColumn.Width = 81;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1784, 812);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tabControlMain);
            this.MaximumSize = new System.Drawing.Size(1800, 850);
            this.Name = "MainForm";
            this.Text = "Acoustic Environment Browser";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageSourceFiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileList)).EndInit();
            this.tabPageOutputFiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCSVfiles)).EndInit();
            this.tabPageDisplay.ResumeLayout(false);
            this.splitContainerDisplay.Panel1.ResumeLayout(false);
            this.splitContainerDisplay.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDisplay)).EndInit();
            this.splitContainerDisplay.ResumeLayout(false);
            this.panelDisplayControls.ResumeLayout(false);
            this.panelDisplayControls.PerformLayout();
            this.panelSonogramControls.ResumeLayout(false);
            this.panelSonogramControls.PerformLayout();
            this.panelDisplayVisual.ResumeLayout(false);
            this.panelSonogram.ResumeLayout(false);
            this.panelSonogram.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSonogram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBarTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualIndex)).EndInit();
            this.tabPageConsole.ResumeLayout(false);
            this.tabPageConsole.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mediaFileItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.csvFileItemBindingSource)).EndInit();
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
        private GroupBox groupBox1;
        private TextBox tfOutputDirectory;
        private Button btnSelectOutputDirectory;
        private Label label2;
        private Label label1;
        private System.ComponentModel.BackgroundWorker backgroundWorkerUpdateSourceFileList;
        private Button btnExtractIndiciesAllSelected;
        private SplitContainer splitContainerDisplay;
        private PictureBox pictureBoxVisualIndex;
        private PictureBox pictureBoxSonogram;
        private Panel panelDisplayControls;
        private Panel panelDisplayVisual;
        private TabPage tabPageOutputFiles;
        private Button btnUpdateOutputFileList;
        private Button btnLoadVisualIndexAllSelected;
        private DataGridView dataGridCSVfiles;
        private System.ComponentModel.BackgroundWorker backgroundWorkerUpdateCSVFileList;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumnSelected;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumnFileName;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumnFileLength;
        private BindingSource csvFileItemBindingSource;
        private Label labelSourceFileDurationInMinutes;
        private Label labelSourceFileName;
        private TextBox textBoxCursorLocation;
        private Label label3;
        private Button buttonAudacityRun;
        private Label labelCursorValue;
        private TextBox textBoxCursorValue;
        private PictureBox pictureBoxBarTrack;
        private Panel panelSonogramControls;
        private Button buttonRefreshSonogram;
        private CheckBox checkBoxSonogramAnnotate;
        private CheckBox checkBoxSonnogramNoiseReduce;
        private Label labelSonogramFileName;
        private Label labelSonogramName;
        public TabControl tabControlMain;
        private Panel panelSonogram;
        private HScrollBar hScrollBarSonogram;
        private DataGridViewCheckBoxColumn selectedDataGridViewCheckBoxColumn;
        private DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn durationDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn mediaTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn fileLengthDataGridViewTextBoxColumn;


    }
}

