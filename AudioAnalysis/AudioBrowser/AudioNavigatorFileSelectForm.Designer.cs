namespace AudioBrowser
{
    partial class AudioNavigatorFileSelectForm
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
            this.txtCsvFile = new System.Windows.Forms.TextBox();
            this.txtAudioFile = new System.Windows.Forms.TextBox();
            this.comboAnalysisType = new System.Windows.Forms.ComboBox();
            this.lblAnalysisType = new System.Windows.Forms.Label();
            this.btnSelectCsvFile = new System.Windows.Forms.Button();
            this.lblAnalysisOutputDir = new System.Windows.Forms.Label();
            this.lblAnalysisFile = new System.Windows.Forms.Label();
            this.btnSelectAudioFile = new System.Windows.Forms.Button();
            this.txtOutputDirectory = new System.Windows.Forms.TextBox();
            this.btnSelectOutputDirectory = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtCsvFile
            // 
            this.txtCsvFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtCsvFile.Location = new System.Drawing.Point(15, 76);
            this.txtCsvFile.Name = "txtCsvFile";
            this.txtCsvFile.Size = new System.Drawing.Size(846, 20);
            this.txtCsvFile.TabIndex = 36;
            // 
            // txtAudioFile
            // 
            this.txtAudioFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtAudioFile.Location = new System.Drawing.Point(15, 35);
            this.txtAudioFile.Name = "txtAudioFile";
            this.txtAudioFile.Size = new System.Drawing.Size(846, 20);
            this.txtAudioFile.TabIndex = 35;
            // 
            // comboAnalysisType
            // 
            this.comboAnalysisType.FormattingEnabled = true;
            this.comboAnalysisType.Location = new System.Drawing.Point(15, 158);
            this.comboAnalysisType.Name = "comboAnalysisType";
            this.comboAnalysisType.Size = new System.Drawing.Size(121, 21);
            this.comboAnalysisType.TabIndex = 34;
            // 
            // lblAnalysisType
            // 
            this.lblAnalysisType.AutoSize = true;
            this.lblAnalysisType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisType.Location = new System.Drawing.Point(12, 140);
            this.lblAnalysisType.Name = "lblAnalysisType";
            this.lblAnalysisType.Size = new System.Drawing.Size(181, 15);
            this.lblAnalysisType.TabIndex = 33;
            this.lblAnalysisType.Text = "4: Select \'analysis type\' from list:";
            // 
            // btnSelectCsvFile
            // 
            this.btnSelectCsvFile.Location = new System.Drawing.Point(867, 72);
            this.btnSelectCsvFile.Name = "btnSelectCsvFile";
            this.btnSelectCsvFile.Size = new System.Drawing.Size(64, 23);
            this.btnSelectCsvFile.TabIndex = 32;
            this.btnSelectCsvFile.Text = "Browse...";
            this.btnSelectCsvFile.UseVisualStyleBackColor = true;
            this.btnSelectCsvFile.Click += new System.EventHandler(this.btnSelectCsvFile_Click);
            // 
            // lblAnalysisOutputDir
            // 
            this.lblAnalysisOutputDir.AutoSize = true;
            this.lblAnalysisOutputDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisOutputDir.Location = new System.Drawing.Point(12, 58);
            this.lblAnalysisOutputDir.Name = "lblAnalysisOutputDir";
            this.lblAnalysisOutputDir.Size = new System.Drawing.Size(206, 15);
            this.lblAnalysisOutputDir.TabIndex = 31;
            this.lblAnalysisOutputDir.Text = "2: Select the csv file for the audio file:";
            // 
            // lblAnalysisFile
            // 
            this.lblAnalysisFile.AutoSize = true;
            this.lblAnalysisFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisFile.Location = new System.Drawing.Point(12, 9);
            this.lblAnalysisFile.Name = "lblAnalysisFile";
            this.lblAnalysisFile.Size = new System.Drawing.Size(185, 15);
            this.lblAnalysisFile.TabIndex = 30;
            this.lblAnalysisFile.Text = "1: Select an audio file to analyse:";
            // 
            // btnSelectAudioFile
            // 
            this.btnSelectAudioFile.Location = new System.Drawing.Point(867, 31);
            this.btnSelectAudioFile.Name = "btnSelectAudioFile";
            this.btnSelectAudioFile.Size = new System.Drawing.Size(64, 23);
            this.btnSelectAudioFile.TabIndex = 29;
            this.btnSelectAudioFile.Text = "Browse...";
            this.btnSelectAudioFile.UseVisualStyleBackColor = true;
            this.btnSelectAudioFile.Click += new System.EventHandler(this.btnSelectAudioFile_Click);
            // 
            // txtOutputDirectory
            // 
            this.txtOutputDirectory.BackColor = System.Drawing.SystemColors.Control;
            this.txtOutputDirectory.Location = new System.Drawing.Point(15, 117);
            this.txtOutputDirectory.Name = "txtOutputDirectory";
            this.txtOutputDirectory.Size = new System.Drawing.Size(846, 20);
            this.txtOutputDirectory.TabIndex = 40;
            // 
            // btnSelectOutputDirectory
            // 
            this.btnSelectOutputDirectory.Location = new System.Drawing.Point(867, 113);
            this.btnSelectOutputDirectory.Name = "btnSelectOutputDirectory";
            this.btnSelectOutputDirectory.Size = new System.Drawing.Size(64, 23);
            this.btnSelectOutputDirectory.TabIndex = 39;
            this.btnSelectOutputDirectory.Text = "Browse...";
            this.btnSelectOutputDirectory.UseVisualStyleBackColor = true;
            this.btnSelectOutputDirectory.Click += new System.EventHandler(this.btnSelectOutputDirectory_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 15);
            this.label1.TabIndex = 38;
            this.label1.Text = "3: Select a directory for the output:";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(867, 158);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(64, 37);
            this.btnOk.TabIndex = 41;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // AudioNavigatorFileSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(943, 203);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtOutputDirectory);
            this.Controls.Add(this.btnSelectOutputDirectory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCsvFile);
            this.Controls.Add(this.txtAudioFile);
            this.Controls.Add(this.comboAnalysisType);
            this.Controls.Add(this.lblAnalysisType);
            this.Controls.Add(this.btnSelectCsvFile);
            this.Controls.Add(this.lblAnalysisOutputDir);
            this.Controls.Add(this.lblAnalysisFile);
            this.Controls.Add(this.btnSelectAudioFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AudioNavigatorFileSelectForm";
            this.Text = "Audio Navigator File Select";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCsvFile;
        private System.Windows.Forms.TextBox txtAudioFile;
        private System.Windows.Forms.ComboBox comboAnalysisType;
        private System.Windows.Forms.Label lblAnalysisType;
        private System.Windows.Forms.Button btnSelectCsvFile;
        private System.Windows.Forms.Label lblAnalysisOutputDir;
        private System.Windows.Forms.Label lblAnalysisFile;
        private System.Windows.Forms.Button btnSelectAudioFile;
        private System.Windows.Forms.TextBox txtOutputDirectory;
        private System.Windows.Forms.Button btnSelectOutputDirectory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;

    }
}