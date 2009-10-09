using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QutSensors.Processor
{
    public class TempDirectory : IDisposable
    {
        public TempDirectory()
		{
			string temp = Utilities.GetTempFileName();
            DirectoryName = Path.Combine(
                !string.IsNullOrEmpty(Settings.TempFolder) ? Settings.TempFolder : Path.GetTempPath(),
                temp
            );
            Directory.CreateDirectory(DirectoryName);
		}

        public string DirectoryName { get; protected set; }

        #region IDisposable Members

        ~TempDirectory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            Directory.Delete(DirectoryName, true);
        }

        #endregion
    }
}
