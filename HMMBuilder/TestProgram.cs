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
            HTKConfig htkConfig = new HTKConfig();

            htkConfig.CallName = "CURLEW1";
            //htkConfig.CallName = "CURRAWONG1";
            
            
            htkConfig.WorkingDir = Directory.GetCurrentDirectory();           
            htkConfig.TemplateDir    = "C:\\SensorNetworks\\temp";
            htkConfig.DataDir        = htkConfig.TemplateDir;
            htkConfig.ConfigDir      = htkConfig.TemplateDir + "\\config_" + htkConfig.CallName;
            htkConfig.ResultsDir     = htkConfig.TemplateDir + "\\results";
            htkConfig.SilenceModelFN = htkConfig.TemplateDir + "\\SilenceModels\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav\n";
            //vars for unzipping
            string zipFile = htkConfig.ConfigDir + ".zip";
            string target  = htkConfig.ConfigDir;
                                 
            Console.WriteLine("CWD=" + htkConfig.WorkingDir);
            Console.WriteLine("CFG=" + htkConfig.ConfigDir);
            Console.WriteLine("DAT=" + htkConfig.DataDir);
            Console.WriteLine("RSL=" + htkConfig.ResultsDir);

            #endregion


            ZipUnzip.UnZip(target, zipFile, true);


            //PREPARE THE TEST FILE AND EXTRACT FEATURES
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
