using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace HMMBuilder
{
    class TestProgram
    {
        static int Main(string[] args)
        {
            #region Variables

            HTKConfig htkConfig = new HTKConfig();
            htkConfig.CallName = "CURLEW1";
            //htkConfig.CallName = "CURRAWONG1";

            htkConfig.WorkingDir     = Directory.GetCurrentDirectory();
            htkConfig.TemplateDir    = "C:\\SensorNetworks\\Templates\\Template_" + htkConfig.CallName;
            htkConfig.DataDir        = htkConfig.TemplateDir + "\\data";
            htkConfig.ConfigDir      = htkConfig.TemplateDir + "\\config_" + htkConfig.CallName;
            htkConfig.ResultsDir     = htkConfig.TemplateDir + "\\results";
            htkConfig.SilenceModelFN = htkConfig.TemplateDir + "\\data\\SilenceModels\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav\n";
            Console.WriteLine("CWD=" + htkConfig.WorkingDir);
            Console.WriteLine("CFG=" + htkConfig.ConfigDir);
            Console.WriteLine("DAT=" + htkConfig.DataDir);
            Console.WriteLine("RSL=" + htkConfig.ResultsDir);

            bool good = true;
            #endregion


            #region Test the HMMs
            try
            {
                //string tmpVal = "";
                //if (HMMSettings.ConfigParam.TryGetValue("HBUILD", out tmpVal))
                //{
                //    if (tmpVal.Equals("Y")) //Generate the network file
                //    {
                //                HTKHelper.HBuild(htkConfig.monophones, htkConfig.wordNet);
                //    }
                //    else
                //    {
                //                //TO DO: Ask the user for the word network file
                //    }
                //}
                //else
                //{
                //   //TO DO: Ask the user for the word network file
                //}

                //True calls
                HTKHelper.HVite(htkConfig.MfccConfigTrainFN, htkConfig.tgtDir2, htkConfig.tTrueF, htkConfig.wordNet,
                                htkConfig.DictFile, htkConfig.resultTrue, htkConfig.monophones);
                //False calls
                HTKHelper.HVite(htkConfig.MfccConfigTrainFN, htkConfig.tgtDir2, htkConfig.tFalseF, htkConfig.wordNet,
                                htkConfig.DictFile, htkConfig.resultFalse, htkConfig.monophones);
            }
            catch
            {
                //Console.WriteLine(ex.ToString());
                good = false;
            }
            #endregion



            #region Accuracy Measurements: Accuracy = (TruePositives + TrueNegative)/TotalSamples
                
            //Read the output files
            float threshold = -1900f; //magic number for Currawong is 1900
            try
            {
                string callName = htkConfig.CallName;

                float accuracy = Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, ref callName, threshold);
                Console.WriteLine("System Accuracy: {0}% for {1} vocalisation using threshold={2}.", accuracy * 100, callName, threshold);
            }
            catch 
            {
                good = false;
            }

            #endregion

            Console.WriteLine("FINISHED!");
            Console.ReadLine();
            return good ? 0 : -1;
        }// end Main()

    }
}
