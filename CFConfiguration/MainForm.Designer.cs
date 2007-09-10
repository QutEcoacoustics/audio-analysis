namespace CFConfiguration
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
			this.lblDuration = new System.Windows.Forms.Label();
			this.tabs = new System.Windows.Forms.TabControl();
			this.tabRecording = new System.Windows.Forms.TabPage();
			this.tabIdentity = new System.Windows.Forms.TabPage();
			this.lblFrequency = new System.Windows.Forms.Label();
			this.txtFrequency = new System.Windows.Forms.TextBox();
			this.txtDuration = new System.Windows.Forms.TextBox();
			this.cmdSaveRecordings = new System.Windows.Forms.Button();
			this.tabInfo = new System.Windows.Forms.TabPage();
			this.lblLastRecording = new System.Windows.Forms.Label();
			this.txtLastRecording = new System.Windows.Forms.TextBox();
			this.cmdRefreshInfo = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.lblLog = new System.Windows.Forms.Label();
			this.txtRecordingPath = new System.Windows.Forms.TextBox();
			this.lblRecordingPath = new System.Windows.Forms.Label();
			this.cmdChooseRecordingPath = new System.Windows.Forms.Button();
			this.tabServer = new System.Windows.Forms.TabPage();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.cmdRecordNow = new System.Windows.Forms.Button();
			this.tabs.SuspendLayout();
			this.tabRecording.SuspendLayout();
			this.tabInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblDuration
			// 
			this.lblDuration.Location = new System.Drawing.Point(7, 37);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(56, 20);
			this.lblDuration.Text = "Duration:";
			// 
			// tabs
			// 
			this.tabs.Controls.Add(this.tabRecording);
			this.tabs.Controls.Add(this.tabServer);
			this.tabs.Controls.Add(this.tabIdentity);
			this.tabs.Controls.Add(this.tabInfo);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.Location = new System.Drawing.Point(0, 0);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(240, 268);
			this.tabs.TabIndex = 1;
			// 
			// tabRecording
			// 
			this.tabRecording.Controls.Add(this.cmdRecordNow);
			this.tabRecording.Controls.Add(this.cmdChooseRecordingPath);
			this.tabRecording.Controls.Add(this.txtRecordingPath);
			this.tabRecording.Controls.Add(this.lblRecordingPath);
			this.tabRecording.Controls.Add(this.cmdSaveRecordings);
			this.tabRecording.Controls.Add(this.txtDuration);
			this.tabRecording.Controls.Add(this.txtFrequency);
			this.tabRecording.Controls.Add(this.lblFrequency);
			this.tabRecording.Controls.Add(this.lblDuration);
			this.tabRecording.Location = new System.Drawing.Point(0, 0);
			this.tabRecording.Name = "tabRecording";
			this.tabRecording.Size = new System.Drawing.Size(240, 245);
			this.tabRecording.Text = "Recordings";
			// 
			// tabIdentity
			// 
			this.tabIdentity.Location = new System.Drawing.Point(0, 0);
			this.tabIdentity.Name = "tabIdentity";
			this.tabIdentity.Size = new System.Drawing.Size(240, 245);
			this.tabIdentity.Text = "Identity";
			// 
			// lblFrequency
			// 
			this.lblFrequency.Location = new System.Drawing.Point(7, 9);
			this.lblFrequency.Name = "lblFrequency";
			this.lblFrequency.Size = new System.Drawing.Size(65, 21);
			this.lblFrequency.Text = "Frequency:";
			// 
			// txtFrequency
			// 
			this.txtFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFrequency.Location = new System.Drawing.Point(78, 7);
			this.txtFrequency.Name = "txtFrequency";
			this.txtFrequency.Size = new System.Drawing.Size(155, 21);
			this.txtFrequency.TabIndex = 3;
			// 
			// txtDuration
			// 
			this.txtDuration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDuration.Location = new System.Drawing.Point(78, 34);
			this.txtDuration.Name = "txtDuration";
			this.txtDuration.Size = new System.Drawing.Size(155, 21);
			this.txtDuration.TabIndex = 4;
			// 
			// cmdSaveRecordings
			// 
			this.cmdSaveRecordings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdSaveRecordings.Location = new System.Drawing.Point(161, 222);
			this.cmdSaveRecordings.Name = "cmdSaveRecordings";
			this.cmdSaveRecordings.Size = new System.Drawing.Size(72, 20);
			this.cmdSaveRecordings.TabIndex = 5;
			this.cmdSaveRecordings.Text = "&Save";
			this.cmdSaveRecordings.Click += new System.EventHandler(this.cmdSaveRecordings_Click);
			// 
			// tabInfo
			// 
			this.tabInfo.Controls.Add(this.lblLog);
			this.tabInfo.Controls.Add(this.txtLog);
			this.tabInfo.Controls.Add(this.cmdRefreshInfo);
			this.tabInfo.Controls.Add(this.txtLastRecording);
			this.tabInfo.Controls.Add(this.lblLastRecording);
			this.tabInfo.Location = new System.Drawing.Point(0, 0);
			this.tabInfo.Name = "tabInfo";
			this.tabInfo.Size = new System.Drawing.Size(240, 245);
			this.tabInfo.Text = "Info";
			// 
			// lblLastRecording
			// 
			this.lblLastRecording.Location = new System.Drawing.Point(7, 10);
			this.lblLastRecording.Name = "lblLastRecording";
			this.lblLastRecording.Size = new System.Drawing.Size(91, 20);
			this.lblLastRecording.Text = "Last Recording:";
			// 
			// txtLastRecording
			// 
			this.txtLastRecording.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtLastRecording.Location = new System.Drawing.Point(104, 7);
			this.txtLastRecording.Name = "txtLastRecording";
			this.txtLastRecording.ReadOnly = true;
			this.txtLastRecording.Size = new System.Drawing.Size(129, 21);
			this.txtLastRecording.TabIndex = 1;
			// 
			// cmdRefreshInfo
			// 
			this.cmdRefreshInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRefreshInfo.Location = new System.Drawing.Point(161, 220);
			this.cmdRefreshInfo.Name = "cmdRefreshInfo";
			this.cmdRefreshInfo.Size = new System.Drawing.Size(72, 20);
			this.cmdRefreshInfo.TabIndex = 2;
			this.cmdRefreshInfo.Text = "&Refresh";
			this.cmdRefreshInfo.Click += new System.EventHandler(this.cmdRefreshInfo_Click);
			// 
			// txtLog
			// 
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.Location = new System.Drawing.Point(7, 54);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(226, 160);
			this.txtLog.TabIndex = 3;
			// 
			// lblLog
			// 
			this.lblLog.Location = new System.Drawing.Point(7, 31);
			this.lblLog.Name = "lblLog";
			this.lblLog.Size = new System.Drawing.Size(29, 20);
			this.lblLog.Text = "Log:";
			// 
			// txtRecordingPath
			// 
			this.txtRecordingPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtRecordingPath.Location = new System.Drawing.Point(78, 61);
			this.txtRecordingPath.Name = "txtRecordingPath";
			this.txtRecordingPath.Size = new System.Drawing.Size(126, 21);
			this.txtRecordingPath.TabIndex = 7;
			// 
			// lblRecordingPath
			// 
			this.lblRecordingPath.Location = new System.Drawing.Point(7, 64);
			this.lblRecordingPath.Name = "lblRecordingPath";
			this.lblRecordingPath.Size = new System.Drawing.Size(56, 20);
			this.lblRecordingPath.Text = "Path:";
			// 
			// cmdChooseRecordingPath
			// 
			this.cmdChooseRecordingPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdChooseRecordingPath.Location = new System.Drawing.Point(210, 61);
			this.cmdChooseRecordingPath.Name = "cmdChooseRecordingPath";
			this.cmdChooseRecordingPath.Size = new System.Drawing.Size(23, 20);
			this.cmdChooseRecordingPath.TabIndex = 9;
			this.cmdChooseRecordingPath.Text = " ...";
			this.cmdChooseRecordingPath.Click += new System.EventHandler(this.cmdChooseRecordingPath_Click);
			// 
			// tabServer
			// 
			this.tabServer.Location = new System.Drawing.Point(0, 0);
			this.tabServer.Name = "tabServer";
			this.tabServer.Size = new System.Drawing.Size(240, 245);
			this.tabServer.Text = "Server";
			// 
			// cmdRecordNow
			// 
			this.cmdRecordNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cmdRecordNow.Location = new System.Drawing.Point(7, 222);
			this.cmdRecordNow.Name = "cmdRecordNow";
			this.cmdRecordNow.Size = new System.Drawing.Size(85, 20);
			this.cmdRecordNow.TabIndex = 10;
			this.cmdRecordNow.Text = "Record Now!";
			this.cmdRecordNow.Click += new System.EventHandler(this.cmdRecordNow_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(240, 268);
			this.Controls.Add(this.tabs);
			this.Menu = this.mainMenu;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.Text = "QUT Sensors Configuration";
			this.tabs.ResumeLayout(false);
			this.tabRecording.ResumeLayout(false);
			this.tabInfo.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblDuration;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage tabRecording;
		private System.Windows.Forms.TextBox txtFrequency;
		private System.Windows.Forms.Label lblFrequency;
		private System.Windows.Forms.TabPage tabIdentity;
		private System.Windows.Forms.TextBox txtDuration;
		private System.Windows.Forms.Button cmdSaveRecordings;
		private System.Windows.Forms.TabPage tabInfo;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.Button cmdRefreshInfo;
		private System.Windows.Forms.TextBox txtLastRecording;
		private System.Windows.Forms.Label lblLastRecording;
		private System.Windows.Forms.Label lblLog;
		private System.Windows.Forms.Button cmdChooseRecordingPath;
		private System.Windows.Forms.TextBox txtRecordingPath;
		private System.Windows.Forms.Label lblRecordingPath;
		private System.Windows.Forms.TabPage tabServer;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.Button cmdRecordNow;
	}
}

