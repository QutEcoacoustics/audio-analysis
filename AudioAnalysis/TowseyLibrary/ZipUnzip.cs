using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace TowseyLibrary
{
    public class ZipUnzip
    {

        static void Main(string[] args)
        {

            string dir          = "C:\\SensorNetworks\\Templates\\Template_CURRAWONG1";
            string Dir2Compress = dir + "\\config_CURRAWONG1";
            string OutZipFile   = Dir2Compress + ".zip";
            string Target       = dir + "\\config_CURRAWONG2";

            //ZipDirectory(Dir2Compress, OutZipFile);
            ZipDirectoryRecursive(Dir2Compress, OutZipFile, true);
            UnZip(Target, OutZipFile, true);

            LoggedConsole.WriteLine("FINISHED");
            Console.ReadLine();

        } //end method Main()


        /// <summary>
        /// zips all hte files in passed directory.
        /// Does NOT zip directories recursively.
        /// </summary>
        /// <param name="Dir2Compress"></param>
        /// <param name="OutZipFile"></param>
        public static void ZipDirectory(string Dir2Compress, string OutZipFile)
        {

            try
            {
                string[] filenames = Directory.GetFiles(Dir2Compress);

                // 'using' statements gaurantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                using (ZipOutputStream s = new ZipOutputStream(File.Create(OutZipFile)))
                {

                    s.SetLevel(9); // 0 - store only to 9 - means best compression

                    byte[] buffer = new byte[4096];

                    foreach (string file in filenames)
                    {

                        // Using GetFileName makes the result compatible with XP
                        // as the resulting path is not absolute.
                        LoggedConsole.WriteLine("file=" + file);

                        ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                        // Setup the entry data as required.
                        // Crc and size are handled by the library for seakable streams so no need to do them here.

                        // Could also use the last write time or similar for the file.
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);

                        using (FileStream fs = File.OpenRead(file))
                        {

                            // Using a fixed size buffer here makes no noticeable difference for output
                            // but keeps a lid on memory usage.
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }

                    // Finish/Close arent needed strictly as the using statement does this automatically
                    // Finish is important to ensure trailing information for a Zip file is appended. 
                    // Without this the created file would be invalid.
                    s.Finish();

                    // Close is important to wrap things up and unlock the file.
                    s.Close();
                }

                if (File.Exists(OutZipFile))
                {
                    FileInfo fi = new FileInfo(OutZipFile);
                    LoggedConsole.WriteLine("Zipped file has been created at " + fi.FullName);
                }
                else LoggedConsole.WriteLine("Zipped File WAS NOT CREATED");
            }
            catch (Exception ex)
            {
                LoggedConsole.WriteLine("Exception during processing {0}", ex);

                // No need to rethrow the exception as for our purposes its handled.
            }

        } //end method ZipDirectory()


        public static void ZipDirectoryRecursive(string Dir2Compress, string OutZipFile, bool verbose)
        {
            string fileFilter = null;
            string dirFilter = null;
            //bool restoreDates = false;
            //bool restoreAttributes = false;
            bool recurse = true;
            //bool createEmptyDirs = false;

            //bool progress = false;
            TimeSpan interval = TimeSpan.FromSeconds(1);
            FastZipEvents events = null;

            if (verbose)
            {
                //events = new FastZipEvents();
                //events.ProcessDirectory = new ProcessDirectoryHandler(ProcessDirectory);
                //events.ProcessFile      = new ProcessFileHandler(ProcessFile);

                //if (progress)
                //{
                //    events.Progress = new ProgressHandler(ShowProgress);
                //    events.ProgressInterval = interval;
                //}
            }

            FastZip fastZip = new FastZip(events);
            //fastZip.CreateEmptyDirectories = createEmptyDirs;
            //fastZip.RestoreAttributesOnExtract = restoreAttributes;
            //fastZip.RestoreDateTimeOnExtract = restoreDates;
            fastZip.CreateZip(OutZipFile, Dir2Compress, recurse, fileFilter, dirFilter);

        }


        public static void UnZip(string targetDir, string zipFN, bool verbose)
        {
            bool restoreDates = false;
            bool restoreAttributes = false;
            bool recurse = true;
            bool createEmptyDirs = true;
            string fileFilter = null;
            string dirFilter = null;

            FastZip.Overwrite overwrite = FastZip.Overwrite.Always;
            FastZip.ConfirmOverwriteDelegate confirmOverwrite = null;
            FastZipEvents events = null;

            FastZip fastZip = new FastZip(events);
            fastZip.CreateEmptyDirectories = createEmptyDirs;
            fastZip.RestoreAttributesOnExtract = restoreAttributes;
            fastZip.RestoreDateTimeOnExtract = restoreDates;
            fastZip.ExtractZip(zipFN, targetDir, overwrite, confirmOverwrite, fileFilter, dirFilter, recurse);

        }
    } //end class

} //end namespace TowseyLib
