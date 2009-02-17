namespace ProcessorUI
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
			this.cmdStart = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.txtWorker = new System.Windows.Forms.TextBox();
			this.lblWorker = new System.Windows.Forms.Label();
			this.cmdOptions = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.statusTotal = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusDuration = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmdStart
			// 
			this.cmdStart.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdStart.Location = new System.Drawing.Point(162, 543);
			this.cmdStart.Name = "cmdStart";
			this.cmdStart.Size = new System.Drawing.Size(75, 23);
			this.cmdStart.TabIndex = 0;
			this.cmdStart.Text = "&Start";
			this.cmdStart.UseVisualStyleBackColor = true;
			this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
			// 
			// txtLog
			// 
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.Location = new System.Drawing.Point(12, 38);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(373, 499);
			this.txtLog.TabIndex = 1;
			// 
			// txtWorker
			// 
			this.txtWorker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtWorker.Location = new System.Drawing.Point(70, 12);
			this.txtWorker.Name = "txtWorker";
			this.txtWorker.Size = new System.Drawing.Size(234, 20);
			this.txtWorker.TabIndex = 2;
			this.txtWorker.TextChanged += new System.EventHandler(this.txtWorker_TextChanged);
			// 
			// lblWorker
			// 
			this.lblWorker.AutoSize = true;
			this.lblWorker.Location = new System.Drawing.Point(9, 15);
			this.lblWorker.Name = "lblWorker";
			this.lblWorker.Size = new System.Drawing.Size(55, 13);
			this.lblWorker.TabIndex = 3;
			this.lblWorker.Text = "My Name:";
			// 
			// cmdOptions
			// 
			this.cmdOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOptions.Location = new System.Drawing.Point(310, 10);
			this.cmdOptions.Name = "cmdOptions";
			this.cmdOptions.Size = new System.Drawing.Size(75, 23);
			this.cmdOptions.TabIndex = 4;
			this.cmdOptions.Text = "Options";
			this.cmdOptions.UseVisualStyleBackColor = true;
			this.cmdOptions.Click += new System.EventHandler(this.cmdOptions_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusTotal,
            this.statusDuration});
			this.statusStrip1.Location = new System.Drawing.Point(0, 569);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(397, 22);
			this.statusStrip1.TabIndex = 5;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// statusTotal
			// 
			this.statusTotal.Name = "statusTotal";
			this.statusTotal.Size = new System.Drawing.Size(37, 17);
			this.statusTotal.Text = "Total:";
			// 
			// statusDuration
			// 
			this.statusDuration.Name = "statusDuration";
			this.statusDuration.Size = new System.Drawing.Size(56, 17);
			this.statusDuration.Text = "Duration:";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(397, 591);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.cmdOptions);
			this.Controls.Add(this.lblWorker);
			this.Controls.Add(this.txtWorker);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.cmdStart);
			this.Name = "MainForm";
			this.Text = "QUT Sensors Processor";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cmdStart;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.TextBox txtWorker;
		private System.Windows.Forms.Label lblWorker;
		private System.Windows.Forms.Button cmdOptions;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statusTotal;
		private System.Windows.Forms.ToolStripStatusLabel statusDuration;
	}
}

