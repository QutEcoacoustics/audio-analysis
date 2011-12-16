using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.UI.Display.Classes
{
    using System.IO;

    public class DataStorageItem
    {
        public DirectoryInfo Directory { get; set; }

        public long SubDirectoryCount { get; set; }

        public long FileCount { get; set; }

        public IEnumerable<string> Extensions { get; set; }

        public IEnumerable<string> MimeTypes
        {
            get
            {
                return Extensions.Select(Shared.MediaTypes.GetMediaType);
            }
        }

        public DateTime EarliestFile { get; set; }

        public DateTime LatestFile { get; set; }

        public string StorageType { get; set; }

        public long TotalFileSize { get; set; }
    }
}
