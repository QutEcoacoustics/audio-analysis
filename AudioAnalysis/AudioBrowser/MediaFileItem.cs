namespace AudioBrowser
{
    using System;
    using System.ComponentModel;
    using System.IO;

    using QutSensors.Shared;

    public class MediaFileItem : INotifyPropertyChanged
    {
        private FileInfo fullName;
        private string fileName;
        private TimeSpan duration;
        private string mediaType;
        private long fileLength;

        public MediaFileItem()
        {

        }

        public MediaFileItem(FileInfo file)
        {
            this.FullName = file;
            this.FileName = file.Name;
            this.FileLength = file.Length;

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

    /// <summary>
    /// See: http://stackoverflow.com/questions/3627922/format-timespan-in-datagridview-column
    /// </summary>
    public class TimeSpanFormatter : IFormatProvider, ICustomFormatter
    {
        #region IFormatProvider Members

        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType))
            {
                return this;
            }

            return null;
        }

        #endregion

        #region ICustomFormatter Members

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is TimeSpan)
            {
                var timeSpan = (TimeSpan)arg;
                return timeSpan.ToString(format);
            }

            var formattable = arg as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(format, formatProvider);
            }

            return arg != null ? arg.ToString() : string.Empty;
        }

        #endregion




    }

    /// <summary>
    /// See: http://stackoverflow.com/questions/3627922/format-timespan-in-datagridview-column
    /// </summary>
    public class DateTimeFormatter : IFormatProvider, ICustomFormatter
    {
        #region IFormatProvider Members

        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType))
            {
                return this;
            }

            return null;
        }

        #endregion

        #region ICustomFormatter Members

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is DateTime)
            {
                var dateTime = (DateTime)arg;
                return dateTime.ToString(format);
            }
            else
            {
                var formattable = arg as IFormattable;
                if (formattable != null)
                {
                    return formattable.ToString(format, formatProvider);
                }

                return arg != null ? arg.ToString() : string.Empty;
            }
        }

        #endregion


    }


    /// <summary>
    /// See: http://stackoverflow.com/questions/3627922/format-timespan-in-datagridview-column
    /// </summary>
    public class ByteCountFormatter : IFormatProvider, ICustomFormatter
    {
        #region IFormatProvider Members

        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType))
            {
                return this;
            }

            return null;
        }

        #endregion

        #region ICustomFormatter Members

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is DateTime)
            {
                var dateTime = (DateTime)arg;
                return dateTime.ToString(format);
            }
            else
            {
                var formattable = arg as IFormattable;
                if (formattable != null)
                {
                    return formattable.ToString(format, formatProvider);
                }

                return arg != null ? arg.ToString() : string.Empty;
            }
        }

        #endregion


    }
}
