namespace CFRecorder
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.MainMenu mnuMain;

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
			this.mnuMain = new System.Windows.Forms.MainMenu();
			this.timer = new System.Windows.Forms.Timer();
			this.cmdRecordNow = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.txtSensorName = new System.Windows.Forms.TextBox();
			this.txtFolder = new System.Windows.Forms.TextBox();
			this.cmdSelectFolder = new System.Windows.Forms.Button();
			this.lblWireless = new System.Windows.Forms.Label();
			this.wirelessTimer = new System.Windows.Forms.Timer();
			this.mnuExit = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// mnuMain
			// 
			this.mnuMain.MenuItems.Add(this.mnuExit);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 1800000;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// cmdRecordNow
			// 
			this.cmdRecordNow.Location = new System.Drawing.Point(3, 119);
			this.cmdRecordNow.Name = "cmdRecordNow";
			this.cmdRecordNow.Size = new System.Drawing.Size(83, 20);
			this.cmdRecordNow.TabIndex = 0;
			this.cmdRecordNow.Text = "Record Now";
			this.cmdRecordNow.Click += new System.EventHandler(this.cmdRecordNow_Click);
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(3, 173);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.Size = new System.Drawing.Size(237, 92);
			this.txtLog.TabIndex = 2;
			// 
			// txtSensorName
			// 
			this.txtSensorName.Location = new System.Drawing.Point(92, 119);
			this.txtSensorName.Name = "txtSensorName";
			this.txtSensorName.Size = new System.Drawing.Size(145, 21);
			this.txtSensorName.TabIndex = 3;
			this.txtSensorName.Text = "QUT00";
			this.txtSensorName.TextChanged += new System.EventHandler(this.txtSensorName_TextChanged);
			// 
			// txtFolder
			// 
			this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFolder.Location = new System.Drawing.Point(3, 146);
			this.txtFolder.Name = "txtFolder";
			this.txtFolder.Size = new System.Drawing.Size(209, 21);
			this.txtFolder.TabIndex = 4;
			this.txtFolder.Text = "\\My Flash Disk";
			// 
			// cmdSelectFolder
			// 
			this.cmdSelectFolder.Location = new System.Drawing.Point(218, 147);
			this.cmdSelectFolder.Name = "cmdSelectFolder";
			this.cmdSelectFolder.Size = new System.Drawing.Size(19, 20);
			this.cmdSelectFolder.TabIndex = 5;
			this.cmdSelectFolder.Text = "...";
			this.cmdSelectFolder.Click += new System.EventHandler(this.cmdSelectFolder_Click);
			// 
			// lblWireless
			// 
			this.lblWireless.Location = new System.Drawing.Point(4, 4);
			this.lblWireless.Name = "lblWireless";
			this.lblWireless.Size = new System.Drawing.Size(236, 20);
			this.lblWireless.Text = "Wireless:";
			// 
			// wirelessTimer
			// 
			this.wirelessTimer.Enabled = true;
			this.wirelessTimer.Interval = 10000;
			this.wirelessTimer.Tick += new System.EventHandler(this.wirelessTimer_Tick);
			// 
			// mnuExit
			// 
			this.mnuExit.Text = "Exit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(240, 268);
			this.Controls.Add(this.lblWireless);
			this.Controls.Add(this.cmdSelectFolder);
			this.Controls.Add(this.txtFolder);
			this.Controls.Add(this.txtSensorName);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.cmdRecordNow);
			this.Menu = this.mnuMain;
			this.Name = "MainForm";
			this.Text = "QUT Sensor Recorder";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Button cmdRecordNow;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.TextBox txtSensorName;
		private System.Windows.Forms.TextBox txtFolder;
		private System.Windows.Forms.Button cmdSelectFolder;
		private System.Windows.Forms.Label lblWireless;
		private System.Windows.Forms.Timer wirelessTimer;
		private System.Windows.Forms.MenuItem mnuExit;
	}
}

