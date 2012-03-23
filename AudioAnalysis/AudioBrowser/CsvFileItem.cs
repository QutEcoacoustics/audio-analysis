namespace AudioBrowser
{
    using System;
    using System.ComponentModel;
    using System.IO;

    public class CsvFileItem : INotifyPropertyChanged
    {
        private FileInfo fullName;
        private string fileName;
        private long fileLength;

        public CsvFileItem()
        {

        }

        public CsvFileItem(FileInfo file)
        {
            this.FullName = file;
            this.FileName = file.Name;
            this.FileLength = file.Length;
        }

        public FileInfo FullName
        {
            get
            {
                return this.fullName;
            }
            set
            {
                if (value != this.fullName)
                {
                    this.fullName = value;
                    OnPropertyChanged("fullName");
                }
            }
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                if (value != this.fileName)
                {
                    this.fileName = value;
                    OnPropertyChanged("fileName");
                }
            }
        }



        public long FileLength
        {
            get
            {
                return this.fileLength;
            }
            set
            {
                if (value != this.fileLength)
                {
                    this.fileLength = value;
                    OnPropertyChanged("fileLength");
                }
            }
        }

        public FileInfo GetFileInfo()
        {
            return this.fullName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
