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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageSourceFiles = new System.Windows.Forms.TabPage();
            this.btnExtractIndiciesAllSelected = new System.Windows.Forms.Button();
            this.dataGridViewFileList = new System.Windows.Forms.DataGridView();
            this.selectedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.durationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mediaTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileLengthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mediaFileItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabPageOutputFiles = new System.Windows.Forms.TabPage();
            this.btnLoadVisualIndexAllSelected = new System.Windows.Forms.Button();
            this.dataGridCSVfiles = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumnSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumnFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumnFileLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.csvFileItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabPageDisplay = new System.Windows.Forms.TabPage();
            this.splitContainerDisplay = new System.Windows.Forms.SplitContainer();
            this.panelDisplayControls = new System.Windows.Forms.Panel();
            this.labelCursorValue = new System.Windows.Forms.Label();
            this.textBoxCursorValue = new System.Windows.Forms.TextBox();
            this.labelFileDurationInMinutes = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxCursorLocation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panelDisplayVisual = new System.Windows.Forms.Panel();
            this.pictureBoxSonogram = new System.Windows.Forms.PictureBox();
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
            this.pictureBoxBarTrack = new System.Windows.Forms.PictureBox();
            this.tabControlMain.SuspendLayout();
            this.tabPageSourceFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mediaFileItemBindingSource)).BeginInit();
            this.tabPageOutputFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCSVfiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.csvFileItemBindingSource)).BeginInit();
            this.tabPageDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDisplay)).BeginInit();
            this.splitContainerDisplay.Panel1.SuspendLayout();
            this.splitContainerDisplay.Panel2.SuspendLayout();
            this.splitContainerDisplay.SuspendLayout();
            this.panelDisplayControls.SuspendLayout();
            this.panelDisplayVisual.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSonogram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualIndex)).BeginInit();
            this.tabPageConsole.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBarTrack)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageSourceFiles);
            this.tabControlMain.Controls.Add(this.tabPageOutputFiles);
            this.tabControlMain.Controls.Add(this.tabPageDisplay);
            this.tabControlMain.Controls.Add(this.tabPageConsole);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControlMain.Location = new System.Drawing.Point(0, 107);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1794, 605);
            this.tabControlMain.TabIndex = 0;
            // 
            // tabPageSourceFiles
            // 
            this.tabPageSourceFiles.Controls.Add(this.btnExtractIndiciesAllSelected);
            this.tabPageSourceFiles.Controls.Add(this.dataGridViewFileList);
            this.tabPageSourceFiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageSourceFiles.Name = "tabPageSourceFiles";
            this.tabPageSourceFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSourceFiles.Size = new System.Drawing.Size(1786, 579);
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
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewFileList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewFileList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFileList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.selectedDataGridViewCheckBoxColumn,
            this.fileNameDataGridViewTextBoxColumn,
            this.durationDataGridViewTextBoxColumn,
            this.mediaTypeDataGridViewTextBoxColumn,
            this.fileLengthDataGridViewTextBoxColumn});
            this.dataGridViewFileList.DataSource = this.mediaFileItemBindingSource;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewFileList.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewFileList.Location = new System.Drawing.Point(3, 35);
            this.dataGridViewFileList.Name = "dataGridViewFileList";
            this.dataGridViewFileList.Size = new System.Drawing.Size(1453, 548);
            this.dataGridViewFileList.TabIndex = 0;
            this.dataGridViewFileList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellClick);
            this.dataGridViewFileList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewFileListSourceFileList_CellContentClick);
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
            this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn.HeaderText = "FileName";
            this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileNameDataGridViewTextBoxColumn.Width = 76;
            // 
            // durationDataGridViewTextBoxColumn
            // 
            this.durationDataGridViewTextBoxColumn.DataPropertyName = "Duration";
            this.durationDataGridViewTextBoxColumn.HeaderText = "Duration";
            this.durationDataGridViewTextBoxColumn.Name = "durationDataGridViewTextBoxColumn";
            this.durationDataGridViewTextBoxColumn.ReadOnly = true;
            this.durationDataGridViewTextBoxColumn.Width = 72;
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
            // mediaFileItemBindingSource
            // 
            this.mediaFileItemBindingSource.DataSource = typeof(AudioBrowser.MediaFileItem);
            // 
            // tabPageOutputFiles
            // 
            this.tabPageOutputFiles.Controls.Add(this.btnLoadVisualIndexAllSelected);
            this.tabPageOutputFiles.Controls.Add(this.dataGridCSVfiles);
            this.tabPageOutputFiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageOutputFiles.Name = "tabPageOutputFiles";
            this.tabPageOutputFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOutputFiles.Size = new System.Drawing.Size(1786, 579);
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
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridCSVfiles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridCSVfiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCSVfiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumnSelected,
            this.dataGridViewTextBoxColumnFileName,
            this.dataGridViewTextBoxColumnFileLength});
            this.dataGridCSVfiles.DataSource = this.csvFileItemBindingSource;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridCSVfiles.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridCSVfiles.Location = new System.Drawing.Point(4, 35);
            this.dataGridCSVfiles.Name = "dataGridCSVfiles";
            this.dataGridCSVfiles.Size = new System.Drawing.Size(1474, 544);
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
            // tabPageDisplay
            // 
            this.tabPageDisplay.Controls.Add(this.splitContainerDisplay);
            this.tabPageDisplay.Location = new System.Drawing.Point(4, 22);
            this.tabPageDisplay.Name = "tabPageDisplay";
            this.tabPageDisplay.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDisplay.Size = new System.Drawing.Size(1786, 579);
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
            this.splitContainerDisplay.Size = new System.Drawing.Size(1780, 573);
            this.splitContainerDisplay.SplitterDistance = 190;
            this.splitContainerDisplay.TabIndex = 0;
            // 
            // panelDisplayControls
            // 
            this.panelDisplayControls.BackColor = System.Drawing.Color.Transparent;
            this.panelDisplayControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDisplayControls.Controls.Add(this.labelCursorValue);
            this.panelDisplayControls.Controls.Add(this.textBoxCursorValue);
            this.panelDisplayControls.Controls.Add(this.labelFileDurationInMinutes);
            this.panelDisplayControls.Controls.Add(this.label4);
            this.panelDisplayControls.Controls.Add(this.textBoxCursorLocation);
            this.panelDisplayControls.Controls.Add(this.label3);
            this.panelDisplayControls.Controls.Add(this.button1);
            this.panelDisplayControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDisplayControls.Location = new System.Drawing.Point(0, 0);
            this.panelDisplayControls.Name = "panelDisplayControls";
            this.panelDisplayControls.Size = new System.Drawing.Size(190, 573);
            this.panelDisplayControls.TabIndex = 0;
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
            // labelFileDurationInMinutes
            // 
            this.labelFileDurationInMinutes.AutoSize = true;
            this.labelFileDurationInMinutes.Location = new System.Drawing.Point(10, 41);
            this.labelFileDurationInMinutes.Name = "labelFileDurationInMinutes";
            this.labelFileDurationInMinutes.Size = new System.Drawing.Size(111, 13);
            this.labelFileDurationInMinutes.TabIndex = 4;
            this.labelFileDurationInMinutes.Text = "File Duration (minutes)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "File Name";
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
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(30, 422);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 26);
            this.button1.TabIndex = 0;
            this.button1.Text = "Run Audacity";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // panelDisplayVisual
            // 
            this.panelDisplayVisual.Controls.Add(this.pictureBoxBarTrack);
            this.panelDisplayVisual.Controls.Add(this.pictureBoxSonogram);
            this.panelDisplayVisual.Controls.Add(this.pictureBoxVisualIndex);
            this.panelDisplayVisual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDisplayVisual.Location = new System.Drawing.Point(0, 0);
            this.panelDisplayVisual.Name = "panelDisplayVisual";
            this.panelDisplayVisual.Size = new System.Drawing.Size(1586, 573);
            this.panelDisplayVisual.TabIndex = 3;
            this.panelDisplayVisual.Paint += new System.Windows.Forms.PaintEventHandler(this.panelDisplayVisual_Paint);
            // 
            // pictureBoxSonogram
            // 
            this.pictureBoxSonogram.BackColor = System.Drawing.Color.DarkGray;
            this.pictureBoxSonogram.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pictureBoxSonogram.Location = new System.Drawing.Point(0, 365);
            this.pictureBoxSonogram.Name = "pictureBoxSonogram";
            this.pictureBoxSonogram.Size = new System.Drawing.Size(1586, 208);
            this.pictureBoxSonogram.TabIndex = 2;
            this.pictureBoxSonogram.TabStop = false;
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
            this.tabPageConsole.Size = new System.Drawing.Size(1786, 579);
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
            this.textBoxConsole.Size = new System.Drawing.Size(1780, 573);
            this.textBoxConsole.TabIndex = 0;
            // 
            // btnUpdateSourceFileList
            // 
            this.btnUpdateSourceFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdateSourceFileList.CausesValidation = false;
            this.btnUpdateSourceFileList.Location = new System.Drawing.Point(1635, 17);
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
            this.btnSelectSourceDirectory.Location = new System.Drawing.Point(1510, 17);
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
            this.tfSourceDirectory.Size = new System.Drawing.Size(1418, 20);
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
            this.groupBox1.Size = new System.Drawing.Size(1770, 77);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Step 1: Select Source and/or CSV Folders";
            // 
            // btnUpdateOutputFileList
            // 
            this.btnUpdateOutputFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdateOutputFileList.CausesValidation = false;
            this.btnUpdateOutputFileList.Location = new System.Drawing.Point(1635, 43);
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
            this.tfOutputDirectory.Size = new System.Drawing.Size(1418, 20);
            this.tfOutputDirectory.TabIndex = 11;
            // 
            // btnSelectOutputDirectory
            // 
            this.btnSelectOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectOutputDirectory.CausesValidation = false;
            this.btnSelectOutputDirectory.Location = new System.Drawing.Point(1510, 43);
            this.btnSelectOutputDirectory.Name = "btnSelectOutputDirectory";
            this.btnSelectOutputDirectory.Size = new System.Drawing.Size(126, 23);
            this.btnSelectOutputDirectory.TabIndex = 12;
            this.btnSelectOutputDirectory.Text = "Select CSV Folder...";
            this.btnSelectOutputDirectory.UseVisualStyleBackColor = true;
            this.btnSelectOutputDirectory.Click += new System.EventHandler(this.btnSelectOutputDirectory_Click);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1794, 712);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tabControlMain);
            this.Name = "MainForm";
            this.Text = "Acoustic Environment Browser";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageSourceFiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFileList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mediaFileItemBindingSource)).EndInit();
            this.tabPageOutputFiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCSVfiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.csvFileItemBindingSource)).EndInit();
            this.tabPageDisplay.ResumeLayout(false);
            this.splitContainerDisplay.Panel1.ResumeLayout(false);
            this.splitContainerDisplay.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDisplay)).EndInit();
            this.splitContainerDisplay.ResumeLayout(false);
            this.panelDisplayControls.ResumeLayout(false);
            this.panelDisplayControls.PerformLayout();
            this.panelDisplayVisual.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSonogram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualIndex)).EndInit();
            this.tabPageConsole.ResumeLayout(false);
            this.tabPageConsole.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBarTrack)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TabControl tabControlMain;
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
        private DataGridViewCheckBoxColumn selectedDataGridViewCheckBoxColumn;
        private DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn durationDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn mediaTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn fileLengthDataGridViewTextBoxColumn;
        private Label labelFileDurationInMinutes;
        private Label label4;
        private TextBox textBoxCursorLocation;
        private Label label3;
        private Button button1;
        private Label labelCursorValue;
        private TextBox textBoxCursorValue;
        private PictureBox pictureBoxBarTrack;


    }
}

