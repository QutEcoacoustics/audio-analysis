namespace AudioBrowser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Helpers
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

            return dir.EnumerateFiles().Where(f => allowedExtensions.Contains(f.Extension));

        }


        public static bool ValidDirectory(string directory)
        {
            return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory);
        }

        public static DirectoryInfo GetDirectory(string directory)
        {
            if (ValidDirectory(directory)) return new DirectoryInfo(directory);
            return null;
        }
    }
}
