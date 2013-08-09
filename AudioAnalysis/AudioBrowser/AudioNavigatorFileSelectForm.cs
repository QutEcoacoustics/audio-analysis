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
                return new FileInfo(this.txtAudioFile.Text);
            }
            set
            {
                this.txtAudioFile.Text = value.FullName;
            }
        }

        public FileInfo CsvFile
        {
            get
            {
                return new FileInfo(this.txtCsvFile.Text);
            }
            set
            {
                this.txtCsvFile.Text = value.FullName;
            }
        }

        public DirectoryInfo OutputDirectory
        {
            get
            {
                return new DirectoryInfo(this.txtOutputDirectory.Text);
            }
            set
            {
                this.txtOutputDirectory.Text = value.FullName;
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
                this.comboAnalysisType.SelectedValue = value;
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
                this.Close();
            }
            else
            {
                MessageBox.Show("Audio file, csv file, output dir are all required, and the paths must exist.");
            }

            // if all information is available and valid, close the form
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
