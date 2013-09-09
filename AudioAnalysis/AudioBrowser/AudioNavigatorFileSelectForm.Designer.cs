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
            this.comboAnalysisType = new System.Windows.Forms.ComboBox();
            this.btnCsvFileBrowse = new System.Windows.Forms.Button();
            this.lblAnalysisOutputDir = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnOutputDirBrowse = new System.Windows.Forms.Button();
            this.labelSelectedAnalyserKey = new System.Windows.Forms.Label();
            this.btnConfigFileEdit = new System.Windows.Forms.Button();
            this.txtConfigFile = new System.Windows.Forms.TextBox();
            this.btnConfigFileBrowse = new System.Windows.Forms.Button();
            this.lblAnalysisEditConfig = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtImageFile = new System.Windows.Forms.TextBox();
            this.btnImageFileBrowse = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnAudioFileBrowse = new System.Windows.Forms.Button();
            this.lblAnalysisFile = new System.Windows.Forms.Label();
            this.txtAudioFile = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtCsvFile
            // 
            this.txtCsvFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtCsvFile.Location = new System.Drawing.Point(292, 53);
            this.txtCsvFile.Name = "txtCsvFile";
            this.txtCsvFile.Size = new System.Drawing.Size(457, 20);
            this.txtCsvFile.TabIndex = 36;
            // 
            // comboAnalysisType
            // 
            this.comboAnalysisType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAnalysisType.FormattingEnabled = true;
            this.comboAnalysisType.Location = new System.Drawing.Point(216, 140);
            this.comboAnalysisType.Name = "comboAnalysisType";
            this.comboAnalysisType.Size = new System.Drawing.Size(271, 21);
            this.comboAnalysisType.TabIndex = 34;
            this.comboAnalysisType.SelectedIndexChanged += new System.EventHandler(this.comboAnalysisType_SelectedIndexChanged);
            // 
            // btnCsvFileBrowse
            // 
            this.btnCsvFileBrowse.Location = new System.Drawing.Point(216, 51);
            this.btnCsvFileBrowse.Name = "btnCsvFileBrowse";
            this.btnCsvFileBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnCsvFileBrowse.TabIndex = 32;
            this.btnCsvFileBrowse.Text = "Browse...";
            this.btnCsvFileBrowse.UseVisualStyleBackColor = true;
            this.btnCsvFileBrowse.Click += new System.EventHandler(this.btnSelecttxtCsvOFileFile_Click);
            // 
            // lblAnalysisOutputDir
            // 
            this.lblAnalysisOutputDir.AutoSize = true;
            this.lblAnalysisOutputDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisOutputDir.Location = new System.Drawing.Point(151, 54);
            this.lblAnalysisOutputDir.Name = "lblAnalysisOutputDir";
            this.lblAnalysisOutputDir.Size = new System.Drawing.Size(49, 15);
            this.lblAnalysisOutputDir.TabIndex = 31;
            this.lblAnalysisOutputDir.Text = "CSV file";
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(685, 310);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(64, 37);
            this.btnOk.TabIndex = 41;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(615, 310);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 37);
            this.btnCancel.TabIndex = 42;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 15);
            this.label1.TabIndex = 43;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 30);
            this.label2.TabIndex = 36;
            this.label2.Text = "1: Select either an image or \r\na csv results file:";
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.BackColor = System.Drawing.SystemColors.Control;
            this.txtOutputDir.Location = new System.Drawing.Point(292, 243);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(457, 20);
            this.txtOutputDir.TabIndex = 55;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 244);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(163, 15);
            this.label4.TabIndex = 54;
            this.label4.Text = "4: Select the output directory:";
            // 
            // btnOutputDirBrowse
            // 
            this.btnOutputDirBrowse.Location = new System.Drawing.Point(216, 241);
            this.btnOutputDirBrowse.Name = "btnOutputDirBrowse";
            this.btnOutputDirBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnOutputDirBrowse.TabIndex = 53;
            this.btnOutputDirBrowse.Text = "Browse...";
            this.btnOutputDirBrowse.UseVisualStyleBackColor = true;
            this.btnOutputDirBrowse.Click += new System.EventHandler(this.btnOutputDirBrowse_Click);
            // 
            // labelSelectedAnalyserKey
            // 
            this.labelSelectedAnalyserKey.AutoSize = true;
            this.labelSelectedAnalyserKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSelectedAnalyserKey.Location = new System.Drawing.Point(491, 141);
            this.labelSelectedAnalyserKey.Name = "labelSelectedAnalyserKey";
            this.labelSelectedAnalyserKey.Size = new System.Drawing.Size(25, 15);
            this.labelSelectedAnalyserKey.TabIndex = 52;
            this.labelSelectedAnalyserKey.Text = "key";
            // 
            // btnConfigFileEdit
            // 
            this.btnConfigFileEdit.Location = new System.Drawing.Point(629, 193);
            this.btnConfigFileEdit.Name = "btnConfigFileEdit";
            this.btnConfigFileEdit.Size = new System.Drawing.Size(120, 23);
            this.btnConfigFileEdit.TabIndex = 51;
            this.btnConfigFileEdit.Text = "Edit Config File";
            this.btnConfigFileEdit.UseVisualStyleBackColor = true;
            this.btnConfigFileEdit.Click += new System.EventHandler(this.btnConfigFileEdit_Click);
            // 
            // txtConfigFile
            // 
            this.txtConfigFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtConfigFile.Location = new System.Drawing.Point(292, 195);
            this.txtConfigFile.Name = "txtConfigFile";
            this.txtConfigFile.Size = new System.Drawing.Size(331, 20);
            this.txtConfigFile.TabIndex = 50;
            // 
            // btnConfigFileBrowse
            // 
            this.btnConfigFileBrowse.Location = new System.Drawing.Point(216, 193);
            this.btnConfigFileBrowse.Name = "btnConfigFileBrowse";
            this.btnConfigFileBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnConfigFileBrowse.TabIndex = 48;
            this.btnConfigFileBrowse.Text = "Browse...";
            this.btnConfigFileBrowse.UseVisualStyleBackColor = true;
            this.btnConfigFileBrowse.Click += new System.EventHandler(this.btnConfigFileBrowse_Click);
            // 
            // lblAnalysisEditConfig
            // 
            this.lblAnalysisEditConfig.AutoSize = true;
            this.lblAnalysisEditConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisEditConfig.Location = new System.Drawing.Point(12, 196);
            this.lblAnalysisEditConfig.Name = "lblAnalysisEditConfig";
            this.lblAnalysisEditConfig.Size = new System.Drawing.Size(179, 15);
            this.lblAnalysisEditConfig.TabIndex = 47;
            this.lblAnalysisEditConfig.Text = "3: Select the analysis config file:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(188, 15);
            this.label5.TabIndex = 46;
            this.label5.Text = "2: Select type of analysis from list:";
            // 
            // txtImageFile
            // 
            this.txtImageFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtImageFile.Location = new System.Drawing.Point(292, 91);
            this.txtImageFile.Name = "txtImageFile";
            this.txtImageFile.Size = new System.Drawing.Size(457, 20);
            this.txtImageFile.TabIndex = 60;
            // 
            // btnImageFileBrowse
            // 
            this.btnImageFileBrowse.Location = new System.Drawing.Point(216, 89);
            this.btnImageFileBrowse.Name = "btnImageFileBrowse";
            this.btnImageFileBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnImageFileBrowse.TabIndex = 59;
            this.btnImageFileBrowse.Text = "Browse...";
            this.btnImageFileBrowse.UseVisualStyleBackColor = true;
            this.btnImageFileBrowse.Click += new System.EventHandler(this.btnImageFileBrowse_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(139, 92);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 15);
            this.label6.TabIndex = 58;
            this.label6.Text = "Image file";
            // 
            // btnAudioFileBrowse
            // 
            this.btnAudioFileBrowse.Location = new System.Drawing.Point(216, 282);
            this.btnAudioFileBrowse.Name = "btnAudioFileBrowse";
            this.btnAudioFileBrowse.Size = new System.Drawing.Size(70, 23);
            this.btnAudioFileBrowse.TabIndex = 29;
            this.btnAudioFileBrowse.Text = "Browse...";
            this.btnAudioFileBrowse.UseVisualStyleBackColor = true;
            this.btnAudioFileBrowse.Click += new System.EventHandler(this.btnSelectAudioFile_Click);
            // 
            // lblAnalysisFile
            // 
            this.lblAnalysisFile.AutoSize = true;
            this.lblAnalysisFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisFile.Location = new System.Drawing.Point(12, 280);
            this.lblAnalysisFile.Name = "lblAnalysisFile";
            this.lblAnalysisFile.Size = new System.Drawing.Size(172, 30);
            this.lblAnalysisFile.TabIndex = 30;
            this.lblAnalysisFile.Text = "5: (optional) Select the source \r\naudio file:";
            // 
            // txtAudioFile
            // 
            this.txtAudioFile.BackColor = System.Drawing.SystemColors.Control;
            this.txtAudioFile.Location = new System.Drawing.Point(292, 284);
            this.txtAudioFile.Name = "txtAudioFile";
            this.txtAudioFile.Size = new System.Drawing.Size(457, 20);
            this.txtAudioFile.TabIndex = 35;
            // 
            // AudioNavigatorFileSelectForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(761, 358);
            this.Controls.Add(this.txtImageFile);
            this.Controls.Add(this.btnImageFileBrowse);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnOutputDirBrowse);
            this.Controls.Add(this.labelSelectedAnalyserKey);
            this.Controls.Add(this.btnConfigFileEdit);
            this.Controls.Add(this.txtConfigFile);
            this.Controls.Add(this.btnConfigFileBrowse);
            this.Controls.Add(this.lblAnalysisEditConfig);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtCsvFile);
            this.Controls.Add(this.txtAudioFile);
            this.Controls.Add(this.comboAnalysisType);
            this.Controls.Add(this.btnCsvFileBrowse);
            this.Controls.Add(this.lblAnalysisOutputDir);
            this.Controls.Add(this.lblAnalysisFile);
            this.Controls.Add(this.btnAudioFileBrowse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(777, 397);
            this.MinimumSize = new System.Drawing.Size(777, 397);
            this.Name = "AudioNavigatorFileSelectForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Analysis Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCsvFile;
        private System.Windows.Forms.ComboBox comboAnalysisType;
        private System.Windows.Forms.Button btnCsvFileBrowse;
        private System.Windows.Forms.Label lblAnalysisOutputDir;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnOutputDirBrowse;
        private System.Windows.Forms.Label labelSelectedAnalyserKey;
        private System.Windows.Forms.Button btnConfigFileEdit;
        private System.Windows.Forms.TextBox txtConfigFile;
        private System.Windows.Forms.Button btnConfigFileBrowse;
        private System.Windows.Forms.Label lblAnalysisEditConfig;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtImageFile;
        private System.Windows.Forms.Button btnImageFileBrowse;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnAudioFileBrowse;
        private System.Windows.Forms.Label lblAnalysisFile;
        private System.Windows.Forms.TextBox txtAudioFile;

    }
}