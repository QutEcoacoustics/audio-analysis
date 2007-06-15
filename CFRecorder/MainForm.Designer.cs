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
            this.mnuExit = new System.Windows.Forms.MenuItem();
            this.mnuRecordNow = new System.Windows.Forms.MenuItem();
            this.mnuWSTest = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.timer = new System.Windows.Forms.Timer();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtSensorName = new System.Windows.Forms.TextBox();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.cmdSelectFolder = new System.Windows.Forms.Button();
            this.lblWireless = new System.Windows.Forms.Label();
            this.wirelessTimer = new System.Windows.Forms.Timer();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.MenuItems.Add(this.mnuExit);
            this.mnuMain.MenuItems.Add(this.mnuRecordNow);
            this.mnuMain.MenuItems.Add(this.mnuWSTest);
            this.mnuMain.MenuItems.Add(this.menuItem1);
            // 
            // mnuExit
            // 
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // mnuRecordNow
            // 
            this.mnuRecordNow.Text = "Record Now";
            this.mnuRecordNow.Click += new System.EventHandler(this.mnuRecordNow_Click);
            // 
            // mnuWSTest
            // 
            this.mnuWSTest.Text = "WS Test";
            this.mnuWSTest.Click += new System.EventHandler(this.mnuWSTest_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.MenuItems.Add(this.menuItem2);
            this.menuItem1.MenuItems.Add(this.menuItem3);
            this.menuItem1.MenuItems.Add(this.menuItem4);
            this.menuItem1.MenuItems.Add(this.menuItem5);
            this.menuItem1.MenuItems.Add(this.menuItem6);
            this.menuItem1.MenuItems.Add(this.menuItem7);
            this.menuItem1.Text = "PDA";
            // 
            // menuItem2
            // 
            this.menuItem2.Text = "Switch off Screen";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Text = "Restart";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Text = "Check Available Diskspace";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 1800000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(3, 108);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(237, 157);
            this.txtLog.TabIndex = 2;
            // 
            // txtSensorName
            // 
            this.txtSensorName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSensorName.Location = new System.Drawing.Point(3, 27);
            this.txtSensorName.Name = "txtSensorName";
            this.txtSensorName.Size = new System.Drawing.Size(234, 21);
            this.txtSensorName.TabIndex = 3;
            this.txtSensorName.Text = "QUT00";
            this.txtSensorName.TextChanged += new System.EventHandler(this.txtSensorName_TextChanged);
            // 
            // txtFolder
            // 
            this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolder.Location = new System.Drawing.Point(3, 81);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(209, 21);
            this.txtFolder.TabIndex = 4;
            this.txtFolder.Text = "\\My Flash Disk";
            // 
            // cmdSelectFolder
            // 
            this.cmdSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSelectFolder.Location = new System.Drawing.Point(218, 82);
            this.cmdSelectFolder.Name = "cmdSelectFolder";
            this.cmdSelectFolder.Size = new System.Drawing.Size(19, 20);
            this.cmdSelectFolder.TabIndex = 5;
            this.cmdSelectFolder.Text = "...";
            this.cmdSelectFolder.Click += new System.EventHandler(this.cmdSelectFolder_Click);
            // 
            // lblWireless
            // 
            this.lblWireless.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWireless.Location = new System.Drawing.Point(4, 4);
            this.lblWireless.Name = "lblWireless";
            this.lblWireless.Size = new System.Drawing.Size(236, 20);
            this.lblWireless.Text = "Wireless:";
            // 
            // wirelessTimer
            // 
            this.wirelessTimer.Enabled = true;
            this.wirelessTimer.Interval = 20000;
            this.wirelessTimer.Tick += new System.EventHandler(this.wirelessTimer_Tick);
            // 
            // txtServer
            // 
            this.txtServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServer.Location = new System.Drawing.Point(3, 54);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(234, 21);
            this.txtServer.TabIndex = 6;
            this.txtServer.TextChanged += new System.EventHandler(this.txtServer_TextChanged);
            // 
            // menuItem5
            // 
            this.menuItem5.Text = "Check battery life";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Text = "Turn off backlight";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Text = "Check physical memory";
            this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.lblWireless);
            this.Controls.Add(this.cmdSelectFolder);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.txtSensorName);
            this.Controls.Add(this.txtLog);
            this.Menu = this.mnuMain;
            this.Name = "MainForm";
            this.Text = "QUT Sensor Recorder";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.TextBox txtSensorName;
		private System.Windows.Forms.TextBox txtFolder;
		private System.Windows.Forms.Button cmdSelectFolder;
		private System.Windows.Forms.Label lblWireless;
		private System.Windows.Forms.Timer wirelessTimer;
		private System.Windows.Forms.MenuItem mnuExit;
		private System.Windows.Forms.MenuItem mnuRecordNow;
		private System.Windows.Forms.TextBox txtServer;
		private System.Windows.Forms.MenuItem mnuWSTest;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;
	}
}

