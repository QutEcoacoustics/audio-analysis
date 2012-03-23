namespace Acoustics.Shared
{
    using System.Configuration;
    using System.IO;

    public static class TempFileHelper
    {
        /// <summary>
        /// Gets a valid temp directory.
        /// Directory will exist.
        /// </summary>
        public static DirectoryInfo TempDir
        {
            get
            {
                var tempDir = ConfigurationManager.AppSettings["TempDir"];

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
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.ijk).
        /// File will not exist.
        /// </summary>
        public static FileInfo NewTempFile
        {
            get
            {
                return new FileInfo(Path.Combine(TempDir.FullName, Path.GetRandomFileName()));
            }
        }

        /// <summary>
        /// Gets a temporary file location. 
        /// File will be 8.3 (eg. abcdefgh.[given ext]).
        /// File will not exist.
        /// </summary>
        /// <param name="ext">File extension (without dot).</param>
        /// <returns>File with extension.</returns>
        public static FileInfo NewTempFileWithExt(string ext)
        {
            // ensure extension is valid (or not present).
            if (string.IsNullOrEmpty(ext))
            {
                // no extension
                ext =  string.Empty;
            }
            else if (!ext.StartsWith("."))
            {
                ext = "." + ext;
            }

            // get a new temp file name, and remove the extension and dot.
            var currentTempFile = NewTempFile;
            var tempFile = currentTempFile.FullName;
            var toremove = currentTempFile.Extension.Length + 1; // also remove dot.
            var tokeep = tempFile.Length - toremove;

            tempFile = tempFile.Substring(0, tokeep);
            tempFile = tempFile + ext;// add ext

            return new FileInfo(Path.Combine(TempDir.FullName, tempFile));
        }

        public static void SafeDeleteFile(this FileInfo file)
        {
            //try
            //{
                //if (file != null)
                //{
                    File.Delete(file.FullName);
                //}
            //}
            //catch
            //{
                // if we can't delete, that's ok.
            //}
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
