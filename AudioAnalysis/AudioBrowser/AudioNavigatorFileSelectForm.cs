using AudioBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AudioBrowser
{
    public partial class AudioNavigatorFileSelectForm : Form
    {
        public FileInfo AudioFile
        {
            get { try { return new FileInfo(this.txtAudioFile.Text); } catch { return null; } }
            set { if (value != null) { this.txtAudioFile.Text = value.FullName; } }
        }
        public FileInfo CsvFile
        {
            get { try { return new FileInfo(this.txtCsvFile.Text); } catch { return null; } }
            set { if (value != null) { this.txtCsvFile.Text = value.FullName; } }
        }
        public FileInfo ImgFile
        {
            get { try { return new FileInfo(this.txtImageFile.Text); } catch { return null; } }
            set { if (value != null) { this.txtImageFile.Text = value.FullName; } }
        }
        public string AnalysisId
        {
            get
            {
                var selectedAnalysisType = ((KeyValuePair<string, string>)this.comboAnalysisType.SelectedItem).Key;
                return selectedAnalysisType;
            }
            set
            {
                if (value != null)
                {
                    this.comboAnalysisType.SelectedValue = value;
                }
            }
        }

        public FileInfo AnalysisConfigFile
        {
            get { try { return new FileInfo(this.txtConfigFile.Text); } catch { return null; } }
            set { if (value != null) { this.txtConfigFile.Text = value.FullName; } }
        }
        public DirectoryInfo OutputDir
        {
            get { try { return new DirectoryInfo(this.txtOutputDir.Text); } catch { return null; } }
            set { if (value != null) { this.txtOutputDir.Text = value.FullName; } }
        }

        private Helper helper;

        public AudioNavigatorFileSelectForm(Helper helper)
        {
            this.helper = helper;

            InitializeComponent();

            this.comboAnalysisType.DataSource = this.helper.AnalysersAvailable.ToList();
            this.comboAnalysisType.DisplayMember = "Value";
            this.comboAnalysisType.ValueMember = "Key";

            this.AnalysisId = this.helper.DefaultAnalysisIdentifier;
        }

        private void btnSelecttxtCsvOFileFile_Click(object sender, EventArgs e)
        {
            var currentDir = string.Empty;
            if (this.CsvFile != null && Directory.Exists(this.CsvFile.DirectoryName))
            {
                currentDir = this.CsvFile.DirectoryName;
            }

            var file = Helper.PromptUserToSelectFile("Select Csv File", this.helper.SelectCsvFilter, currentDir);
            if (file != null)
            {
                this.CsvFile = file;
            }
        }

        private void btnImageFileBrowse_Click(object sender, EventArgs e)
        {
            var currentDir = string.Empty;
            if (this.ImgFile != null && Directory.Exists(this.ImgFile.DirectoryName))
            {
                currentDir = this.ImgFile.DirectoryName;
            }

            var file = Helper.PromptUserToSelectFile("Select Image File", this.helper.SelectImageFilter, currentDir);
            if (file != null)
            {
                this.ImgFile = file;
            }
        }

        private void btnConfigFileBrowse_Click(object sender, EventArgs e)
        {
            var currentDir = string.Empty;
            if (this.AnalysisConfigFile != null && Directory.Exists(this.AnalysisConfigFile.DirectoryName))
            {
                currentDir = this.AnalysisConfigFile.DirectoryName;
            }

            var file = Helper.PromptUserToSelectFile("Select configuration file for analyser", this.helper.SelectConfigFilter, currentDir);
            if (file != null)
            {
                this.AnalysisConfigFile = file;
            }

        }

        private void btnOutputDirBrowse_Click(object sender, EventArgs e)
        {
            var selectedDir = Helper.PromptUserToSelectDirectory("Select output directory for analysis");

            if (selectedDir != null && Directory.Exists(selectedDir.FullName))
            {
                this.OutputDir = selectedDir;
            }
        }

        private void btnConfigFileEdit_Click(object sender, EventArgs e)
        {
            if (this.AnalysisConfigFile == null || !File.Exists(this.AnalysisConfigFile.FullName))
            {
                MessageBox.Show("Please specify a config file.");
            }
            else if (this.helper.TextEditorExe == null || !File.Exists(this.helper.TextEditorExe.FullName))
            {
                MessageBox.Show("Could not find a program to edit text files.");
            }
            else
            {
                TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(this.helper.TextEditorExe.FullName);
                process.Run(this.AnalysisConfigFile.FullName, this.helper.DefaultOutputDir.FullName, false);
            }
        }

        private void btnSelectAudioFile_Click(object sender, EventArgs e)
        {
            var currentDir = string.Empty;
            if (this.AudioFile != null && Directory.Exists(this.AudioFile.DirectoryName))
            {
                currentDir = this.AudioFile.DirectoryName;
            }

            var file = Helper.PromptUserToSelectFile("Select Audio File", this.helper.SelectAudioFilter, currentDir);
            if (file != null)
            {
                this.AudioFile = file;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // check for valid settings

            // image or csv file, analyser, config file, output
            var csvExists = this.CsvFile != null && File.Exists(this.CsvFile.FullName);
            var imgExists = this.ImgFile != null && File.Exists(this.ImgFile.FullName);
            var configExists = this.AnalysisConfigFile != null && File.Exists(this.AnalysisConfigFile.FullName);
            var outputExists = this.OutputDir != null && Directory.Exists(this.OutputDir.FullName);
            var analysisSelected = !string.IsNullOrWhiteSpace(this.AnalysisId);
            var audioExistsIfPathEntered = this.AudioFile == null ? true : File.Exists(this.AudioFile.FullName);

            if ((csvExists || imgExists) && configExists && outputExists && analysisSelected && audioExistsIfPathEntered)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Please select a csv result file or an image file. The analyser type, config file and output directory are also required. If you select an audio file, ensure the path is correct.");
                this.DialogResult = DialogResult.None;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // close form
            this.Close();
        }

        private void comboAnalysisType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.labelSelectedAnalyserKey.Text = "Id: " + this.AnalysisId;
        }
    }
}
