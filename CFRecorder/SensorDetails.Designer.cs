namespace CFRecorder
{
	partial class SensorDetails
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.MainMenu mainMenu1;

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
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.mnuSave = new System.Windows.Forms.MenuItem();
			this.mnuCancel = new System.Windows.Forms.MenuItem();
			this.lblID = new System.Windows.Forms.Label();
			this.lblDescription = new System.Windows.Forms.Label();
			this.txtID = new System.Windows.Forms.TextBox();
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.txtName = new System.Windows.Forms.TextBox();
			this.lblName = new System.Windows.Forms.Label();
			this.lblFrequency = new System.Windows.Forms.Label();
			this.dtpFrequency = new System.Windows.Forms.DateTimePicker();
			this.lblDuration = new System.Windows.Forms.Label();
			this.dtpDuration = new System.Windows.Forms.DateTimePicker();
			this.chkEnableLogging = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.Add(this.mnuSave);
			this.mainMenu1.MenuItems.Add(this.mnuCancel);
			// 
			// mnuSave
			// 
			this.mnuSave.Text = "&Save";
			this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
			// 
			// mnuCancel
			// 
			this.mnuCancel.Text = "&Cancel";
			this.mnuCancel.Click += new System.EventHandler(this.mnuCancel_Click);
			// 
			// lblID
			// 
			this.lblID.Location = new System.Drawing.Point(3, 4);
			this.lblID.Name = "lblID";
			this.lblID.Size = new System.Drawing.Size(45, 20);
			this.lblID.Text = "ID:";
			// 
			// lblDescription
			// 
			this.lblDescription.Location = new System.Drawing.Point(6, 137);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(70, 20);
			this.lblDescription.Text = "Description:";
			// 
			// txtID
			// 
			this.txtID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtID.Enabled = false;
			this.txtID.Location = new System.Drawing.Point(54, 3);
			this.txtID.Name = "txtID";
			this.txtID.Size = new System.Drawing.Size(183, 21);
			this.txtID.TabIndex = 3;
			// 
			// txtDescription
			// 
			this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDescription.Enabled = false;
			this.txtDescription.Location = new System.Drawing.Point(3, 160);
			this.txtDescription.Multiline = true;
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.Size = new System.Drawing.Size(234, 108);
			this.txtDescription.TabIndex = 4;
			// 
			// txtName
			// 
			this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtName.Enabled = false;
			this.txtName.Location = new System.Drawing.Point(54, 30);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(183, 21);
			this.txtName.TabIndex = 6;
			// 
			// lblName
			// 
			this.lblName.Location = new System.Drawing.Point(3, 31);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(45, 20);
			this.lblName.Text = "Name:";
			// 
			// lblFrequency
			// 
			this.lblFrequency.Location = new System.Drawing.Point(3, 60);
			this.lblFrequency.Name = "lblFrequency";
			this.lblFrequency.Size = new System.Drawing.Size(70, 20);
			this.lblFrequency.Text = "Frequency:";
			// 
			// dtpFrequency
			// 
			this.dtpFrequency.CustomFormat = "HH:mm:ss";
			this.dtpFrequency.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpFrequency.Location = new System.Drawing.Point(79, 58);
			this.dtpFrequency.Name = "dtpFrequency";
			this.dtpFrequency.Size = new System.Drawing.Size(158, 22);
			this.dtpFrequency.TabIndex = 13;
			// 
			// lblDuration
			// 
			this.lblDuration.Location = new System.Drawing.Point(3, 88);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(70, 20);
			this.lblDuration.Text = "Duration:";
			// 
			// dtpDuration
			// 
			this.dtpDuration.CustomFormat = "HH:mm:ss";
			this.dtpDuration.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpDuration.Location = new System.Drawing.Point(79, 86);
			this.dtpDuration.Name = "dtpDuration";
			this.dtpDuration.Size = new System.Drawing.Size(158, 22);
			this.dtpDuration.TabIndex = 16;
			// 
			// chkEnableLogging
			// 
			this.chkEnableLogging.Location = new System.Drawing.Point(6, 114);
			this.chkEnableLogging.Name = "chkEnableLogging";
			this.chkEnableLogging.Size = new System.Drawing.Size(234, 20);
			this.chkEnableLogging.TabIndex = 22;
			this.chkEnableLogging.Text = "Enable Logging:";
			// 
			// SensorDetails
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(240, 268);
			this.Controls.Add(this.chkEnableLogging);
			this.Controls.Add(this.dtpDuration);
			this.Controls.Add(this.lblDuration);
			this.Controls.Add(this.dtpFrequency);
			this.Controls.Add(this.lblFrequency);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.txtDescription);
			this.Controls.Add(this.txtID);
			this.Controls.Add(this.lblDescription);
			this.Controls.Add(this.lblID);
			this.Menu = this.mainMenu1;
			this.Name = "SensorDetails";
			this.Text = "SensorDetails";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblID;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.TextBox txtID;
		private System.Windows.Forms.TextBox txtDescription;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.MenuItem mnuSave;
		private System.Windows.Forms.MenuItem mnuCancel;
		private System.Windows.Forms.Label lblFrequency;
		private System.Windows.Forms.DateTimePicker dtpFrequency;
		private System.Windows.Forms.Label lblDuration;
		private System.Windows.Forms.DateTimePicker dtpDuration;
		private System.Windows.Forms.CheckBox chkEnableLogging;
	}
}