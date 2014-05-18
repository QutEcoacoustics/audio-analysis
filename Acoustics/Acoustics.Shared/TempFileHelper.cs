namespace Acoustics.Shared
{
    using System.Configuration;
    using System.IO;
    using System.Linq;

    public static class TempFileHelper
    {
        /// <summary>
        /// Gets a valid temp directory.
        /// Directory will exist.
        /// </summary>
        public static DirectoryInfo TempDir()
        {
            var tempDirString = "TempDir";
            var tempDirSet = ConfigurationManager.AppSettings.AllKeys.Any(i => i == tempDirString);

            var tempDir = string.Empty;

            if (tempDirSet)
            {
                tempDir = ConfigurationManager.AppSettings["TempDir"];
            }

            if (string.IsNullOrEmpty(tempDir))
            {
                tempDir = Path.GetTempPath();
            }

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            return new DirectoryInfo(tempDir);
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.ijk).
        /// File will not exist.
        /// </summary>
        public static FileInfo NewTempFile()
        {
            return new FileInfo(Path.Combine(TempDir().FullName, Path.GetRandomFileName()));
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.ijk).
        /// File will not exist.
        /// </summary>
        /// <param name="tempDir">Temporary directory.</param>
        /// <returns>Temp file that does not exist.</returns>
        public static FileInfo NewTempFile(DirectoryInfo tempDir)
        {
            return new FileInfo(Path.Combine(tempDir.FullName, Path.GetRandomFileName()));
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.[given ext]).
        /// File will not exist.
        /// </summary>
        /// <param name="extension">File extension (without dot).</param>
        /// <returns>File with extension.</returns>
        public static FileInfo NewTempFile(string extension)
        {
            return NewTempFile(TempDir(), extension);
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.[given ext]).
        /// File will not exist.
        /// </summary>
        /// <param name="tempDir">Temporary directory.</param>
        /// <param name="ext">File extension (without dot).</param>
        /// <returns>File with extension.</returns>
        public static FileInfo NewTempFile(DirectoryInfo tempDir, string extension)
        {
            // ensure extension is valid (or not present).
            if (string.IsNullOrEmpty(extension))
            {
                // no extension
                extension = string.Empty;
            }
            else if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            var fileName = Path.GetRandomFileName();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            return new FileInfo(Path.Combine(tempDir.FullName, fileNameWithoutExtension+extension));
        }

        /// <summary>
        /// Copy from <paramref name="source"/> Stream to Working File.
        /// </summary>
        /// <param name="source">
        /// The source stream.
        /// </param>
        /// <param name="destinationFile">File to write to.</param>
        /// <param name="append">
        /// True to append to existing file, false to replace any existing data in file.
        /// </param>
        public static void CopyFromStream(this Stream source, FileInfo destinationFile, bool append = false)
        {
            FileMode mode = FileMode.OpenOrCreate;

            if (append)
            {
                mode = FileMode.Append;
            }

            using (var target = new FileStream(destinationFile.FullName, mode, FileAccess.Write, FileShare.None))
            {
                source.CopyToStream(target);
            }
        }
    }
}
