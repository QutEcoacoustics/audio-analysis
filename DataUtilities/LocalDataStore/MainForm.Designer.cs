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
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panelSettings = new System.Windows.Forms.Panel();
			this.lblNoOfReadings = new System.Windows.Forms.Label();
			this.cmdStart = new System.Windows.Forms.Button();
			this.lblDataFolder = new System.Windows.Forms.Label();
			this.txtDataFolder = new System.Windows.Forms.TextBox();
			this.cmdChooseDataFolder = new System.Windows.Forms.Button();
			this.statusStrip1.SuspendLayout();
			this.panelSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstDeployments
			// 
			this.lstDeployments.CheckOnClick = true;
			this.lstDeployments.Dock = System.Windows.Forms.DockStyle.Left;
			this.lstDeployments.FormattingEnabled = true;
			this.lstDeployments.Location = new System.Drawing.Point(0, 0);
			this.lstDeployments.Name = "lstDeployments";
			this.lstDeployments.Size = new System.Drawing.Size(210, 229);
			this.lstDeployments.TabIndex = 0;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.progressBar});
			this.statusStrip1.Location = new System.Drawing.Point(0, 229);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(483, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// lblStatus
			// 
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(366, 17);
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
			this.txtDownloadCount.Size = new System.Drawing.Size(63, 20);
			this.txtDownloadCount.TabIndex = 4;
			this.txtDownloadCount.Text = "1000";
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(210, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 229);
			this.splitter1.TabIndex = 5;
			this.splitter1.TabStop = false;
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
			this.panelSettings.Location = new System.Drawing.Point(213, 0);
			this.panelSettings.Name = "panelSettings";
			this.panelSettings.Size = new System.Drawing.Size(270, 229);
			this.panelSettings.TabIndex = 6;
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
			// cmdStart
			// 
			this.cmdStart.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdStart.Location = new System.Drawing.Point(98, 203);
			this.cmdStart.Name = "cmdStart";
			this.cmdStart.Size = new System.Drawing.Size(75, 23);
			this.cmdStart.TabIndex = 6;
			this.cmdStart.Text = "Go!";
			this.cmdStart.UseVisualStyleBackColor = true;
			this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
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
			// txtDataFolder
			// 
			this.txtDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDataFolder.Location = new System.Drawing.Point(77, 29);
			this.txtDataFolder.Name = "txtDataFolder";
			this.txtDataFolder.Size = new System.Drawing.Size(160, 20);
			this.txtDataFolder.TabIndex = 8;
			this.txtDataFolder.TextChanged += new System.EventHandler(this.txtDataFolder_TextChanged);
			// 
			// cmdChooseDataFolder
			// 
			this.cmdChooseDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdChooseDataFolder.Location = new System.Drawing.Point(243, 27);
			this.cmdChooseDataFolder.Name = "cmdChooseDataFolder";
			this.cmdChooseDataFolder.Size = new System.Drawing.Size(24, 23);
			this.cmdChooseDataFolder.TabIndex = 9;
			this.cmdChooseDataFolder.Text = "...";
			this.cmdChooseDataFolder.UseVisualStyleBackColor = true;
			this.cmdChooseDataFolder.Click += new System.EventHandler(this.cmdChooseDataFolder_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(483, 251);
			this.Controls.Add(this.panelSettings);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.lstDeployments);
			this.Controls.Add(this.statusStrip1);
			this.Name = "MainForm";
			this.Text = "QUT Sensors Local Data Manager Ultimate Edition";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.panelSettings.ResumeLayout(false);
			this.panelSettings.PerformLayout();
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
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel panelSettings;
		private System.Windows.Forms.Button cmdStart;
		private System.Windows.Forms.Label lblNoOfReadings;
		private System.Windows.Forms.Button cmdChooseDataFolder;
		private System.Windows.Forms.TextBox txtDataFolder;
		private System.Windows.Forms.Label lblDataFolder;
	}
}

