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
            get
            {
                if (!string.IsNullOrWhiteSpace(this.txtAudioFile.Text))
                {
                    return new FileInfo(this.txtAudioFile.Text);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    this.txtAudioFile.Text = value.FullName;
                }
            }
        }

        public FileInfo CsvFile
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.txtCsvFile.Text))
                {
                    return new FileInfo(this.txtCsvFile.Text);
                } 
                return null;
            }
            set
            {
                if (value != null)
                {
                    this.txtCsvFile.Text = value.FullName;
                }
            }
        }

        public DirectoryInfo OutputDirectory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.txtOutputDirectory.Text))
                {
                    return new DirectoryInfo(this.txtOutputDirectory.Text);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    this.txtOutputDirectory.Text = value.FullName;
                }
            }
        }

        public string AnalysisName
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

        private PluginHelper pluginHelper;

        public AudioNavigatorFileSelectForm()
        {
            InitializeComponent();

            this.pluginHelper = new PluginHelper();
            this.pluginHelper.FindIAnalysisPlugins();
            var analyserList = this.pluginHelper.GetAnalysisPluginsList();

            this.comboAnalysisType.DataSource = analyserList.ToList();
            this.comboAnalysisType.DisplayMember = "Value";
            this.comboAnalysisType.ValueMember = "Key";
            this.comboAnalysisType.SelectedIndex = 1;
        }

        private void btnSelectAudioFile_Click(object sender, EventArgs e)
        {
            var currentDir = this.txtAudioFile.Text;
            if (!string.IsNullOrWhiteSpace(currentDir))
            {
                currentDir = Path.GetDirectoryName(currentDir);
            }

            var file = PromptUserToSelectFile("Select Audio File", "Audio Files|*.mp3;*.wav|All files|*.*", currentDir);
            if (file != null)
            {
                this.txtAudioFile.Text = file.FullName;
            }
        }

        private void btnSelectCsvFile_Click(object sender, EventArgs e)
        {
            var currentDir = this.txtCsvFile.Text;
            if (!string.IsNullOrWhiteSpace(currentDir))
            {
                currentDir = Path.GetDirectoryName(currentDir);
            }

            var file = PromptUserToSelectFile("Select Csv File", "CSV files (*.csv, *.tsv)|*.csv;*.tsv", currentDir);
            if (file != null)
            {
                this.txtCsvFile.Text = file.FullName;
            }
        }

        private void btnSelectOutputDirectory_Click(object sender, EventArgs e)
        {
            var dir = PromptUserToSelectDirectory("Select the directory that you want to use for output files.");
            if (dir != null)
            {
                this.txtOutputDirectory.Text = dir.FullName;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // construct the new audio navigator object.

            if (File.Exists(this.txtAudioFile.Text) &&
                File.Exists(this.txtCsvFile.Text) &&
                Directory.Exists(this.txtOutputDirectory.Text))
            {
                // if all information is available and valid, close the form
                this.Close();
            }
            else
            {
                MessageBox.Show("Audio file, csv file, output dir are all required, and the paths must exist.");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtAudioFile.Text) || !File.Exists(this.txtAudioFile.Text)){
                this.txtAudioFile.Text = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(this.txtCsvFile.Text) || !File.Exists(this.txtCsvFile.Text))
            {
                this.txtCsvFile.Text = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(this.txtOutputDirectory.Text) || !Directory.Exists(this.txtOutputDirectory.Text))
            {
                this.txtOutputDirectory.Text = string.Empty;
            }

            this.Close();
        }

        private FileInfo PromptUserToSelectFile(string title, string filter, string initialDirectory)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = title;
            fdlg.Filter = filter;
            fdlg.FilterIndex = 1;
            fdlg.RestoreDirectory = false;

            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            {
                fdlg.InitialDirectory = initialDirectory;
            }

            fdlg.Multiselect = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                return new FileInfo(fdlg.FileName);
            }
            else
            {
                return null;
            }
        }

        private DirectoryInfo PromptUserToSelectDirectory(string descr)
        {
            FolderBrowserDialog fdlg = new FolderBrowserDialog();

            // Set the help text description for the FolderBrowserDialog. 
            fdlg.Description = descr;

            // Do not allow the user to create new files via the FolderBrowserDialog. 
            fdlg.ShowNewFolderButton = false;

            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                return new DirectoryInfo(fdlg.SelectedPath);
            }
            else
            {
                return null;
            }
        }
    }
}
