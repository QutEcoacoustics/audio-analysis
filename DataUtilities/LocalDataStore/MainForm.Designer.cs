namespace LocalDataStore
{
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
			this.lstDeployments = new System.Windows.Forms.CheckedListBox();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
			this.txtDownloadCount = new System.Windows.Forms.TextBox();
			this.panelSettings = new System.Windows.Forms.Panel();
			this.cmdChooseDataFolder = new System.Windows.Forms.Button();
			this.txtDataFolder = new System.Windows.Forms.TextBox();
			this.lblDataFolder = new System.Windows.Forms.Label();
			this.cmdStart = new System.Windows.Forms.Button();
			this.lblNoOfReadings = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.txtUploadPath = new System.Windows.Forms.TextBox();
			this.cmdUploadBrowse = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.updChunkSize = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.cmdRestUpload = new System.Windows.Forms.Button();
			this.cmdSoapUpload = new System.Windows.Forms.Button();
			this.statusStrip1.SuspendLayout();
			this.panelSettings.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.updChunkSize)).BeginInit();
			this.SuspendLayout();
			// 
			// lstDeployments
			// 
			this.lstDeployments.CheckOnClick = true;
			this.lstDeployments.Dock = System.Windows.Forms.DockStyle.Left;
			this.lstDeployments.FormattingEnabled = true;
			this.lstDeployments.Location = new System.Drawing.Point(3, 3);
			this.lstDeployments.Name = "lstDeployments";
			this.lstDeployments.Size = new System.Drawing.Size(210, 409);
			this.lstDeployments.TabIndex = 0;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.progressBar});
			this.statusStrip1.Location = new System.Drawing.Point(0, 441);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(744, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// lblStatus
			// 
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(627, 17);
			this.lblStatus.Spring = true;
			// 
			// progressBar
			// 
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(100, 16);
			// 
			// backgroundWorker
			// 
			this.backgroundWorker.WorkerReportsProgress = true;
			this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
			this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
			this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
			// 
			// txtDownloadCount
			// 
			this.txtDownloadCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDownloadCount.Location = new System.Drawing.Point(204, 3);
			this.txtDownloadCount.Name = "txtDownloadCount";
			this.txtDownloadCount.Size = new System.Drawing.Size(313, 20);
			this.txtDownloadCount.TabIndex = 4;
			this.txtDownloadCount.Text = "1000";
			// 
			// panelSettings
			// 
			this.panelSettings.Controls.Add(this.cmdChooseDataFolder);
			this.panelSettings.Controls.Add(this.txtDataFolder);
			this.panelSettings.Controls.Add(this.lblDataFolder);
			this.panelSettings.Controls.Add(this.cmdStart);
			this.panelSettings.Controls.Add(this.lblNoOfReadings);
			this.panelSettings.Controls.Add(this.txtDownloadCount);
			this.panelSettings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSettings.Location = new System.Drawing.Point(213, 3);
			this.panelSettings.Name = "panelSettings";
			this.panelSettings.Size = new System.Drawing.Size(520, 409);
			this.panelSettings.TabIndex = 6;
			// 
			// cmdChooseDataFolder
			// 
			this.cmdChooseDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdChooseDataFolder.Location = new System.Drawing.Point(493, 27);
			this.cmdChooseDataFolder.Name = "cmdChooseDataFolder";
			this.cmdChooseDataFolder.Size = new System.Drawing.Size(24, 23);
			this.cmdChooseDataFolder.TabIndex = 9;
			this.cmdChooseDataFolder.Text = "...";
			this.cmdChooseDataFolder.UseVisualStyleBackColor = true;
			this.cmdChooseDataFolder.Click += new System.EventHandler(this.cmdChooseDataFolder_Click);
			// 
			// txtDataFolder
			// 
			this.txtDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDataFolder.Location = new System.Drawing.Point(77, 29);
			this.txtDataFolder.Name = "txtDataFolder";
			this.txtDataFolder.Size = new System.Drawing.Size(410, 20);
			this.txtDataFolder.TabIndex = 8;
			this.txtDataFolder.TextChanged += new System.EventHandler(this.txtDataFolder_TextChanged);
			// 
			// lblDataFolder
			// 
			this.lblDataFolder.AutoSize = true;
			this.lblDataFolder.Location = new System.Drawing.Point(6, 32);
			this.lblDataFolder.Name = "lblDataFolder";
			this.lblDataFolder.Size = new System.Drawing.Size(65, 13);
			this.lblDataFolder.TabIndex = 7;
			this.lblDataFolder.Text = "Data Folder:";
			// 
			// cmdStart
			// 
			this.cmdStart.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdStart.Location = new System.Drawing.Point(223, 383);
			this.cmdStart.Name = "cmdStart";
			this.cmdStart.Size = new System.Drawing.Size(75, 23);
			this.cmdStart.TabIndex = 6;
			this.cmdStart.Text = "Go!";
			this.cmdStart.UseVisualStyleBackColor = true;
			this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
			// 
			// lblNoOfReadings
			// 
			this.lblNoOfReadings.AutoSize = true;
			this.lblNoOfReadings.Location = new System.Drawing.Point(6, 6);
			this.lblNoOfReadings.Name = "lblNoOfReadings";
			this.lblNoOfReadings.Size = new System.Drawing.Size(192, 13);
			this.lblNoOfReadings.TabIndex = 5;
			this.lblNoOfReadings.Text = "# of readings/deployment to download:";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(744, 441);
			this.tabControl1.TabIndex = 10;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.splitter1);
			this.tabPage1.Controls.Add(this.panelSettings);
			this.tabPage1.Controls.Add(this.lstDeployments);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(736, 415);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Download";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.cmdSoapUpload);
			this.tabPage2.Controls.Add(this.cmdRestUpload);
			this.tabPage2.Controls.Add(this.label2);
			this.tabPage2.Controls.Add(this.updChunkSize);
			this.tabPage2.Controls.Add(this.label1);
			this.tabPage2.Controls.Add(this.cmdUploadBrowse);
			this.tabPage2.Controls.Add(this.txtUploadPath);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(736, 415);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Upload";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(213, 3);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 409);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// txtUploadPath
			// 
			this.txtUploadPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtUploadPath.Location = new System.Drawing.Point(87, 6);
			this.txtUploadPath.Name = "txtUploadPath";
			this.txtUploadPath.Size = new System.Drawing.Size(610, 20);
			this.txtUploadPath.TabIndex = 0;
			this.txtUploadPath.Text = "F:\\Wynton Marsalis & Ellis Marsalis\\Joe Cool\'s Blues\\01 - Linus And Lucy.mp3";
			// 
			// cmdUploadBrowse
			// 
			this.cmdUploadBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdUploadBrowse.Location = new System.Drawing.Point(703, 3);
			this.cmdUploadBrowse.Name = "cmdUploadBrowse";
			this.cmdUploadBrowse.Size = new System.Drawing.Size(25, 23);
			this.cmdUploadBrowse.TabIndex = 1;
			this.cmdUploadBrowse.Text = "...";
			this.cmdUploadBrowse.UseVisualStyleBackColor = true;
			this.cmdUploadBrowse.Click += new System.EventHandler(this.cmdUploadBrowse_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "File to upload:";
			// 
			// updChunkSize
			// 
			this.updChunkSize.Increment = new decimal(new int[] {
            102400,
            0,
            0,
            0});
			this.updChunkSize.Location = new System.Drawing.Point(87, 32);
			this.updChunkSize.Maximum = new decimal(new int[] {
            10240000,
            0,
            0,
            0});
			this.updChunkSize.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.updChunkSize.Name = "updChunkSize";
			this.updChunkSize.Size = new System.Drawing.Size(79, 20);
			this.updChunkSize.TabIndex = 3;
			this.updChunkSize.Value = new decimal(new int[] {
            102400,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 34);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Chunk Size:";
			// 
			// cmdRestUpload
			// 
			this.cmdRestUpload.Location = new System.Drawing.Point(11, 58);
			this.cmdRestUpload.Name = "cmdRestUpload";
			this.cmdRestUpload.Size = new System.Drawing.Size(82, 23);
			this.cmdRestUpload.TabIndex = 5;
			this.cmdRestUpload.Text = "REST Upload";
			this.cmdRestUpload.UseVisualStyleBackColor = true;
			this.cmdRestUpload.Click += new System.EventHandler(this.cmdRestUpload_Click);
			// 
			// cmdSoapUpload
			// 
			this.cmdSoapUpload.Enabled = false;
			this.cmdSoapUpload.Location = new System.Drawing.Point(99, 58);
			this.cmdSoapUpload.Name = "cmdSoapUpload";
			this.cmdSoapUpload.Size = new System.Drawing.Size(82, 23);
			this.cmdSoapUpload.TabIndex = 6;
			this.cmdSoapUpload.Text = "SOAP Upload";
			this.cmdSoapUpload.UseVisualStyleBackColor = true;
			this.cmdSoapUpload.Click += new System.EventHandler(this.cmdSoapUpload_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(744, 463);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.statusStrip1);
			this.Name = "MainForm";
			this.Text = "QUT Sensors Local Data Manager Ultimate Edition";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.panelSettings.ResumeLayout(false);
			this.panelSettings.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.updChunkSize)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckedListBox lstDeployments;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel lblStatus;
		private System.Windows.Forms.ToolStripProgressBar progressBar;
		private System.ComponentModel.BackgroundWorker backgroundWorker;
		private System.Windows.Forms.TextBox txtDownloadCount;
		private System.Windows.Forms.Panel panelSettings;
		private System.Windows.Forms.Button cmdStart;
		private System.Windows.Forms.Label lblNoOfReadings;
		private System.Windows.Forms.Button cmdChooseDataFolder;
		private System.Windows.Forms.TextBox txtDataFolder;
		private System.Windows.Forms.Label lblDataFolder;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button cmdUploadBrowse;
		private System.Windows.Forms.TextBox txtUploadPath;
		private System.Windows.Forms.Button cmdSoapUpload;
		private System.Windows.Forms.Button cmdRestUpload;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown updChunkSize;
	}
}

