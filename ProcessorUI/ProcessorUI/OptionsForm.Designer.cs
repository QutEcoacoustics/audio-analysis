namespace ProcessorUI
{
	partial class OptionsForm
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
			System.Windows.Forms.Label lblServer;
			System.Windows.Forms.Label lblDataFolder;
			this.txtServer = new System.Windows.Forms.TextBox();
			this.txtDataFolder = new System.Windows.Forms.TextBox();
			this.cmdChooseDataFolder = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.cmdOK = new System.Windows.Forms.Button();
			lblServer = new System.Windows.Forms.Label();
			lblDataFolder = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblServer
			// 
			lblServer.AutoSize = true;
			lblServer.Location = new System.Drawing.Point(12, 15);
			lblServer.Name = "lblServer";
			lblServer.Size = new System.Drawing.Size(41, 13);
			lblServer.TabIndex = 0;
			lblServer.Text = "Server:";
			// 
			// lblDataFolder
			// 
			lblDataFolder.AutoSize = true;
			lblDataFolder.Location = new System.Drawing.Point(12, 41);
			lblDataFolder.Name = "lblDataFolder";
			lblDataFolder.Size = new System.Drawing.Size(65, 13);
			lblDataFolder.TabIndex = 1;
			lblDataFolder.Text = "Data Folder:";
			// 
			// txtServer
			// 
			this.txtServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtServer.Location = new System.Drawing.Point(59, 12);
			this.txtServer.Name = "txtServer";
			this.txtServer.Size = new System.Drawing.Size(295, 20);
			this.txtServer.TabIndex = 2;
			// 
			// txtDataFolder
			// 
			this.txtDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDataFolder.Location = new System.Drawing.Point(83, 38);
			this.txtDataFolder.Name = "txtDataFolder";
			this.txtDataFolder.Size = new System.Drawing.Size(241, 20);
			this.txtDataFolder.TabIndex = 3;
			// 
			// cmdChooseDataFolder
			// 
			this.cmdChooseDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdChooseDataFolder.Location = new System.Drawing.Point(330, 35);
			this.cmdChooseDataFolder.Name = "cmdChooseDataFolder";
			this.cmdChooseDataFolder.Size = new System.Drawing.Size(24, 23);
			this.cmdChooseDataFolder.TabIndex = 4;
			this.cmdChooseDataFolder.Text = "...";
			this.cmdChooseDataFolder.UseVisualStyleBackColor = true;
			this.cmdChooseDataFolder.Click += new System.EventHandler(this.cmdChooseDataFolder_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(279, 69);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.Location = new System.Drawing.Point(198, 69);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 6;
			this.cmdOK.Text = "&OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// OptionsForm
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(366, 104);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdChooseDataFolder);
			this.Controls.Add(this.txtDataFolder);
			this.Controls.Add(this.txtServer);
			this.Controls.Add(lblDataFolder);
			this.Controls.Add(lblServer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "OptionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Processor Options";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtServer;
		private System.Windows.Forms.TextBox txtDataFolder;
		private System.Windows.Forms.Button cmdChooseDataFolder;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Button cmdOK;
	}
}