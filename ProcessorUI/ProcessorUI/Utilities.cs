using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace ProcessorUI
{
	static class Utilities
	{
		public static string GetTempFileName()
		{
			string tempFile = Path.GetTempFileName();
			File.Delete(tempFile);
			if (!string.IsNullOrEmpty(Settings.TempFolder))
				return Path.Combine(Settings.TempFolder, Path.GetFileNameWithoutExtension(tempFile));
			else
				return Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(tempFile));
		}

		public static void DownloadFile(string sourceUrl, string targetPath)
		{
			var client = new WebClient();
			client.DownloadFile(sourceUrl, targetPath);
		}
	}
}