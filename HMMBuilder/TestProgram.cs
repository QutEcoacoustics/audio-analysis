using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;


namespace HMMBuilder
{
    class TestProgram
    {
        static void Main(string[] args)
        {
            #region Variables


            string dir = "C:\\SensorNetworks\\Templates\\Template_CURRAWONG1";
            string Dir2Compress = dir + "\\config_CURRAWONG1";
            string zipFile = dir+"\\zipfile.zip";

            try
            {
                string[] filenames = Directory.GetFiles(Dir2Compress);

                // 'using' statements gaurantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFile)))
                {

                    s.SetLevel(9); // 0 - store only to 9 - means best compression

                    byte[] buffer = new byte[4096];

                    foreach (string file in filenames)
                    {

                        // Using GetFileName makes the result compatible with XP
                        // as the resulting path is not absolute.
                        Console.WriteLine("file=" + file);

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

                if (File.Exists(zipFile))
                {
                    Console.WriteLine("File Exists");
                    FileInfo fi = new FileInfo(zipFile);
                    Console.WriteLine("File path = "+fi.FullName);
                }
                else Console.WriteLine("File DOES NOT Exist");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during processing {0}", ex);

                // No need to rethrow the exception as for our purposes its handled.
            }



            Console.WriteLine("FINISHED");
            Console.ReadLine();


            HTKConfig htkConfig = new HTKConfig();
            htkConfig.WorkingDir = Directory.GetCurrentDirectory();

            
            //htkConfig.CallName = "CURLEW1";
            htkConfig.CallName = "CURRAWONG1";
            
            htkConfig.TemplateDir    = "C:\\SensorNetworks\\temp";
            htkConfig.DataDir        = htkConfig.TemplateDir;
            htkConfig.ConfigDir      = htkConfig.TemplateDir + "\\config_" + htkConfig.CallName;
            htkConfig.ResultsDir     = htkConfig.TemplateDir + "\\results";
            htkConfig.SilenceModelFN = htkConfig.TemplateDir + "\\SilenceModels\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav\n";
                        
            
            Console.WriteLine("CWD=" + htkConfig.WorkingDir);
            Console.WriteLine("CFG=" + htkConfig.ConfigDir);
            Console.WriteLine("DAT=" + htkConfig.DataDir);
            Console.WriteLine("RSL=" + htkConfig.ResultsDir);

            #endregion


            //write script files
            HTKHelper.WriteScriptFiles(htkConfig.DataDir, htkConfig.TestFileCode, htkConfig.TestFile, htkConfig.wavExt, htkConfig.mfcExt);
            //extract features from the test file
            HTKHelper.ExtractFeatures(htkConfig.aOptionsStr, htkConfig.MfccConfigFN, htkConfig.TestFileCode); //test data
            //scan the file with HTK HMM
            HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.TestFile, htkConfig.wordNet,
                            htkConfig.DictFile, htkConfig.resultTest, htkConfig.monophones);

            

            Console.WriteLine("FINISHED!");
            Console.ReadLine();
        }// end Main()

    }
}
