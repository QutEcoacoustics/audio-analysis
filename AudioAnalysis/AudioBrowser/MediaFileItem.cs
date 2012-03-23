namespace AudioBrowser
{
    using System;
    using System.ComponentModel;
    using System.IO;

    using Acoustics.Shared;


    public class MediaFileItem : INotifyPropertyChanged
    {
        private FileInfo fullName;
        private string fileName;
        private TimeSpan duration;
        private string mediaType;
        private DateTime lastModified;
        private long fileLength;

        public MediaFileItem()
        {

        }

        public MediaFileItem(FileInfo file)
        {
            this.FullName = file;
            this.FileName = file.Name;
            this.FileLength = file.Length;
            this.LastModified = file.LastWriteTime;

            this.MediaType = MediaTypes.GetMediaType(file.Extension);
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

        public DateTime LastModified
        {
            get
            {
                return this.lastModified;
            }
            set
            {
                if (value != this.lastModified)
                {
                    this.lastModified = value;
                    OnPropertyChanged("lastModified");
                }
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return this.duration;
            }
            set
            {
                if (value != this.duration)
                {
                    this.duration = value;
                    OnPropertyChanged("duration");
                }
            }
        }

        public string MediaType
        {
            get
            {
                return this.mediaType;
            }
            set
            {
                if (value != this.mediaType)
                {
                    this.mediaType = value;
                    OnPropertyChanged("mediaType");
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
